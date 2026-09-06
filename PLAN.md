# Proyecto Silencio: The Mage is Cooking - Plan de Desarrollo e Implementación

## 1. Auditoría del Proyecto y Estado de Issues de GitHub (en orden cronológico)

### Issue #1: "Make elf enemies, santa, and more."
- **Estado GitHub**: Cerrado.
- **Estado Real**: ~65% completado (incompleto y con bugs graves).
- **Detalle de lo que faltaba o estaba roto**:
  1. **Dirección de Sprites Invertida**: Los sprites originales de los elfos y Santa miran de forma natural hacia la IZQUIERDA. El script `Enemy.cs` ponía `bodySR.flipX = (facing == -1)`. Al moverse a la izquierda hacia el jugador, activaba `flipX = true`, haciendo que el elfo mire a la derecha mientras camina a la izquierda, y viceversa.
  2. **Armas de los Elfos**: La posición `weaponOffset` y el espejo del arma quedaban desfasados con respecto a la orientación del cuerpo.
  3. **Santa**: `Santa.cs` tiene la lógica básica de alternar entre invocar 3 elfos y disparar proyectiles de hielo, pero no estaba conectado al sistema de oleadas ni a la oleada 5.

### Issue #2: "no more git problems, please" (Pull Request #2)
- **Estado GitHub**: Abierto.
- **Estado Real**: Rama desincronizada con archivos temporales y eliminaciones sin commitear.

### Issue #3: "Making character movement"
- **Estado GitHub**: Abierto.
- **Estado Real**: ~60% completado (incompleto y con bugs).
- **Detalle de lo que faltaba o estaba roto**:
  1. **Dirección del Jugador Invertida**: En `PlayerMovement.cs`, `if (movement.x > 0.01f) sr.flipX = false; else if (movement.x < -0.01f) sr.flipX = true;`. Dado que el sprite del mago mira hacia la IZQUIERDA en la textura base, al moverse a la derecha no se volteaba y al moverse a la izquierda se volteaba a la derecha. ¡Iba caminando hacia atrás!
  2. **Salto retro (Bob/Hop)**: `CharacterBob.cs` aplicaba squash-stretch a todo el transform raíz incluyendo hijos de interfaz y posición de pivote. Requiere un rebote limpio y retro sin descalibrar armas ni barras de vida.
  3. **Animación de báculo y ataque**: Animaciones de ataque sincronizadas con `StaffAim`.

### Issue #4: "Aplying actual spells"
- **Estado GitHub**: Abierto.
- **Estado Real**: ~60% completado (incompleto y con bugs de escala).
- **Detalle de lo que faltaba o estaba roto**:
  1. **Tamaño EXCESIVO de los Hechizos**: Los proyectiles y efectos de impacto tenían escalas entre 1.0 y 2.5, mientras que el jugador tiene escala 0.1 y los elfos 0.7. Los hechizos eran gigantescos comparados con los personajes.
  2. **Hechizos planos sin dinamismo**: Electricidad 4, Fuego 3 y Fuego 4 requerían efectos dinámicos en código (flicker, jitter de chispas, rotación y pulso ígneo, área de daño por impacto).
  3. **Agua 4 (Flor)**: `WaterFlowerAttack.cs` requería rotación radial orientada de los 6 pétalos en círculo con propagación expansiva.

### Issue #5: "Making the waves system."
- **Estado GitHub**: Abierto.
- **Estado Real**: ~20% completado (gravemente roto y ausente en escena).
- **Detalle de lo que faltaba o estaba roto**:
  1. **WaveManager ausente en la escena Game.unity**: No existía el GameObject ni componente en la escena; el jugador tenía que arrastrar elfos a mano para probar el juego.
  2. **Menú de Selección de Hechizo al Inicio ausente**: Al dar Play a la escena Game, no aparecía ningún menú para seleccionar el hechizo inicial.
  3. **Menú de Mejoras de Estadísticas (Stats) NUNCA IMPLEMENTADO**: El issue requería explícitamente un menú de estadísticas para subir: HP, Velocidad (Speed), Daño (Damage) y Reducción de Enfriamiento (Cooldown Reduction). No existía ni la clase ni la UI.
  4. **Tiempo de recuperación de 20 segundos ausente**: El juego solo esperaba 5 segundos silenciosos sin interfaz de descanso ni cuenta regresiva.
  5. **Jefe Santa en la Oleada 5**: No estaba instanciado en oleada 5.

### Issue #6: "Functional camera"
- **Estado GitHub**: Abierto.
- **Estado Real**: ~40% completado.
- **Detalle de lo que faltaba o estaba roto**:
  1. `CameraFollow.cs` solo seguía al jugador mediante un Lerp básico.
  2. No tenía ningún límite (clamp) con los bordes del mapa (el plano de 20x15).

