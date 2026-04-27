using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;


public class ShowerManager : MonoBehaviour
{
    public PlayerManager playerManager;

    [Header("UI References")]
    public Image soapImage;
    public Image waterImage;

    [Header("Gameplay")]
    public Transform bodyCenter;
    public GameObject foamPrefab;
    public Transform foamParent;
    public AudioSource waterAudio;
    public Image showerProgressImage;
    public float fillAmount = 0.05f;
    
    [Header("Mud System")]
    public Renderer characterRenderer;   // drag character renderer here
    public Material cleanMaterial;       // normal clean material
    public Material dirtyMaterial;       // dirty/mud material
    public float dirtyTimer = 60f;       // 1 minute before getting dirty again
    public float fadeDuration = 2f; // ✅ how long the fade takes in seconds
    

    private float dirtyElapsed = 0f;
    private bool isClean = true;
    private bool isFading = false;

    [Header("Body Area (screen space)")]
    public float bodyRadius = 150f;


    [Header("Timing")]
    public float _waitTime = 0.15f;
    float _elapsed;

    Vector3 soapStartPos, waterStartPos;
    bool isHoldingSoap, isHoldingWater;

    float maxSoap, maxWater;


    [Header("Effects")]
    public GameObject shinyParticleEffect; // drag your particle prefab here
    public AudioSource bubbleAudio; // ✅ drag your bubble sound AudioSource here
    
    
    
    
    void Start()
    {
        soapStartPos = soapImage.transform.position;
        waterStartPos = waterImage.transform.position;
    }

