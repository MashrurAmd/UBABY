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

    // --- Threading & Initialization Management ---
    private bool isGoogleSignInInitialized = false;
    private readonly Queue<Action> mainThreadExecutionQueue = new Queue<Action>();

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void Update()
    {
        // Executes multi-threaded Firebase/Google background tasks back safely onto the Unity Main Thread
        lock (mainThreadExecutionQueue)
        {
            while (mainThreadExecutionQueue.Count > 0)
            {
                mainThreadExecutionQueue.Dequeue().Invoke();
            }
        }
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
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
    // --- GOOGLE SIGN-IN SYSTEM ---
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
                EnqueueOnMainThread(() => {
                    UIManager.Instance.ShowLoginMessage("Google Sign-In cancelled.", isError: true);
                });
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("Google Sign-In error: " + task.Exception);
                EnqueueOnMainThread(() => {
                    string errorMsg = task.Exception?.GetBaseException().Message ?? "Unknown Error";
                    UIManager.Instance.ShowLoginMessage($"Google Error: {errorMsg}", isError: true);
                });
            }
            else
            {
                Debug.Log("Google Auth Success. Exchanging token with Firebase...");
                string idToken = task.Result.IdToken;
                Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

                EnqueueOnMainThread(() => {
                    StartCoroutine(LoginWithCredentialAsync(credential));
                });
            }
        });
    }

    private IEnumerator LoginWithCredentialAsync(Credential credential)
    {
        var loginTask = auth.SignInWithCredentialAsync(credential);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogError("Firebase Credential Authentication Failed: " + loginTask.Exception);
            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Google Login Failed! Because ";
            switch (authError)
            {
                case AuthError.AccountExistsWithDifferentCredentials:
                    failedMessage += "An account already exists with this email using a different login method.";
                    break;
                case AuthError.InvalidCredential:
                    failedMessage += "The login token is expired or invalid.";
                    break;
                case AuthError.UserDisabled:
                    failedMessage += "This user account has been disabled.";
                    break;
                default:
                    failedMessage += firebaseException.Message;
                    break;
            }
            UIManager.Instance.ShowLoginMessage(failedMessage, isError: true);
        }
        else
        {
            ProcessSuccessfulLogin(loginTask.Result);
        }
    }

    // =========================================================================
    // --- EMAIL / PASSWORD SYSTEM ---
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
            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Login Failed! Because ";
            switch (authError)
            {
                case AuthError.InvalidEmail: failedMessage += "Email is invalid"; break;
                case AuthError.WrongPassword: failedMessage += "Wrong Password"; break;
                case AuthError.MissingEmail: failedMessage += "Email is missing"; break;
                case AuthError.MissingPassword: failedMessage += "Password is missing"; break;
                case AuthError.UserNotFound: failedMessage += "Account not found"; break;
                default:
                    string errorMsg = firebaseException.Message.ToLower();
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
                        Debug.Log("Unhandled Firebase error: " + firebaseException.Message);
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
                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseException.ErrorCode;

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
                    FirebaseException firebaseException = updateProfileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError authError = (AuthError)firebaseException.ErrorCode;

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
                    UIManager.Instance.ShowRegistrationMessage("Welcome " + user.DisplayName + "! Registration Successful.", isError: false);
                    UIManager.Instance.OpenLoginPanel();
                }
            }
        }
    }

    // =========================================================================
    // --- POST-LOGIN FLOW ---
    // =========================================================================

    private void ProcessSuccessfulLogin(FirebaseUser targetUser)
    {
        user = targetUser;
        string displayName = string.IsNullOrEmpty(user.DisplayName) ? "User" : user.DisplayName;

        UIManager.Instance.ShowLoginMessage($"Welcome, {displayName}! Logged in successfully.", isError: false);
        References.userName = displayName;

        // Start the transition video sequence and change scene
        StartCoroutine(UIManager.Instance.PlayVideoAndLoad("GameScene"));
    }
}