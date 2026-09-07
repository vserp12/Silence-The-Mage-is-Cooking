using UnityEngine;
using UnityEngine.InputSystem;

// Rotates the staff to always point toward the mouse cursor, flipping vertically so it stays upright
public class StaffAim : MonoBehaviour
{
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Camera.main == null) return;

        Vector3 mouseScreen = Vector3.zero;
        if (Mouse.current != null)
        {
            Vector2 m = Mouse.current.position.ReadValue();
            mouseScreen = new Vector3(m.x, m.y, 0f);
        }
        else
        {
            try { mouseScreen = Input.mousePosition; } catch { }
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = transform.position.z;

        Vector3 origin = transform.parent != null ? transform.parent.position : transform.position;
        Vector3 dir = mouseWorld - origin;
        if (dir.sqrMagnitude < 0.001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Keep staff sprite right-side up when aiming left
        if (sr != null)
        {
            sr.flipY = (dir.x < 0f);
        }
    }
}
