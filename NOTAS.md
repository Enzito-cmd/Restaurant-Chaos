# Restaurant Chaos — Notas de desarrollo

> Archivo de trabajo personal. Sirve como memoria del proyecto: contexto, bitácora de
> cambios, bugs detectados, pendientes e ideas. **Leer este archivo al empezar cada
> sesión** para retomar el contexto sin tener que reexplorar todo.
>
> Última actualización: 2026-09-07

---

## 1. Contexto del proyecto

**Qué es:** juego 3D estilo *Overcooked* ambientado en un restaurante asiático. El jugador
atiende clientes (los recibe, los sienta, toma el pedido implícito, cocina en distintas
estaciones vía minijuegos, y les entrega el plato antes de que se les acabe la paciencia).

**Stack:**
- Unity (C#), proyecto de facultad.
- Navegación de NPCs con **NavMesh** (`NavMeshAgent`).
- UI con **TextMesh Pro**.
- Solución de VS: `Restaurant-Chaos.sln`.

**Rama de trabajo actual:** `enzo` (la principal es `main`).

**Aviso importante:** buena parte del código de clientes **no lo escribí yo**, viene
heredado y está bastante desprolijo. Los problemas concretos están en la sección
[Bugs y deuda técnica](#4-bugs-y-deuda-técnica).

### Estructura de carpetas relevante

```
Assets/
├── Scripts/
│   ├── Interfaces/          IInteractable, IMinigame
│   ├── Items/               HoldableItem, ItemType
│   ├── Managers/            CameraManager, CursorManager, MinigameManager, SoundManager
│   ├── Minigames/           Fryer, Tempura, Wok (cada uno con su controller/visuals)
│   ├── Player/              PlayerController, PlayerHoldSystem, PlayerInteraction
│   ├── Stations/            Extinguisher, Fridge, Fryer, Mochis, Tempura, TrashBin, Wok
│   ├── clientes level 1/    ← módulo de clientes (nombre viejo, con espacios)
│   └── Clients/             ← carpeta nueva, todavía vacía
├── Scenes/                  Menu, LevelSelector, Configuration, GameScene, Level 1, Level 2
├── 2D Assets/  3D Assets/  Materials/  Prefab/  Sounds/
```

---

## 2. Arquitectura clave

### Interacción del jugador
- Todo lo interactuable implementa `IInteractable` (un solo método: `Interact()`).
- [`PlayerInteraction`](Assets/Scripts/Player/PlayerInteraction.cs) escucha la tecla **E**,
  hace un `Physics.OverlapSphere` desde `interactPoint` filtrando por `interactableLayer`,
  y llama a `Interact()` del objetivo.
- **Prioridad de targets:** si hay un `RestaurantClient` en rango, gana sobre cualquier otra
  cosa (`break` en el loop). Si el jugador está siendo perseguido por un cliente enojado
  (`IsBeingChased()`), lo único con lo que puede interactuar es el `ExtinguisherProp`.

### Manejo de items
- [`PlayerHoldSystem`](Assets/Scripts/Player/PlayerHoldSystem.cs): el jugador sostiene **un
  solo item a la vez** en un `holdPoint`.
  - `HoldItem(prefab)` → instancia uno nuevo.
  - `HoldExistingItem(go)` → toma uno que ya existe en el mundo (desactiva su collider).
  - `ReleaseItem()` → lo suelta al mundo (reactiva collider).
  - `ClearHeldItem()` → lo destruye (esto es lo que se usa al entregar comida).
- Los items se identifican con el enum `ItemType`:
  `None, Chocolate, Strawberry, Peach, Seafood, BreadedTempura, FriedTempura, WokRice`.

### Managers (patrón singleton `Instance`)
- `SoundManager.Instance.PlaySound(SoundType.X)` — usado en todos lados.
- `MoneyManager.Instance.AddMoney(int)` — dinero + UI.
- `LevelManager.Instance` — estado del nivel, flag `isLevel2`, contador de clientes servidos.
- `CursorManager.Instance` — mostrar/ocultar cursor.

---

## 3. Sistema de clientes (el módulo en el que estoy trabajando)

Archivos: `Assets/Scripts/clientes level 1/` + `ClientHighlight.cs` y
`ClientLeaveSoundZone.cs` (que están sueltos en `Assets/Scripts/`).

### Máquina de estados de `RestaurantClient`

```
WaitingInQueue → FollowingPlayer → Sitting → Leaving → (destruido)
                                      ↓
                                 AngryChasing   (si se acaba la paciencia y es isAngryCustomer)
```

### Flujo completo

1. **Spawn** — [`ClientQueueSpawner`](Assets/Scripts/clientes%20level%201/ClientQueueSpawner.cs)
   instancia un prefab random de `clientPrefabs[]`, uno cada `spawnInterval` seg, hasta
   `maxClients` (limitado también por la cantidad de `queuePositions`). Mantiene la lista
   `clients` con el orden de la fila. Al terminar marca `HasFinishedSpawning = true`.

2. **Cola** — el cliente camina a su `queuePosition`. Solo se puede interactuar con el
   **primero de la fila** (`CanPickClient()` compara contra `clients[0]`). Al interactuar
   pasa a `FollowingPlayer`, sale de la lista, y `UpdateQueue()` reacomoda a los demás.

3. **Sentarse** — el jugador interactúa con una [`Chair`](Assets/Scripts/clientes%20level%201/Chair.cs)
   libre; la silla busca al cliente en estado `FollowingPlayer` y llama a `SitOnChair()`:
   - Desactiva el `NavMeshAgent` y **teletransporta** al cliente al `SitPoint`.
   - Elige el plato: en **Level 1** siempre `possibleMeals[0]`; en **Level 2**
     (`LevelManager.Instance.isLevel2`) elige uno random.
   - `OrderSequence()`: muestra el ícono "pensando" 5 seg → muestra el ícono del plato →
     prende el indicador de pedido en la silla.
   - `ClientHappiness` ya venía corriendo su timer desde el `Awake` del cliente.

4. **Entrega** — `TryDeliverFood()` valida distancia (`foodDeliveryDistance`, 4 por defecto)
   y que el `ItemType` que sostiene el jugador coincida con `chosenMeal.foodType`.
   Si coincide → `DeliverFood()`: frena el timer, `LevelManager.AddServedClient()`,
   destruye el item de la mano, spawnea dinero en la mesa, libera la silla, reactiva el
   agent con `Warp()` y pasa a `Leaving`.

5. **Salida** — camina hacia el objeto con tag `"Exit"` y se destruye al llegar.

6. **Paciencia agotada** — [`ClientHappiness`](Assets/Scripts/clientes%20level%201/ClientHappiness.cs)
   descuenta `maxTime` (90 seg por defecto) y actualiza una barra (`Image.fillAmount`).
   Al llegar a 0: si `isAngryCustomer` → `TriggerAngryChase()` (persigue al jugador un 50%
   más rápido y le grita cada `yellCooldown`); si no → `Die()` (se destruye sin más).

7. **Extintor** — `GetBlownAway(force)` saca al cliente por la fuerza: desactiva el agent,
   le agrega un `Rigidbody`, le aplica impulso + torque y lo destruye a los 2.5 seg.

8. **Fin de nivel** — [`LevelManager`](Assets/Scripts/clientes%20level%201/LevelManager.cs)
   chequea en `Update()` (después de 1 seg de gracia y solo si el spawner terminó) si quedan
   `RestaurantClient` en escena. Si no queda ninguno → panel final + animación de estrellas
   (una estrella por cliente servido, hasta `stars.Length`).

### Otros scripts de soporte
- `ClientHighlight` — prende/apaga emisión en los materiales del cliente (resaltado visual).
- `ClientLeaveSoundZone` — trigger que reproduce el sonido de salida una sola vez por cliente
  (usa un `HashSet` para no repetir).
- `MoneyPickup` — `IInteractable` que suma dinero y se destruye.

---

## 4. Bugs y deuda técnica

### 🐛 Bugs confirmados

| # | Bug | Archivo | Estado |
|---|-----|---------|--------|
| B1 | `OnTriggerExit` tiene lógica contradictoria: pone `playerInFoodRange = false` y en el bloque siguiente, con la **misma condición**, lo vuelve a poner `true`. Efecto: nunca queda en `false`. Copy-paste mal hecho. | [RestaurantClient.cs:90-99](Assets/Scripts/clientes%20level%201/RestaurantClient.cs#L90) | ⬜ Pendiente |
| B2 | `chosenMeal.rewardAmount` **nunca se usa**. El dinero que suelta el cliente es el valor hardcodeado del prefab `MoneyPickup` (30). La recompensa por plato configurada en el inspector no tiene efecto; solo se loguea con `Debug.Log`. | [RestaurantClient.cs:432-439](Assets/Scripts/clientes%20level%201/RestaurantClient.cs#L432) | ⬜ Pendiente |
| B3 | `Chair.ShowOrderIndicator()` / `HideOrderIndicator()` chequean `orderIndicator != null` pero después usan `cookIndicator` **sin chequear null** → `NullReferenceException` si esa referencia quedó vacía en el inspector. | [Chair.cs:28-44](Assets/Scripts/clientes%20level%201/Chair.cs#L28) | ⬜ Pendiente |
| B4 | `SoundManager.Awake()` hacía `Instance = this` **sin guarda de singleton** + `DontDestroyOnLoad`. Al cambiar de escena se acumulaban instancias que nunca se destruían → sonidos duplicados/triplicados y los sliders de volumen solo afectaban al último. | [SoundManager.cs:63-77](Assets/Scripts/Managers/SoundManager.cs#L63) | ✅ Arreglado 2026-09-08 |
| B5 | `StartLoopingSound` y `PlaySound` comparten el **mismo `AudioSource`**, así que `StopLoopingSound()` → `soundSource.Stop()` corta también los one-shots que estén sonando en ese momento. | [SoundManager.cs:98-127](Assets/Scripts/Managers/SoundManager.cs#L98) | ⬜ Pendiente |
| B7 | `LevelManager.AnimateStar()` anima la escala de la estrella de `Vector3.zero` a `Vector3.one` y la deja fija en `Vector3.one * 0.8f` al terminar — un valor absoluto, sin importar qué escala le hayas puesto a mano en el editor. Mismo bug ya corregido en el `LevelManager` nuevo (`Scripts/Clients/LevelManager.cs`); acá queda documentado por si algún día se retoman las escenas viejas. | [LevelManager.cs:117-124](Assets/Scripts/clientes%20level%201/LevelManager.cs#L117) | ⬜ Pendiente (código viejo, no bloquea el refactor) |
| B6 | `Chair.orderIndicator` es un GameObject **fijo** asignado a mano en el Inspector (no se instancia dinámicamente, solo se prende/apaga) — siempre muestra el mismo modelo sin importar qué plato eligió realmente el cliente. "Funciona" en Level 1 solo porque ahí el pedido siempre es Wok (fijo); en Level 2 (pedido random) muestra el plato **equivocado** cada vez que no toca Wok. Descubierto al revisar la escena vieja — Enzo notó que apuntaba al Wok y no entendía por qué. | [Chair.cs:17-44](Assets/Scripts/clientes%20level%201/Chair.cs#L17) | ⬜ Pendiente (código viejo, no bloquea el refactor) |

### 🧹 Deuda técnica / código desprolijo

| # | Problema | Detalle |
|---|----------|---------|
| D1 | **Campo muerto:** `playerInFoodRange` se setea en los triggers pero `TryDeliverFood()` no lo usa — recalcula la distancia a mano con `Vector3.Distance`. | `RestaurantClient.cs` |
| D2 | **Tres caminos distintos para destruir al cliente al salir:** (a) el `OnTriggerExit` con tag `"Exit"` dentro de `RestaurantClient`, (b) el componente aparte `ExitTrigger.cs`, (c) el chequeo de distancia ≤2.5m en `LeaveRestaurant()`. Hay un flag `hasBeenRemoved` que evita la doble destrucción, pero si cambia el exit point hay que tocar 3 lugares. **Unificar en uno solo.** | `RestaurantClient.cs`, `ExitTrigger.cs` |
| D3 | **`FindObjectsByType` en caliente:** `LevelManager.Update()` lo llama **cada frame**; `PlayerInteraction.IsBeingChased()` lo llama en cada interacción; `Chair` lo usa para encontrar clientes. Es caro y frágil. Mejor: que el spawner/manager lleve un registro de clientes activos. | varios |
| D4 | **Nombre de carpeta:** `Scripts/clientes level 1` — español, minúsculas, con espacios, y ya no es solo de "level 1". Ya creé `Scripts/Clients` para migrar. ⚠️ Al mover scripts hay que mover también los `.meta` para no perder las referencias en prefabs/escenas. | estructura |
| D5 | **Scripts de clientes sueltos:** `ClientHighlight.cs` y `ClientLeaveSoundZone.cs` están en la raíz de `Scripts/` en vez de con el resto del módulo. | estructura |
| D6 | **`RestaurantClient` hace demasiado** (~550 líneas): movimiento, estados, pedidos, visuales, animación, dinero, física del extintor. Candidato a dividir en componentes. | `RestaurantClient.cs` |
| D7 | Textos de `Debug.Log` con acentos rotos por encoding (`"Atend� al primero"`), y logs de debug que deberían sacarse antes de entregar. | varios |
| D8 | El timer de `ClientHappiness` arranca en el `Awake` — o sea corre desde que el cliente spawnea, no desde que se sienta o hace el pedido. **Resuelto por diseño en la sección 7.5** (dos timers). | `ClientHappiness.cs` |
| D9 | **Triple mantenimiento en `SoundManager`:** agregar un sonido obliga a tocar 3 lugares (el enum, un `[SerializeField]`, y un `case` en un switch de 60 líneas). Reemplazar por un array de `SoundEntry { SoundType, AudioClip }` + `Dictionary` armado en `Awake`. El switch desaparece entero. | `SoundManager.cs` |
| D10 | **Volumen master partido en dos caminos:** vive en `SoundManager` (no le corresponde conceptualmente) y se propaga por dos vías distintas — `SoundManager` se lo empuja a `MusicManager` con `ApplyMasterVolume()`, pero `MusicManager.ApplyVolume()` además lo lee por su cuenta de `PlayerPrefs`. Solución nativa: **AudioMixer** con parámetros expuestos; elimina toda la multiplicación manual y desacopla los dos managers. | `SoundManager.cs`, `MusicManager.cs` |
| D11 | Sin audio posicional: todo sale de un único `AudioSource` global, así que ningún sonido es espacial (un cliente del otro lado del salón suena igual que uno al lado). Evaluar sources 3D pooleados para sonidos del mundo. | `SoundManager.cs` |
| D12 | Nomenclatura inconsistente en el enum `SoundType`: `pedido` (minúscula y en español) conviviendo con `Stars` y `WokHit` (PascalCase, rompiendo el camelCase de los demás campos). | `SoundManager.cs` |

---

## 5. Pendientes (TODO)

- [ ] Migrar el módulo de clientes de `Scripts/clientes level 1` → `Scripts/Clients` (con `.meta`).
- [ ] Mover `ClientHighlight.cs` y `ClientLeaveSoundZone.cs` a la carpeta del módulo.
- [ ] Arreglar B1 (trigger contradictorio).
- [ ] Arreglar B2 (conectar `rewardAmount` con `MoneyPickup`).
- [ ] Arreglar B3 (null check de `cookIndicator`).
- [ ] Unificar la lógica de salida del cliente (D2).
- [x] ~~Decidir si el timer de paciencia arranca al spawnear o al sentarse (D8).~~ → resuelto: dos timers, ver 7.5.
- [x] ~~Arreglar B4 (guarda de singleton en `SoundManager`).~~ → hecho 2026-09-08.
- [ ] Refactor de audio (fase aparte, después de clientes): B5, D9, D10, D11, D12.
- [x] ~~**Fase 1 — ScriptableObjects** (`MealDefinition`, `ClientTypeDefinition`, `PatienceCarryOver`).~~ → código hecho 2026-09-08. **Falta crear los assets en Unity, ver checklist abajo.**
- [ ] **Fase 2** — FSM base (`ClientState`, `ClientStateMachine`) + `ClientBase` + estados principales.

---

## 6. Ideas de features

- **Variedad de clientes:** distintos tipos con paciencia, propina y velocidad propias
  (ya existe la base con `isAngryCustomer`; escalarlo a un ScriptableObject de "tipo de cliente").
- **Pedidos múltiples:** que un cliente pida más de un plato, o que una mesa con varios
  clientes comparta un pedido.
- **Propina por rapidez:** que `rewardAmount` se multiplique según cuánta paciencia le
  quedaba al cliente al momento de la entrega.
- **Grupos de clientes:** que entren de a dos o tres y ocupen mesas juntas.
- **Feedback visual del enojo:** que el modelo/material cambie progresivamente a medida que
  baja la barra de paciencia (`ClientHighlight` ya tiene la infraestructura de emisión).
- **Dificultad progresiva:** que `spawnInterval` y `maxClients` escalen con el tiempo del
  nivel en vez de ser fijos.
- **Clientes que se van solos de la fila** si esperan demasiado sin ser atendidos.

---

## 7. Plan de refactor — Sistema de clientes v2

> Diseño acordado el 2026-09-07. Objetivo: rehacer el módulo de clientes de forma
> **escalable**, porque el proyecto sigue creciendo. Se implementa **en paralelo** al viejo;
> el código antiguo no se borra hasta que lo nuevo funcione.

### 7.1 Decisiones cerradas

| Tema | Decisión |
|------|----------|
| Tipos de cliente | **2 por ahora**: Normal y Yakuza. El diseño deja preparado un tercero, pero no se implementa. |
| Consecuencia del Yakuza | Te persigue, te grita y **bloquea todas las interacciones** salvo el extintor. Con el extintor lo matás. (Ya funciona hoy, se conserva.) |
| Paciencia | **Dos timers independientes**, ver 7.5. |
| Sistema de pedidos | **Sí entra en el alcance.** UI con número de mesa + plato pedido. Ver 7.6. |
| Convivencia | Todo lo nuevo va en el namespace `RestaurantChaos.Clients` para no chocar con las clases viejas. |
| Wiring en Unity | Lo hace Enzo a mano. Cada fase entrega un checklist de pasos en el editor. |

### 7.2 Estructura de archivos nueva

```
Assets/Scripts/Clients/
├── Data/
│   ├── MealDefinition.cs          (SO) datos de un plato
│   └── ClientTypeDefinition.cs    (SO) config de un tipo de cliente
├── States/
│   ├── ClientState.cs             clase base abstracta
│   ├── QueueState.cs
│   ├── FollowPlayerState.cs
│   ├── SitState.cs
│   ├── LeaveState.cs
│   ├── AngryChaseState.cs
│   └── BlownAwayState.cs
├── Types/
│   ├── ClientBase.cs              el "molde": gameloop común
│   ├── NormalClient.cs
│   └── YakuzaClient.cs
├── Spawning/
│   ├── ClientSpawner.cs           solo instancia
│   ├── ClientQueue.cs             solo maneja el orden de la fila
│   └── ClientRegistry.cs          lista de clientes activos + eventos
├── Orders/
│   ├── OrderTicket.cs             datos de un pedido activo
│   ├── OrderBoard.cs              registro de pedidos + eventos
│   ├── OrderBoardUI.cs            contenedor de tarjetas
│   └── OrderTicketUI.cs           una tarjeta (mesa + ícono + barra)
└── Components/
    ├── ClientPatience.cs          los dos timers
    ├── ClientOrderDisplay.cs      el visual 3D sobre el cliente
    └── ClientStateMachine.cs      el motor de la FSM
```

`ClientHighlight.cs` **se reusa tal cual está**, no se reescribe.

### 7.3 ScriptableObjects

```csharp
[CreateAssetMenu(menuName = "Restaurant/Meal")]
public class MealDefinition : ScriptableObject
{
    public string displayName;
    public ItemType itemType;             // llave que ya usan las estaciones
    public GameObject orderVisualPrefab;  // el 3D que flota sobre el cliente
    public Sprite uiIcon;                 // para la tarjeta de pedido
    public int basePrice;                 // resuelve el bug B2
}
```

```csharp
[CreateAssetMenu(menuName = "Restaurant/Client Type")]
public class ClientTypeDefinition : ScriptableObject
{
    public string displayName;

    [Header("Movimiento")]
    public float moveSpeed;
    public float chaseSpeedMultiplier;

    [Header("Paciencia")]
    public float queuePatienceSeconds;    // timer A
    public float seatedPatienceSeconds;   // timer B
    public PatienceCarryOver[] carryOverRules;

    [Header("Pedidos")]
    public MealDefinition[] possibleMeals; // ChooseMeal() siempre elige random de esta lista

    [Header("Comportamiento agresivo")]
    public float yellCooldown;
    public float yellDistance;
}
```

**Ventaja clave:** agregar un plato o un tipo de cliente pasa a ser *crear un asset*, no
tocar código ni rellenar arrays en cada prefab.

⚠️ **Simplificación (2026-09-09):** se sacó el bool `picksRandomMeal` que estaba acá antes.
`ChooseMeal()` **siempre** elige al azar de `possibleMeals`. No hace falta una rama "fijo"
separada: elegir al azar sobre una lista de un solo plato da ese plato siempre — el
comportamiento "fijo" del día 1 (ver sección 8) sale solo, gratis, sin código especial.

### 7.4 FSM de clases

```csharp
public abstract class ClientState
{
    protected readonly ClientBase client;
    protected ClientState(ClientBase client) => this.client = client;

    public virtual void Enter() { }
    public virtual void Tick()  { }
    public virtual void Exit()  { }
}

public class ClientStateMachine
{
    public ClientState Current { get; private set; }

    public void ChangeState(ClientState next)
    {
        Current?.Exit();
        Current = next;
        Current.Enter();
    }

    public void Tick() => Current?.Tick();
}
```

Transiciones:

```
QueueState ──(interacción del jugador)──> FollowPlayerState ──(silla libre)──> SitState
                                                                                  │
                                                                       (comida correcta)
                                                                                  ▼
                                                                             LeaveState
     │                    │                       │
     └────────────────────┴───────────────────────┘
              (se acaba la paciencia)
                          │
              ┌───────────┴───────────┐
         NormalClient            YakuzaClient
         LeaveState              AngryChaseState ──(extintor)──> BlownAwayState
```

El `Enter`/`Exit` de cada estado es donde vive el manejo del `NavMeshAgent`, la silla y los
timers. Hoy eso está desparramado en 6 métodos distintos, que es de donde salen los bugs.

### 7.5 Sistema de paciencia (dos fases)

Son **dos timers totalmente independientes**:

- **Timer A — llegada / fila.** Arranca cuando el cliente spawnea. Corre mientras está en
  `QueueState` **y** en `FollowPlayerState` (sigue impaciente mientras lo llevás a la mesa).
  Se detiene al sentarse.
- **Timer B — sentado.** ⚠️ Arranca **cuando se revela el pedido**, no al sentarse. Entre que
  se sienta y termina la animación de "pensando" **no corre ningún timer**. Decisión tomada
  para no castigar al jugador con tiempo que corre antes de que pueda saber qué cocinar
  (mismo criterio que Overcooked).

Podés sentar a un cliente con 1 segundo restante del timer A y no pasa nada: son tiempos
distintos.

**Regla de arrastre (carry-over):** si al sentarse la barra del timer A quedó por debajo del
umbral, el timer B **no arranca lleno**.

- Configuración actual: `{ queueThreshold: 0.60, seatedStartFill: 0.80 }`
- O sea: si la barra de fila bajó del 60%, el timer B arranca al 80%.
- Si se cumple más de una regla, gana la más severa (el `seatedStartFill` más bajo).

```csharp
public class ClientPatience : MonoBehaviour
{
    public enum Phase { Idle, Queue, Seated, Stopped }

    public event Action Expired;
    public float NormalizedFill { get; }   // 0..1, alimenta la barra de UI

    public void BeginQueuePhase(float duration);
    public void BeginSeatedPhase(float duration, float startFill);
    public void Stop();
}
```

Cuando `Expired` se dispara, `ClientBase` llama al hook `OnPatienceExpired()`, que cada tipo
redefine. Esto vale para **las dos fases** — si se le acaba en la fila, el Yakuza también se
enoja.

**Visual de la barra (2026-09-09):** por ahora la paciencia **no es UI de pantalla** — sigue
siendo, igual que en el código viejo (`ClientHappiness.happinessFill`), un `Canvas` en
**World Space** flotando arriba de la cabeza del cliente. `ClientPatience.NormalizedFill`
alimenta ese mismo `Image.fillAmount` de siempre; solo cambia qué componente lo calcula (dos
fases en vez de un timer único). No se toca el `OrderTicket` / la barra de pedidos para nada
de esto — son dos sistemas de UI completamente separados. Si más adelante se quiere mostrar
la paciencia también en la barra de pedidos, es decisión de diseño/arte pendiente, no algo
que se está construyendo ahora.

### 7.6 Sistema de pedidos + UI

- `Chair` gana un campo `tableNumber` (int). Si varias sillas comparten mesa, comparten número.
- Al revelarse el pedido (después del "pensando"), se abre un `OrderTicket`:

```csharp
public class OrderTicket
{
    public int TableNumber;
    public MealDefinition Meal;
    public ClientBase Owner;
}
```

- `OrderBoard` mantiene la lista de tickets activos y expone `TicketOpened` / `TicketClosed`.
- `OrderTicketUI` = **solo** el fondo (`Image`, sprite = `MealDefinition.uiIcon`, el cartel
  completo). Nada más — ver por qué en la nota de abajo.

El ticket se cierra cuando el cliente es servido, se va enojado, o es eliminado.

⚠️ **Decisión de arte (2026-09-09):** la artista entrega el arte de cada plato como **una
sola imagen completa** (el cartel/bolsita entero, con la comida ya dibujada adentro) — no
como un cartel vacío + una foto separada para poner encima. No se dibuja nada por código
encima del cartel; solo se elige qué sprite va en cada casillero.

⚠️ **La barra de paciencia NO va acá.** Por ahora sigue siendo únicamente el `Canvas` en
World Space arriba de la cabeza del cliente (ver 7.5) — no se agrega a la barra de pedidos.
Es una decisión de diseño/arte que puede cambiar más adelante, pero no forma parte de esta
implementación.

⚠️ **Decisión de layout (2026-09-09) — reemplaza el `VerticalLayoutGroup`:** la artista hizo
una **"barra de pedidos"**: una imagen fija horizontal con 4 posiciones para mesas. Los
números **van arriba de la barra** (parte del arte, no texto dinámico) y el ticket aparece
debajo, sobre la barra misma — no se superponen, así que el ticket **no necesita repetir el
número de mesa**, la posición ya lo indica:

```
1        2      3        4
-------------------------------   ← la barra
[ticket]        [ticket]          ← aparecen debajo, solo en las mesas ocupadas
```

Esto es más simple que la lista dinámica que teníamos pensada:

- `OrderBoardUI` ya no instancia/destruye tarjetas en una lista que crece. Tiene un array
  fijo `[SerializeField] private OrderTicketUI[] slots`, uno por posición de la barra
  (hoy 4), **ubicados a mano en el editor** justo debajo de cada número del arte.
- Cada `OrderTicketUI` arranca oculto (`SetActive(false)`).
- `TicketOpened(ticket)` → `slots[ticket.TableNumber - 1].Show(ticket.Meal)`.
- `TicketClosed(tableNumber)` → `slots[tableNumber - 1].Hide()`.

**No confundir dos números distintos que existen en el juego:**
- `ClientQueueSpawner.maxClients` (hoy 3) — cuántos clientes esperan en la fila **antes**
  de sentarse. Es la cola.
- Los 4 casilleros de la barra de pedidos — cuántas **mesas físicas** hay en el nivel para
  sentarse. Es la capacidad de mesas.

Son conceptos independientes, no tienen que coincidir en cantidad.

La barra de pedidos es arte de nivel, hecho a mano por la artista con una cantidad fija de
casilleros — a diferencia de `MealDefinition`/`ClientTypeDefinition`, **no** está pensada
para ser escalable por datos: agregar una 5ª mesa el día de mañana requiere que ella
redibuje la barra con un número más, así que hardcodear el array en 4 es lo correcto acá,
no una limitación a evitar.

### 7.7 Desacople de sistemas

`ClientRegistry` lleva la lista de clientes activos y dispara eventos:

```csharp
public static event Action<ClientBase> ClientSpawned;
public static event Action<ClientBase> ClientServed;
public static event Action<ClientBase> ClientLost;
public static event Action<ClientBase> ClientDespawned;
public static bool AnyChasing { get; }   // reemplaza IsBeingChased()
```

Esto elimina de una:
- El `FindObjectsByType` **por frame** de `LevelManager.Update()` (deuda D3).
- El `FindObjectsByType` **por tecla E** de `PlayerInteraction.IsBeingChased()`.
- Los `FindObjectsByType` de `Chair` para encontrar clientes.

`LevelManager` pasa a suscribirse a eventos en vez de encuestar la escena.

### 7.8 Economía

`MoneyPickup` gana un `SetAmount(int)`. Al entregar, el monto sale de
`MealDefinition.basePrice` en vez del 30 hardcodeado — esto cierra el bug B2.

Queda un hook preparado (sin implementar) para propina según cuánta paciencia le quedaba
al cliente.

### 7.9 Orden de implementación

| Fase | Qué se hace | Se puede probar en Unity al terminar |
|------|-------------|--------------------------------------|
| **1** | Los 2 ScriptableObjects + crear los assets | ✅ Código hecho 2026-09-08. Falta crear los assets — ver checklist abajo |
| **2** | FSM base (`ClientState`, `ClientStateMachine`) + `ClientBase` + los 5 estados principales | ✅ Código hecho 2026-09-09. No testable todavía — falta `NormalClient` (fase 3) |
| **3** | `NormalClient` (`ClientPatience` ya se hizo en la fase 2) + botón de debug en `ClientBase` para probar sin esperar a la fase 4 | ✅ **Probado end-to-end en Unity 2026-09-10** — cola, seguir, sentarse, pedido, entrega, salida, todo funcionando |
| **4** | `ClientSpawner` + `ClientQueue` + `ClientRegistry` (el spawner **inyecta** el `ClientTypeDefinition` al spawnear, no lo trae fijo el prefab — ver sección 8) | ✅ Código hecho 2026-09-11 — checklist en 7.12 |
| **5** | `YakuzaClient` + `AngryChaseState` + `BlownAwayState` | ✅ Código hecho 2026-09-12 — checklist en 7.13 |
| **6** | Sistema de pedidos + UI de tickets | ✅ Código hecho 2026-09-12 — checklist en 7.14 |
| **7** | `LevelManager` nuevo (evento-driven) + `MoneyPickup` conectado al precio real | ✅ Código hecho 2026-09-12 — checklist en 7.15 |
| **8** | Borrar `Scripts/clientes level 1` y los scripts sueltos viejos | — |

Cada fase entrega su propio checklist de pasos a hacer en el editor de Unity.

### 7.10 Checklist Unity — Fase 1 (ScriptableObjects)

> Revisado 2026-09-09 después de definir el sistema de días (sección 8). **Alcance recortado
> a solo lo que hace falta para el Día 1** — Yakuza y Tempura son día 2, no bloquean nada
> todavía. Esta es la versión vigente, ignorar cualquier lista anterior.

Código ya escrito en `Assets/Scripts/Clients/Data/`: `MealDefinition.cs`,
`ClientTypeDefinition.cs` (ya sin `picksRandomMeal`), `PatienceCarryOver.cs`.

**Lo que tenés que crear en el editor — 2 assets en total:**

1. **Esperar a que Unity compile** (sin errores en la consola).
2. Carpeta `Assets/Data/Meals` → click derecho → `Create > Restaurant > Meal` → **un solo
   asset, el Wok**:
   - `Display Name` = "Wok Rice" (o como le digan)
   - `Item Type` = `WokRice`
   - `Order Visual Prefab` = `Food_ArrozAlWok.fbx` (el mismo que ya usa `Client.prefab` hoy
     en `possibleMeals[].visualPrefab` para ese plato — copiá esa referencia)
   - `Ui Icon` = dejalo vacío por ahora, se usa recién en la fase 6
   - `Base Price` = el valor que tenga hoy ese plato en `rewardAmount` (no importa si no
     coincide entre prefabs viejos, ya sabemos que ahí hay un error de carga — elegí
     cualquiera razonable, se ajusta fácil después)
3. Carpeta `Assets/Data/ClientTypes` → click derecho → `Create > Restaurant > Client Type`
   → **un solo asset, `Normal_Dia1`**:
   - `Move Speed` = lo que tenga hoy `RestaurantClient.moveSpeed` en `Client.prefab`
   - `Queue Patience Seconds` / `Seated Patience Seconds` = 90 (lo que tiene hoy
     `ClientHappiness.maxTime`)
   - `Carry Over Rules` → un elemento: `Queue Threshold = 0.6`, `Seated Start Fill = 0.8`
   - `Possible Meals` = arrastrar el asset de Wok del paso 2 (**el único por ahora**)
   - Los campos de `Aggressive Behavior` (`yellCooldown`, etc.) no aplican a Normal — dejalos
     con el valor por defecto, no se van a usar.
4. Avisame cuando estén creados. No hace falta engancharlos a nada todavía — eso es la fase 3.

**No crear todavía** (son para cuando lleguemos al día 2, no ahora): el `MealDefinition` de
Tempura, ni el `ClientTypeDefinition` de Yakuza. Cuando lleguemos ahí es literalmente repetir
estos mismos dos pasos con otros datos — no hay nada más que aprender.

No hace falta tocar los prefabs de cliente viejos ni las escenas en esta fase.

### 7.11 Checklist Unity — Fase 3 (`NormalClient` de prueba)

Código ya escrito: `Types/NormalClient.cs`, y `ClientBase` ganó un botón de debug
(`[ContextMenu] "Debug: Enter Queue Here"`) para poder probar sin esperar a la fase 4.

**Antes que nada:** completar los 2 campos vacíos de `Normal.asset` (pendiente desde la
fase 1) — `Possible Meals` = arrastrar `WokRice.asset`, `Carry Over Rules` = un elemento
`0.6 → 0.8`. Sin esto el cliente no tiene qué pedir.

**Armar el prefab de prueba** — el camino más corto es duplicar el `Client.prefab` viejo
(así reusás todo lo que ya está armado: modelo, `NavMeshAgent`, `Animator`, collider en la
layer correcta, el Canvas World Space con la barra, el punto donde flota el pedido) y
cambiarle el "cerebro":

1. Duplicar `Assets/Prefab/Client.prefab` → renombrarlo, por ejemplo `NormalClient_TEST.prefab`.
2. En el duplicado, **sacar** el componente `RestaurantClient` (el viejo).
3. Agregar el componente `NormalClient` (el nuevo). Al agregarlo, Unity va a pedir también
   `ClientPatience` y `ClientOrderDisplay` solo — es el `[RequireComponent]` que dejamos en
   `ClientBase`, están garantizados a existir juntos.
4. En `ClientPatience`, arrastrar al campo `Fill Image` la misma `Image` que usaba
   `ClientHappiness` para la barra (el nombre del campo cambió, así que **esta referencia no
   se copia sola** al sacar el componente viejo — hay que volver a arrastrarla a mano).
5. En `ClientOrderDisplay`, arrastrar al campo `Order Visual Point` el mismo transform que
   usaba `RestaurantClient` para eso.
6. En `ClientBase` (los campos aparecen igual en un componente heredado), completar
   `Thinking Prefab` con el mismo que tenía el viejo, y en **`Debug Config`** arrastrar
   `Normal.asset`.
7. **Armar una silla de prueba con `ClientChair`** (no la `Chair` vieja — no es compatible,
   ver bitácora). Duplicar una silla existente, sacarle el componente `Chair`, ponerle
   `ClientChair`, y completar a mano: `Sit Point` y `Money Spawn Point` (mismos transforms
   que ya tenía la silla vieja), y opcionalmente `Free Indicator Point` + `Free Indicator
   Prefab` si querés ver el indicador de silla libre (si no tenés un prefab genérico a mano
   todavía, dejalo vacío — no rompe nada, simplemente no se va a ver ese aviso).
8. Poner el cliente y la silla en una escena con NavMesh horneado, y algo con tag `"Exit"` en
   algún lado.
9. Play → clic derecho sobre el componente `NormalClient` en el Inspector → **"Debug: Enter
   Queue Here"**. El cliente debería quedar parado ahí con el timer de paciencia corriendo.
   Caminale cerca y apretá E: te debería empezar a seguir. Llevalo a una silla libre e
   interactuá con la silla para sentarlo. Esperá el "pensando" → debería mostrar el plato.
   Cociná un Wok y entregáselo → debería pararse, dejar plata, e irse por el `Exit`.

Si algo de esto no pasa, avisame el paso exacto donde se traba — con eso identifico rápido
si es un problema de wiring o un bug real en el código.

**No hace falta esperar a la fase 4** para este test — el botón de debug reemplaza
temporalmente al spawner. Cuando la fase 4 exista, este prefab de prueba se puede borrar
(o dejar solo para debugging futuro, no molesta a nada).

### 7.12 Checklist Unity — Fase 4 (`ClientSpawner` + `ClientQueue` + `ClientRegistry`)

Código ya escrito en `Assets/Scripts/Clients/Spawning/`: `ClientSpawner.cs`, `ClientQueue.cs`,
`ClientRegistry.cs`, más `Data/ClientSpawnEntry.cs`. `ClientRegistry` no se pone en ningún
GameObject — es una clase estática, no hace falta wiring para ella.

**Dos cambios en código ya existente, por qué hicieron falta:**
- `QueueState` ahora puede recibir una posición nueva (`SetQueuePosition`) sin reiniciarse.
  Hacía falta porque cuando el primero de la fila se va, todos los demás tienen que
  reacomodarse un lugar — pero si hubiera usado `EnterQueue` de nuevo para moverlos, les
  habría **reiniciado el timer de paciencia** a full. Con este método nuevo, siguen caminando
  a su nueva posición sin perder lo que ya esperaron.
- `ClientBase` ganó un evento `Served` (se dispara al entregar la comida, antes de que se
  vaya) — lo usa `ClientRegistry` para distinguir "cliente servido" de "cliente perdido" sin
  tocar `LevelManager` todavía. **`LevelManager.Instance.AddServedClient()` se sigue llamando
  igual que antes** desde `ClientBase.DeliverFood()` — queda duplicado a propósito hasta la
  fase 7, para no romper el conteo que ya funciona hoy.

**Armar en `GameScene`:**

1. Crear un GameObject vacío `ClientQueue`, agregarle el componente `ClientQueue`. En
   `Queue Positions`, poner 3 (o los que quieras) transforms vacíos marcando dónde se para
   cada lugar de la fila.
2. Crear un GameObject vacío `ClientSpawner`, agregarle el componente `ClientSpawner`.
   Completar: `Spawn Point` (de dónde aparecen), `Max Clients`, `Spawn Interval`, y en
   `References` → `Client Queue`, arrastrar el objeto del paso 1.
3. En `Available Entries`, agregar un elemento: `Client Prefab` = tu `NormalClient.prefab`,
   `Config` = `Normal.asset`.
4. Con esto ya no hace falta el botón de debug para probar — Play, y a los pocos segundos
   debería aparecer un cliente caminando solo hacia la cola.

**Qué probar específicamente de nuevo en esta fase** (más allá de lo que ya probamos en la
fase 3): que la fila reacomode bien. Con `Max Clients` en 2 o 3, dejá que se acumulen varios
en la cola, agarrá al del medio (no debería dejarte — solo el primero es interactuable),
agarrá al primero, y confirmá que el segundo pasa a ser el primero y camina a esa posición
sin que su barra de paciencia salte de golpe.

### 7.13 Checklist Unity — Fase 5 (`YakuzaClient` + `AngryChaseState` + `BlownAwayState`)

`AngryChaseState` ya existía desde la fase 2 (escrito y esperando un cliente real que lo use).
Código nuevo en esta fase: `Types/YakuzaClient.cs` (solo redefine `OnPatienceExpired()` para
entrar en `AngryChaseState` — en cualquiera de las dos fases de paciencia, fila o sentado, ver
7.5) y `States/BlownAwayState.cs` (física del extintor, portada 1:1 de `RestaurantClient.
GetBlownAway()`: `Rigidbody` + impulso + torque + se destruye a los 2.5 seg).

**Dos archivos fuera de `Scripts/Clients` tocados, por qué hicieron falta:**
- `ClientBase` ganó `GetBlownAway(Vector3 force)` — dispara `BlownAwayState` salvo que el
  cliente ya esté yéndose o ya esté volando.
- `ExtinguisherFoamCollision.cs` ahora reconoce **tanto** al `RestaurantClient` viejo como a un
  `ClientBase` nuevo persiguiendo (`IsChasing`) — los dos caminos conviven, no se tocó el viejo.
- `PlayerInteraction.IsBeingChased()` ahora también devuelve `true` si `ClientRegistry.
  AnyChasing` es cierto, para que la restricción "solo podés usar el extintor" aplique también
  cuando el que persigue es un Yakuza nuevo.

**Armar en Unity:**

1. Crear el asset `Assets/Data/ClientTypes/ClientType_Yakuza.asset` (`Create > Restaurant >
   Client Type`). Los campos de `Aggressive Behavior` (`Yell Cooldown/Distance/Duration`) sí
   aplican acá — dejalos en los valores que ya usaba `RestaurantClient` para el Yakuza viejo,
   o los defaults si no te acordás. `Possible Meals` = el mismo `WokRice.asset` por ahora
   (Tempura es día 2, todavía no existe el asset).
2. Duplicar `NormalClient.prefab` → renombrar a `YakuzaClient.prefab`. Sacarle el componente
   `NormalClient` y ponerle `YakuzaClient` en su lugar (mismo `[RequireComponent]` que ya
   conocés, no pide nada nuevo).
3. Cambiarle el modelo/animator al prefab por los de `AngryClient` (el controller
   `AngryCustomer.controller` ya tiene los tres parámetros que usa el código —
   `Speed`, `Sitting`, `Yell` — no hay que tocar nada ahí).
4. En `ClientSpawner` → `Available Entries`, agregar un segundo elemento: `Client Prefab` =
   `YakuzaClient.prefab`, `Config` = `ClientType_Yakuza.asset`.
5. Confirmar que el `ExtinguisherProp` (el que ya usás hoy) sigue en la layer/tag que
   `PlayerInteraction` espera — no cambió nada de ese lado.

**Qué probar:** dejar que un Yakuza se quede sin paciencia (en la fila o ya sentado) → debería
levantarse/salir de la fila y perseguirte gritando cada `Yell Cooldown` segundos cuando estás
cerca. Mientras te persigue, `E` no debería interactuar con nada salvo el extintor. Apagalo de
cerca → debería salir volando y desaparecer a los ~2.5 seg.

### 7.14 Checklist Unity — Fase 6 (sistema de pedidos + UI de tickets)

Código nuevo en `Assets/Scripts/Clients/Orders/`: `OrderTicket.cs` (datos: `TableNumber`,
`Meal`, `Owner`), `OrderBoard.cs` (clase estática, eventos `TicketOpened`/`TicketClosed` — no
va en ningún GameObject, mismo patrón que `ClientRegistry`), `OrderTicketUI.cs` (una tarjeta:
`Show(meal)` le pone el sprite y se activa, `Hide()` se desactiva, arranca oculta sola en
`Start()`), `OrderBoardUI.cs` (array fijo `slots[4]`, se suscribe a `OrderBoard` y hace
`slots[ticket.TableNumber - 1].Show(...)` / `slots[tableNumber - 1].Hide(...)`).

**Dos cambios en código existente:**
- `ClientChair` ganó el campo `Table Number` (default 1) — si varias sillas comparten mesa,
  ponerles el mismo número.
- `SitState` ahora abre el ticket (`OrderBoard.OpenTicket`) en el mismo momento en que ya
  revela el plato (`OrderRoutine`, después del "pensando"), y lo cierra (`OrderBoard.
  CloseTicket`) en `Exit()`. Como `ChangeState()` siempre llama `Exit()` del estado viejo antes
  de entrar al nuevo, esto cierra el ticket automáticamente sin importar el motivo de la salida
  (entrega, paciencia agotada, o Yakuza enojándose) — no hizo falta tocar `LeaveState` ni
  `AngryChaseState` para nada.

**Ya tenés el arte** en `Assets/2D Assets/Tickets/`: `barraDePedidos.png` (la barra con los 4
números) y `Pedido_ArrozAlWok.png` / `Pedido_Tempuras.png` (las tarjetas — `WokRice.asset` ya
apunta a la suya en `Ui Icon`).

**Armar en `GameScene`:**

1. En el Canvas de UI, poner `barraDePedidos.png` como imagen de fondo donde quieras que viva
   la barra (ej. arriba de la pantalla).
2. Crear 4 GameObjects de UI (`Image`), uno debajo de cada número del arte de la barra.
   Agregarles el componente `OrderTicketUI` y arrastrar esa misma `Image` al campo `Icon`.
   Dejarlos sin sprite asignado — se les pone solo runtime.
3. Crear un GameObject vacío (puede ser el padre de la barra) con el componente
   `OrderBoardUI`. En `Table Slots`, arrastrar los 4 del paso 2 **en orden** (posición 0 =
   mesa 1, ..., posición 3 = mesa 4).
4. En cada `ClientChair` de la escena, completar `Table Number` según a qué mesa física
   corresponde (mesas con varias sillas comparten número).

**Qué probar:** sentar un cliente y esperar el "pensando" → debería aparecer la tarjeta del
plato justo debajo de su número de mesa. Entregarle la comida (o dejar que se vaya
enojado) → la tarjeta debería desaparecer. Con dos clientes en mesas distintas al mismo
tiempo, confirmar que cada tarjeta aparece en su propio número sin pisar a la otra.

### 7.15 Checklist Unity — Fase 7 (`LevelManager` nuevo + dinero real)

**Importante — esto NO es un rewire del `LevelManager` viejo.** Ese vive en
`Scripts/clientes level 1/LevelManager.cs` (namespace global) y sigue intacto, usado solo por
las escenas viejas. `GameScene` todavía no tenía ningún manager de nivel armado — se creó uno
**nuevo**, `RestaurantChaos.Clients.LevelManager` (`Assets/Scripts/Clients/LevelManager.cs`),
mismo nombre pero namespace distinto (igual que `ClientBase` convive con `RestaurantClient`),
así que no hay conflicto de compilación ni dependencia del viejo.

Qué cambia respecto al viejo:
- **Nada de `FindObjectsByType` por frame** (deuda D3, resuelta acá): en vez de contar
  `RestaurantClient` en escena, `Update()` chequea `ClientRegistry.ActiveCount` (ya existe
  desde la fase 4, es solo leer el tamaño de una lista) y `ClientSpawner.HasFinishedSpawning`
  (el spawner **nuevo**, no `ClientQueueSpawner`).
- **Cuenta clientes servidos por evento**, no por llamada directa: se suscribe a
  `ClientRegistry.ClientServed` en vez de que `ClientBase` le avise a mano. Por esto se sacó
  de `ClientBase.DeliverFood()` el `LevelManager.Instance.AddServedClient()` que había quedado
  ahí a propósito desde la fase 4 (duplicado temporal, documentado en su momento).
- **Bug B2 resuelto para el sistema nuevo:** `MoneyPickup` ganó `SetAmount(int)`;
  `ClientBase.SpawnMoneyOnTable()` ahora se lo llama con `chosenMeal.basePrice` en vez de
  dejar el `30` hardcodeado del prefab. El `RestaurantClient` viejo sigue con el bug tal cual
  (no se tocó, sigue siendo código viejo con B2 pendiente ahí).
- Panel final + animación de estrellas: **portado tal cual** del viejo (mismo criterio: una
  estrella por cliente servido) — no cambió nada de diseño, solo de dónde saca los datos.

**Qué NO hace todavía (a propósito, sigue siendo lo pospuesto):** no dispara ningún cambio de
día ni next-level — `EndLevel()` hoy solo prende el panel y anima las estrellas, igual que
antes. Conectarlo a "terminó el día, pasar al siguiente" es trabajo del futuro manejador de
días (sección 8.5), todavía no se construye.

**Armar en `GameScene`** (acá sí es todo nuevo, no había nada wireado):

1. Crear el panel final de UI (podés duplicar el `endPanel` de una escena vieja y traerlo, o
   armar uno nuevo) y las estrellas, igual que en Level 1/2.
2. Crear un GameObject vacío `LevelManager` (o usar el que ya tengas para managers), agregarle
   el componente `LevelManager` (el de `RestaurantChaos.Clients` — si Unity te ofrece dos en
   el buscador de componentes, elegí el que está en `Assets/Scripts/Clients/`).
3. Completar `References → Spawner` con tu `ClientSpawner` de la fase 4, `End Panel` con el
   panel del paso 1, y `Stars` con el array de estrellas.
4. Confirmar que el prefab de dinero (`moneyPrefab` en el `ClientBase` de cada cliente) tiene
   el componente `MoneyPickup` — si es el mismo prefab que ya usaba el sistema viejo, ya lo
   tiene, no hay que tocar nada ahí.

**Qué probar:** servir a un cliente y agarrar la plata que deja → el monto tiene que coincidir
con el `Base Price` del `MealDefinition` que pidió (no el 30 fijo de antes). Dejar que el
spawner termine y se vacíe la fila/las mesas → debería aparecer el panel final con una
estrella por cliente servido, en el mismo estilo de siempre.

---

## 8. Sistema de progresión por días

> **Decisión final (2026-09-11): una sola escena persistente para todo el juego** — probable-
> mente `GameScene` (a confirmar con Enzo). Hubo un ida y vuelta: 2026-09-09 arrancó como "una
> escena persistente", se pasó a "una escena por día" ese mismo día, y ahora vuelve a ser "una
> sola escena". Sin costo en ningún momento — el código de clientes (fases 1-3) nunca dependió
> de esta decisión, así que no hubo nada que deshacer ninguna de las dos veces.

### 8.1 La idea

Cada día que pasa se desbloquean más estaciones de cocina, más tipos de cliente, y más
platos posibles — y cada uno de esos días tiene su propio diseño de nivel. Ejemplo real que
dio Enzo:

- **Día 1** — solo cliente Normal, solo puede pedir Wok (un plato → "fijo" sale solo, ver 7.3).
- **Día 2** — se suma el Yakuza. Ambos pueden pedir Wok o Tempura, al azar.
- **Día 3/4** (todavía sin decidir cuál exactamente) — se suma otro plato más.

Al terminar un día se muestra una pantalla de resultados/score, y se pasa a la escena del
día siguiente.

### 8.2 Decisión: una sola escena, activando cosas con el tiempo

Todo el juego corre en una única escena persistente. A medida que pasan los días, se activan
más estaciones, más tipos de cliente y más platos — sin cargar ninguna escena nueva en
ningún momento del gameplay.

Esto trae de vuelta dos piezas que la versión anterior de este diseño había descartado —
**ninguna de las dos se programa todavía**, quedan para cuando se retome "el tema del
LevelManager y el score":

- **Desbloqueo de estaciones vuelve a ser código.** Cada estación arranca con su GameObject
  desactivado; algo (el futuro manejador de días) la activa cuando corresponde.
- **`DayDefinition` (asset por día) y un manejador de días** vuelven a hacer falta — qué se
  desbloquea cada día, y quién aplica eso y muestra el panel de resultados al cerrar el día.

### 8.3 Lo único que SÍ toca ahora: `ClientSpawner` (fase 4) tiene que ser "día-consciente"

Este es el único impacto real sobre el trabajo de clientes que sigue pendiente. Con una sola
escena, el spawner no puede tener una lista fija de `ClientSpawnEntry[]` configurada una vez
en el Inspector — esa lista tiene que poder **cambiar mientras el juego está corriendo**, a
medida que el manejador de días (cuando exista) habilite más tipos de cliente.

Diseño: en vez de leer un array fijo, `ClientSpawner` expone un método público para
actualizar qué puede spawnear:

```csharp
public void SetAvailableEntries(ClientSpawnEntry[] entries)
```

El día 1 arranca con `{ NormalClient.prefab, Normal_Dia1.asset }` únicamente. Cuando exista
el manejador de días, el día 2 va a llamar este mismo método con la lista ampliada — sin
tocar el spawner para nada. Mientras tanto (sin manejador de días todavía), el spawner puede
arrancar con una lista por defecto seteada a mano en el Inspector, para poder probarlo ya —
`SetAvailableEntries` queda listo para cuando haga falta usarlo de verdad.

### 8.4 Lo que NO cambia (fases 1-3, ya programadas e intactas)

- `MealDefinition` / `ClientTypeDefinition` siguen siendo assets reusables, sin tocar.
- `ClientBase.Initialize(config)` sigue inyectando la config al spawnear, nunca fija en el
  prefab — esto es exactamente lo que hace que este vaivén entre "una escena" y "muchas
  escenas" no le importe nada al cliente en sí. Por eso no hubo que deshacer nada ninguna de
  las dos veces que cambió esta decisión.

### 8.5 Qué faltaba resolver (histórico — ya implementado en 8.6)

- ~~Activar estaciones y llamar `ClientSpawner.SetAvailableEntries` al empezar cada día, más el
  panel de resultados al cerrarlo.~~
- ~~`DayDefinition` y su manejador.~~
- `MoneyManager` sigue sin `DontDestroyOnLoad` — con una sola escena esto deja de ser
  urgente durante el gameplay en sí (no hay reload entre días), pero sigue siendo relevante
  si en algún momento se vuelve al Menu y se retoma la partida. **Todavía sin resolver.**
- Cuántos días tiene el juego y qué desbloquea cada uno más allá del día 2 — sigue **sin
  decidir**, y no hace falta decidirlo para que el sistema funcione (ver 8.6, la lista de
  días es un array, se extiende agregando assets).

### 8.6 Implementación — `DayDefinition` + `DayManager` (2026-09-13)

⚠️ **Corrección de diseño (2026-09-13):** las estaciones **no** arrancan desactivadas — están
todas presentes en escena desde el día 1, pero **bloqueadas**. Enzo aclaró el diseño real:
mirando una estación bloqueada (heladera, tempuras) se ve un ícono de candado, y si apretás E
no pasa nada (el sonido de "bloqueado" queda pendiente, se decide después si se reworkea
`SoundManager` antes). Esto es distinto de "desactivada" — la estación sigue ahí, visible,
solo no responde a la interacción todavía.

Nuevo componente `Stations/LockableStation.cs` (no vive en `Clients/`, es de uso general para
cualquier estación): campo `Is Locked` (bool, default `true`) + `Lock Icon` (GameObject). Se le
suma como componente aparte a la estación (Fridge, TempuraStation, etc.) — **no hace falta
tocar ningún script de estación existente**: el bloqueo se resuelve en
`PlayerInteraction.TryInteract()`, que ahora chequea si el candidato tiene un `LockableStation`
bloqueado y, si es así, lo saltea (`continue`) antes de llegar a `interactable.Interact()`.
Mismo patrón que ya usa para el matafuegos/persecución.

⚠️ **Ajuste (2026-09-13, más tarde):** el candado no se muestra solo por estar bloqueado — solo
cuando **además** el jugador está dentro del rango de interacción (la misma esfera verde del
gizmo, `interactPoint` + `interactRadius`, no algo nuevo). `LockableStation.SetIconVisible(bool)`
es un segundo campo interno (`isPlayerInRange`) separado de `isLocked`; el ícono se prende solo
si las dos condiciones dan verdadero (`isLocked && isPlayerInRange`). `PlayerInteraction` ahora
hace un `OverlapSphere` extra por frame en `Update()` (con el mismo `interactPoint`/
`interactRadius`/`interactableLayer` de siempre, no un raycast) para saber qué `LockableStation`
están dentro de esa esfera en cada momento, y les avisa cuando entran o salen — llevando un
registro (`visibleLockIcons`) de a quién le prendió el ícono para poder apagárselo cuando deja
de estar en rango. Si hay dos estaciones bloqueadas dentro de la esfera a la vez, se ven las
dos — no hay noción de "una sola a la vez", es simplemente "¿estás cerca o no?".

⚠️ **Segunda corrección de diseño (2026-09-13, más tarde):** `stationsToUnlock` **no puede
vivir en `DayDefinition`.** Es una limitación de Unity, no un error de tipeo: un asset
ScriptableObject no puede guardar una referencia a un objeto que vive **en la escena** (la
heladera, las tempuras) — solo puede referenciar otros *assets* (prefabs, otros SO). Por eso
tirab a "type mismatch" al arrastrar, incluso con un GameObject de prueba nuevo con el
componente bien puesto: el problema nunca fue el componente, era que el *destino* era un
asset. `clientEntries` nunca tuvo este problema porque ahí todo son referencias a assets
(prefabs, `ClientTypeDefinition`), nada de la escena.

Solución: la lista de estaciones se mudó a `DayManager` (que sí vive en la escena, así que
**sí** puede referenciar objetos de la escena). Ahora `DayManager` tiene un struct
`DaySetup { DayDefinition definition; LockableStation[] stationsToUnlock; }`, y su array
`Days` es de `DaySetup`, no de `DayDefinition` directamente. Cada elemento del array te va a
mostrar dos campos en el Inspector: `Definition` (ahí arrastrás el asset `Day_1`/`Day_2`) y
`Stations To Unlock` (ahí sí podés arrastrar la heladera/tempuras sin problema, porque el
campo ahora vive en un componente de escena).

**`DayDefinition`** (`Data/DayDefinition.cs`, SO) quedó más chico: solo `displayName` y
`clientEntries[]` (lo mismo que ya le pasabas a mano al `ClientSpawner`). Ya no tiene ningún
campo de estaciones.

**`DayManager`** (`DayManager.cs`, singleton): tiene el array `Days` (de `DaySetup`, ver
arriba). Al arrancar la escena, `Start()` hace `StartDay(0)`. `AdvanceToNextDay()` (pensado
para colgarlo del botón "Continuar" del panel final) avanza al siguiente índice — si ya era
el último día, no hace nada (qué pasa al terminar **todos** los días es una pantalla de
"juego completo" que todavía no se diseñó, queda para más adelante). `StartDay(i)` hace tres
cosas: desbloquea (`SetLocked(false)`) las estaciones de `stationsToUnlock` de ese
`DaySetup` — **acumulativo**, cada uno lista solo las estaciones **nuevas** de ese día, nunca
hace falta repetir las de días anteriores, porque `SetLocked(false)` nunca las vuelve a
bloquear — le pasa `definition.clientEntries` al `ClientSpawner` (`SetAvailableEntries` +
`StartSpawning()`), y le avisa al `LevelManager` (`StartNewDay()`) para que resetee su propio
estado (paciencia del panel, contador de servidos, estrellas).

**Dos cambios de comportamiento en código ya existente, importantes para el wiring:**
- `ClientSpawner` **ya no arranca solo.** Antes, `Start()` lanzaba el spawneo apenas cargaba
  la escena; ahora necesita que alguien externo llame `StartSpawning()` (hoy, exclusivamente
  `DayManager`). **Si armás una escena de prueba sin `DayManager`, el spawner no va a
  spawnear nada** — no es un bug, es que ahora el manejador de días es quien decide cuándo
  empieza cada tanda.
- `LevelManager` sí sigue auto-inicializándose solo en su propio `Start()` (sigue siendo
  testeable de forma independiente, sin `DayManager`) — pero cuando `DayManager` también
  existe, ambos corren su reset al arrancar la escena (uno por su propio `Start()`, otro
  porque `DayManager.StartDay(0)` llama `StartNewDay()`). Es inofensivo — el segundo reset
  simplemente vuelve a programar el mismo `Invoke` de 1 segundo — pero es intencional, no un
  descuido: así `LevelManager` nunca depende obligatoriamente de que exista un `DayManager`.

**Armar en `GameScene`:**

1. Crear un GameObject vacío `DayManager`, agregarle el componente `DayManager`. Completar
   `References → Spawner` (tu `ClientSpawner`) y `Level Manager` (tu `LevelManager`).
2. En **las tres** estaciones que deben arrancar bloqueadas el día 1 — `Fridge`,
   `TempuraStation` **y** `FryerStation` — agregarle el componente `Lockable Station`. Dejar
   `Is Locked` tildado (`true`, el default). En `Lock Icon`, poner el GameObject del candado
   (podés armar un ícono simple por ahora — un sprite de candado en un Canvas World Space
   sobre la estación, mismo tipo de setup que ya usaste para la barra de paciencia — y
   perfeccionarlo después). Las estaciones que **sí** están disponibles desde el día 1 (Wok)
   no necesitan este componente para nada.
   ⚠️ Aunque bloquear solo la heladera ya alcanzaría mecánicamente (Tempura pide `Seafood`,
   que solo sale de la heladera, y Fryer pide `BreadedTempura`, que solo sale de Tempura —
   sin heladera nunca llegás a ninguna de las dos), **se decidió bloquear las tres** igual:
   sin candado, tocar Tempura/Fryer sin el ingrediente correcto no da ningún feedback visible
   en el juego (`TempuraStation`/`FryerStation` solo hacen `Debug.Log`, que no se ve fuera del
   editor) — con las tres bloqueadas, el jugador siempre tiene una señal clara de "todavía no".
3. Carpeta `Assets/Data/Days/` → `Create > Restaurant > Day` → **`Day_1`**: `Client Entries` =
   lo mismo que ya tenías cargado a mano en el `ClientSpawner` (Normal + `Normal.asset`). Ya
   no tiene campo de estaciones — eso ahora va en el `DayManager` (paso 5).
4. Crear **`Day_2`**: `Client Entries` = Normal + `Normal.asset` **y** Yakuza +
   `Yakuza.asset`. Ojo: para que el día 2 realmente pueda servir Tempura hace falta además
   crear el `MealDefinition` de Tempura (`Pedido_Tempuras.png` ya está en
   `Assets/2D Assets/Tickets/`) y sumarlo a `Possible Meals` en ambos `ClientTypeDefinition`
   (Normal y Yakuza) — si no, `ChooseMeal()` va a seguir devolviendo solo Wok aunque la
   estación ya esté desbloqueada.
5. En `DayManager → Days`, poner tamaño 2 y completar cada elemento (ahora cada uno tiene
   **dos** campos):
   - Índice 0: `Definition` = `Day_1`, `Stations To Unlock` = vacío (el día 1 no desbloquea
     nada nuevo).
   - Índice 1: `Definition` = `Day_2`, `Stations To Unlock` = arrastrar acá la heladera y
     las tempuras (las que tienen `LockableStation` del paso 2) — esto **sí** funciona ahora
     porque el campo vive en `DayManager`, un componente de escena, no en el asset.
6. En el botón del panel final (`End Panel`), agregarle un `OnClick` que llame
   `DayManager.AdvanceToNextDay()` — arrastrá el GameObject `DayManager` de la escena al slot
   del botón (no se puede apuntar a `DayManager.Instance` directo desde el Inspector, tiene
   que ser la referencia real del objeto).
7. Confirmar que `ClientSpawner` ya **no** tiene nada cargado a mano en `Available Entries`
   en el Inspector — ahora se lo pisa `DayManager` apenas arranca, así que lo que haya ahí no
   importa, pero para evitar confusión futura podés dejarlo vacío.

**Qué probar:** al entrar a `GameScene`, la heladera, la Tempura station y el Fryer deberían
verse con su candado y no responder a la E. Debería spawnear como el Día 1 de siempre (solo
Normal, solo Wok). Serví/dejá ir a todos hasta que aparezca el panel final. Apretá "Continuar"
→ el panel se cierra, los tres candados desaparecen (ya interactuás normal con las tres), y a
los pocos segundos empiezan a aparecer clientes Normales **y** Yakuzas, ambos pudiendo pedir
Wok o Tempura.

---

## 9. Checklist consolidado — estado de `GameScene` al 2026-09-13

> Snapshot puntual de qué falta armar en Unity **en este momento**, después de que un
> cambio de rama en GitHub Desktop borrara `LevelManager` y `MoneyManager` de la escena.
> Es un resumen accionable en orden; el detalle de cada pieza está en las secciones 7.13-8.6.
> Una vez armado todo, este checklist pierde vigencia (no lo mantengo actualizado después).

1. **Recrear `LevelManager`** — GameObject nuevo, componente `LevelManager` (el de
   `Scripts/Clients/`, no el viejo). Completar `Spawner` (tu `ClientSpawner`), `End Panel` y
   `Stars` (revisá primero si esos objetos de UI sobrevivieron o también hay que rehacerlos).
2. **Recrear `MoneyManager`** — GameObject nuevo, componente `MoneyManager`, completar
   `Money Text` con el texto de plata de la UI (revisá si ese texto sigue existiendo).
3. **Crear `DayManager`** — GameObject nuevo, componente `DayManager`. Completar
   `References → Spawner` y `Level Manager` (los de los pasos 1, ya tienen que existir).
4. **Agregar `Lockable Station`** a `Fridge`, `TempuraStation` **y** `FryerStation` (las tres,
   aunque bloquear solo la heladera ya alcanzaría mecánicamente — sin candado en Tempura/Fryer
   el jugador no recibe ningún feedback visible si las toca sin el ingrediente) — dejar
   `Is Locked` tildado, asignar un `Lock Icon` (podés armar uno simple ahora y mejorarlo
   después). Las estaciones del día 1 (Wok) no necesitan este componente.
5. **Crear el `MealDefinition` de Tempura** (`Assets/Data/Meals/`) — `Ui Icon` =
   `Pedido_Tempuras.png` (ya está en `Assets/2D Assets/Tickets/`), completar el resto igual
   que hiciste con `WokRice.asset`.
6. **Sumar Tempura a `Possible Meals`** en `Normal.asset` **y** `Yakuza.asset` (los dos
   `ClientTypeDefinition`) — si no, aunque desbloquees la estación van a seguir pidiendo
   solo Wok.
7. **Crear los assets `Day_1` y `Day_2`** (`Assets/Data/Days/`, `Create > Restaurant > Day`):
   solo `Client Entries` en cada uno — `Day_1` = Normal + `Normal.asset`; `Day_2` = eso
   **más** Yakuza + `Yakuza.asset`. (Ya no tienen campo de estaciones, ver nota abajo.)
8. **Cargar los días en `DayManager → Days`** — ⚠️ `Days` ahora es un array de dos campos por
   elemento, no un array de assets directo (Unity no deja que un asset referencie objetos de
   la escena, así que las estaciones se movieron acá). Índice 0: `Definition` = `Day_1`,
   `Stations To Unlock` = vacío. Índice 1: `Definition` = `Day_2`, `Stations To Unlock` =
   arrastrar la heladera, la Tempura station y el Fryer del paso 4 (esto sí funciona, porque
   ahora el campo vive en `DayManager`, no en el asset).
9. **Conectar el botón "Continuar"** del panel final: `OnClick` → arrastrar el GameObject
   `DayManager` de la escena → método `AdvanceToNextDay()`.
10. **Agregar `Hide While Blocking UI`** en dos lugares (componente ya existe, ver 8/9-cont.8):
    - En el `OrderBoardLogic` que ya tenés (el que tiene `OrderBoardUI`): `Visual Root` →
      `OrderBoardVisuals` (el mismo de siempre).
    - En algún objeto siempre activo (ej. el mismo `MoneyManager` del paso 2): `Visual Root`
      → el grupo que contiene la UI de plata (agrupalo bajo un GameObject si todavía no lo
      está).
11. **(Opcional, prolijidad)** Vaciar `Available Entries` en el Inspector del `ClientSpawner`
    — ya no importa lo que haya ahí, `DayManager` lo pisa apenas arranca.

**Qué probar al final, de punta a punta:** entrar a `GameScene` → heladera/tempuras con
candado, no responden a E → clientes Normales pidiendo solo Wok → servir/perder a todos →
panel final con estrellas y plata correcta → apretar "Continuar" → candados desaparecen →
empiezan a aparecer Normales **y** Yakuzas pidiendo Wok o Tempura al azar.

---

## 10. Bitácora

Registro de lo que voy haciendo, lo más nuevo arriba.

### 2026-09-13 (cont. 3)
- **Se confirmó bloquear las tres estaciones** (Fridge, TempuraStation, FryerStation), aunque
  bloquear solo la heladera ya alcanzaría mecánicamente para frenar la cadena completa de
  Tempura (Tempura pide `Seafood`, que solo sale de la heladera; Fryer pide `BreadedTempura`,
  que solo sale de Tempura) — la razón es que sin candado, tocar Tempura/Fryer sin el
  ingrediente no da ningún feedback visible (`Debug.Log` no se ve en build).
- **Candado visible solo si el jugador está cerca** (pedido de Enzo, no un bug): reusa la
  misma esfera de `PlayerInteraction` (`interactPoint` + `interactRadius`), no agrega ningún
  raycast — primer intento fue con raycast y no era lo que quería, se corrigió. `LockableStation`
  ganó `SetIconVisible()` (visibilidad separada del estado de bloqueo, campo interno
  `isPlayerInRange`); `PlayerInteraction` hace un `OverlapSphere` extra por frame para saber
  qué estaciones bloqueadas están dentro de rango en cada momento. Detalle en 8.6.

### 2026-09-13 (cont. 2)
- **`Stations To Unlock` no dejaba arrastrar nada — "type mismatch" incluso con un GameObject
  de prueba nuevo.** No era el componente: es que `DayDefinition` es un `ScriptableObject`
  (asset), y Unity **no permite que un asset referencie un objeto que vive en la escena**
  (solo otros assets — prefabs, otros SO). `clientEntries` nunca lo sufrió porque ahí todo son
  referencias a assets. Fix: el campo se mudó de `DayDefinition` a `DayManager` (que sí vive
  en la escena), envuelto en un nuevo struct `DaySetup { DayDefinition definition;
  LockableStation[] stationsToUnlock; }` — `DayManager.Days` pasó de `DayDefinition[]` a
  `DaySetup[]`. Detalle no obvio de Unity, buen candidato para recordar si vuelve a pasar
  con otro campo en el futuro. Checklist corregido en 8.6 y en la sección 9.

### 2026-09-13
- **Arrancado el manejador de días** (sección 8.6): `DayDefinition.cs` (SO: clientes +
  estaciones a desbloquear ese día) y `DayManager.cs` (singleton, array ordenado de días,
  `AdvanceToNextDay()` pensado para el futuro botón "Continuar" del panel final — que sigue
  sin existir, era la pregunta pendiente de ayer; ahora ya tiene a dónde apuntar).
  Confirmado antes de arrancar: nada bloqueaba empezar (fase 7 ya probada, fase 8 de borrado
  de código viejo puede esperar a que el sistema de días esté probado de punta a punta).
- Dos cambios de comportamiento necesarios para que el manejador pueda orquestar todo:
  `ClientSpawner` ganó `StartSpawning()` y **dejó de auto-arrancar solo** en `Start()` (ahora
  requiere que algo externo — hoy, `DayManager` — se lo pida); `LevelManager` ganó
  `StartNewDay()` (extrajo su reset de `Start()` a un método propio reusable, con
  `CancelInvoke` de por medio para no duplicar el chequeo de 1 segundo).
  Checklist completo (incluye qué falta para que el día 2 sirva Tempura de verdad —
  `MealDefinition` todavía no creado) en 8.6.
- **Corrección sobre la marcha:** el desbloqueo de estaciones no es `SetActive`. Enzo aclaró
  que las estaciones están todas presentes desde el día 1, pero **bloqueadas** — con un ícono
  de candado al mirarlas y sin responder a la E (el sonido de "bloqueado" queda pendiente a
  propósito, no tocar `SoundManager` todavía). Se creó `Stations/LockableStation.cs`
  (`Is Locked` + `Lock Icon`, de uso general, no específico de clientes) y el bloqueo se
  resuelve en `PlayerInteraction.TryInteract()` — si el candidato tiene un `LockableStation`
  bloqueado, se saltea antes de llegar a `Interact()`. **No hizo falta tocar ningún script de
  estación existente** (Fridge, TempuraStation, etc. quedan intactos). `DayDefinition.
  stationsToUnlock` pasó de `GameObject[]` a `LockableStation[]`, y `DayManager` llama
  `SetLocked(false)` en vez de `SetActive(true)`.

### 2026-09-12 (cont. 8)
- **Fase 7 confirmada funcionando en Unity** ("anda todo joya").
- **La UI del dinero también se pisaba con paneles bloqueantes** (heladera, minijuegos, etc.),
  igual que le pasaba antes a la UI de pedidos. En vez de copiar la misma lógica de
  `Cursor.visible` adentro de `MoneyManager`, se sacó esa responsabilidad de `OrderBoardUI` y
  se armó un componente genérico y reusable: `Managers/HideWhileBlockingUI.cs`. Cualquier UI
  que necesite esconderse detrás de un panel bloqueante le suma este componente aparte — nada
  que ver con qué UI es, solo necesita un `Visual Root`.
  **Pendiente en Unity:**
  - En el `OrderBoardLogic` que ya armaste (el que tiene `OrderBoardUI`), agregale también el
    componente `Hide While Blocking UI`, y arrastrale el mismo `OrderBoardVisuals` de siempre
    a su `Visual Root`. `OrderBoardUI` ya no tiene ese campo — se lo saqué, ahora solo maneja
    los tickets.
  - Para el dinero: agrupá la UI de plata (el texto + lo que la rodee) bajo un GameObject
    propio si todavía no lo está, y en algún objeto **siempre activo** (por ejemplo el mismo
    que tiene `MoneyManager`) agregá `Hide While Blocking UI`, apuntando `Visual Root` a ese
    grupo. Mismo cuidado de siempre: el componente no puede vivir adentro del grupo que apaga.

### 2026-09-12 (cont. 7)
- **Fase 7 probada en Unity, anda bien** (plata correcta, panel final, conteo de estrellas).
- **Bug de escala en las estrellas del panel final:** mismo patrón que el indicador de silla
  libre y el visual de pedido — `AnimateStar()` (portado del viejo) animaba hacia
  `Vector3.one` y la dejaba fija en `Vector3.one * 0.8f`, un valor absoluto que ignoraba
  cualquier escala puesta a mano en el editor (Enzo había agrandado las estrellas para que se
  vieran bien, y quedaban chiquitas igual). Fix: capturar `targetScale =
  starTransform.localScale` **antes** de resetear a cero, y animar/asentar contra ese valor en
  vez de `Vector3.one`. Mismo bug documentado como B7 para el `LevelManager` viejo (no se
  tocó, es código viejo).

### 2026-09-12 (cont. 6)
- **Fase 6 confirmada funcionando en Unity** ("anda todo joya").
- **Fase 7 hecha (código):** `Assets/Scripts/Clients/LevelManager.cs`, nuevo (no un rewire del
  viejo — `GameScene` no tenía ninguno armado todavía, se preguntó y Enzo confirmó). Mismo
  nombre que el viejo pero en namespace `RestaurantChaos.Clients`, sin conflicto. Event-driven
  (`ClientRegistry.ActiveCount` + `ClientRegistry.ClientServed`, nada de `FindObjectsByType`
  por frame — resuelve D3 para el sistema nuevo). De paso: `MoneyPickup.SetAmount(int)` nuevo,
  `ClientBase.SpawnMoneyOnTable()` ahora usa `chosenMeal.basePrice` (resuelve B2 para el
  sistema nuevo; el `RestaurantClient` viejo sigue con el bug, no se tocó) y se sacó de
  `ClientBase.DeliverFood()` el llamado directo a `LevelManager.Instance.AddServedClient()`
  que había quedado ahí a propósito desde la fase 4. Checklist completo (es un armado nuevo de
  UI en `GameScene`, no solo wiring) en 7.15.

### 2026-09-12 (cont. 5)
- **La UI de pedidos seguía tapándose con la heladera:** `FridgeUI` no pasa para nada por
  `MinigameManager` (abre su propio panel a mano), así que el chequeo de `isMinigameActive`
  no la detectaba. En vez de agregar un caso más, se buscó la señal que **ya** comparten
  todos los paneles bloqueantes del proyecto (heladera, los tres minijuegos, el menú de pausa,
  la pantalla de fin de nivel): todos llaman `CursorManager.ShowCursor()` exactamente al
  abrirse. `OrderBoardUI.Update()` ahora chequea directamente `Cursor.visible` en vez de
  `MinigameManager.isMinigameActive` — cubre cualquier UI bloqueante actual **y futura** sin
  tener que acordarse de sumarla a mano cada vez. Sin cambios pendientes en Unity, ya debería
  funcionar con el mismo `Visual Root` armado en el paso anterior.

### 2026-09-12 (cont. 4) — ajustes sueltos, no ligados a una fase
- **Bug de la cesta del Fryer:** `FryerVisuals.Start()` seteaba `targetBasketPos =
  basketUpPoint.position` a mano, así que la cesta arrancaba viajando hacia esa posición
  apenas cargaba la escena — sin importar si alguien jugó al Fryer alguna vez. Por eso se veía
  "volando" en el fondo mientras jugabas otro minijuego (ej. Wok). Fix: arranca en su propia
  posición (`targetBasketPos = basket.position`) y no se mueve hasta que `FryerController`
  llame `MoveBasketDown()`/`MoveBasketUp()` durante la secuencia real del minijuego — eso ya
  estaba bien encadenado, el único problema era el valor inicial.
- **UI de pedidos tapaba la UI del minijuego:** `OrderBoardUI` ganó conciencia de
  `MinigameManager.isMinigameActive` (mismo campo público que ya usa `FryerStation.Interact()`,
  no hizo falta agregar eventos nuevos al manager). Ojo con el detalle de diseño: el script
  **no puede desactivar su propio GameObject** en `Update()` — si lo hiciera, ese mismo
  `Update()` dejaría de correr y nunca podría reactivarse solo. Por eso el campo nuevo
  `Visual Root` apunta a un GameObject **distinto** (el que agrupa la barra + los 4 tickets);
  `OrderBoardUI` en sí sigue siempre activo escuchando `OrderBoard`, así que si un ticket se
  abre/cierra mientras el board está oculto por un minijuego, el estado ya queda correcto
  aplicado por debajo y se ve bien apenas se reactiva.
  **Pendiente en Unity — ojo con la jerarquía:** `OrderBoardUI` tiene que vivir en un
  GameObject **aparte**, hermano del que agrupa la barra + los 4 tickets — nunca adentro de
  ese grupo ni siendo el padre directo de él. Si el script quedara dentro del objeto que se
  apaga, se apagaría a sí mismo y su propio `Update()` dejaría de correr, sin nadie que lo
  vuelva a prender.
  ```
  Canvas
  ├── OrderBoardLogic        ← tiene el componente OrderBoardUI, siempre activo
  │                             Visual Root → OrderBoardVisuals (de abajo)
  └── OrderBoardVisuals      ← este es el que Update() prende/apaga
      ├── BarraDePedidos (Image)
      ├── Ticket1..4 (Image + OrderTicketUI)
  ```
- **`ClientChair` ganó `Is Sittable`** (bool, default true) — sillas no sentables (ej. dadas
  vuelta sobre una mesa) no dejan interactuar (`Interact()` corta al toque) ni muestran nunca
  el indicador de libre. Expuesto `SetSittable(bool)` para que el futuro manejador de días
  pueda cambiarlo en runtime. Sobre volverla prefab: el script ya es prefab-safe tal cual está
  (todas las referencias — `sitPoint`, `moneySpawnPoint`, `freeIndicatorPoint` — son hijos
  propios de la silla, nada apunta afuera), así que es una acción pura de editor: agarrar una
  silla ya armada y arrastrarla a una carpeta de `Assets` para crear el prefab, sin tocar nada
  de código.

### 2026-09-12 (cont. 3)
- **Fase 5 confirmada funcionando en Unity** (persecución, grito, extintor) y los dos ajustes
  de esta sesión (offset del extintor, escala del indicador) probados y andando ("anda joya").
- **Fase 5.5 (interacción con matafuegos en mano):** unificado con `IsBeingChased()` — ver
  entrada anterior.
- **Fase 6 hecha (código):** `Orders/OrderTicket.cs`, `OrderBoard.cs`, `OrderTicketUI.cs`,
  `OrderBoardUI.cs`. `ClientChair` ganó `Table Number`, `SitState` abre/cierra el ticket en
  `OrderRoutine()`/`Exit()`. Checklist de Unity en 7.14.

### 2026-09-12 (cont. 2)
- **Con el matafuegos en la mano se podía interactuar con cualquier otra cosa** (sillas,
  estaciones, etc.), algo que no tiene sentido mientras lo tenés equipado. En vez de sumar un
  caso aparte, se unificó con la restricción que ya existía para "me persigue el Yakuza"
  (ambas situaciones necesitan exactamente lo mismo: solo se puede interactuar con
  `ExtinguisherProp`, todo lo demás bloqueado). `PlayerInteraction` ahora cachea el
  `PlayerHoldSystem` del jugador en `Awake()` y arma un solo flag,
  `restrictToExtinguisherOnly = IsBeingChased() || IsHoldingExtinguisher()`, que reemplaza al
  viejo `isChased`. `IsHoldingExtinguisher()` detecta el ítem agarrado buscando el componente
  `ExtinguisherItem` (el que ya usa el prefab del matafuegos para el F de rociar), sin
  necesidad de ningún marcador nuevo.

### 2026-09-12 (cont.)
- **Bug del indicador de silla libre y del visual de pedido:** `ClientChair.ShowFreeIndicator()`
  y `ClientOrderDisplay.Show()` normalizaban la escala dividiendo `1f / parentScale`, lo cual
  tira a la basura la escala propia del prefab (por eso el círculo aplastado de la silla libre
  dejó de verse aplastado al instanciarse dentro de una silla con escala 100). Fix: usar
  `prefab.transform.localScale / parentScale` en vez de `1f / parentScale`, así se cancela la
  escala del padre pero se conserva la forma que tiene el prefab por sí solo.
- **Offset del extintor al agarrarlo:** `PlayerHoldSystem.HoldItem()`/`HoldExistingItem()`
  forzaban `localPosition = zero` y `localRotation = identity` para cualquier ítem agarrado,
  sin posibilidad de ajuste por ítem — Enzo lo había parcheado a mano moviendo la malla adentro
  del prefab del extintor, lo cual lo dejaba muy arriba/lejos del Yakuza. Como el extintor no
  usa `HoldableItem` (eso es solo para comida entregable, con `itemType`), se creó un
  componente nuevo y separado, `Items/HeldItemOffset.cs` (`positionOffset`/`rotationOffset`),
  que cualquier prefab agarrable puede sumar opcionalmente. `PlayerHoldSystem` lo busca y lo
  aplica si existe (cae a zero/identity si no está). Pendiente en Unity: revertir el
  movimiento manual de la malla dentro de `extinguisherHoldPrefab`, agregarle el componente
  `Held Item Offset`, y ajustar `Position Offset` / `Rotation Offset` a ojo en Play.

### 2026-09-12
- **Fase 5 hecha (código):** `Types/YakuzaClient.cs` y `States/BlownAwayState.cs` en
  `Assets/Scripts/Clients/`. `AngryChaseState` ya estaba escrito desde la fase 2, solo le
  faltaba un cliente real que lo dispare.
- `ClientBase` ganó `GetBlownAway(Vector3 force)`. Tocados (fuera del módulo nuevo, con
  cuidado de no romper el camino viejo): `ExtinguisherFoamCollision.cs` (ahora reconoce
  también `ClientBase.IsChasing`, además del `RestaurantClient` viejo) y
  `PlayerInteraction.IsBeingChased()` (ahora suma `ClientRegistry.AnyChasing`).
- Checklist de Unity para el asset del Yakuza y el prefab, en 7.13.

### 2026-09-11 (cont.)
- **Fase 4 hecha (código):** `ClientSpawner`, `ClientQueue`, `ClientRegistry`,
  `ClientSpawnEntry`, en `Assets/Scripts/Clients/Spawning/` y `Data/`. `ClientSpawner` ya
  soporta `SetAvailableEntries()` como se diseñó en 8.3, con una lista fija en el Inspector
  como default hasta que exista un manejador de días que la llame.
- Ajustes en código existente: `QueueState.SetQueuePosition()` (reacomodar la fila sin
  resetear el timer de paciencia — ver detalle en 7.12), y `ClientBase` ganó `IsChasing`
  (para `ClientRegistry.AnyChasing`, todavía sin usuario real hasta que exista Yakuza) y el
  evento `Served`.
- Checklist de Unity para armar el spawner y la cola de prueba, en 7.12.

### 2026-09-11
- **Enzo trajo la decisión final sobre escenas:** vuelta a "una sola escena persistente para
  todo el juego" (sección 8 reescrita). Sin costo — las fases 1-3 de clientes no dependían de
  esto. Único impacto real: la fase 4 (`ClientSpawner`, todavía sin escribir) va a necesitar
  un método `SetAvailableEntries(ClientSpawnEntry[])` para poder cambiar qué clientes puede
  spawnear en caliente, sin recargar escena — diseño anotado en 8.3, no implementado todavía.
- El tema `LevelManager`/panel de score/`DayDefinition` queda explícitamente pospuesto, por
  pedido de Enzo — se retoma después de terminar clientes.
- Verificado el `git status`: los borrados masivos que aparecían son reorganización propia de
  Enzo entre sesiones (renombró carpetas para que coincidan con la convención en inglés:
  `AngryCustomer`→`AngryClient`, `Barra feli`→`HappinessBar`, etc.), no pérdida de datos. De
  paso ya armó `FreeChair.prefab` (el prefab que necesitaba `ClientChair.freeIndicatorPrefab`)
  y una carpeta `Tickets` con el arte de la UI de pedidos. Nada de esto está commiteado.

### 2026-09-10 (cont. 3)
- **🎉 Fase 3 confirmada funcionando end-to-end en Unity.** Ciclo completo probado: cola →
  seguir al jugador → sentarse → "pensando" → mostrar plato → cocinar y entregar → plata →
  salir por el Exit → destruirse. Cierre de sesión por hoy.
- **Próximo paso: fase 4** — `ClientSpawner`, `ClientQueue`, `ClientRegistry`. El prefab de
  prueba (`NormalClient` + `ClientChair`) ya queda armado y sirve de base real para eso; el
  botón de debug deja de ser necesario una vez que el spawner llame `Initialize`/`EnterQueue`
  de verdad, pero no hace falta borrarlo.

### 2026-09-10 (cont. 2)
- **Bug encontrado probando:** la silla de prueba tiene escala 100 en los 3 ejes, así que el
  `Free Indicator` instanciado como hijo heredaba esa escala y aparecía gigante. Arreglado en
  `ClientChair.ShowFreeIndicator()` contrarrestando la escala del padre (`1 / lossyScale`) al
  instanciar, sin tocar el modelo de la silla. Aplicado el mismo arreglo preventivo en
  `ClientOrderDisplay.Show()`, que instancia con el mismo patrón (hijo de un punto) y tendría
  el mismo problema si algún prefab de cliente futuro trae una escala rara.
- Player de la escena de prueba resuelto: le faltaba tildar la layer `Client` en
  `Interactable Layer` de su `PlayerInteraction` — como es una escena nueva, el Player no
  tenía la configuración que ya traen Level 1/2. **Va a pasar de nuevo en cada escena nueva
  que se arme** (cada día es su propia escena, sección 8) — tenerlo presente.

### 2026-09-10 (cont.)
- **Detectado sobre la marcha:** el `Chair.cs` viejo busca clientes con
  `FindObjectsByType<RestaurantClient>()` — la clase vieja. No es compatible con
  `ClientBase`/`NormalClient` tal cual está. No se puede reusar sin reescribirlo.
- Creado [`ClientChair.cs`](Assets/Scripts/Clients/ClientChair.cs) — versión simple, en
  namespace, pensada para crecer. Actualizados `ClientBase` (tipo de `CurrentChair`, nueva
  property `IsFollowingPlayer`, nuevo método `SitOnChair(ClientChair)`), `SitState`,
  `LeaveState` y `FollowPlayerState` para usarla.
- **Simplificaciones a propósito, avisadas a Enzo:** no incluye `cookIndicator` (el viejo
  tenía dos indicadores separados, este solo uno); si la silla está ocupada, `Interact()` no
  hace nada (el viejo reenviaba el `Interact()` al cliente sentado, para el caso de entregar
  comida apuntando a la silla en vez de al cliente). Se agranda si hace falta en la práctica.
- **Revisado (2026-09-10):** se **sacó `orderIndicator` de `ClientChair` por completo** — es
  información redundante (ya se ve arriba de la cabeza del cliente, y se va a ver en la barra
  de pedidos de la fase 6; no hacía falta un tercer lugar mostrando lo mismo). Se sacaron
  también las llamadas correspondientes en `SitState`, `LeaveState` y `AngryChaseState`.
  (Ver B6: la silla vieja sí tenía este indicador, y encima mostraba el plato equivocado.)
- **`freeIndicator` pasó de objeto fijo a prefab + posición**, mismo patrón que ya usa
  `ClientOrderDisplay`: en vez de tener un indicador de "silla libre" pre-puesto abajo de
  cada silla, cada `ClientChair` tiene un `Free Indicator Point` (transform, dónde) y se
  instancia/destruye un `Free Indicator Prefab` (compartido entre todas las sillas) ahí.
  Menos objetos para mantener a mano por silla.
- **Resuelto:** `PlayerInteraction.cs` es de Enzo, así que se arregló directamente — ahora
  `interactable is RestaurantClient || interactable is ClientBase` en la prioridad de
  interacción. No se tocó `IsBeingChased()` (la otra mención a `RestaurantClient` en el mismo
  archivo) porque todavía no hay forma de que un cliente nuevo entre en `AngryChaseState`
  (eso es `YakuzaClient`, fase 5) — se agrega ahí cuando haya algo real que probar.

### 2026-09-10
- Antes de escribir código, verifiqué el estado actual: `Normal.asset` seguía con
  `possibleMeals`/`carryOverRules` vacíos (pendiente de Enzo), y `ClientBase.cs` sin cambios
  desde la fase 2.
- **Detectado un gap real:** la fase 3 no iba a ser probable en Unity como decía la tabla
  original, porque nada llama `EnterQueue()` sin el spawner de la fase 4. Solución: se agregó
  un botón de debug (`[ContextMenu]`) en `ClientBase` para poder probar el ciclo completo sin
  esperar a la fase 4.
- **Fase 3 hecha (código):** creado `Types/NormalClient.cs` (subclase mínima — el patience
  expirado lo manda a `LeaveState`). Checklist de Unity completo en la sección 7.11, incluye
  reusar el `Client.prefab` viejo duplicado en vez de armar uno desde cero.

### 2026-09-09 (cont. 2)
- **Revisada la sección 8:** cada día tiene su propio Level Design → cada día es una escena
  distinta, no un solo nivel persistente con paneles. Se sacaron `DayDefinition` y
  `DayManager` del diseño (nunca se habían programado, así que no hubo que deshacer código).
  Las fases 1 y 2 del refactor de clientes no se ven afectadas por este cambio — la inyección
  de config al spawnear seguía siendo necesaria igual. Quedó anotado que `MoneyManager` no
  tiene `DontDestroyOnLoad` (se va a necesitar cuando el dinero deba acumularse entre días,
  fase 7, no ahora).

### 2026-09-09 (cont.)
- **Fase 2 hecha (código).** Verificados los 2 assets de la fase 1: `WokRice.asset` perfecto
  (arte del ticket ya enganchado, adelantado); `Normal.asset` con `possibleMeals` y
  `carryOverRules` vacíos — pendiente que Enzo los complete.
- Creados en `Assets/Scripts/Clients/`: `States/ClientState.cs`, `States/ClientStateMachine.cs`,
  `States/QueueState.cs`, `States/FollowPlayerState.cs`, `States/SitState.cs`,
  `States/LeaveState.cs`, `States/AngryChaseState.cs`, `Components/ClientPatience.cs`,
  `Components/ClientOrderDisplay.cs`, `Types/ClientBase.cs`.
- **Se adelantó `ClientPatience` desde la fase 3** — los estados necesitaban compilar contra
  algo real, no tenía sentido dejarlo en referencia a una clase que no existe. La fase 3 queda
  reducida a escribir `NormalClient` (subclase chica) + el wiring en Unity.
- **`ClientOrderDisplay` se separó de `ClientBase`** (estaba en el plan original 7.2, folder
  `Components/`) — el visual flotante del pedido es su propio componente, no un método más en
  la clase que ya hace de más (evita repetir la deuda D6 del código viejo en el nuevo).
- **Resuelto D2 en el nuevo código:** un solo mecanismo de salida — `LeaveState.Tick()` mide
  distancia al `Exit` y se autodestruye. No hay `OnTriggerExit` duplicado ni un `ExitTrigger`
  aparte como en `RestaurantClient.cs` viejo.
- Puntos de extensión dejados para la fase 4 (todavía no la usa nadie, pero ya están):
  `ClientBase.IsFrontOfQueue` (bool, default `true`), evento `LeftQueue`, evento `Despawned`,
  `ClientBase.Initialize(config)` (inyecta la config, no va fija en el prefab — sección 8),
  `ClientBase.EnterQueue(posición)`.
- **B2 (recompensa por plato) todavía NO se toca** — sigue planificado para la fase 7 como
  ya estaba, no se adelantó.

### 2026-09-09
- **Revisión de `ClientTypeDefinition`:** se sacó el bool `picksRandomMeal` — `ChooseMeal()`
  ahora siempre elige al azar (código ya editado en `ClientTypeDefinition.cs`).
- **Definido el sistema de progresión por días (sección 8), reemplaza el enfoque anterior de
  "Level 1 / Level 2" como workaround.** Un solo Level Design persistente; los días
  desbloquean estaciones, tipos de cliente y platos. Decidido: misma escena + paneles para
  el resumen de fin de día (no escenas separadas) — más profesional y más simple a la vez.
  Piezas nuevas: `DayDefinition` (SO) y `DayManager` (clase), conviven con el `LevelManager`
  viejo. Confirma que `ClientTypeDefinition` se inyecta al spawnear, no va fija en el prefab.
- Ajustada la barra de pedidos (sección 7.6): números fijos arriba del arte, tickets debajo
  sin superponerse — no hace falta repetir el número de mesa en el ticket. La barra de
  paciencia se sacó de la UI de pedidos: sigue siendo el Canvas World Space de siempre
  arriba de la cabeza del cliente (sección 7.5), son dos sistemas separados.
- Confirmado con la artista: el arte de cada plato (cartel completo) y de la barra de
  paciencia ya viene terminado, no se compone por código.

### 2026-09-08
- **B4 arreglado:** agregada la guarda de singleton en `SoundManager.Awake()` (mismo patrón
  que `MusicManager`). Ya no se acumulan instancias entre escenas.
- **Fase 1 hecha (código):** creados `Assets/Scripts/Clients/Data/MealDefinition.cs`,
  `ClientTypeDefinition.cs` y `PatienceCarryOver.cs`, todo bajo el namespace
  `RestaurantChaos.Clients`. `ClientTypeDefinition` incluye `ChooseMeal()` y
  `GetSeatedStartFill()` con la lógica de arrastre acordada en 7.5.
- **Pendiente de Enzo:** crear los assets en Unity — checklist en la sección 7.10.

### 2026-09-07
- Auditado el sistema de audio: encontrados 2 bugs (B4 el crítico: falta guarda de singleton
  en `SoundManager`) y 4 items de deuda (D9–D12). Se decide **no** desviarse ahora al refactor
  de audio; solo se arregla B4 antes de empezar con clientes.
- Decidido: el **timer B de paciencia arranca al revelarse el pedido**, no al sentarse.
- **Diseñado el refactor completo del sistema de clientes (ver sección 7).** Decisiones:
  FSM de clases, dos ScriptableObjects (`MealDefinition` y `ClientTypeDefinition`), paciencia
  de dos fases con regla de arrastre, sistema de pedidos con UI de mesas, y `ClientRegistry`
  por eventos para matar los `FindObjectsByType`. Implementación en paralelo al código viejo,
  bajo el namespace `RestaurantChaos.Clients`.
- Creada la carpeta `Assets/Scripts/Clients` (todavía vacía) para reordenar el módulo de clientes.
- Relevamiento completo del sistema de clientes: documentada la máquina de estados, el flujo
  spawn → cola → sentarse → pedido → entrega → salida, y detectados los bugs B1–B3 y la
  deuda técnica D1–D8.
- Creado este archivo de notas.

### Commits previos (referencia)
```
e938fb7  Yakuza, feedback, extinguisher, and some other things
0fcc693  level+shaders
4e96c88  recomodamiento
c9ece51  obj
a0afee0  lugar
```
