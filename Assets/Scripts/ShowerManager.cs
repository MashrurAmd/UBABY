using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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

    [Header("Body Area (screen space)")]
    public float bodyRadius = 150f;

    [Header("Timing")]
    public float _waitTime = 0.15f;
    float _elapsed;

    Vector3 soapStartPos, waterStartPos;
    bool isHoldingSoap, isHoldingWater;

    float maxSoap, maxWater;

    void Start()
    {
        soapStartPos = soapImage.transform.position;
        waterStartPos = waterImage.transform.position;
    }

    void Update()
    {
        if (playerManager.currentRoom != "shower") return;

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
            soapImage.transform.position = soapStartPos;

        if (isHoldingWater)
        {
            waterImage.transform.position = waterStartPos;
            waterAudio.Stop();
        }

        isHoldingSoap = false;
        isHoldingWater = false;
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
        Vector2 bodyScreen = Camera.main.WorldToScreenPoint(bodyCenter.position);
        return Vector2.Distance(screenPos, bodyScreen) < bodyRadius;
    }

    // -------------------- SOAP --------------------

    void TryCreateFoam(Vector2 pos)
    {
        if (!IsOverBody(pos)) return;

        _elapsed += Time.deltaTime;
        if (_elapsed < _waitTime) return;
        _elapsed = 0f;

        GameObject foam = Instantiate(foamPrefab, soapImage.transform.position, Quaternion.identity, foamParent);

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
    }

    // --------------------

    public void ClearAllFoam()
    {
        foreach (Transform t in foamParent)
            Destroy(t.gameObject);
    }
}
