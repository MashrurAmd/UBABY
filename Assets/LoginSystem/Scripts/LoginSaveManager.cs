using UnityEngine;

/// <summary>
/// Manages login persistence across game sessions using PlayerPrefs.
/// Stores the Firebase UID and display name so the game can skip the
/// login scene on the next launch if the user is already authenticated.
/// </summary>
public static class LoginSaveManager
{
    // PlayerPrefs keys
    private const string KEY_IS_LOGGED_IN  = "UBaby_IsLoggedIn";
    private const string KEY_USER_ID       = "UBaby_UserId";
    private const string KEY_DISPLAY_NAME  = "UBaby_DisplayName";

    /// <summary>
    /// Call this right after a successful Firebase login to persist the session.
    /// </summary>
    public static void SaveLoginState(string userId, string displayName)
    {
        PlayerPrefs.SetInt(KEY_IS_LOGGED_IN, 1);
        PlayerPrefs.SetString(KEY_USER_ID, userId);
        PlayerPrefs.SetString(KEY_DISPLAY_NAME, displayName ?? "User");
        PlayerPrefs.Save();

        Debug.Log($"[LoginSaveManager] Login state saved for: {displayName} ({userId})");
    }

    /// <summary>
    /// Call this on logout or when Firebase token is invalid/expired.
    /// </summary>
    public static void ClearLoginState()
    {
        PlayerPrefs.DeleteKey(KEY_IS_LOGGED_IN);
        PlayerPrefs.DeleteKey(KEY_USER_ID);
        PlayerPrefs.DeleteKey(KEY_DISPLAY_NAME);
        PlayerPrefs.Save();

        Debug.Log("[LoginSaveManager] Login state cleared.");
    }

    /// <summary>
    /// Returns true if a previous login has been saved on this device.
    /// </summary>
    public static bool HasSavedLogin()
    {
        return PlayerPrefs.GetInt(KEY_IS_LOGGED_IN, 0) == 1;
    }

    /// <summary>
    /// The saved Firebase UID, or empty string if none.
    /// </summary>
    public static string SavedUserId =>
        PlayerPrefs.GetString(KEY_USER_ID, string.Empty);

    /// <summary>
    /// The saved display name, or "User" if none.
    /// </summary>
    public static string SavedDisplayName =>
        PlayerPrefs.GetString(KEY_DISPLAY_NAME, "User");
}
