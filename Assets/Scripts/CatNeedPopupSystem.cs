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
            playerManager.playerAnimator.GetBool("Listening"))
        {
            yield return null;
            continue;
        }

        // ✅ Skip all popups if player is sleeping
        if (playerManager.isSleeping)
        {
            yield return null;
            continue;
        }

        // ✅ Global cooldown check
        if (Time.time - lastPopupTime < cooldownBetweenPopups)
        {
            yield return null;
            continue;
        }

        if (!popupActive)
        {
            // --- HUNGER --- ✅ skip if sleeping
            if (storeManager.kitchenProgressBar.fillAmount < hungerThreshold)
            {
                popupActive = true;
                yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

                // ✅ Check again after delay in case player fell asleep during wait
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

            // --- SLEEP ---
            if (playerManager.sleepProgressBar.fillAmount < sleepThreshold)
            {
                popupActive = true;
                yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
                ShowPopup("I want to sleep 💤", sleepClip);
                lastPopupTime = Time.time;
                popupActive = false;
            }

            // --- SHOWER --- ✅ skip if sleeping
            if (showerManager.showerProgressImage.fillAmount < showerThreshold)
            {
                popupActive = true;
                yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

                // ✅ Check again after delay in case player fell asleep during wait
                if (!playerManager.isSleeping)
                {
                    ShowPopup("I need shower 🚿", showerClip);
                    lastPopupTime = Time.time;
                }
                popupActive = false;
            }

            // --- FULL ---
            if (storeManager.kitchenProgressBar.fillAmount >= 1f)
            {
                popupActive = true;
                ShowPopup("I am full 😺", fullClip, false);
                lastPopupTime = Time.time;
                popupActive = false;
            }

            // --- SLEEP DONE ---
            if (playerManager.sleepProgressBar.fillAmount >= 1f)
            {
                popupActive = true;
                ShowPopup("Sleep done 😴", fullClip, false);
                lastPopupTime = Time.time;
                popupActive = false;
            }

            // --- SHOWER DONE ---
            if (showerManager.showerProgressImage.fillAmount >= 1f)
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