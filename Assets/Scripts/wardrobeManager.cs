using UnityEngine;
using UnityEngine.UI;

public class WardrobeManager : MonoBehaviour
{
    [Header("Glasses")]
    public GameObject[] glasses;
    public Text glassesNameText;
    public string[] glassesNames;

    [Header("Sounds")]
    public AudioSource glassesSound;  // drag glass change sound here
    public AudioSource watchSound;    // drag watch change sound here
    public AudioSource hatSound;      // drag hat change sound here
    public AudioSource purchaseSound; // ✅ "adorable" SFX — plays whenever a new outfit item is bought

    [Header("Shop UI")]
    public Text priceText;          // shows price of current item
    public Text statusText;         // shows "Owned" or price
    public GameObject buyButton;    // the buy button

    [Header("Prices")]
    public int[] glassesPrices;  // set prices in Inspector for each glass
    public int[] watchPrices;    // set prices in Inspector for each watch
    public int[] hatPrices;      // set prices in Inspector for each hat

    [Header("References")]
    public GameManager gameManager;       // drag GameManager here
    public GameObject notEnoughCoinsUI;   // drag a popup UI here
    public Text notEnoughCoinsText;       // drag the Text inside that popup here
    public GameObject getCoinsUI;         // drag Get Coins UI panel here
    public Color normalPriceColor = Color.white;
    public Color ownedColor = Color.green;
    public Color notEnoughCoinsColor = Color.red;
    public Color alreadyPurchasedColor = Color.yellow;
    public float notEnoughCoinsDuration = 2f; // how long the popup stays visible
    private Coroutine notEnoughCoinsRoutine;

    [Header("Hats")]
    public GameObject[] hats;
    public Text hatNameText;
    public string[] hatNames;

    [Header("Watches")]
    public GameObject[] watches;
    public Text watchNameText;
    public string[] watchNames;

    [Header("Navigation Buttons")]
    public GameObject leftButton;
    public GameObject rightButton;

    [Header("Category Button Images")]
    public Image glassesButtonImage;
    public Image watchButtonImage;
    public Image hatButtonImage;
    public Image cardIconImage;          // preview icon inside Store_Card frame
    public Sprite glassesActiveSprite;
    public Sprite glassesInactiveSprite;
    public Sprite watchActiveSprite;
    public Sprite watchInactiveSprite;
    public Sprite hatActiveSprite;
    public Sprite hatInactiveSprite;

    private int currentGlassIndex = -1;
    private int currentWatchIndex = -1;
    private int currentHatIndex = -1;

    private enum ActiveCategory { None, Glasses, Watch, Hat }
    private ActiveCategory activeCategory = ActiveCategory.None;

    // ===========================
    // 💾 SAVE / LOAD (PlayerPrefs)
    // ===========================
    private const string PP_GLASSES = "Wardrobe_GlassIndex";
    private const string PP_WATCH = "Wardrobe_WatchIndex";
    private const string PP_HAT = "Wardrobe_HatIndex";

