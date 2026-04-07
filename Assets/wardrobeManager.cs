using UnityEngine;
using UnityEngine.UI;

public class WardrobeManager : MonoBehaviour
{
    [Header("Glasses")]
    public GameObject[] glasses;        // drag all 6 glasses here in order
    public Text glassesNameText;        // text to show glasses name
    public string[] glassesNames;       // names like "Sunglasses", "Round Glasses" etc

    [Header("Navigation Buttons")]
    public GameObject leftButton;
    public GameObject rightButton;

    private int currentGlassIndex = -1; // -1 means none selected
    private bool glassesActive = false;

    // ===========================
    // 👓 GLASSES
    // ===========================

    public void OnGlassesButtonClicked()
    {
        glassesActive = true;

        // Show navigation buttons
        leftButton.SetActive(true);
        rightButton.SetActive(true);

        // Start with first glasses
        currentGlassIndex = 0;
        ShowGlass(currentGlassIndex);
    }

    public void NextGlass()
    {
        currentGlassIndex++;
        if (currentGlassIndex >= glasses.Length)
            currentGlassIndex = 0; // loop back to first

        ShowGlass(currentGlassIndex);
    }

    public void PreviousGlass()
    {
        currentGlassIndex--;
        if (currentGlassIndex < 0)
            currentGlassIndex = glasses.Length - 1; // loop to last

        ShowGlass(currentGlassIndex);
    }

    void ShowGlass(int index)
    {
        // Hide all glasses first
        for (int i = 0; i < glasses.Length; i++)
            glasses[i].SetActive(false);

        // Show selected glass
        glasses[index].SetActive(true);

        // Update name text
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
}