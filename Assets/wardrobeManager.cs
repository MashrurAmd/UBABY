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
    public AudioSource dressSound;    // drag dress change sound here
    public AudioSource purchaseSound; // ✅ NEW: "adorable" SFX — plays whenever a new outfit item is bought

    [Header("Shop UI")]
    public Text priceText;          // shows price of current item
    public Text statusText;         // shows "Owned" or price
    public GameObject buyButton;    // the buy button

    [Header("Prices")]
    public int[] glassesPrices;  // set prices in Inspector for each glass
    public int[] watchPrices;    // set prices in Inspector for each watch
    public int[] hatPrices;      // set prices in Inspector for each hat
    public int[] dressPrices;    // set prices in Inspector for each dress

    [Header("References")]
    public GameManager gameManager;       // drag GameManager here
    public GameObject notEnoughCoinsUI;   // drag a popup UI here

    [Header("Hats")]
    public GameObject[] hats;
    public Text hatNameText;
    public string[] hatNames;

    [Header("Watches")]
    public GameObject[] watches;
    public Text watchNameText;
    public string[] watchNames;

    [Header("Dresses")]
    public GameObject[] dresses;        // drag 3 dresses here
    public Text dressNameText;          // text to show dress name
    public string[] dressNames;         // names like "Red Dress", "Blue Dress" etc

    [Header("Navigation Buttons")]
    public GameObject leftButton;
    public GameObject rightButton;

    [Header("Category Button Images")]
    public Image glassesButtonImage;
    public Image watchButtonImage;
    public Image hatButtonImage;
    public Image dressButtonImage;
    public Sprite glassesActiveSprite;
    public Sprite glassesInactiveSprite;
    public Sprite watchActiveSprite;
    public Sprite watchInactiveSprite;
    public Sprite hatActiveSprite;
    public Sprite hatInactiveSprite;
    public Sprite dressActiveSprite;
    public Sprite dressInactiveSprite;

    // -1 means "None" (nothing equipped) for every category, and is a
    // valid, reachable state via the left/right browse buttons.
    private int currentGlassIndex = -1;
    private int currentWatchIndex = -1;
    private int currentHatIndex = -1;
    private int currentDressIndex = -1;

    private enum ActiveCategory { None, Glasses, Watch, Hat, Dress }
    private ActiveCategory activeCategory = ActiveCategory.None;

    // ===========================
    // 💾 SAVE / LOAD (PlayerPrefs)
    // ===========================
    private const string PP_GLASSES = "Wardrobe_GlassIndex";
    private const string PP_WATCH = "Wardrobe_WatchIndex";
    private const string PP_HAT = "Wardrobe_HatIndex";
    private const string PP_DRESS = "Wardrobe_DressIndex";

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
        if (dressButtonImage != null && dressInactiveSprite != null) dressButtonImage.sprite = dressInactiveSprite;
    }

    // Reads the saved indices and re-activates the correct item GameObjects
    // so equipped items are still visible after the game is closed and reopened.
    void LoadWardrobe()
    {
        currentGlassIndex = PlayerPrefs.GetInt(PP_GLASSES, -1);
        currentWatchIndex = PlayerPrefs.GetInt(PP_WATCH, -1);
        currentHatIndex = PlayerPrefs.GetInt(PP_HAT, -1);
        currentDressIndex = PlayerPrefs.GetInt(PP_DRESS, -1);

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

        if (dresses != null)
        {
            for (int i = 0; i < dresses.Length; i++)
                if (dresses[i] != null) dresses[i].SetActive(false);
            if (currentDressIndex >= 0 && currentDressIndex < dresses.Length && dresses[currentDressIndex] != null)
                dresses[currentDressIndex].SetActive(true);
        }
    }

    // ===========================
    // 👓 GLASSES
    // ===========================

    public void OnGlassesButtonClicked()
    {
        activeCategory = ActiveCategory.Glasses;

        leftButton.SetActive(true);
        rightButton.SetActive(true);

        glassesButtonImage.sprite = glassesActiveSprite;
        watchButtonImage.sprite = watchInactiveSprite;
        hatButtonImage.sprite = hatInactiveSprite;
        dressButtonImage.sprite = dressInactiveSprite;

        // Show whatever is currently equipped, including "None" (-1) —
        // don't force the player onto item 0 just for opening the tab.
        RefreshGlassInfo(currentGlassIndex);
    }

    public void NextGlass()
    {
        currentGlassIndex++;
        if (currentGlassIndex >= glasses.Length)
            currentGlassIndex = -1; // wrap around to None
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

    // Browsing previews the item and shows price/owned status.
    // index == -1 means "None" — nothing equipped in this category.
    // Equipping an unowned item happens only after BuyGlass() succeeds.
    void RefreshGlassInfo(int index)
    {
        for (int i = 0; i < glasses.Length; i++)
            glasses[i].SetActive(false);

        currentGlassIndex = index;

        if (index == -1)
        {
            glassesNameText.text = "None";
            priceText.text = "";
            buyButton.SetActive(false);

            PlayerPrefs.SetInt(PP_GLASSES, -1);
            PlayerPrefs.Save();
            return;
        }

        glasses[index].SetActive(true);

        if (IsGlassPurchased(index))
        {
            priceText.text = "Owned ✅";
            buyButton.SetActive(false);

            // Already owned items are free to wear — equip them as soon
            // as the player browses to them.
            PlayerPrefs.SetInt(PP_GLASSES, index);
            PlayerPrefs.Save();
        }
        else
        {
            int price = glassesPrices.Length > index ? glassesPrices[index] : 0;
            priceText.text = price + " 🪙";
            buyButton.SetActive(true);
        }

        if (glassesNames.Length > index)
            glassesNameText.text = glassesNames[index];
    }

    // Explicit "take off" call (e.g. wired to a dedicated remove button).
    public void HideAllGlasses()
    {
        RefreshGlassInfo(-1);
        glassesButtonImage.sprite = glassesInactiveSprite;
    }

    // ===========================
    // ⌚ WATCHES
    // ===========================

    public void OnWatchButtonClicked()
    {
        activeCategory = ActiveCategory.Watch;

        leftButton.SetActive(true);
        rightButton.SetActive(true);

        watchButtonImage.sprite = watchActiveSprite;
        glassesButtonImage.sprite = glassesInactiveSprite;
        hatButtonImage.sprite = hatInactiveSprite;
        dressButtonImage.sprite = dressInactiveSprite;

        RefreshWatchInfo(currentWatchIndex);
    }

    public void NextWatch()
    {
        currentWatchIndex++;
        if (currentWatchIndex >= watches.Length)
            currentWatchIndex = -1; // wrap around to None
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
            watches[i].SetActive(false);

        currentWatchIndex = index;

        if (index == -1)
        {
            watchNameText.text = "None";
            priceText.text = "";
            buyButton.SetActive(false);

            PlayerPrefs.SetInt(PP_WATCH, -1);
            PlayerPrefs.Save();
            return;
        }

        watches[index].SetActive(true);

        if (IsWatchPurchased(index))
        {
            priceText.text = "Owned ✅";
            buyButton.SetActive(false);

            PlayerPrefs.SetInt(PP_WATCH, index);
            PlayerPrefs.Save();
        }
        else
        {
            int price = watchPrices.Length > index ? watchPrices[index] : 0;
            priceText.text = price + " 🪙";
            buyButton.SetActive(true);
        }

        if (watchNames.Length > index)
            watchNameText.text = watchNames[index];
    }

    public void HideAllWatches()
    {
        RefreshWatchInfo(-1);
        watchButtonImage.sprite = watchInactiveSprite;
    }

    // ===========================
    // 🎩 HATS
    // ===========================

    public void OnHatButtonClicked()
    {
        activeCategory = ActiveCategory.Hat;

        leftButton.SetActive(true);
        rightButton.SetActive(true);

        hatButtonImage.sprite = hatActiveSprite;
        glassesButtonImage.sprite = glassesInactiveSprite;
        watchButtonImage.sprite = watchInactiveSprite;
        dressButtonImage.sprite = dressInactiveSprite;

        RefreshHatInfo(currentHatIndex);
    }

    public void NextHat()
    {
        currentHatIndex++;
        if (currentHatIndex >= hats.Length)
            currentHatIndex = -1; // wrap around to None
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
            hats[i].SetActive(false);

        currentHatIndex = index;

        if (index == -1)
        {
            hatNameText.text = "None";
            priceText.text = "";
            buyButton.SetActive(false);

            PlayerPrefs.SetInt(PP_HAT, -1);
            PlayerPrefs.Save();
            return;
        }

        hats[index].SetActive(true);

        if (IsHatPurchased(index))
        {
            priceText.text = "Owned ✅";
            buyButton.SetActive(false);

            PlayerPrefs.SetInt(PP_HAT, index);
            PlayerPrefs.Save();
        }
        else
        {
            int price = hatPrices.Length > index ? hatPrices[index] : 0;
            priceText.text = price + " 🪙";
            buyButton.SetActive(true);
        }

        if (hatNames.Length > index)
            hatNameText.text = hatNames[index];
    }

    public void HideAllHats()
    {
        RefreshHatInfo(-1);
        hatButtonImage.sprite = hatInactiveSprite;
    }

    // ===========================
    // 👗 DRESSES
    // ===========================

    public void OnDressButtonClicked()
    {
        activeCategory = ActiveCategory.Dress;

        leftButton.SetActive(true);
        rightButton.SetActive(true);

        dressButtonImage.sprite = dressActiveSprite;
        glassesButtonImage.sprite = glassesInactiveSprite;
        watchButtonImage.sprite = watchInactiveSprite;
        hatButtonImage.sprite = hatInactiveSprite;

        RefreshDressInfo(currentDressIndex);
    }

    public void NextDress()
    {
        currentDressIndex++;
        if (currentDressIndex >= dresses.Length)
            currentDressIndex = -1; // wrap around to None
        RefreshDressInfo(currentDressIndex);
        PlayCategorySound(dressSound);
    }

    public void PreviousDress()
    {
        currentDressIndex--;
        if (currentDressIndex < -1)
            currentDressIndex = dresses.Length - 1;
        RefreshDressInfo(currentDressIndex);
        PlayCategorySound(dressSound);
    }

    void RefreshDressInfo(int index)
    {
        for (int i = 0; i < dresses.Length; i++)
            dresses[i].SetActive(false);

        currentDressIndex = index;

        if (index == -1)
        {
            dressNameText.text = "None";
            priceText.text = "";
            buyButton.SetActive(false);

            PlayerPrefs.SetInt(PP_DRESS, -1);
            PlayerPrefs.Save();
            return;
        }

        dresses[index].SetActive(true);

        if (IsDressPurchased(index))
        {
            priceText.text = "Owned ✅";
            buyButton.SetActive(false);

            PlayerPrefs.SetInt(PP_DRESS, index);
            PlayerPrefs.Save();
        }
        else
        {
            int price = dressPrices.Length > index ? dressPrices[index] : 0;
            priceText.text = price + " 🪙";
            buyButton.SetActive(true);
        }

        if (dressNames.Length > index)
            dressNameText.text = dressNames[index];
    }

    public void HideAllDresses()
    {
        RefreshDressInfo(-1);
        dressButtonImage.sprite = dressInactiveSprite;
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
            case ActiveCategory.Dress: BuyDress(currentDressIndex); break;
        }
    }

    void BuyGlass(int index)
    {
        if (index < 0) return;
        if (IsGlassPurchased(index)) return; // already owned, nothing to buy

        int price = glassesPrices.Length > index ? glassesPrices[index] : 0;
        if (gameManager.SpendCoins(price))
        {
            MarkGlassPurchased(index);
            priceText.text = "Owned ✅";
            buyButton.SetActive(false);
            PlayerPrefs.SetInt(PP_GLASSES, index);
            PlayerPrefs.Save();
            PlayCategorySound(purchaseSound); // ✅ NEW: "adorable" SFX
            Debug.Log("✅ Glass purchased!");
        }
        else
        {
            notEnoughCoinsUI.SetActive(true);
            Debug.Log("❌ Not enough coins!");
        }
    }

    void BuyWatch(int index)
    {
        if (index < 0) return;
        if (IsWatchPurchased(index)) return;

        int price = watchPrices.Length > index ? watchPrices[index] : 0;
        if (gameManager.SpendCoins(price))
        {
            MarkWatchPurchased(index);
            priceText.text = "Owned ✅";
            buyButton.SetActive(false);
            PlayerPrefs.SetInt(PP_WATCH, index);
            PlayerPrefs.Save();
            PlayCategorySound(purchaseSound); // ✅ NEW: "adorable" SFX
            Debug.Log("✅ Watch purchased!");
        }
        else
        {
            notEnoughCoinsUI.SetActive(true);
            Debug.Log("❌ Not enough coins!");
        }
    }

    void BuyHat(int index)
    {
        if (index < 0) return;
        if (IsHatPurchased(index)) return;

        int price = hatPrices.Length > index ? hatPrices[index] : 0;
        if (gameManager.SpendCoins(price))
        {
            MarkHatPurchased(index);
            priceText.text = "Owned ✅";
            buyButton.SetActive(false);
            PlayerPrefs.SetInt(PP_HAT, index);
            PlayerPrefs.Save();
            PlayCategorySound(purchaseSound); // ✅ NEW: "adorable" SFX
            Debug.Log("✅ Hat purchased!");
        }
        else
        {
            notEnoughCoinsUI.SetActive(true);
            Debug.Log("❌ Not enough coins!");
        }
    }

    void BuyDress(int index)
    {
        if (index < 0) return;
        if (IsDressPurchased(index)) return;

        int price = dressPrices.Length > index ? dressPrices[index] : 0;
        if (gameManager.SpendCoins(price))
        {
            MarkDressPurchased(index);
            priceText.text = "Owned ✅";
            buyButton.SetActive(false);
            PlayerPrefs.SetInt(PP_DRESS, index);
            PlayerPrefs.Save();
            PlayCategorySound(purchaseSound); // ✅ NEW: "adorable" SFX
            Debug.Log("✅ Dress purchased!");
        }
        else
        {
            notEnoughCoinsUI.SetActive(true);
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

    bool IsDressPurchased(int index)
    {
        return PlayerPrefs.GetInt("Dress_Purchased_" + index, 0) == 1;
    }

    void MarkDressPurchased(int index)
    {
        PlayerPrefs.SetInt("Dress_Purchased_" + index, 1);
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
            case ActiveCategory.Dress: NextDress(); break;
        }
    }

    public void OnLeftButtonClicked()
    {
        switch (activeCategory)
        {
            case ActiveCategory.Glasses: PreviousGlass(); break;
            case ActiveCategory.Watch: PreviousWatch(); break;
            case ActiveCategory.Hat: PreviousHat(); break;
            case ActiveCategory.Dress: PreviousDress(); break;
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
        HideAllDresses();
        activeCategory = ActiveCategory.None;
        leftButton.SetActive(false);
        rightButton.SetActive(false);
    }
}