using UnityEngine;
using UnityEngine.UI;

public class WardrobeManager : MonoBehaviour
{
    [Header("Glasses")]
    public GameObject[] glasses;
    public Text glassesNameText;
    public string[] glassesNames;

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
    public Sprite glassesActiveSprite;  // sprite when glasses is selected
    public Sprite glassesInactiveSprite;// sprite when glasses is not selected
    public Sprite watchActiveSprite;    // sprite when watch is selected
    public Sprite watchInactiveSprite;  // sprite when watch is not selected

    private int currentGlassIndex = -1;
    private int currentWatchIndex = -1;

    // ✅ Single flag to track which category is being controlled
    private enum ActiveCategory { None, Glasses, Watch }
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
    // 🔄 SHARED LEFT/RIGHT BUTTONS
    // ===========================

    public void OnRightButtonClicked()
    {
        switch (activeCategory)
        {
            case ActiveCategory.Glasses: NextGlass(); break;
            case ActiveCategory.Watch:   NextWatch(); break;
        }
    }

    public void OnLeftButtonClicked()
    {
        switch (activeCategory)
        {
            case ActiveCategory.Glasses: PreviousGlass(); break;
            case ActiveCategory.Watch:   PreviousWatch();  break;
        }
    }

    // ===========================
    // 🔄 RESET ALL (call when leaving wardrobe)
    // ===========================

    public void ResetAll()
    {
        HideAllGlasses();
        HideAllWatches();
        activeCategory = ActiveCategory.None;
        leftButton.SetActive(false);
        rightButton.SetActive(false);
    }
}