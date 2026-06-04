using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    [Header("Camera Positions")]
    public Vector3 behindCarOffset = new Vector3(0, 3, -6);
    public Vector3 topDownOffset = new Vector3(0, 15, -5);
    [Header("Settings")]
    public float smoothSpeed = 10f;
    public bool isTopDown = false;
    private Vector3 currentOffset;
    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("No Player found! Make sure your car has 'Player' tag.");
            }
        }
        currentOffset = behindCarOffset;
    }
    void LateUpdate()
    {
        if (target == null) return;
        // Toggle camera with 'C' key - compatible with both systems
        if (GetCameraToggleInput())
        {
            isTopDown = !isTopDown;
            currentOffset = isTopDown ? topDownOffset : behindCarOffset;
        }
        // Desired position
        Vector3 desiredPosition = target.position + target.TransformDirection(currentOffset);
        // Smooth follow
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
        // Look at car
        transform.LookAt(target.position + Vector3.up);
    }
    bool GetCameraToggleInput()
    {
#if ENABLE_INPUT_SYSTEM
        // New Input System
        return UnityEngine.InputSystem.Keyboard.current != null && 
               UnityEngine.InputSystem.Keyboard.current.cKey.wasPressedThisFrame;
#else
        // Old Input System
        return Input.GetKeyDown(KeyCode.C);
#endif
    }
}