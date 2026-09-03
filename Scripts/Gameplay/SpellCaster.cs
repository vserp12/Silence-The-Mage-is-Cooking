using UnityEngine;
using UnityEngine.InputSystem;

public class SpellCaster : MonoBehaviour
{
    public SpellData currentSpell; // Acá asignás el hechizo desde el Inspector
    
    private float castProgress = 0f;
    private bool isCasting = false;
    private Transform castBar;

    void Start()
    {
        // Crear una barra de casteo simple (un rectángulo encima del jugador)
        GameObject barObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        barObj.transform.SetParent(transform);
        barObj.transform.localPosition = new Vector3(0, 1f, 0); // Un poco arriba
        barObj.transform.localScale = new Vector3(1f, 0.1f, 1f);
        Destroy(barObj.GetComponent<Collider>()); // Quitar el collider del quad
        
        Renderer barRenderer = barObj.GetComponent<Renderer>();
        barRenderer.material.color = Color.gray;
        
        castBar = barObj.transform;
        castBar.localScale = new Vector3(0, 0.1f, 1f); // Empieza vacía
    }

    void Update()
    {
        // Mantener click izquierdo para castear
        if (Mouse.current.leftButton.isPressed)
        {
            if (!isCasting) isCasting = true;
            
            castProgress += Time.deltaTime / currentSpell.castTime;
            
            // Actualizar barra visual
            castBar.localScale = new Vector3(Mathf.Clamp01(castProgress), 0.1f, 1f);
            
            // Si completó el casteo
            if (castProgress >= 1f)
            {
                CastSpell();
                castProgress = 0f;
                isCasting = false;
            }
        }
        else
        {
            // Si soltó el click, cancelar
            if (isCasting)
            {
                castProgress = 0f;
                isCasting = false;
                castBar.localScale = new Vector3(0, 0.1f, 1f);
            }
        }
    }

    void CastSpell()
    {
        if (currentSpell == null || currentSpell.projectilePrefab == null) return;

        // Obtener la posición del cursor en el mundo
        if (Camera.main != null)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint((Vector3)Mouse.current.position.ReadValue());
            mouseWorldPos.z = transform.position.z;

            // Calcular dirección hacia el cursor
            Vector3 dir = (mouseWorldPos - transform.position).normalized;

            // Crear proyectil
            GameObject proj = Instantiate(currentSpell.projectilePrefab, transform.position, Quaternion.identity);
            Projectile p = proj.GetComponent<Projectile>();
            if (p != null)
            {
                p.Setup(dir, currentSpell.projectileSpeed, currentSpell.damage, currentSpell.projectileVisuals);
            }
        }
    }
}