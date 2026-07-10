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
        FirebaseUserDataManager.Instance.AddCoins(amount);
    }

    public bool SpendCoins(int amount)
    {
        if (currentCoins < amount)
            return false;

        FirebaseUserDataManager.Instance.SaveCoins(currentCoins - amount);
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
