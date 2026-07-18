using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registrationPanel;

    [Header("Startup 'Image' Panel (to be replaced by IntroTutorial)")]
    [Tooltip("The 'Image' GameObject currently shown first — will be hidden so IntroTutorial shows instead")]
    [SerializeField] private GameObject startupImagePanel;

    [Header("Intro Panel (EventWorld splash: Sign In / Explore Events)")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private Button signInButton;
    [SerializeField] private Button exploreEventsButton;

    [System.Serializable]
    public class TutorialPage
    {
        public GameObject page;
        [Tooltip("Untick to hide the cursor entirely on this page")]
        public bool showCursor = true;
        [Tooltip("Anchored position the cursor image should jump to on this page")]
        public Vector2 cursorPosition;
    }

    [Header("Tutorial Walkthrough")]
    [Tooltip("The 'Tutorial' parent GameObject")]
    [SerializeField] private GameObject tutorialPanel;
    [Tooltip("Assign in order: Office, kitchen, shop, shower, game — each with its own cursor position")]
    [SerializeField] private TutorialPage[] tutorialPages;
    [SerializeField] private Button tutorialLeftButton;
    [SerializeField] private Button tutorialRightButton;
    [Tooltip("Back button that lives inside the last page (e.g. 'game')")]
    [SerializeField] private Button tutorialBackButton;
    [Tooltip("The 'Cursor' image's RectTransform, moved between pages")]
    [SerializeField] private RectTransform cursorRect;

    [Header("Sign In Loading")]
    [Tooltip("How long the throbber shows after tapping Sign In, before the Sign In panel appears")]
    [SerializeField] private float signInLoadingDelay = 1f;

    [Header("Office Page Cursor Intro")]
    [Tooltip("Seconds to wait before the cursor appears the first time the Office page is shown")]
    [SerializeField] private float officeCursorDelay = 2f;

    private bool officeCursorShownOnce = false;
    private Coroutine officeCursorCoroutine;

    private int currentTutorialPage = 0;

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
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registrationPanel != null) registrationPanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (startupImagePanel != null) startupImagePanel.SetActive(false);
        if (introPanel != null) introPanel.SetActive(true);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (cursorRect != null) cursorRect.gameObject.SetActive(false);

        if (signInButton != null) signInButton.onClick.AddListener(OnSignInButtonClicked);
        if (exploreEventsButton != null) exploreEventsButton.onClick.AddListener(OnExploreEventsButtonClicked);
        if (tutorialLeftButton != null) tutorialLeftButton.onClick.AddListener(ShowPreviousTutorialPage);
        if (tutorialRightButton != null) tutorialRightButton.onClick.AddListener(ShowNextTutorialPage);
        if (tutorialBackButton != null) tutorialBackButton.onClick.AddListener(OnTutorialBackButtonClicked);

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

    public void OpenIntroPanel()
    {
        HideThrobber();
        if (startupImagePanel != null) startupImagePanel.SetActive(false);
        if (introPanel != null) introPanel.SetActive(true);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registrationPanel != null) registrationPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (cursorRect != null) cursorRect.gameObject.SetActive(false);
        ClearMessages();
    }

    private void OnSignInButtonClicked()
    {
        if (introPanel != null) introPanel.SetActive(false);
        StartCoroutine(ShowLoginAfterLoading());
    }

    private IEnumerator ShowLoginAfterLoading()
    {
        ShowThrobber();
        yield return new WaitForSeconds(signInLoadingDelay);
        OpenLoginPanel(); // hides the throbber internally
    }

    private void OnExploreEventsButtonClicked()
    {
        if (introPanel != null) introPanel.SetActive(false);

        if (tutorialPanel != null) tutorialPanel.SetActive(true);

        currentTutorialPage = 0;
        officeCursorShownOnce = false; // fresh tutorial session — Office gets its delayed reveal again
        RefreshTutorialPages();
    }

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
        if (loginOutputText != null) loginOutputText.text = "";
        if (registrationOutputText != null) registrationOutputText.text = "";
    }

    // =========================================================================
    // TUTORIAL WALKTHROUGH
    // =========================================================================

    private void RefreshTutorialPages()
    {
        if (tutorialPages == null || tutorialPages.Length == 0) return;

        for (int i = 0; i < tutorialPages.Length; i++)
        {
            if (tutorialPages[i].page != null)
                tutorialPages[i].page.SetActive(i == currentTutorialPage);
        }

        // Cancel any pending Office cursor reveal if we've navigated to a different page
        if (officeCursorCoroutine != null)
        {
            StopCoroutine(officeCursorCoroutine);
            officeCursorCoroutine = null;
        }

        bool isOfficePage = currentTutorialPage == 0;

        if (isOfficePage && !officeCursorShownOnce)
        {
            // First time landing on Office: keep the cursor hidden, then reveal it after a delay
            if (cursorRect != null) cursorRect.gameObject.SetActive(false);
            officeCursorCoroutine = StartCoroutine(ShowOfficeCursorAfterDelay());
        }
        else if (isOfficePage)
        {
            // Already showed the Office cursor once this session — don't show it again
            if (cursorRect != null) cursorRect.gameObject.SetActive(false);
        }
        else
        {
            ApplyCursorForCurrentPage();
        }

        UpdateTutorialNavButtons();
    }

    private IEnumerator ShowOfficeCursorAfterDelay()
    {
        yield return new WaitForSeconds(officeCursorDelay);

        officeCursorShownOnce = true;
        ApplyCursorForCurrentPage();
        officeCursorCoroutine = null;
    }

    private void ApplyCursorForCurrentPage()
    {
        if (cursorRect == null) return;

        bool showCursor = tutorialPages[currentTutorialPage].showCursor;
        cursorRect.gameObject.SetActive(showCursor);

        if (showCursor)
            cursorRect.anchoredPosition = tutorialPages[currentTutorialPage].cursorPosition;
    }

    private void UpdateTutorialNavButtons()
    {
        // Hide the left arrow on the first page, hide the right arrow on the
        // last page. Remove these two lines if you'd rather keep both arrows
        // always visible.
        if (tutorialLeftButton != null)
        {
            bool shouldShowLeft = currentTutorialPage > 0;
            Debug.Log($"[UIManager] UpdateTutorialNavButtons: page={currentTutorialPage}, setting LeftButton active={shouldShowLeft}");
            tutorialLeftButton.gameObject.SetActive(shouldShowLeft);
        }

        if (tutorialRightButton != null)
            tutorialRightButton.gameObject.SetActive(currentTutorialPage < tutorialPages.Length - 1);
    }

    public void ShowNextTutorialPage()
    {
        if (tutorialPages == null || currentTutorialPage >= tutorialPages.Length - 1) return;

        currentTutorialPage++;
        RefreshTutorialPages();
    }

    public void ShowPreviousTutorialPage()
    {
        if (tutorialPages == null || currentTutorialPage <= 0) return;

        currentTutorialPage--;
        RefreshTutorialPages();
    }

    private void OnTutorialBackButtonClicked()
    {
        if (officeCursorCoroutine != null)
        {
            StopCoroutine(officeCursorCoroutine);
            officeCursorCoroutine = null;
        }

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (cursorRect != null) cursorRect.gameObject.SetActive(false);
        OpenIntroPanel();
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