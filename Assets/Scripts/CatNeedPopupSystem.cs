using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CatNeedsPopup : MonoBehaviour
{
    [Header("References")]
    public PlayerManager playerManager;      // Sleep + Talking/Listening Anim
    public ShowerManager showerManager;      // Shower
    public StoreManager storeManager;        // Hunger

    [Header("Popup UI")]
    public Text popupText;
    public AudioSource popupAudio;
    public AudioClip hungryClip, sleepClip, showerClip, fullClip;
    public Vector3 popupMoveOffset = new Vector3(0, 40, 0);
    public float popupDuration = 1f;

    [Header("Settings")]
    public float minDelay = 30f;  // seconds
    public float maxDelay = 60f;  // seconds
    public float hungerThreshold = 0.5f;
    public float sleepThreshold = 0.5f;
    public float showerThreshold = 0.5f;

    private bool popupActive;

    void Start()
    {
        StartCoroutine(NeedsRoutine());
    }

    IEnumerator NeedsRoutine()
    {
        while (true)
        {
            // Agar Talking ya Listening animation chal rahi hai → skip popup
            if (playerManager.playerAnimator.GetBool("Talking") ||
                playerManager.playerAnimator.GetBool("Listening"))
            {
                yield return null;
                continue;
            }

            if (!popupActive)
            {
                // ----- PRIORITY CHECK: Hunger > Sleep > Shower -----

                // --- HUNGER ---
                if (storeManager.kitchenProgressBar.fillAmount < hungerThreshold)
                {
                    popupActive = true;
                    yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
                    ShowPopup("I am hungry 😿", hungryClip);
                    popupActive = false;
                }
                else if (storeManager.kitchenProgressBar.fillAmount >= 1f)
                {
                    popupActive = true;
                    ShowPopup("I am full 😺", fullClip, false); // no animation
                    popupActive = false;
                }

                // --- SLEEP ---
                else if (playerManager.sleepProgressBar.fillAmount < sleepThreshold)
                {
                    popupActive = true;
                    yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
                    ShowPopup("I want to sleep 💤", sleepClip);
                    popupActive = false;
                }
                else if (playerManager.sleepProgressBar.fillAmount >= 1f)
                {
                    popupActive = true;
                    ShowPopup("Sleep done 😴", fullClip, false); // no animation
                    popupActive = false;
                }

                // --- SHOWER ---
                else if (showerManager.showerProgressImage.fillAmount < showerThreshold)
                {
                    popupActive = true;
                    yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
                    ShowPopup("I need shower 🚿", showerClip);
                    popupActive = false;
                }
                else if (showerManager.showerProgressImage.fillAmount >= 1f)
                {
                    popupActive = true;
                    ShowPopup("Shower done 🚿", fullClip, false); // no animation
                    popupActive = false;
                }
            }

            yield return null;
        }
    }

    void ShowPopup(string msg, AudioClip clip, bool allowAnimation = true)
    {
        if (popupAudio && clip) popupAudio.PlayOneShot(clip);

        if (popupText)
        {
            // Agar allowAnimation false → No animation, sirf fade
            if (allowAnimation)
                StartCoroutine(PopupRoutine(msg));
            else
            {
                popupText.gameObject.SetActive(true);
                popupText.text = msg;
                StartCoroutine(HidePopupAfterTime(popupDuration));
            }
        }
    }

    IEnumerator PopupRoutine(string msg)
    {
        popupText.gameObject.SetActive(true);
        popupText.text = msg;

        Vector3 startPos = popupText.transform.localPosition;
        Vector3 endPos = startPos + popupMoveOffset;

        float t = 0f;
        while (t < popupDuration)
        {
            t += Time.deltaTime;
            float p = t / popupDuration;
            popupText.transform.localPosition = Vector3.Lerp(startPos, endPos, p);
            popupText.color = new Color(1, 1, 1, 1f - p);
            yield return null;
        }

        popupText.transform.localPosition = startPos;
        popupText.gameObject.SetActive(false);
    }

    IEnumerator HidePopupAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        popupText.gameObject.SetActive(false);
    }
}
