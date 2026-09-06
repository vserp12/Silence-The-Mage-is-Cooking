using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private SpriteRenderer sr;
    private CharacterBob charBob;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        charBob = GetComponent<CharacterBob>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        movement.x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
        movement.y = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
        movement.Normalize();

        if (sr != null)
        {
            if (movement.x > 0.01f) sr.flipX = false;
            else if (movement.x < -0.01f) sr.flipX = true;
        }

        charBob?.SetMoving(movement.sqrMagnitude > 0.01f);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
