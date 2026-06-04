using UnityEngine;

public class BasketController : MonoBehaviour
{
    public float speed = 800f;

    RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        float move = Input.GetAxis("Horizontal");

        rect.anchoredPosition +=
            Vector2.right * move * speed * Time.deltaTime;

        float x =
            Mathf.Clamp(rect.anchoredPosition.x, -450, 450);

        rect.anchoredPosition =
            new Vector2(x, rect.anchoredPosition.y);
    }
}