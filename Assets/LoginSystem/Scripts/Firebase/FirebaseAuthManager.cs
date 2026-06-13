using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Google;

public class FirebaseAuthManager : MonoBehaviour
{
    // --- Firebase Core Variables ---
    [Header("Firebase Setup")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;

    [Header("Google Sign-In Configuration")]
    public string googleWebClientId = "843319817240-a3kd2bdvlsmd644o2gsdtf0fcffkrl5s.apps.googleusercontent.com";

    // --- UI Fields ---
    [Space]
    [Header("Login Fields")]
    public InputField emailLoginField;
    public InputField passwordLoginField;

    [Space]
    [Header("Registration Fields")]
    public InputField nameRegisterField;
    public InputField emailRegisterField;
    public InputField passwordRegisterField;
    public InputField confirmPasswordRegisterField;

    // --- Internal ---
    private bool isGoogleSignInInitialized = false;
    private bool firebaseReady = false;
    private readonly Queue<Action> mainThreadExecutionQueue = new Queue<Action>();

    // =========================================================================
    // --- INITIALIZATION ---
    // =========================================================================

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                EnqueueOnMainThread(() =>
                {
                    InitializeFirebase();
                });
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void Update()
    {
        lock (mainThreadExecutionQueue)
        {
            while (mainThreadExecutionQueue.Count > 0)
                mainThreadExecutionQueue.Dequeue().Invoke();
        }
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        firebaseReady = true;

        // Start the auto-login coroutine now that Firebase is ready on the main thread
        StartCoroutine(AutoLoginCoroutine());
    }

    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
                Debug.Log("Signed out: " + user.UserId);

            user = auth.CurrentUser;

            if (signedIn)
                Debug.Log("Signed in: " + user.UserId);
        }
    }

    private void EnqueueOnMainThread(Action action)
    {
        lock (mainThreadExecutionQueue)
        {
            mainThreadExecutionQueue.Enqueue(action);
        }
    }

    // =========================================================================
    // --- AUTO-LOGIN ON LAUNCH ---
    // =========================================================================

    /// <summary>
    /// Waits for Firebase to restore its cached auth session (up to a short
    /// timeout), then decides whether to jump to GameScene or stay on LoginScene.
    ///
    /// Why a coroutine?  Firebase restores its persistent token asynchronously
    /// after CheckAndFixDependencies completes. auth.CurrentUser can be null for
    /// a few frames even when the user IS still authenticated. Polling with a
    /// timeout gives the SDK time to hydrate the session before we decide.
    /// </summary>
    private IEnumerator AutoLoginCoroutine()
    {
        // No saved login on this device → stay on login screen immediately
        if (!LoginSaveManager.HasSavedLogin())
        {
            Debug.Log("[AutoLogin] No saved login. Showing login screen.");
            yield break;
        }

        Debug.Log("[AutoLogin] Saved login found. Waiting for Firebase session restore...");

        // Wait up to 3 seconds for Firebase to restore the cached token
        float timeout = 3f;
        float elapsed = 0f;

        while (auth.CurrentUser == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (auth.CurrentUser != null)
        {
            // Firebase confirmed the session is still valid
            string displayName = string.IsNullOrEmpty(auth.CurrentUser.DisplayName)
                ? LoginSaveManager.SavedDisplayName
                : auth.CurrentUser.DisplayName;

            Debug.Log($"[AutoLogin] Session restored for {displayName}. Going to GameScene.");
            References.userName = displayName;
            StartCoroutine(UIManager.Instance.PlayVideoAndLoad("GameScene"));
        }
        else
        {
            // Timed out — token expired, revoked, or no network to refresh
            Debug.LogWarning("[AutoLogin] Firebase session not restored within timeout. Clearing save and showing login.");
            LoginSaveManager.ClearLoginState();
        }
    }

    // =========================================================================
    // --- GOOGLE SIGN-IN ---
    // =========================================================================

    public void LoginWithGoogle()
    {
        if (!isGoogleSignInInitialized)
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = googleWebClientId,
                RequestEmail = true
            };
            isGoogleSignInInitialized = true;
        }

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogWarning("Google Sign-In cancelled.");
                EnqueueOnMainThread(() =>
                    UIManager.Instance.ShowLoginMessage("Google Sign-In cancelled.", isError: true));
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("Google Sign-In error: " + task.Exception);
                EnqueueOnMainThread(() =>
                {
                    string errorMsg = task.Exception?.GetBaseException().Message ?? "Unknown Error";
                    UIManager.Instance.ShowLoginMessage($"Google Error: {errorMsg}", isError: true);
                });
            }
            else
            {
                string idToken = task.Result.IdToken;
                Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
                EnqueueOnMainThread(() => StartCoroutine(LoginWithCredentialAsync(credential)));
            }
        });
    }

    private IEnumerator LoginWithCredentialAsync(Credential credential)
    {
        var loginTask = auth.SignInWithCredentialAsync(credential);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogError("Firebase Credential Auth Failed: " + loginTask.Exception);
            FirebaseException firebaseEx = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseEx.ErrorCode;

            string failedMessage = "Google Login Failed! Because ";
            switch (authError)
            {
                case AuthError.AccountExistsWithDifferentCredentials:
                    failedMessage += "An account already exists with this email using a different login method."; break;
                case AuthError.InvalidCredential:
                    failedMessage += "The login token is expired or invalid."; break;
                case AuthError.UserDisabled:
                    failedMessage += "This user account has been disabled."; break;
                default:
                    failedMessage += firebaseEx.Message; break;
            }
            UIManager.Instance.ShowLoginMessage(failedMessage, isError: true);
        }
        else
        {
            ProcessSuccessfulLogin(loginTask.Result);
        }
    }

    // =========================================================================
    // --- EMAIL / PASSWORD ---
    // =========================================================================

    public void Login()
    {
        StartCoroutine(LoginAsync(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogError(loginTask.Exception);
            FirebaseException firebaseEx = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseEx.ErrorCode;

            string failedMessage = "Login Failed! Because ";
            switch (authError)
            {
                case AuthError.InvalidEmail: failedMessage += "Email is invalid"; break;
                case AuthError.WrongPassword: failedMessage += "Wrong Password"; break;
                case AuthError.MissingEmail: failedMessage += "Email is missing"; break;
                case AuthError.MissingPassword: failedMessage += "Password is missing"; break;
                case AuthError.UserNotFound: failedMessage += "Account not found"; break;
                default:
                    string errorMsg = firebaseEx.Message.ToLower();
                    if (errorMsg.Contains("invalid_password") || errorMsg.Contains("wrong password"))
                        failedMessage += "Wrong Password";
                    else if (errorMsg.Contains("invalid_login_credentials") || errorMsg.Contains("invalid login credentials"))
                        failedMessage += "Invalid email or password";
                    else if (errorMsg.Contains("too_many_attempts_try_later"))
                        failedMessage += "Too many failed attempts. Try again later";
                    else if (errorMsg.Contains("user_not_found"))
                        failedMessage += "Account not found";
                    else
                    {
                        Debug.Log("Unhandled Firebase error: " + firebaseEx.Message);
                        failedMessage = "Login Failed. Please try again";
                    }
                    break;
            }
            UIManager.Instance.ShowLoginMessage(failedMessage, isError: true);
        }
        else
        {
            ProcessSuccessfulLogin(loginTask.Result.User);
        }
    }

    public void Register()
    {
        StartCoroutine(RegisterAsync(nameRegisterField.text, emailRegisterField.text,
            passwordRegisterField.text, confirmPasswordRegisterField.text));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        if (name == "")
        {
            UIManager.Instance.ShowRegistrationMessage("User Name is empty", isError: true);
        }
        else if (email == "")
        {
            UIManager.Instance.ShowRegistrationMessage("Email field is empty", isError: true);
        }
        else if (password != confirmPassword)
        {
            UIManager.Instance.ShowRegistrationMessage("Password does not match", isError: true);
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                Debug.LogError(registerTask.Exception);
                FirebaseException firebaseEx = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseEx.ErrorCode;

                string failedMessage = "Registration Failed! Because ";
                switch (authError)
                {
                    case AuthError.InvalidEmail: failedMessage += "Email is invalid"; break;
                    case AuthError.WrongPassword: failedMessage += "Wrong Password"; break;
                    case AuthError.MissingEmail: failedMessage += "Email is missing"; break;
                    case AuthError.MissingPassword: failedMessage += "Password is missing"; break;
                    default: failedMessage = "Registration Failed"; break;
                }
                UIManager.Instance.ShowRegistrationMessage(failedMessage, isError: true);
            }
            else
            {
                user = registerTask.Result.User;
                UserProfile userProfile = new UserProfile { DisplayName = name };
                var updateProfileTask = user.UpdateUserProfileAsync(userProfile);
                yield return new WaitUntil(() => updateProfileTask.IsCompleted);

                if (updateProfileTask.Exception != null)
                {
                    user.DeleteAsync();
                    Debug.LogError(updateProfileTask.Exception);
                    FirebaseException firebaseEx = updateProfileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError authError = (AuthError)firebaseEx.ErrorCode;

                    string failedMessage = "Profile Update Failed! Because ";
                    switch (authError)
                    {
                        case AuthError.InvalidEmail: failedMessage += "Email is invalid"; break;
                        case AuthError.EmailAlreadyInUse: failedMessage += "Email is already in use"; break;
                        case AuthError.WeakPassword: failedMessage += "Password is too weak"; break;
                        case AuthError.MissingEmail: failedMessage += "Email is missing"; break;
                        case AuthError.MissingPassword: failedMessage += "Password is missing"; break;
                        default: failedMessage = "Registration Failed"; break;
                    }
                    UIManager.Instance.ShowRegistrationMessage(failedMessage, isError: true);
                }
                else
                {
                    UIManager.Instance.ShowRegistrationMessage(
                        "Welcome " + user.DisplayName + "! Registration Successful.", isError: false);
                    UIManager.Instance.OpenLoginPanel();
                }
            }
        }
    }

    // =========================================================================
    // --- POST-LOGIN ---
    // =========================================================================

    private void ProcessSuccessfulLogin(FirebaseUser targetUser)
    {
        user = targetUser;
        string displayName = string.IsNullOrEmpty(user.DisplayName) ? "User" : user.DisplayName;

        // Persist login so next launch skips this scene
        LoginSaveManager.SaveLoginState(user.UserId, displayName);

        UIManager.Instance.ShowLoginMessage($"Welcome, {displayName}! Logged in successfully.", isError: false);
        References.userName = displayName;

        // Start the transition video sequence and change scene
        StartCoroutine(UIManager.Instance.PlayVideoAndLoad("GameScene"));
    }

    // =========================================================================
    // --- LOGOUT ---
    // =========================================================================

    /// <summary>
    /// Signs the user out and clears the saved login so next launch shows LoginScene.
    /// Wire this to your in-game logout/settings button.
    /// </summary>
    public void Logout()
    {
        if (auth != null)
            auth.SignOut();

        if (isGoogleSignInInitialized)
            GoogleSignIn.DefaultInstance.SignOut();

        LoginSaveManager.ClearLoginState();
        UnityEngine.SceneManagement.SceneManager.LoadScene("FirebaseLogin");
    }
}
