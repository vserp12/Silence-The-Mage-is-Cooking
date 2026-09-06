using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float baseMoveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private SpriteRenderer sr;
    private CharacterBob charBob;
    private PlayerStats stats;
    private Transform staffChild;
    private float staffBaseX = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        charBob = GetComponent<CharacterBob>();
        stats = GetComponent<PlayerStats>() ?? gameObject.AddComponent<PlayerStats>();

        staffChild = transform.Find("Staff");
        if (staffChild != null)
        {
            staffBaseX = Mathf.Abs(staffChild.localPosition.x);
        }
    }

    void Update()
    {
        movement = Vector2.zero;

        // Try InputSystem first
        var kb = Keyboard.current;
        if (kb != null)
        {
            movement.x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
            movement.y = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
        }

        // Legacy input fallback if InputSystem keyboard has no input
        if (movement.sqrMagnitude < 0.001f)
        {
            try
            {
                movement.x = Input.GetAxisRaw("Horizontal");
                movement.y = Input.GetAxisRaw("Vertical");
            }
            catch { }
        }

        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        // Sprite naturally faces LEFT in texture:
        // Going left (< 0) => flipX = false (looks left)
        // Going right (> 0) => flipX = true (looks right)
        if (sr != null)
        {
            if (movement.x < -0.01f)
            {
                sr.flipX = false;
                if (staffChild != null)
                {
                    var p = staffChild.localPosition;
                    p.x = -staffBaseX;
                    staffChild.localPosition = p;
                }
            }
            else if (movement.x > 0.01f)
            {
                sr.flipX = true;
                if (staffChild != null)
                {
                    var p = staffChild.localPosition;
                    p.x = staffBaseX;
                    staffChild.localPosition = p;
                }
            }
        }

        charBob?.SetMoving(movement.sqrMagnitude > 0.01f);
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        float speed = baseMoveSpeed * (stats != null ? stats.GetSpeedMultiplier() : 1f);
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
}
