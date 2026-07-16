using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;

public class FirebaseRedemptionManager : MonoBehaviour
{
    public FirebaseAuth auth;
    public DatabaseReference dbRef;

    [Header("UI")]
    public InputField redemptionInput;
    public Text coinsText;
    public Text popupText;

    [Header("Popup Settings")]
    public float popupDuration = 1f;
    public Vector3 popupMoveOffset = new Vector3(0, 40, 0);

    void Start()
    {
        StartCoroutine(InitFirebaseSafe());
    }

    IEnumerator InitFirebaseSafe()
    {
        var depTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => depTask.IsCompleted);

        if (depTask.Result != DependencyStatus.Available)
        {
            Debug.LogError("Firebase dependency error: " + depTask.Result);
            yield break;
        }

        auth = FirebaseAuth.DefaultInstance;
        
        FirebaseApp app = FirebaseApp.DefaultInstance;
        if (app != null)
        {
            dbRef = FirebaseDatabase.GetInstance(app).RootReference;
        }
        else
        {
            dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        }

        if (FirebaseUserDataManager.Instance != null)
        {
            FirebaseUserDataManager.Instance.OnCoinsUpdated += UpdateCoinsUI;
            UpdateCoinsUI(FirebaseUserDataManager.Instance.currentCoins);
        }
        else
        {
            Debug.LogWarning("FirebaseUserDataManager.Instance is null during initialization.");
        }
    }

    void OnDestroy()
    {
        if (FirebaseUserDataManager.Instance != null)
            FirebaseUserDataManager.Instance.OnCoinsUpdated -= UpdateCoinsUI;
    }

    public void Redeem()
    {
        StartCoroutine(RedeemProcess(redemptionInput.text.Trim()));
    }

    IEnumerator RedeemProcess(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            ShowPopup("Enter code ❌", Color.red);
            yield break;
        }

        if (auth == null)
        {
            auth = FirebaseAuth.DefaultInstance;
        }

        if (auth == null || auth.CurrentUser == null)
        {
            ShowPopup("Not Logged In ❌", Color.red);
            Debug.LogError("Cannot redeem code: Firebase Auth or CurrentUser is null.");
            yield break;
        }

        if (dbRef == null)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            if (app != null)
            {
                dbRef = FirebaseDatabase.GetInstance(app).RootReference;
            }
        }

        if (dbRef == null)
        {
            ShowPopup("Database Error ❌", Color.red);
            Debug.LogError("Cannot redeem code: Firebase Database Reference is null.");
            yield break;
        }

        string uid = auth.CurrentUser.UserId;

        var task = dbRef.Child("RedemptionCodes").Child(code).GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Result == null || !task.Result.Exists)
        {
            ShowPopup("Invalid Code ❌", Color.red);
            yield break;
        }

        bool used = Convert.ToBoolean(task.Result.Child("used").Value);
        int coins = Convert.ToInt32(task.Result.Child("coins").Value);

        if (used)
        {
            ShowPopup("Already Used ⚠️", Color.yellow);
            yield break;
        }

        long serverTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        dbRef.Child("RedemptionCodes").Child(code).Child("used").SetValueAsync(true);
        dbRef.Child("RedemptionCodes").Child(code).Child("redeemedBy").SetValueAsync(uid);
        dbRef.Child("RedemptionCodes").Child(code).Child("usedAt").SetValueAsync(serverTime);

        if (FirebaseUserDataManager.Instance != null)
        {
            FirebaseUserDataManager.Instance.AddCoins(coins);
        }
        else
        {
            Debug.LogError("FirebaseUserDataManager.Instance is null. Unable to add redeemed coins.");
            ShowPopup("Error Adding Coins ❌", Color.red);
            yield break;
        }

        ShowPopup("+" + coins + " 💰", Color.green);
        redemptionInput.text = "";
    }

    void UpdateCoinsUI(int coins)
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();
    }

    void ShowPopup(string msg, Color col)
    {
        StopAllCoroutines();
        StartCoroutine(PopupRoutine(msg, col));
    }

    IEnumerator PopupRoutine(string msg, Color col)
    {
        popupText.gameObject.SetActive(true);
        popupText.text = msg;
        popupText.color = col;

        Vector3 startPos = popupText.transform.localPosition;
        Vector3 endPos = startPos + popupMoveOffset;

        float t = 0;
        while (t < popupDuration)
        {
            t += Time.deltaTime;
            float p = t / popupDuration;

            popupText.transform.localPosition = Vector3.Lerp(startPos, endPos, p);
            popupText.color = new Color(col.r, col.g, col.b, 1f - p);
            yield return null;
        }

        popupText.transform.localPosition = startPos;
        popupText.gameObject.SetActive(false);
    }
}
