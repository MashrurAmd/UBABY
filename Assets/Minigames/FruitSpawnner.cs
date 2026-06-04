using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    public GameObject fruitPrefab;

    public RectTransform canvasRect;

    public float spawnRate = 1f;

    void Start()
    {
        InvokeRepeating(
            nameof(SpawnFruit),
            1f,
            spawnRate);
    }

    void SpawnFruit()
    {
        GameObject fruit =
            Instantiate(
                fruitPrefab,
                canvasRect);

        RectTransform fruitRect =
            fruit.GetComponent<RectTransform>();

        float randomX =
            Random.Range(-450f, 450f);

        fruitRect.anchoredPosition =
            new Vector2(randomX, 1200);
    }
}