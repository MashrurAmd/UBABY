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

    [Header("Navigation Buttons")]
    public GameObject leftButton;
    public GameObject rightButton;

    [Header("Category Button Images")]
    public Image glassesButtonImage;    // Image component on glasses button
    public Image watchButtonImage;      // Image component on watch button
    public Image hatButtonImage;
    public Sprite glassesActiveSprite;  // sprite when glasses is selected
    public Sprite glassesInactiveSprite;// sprite when glasses is not selected
    public Sprite watchActiveSprite;    // sprite when watch is selected
    public Sprite watchInactiveSprite;  // sprite when watch is not selected
    public Sprite hatActiveSprite;
    public Sprite hatInactiveSprite;

    private int currentGlassIndex = -1;
    private int currentWatchIndex = -1;
    private int currentHatIndex = -1;

    // ✅ Single flag to track which category is being controlled
    private enum ActiveCategory { None, Glasses, Watch, Hat }
    private ActiveCategory activeCategory = ActiveCategory.None;

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
        hatButtonImage.sprite = hatInactiveSprite; // ✅ reset hat

        // ✅ Always show current glass, no index check needed
        if (currentGlassIndex == -1)
            currentGlassIndex = 0;

        ShowGlass(currentGlassIndex); // ✅ always call this
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
    }

    public void HideAllGlasses()
    {
        for (int i = 0; i < glasses.Length; i++)
            glasses[i].SetActive(false);

        glassesNameText.text = "";
        currentGlassIndex = -1;
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
        hatButtonImage.sprite = hatInactiveSprite; // ✅ reset hat

        // ✅ Always show current watch, no index check needed
        if (currentWatchIndex == -1)
            currentWatchIndex = 0;

        ShowWatch(currentWatchIndex); // ✅ always call this
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
    }

    public void HideAllWatches()
    {
        for (int i = 0; i < watches.Length; i++)
            watches[i].SetActive(false);

        watchNameText.text = "";
        currentWatchIndex = -1;
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
        glassesButtonImage.sprite = glassesInactiveSprite; // ✅ reset glasses
        watchButtonImage.sprite = watchInactiveSprite;     // ✅ reset watch

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
        }

        public void HideAllHats()
        {
            for (int i = 0; i < hats.Length; i++)
                hats[i].SetActive(false);

            hatNameText.text = "";
            currentHatIndex = -1;
            hatButtonImage.sprite = hatInactiveSprite;
        }



    // ===========================
    // 🔄 SHARED LEFT/RIGHT BUTTONS
    // ===========================

    public void OnRightButtonClicked()
    {
        switch (activeCategory)
        {
            case ActiveCategory.Glasses: NextGlass(); break;
            case ActiveCategory.Watch:   NextWatch(); break;
            case ActiveCategory.Hat:     NextHat();break;
        }
    }

    public void OnLeftButtonClicked()
    {
        switch (activeCategory)
        {
            case ActiveCategory.Glasses: PreviousGlass(); break;
            case ActiveCategory.Watch:   PreviousWatch();  break;
            case ActiveCategory.Hat:     PreviousHat();break;
        }
    }

    // ===========================
    // 🔄 RESET ALL (call when leaving wardrobe)
    // ===========================

    public void ResetAll()
    {
        HideAllGlasses();
        HideAllWatches();
        HideAllHats();
        activeCategory = ActiveCategory.None;
        leftButton.SetActive(false);
        rightButton.SetActive(false);
    }
}