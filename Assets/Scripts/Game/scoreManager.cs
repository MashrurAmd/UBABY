using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Score Settings")]
    public int score = 0;
    public Text scoreText;

    [Header("Level Settings")]
    public int currentLevel = 1;
    public int[] scoreThresholds = { 50, 150, 300 };
    

    public delegate void LevelUpAction(int newLevel);
    public event LevelUpAction OnLevelUp;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
        CheckLevelProgression();
    }



    //nothing to see here   
    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    private void CheckLevelProgression()
    {
        if (currentLevel < scoreThresholds.Length && score >= scoreThresholds[currentLevel - 1])
        {
            currentLevel++;
            Debug.Log("LEVEL UP! Now Level: " + currentLevel);

            if (OnLevelUp != null)
                OnLevelUp(currentLevel);
        }
    }

    public void ResetGame()
    {
        score = 0;
        currentLevel = 1;
        UpdateUI();
    }
}


