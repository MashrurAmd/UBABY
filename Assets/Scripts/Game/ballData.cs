using UnityEngine;

public class BallData : MonoBehaviour
{
    public string colorID; // e.g. "Red", "Blue", "Green"

    private void Awake()
    {
        if (string.IsNullOrEmpty(colorID))
            colorID = gameObject.name.Replace("(Clone)", "");
    }
}
