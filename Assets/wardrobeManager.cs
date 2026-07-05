using UnityEngine;
using UnityEngine.UI;

public class WardrobeManager : MonoBehaviour
{
    [Header("Glasses")]
    public GameObject[] glasses;
    public Text glassesNameText;
    public string[] glassesNames;

    [Header("Hats")]
    public GameObject[] hats;
    public Text hatNameText;
    public string[] hatNames;

    [Header("Watches")]
    public GameObject[] watches;
    public Text watchNameText;
    public string[] watchNames;

    [Header("Dresses")]
    public GameObject[] dresses;        // ✅ drag 3 dresses here
    public Text dressNameText;          // ✅ text to show dress name
    public string[] dressNames;         // ✅ names like "Red Dress", "Blue Dress" etc

    [Header("Navigation Buttons")]
    public GameObject leftButton;
    public GameObject rightButton;

    [Header("Category Button Images")]
    public Image glassesButtonImage;
    public Image watchButtonImage;
    public Image hatButtonImage;
    public Image dressButtonImage;      // ✅ dress button image
    public Sprite glassesActiveSprite;
    public Sprite glassesInactiveSprite;
    public Sprite watchActiveSprite;
    public Sprite watchInactiveSprite;
    public Sprite hatActiveSprite;
    public Sprite hatInactiveSprite;
    public Sprite dressActiveSprite;    // ✅ dress active sprite
    public Sprite dressInactiveSprite;  // ✅ dress inactive sprite

    private int currentGlassIndex = -1;
    private int currentWatchIndex = -1;
    private int currentHatIndex = -1;
    private int currentDressIndex = -1; // ✅

    private enum ActiveCategory { None, Glasses, Watch, Hat, Dress } // ✅ add Dress
    private ActiveCategory activeCategory = ActiveCategory.None;

    // ===========================
    // 💾 SAVE / LOAD (PlayerPrefs)
    // ===========================
    private const string PP_GLASSES = "Wardrobe_GlassIndex";
    private const string PP_WATCH = "Wardrobe_WatchIndex";
    private const string PP_HAT = "Wardrobe_HatIndex";
    private const string PP_DRESS = "Wardrobe_DressIndex";

    void Start()
    {
        LoadWardrobe();
    }

    // Reads the saved indices and re-activates the correct item GameObjects
    // so equipped items are still visible after the game is closed and reopened.
    void LoadWardrobe()
    {
        currentGlassIndex = PlayerPrefs.GetInt(PP_GLASSES, -1);
        currentWatchIndex = PlayerPrefs.GetInt(PP_WATCH, -1);
        currentHatIndex = PlayerPrefs.GetInt(PP_HAT, -1);
        currentDressIndex = PlayerPrefs.GetInt(PP_DRESS, -1);

        for (int i = 0; i < glasses.Length; i++) glasses[i].SetActive(false);
        for (int i = 0; i < watches.Length; i++) watches[i].SetActive(false);
        for (int i = 0; i < hats.Length; i++) hats[i].SetActive(false);
        for (int i = 0; i < dresses.Length; i++) dresses[i].SetActive(false);

        if (currentGlassIndex >= 0 && currentGlassIndex < glasses.Length)
            glasses[currentGlassIndex].SetActive(true);

        if (currentWatchIndex >= 0 && currentWatchIndex < watches.Length)
            watches[currentWatchIndex].SetActive(true);

        if (currentHatIndex >= 0 && currentHatIndex < hats.Length)
            hats[currentHatIndex].SetActive(true);

        if (currentDressIndex >= 0 && currentDressIndex < dresses.Length)
            dresses[currentDressIndex].SetActive(true);
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
        dressButtonImage.sprite = dressInactiveSprite; // ✅

        if (currentGlassIndex == -1)
            currentGlassIndex = 0;

        ShowGlass(currentGlassIndex);
    }

    public void NextGlass()
    {
        currentGlassIndex++;
        if (currentGlassIndex >= glasses.Length)
            currentGlassIndex = 0;
        ShowGlass(currentGlassIndex);
    }

    public void PreviousGlass()
    {
        currentGlassIndex--;
        if (currentGlassIndex < 0)
            currentGlassIndex = glasses.Length - 1;
        ShowGlass(currentGlassIndex);
    }

    void ShowGlass(int index)
    {
        for (int i = 0; i < glasses.Length; i++)
            glasses[i].SetActive(false);

        glasses[index].SetActive(true);

        if (glassesNames.Length > index)
            glassesNameText.text = glassesNames[index];

        PlayerPrefs.SetInt(PP_GLASSES, index);
        PlayerPrefs.Save();
    }

    public void HideAllGlasses()
    {
        for (int i = 0; i < glasses.Length; i++)
            glasses[i].SetActive(false);

        glassesNameText.text = "";
        currentGlassIndex = -1;
        glassesButtonImage.sprite = glassesInactiveSprite;

        PlayerPrefs.SetInt(PP_GLASSES, -1);
        PlayerPrefs.Save();
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
        dressButtonImage.sprite = dressInactiveSprite; // ✅

        if (currentWatchIndex == -1)
            currentWatchIndex = 0;

        ShowWatch(currentWatchIndex);
    }

