# Unity C# Core Patterns (Unity 6000.4.0f1)

## Table of Contents
1. MonoBehaviour Lifecycle
2. Composition Over Inheritance
3. Caching and Allocation
4. SerializeField Best Practices
5. Awaitable Async Patterns
6. Input System Basics
7. Physics Notes
8. Rendering Pipeline Awareness
9. Common Quick Fixes
10. Script Templates

---

## 1. MonoBehaviour Lifecycle

Execution order: `Awake` -> `OnEnable` -> `Start` -> `FixedUpdate` -> `Update` -> `LateUpdate` -> `OnDisable` -> `OnDestroy`

Rules:
- `Awake`: self-init only (cache own components via `GetComponent`).
- `Start`: cross-object init (references that might not exist in `Awake`).
- `OnEnable`/`OnDisable`: event subscription/unsubscription. Always pair them.
- Never assume execution order between scripts unless set via `[DefaultExecutionOrder(n)]`.

---

## 2. Composition Over Inheritance

Keep MonoBehaviours small and single-purpose:
- **ScriptableObjects** for shared data, config, game events.
- **Plain C# classes** for pure logic with no Unity lifecycle dependency.
- **Interfaces** for polymorphism without deep inheritance chains.

---

## 3. Caching and Allocation

```csharp
// BAD
void Update()
{
    var rb = GetComponent<Rigidbody>();
    var enemies = GameObject.FindGameObjectsWithTag("Enemy");
}

// GOOD
Rigidbody _rb;
void Awake() => _rb = GetComponent<Rigidbody>();
```

Never call in Update/FixedUpdate:
- `GetComponent<T>()`, `Find*()`, `SendMessage()`
- String concatenation (use `StringBuilder` for repeated ops)
- LINQ (heap allocations)
- `foreach` on non-generic collections (boxing)

---

## 4. SerializeField Best Practices

```csharp
[Header("Movement")]
[SerializeField] private float _moveSpeed = 5f;
[Tooltip("Target to follow")]
[SerializeField] private Transform _target;

// Public property with private setter for external read access
public int Health { get; private set; }
```

Use `[Range(min, max)]` for float/int sliders in the Inspector.
Use `[TextArea]` for multiline string fields.

---

## 5. Awaitable Async Patterns

Unity 6 has first-class `Awaitable` support. Prefer over coroutines for new code.

```csharp
// Basic delay
async Awaitable WaitAndFireAsync()
{
    await Awaitable.WaitForSecondsAsync(1f);
    Fire();
}

// Cancellation via destroyCancellationToken
async Awaitable FadeOutAsync()
{
    float t = 1f;
    while (t > 0f)
    {
        t -= Time.deltaTime;
        _renderer.material.SetFloat("_Alpha", t);
        await Awaitable.NextFrameAsync(destroyCancellationToken);
    }
}

// Background thread work
async Awaitable<int[]> ProcessDataAsync()
{
    await Awaitable.BackgroundThreadAsync();
    int[] result = HeavyComputation();
    await Awaitable.MainThreadAsync();
    return result;
}
```

Key points:
- Return `Awaitable` (void-like) or `Awaitable<T>` (value-returning).
- Use `destroyCancellationToken` to auto-cancel when the MonoBehaviour is destroyed.
- Use `Awaitable.BackgroundThreadAsync()` to move off the main thread, then `MainThreadAsync()` to return.

---

## 6. Input System Basics

Unity 6 ships with the new Input System by default. Never use legacy `Input.GetKey()` / `Input.GetAxis()`.

Recommended pattern with Input Actions:

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyGame.Gameplay
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _jumpAction;

        private void OnEnable()
        {
            _moveAction.action.Enable();
            _jumpAction.action.Enable();
            _jumpAction.action.performed += OnJump;
        }

        private void OnDisable()
        {
            _jumpAction.action.performed -= OnJump;
            _moveAction.action.Disable();
            _jumpAction.action.Disable();
        }

        private void Update()
        {
            Vector2 move = _moveAction.action.ReadValue<Vector2>();
            // Pass to movement system
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            // Handle jump
        }
    }
}
```

For VR: use XR Interaction Toolkit input actions mapped to controller buttons/axes.

---

## 7. Physics Notes (Unity 6)

- `Rigidbody.velocity` is renamed `Rigidbody.linearVelocity`.
- `Rigidbody2D.linearVelocity` for 2D.
- `angularVelocity` unchanged.
- Always do physics movement in `FixedUpdate`.
- Use `Physics.Simulate` / `Physics2D.Simulate` for deterministic testing.
- In 6000.4.0f1: Rigidbody on prefabs no longer creates internal physics objects unintentionally.
- MeshCollider with Fast Midphase no longer causes stack overflow on raycast.

---

## 8. Rendering Pipeline Awareness

Unity 6 projects typically use URP or HDRP.

- Shader properties differ: `_BaseColor` (URP) vs `_Color` (Built-in).
- Cache property IDs: `Shader.PropertyToID("_BaseColor")`.
- Use `MaterialPropertyBlock` to avoid material instancing overhead.
- In 6000.4: Render Graph has 2-4% CPU improvement on recording step.

---

## 9. Common Quick Fixes

| Symptom | Cause | Fix |
|---------|-------|-----|
| NullReferenceException on GetComponent | Component not attached | Use `TryGetComponent`, check Inspector |
| Script does not run | Class name != file name | Rename to match |
| Play Mode changes lost | Unity resets on exit | Use ScriptableObjects or serialize to file |
| Physics jitter | Moving Rigidbody in Update | Use FixedUpdate + linearVelocity |
| Input not responding | Legacy Input code | Migrate to Input System |
| Serialized field null | Missing [SerializeField] or static field | Add attribute, remove static |
| Slow builds | No Assembly Definitions | Add .asmdef files |

---

## 10. Script Templates

### Basic MonoBehaviour

```csharp
using UnityEngine;

namespace MyGame.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpForce = 8f;

        [Header("References")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private LayerMask _groundLayer;

        private Rigidbody _rb;
        private bool _isGrounded;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            _isGrounded = Physics.CheckSphere(
                _groundCheck.position, 0.2f, _groundLayer);
        }

        public void Move(Vector2 input)
        {
            var movement = new Vector3(input.x, 0f, input.y) * _moveSpeed;
            _rb.linearVelocity = new Vector3(
                movement.x, _rb.linearVelocity.y, movement.z);
        }

        public void Jump()
        {
            if (_isGrounded)
                _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }
}
```

### ScriptableObject Data Container

```csharp
using UnityEngine;

namespace MyGame.Data
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "Game/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Stats")]
        public string WeaponName;
        public int Damage = 10;
        public float AttackSpeed = 1f;
        public float Range = 2f;

        [Header("Visual")]
        public Sprite Icon;
        public GameObject Prefab;
    }
}
```

### ScriptableObject Event Channel

```csharp
using System;
using UnityEngine;

namespace MyGame.Events
{
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "Game/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        private Action _listeners;

        public void Raise() => _listeners?.Invoke();
        public void Register(Action listener) => _listeners += listener;
        public void Unregister(Action listener) => _listeners -= listener;
    }
}
```

### Singleton (Use Sparingly)

```csharp
using UnityEngine;

namespace MyGame.Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = (T)(MonoBehaviour)this;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
```
