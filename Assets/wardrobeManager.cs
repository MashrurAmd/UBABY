using UnityEngine;
using UnityEngine.UI;

public class WardrobeManager : MonoBehaviour
{
    [Header("Glasses")]
    public GameObject[] glasses;
    public Text glassesNameText;
    public string[] glassesNames;

    [Header("Watches")]
    public GameObject[] watches;        // drag all watches here
    public Text watchNameText;          // text to show watch name
    public string[] watchNames;         // names like "Gold Watch", "Silver Watch" etc

    [Header("Navigation Buttons")]
    public GameObject leftButton;
    public GameObject rightButton;

    private int currentGlassIndex = -1;
    private int currentWatchIndex = -1;

    private bool glassesActive = false;
    private bool watchActive = false;

    // ===========================
    // 👓 GLASSES
    // ===========================

    public void OnGlassesButtonClicked()
    {
        // Hide watches if active
        HideAllWatches();

        glassesActive = true;
        leftButton.SetActive(true);
        rightButton.SetActive(true);

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
    }

    public void HideAllGlasses()
    {
        for (int i = 0; i < glasses.Length; i++)
            glasses[i].SetActive(false);

        glassesNameText.text = "";
        glassesActive = false;
        currentGlassIndex = -1;

        leftButton.SetActive(false);
        rightButton.SetActive(false);
    }

    // ===========================
    // ⌚ WATCHES
    // ===========================

    public void OnWatchButtonClicked()
    {
        // Hide glasses if active
        HideAllGlasses();

        watchActive = true;
        leftButton.SetActive(true);
        rightButton.SetActive(true);

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
    }

    public void HideAllWatches()
    {
        for (int i = 0; i < watches.Length; i++)
            watches[i].SetActive(false);

        watchNameText.text = "";
        watchActive = false;
        currentWatchIndex = -1;

        leftButton.SetActive(false);
        rightButton.SetActive(false);
    }

    // ===========================
    // 🔄 SHARED LEFT/RIGHT BUTTONS
    // ===========================

    public void OnRightButtonClicked()
    {
        if (glassesActive) NextGlass();
        else if (watchActive) NextWatch();
    }

    public void OnLeftButtonClicked()
    {
        if (glassesActive) PreviousGlass();
        else if (watchActive) PreviousWatch();
    }
}