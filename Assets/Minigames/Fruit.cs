using UnityEngine;

public class Fruit : MonoBehaviour
{
    public float fallSpeed = 300f;

    RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        rect.anchoredPosition +=
            Vector2.down * fallSpeed * Time.deltaTime;

        if(rect.anchoredPosition.y < -1000)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Basket"))
        {
            MiniGameManager.Instance.AddScore(1);

            Destroy(gameObject);
        }
    }
}