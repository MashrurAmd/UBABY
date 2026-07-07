using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BallSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float spawnY = 10f;
    public float fixedZ = 0f;
    public Vector2 spawnXRange = new Vector2(6f, 12f);

    [Header("Difficulty Settings")]
    public float spawnDelay = 2f; // base delay between balls

    [Header("Pre-Spawn Settings")]
    public int preSpawnCount = 20;
    public float groundY = 0f;

    [Header("Power Ball Settings")]
    public int maxPowerUses = 3;
    private int remainingPowerUses;
    private bool usePowerBallNext = false;

    [Header("Bomb Settings")]
    public int maxBombUses = 3;
    private int remainingBombUses;
    private bool useBombNext = false;

    [Header("UI")]
    public Text powerBallCountText;
    public Text bombCountText;
    //public RectTransform spawnLine;

    private BallController currentBall;

    [Header("Line Settings")]
    public float lineXOffset = 0f;

    private string[] tags = {
        "RedBall", "BlueBall", "GreenBall", "YellowBall", "BlackBall",
        "CyanBall", "DarkBlueBall", "MaroonBall", "PurpleBall",
        "WhiteBall", "PinkBall"
    };

    private void Start()
    {
        remainingPowerUses = maxPowerUses;
        remainingBombUses = maxBombUses;

        UpdatePowerUI();
        PreSpawnGroundBalls();
        SpawnBall(centerTop: true);

        // Subscribe to level up events
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnLevelUp += HandleLevelUp;
    }

    private void Update()
    {
        if (currentBall != null && !currentBall.IsPlaced)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(currentBall.transform.position);
        }
    }

    private void UpdatePowerUI()
    {
        if (powerBallCountText != null)
            powerBallCountText.text = remainingPowerUses.ToString();

        if (bombCountText != null)
            bombCountText.text = remainingBombUses.ToString();
    }

    private void PreSpawnGroundBalls()
    {
        float minSpacing = 1f;
        for (int i = 0; i < preSpawnCount; i++)
        {
            string randomTag = tags[Random.Range(0, tags.Length)];
            float randomX = spawnXRange.x + (i * minSpacing) % (spawnXRange.y - spawnXRange.x);
            randomX += Random.Range(0f, 0.5f);

            Vector3 spawnPos = new Vector3(randomX, groundY, fixedZ);
            GameObject newBall = ObjectPooler.Instance.SpawnFromPool(randomTag, spawnPos, Quaternion.identity);

            Rigidbody rb = newBall.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;

            BallController ballCtrl = newBall.GetComponent<BallController>();
            if (ballCtrl != null) ballCtrl.enabled = false;
        }
    }

    public void SpawnBall(bool centerTop = false)
    {
        string chosenTag;

        if (usePowerBallNext)
        {
            chosenTag = "PowerBall";
            usePowerBallNext = false;
        }
        else if (useBombNext)
        {
            chosenTag = "Bomb";
            useBombNext = false;
        }
        else
        {
            chosenTag = tags[Random.Range(0, tags.Length)];
        }

        float spawnX = centerTop ? (spawnXRange.x + spawnXRange.y) / 2f : Random.Range(spawnXRange.x, spawnXRange.y);
        Vector3 spawnPos = new Vector3(spawnX, spawnY, fixedZ);

        GameObject newBall = ObjectPooler.Instance.SpawnFromPool(chosenTag, spawnPos, Quaternion.identity);

        BallController ballCtrl = newBall.GetComponent<BallController>();
        if (currentBall != null) currentBall.enabled = false;
        currentBall = ballCtrl;
        currentBall.spawner = this;
    }

    public void SpawnBallDelayed(float delay = 1f)
    {
        StartCoroutine(SpawnBallCoroutine(delay));
    }

    private IEnumerator SpawnBallCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnBall();
    }

    public void ActivatePowerBall()
    {
        if (remainingPowerUses <= 0) return;

        if (currentBall != null && !currentBall.IsPlaced)
        {
            // Replace the current ball with PowerBall
            Vector3 pos = currentBall.transform.position;
            currentBall.gameObject.SetActive(false);

            GameObject newBall = ObjectPooler.Instance.SpawnFromPool("PowerBall", pos, Quaternion.identity);
            BallController ballCtrl = newBall.GetComponent<BallController>();
            ballCtrl.spawner = this;
            currentBall = ballCtrl;
        }
        else
        {
            // If no active ball, spawn next as PowerBall
            usePowerBallNext = true;
        }

        remainingPowerUses--;
        UpdatePowerUI();
    }

    public void ActivateBomb()
    {
        if (remainingBombUses <= 0) return;

        if (currentBall != null && !currentBall.IsPlaced)
        {
            // Replace the current ball with Bomb
            Vector3 pos = currentBall.transform.position;
            currentBall.gameObject.SetActive(false);

            GameObject newBall = ObjectPooler.Instance.SpawnFromPool("Bomb", pos, Quaternion.identity);
            BallController ballCtrl = newBall.GetComponent<BallController>();
            ballCtrl.spawner = this;
            currentBall = ballCtrl;
        }
        else
        {
            // If no active ball, spawn next as Bomb
            useBombNext = true;
        }

        remainingBombUses--;
        UpdatePowerUI();
    }


    private void HandleLevelUp(int newLevel)
    {
        // Example difficulty scaling: decrease spawn delay
        spawnDelay = Mathf.Max(0.5f, spawnDelay - 0.5f);
        Debug.Log("Spawn delay decreased! Now: " + spawnDelay);
    }
}

