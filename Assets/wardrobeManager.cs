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

        // Just refresh the display for whatever is currently equipped (including None)
        ShowGlass(currentGlassIndex);
    }

    public void NextGlass()
    {
        currentGlassIndex++;
        if (currentGlassIndex >= glasses.Length)
            currentGlassIndex = -1; // wrap around to None
        ShowGlass(currentGlassIndex);
    }

    public void PreviousGlass()
    {
        currentGlassIndex--;
        if (currentGlassIndex < -1)
            currentGlassIndex = glasses.Length - 1;
        ShowGlass(currentGlassIndex);
    }

    // index == -1 means "None" selected
    void ShowGlass(int index)
    {
        for (int i = 0; i < glasses.Length; i++)
            glasses[i].SetActive(false);

        if (index == -1)
        {
            glassesNameText.text = "None";
        }
        else
        {
            glasses[index].SetActive(true);
            if (glassesNames.Length > index)
                glassesNameText.text = glassesNames[index];
        }

        currentGlassIndex = index;
        PlayerPrefs.SetInt(PP_GLASSES, index);
        PlayerPrefs.Save();
    }

    public void HideAllGlasses()
    {
        ShowGlass(-1);
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
        dressButtonImage.sprite = dressInactiveSprite; // ✅

        ShowWatch(currentWatchIndex);
    }

    public void NextWatch()
    {
        currentWatchIndex++;
        if (currentWatchIndex >= watches.Length)
            currentWatchIndex = -1; // wrap around to None
        ShowWatch(currentWatchIndex);
    }

    public void PreviousWatch()
    {
        currentWatchIndex--;
        if (currentWatchIndex < -1)
            currentWatchIndex = watches.Length - 1;
        ShowWatch(currentWatchIndex);
    }

    // index == -1 means "None" selected
    void ShowWatch(int index)
    {
        for (int i = 0; i < watches.Length; i++)
            watches[i].SetActive(false);

        if (index == -1)
        {
            watchNameText.text = "None";
        }
        else
        {
            watches[index].SetActive(true);
            if (watchNames.Length > index)
                watchNameText.text = watchNames[index];
        }

        currentWatchIndex = index;
        PlayerPrefs.SetInt(PP_WATCH, index);
        PlayerPrefs.Save();
    }

    public void HideAllWatches()
    {
        ShowWatch(-1);
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
        dressButtonImage.sprite = dressInactiveSprite; // ✅

        ShowHat(currentHatIndex);
    }

    public void NextHat()
    {
        currentHatIndex++;
        if (currentHatIndex >= hats.Length)
            currentHatIndex = -1; // wrap around to None
        ShowHat(currentHatIndex);
    }

    public void PreviousHat()
    {
        currentHatIndex--;
        if (currentHatIndex < -1)
            currentHatIndex = hats.Length - 1;
        ShowHat(currentHatIndex);
    }

    // index == -1 means "None" selected
    void ShowHat(int index)
    {
        for (int i = 0; i < hats.Length; i++)
            hats[i].SetActive(false);

        if (index == -1)
        {
            hatNameText.text = "None";
        }
        else
        {
            hats[index].SetActive(true);
            if (hatNames.Length > index)
                hatNameText.text = hatNames[index];
        }

        currentHatIndex = index;
        PlayerPrefs.SetInt(PP_HAT, index);
        PlayerPrefs.Save();
    }

    public void HideAllHats()
    {
        ShowHat(-1);
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

        ShowDress(currentDressIndex);
    }

    public void NextDress()
    {
        currentDressIndex++;
        if (currentDressIndex >= dresses.Length)
            currentDressIndex = -1; // wrap around to None
        ShowDress(currentDressIndex);
    }

    public void PreviousDress()
    {
        currentDressIndex--;
        if (currentDressIndex < -1)
            currentDressIndex = dresses.Length - 1;
        ShowDress(currentDressIndex);
    }

    // index == -1 means "None" selected
    void ShowDress(int index)
    {
        for (int i = 0; i < dresses.Length; i++)
            dresses[i].SetActive(false);

        if (index == -1)
        {
            dressNameText.text = "None";
        }
        else
        {
            dresses[index].SetActive(true);
            if (dressNames.Length > index)
                dressNameText.text = dressNames[index];
        }

        currentDressIndex = index;
        PlayerPrefs.SetInt(PP_DRESS, index);
        PlayerPrefs.Save();
    }

    public void HideAllDresses()
    {
        ShowDress(-1);
        dressButtonImage.sprite = dressInactiveSprite;
    }

    // ===========================
    // 🔄 SHARED LEFT/RIGHT BUTTONS
    // ===========================

    public void OnRightButtonClicked()
    {
        switch (activeCategory)
        {
            case ActiveCategory.Glasses: 
                PlayCategorySound(glassesSound);
                NextGlass();  
                break;
            case ActiveCategory.Watch:   
                PlayCategorySound(watchSound);
                NextWatch();  
                break;
            case ActiveCategory.Hat:     
                PlayCategorySound(hatSound);
                NextHat();    
                break;
            case ActiveCategory.Dress:   
                PlayCategorySound(dressSound);
                NextDress();  
                break;
        }
    }

    public void OnLeftButtonClicked()
    {
        switch (activeCategory)
        {
            case ActiveCategory.Glasses: 
                PlayCategorySound(glassesSound);
                PreviousGlass();  
                break;
            case ActiveCategory.Watch:   
                PlayCategorySound(watchSound);
                PreviousWatch();  
                break;
            case ActiveCategory.Hat:     
                PlayCategorySound(hatSound);
                PreviousHat();    
                break;
            case ActiveCategory.Dress:   
                PlayCategorySound(dressSound);
                PreviousDress();  
                break;
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
        HideAllDresses(); // ✅
        activeCategory = ActiveCategory.None;
        leftButton.SetActive(false);
        rightButton.SetActive(false);
    }
}