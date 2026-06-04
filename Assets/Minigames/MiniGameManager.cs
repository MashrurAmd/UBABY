using TMPro;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance;
    
    public int lives = 3;

    public int score;

    public TMP_Text scoreText;

    void Awake()
    {
        Instance = this;
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score;
    }

    public void GameOver()
    {
        scoreText.text = "Score: " + score;
        //print game over
        
        
    }

    public void ReduceLife(int amount)

    {
        
        lives--;
        if (lives <= 0)
        {
            GameOver();
        }
    }

}