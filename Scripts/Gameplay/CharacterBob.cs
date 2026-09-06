using UnityEngine;

// Squash-stretch scale bounce to simulate a hop while moving (retro aesthetic)
public class CharacterBob : MonoBehaviour
{
    public float bobSpeed = 12f;
    public float stretchAmount = 0.12f; // max Y stretch

    private bool moving;
    private float phase;
    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    public void SetMoving(bool isMoving)
    {
        moving = isMoving;
        if (!isMoving)
        {
            phase = 0f;
            transform.localScale = baseScale;
        }
    }

    void Update()
    {
        if (!moving) return;
        phase += Time.deltaTime * bobSpeed;
        float t = Mathf.Abs(Mathf.Sin(phase));
        transform.localScale = new Vector3(
            baseScale.x * (1f - t * stretchAmount * 0.4f),
            baseScale.y * (1f + t * stretchAmount),
            baseScale.z
        );
    }
}
