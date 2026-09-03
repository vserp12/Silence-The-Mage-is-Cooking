# Silence! The Mage is Cooking 🧙‍♂️🍳

Documentación general del proyecto, escenas y scripts de la aplicación.

---

## 🎬 Escenas (`Assets/Scenes`)

| Escena | Descripción |
| :--- | :--- |
| **`MainMenu.unity`** | Escena del menú principal del juego. Contiene la interfaz de inicio (botón Play, panel de ingreso de nombre de jugador y tabla de puntuaciones/leaderboard). Utiliza animaciones como `ButtonSlideIn.anim` y `startAnimation.anim` controladas por `PlayButton.controller`. |
| **`Game.unity`** | Escena principal de gameplay. Contiene el personaje del jugador, la cámara de seguimiento y la lógica principal del mapa de juego. |
| **`SampleScene.unity`** | Escena de prueba/prototipado predeterminada de Unity. |

---

## 📜 Scripts (`Assets/Scripts`)

### 🛠️ Core (`Assets/Scripts/Core`)
* **[`CameraFollow.cs`](file:///c:/Users/julis/Silence!%20The%20Mage%20is%20Cooking/Assets/Scripts/Core/CameraFollow.cs)**
  * **Descripción:** Controla el seguimiento suave de la cámara principal hacia un objetivo transform (`target`).
  * **Detalles técnicos:** Utiliza `Vector3.Lerp` en el evento `LateUpdate` con distancia/desplazamiento configurable (`offset`) y suavizado de movimiento (`smoothSpeed`).

---

### 📦 Data (`Assets/Scripts/Data`)
* **[`SpellData.cs`](file:///c:/Users/julis/Silence!%20The%20Mage%20is%20Cooking/Assets/Scripts/Data/SpellData.cs)**
  * **Descripción:** `ScriptableObject` (`Silence/Spell Data`) que define las estadísticas base y propiedades visuales de los hechizos del juego.
  * **Campos:** `spellName`, `castTime` (tiempo de casteo), `damage` (daño), `projectileSpeed` (velocidad) y `spellColor` (color del efecto visual).
  * **Assets de hechizos:** Ubicados en `Assets/ScriptableObjects/Spells` (`BolaDeFuego`, `ChispaArcana`, `NovaDeEscarcha`).

---

### ⚔️ Gameplay (`Assets/Scripts/Gameplay`)
* **[`PlayerMovement.cs`](file:///c:/Users/julis/Silence!%20The%20Mage%20is%20Cooking/Assets/Scripts/Gameplay/PlayerMovement.cs)**
  * **Descripción:** Controla el movimiento 2D del jugador/mago en 8 direcciones.
  * **Detalles técnicos:** Lee entradas direccionales (`Input.GetAxisRaw`), normaliza el vector de movimiento para mantener una velocidad constante en diagonales y aplica el movimiento a `Rigidbody2D.MovePosition` dentro de `FixedUpdate`.

---

### 🖥️ UI (`Assets/Scripts/UI`)
* **[`MainMenuUI.cs`](file:///c:/Users/julis/Silence!%20The%20Mage%20is%20Cooking/Assets/Scripts/UI/MainMenuUI.cs)**
  * **Descripción:** Gestiona las interacciones básicas del menú principal.
  * **Métodos principales:** 
    * `PlayGame()`: Carga la escena `"Game"`.
    * `QuitGame()`: Cierra la aplicación (soporta ejecución en Editor y Build compilada).

* **[`NameInputManager.cs`](file:///c:/Users/julis/Silence!%20The%20Mage%20is%20Cooking/Assets/Scripts/UI/NameInputManager.cs)**
  * **Descripción:** Administra la interfaz de ingreso del nombre del usuario mediante `TMP_InputField`.
  * **Detalles técnicos:** Permite la confirmación mediante botón o tecla *Enter* (`onSubmit`), asigna por defecto `"Mago Anónimo"`, guarda el nombre en `PlayerPrefs` (`"PlayerName"`) y realiza la transición hacia la escena `"Game"`.

* **[`LeaderboardUI.cs`](file:///c:/Users/julis/Silence!%20The%20Mage%20is%20Cooking/Assets/Scripts/UI/LeaderboardUI.cs)**
  * **Descripción:** Controla la visualización de la tabla de puntuaciones/clasificación usando componentes `TextMeshProUGUI`.
  * **Detalles técnicos:** Inicializa las ranuras (`scoreSlots`), consulta el nombre guardado en `PlayerPrefs` y formatea la información para la UI.