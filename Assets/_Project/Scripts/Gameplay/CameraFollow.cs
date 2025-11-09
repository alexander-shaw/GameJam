using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target; // Player transform
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10f);
    
    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
        
        // Set camera to orthographic for 2D top-down
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 10f;
        }
    }
    
    void OnEnable()
    {
        // Find player when camera becomes active
        FindPlayer();
    }
    
    void FindPlayer()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void LateUpdate()
    {
        // Try to find player if target is null
        if (target == null)
        {
            FindPlayer();
            if (target == null) return;
        }
        
        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly move camera
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}

