using UnityEngine;
using UnityEngine.UI;

public class MenuSwipe : MonoBehaviour
{
    public RectTransform[] panels; // assign BONUS, CLASSIC, RANKED
    public float swipeThreshold = 50f;
    public float transitionSpeed = 5f;

    private int currentIndex = 1; // start at Classic
    private Vector2 startTouch;
    private float targetX; // only store X movement
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        targetX = -panels[currentIndex].anchoredPosition.x;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            startTouch = Input.mousePosition;

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 endTouch = Input.mousePosition;
            float deltaX = endTouch.x - startTouch.x;

            if (Mathf.Abs(deltaX) > swipeThreshold)
            {
                if (deltaX > 0) currentIndex = Mathf.Max(0, currentIndex - 1); // swipe right
                else currentIndex = Mathf.Min(panels.Length - 1, currentIndex + 1); // swipe left
            }

            targetX = -panels[currentIndex].anchoredPosition.x;
        }

        // Smooth move only on X, keep Y fixed
        Vector2 currentPos = rectTransform.anchoredPosition;
        float newX = Mathf.Lerp(currentPos.x, targetX, Time.deltaTime * transitionSpeed);
        rectTransform.anchoredPosition = new Vector2(newX, currentPos.y);
    }
}