### Problemas Adicionales Reportados por el Usuario:
- **Audio entre Escenas**: En `Game.unity`, `SceneMusic.cs` tenía un GUID corrupto/inexistente (`9cccaa8b28a37b4408b8dbf8a401a5ba`), por lo que al pasar del menú al juego, `AudioManager.PlayMusic(null)` no hacía nada y seguía sonando la música del menú principal (`MenuMusic.mp3`) en lugar de `GameMusic.mp3`.
- **Cero Configuración Manual para el Usuario**: El usuario no debe arrastrar ni configurar nada en el Inspector de Unity. Todo debe quedar cableado en la escena, en los prefabs y con inicialización automática a prueba de fallos en runtime.

---

## 2. Orden de Implementación Controlado

1. **Fase 1: Capa de Datos y Sistema de Estadísticas del Jugador (Issue #5)**
   - Crear `PlayerStats.cs`: Sistema de niveles y modificadores para HP, Velocidad, Daño y Cooldown Reduction.
   - Actualizar `PlayerHealth.cs`: Escalar con maxHealth de PlayerStats y curar al mejorar vida.
   - Actualizar `PlayerMovement.cs`: Escalar velocidad con la estadística de Speed.
   - Actualizar `SpellCaster.cs`: Escalar castTime con Cooldown Reduction y daño con Damage Multiplier.

2. **Fase 2: Orientación de Personajes y Movimiento Retro (Issues #1 y #3)**
   - Corregir orientación en `PlayerMovement.cs`:
     - Natural = Izquierda (`flipX = false`).
     - Movimiento izquierda (`movement.x < -0.01f`) -> `flipX = false`.
     - Movimiento derecha (`movement.x > 0.01f`) -> `flipX = true`.
   - Corregir orientación en `Enemy.cs`:
     - Natural = Izquierda (`flipX = false`).
     - Al moverse hacia la izquierda (`xDiff < 0`) -> `flipX = false`.
     - Al moverse hacia la derecha (`xDiff > 0`) -> `flipX = true`.
     - Reflejar `weaponChild` y su posición X según la orientación correcta.
   - Optimizar `CharacterBob.cs` para un rebote elástico retro sin distorsionar interfaces hijas.

3. **Fase 3: Reescalado y Efectos Fluidos de Hechizos (Issue #4)**
   - Reescalar todos los prefabs de hechizos en `Assets/Prefabs/Spells/` de (1.0~2.5) a proporciones adecuadas retro (~0.35~0.5).
   - Ajustar colisionadores circulares.
   - `FireAoEProjectile.cs`: Escalar a tamaño balanceado (0.45f ~ 0.55f) y añadir efecto dinámico de pulsación y partículas de fuego.
   - `ChainLightningProjectile.cs`: Añadir jitter eléctrico y chispas dinámicas.
   - `WaterFlowerAttack.cs`: Orientar pétalos radialmente hacia afuera en un círculo perfecto.

4. **Fase 4: Sistema Completo de Oleadas, Intermedio de 20s y Menú de Mejoras (Issue #5)**
   - Desarrollar `StatsUpgradeUI.cs` / `SpellSelectionUI.cs`:
     - Interfaz completa y responsiva (con soporte dual Canvas UI y fallback OnGUI garantizado al 100% de ejecución).
     - Selección de hechizo inicial en oleada 0.
     - Al terminar cada oleada:
       - Cuenta regresiva de 20 segundos de recuperación.
       - Botón "Ready / Comenzar Oleada" para saltar la espera si el jugador lo desea.
       - Menú de estadísticas: HP, Speed, Damage, Cooldown Reduction.
       - Menú de selección/mejora de hechizos (1 a 4).
   - Actualizar `WaveManager.cs`:
     - Fórmulas de oleadas progresivas.
     - Oleada 5: Aparición garantizada del jefe Santa con sus invocaciones.
     - Limpieza de enemigos muertos y detección precisa de victoria de oleada.

5. **Fase 5: Cámara con Límites de Mapa (Issue #6)**
   - Actualizar `CameraFollow.cs` con cálculo ortográfico dinámico de ancho y alto de pantalla (`orthographicSize * aspect`).
   - Confinar la cámara al plano del mapa (`x: [-10, 10]`, `y: [-7.5, 7.5]`).

6. **Fase 6: Sistema de Audio y Música por Escena**
   - Asignar `GameMusic.mp3` (`guid: 17172da21e6e983139cfeb8a5b2f8795`) en `Game.unity`.
   - `SceneMusic.cs`: Carga de respaldo desde Resources/Audio para garantizar música correcta en cualquier situación.
   - `AudioManager.cs`: Transición y corte de la música anterior al entrar en escena de juego.

7. **Fase 7: Cableado Automático Total en Escenas y Prefabs (Cumplimiento Paso 4)**
   - Modificar `Game.unity` para integrar `WaveManager`, `GameBootstrap`, `CameraFollow` configurado y eliminar elfos manuales huérfanos.
   - Crear `GameBootstrap.cs` auto-ejecutable con `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]` para que el juego funcione sin requerir ninguna acción en el Editor de Unity.
   - Validar compilación limpia en Unity Editor.