    public void NextWatch()
    {
        currentWatchIndex++;
        if (currentWatchIndex >= watches.Length)
            currentWatchIndex = 0;
        ShowWatch(currentWatchIndex);
    }

    public void PreviousWatch()
    {
        currentWatchIndex--;
        if (currentWatchIndex < 0)
            currentWatchIndex = watches.Length - 1;
        ShowWatch(currentWatchIndex);
    }

    void ShowWatch(int index)
    {
        for (int i = 0; i < watches.Length; i++)
            watches[i].SetActive(false);

        watches[index].SetActive(true);

        if (watchNames.Length > index)
            watchNameText.text = watchNames[index];

        PlayerPrefs.SetInt(PP_WATCH, index);
        PlayerPrefs.Save();
    }

    public void HideAllWatches()
    {
        for (int i = 0; i < watches.Length; i++)
            watches[i].SetActive(false);

        watchNameText.text = "";
        currentWatchIndex = -1;
        watchButtonImage.sprite = watchInactiveSprite;

        PlayerPrefs.SetInt(PP_WATCH, -1);
        PlayerPrefs.Save();
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
        dressButtonImage.sprite = dressInactiveSprite; // ✅

        if (currentHatIndex == -1)
            currentHatIndex = 0;

        ShowHat(currentHatIndex);
    }

    public void NextHat()
    {
        currentHatIndex++;
        if (currentHatIndex >= hats.Length)
            currentHatIndex = 0;
        ShowHat(currentHatIndex);
    }

    public void PreviousHat()
    {
        currentHatIndex--;
        if (currentHatIndex < 0)
            currentHatIndex = hats.Length - 1;
        ShowHat(currentHatIndex);
    }

    void ShowHat(int index)
    {
        for (int i = 0; i < hats.Length; i++)
            hats[i].SetActive(false);

        hats[index].SetActive(true);

        if (hatNames.Length > index)
            hatNameText.text = hatNames[index];

        PlayerPrefs.SetInt(PP_HAT, index);
        PlayerPrefs.Save();
    }

    public void HideAllHats()
    {
        for (int i = 0; i < hats.Length; i++)
            hats[i].SetActive(false);

        hatNameText.text = "";
        currentHatIndex = -1;
        hatButtonImage.sprite = hatInactiveSprite;

        PlayerPrefs.SetInt(PP_HAT, -1);
        PlayerPrefs.Save();
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

        if (currentDressIndex == -1)
            currentDressIndex = 0;

        ShowDress(currentDressIndex);
    }

    public void NextDress()
    {
        currentDressIndex++;
        if (currentDressIndex >= dresses.Length)
            currentDressIndex = 0;
        ShowDress(currentDressIndex);
    }

    public void PreviousDress()
    {
        currentDressIndex--;
        if (currentDressIndex < 0)
            currentDressIndex = dresses.Length - 1;
        ShowDress(currentDressIndex);
    }

    void ShowDress(int index)
    {
        for (int i = 0; i < dresses.Length; i++)
            dresses[i].SetActive(false);

        dresses[index].SetActive(true);

        if (dressNames.Length > index)
            dressNameText.text = dressNames[index];

        PlayerPrefs.SetInt(PP_DRESS, index);
        PlayerPrefs.Save();
    }

    public void HideAllDresses()
    {
        for (int i = 0; i < dresses.Length; i++)
            dresses[i].SetActive(false);

        dressNameText.text = "";
        currentDressIndex = -1;
        dressButtonImage.sprite = dressInactiveSprite;

        PlayerPrefs.SetInt(PP_DRESS, -1);
        PlayerPrefs.Save();
    }

    // ===========================
    // 🔄 SHARED LEFT/RIGHT BUTTONS
    // ===========================

    public void OnRightButtonClicked()
    {
        switch (activeCategory)
        {
            case ActiveCategory.Glasses: NextGlass();  break;
            case ActiveCategory.Watch:   NextWatch();  break;
            case ActiveCategory.Hat:     NextHat();    break;
            case ActiveCategory.Dress:   NextDress();  break; // ✅
        }
    }

    public void OnLeftButtonClicked()
    {
        switch (activeCategory)
        {
            case ActiveCategory.Glasses: PreviousGlass();  break;
            case ActiveCategory.Watch:   PreviousWatch();  break;
            case ActiveCategory.Hat:     PreviousHat();    break;
            case ActiveCategory.Dress:   PreviousDress();  break; // ✅
        }
    }

    // ===========================
    // 🔄 RESET ALL
    // ===========================

    public void ResetAll()
    {
        HideAllGlasses();
        HideAllWatches();
        HideAllHats();
        HideAllDresses(); // ✅
        activeCategory = ActiveCategory.None;
        leftButton.SetActive(false);
        rightButton.SetActive(false);
    }
}