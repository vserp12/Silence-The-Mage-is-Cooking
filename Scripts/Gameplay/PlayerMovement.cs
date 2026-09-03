using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Leer input
        var kb = Keyboard.current;
        movement.x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
        movement.y = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
        
        // Normalizar para que no se mueva más rápido en diagonal
        movement.Normalize();
    }

    void FixedUpdate()
    {
        // Mover el personaje
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}