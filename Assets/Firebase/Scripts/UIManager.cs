using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // ✅ add this

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registrationPanel;

    [Header("Output Messages")]
    [SerializeField] private Text loginOutputText;
    [SerializeField] private Text registrationOutputText;

    [Header("Loading Video")]
    [SerializeField] private GameObject loadingPanel;    // panel that shows during loading
    [SerializeField] private VideoPlayer loadingVideo;   // video player component

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
    // 🎬 LOADING VIDEO
    // ===========================

// ✅ ADD THIS INSTEAD
    public IEnumerator PlayVideoAndLoad(string sceneName)
    {
        loadingPanel.SetActive(true);
        loginPanel.SetActive(false);

        yield return null;

        loadingVideo.Stop();
        loadingVideo.time = 0;

        loadingVideo.Prepare();
        Debug.Log("🎬 Preparing video...");

        yield return new WaitUntil(() => loadingVideo.isPrepared);
        Debug.Log("🎬 Video prepared!");

        loadingVideo.Play();
        Debug.Log("🎬 Video playing!");

        yield return new WaitUntil(() => !loadingVideo.isPlaying);
        Debug.Log("🎬 Video finished!");

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}