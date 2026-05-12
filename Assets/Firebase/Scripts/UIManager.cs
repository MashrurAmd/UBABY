using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registrationPanel;

    [Header("Output Messages")]
    [SerializeField] private Text loginOutputText;
    [SerializeField] private Text registrationOutputText;

    [Header("Loading Slider")]
    [SerializeField] private GameObject loadingSliderObject; // parent object to show/hide
    [SerializeField] private Slider loadingSlider;           // the slider itself

    private void Awake()
    {
        CreateInstance();
    }

    private void CreateInstance()
    {
        if (Instance == null)
            Instance = this;
    }

    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        registrationPanel.SetActive(false);
        ClearMessages();
    }

    public void OpenRegistrationPanel()
    {
        registrationPanel.SetActive(true);
        loginPanel.SetActive(false);
        ClearMessages();
    }

    public void ShowLoginMessage(string message, bool isError = false)
    {
        if (loginOutputText == null) return;
        loginOutputText.text = message;
        loginOutputText.color = isError ? Color.red : Color.green;
    }

    public void ShowRegistrationMessage(string message, bool isError = false)
    {
        if (registrationOutputText == null) return;
        registrationOutputText.text = message;
        registrationOutputText.color = isError ? Color.red : Color.green;
    }

    private void ClearMessages()
    {
        if (loginOutputText != null) loginOutputText.text = "";
        if (registrationOutputText != null) registrationOutputText.text = "";
    }

    // ===========================
    // ⏳ LOADING SLIDER
    // ===========================

    public void ShowLoadingSlider()
    {
        if (loadingSliderObject != null)
            loadingSliderObject.SetActive(true);
        if (loadingSlider != null)
            loadingSlider.value = 0f;
    }

    public void HideLoadingSlider()
    {
        if (loadingSliderObject != null)
            loadingSliderObject.SetActive(false);
    }

    public void SetSliderValue(float value)
    {
        if (loadingSlider != null)
            loadingSlider.value = value;
    }

    public IEnumerator FillSliderAndLoad(string sceneName)
    {
        ShowLoadingSlider();

        float duration = 5f; // ✅ how long to fill the slider
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            SetSliderValue(progress);
            yield return null;
        }

        SetSliderValue(1f);
        yield return new WaitForSeconds(0.2f); // ✅ small pause at full

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}