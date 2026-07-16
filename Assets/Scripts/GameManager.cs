using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Coins")]
    public int currentCoins;
    public Text currentCoinsText;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Listen to Firebase coin updates
        FirebaseUserDataManager.Instance.OnCoinsUpdated += OnFirebaseCoinsUpdated;

        // If already loaded
        currentCoins = FirebaseUserDataManager.Instance.currentCoins;
        UpdateUI();
    }

    void OnDestroy()
    {
        if (FirebaseUserDataManager.Instance != null)
            FirebaseUserDataManager.Instance.OnCoinsUpdated -= OnFirebaseCoinsUpdated;
    }

    void OnFirebaseCoinsUpdated(int coins)
    {
        // Authoritative reconciliation from the server. Under normal
        // conditions this just confirms the optimistic update already
        // applied in AddCoins/SpendCoins below; if the server value ever
        // differs (failed write, another device, etc.) this corrects it.
        currentCoins = coins;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (currentCoinsText != null)
            currentCoinsText.text = currentCoins.ToString();
    }

    public void AddCoins(int amount)
    {
        // Update the local, authoritative-for-this-session value and the
        // UI immediately (optimistic update). Without this, currentCoins
        // only changes once the Firebase round-trip completes, which
        // means any check made in the meantime (e.g. a second purchase
        // fired before the first one's confirmation arrives) reads a
        // stale balance and can pass when it shouldn't.
        currentCoins += amount;
        UpdateUI();

        FirebaseUserDataManager.Instance.AddCoins(amount);
    }

    public bool SpendCoins(int amount)
    {
        if (currentCoins < amount)
            return false;

        // Same optimistic-update reasoning as AddCoins: deduct locally
        // and refresh the UI right away, then persist to Firebase.
        currentCoins -= amount;
        UpdateUI();

        FirebaseUserDataManager.Instance.SaveCoins(currentCoins);
        return true;
    }

    [Header("Website")]
    public string websiteURL = "https://babyu.tech/topup";

    public void VisitWebsite()
    {
        if (!string.IsNullOrEmpty(websiteURL))
        {
            Application.OpenURL(websiteURL);
        }
        else
        {
            Debug.LogWarning("Website URL is empty!");
        }
    }
}