using UnityEngine;

// Squash-stretch scale bounce and slight vertical hop while moving (retro hop aesthetic)
public class CharacterBob : MonoBehaviour
{
    public float bobSpeed = 14f;
    public float stretchAmount = 0.12f; // max Y stretch
    public float hopHeight = 0.04f;      // small vertical hop offset

    private bool moving;
    private float phase;
    private Vector3 baseScale;
    private Vector3 baseLocalPos;
    private bool hasRecordedBase;

    void Start()
    {
        RecordBase();
    }

    void RecordBase()
    {
        if (hasRecordedBase) return;
        baseScale = transform.localScale;
        baseLocalPos = transform.localPosition;
        hasRecordedBase = true;
    }

    public void SetMoving(bool isMoving)
    {
        if (!hasRecordedBase) RecordBase();

        if (moving == isMoving) return;
        moving = isMoving;

        if (!moving)
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

        // Squash on ground (low t), stretch at apex of hop (high t)
        transform.localScale = new Vector3(
            baseScale.x * (1f - (t - 0.5f) * stretchAmount * 0.5f),
            baseScale.y * (1f + (t - 0.5f) * stretchAmount),
            baseScale.z
        );
    }
}
