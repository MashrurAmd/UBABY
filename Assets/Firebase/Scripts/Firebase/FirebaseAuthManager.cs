using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;

public class FirebaseAuthManager : MonoBehaviour
{
    // Firebase variable
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;

    // Login Variables
    [Space]
    [Header("Login")]
    public InputField emailLoginField;
    public InputField passwordLoginField;

    // Registration Variables
    [Space]
    [Header("Registration")]
    public InputField nameRegisterField;
    public InputField emailRegisterField;
    public InputField passwordRegisterField;
    public InputField confirmPasswordRegisterField;

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

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
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
                case AuthError.InvalidEmail:
                    failedMessage += "Email is invalid";
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Wrong Password";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email is missing";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Password is missing";
                    break;
                case AuthError.UserNotFound:
                    failedMessage += "Account not found";
                    break;
                default:
                    // Firebase sometimes wraps wrong password as a generic internal error
                    // so we check the message string as a fallback
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
                        // Temporary: log the exact message so you can see what Firebase is returning
                        Debug.Log("Unhandled Firebase error: " + firebaseException.Message);
                        failedMessage = "Login Failed. Please try again";
                    }
                    break;
            }

            UIManager.Instance.ShowLoginMessage(failedMessage, isError: true);
        }
        //else
        //{
        //    user = loginTask.Result.User;

        //    UIManager.Instance.ShowLoginMessage("Welcome, " + user.DisplayName + "! Logged in successfully.", isError: false);

        //    References.userName = user.DisplayName;
        //    UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        //}
        else
        {
            user = loginTask.Result.User;
            UIManager.Instance.ShowLoginMessage("Welcome, " + user.DisplayName + "! Logged in successfully.", isError: false);
            References.userName = user.DisplayName;

            // ✅ Fill slider then load scene
            yield return StartCoroutine(UIManager.Instance.FillSliderAndLoad("GameScene"));
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
                    case AuthError.InvalidEmail:
                        failedMessage += "Email is invalid";
                        break;
                    case AuthError.WrongPassword:
                        failedMessage += "Wrong Password";
                        break;
                    case AuthError.MissingEmail:
                        failedMessage += "Email is missing";
                        break;
                    case AuthError.MissingPassword:
                        failedMessage += "Password is missing";
                        break;
                    default:
                        failedMessage = "Registration Failed";
                        break;
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
                        case AuthError.InvalidEmail:
                            failedMessage += "Email is invalid";
                            break;
                        case AuthError.EmailAlreadyInUse:
                            failedMessage += "Email is already in use";
                            break;
                        case AuthError.WeakPassword:
                            failedMessage += "Password is too weak";
                            break;
                        case AuthError.MissingEmail:
                            failedMessage += "Email is missing";
                            break;
                        case AuthError.MissingPassword:
                            failedMessage += "Password is missing";
                            break;
                        default:
                            failedMessage = "Registration Failed";
                            break;
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
}