    public static WardrobeManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadWardrobe();
    }

    private void OnDisable()
    {
        RevertToSaved();
    }

    public void RevertToSaved()
    {
        LoadWardrobe();
        activeCategory = ActiveCategory.None;
        if (leftButton != null) leftButton.SetActive(false);
        if (rightButton != null) rightButton.SetActive(false);

        if (glassesButtonImage != null && glassesInactiveSprite != null) glassesButtonImage.sprite = glassesInactiveSprite;
        if (watchButtonImage != null && watchInactiveSprite != null) watchButtonImage.sprite = watchInactiveSprite;
        if (hatButtonImage != null && hatInactiveSprite != null) hatButtonImage.sprite = hatInactiveSprite;
    }

    void LoadWardrobe()
    {
        currentGlassIndex = PlayerPrefs.GetInt(PP_GLASSES, -1);
        currentWatchIndex = PlayerPrefs.GetInt(PP_WATCH, -1);
        currentHatIndex = PlayerPrefs.GetInt(PP_HAT, -1);

        if (glasses != null)
        {
            for (int i = 0; i < glasses.Length; i++)
                if (glasses[i] != null) glasses[i].SetActive(false);
            if (currentGlassIndex >= 0 && currentGlassIndex < glasses.Length && glasses[currentGlassIndex] != null)
                glasses[currentGlassIndex].SetActive(true);
        }

        if (watches != null)
        {
            for (int i = 0; i < watches.Length; i++)
                if (watches[i] != null) watches[i].SetActive(false);
            if (currentWatchIndex >= 0 && currentWatchIndex < watches.Length && watches[currentWatchIndex] != null)
                watches[currentWatchIndex].SetActive(true);
        }

        if (hats != null)
        {
            for (int i = 0; i < hats.Length; i++)
                if (hats[i] != null) hats[i].SetActive(false);
            if (currentHatIndex >= 0 && currentHatIndex < hats.Length && hats[currentHatIndex] != null)
                hats[currentHatIndex].SetActive(true);
        }
    }

    // ===========================
    // 👓 GLASSES
    // ===========================

    public void OnGlassesButtonClicked()
    {
        activeCategory = ActiveCategory.Glasses;

        if (leftButton != null) leftButton.SetActive(true);
        if (rightButton != null) rightButton.SetActive(true);

        if (glassesButtonImage != null) glassesButtonImage.sprite = glassesActiveSprite;
        if (watchButtonImage != null) watchButtonImage.sprite = watchInactiveSprite;
        if (hatButtonImage != null) hatButtonImage.sprite = hatInactiveSprite;
        if (cardIconImage != null && glassesActiveSprite != null) cardIconImage.sprite = glassesActiveSprite;

        RefreshGlassInfo(currentGlassIndex);
    }

    public void NextGlass()
    {
        currentGlassIndex++;
        if (currentGlassIndex >= glasses.Length)
            currentGlassIndex = -1;
        RefreshGlassInfo(currentGlassIndex);
        PlayCategorySound(glassesSound);
    }

    public void PreviousGlass()
    {
        currentGlassIndex--;
        if (currentGlassIndex < -1)
            currentGlassIndex = glasses.Length - 1;
        RefreshGlassInfo(currentGlassIndex);
        PlayCategorySound(glassesSound);
    }

    void RefreshGlassInfo(int index)
    {
        for (int i = 0; i < glasses.Length; i++)
            if (glasses[i] != null) glasses[i].SetActive(false);

        currentGlassIndex = index;
        HideStatusPopup();

        if (index == -1)
        {
            if (glassesNameText != null) glassesNameText.text = "None";
            if (priceText != null) { priceText.text = ""; priceText.color = normalPriceColor; }
            if (buyButton != null) buyButton.SetActive(false);

            PlayerPrefs.SetInt(PP_GLASSES, -1);
            PlayerPrefs.Save();
            return;
        }

        if (glasses[index] != null) glasses[index].SetActive(true);

        if (IsGlassPurchased(index))
        {
            int price = glassesPrices.Length > index ? glassesPrices[index] : 0;
            SetPriceStatus(price);
            if (buyButton != null) buyButton.SetActive(true);
            ShowAlreadyPurchasedPopup();

            PlayerPrefs.SetInt(PP_GLASSES, index);
            PlayerPrefs.Save();
        }
        else
        {
            int price = glassesPrices.Length > index ? glassesPrices[index] : 0;
            SetPriceStatus(price);
            if (buyButton != null) buyButton.SetActive(true);
        }

        if (glassesNames.Length > index && glassesNameText != null)
            glassesNameText.text = glassesNames[index];
    }

    public void HideAllGlasses()
    {
        RefreshGlassInfo(-1);
        if (glassesButtonImage != null) glassesButtonImage.sprite = glassesInactiveSprite;
    }

    // ===========================
    // ⌚ WATCHES
    // ===========================

    public void OnWatchButtonClicked()
    {
        activeCategory = ActiveCategory.Watch;

        if (leftButton != null) leftButton.SetActive(true);
        if (rightButton != null) rightButton.SetActive(true);

        if (watchButtonImage != null) watchButtonImage.sprite = watchActiveSprite;
        if (glassesButtonImage != null) glassesButtonImage.sprite = glassesInactiveSprite;
        if (hatButtonImage != null) hatButtonImage.sprite = hatInactiveSprite;
        if (cardIconImage != null && watchActiveSprite != null) cardIconImage.sprite = watchActiveSprite;

        RefreshWatchInfo(currentWatchIndex);
    }

    public void NextWatch()
    {
        currentWatchIndex++;
        if (currentWatchIndex >= watches.Length)
            currentWatchIndex = -1;
        RefreshWatchInfo(currentWatchIndex);
        PlayCategorySound(watchSound);
    }

    public void PreviousWatch()
    {
        currentWatchIndex--;
        if (currentWatchIndex < -1)
            currentWatchIndex = watches.Length - 1;
        RefreshWatchInfo(currentWatchIndex);
        PlayCategorySound(watchSound);
    }

    void RefreshWatchInfo(int index)
    {
        for (int i = 0; i < watches.Length; i++)
            if (watches[i] != null) watches[i].SetActive(false);

        currentWatchIndex = index;
        HideStatusPopup();

        if (index == -1)
        {
            if (watchNameText != null) watchNameText.text = "None";
            if (priceText != null) { priceText.text = ""; priceText.color = normalPriceColor; }
            if (buyButton != null) buyButton.SetActive(false);

            PlayerPrefs.SetInt(PP_WATCH, -1);
            PlayerPrefs.Save();
            return;
        }

        if (watches[index] != null) watches[index].SetActive(true);

        if (IsWatchPurchased(index))
        {
            int price = watchPrices.Length > index ? watchPrices[index] : 0;
            SetPriceStatus(price);
            if (buyButton != null) buyButton.SetActive(true);
            ShowAlreadyPurchasedPopup();

            PlayerPrefs.SetInt(PP_WATCH, index);
            PlayerPrefs.Save();
        }
        else
        {
            int price = watchPrices.Length > index ? watchPrices[index] : 0;
            SetPriceStatus(price);
            if (buyButton != null) buyButton.SetActive(true);
        }

        if (watchNames.Length > index && watchNameText != null)
            watchNameText.text = watchNames[index];
    }

    public void HideAllWatches()
    {
        RefreshWatchInfo(-1);
        if (watchButtonImage != null) watchButtonImage.sprite = watchInactiveSprite;
    }

    // ===========================
    // 🎩 HATS
    // ===========================

    public void OnHatButtonClicked()
    {
        activeCategory = ActiveCategory.Hat;

        if (leftButton != null) leftButton.SetActive(true);
        if (rightButton != null) rightButton.SetActive(true);

        if (hatButtonImage != null) hatButtonImage.sprite = hatActiveSprite;
        if (glassesButtonImage != null) glassesButtonImage.sprite = glassesInactiveSprite;
        if (watchButtonImage != null) watchButtonImage.sprite = watchInactiveSprite;
        if (cardIconImage != null && hatActiveSprite != null) cardIconImage.sprite = hatActiveSprite;

        RefreshHatInfo(currentHatIndex);
    }

    public void NextHat()
    {
        currentHatIndex++;
        if (currentHatIndex >= hats.Length)
            currentHatIndex = -1;
        RefreshHatInfo(currentHatIndex);
        PlayCategorySound(hatSound);
    }

    public void PreviousHat()
    {
        currentHatIndex--;
        if (currentHatIndex < -1)
            currentHatIndex = hats.Length - 1;
        RefreshHatInfo(currentHatIndex);
        PlayCategorySound(hatSound);
    }

    void RefreshHatInfo(int index)
    {
        for (int i = 0; i < hats.Length; i++)
            if (hats[i] != null) hats[i].SetActive(false);

        currentHatIndex = index;
        HideStatusPopup();

        if (index == -1)
        {
            if (hatNameText != null) hatNameText.text = "None";
            if (priceText != null) { priceText.text = ""; priceText.color = normalPriceColor; }
            if (buyButton != null) buyButton.SetActive(false);

            PlayerPrefs.SetInt(PP_HAT, -1);
            PlayerPrefs.Save();
            return;
        }

        if (hats[index] != null) hats[index].SetActive(true);

        if (IsHatPurchased(index))
        {
            int price = hatPrices.Length > index ? hatPrices[index] : 0;
            SetPriceStatus(price);
            if (buyButton != null) buyButton.SetActive(true);
            ShowAlreadyPurchasedPopup();

            PlayerPrefs.SetInt(PP_HAT, index);
            PlayerPrefs.Save();
        }
        else
        {
            int price = hatPrices.Length > index ? hatPrices[index] : 0;
            SetPriceStatus(price);
            if (buyButton != null) buyButton.SetActive(true);
        }

        if (hatNames.Length > index && hatNameText != null)
            hatNameText.text = hatNames[index];
    }

    public void HideAllHats()
    {
        RefreshHatInfo(-1);
        if (hatButtonImage != null) hatButtonImage.sprite = hatInactiveSprite;
    }

    // ===========================
    // 👗 DRESSES (STUBS FOR UI COMPATIBILITY)
    // ===========================

    public void OnDressButtonClicked() { }
    public void NextDress() { }
    public void PreviousDress() { }
    public void HideAllDresses() { }

    // ===========================
    // 🎨 STATUS TEXT HELPERS
    // ===========================

    void SetPriceStatus(int price)
    {
        if (priceText != null)
        {
            priceText.text = price + " 🪙";
            priceText.color = normalPriceColor;
        }
    }

    void ShowAlreadyPurchasedPopup()
    {
        ShowStatusPopup("Already Purchased", alreadyPurchasedColor);
    }

    void ShowNotEnoughCoins()
    {
        ShowStatusPopup("Not enough coins!", notEnoughCoinsColor);
        if (getCoinsUI != null)
        {
            getCoinsUI.SetActive(true);
        }
    }

    void ShowPurchasedPopup()
    {
        ShowStatusPopup("Purchased", ownedColor);
    }

    void ShowStatusPopup(string message, Color color)
    {
        if (notEnoughCoinsUI != null)
            notEnoughCoinsUI.SetActive(true);

        if (notEnoughCoinsText != null)
        {
            notEnoughCoinsText.text = message;
            notEnoughCoinsText.color = color;
        }

        if (notEnoughCoinsRoutine != null)
            StopCoroutine(notEnoughCoinsRoutine);
        notEnoughCoinsRoutine = StartCoroutine(HideNotEnoughCoinsAfterDelay());
    }

    System.Collections.IEnumerator HideNotEnoughCoinsAfterDelay()
    {
        yield return new WaitForSeconds(notEnoughCoinsDuration);
        if (notEnoughCoinsUI != null)
            notEnoughCoinsUI.SetActive(false);
        notEnoughCoinsRoutine = null;
    }

    void HideStatusPopup()
    {
        if (notEnoughCoinsRoutine != null)
        {
            StopCoroutine(notEnoughCoinsRoutine);
            notEnoughCoinsRoutine = null;
        }
        if (notEnoughCoinsUI != null)
            notEnoughCoinsUI.SetActive(false);
    }

    // ===========================
    // 🛒 BUY BUTTON
    // ===========================

    public void BuyCurrentItem()
    {
        switch (activeCategory)
        {
            case ActiveCategory.Glasses: BuyGlass(currentGlassIndex); break;
            case ActiveCategory.Watch: BuyWatch(currentWatchIndex); break;
            case ActiveCategory.Hat: BuyHat(currentHatIndex); break;
        }
    }

    void BuyGlass(int index)
    {
        if (index < 0) return;
        if (IsGlassPurchased(index)) { ShowAlreadyPurchasedPopup(); return; }

        int price = glassesPrices.Length > index ? glassesPrices[index] : 0;
        if (gameManager != null && gameManager.SpendCoins(price))
        {
            MarkGlassPurchased(index);
            SetPriceStatus(price);
            if (buyButton != null) buyButton.SetActive(true);
            ShowPurchasedPopup();
            PlayerPrefs.SetInt(PP_GLASSES, index);
            PlayerPrefs.Save();
            PlayCategorySound(purchaseSound);
            Debug.Log("✅ Glass purchased!");
        }
        else
        {
            ShowNotEnoughCoins();
            Debug.Log("❌ Not enough coins!");
        }
    }

    void BuyWatch(int index)
    {
        if (index < 0) return;
        if (IsWatchPurchased(index)) { ShowAlreadyPurchasedPopup(); return; }

        int price = watchPrices.Length > index ? watchPrices[index] : 0;
        if (gameManager != null && gameManager.SpendCoins(price))
        {
            MarkWatchPurchased(index);
            SetPriceStatus(price);
            if (buyButton != null) buyButton.SetActive(true);
            ShowPurchasedPopup();
            PlayerPrefs.SetInt(PP_WATCH, index);
            PlayerPrefs.Save();
            PlayCategorySound(purchaseSound);
            Debug.Log("✅ Watch purchased!");
        }
        else
        {
            ShowNotEnoughCoins();
            Debug.Log("❌ Not enough coins!");
        }
    }

    void BuyHat(int index)
    {
        if (index < 0) return;
        if (IsHatPurchased(index)) { ShowAlreadyPurchasedPopup(); return; }

        int price = hatPrices.Length > index ? hatPrices[index] : 0;
        if (gameManager != null && gameManager.SpendCoins(price))
        {
            MarkHatPurchased(index);
            SetPriceStatus(price);
            if (buyButton != null) buyButton.SetActive(true);
            ShowPurchasedPopup();
            PlayerPrefs.SetInt(PP_HAT, index);
            PlayerPrefs.Save();
            PlayCategorySound(purchaseSound);
            Debug.Log("✅ Hat purchased!");
        }
        else
        {
            ShowNotEnoughCoins();
            Debug.Log("❌ Not enough coins!");
        }
    }

    // ===========================
    // 💰 PURCHASE TRACKING
    // ===========================

    bool IsGlassPurchased(int index)
    {
        return PlayerPrefs.GetInt("Glass_Purchased_" + index, 0) == 1;
    }

    void MarkGlassPurchased(int index)
    {
        PlayerPrefs.SetInt("Glass_Purchased_" + index, 1);
        PlayerPrefs.Save();
    }

    bool IsWatchPurchased(int index)
    {
        return PlayerPrefs.GetInt("Watch_Purchased_" + index, 0) == 1;
    }

    void MarkWatchPurchased(int index)
    {
        PlayerPrefs.SetInt("Watch_Purchased_" + index, 1);
        PlayerPrefs.Save();
    }

    bool IsHatPurchased(int index)
    {
        return PlayerPrefs.GetInt("Hat_Purchased_" + index, 0) == 1;
    }

    void MarkHatPurchased(int index)
    {
        PlayerPrefs.SetInt("Hat_Purchased_" + index, 1);
        PlayerPrefs.Save();
    }

    // ===========================
    // 🔄 SHARED LEFT/RIGHT BUTTONS
    // ===========================

    public void OnRightButtonClicked()
    {
        switch (activeCategory)
        {
            case ActiveCategory.Glasses: NextGlass(); break;
            case ActiveCategory.Watch: NextWatch(); break;
            case ActiveCategory.Hat: NextHat(); break;
        }
    }

    public void OnLeftButtonClicked()
    {
        switch (activeCategory)
        {
            case ActiveCategory.Glasses: PreviousGlass(); break;
            case ActiveCategory.Watch: PreviousWatch(); break;
            case ActiveCategory.Hat: PreviousHat(); break;
        }
    }

    void PlayCategorySound(AudioSource source)
    {
        if (source != null && !source.isPlaying)
            source.Play();
    }

    // ===========================
    // 🔄 RESET ALL
    // ===========================

    public void ResetAll()
    {
        HideAllGlasses();
        HideAllWatches();
        HideAllHats();
        activeCategory = ActiveCategory.None;
        if (leftButton != null) leftButton.SetActive(false);
        if (rightButton != null) rightButton.SetActive(false);
    }
}