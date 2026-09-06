using UnityEngine;
using UnityEngine.InputSystem;

// Rotates the staff to always point toward the mouse cursor
public class StaffAim : MonoBehaviour
{
    void Update()
    {
        if (Camera.main == null || Mouse.current == null) return;
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0));
        mouseWorld.z = transform.position.z;
        Vector3 dir = mouseWorld - transform.parent.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
