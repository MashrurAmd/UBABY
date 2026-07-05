using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CatNeedsPopup : MonoBehaviour
{
    [Header("References")]
    public PlayerManager playerManager;
    public ShowerManager showerManager;
    public StoreManager storeManager;

    [Header("Popup UI")]
    public Text popupText;
    public AudioSource popupAudio;
    public AudioClip hungryClip, sleepClip, showerClip, fullClip;
    public Vector3 popupMoveOffset = new Vector3(0, 40, 0);
    public float popupDuration = 1f;

    [Header("Settings")]
    public float minDelay = 300f;
    public float maxDelay = 600f;
    public float hungerThreshold = 0.5f;
    public float sleepThreshold = 0.5f;
    public float showerThreshold = 0.5f;
    
    private float lastPopupTime = -999f;
    public float cooldownBetweenPopups = 120f; 

    [Header("Animation Cooldown")]
    public float hungryCooldown = 20f; // ✅ 20 seconds between hungry animations
    private float lastHungryAnimTime = -999f; // ✅ track last time animation played

    public Animator playerAnimator;
    private bool popupActive;

    void Start()
    {
        StartCoroutine(NeedsRoutine());
    }

IEnumerator NeedsRoutine()
{
    while (true)
    {
        if (playerManager.playerAnimator.GetBool("Talking") ||
            playerManager.playerAnimator.GetBool("Listening") ||
            playerManager.isSleeping) // ✅ bail immediately if sleeping
        {
            yield return null;
            continue;
        }

        if (Time.time - lastPopupTime < cooldownBetweenPopups)
        {
            yield return null;
            continue;
        }

        if (!popupActive)
        {
            // --- HUNGER ---
            if (storeManager.kitchenProgressBar.fillAmount < hungerThreshold)
            {
                popupActive = true;
                yield return WaitUnlessSleeping(Random.Range(minDelay, maxDelay));

                if (!playerManager.isSleeping)
                {
                    ShowPopup("I am hungry 😿", hungryClip);

                    if (Time.time - lastHungryAnimTime >= hungryCooldown)
                    {
                        playerAnimator.SetTrigger("Hungry");
                        lastHungryAnimTime = Time.time;
                    }
                    lastPopupTime = Time.time;
                }
                popupActive = false;
            }

            // ✅ re-check between blocks — don't fall through stale state
            if (playerManager.isSleeping) { yield return null; continue; }

            // --- SLEEP ---
            if (playerManager.sleepProgressBar.fillAmount < sleepThreshold)
            {
                popupActive = true;
                yield return WaitUnlessSleeping(Random.Range(minDelay, maxDelay));

                if (!playerManager.isSleeping) // ✅ this check was missing entirely
                {
                    ShowPopup("I want to sleep 💤", sleepClip);
                    lastPopupTime = Time.time;
                }
                popupActive = false;
            }

            if (playerManager.isSleeping) { yield return null; continue; }

            // --- SHOWER ---
            if (showerManager.showerProgressImage.fillAmount < showerThreshold)
            {
                popupActive = true;
                yield return WaitUnlessSleeping(Random.Range(minDelay, maxDelay));

                if (!playerManager.isSleeping)
                {
                    ShowPopup("I need shower 🚿", showerClip);
                    lastPopupTime = Time.time;
                }
                popupActive = false;
            }

            if (playerManager.isSleeping) { yield return null; continue; }

            // --- FULL / DONE checks (instant, no wait) ---
            if (storeManager.kitchenProgressBar.fillAmount >= 1f)
            {
                popupActive = true;
                ShowPopup("I am full 😺", fullClip, false);
                lastPopupTime = Time.time;
                popupActive = false;
            }

            if (playerManager.sleepProgressBar.fillAmount >= 1f)
            {
                popupActive = true;
                ShowPopup("Sleep done 😴", fullClip, false);
                lastPopupTime = Time.time;
                popupActive = false;
            }

            if (!playerManager.isSleeping && showerManager.showerProgressImage.fillAmount >= 1f)
            {
                popupActive = true;
                ShowPopup("Shower done 🚿", fullClip, false);
                lastPopupTime = Time.time;
                popupActive = false;
            }
        }

        yield return null;
    }
}

// ✅ New helper: waits up to `duration`, but bails out early
// the moment the player falls asleep, instead of blocking for
// the full 5–10 minutes regardless of state.
IEnumerator WaitUnlessSleeping(float duration)
{
    float t = 0f;
    while (t < duration && !playerManager.isSleeping)
    {
        t += Time.deltaTime;
        yield return null;
    }
}

    void ShowPopup(string msg, AudioClip clip, bool allowAnimation = true)
    {
        if (popupAudio && clip) popupAudio.PlayOneShot(clip);

        if (popupText)
        {
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