using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 offset;
    public bool moveToFridge,moveBack;
    public Vector3 cameraDefaultPosition;
    public Transform fridge,kitchenCamera;
    public PlayerManager playerManager;

    void Start()
    {
        cameraDefaultPosition = kitchenCamera.position;
    }

    void FixedUpdate()
    {
        MoveToFridge();
    }

    void MoveToFridge()
    {
        if (playerManager.currentRoom == "kitchen")
        {
            if (moveToFridge)
            {
                transform.position = Vector3.MoveTowards(transform.position, fridge.position + offset, 0.1f);
            }
            else if (moveBack)
            {
                transform.position = Vector3.MoveTowards(transform.position, cameraDefaultPosition, 0.1f);
            } 
        }

    }

}
