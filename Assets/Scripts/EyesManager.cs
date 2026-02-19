using UnityEngine;

public class EyesManager : MonoBehaviour
{
    public Transform mycamera;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var mousePosition = new Vector3(ray.origin.x, ray.origin.y, ray.origin.z);
            transform.LookAt(mousePosition);
        }
        else
        {
            transform.LookAt(mycamera);
        }
    }
}
