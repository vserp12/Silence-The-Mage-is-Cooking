using UnityEngine;

// Smoothly follows the player while clamping the camera view to stay within the map borders
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float smoothSpeed = 0.125f;

    [Header("Map Boundaries")]
    public bool clampToBounds = true;
    public Vector2 minBounds = new Vector2(-10f, -7.5f);
    public Vector2 maxBounds = new Vector2(10f, 7.5f);

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }

        // Auto-detect plane map if present
        var plane = GameObject.Find("Plane");
        if (plane != null)
        {
            var rend = plane.GetComponent<Renderer>();
            if (rend != null)
            {
                var b = rend.bounds;
                minBounds = new Vector2(b.min.x, b.min.y);
                maxBounds = new Vector2(b.max.x, b.max.y);
            }
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
            else return;
        }

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        if (clampToBounds && cam != null && cam.orthographic)
        {
            float camH = cam.orthographicSize;
            float camW = camH * cam.aspect;

            float clampedX;
            if (maxBounds.x - minBounds.x > 2f * camW)
            {
                clampedX = Mathf.Clamp(smoothedPosition.x, minBounds.x + camW, maxBounds.x - camW);
            }
            else
            {
                clampedX = (minBounds.x + maxBounds.x) * 0.5f;
            }

            float clampedY;
            if (maxBounds.y - minBounds.y > 2f * camH)
            {
                clampedY = Mathf.Clamp(smoothedPosition.y, minBounds.y + camH, maxBounds.y - camH);
            }
            else
            {
                clampedY = (minBounds.y + maxBounds.y) * 0.5f;
            }

            smoothedPosition = new Vector3(clampedX, clampedY, offset.z);
        }

        transform.position = smoothedPosition;
    }
}