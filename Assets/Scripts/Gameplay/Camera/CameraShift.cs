using UnityEngine;

public class CameraShift : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform player;       
    [SerializeField] private Vector3 followOffset;   
    [SerializeField] private float zoomSpeed = 2f;   
    [SerializeField] private float minZoom = 5f;     
    [SerializeField] private float maxZoom = 15f;    

    private Camera cam;
    private Vector3 staticPosition;                
    private bool isFollowing = true;                

    private void Awake()
    {
        cam = Camera.main;  
    }

    private void Update()
    {
        HandleCameraSwitch();
        HandleZoom();
        MoveCamera();
    }

    private void HandleCameraSwitch()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isFollowing = !isFollowing;
        }
    }

    private void HandleZoom()
    {
        if (cam.orthographic)
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0)
            {
                float targetSize = cam.orthographicSize - scrollInput * zoomSpeed;
                cam.orthographicSize = Mathf.Clamp(targetSize, minZoom, maxZoom);
            }
        }
        else
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0)
            {
                float targetSize = cam.fieldOfView - scrollInput * zoomSpeed;
                cam.fieldOfView = Mathf.Clamp(targetSize, minZoom, maxZoom);
            }
        }
    }

    private void MoveCamera()
    {
        if (isFollowing)
        {
            Vector3 targetPosition = player.position + followOffset;
            targetPosition.z = transform.position.z; 
            transform.position = targetPosition;
        }
        else
        {
            staticPosition.z = transform.position.z; 
            transform.position = staticPosition;
        }
    }

    public void SetCameraPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void SetCameraZoom(float zoomLevel)
    {
        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.Clamp(zoomLevel, minZoom, maxZoom);
        }
        else
        {
            cam.fieldOfView = Mathf.Clamp(zoomLevel, minZoom, maxZoom);
        }
    }
}
