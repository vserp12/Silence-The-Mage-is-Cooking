using UnityEngine;

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
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D o flechas
        movement.y = Input.GetAxisRaw("Vertical");   // W/S o flechas
        
        // Normalizar para que no se mueva más rápido en diagonal
        movement.Normalize();
    }

    void FixedUpdate()
    {
        // Mover el personaje
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}