    void Update()
    {
        // ✅ Dirt timer runs in ALL rooms - move it above the shower check
        if (isClean)
        {
            dirtyElapsed += Time.deltaTime;
            Debug.Log($"Dirty timer: {dirtyElapsed}/{dirtyTimer}"); // remove after fix
            if (dirtyElapsed >= dirtyTimer)
            {
                SetDirty();
            }
        }

        // ⬇️ Everything below is shower-only
        if (playerManager.currentRoom != "shower") return;

        if (Input.GetMouseButton(0))
        {
            Vector2 bodyScreen = Camera.main.WorldToScreenPoint(bodyCenter.position);
            Debug.Log($"Finger: {Input.mousePosition}, Body: {bodyScreen}, Dist: {Vector2.Distance(Input.mousePosition, bodyScreen)}");
        }

#if UNITY_EDITOR || UNITY_WEBGL
        if (Input.GetMouseButtonDown(0))
            OnDown(Input.mousePosition);

        if (Input.GetMouseButton(0))
            OnDrag(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
            OnUp();
#else
    if (Input.touchCount > 0)
    {
        Touch t = Input.GetTouch(0);
        if (t.phase == TouchPhase.Began) OnDown(t.position);
        if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) OnDrag(t.position);
        if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) OnUp();
    }
#endif
    }

    // -------------------- INPUT --------------------

    void OnDown(Vector2 pos)
    {
        GameObject hit = GetUIUnderFinger(pos);

        if (hit == soapImage.gameObject)
            isHoldingSoap = true;

        if (hit == waterImage.gameObject)
        {
            isHoldingWater = true;
            waterAudio.Play();
        }
    }
    
    

    void OnDrag(Vector2 pos)
    {
        if (isHoldingSoap)
        {
            soapImage.transform.position = pos;
            TryCreateFoam(pos);
        }

        if (isHoldingWater)
        {
            waterImage.transform.position = pos;
            TryClearFoam(pos);
        }
    }

    void OnUp()
    {
        if (isHoldingSoap)
        {
            soapImage.transform.position = soapStartPos;
            bubbleAudio.Stop(); // ✅ stop bubble sound when finger lifted
        }

        if (isHoldingWater)
        {
            waterImage.transform.position = waterStartPos;
            waterAudio.Stop();
        }

        isHoldingSoap = false;
        isHoldingWater = false;
    }
    
    
    void SetClean()
    {
        isClean = true;
        dirtyElapsed = 0f;
        StartCoroutine(FadeToMaterial(cleanMaterial)); // ✅ fade instead of snap
        Debug.Log("✅ Fading to CLEAN");
    }

    void SetDirty()
    {
        isClean = false;
        dirtyElapsed = 0f;
        StartCoroutine(FadeToMaterial(dirtyMaterial)); // ✅ fade instead of snap
        Debug.Log("✅ Fading to DIRTY");
    }

    // -------------------- UI RAYCAST --------------------

    GameObject GetUIUnderFinger(Vector2 pos)
    {
        PointerEventData ped = new PointerEventData(EventSystem.current);
        ped.position = pos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        if (results.Count > 0)
            return results[0].gameObject;

        return null;
    }

    // -------------------- BODY CHECK --------------------

    bool IsOverBody(Vector2 screenPos)
    {
        // Use the shower camera explicitly instead of Camera.main
        Camera showerCam = Camera.main; // or assign a public Camera showerCamera field

        Vector2 bodyScreen = showerCam.WorldToScreenPoint(bodyCenter.position);
        float dist = Vector2.Distance(screenPos, bodyScreen);
        Debug.Log($"[Shower] Distance to body: {dist}, bodyRadius: {bodyRadius}"); // remove after fix
        return dist < bodyRadius;
    }

    // -------------------- SOAP --------------------

    void TryCreateFoam(Vector2 pos)
    {
        if (!IsOverBody(pos))
        {
            if (bubbleAudio.isPlaying)
                bubbleAudio.Stop();
            return;
        }

        if (!bubbleAudio.isPlaying)
            bubbleAudio.Play();

        shinyParticleEffect.SetActive(false);

        _elapsed += Time.deltaTime;
        if (_elapsed < _waitTime) return;
        _elapsed = 0f;

        GameObject foam = Instantiate(foamPrefab, soapImage.transform.position,
            Quaternion.identity, foamParent);
        foam.transform.localScale = Vector3.one;

        if (maxSoap < .9f)
        {
            showerProgressImage.fillAmount += fillAmount;
            maxSoap += fillAmount;
        }
    }

    // -------------------- WATER --------------------

    void TryClearFoam(Vector2 pos)
    {
        if (!IsOverBody(pos)) return;

        foreach (Transform t in foamParent)
        {
            Destroy(t.gameObject);
            break;
        }

        if (maxWater < .2f)
        {
            showerProgressImage.fillAmount += fillAmount;
            maxWater += fillAmount;
        }

        // ✅ When all foam cleared and soap was applied → go clean
        if (foamParent.childCount == 0 && maxSoap > 0)
        {
            shinyParticleEffect.SetActive(true);
            SetClean();         // ✅ swap to clean material
            maxSoap = 0f;
            maxWater = 0f;
        }
    }

    // --------------------

    public void ClearAllFoam()
    {
        foreach (Transform t in foamParent)
            Destroy(t.gameObject);

        shinyParticleEffect.SetActive(false);
        maxSoap = 0f;
        maxWater = 0f;
    }
    
    IEnumerator FadeToMaterial(Material targetMaterial)
    {
        if (isFading) yield break; // ✅ prevent multiple fades at once
        isFading = true;

        Material currentMat = characterRenderer.material;
        float elapsed = 0f;

        // ✅ Create a temporary material to lerp between
        Material tempMat = new Material(currentMat);
        characterRenderer.material = tempMat;

        Color startColor = currentMat.color;
        Color endColor = targetMaterial.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // ✅ Smoothly lerp the color
            tempMat.color = Color.Lerp(startColor, endColor, t);

            // ✅ Also lerp any texture if needed
            tempMat.Lerp(currentMat, targetMaterial, t);

            yield return null;
        }

        // ✅ Set final material cleanly
        characterRenderer.material = targetMaterial;
        isFading = false;
    }
    
    
}
