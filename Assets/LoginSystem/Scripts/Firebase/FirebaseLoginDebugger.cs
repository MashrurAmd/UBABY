using UnityEngine;
using Firebase.Auth;

public class FirebaseLoginDebugger : MonoBehaviour
{
    void Start()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            Debug.Log("✅ Logged In Email: " + auth.CurrentUser.Email);
            Debug.Log("🆔 User ID: " + auth.CurrentUser.UserId);
            Debug.Log("👤 Display Name: " + auth.CurrentUser.DisplayName);
        }
        else
        {
            Debug.Log("❌ No user logged in.");
        }
    }
}
