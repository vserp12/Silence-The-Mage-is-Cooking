using UnityEngine;

// Destroys the GameObject once its Animator finishes a non-looping state.
// Attach to impact/spread effect objects.
public class DestroyAfterAnimation : MonoBehaviour
{
    void Update()
    {
        var anim = GetComponent<Animator>();
        if (anim == null) { Destroy(gameObject); return; }

        var info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 1f && !info.loop)
            Destroy(gameObject);
    }
}
