using System.Collections;
using UnityEngine;

// Code-driven weapon swing for elf weapons (no spritesheet needed)
public class WeaponSwing : MonoBehaviour
{
    public float swingAngle = 70f;
    public float swingDuration = 0.25f;

    private Quaternion restRotation;

    void Start()
    {
        restRotation = transform.localRotation;
    }

    public void Swing()
    {
        StopAllCoroutines();
        StartCoroutine(DoSwing());
    }

    private IEnumerator DoSwing()
    {
        float elapsed = 0f;
        while (elapsed < swingDuration)
        {
            float t = elapsed / swingDuration;
            float angle = Mathf.Sin(t * Mathf.PI) * swingAngle;
            transform.localRotation = restRotation * Quaternion.Euler(0f, 0f, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = restRotation;
    }
}
