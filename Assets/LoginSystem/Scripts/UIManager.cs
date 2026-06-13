using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registrationPanel;

    [Header("Output Messages")]
    [SerializeField] private Text loginOutputText;
    [SerializeField] private Text registrationOutputText;

    [Header("Loading Video")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private VideoPlayer loadingVideo;

    [Header("Throbber")]
    [Tooltip("Root panel that covers the whole screen while Firebase is initializing")]
    [SerializeField] private GameObject throbberPanel;
    [Tooltip("The RectTransform that spins (assign the circle/ring image)")]
    [SerializeField] private RectTransform throbberSpinner;
    [Tooltip("Degrees per second — 360 = one full rotation per second")]
    [SerializeField] private float spinSpeed = 360f;

    private bool isSpinning = false;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        // Hide everything except the throbber so there is zero flash of the
        // login UI while Firebase is restoring its auth session.
        if (loginPanel != null)        loginPanel.SetActive(false);
        if (registrationPanel != null) registrationPanel.SetActive(false);
        if (loadingPanel != null)      loadingPanel.SetActive(false);

        ShowThrobber();   // on by default until FirebaseAuthManager decides what to show
    }

    private void Update()
    {
        if (isSpinning && throbberSpinner != null)
            throbberSpinner.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);
    }

    // =========================================================================
    // THROBBER
    // =========================================================================

    public void ShowThrobber()
    {
        if (throbberPanel != null) throbberPanel.SetActive(true);
        isSpinning = true;
    }

    public void HideThrobber()
    {
        if (throbberPanel != null) throbberPanel.SetActive(false);
        isSpinning = false;
    }

    // =========================================================================
    // PANELS
    // =========================================================================

    public void OpenLoginPanel()
    {
        HideThrobber();
        loginPanel.SetActive(true);
        registrationPanel.SetActive(false);
        ClearMessages();
    }

    public void OpenRegistrationPanel()
    {
        HideThrobber();
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
        if (loginOutputText != null)        loginOutputText.text = "";
        if (registrationOutputText != null) registrationOutputText.text = "";
    }

    // =========================================================================
    // LOADING VIDEO
    // =========================================================================

    public IEnumerator PlayVideoAndLoad(string sceneName)
    {
        HideThrobber();
        loadingPanel.SetActive(true);
        loginPanel.SetActive(false);

        yield return null;

        loadingVideo.Stop();
        loadingVideo.time = 0;
        loadingVideo.Prepare();

        Debug.Log("🎬 Preparing video...");
        yield return new WaitUntil(() => loadingVideo.isPrepared);

        loadingVideo.Play();
        Debug.Log("🎬 Video playing!");

        yield return new WaitUntil(() => !loadingVideo.isPlaying);
        Debug.Log("🎬 Video finished!");

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
