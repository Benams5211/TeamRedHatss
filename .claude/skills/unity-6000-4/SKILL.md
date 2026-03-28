---
name: unity-6000-4
description: >
  C# scripting and project configuration skill for Unity 6000.4.0f1 (Unity 6.4 branch).
  Use this skill whenever the user mentions Unity 6000.4, Unity 6.4, or is working in a
  project targeting this version. Also trigger when the user writes, debugs, or reviews
  C# code for Unity and the project version is 6000.4.x -- even if they reference
  GameObjects, transforms, prefabs, MonoBehaviour, ScriptableObject, or any Unity API
  without explicitly naming the version. Covers lifecycle callbacks, physics (linearVelocity),
  Awaitable async, Input System, URP, HDRP, Assembly Definitions, Render Graph, and
  version-specific changes from the 6000.3.x LTS branch. When in doubt, use this skill --
  it is better to have the guidance and not need it than to miss version-specific pitfalls.
---

# Unity 6000.4.0f1 Scripting Skill

Target version: **Unity 6000.4.0f1** (Unity 6.4 branch).
C# language version: **C# 9** (default; some C# 10/11 features available depending on compiler settings).

## What Changed from 6000.3.x LTS

Unity 6000.4.0f1 is a polishing release on top of 6000.3.x LTS. Key differences:

- **Render Graph optimization**: 2-4% main-thread CPU gain during the recording step of the Scriptable Render Pipeline.
- **CAMetalDisplayLink on macOS**: Frame-rate synchronization with display refresh rate. Reduces stuttering, stabilizes `Time.deltaTime`.
- **LOD visualization**: Mesh Renderer and Skinned Mesh Renderer components now have an Inspector slider to scrub through LOD levels visually.
- **Scene View grid transform**: Custom position and rotation for the Scene View grid.
- **visionOS 3.1.0**: `com.unity.polyspatial.visionos` updated to 3.1.0.
- **Deep linking on iOS**: `Application.deepLinkActivated` now fires correctly on cold start and when the app is already running.
- **Editor release builds**: Release builds of the Editor engine assemblies now compile C# in Release mode. Debug editor builds still use Debug. Player engine assemblies always compile in Release.
- **Artifact dependency refinement**: Changed artifact dependency from `ImportResultID` to `ImportResultOutputID`, reducing unnecessary reimports. This triggers a one-time reimport of all assets on first open.
- **Physics fixes**: Rigidbody components on prefabs no longer create internal physics objects unintentionally. Stack overflow on MeshCollider raycast with Fast Midphase fixed.
- **UnityEvent memory leak**: Fixed in player builds.

Everything from `references/unity-csharp-core.md` still applies (MonoBehaviour lifecycle, caching, SerializeField, Awaitable, Input System, naming conventions, null safety).

## When to Read Reference Files

| Topic | File | When |
|-------|------|------|
| Core C# patterns | `references/unity-csharp-core.md` | Any Unity C# code |
| Version-specific notes | `references/version-delta.md` | Migration from 6000.3.x or earlier |
| Project setup checklist | `references/project-setup.md` | New project creation, build settings |

---

## Core Principles (Quick Reference)

These are condensed from the full reference. Read `references/unity-csharp-core.md` for details.

### MonoBehaviour Lifecycle

`Awake` -> `OnEnable` -> `Start` -> `FixedUpdate` -> `Update` -> `LateUpdate` -> `OnDisable` -> `OnDestroy`

- `Awake`: self-initialization (cache own components).
- `Start`: cross-object initialization.
- `OnEnable`/`OnDisable`: event subscribe/unsubscribe.

### Cache Everything in Hot Paths

```csharp
// Cache in Awake, never allocate in Update
Rigidbody _rb;
void Awake() => _rb = GetComponent<Rigidbody>();
```

Avoid in Update/FixedUpdate: `GetComponent`, `Find*`, `SendMessage`, LINQ, string concat, `foreach` on non-generic collections.

### Physics (Unity 6)

- `Rigidbody.velocity` is renamed to `Rigidbody.linearVelocity`. Use `linearVelocity` in all new code.
- `Rigidbody2D.linearVelocity` for 2D.
- `angularVelocity` is unchanged.
- Physics movement belongs in `FixedUpdate`.

### Async: Awaitable Over Coroutines

```csharp
async Awaitable WaitAndFireAsync()
{
    await Awaitable.WaitForSecondsAsync(1f);
    Fire();
}
```

### Input System

Unity 6 uses the new Input System by default. Do NOT use `Input.GetKey()` / `Input.GetAxis()` in new code.

### Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Public property | PascalCase | `public float MoveSpeed` |
| Private field | _camelCase | `private float _moveSpeed` |
| Local / param | camelCase | `float speed = 5f` |
| Method | PascalCase | `void ApplyDamage()` |
| Interface | IPascalCase | `IDamageable` |
| Namespace | PascalCase, match folders | `namespace MyGame.Combat` |

### SerializeField, Not Public Fields

```csharp
[SerializeField] private float _moveSpeed = 5f;
```

### Null Safety

```csharp
// Unity objects: use != null
if (target != null) { /* safe */ }

// Pure C# objects: use is not null
if (data is not null) { /* safe */ }

// TryGetComponent avoids exceptions
if (TryGetComponent<Rigidbody>(out var rb))
    rb.AddForce(Vector3.up);
```

### Assembly Definitions

Use `.asmdef` files for any non-prototype project:

```
Scripts/
  MyGame.Core.asmdef
  MyGame.Gameplay.asmdef
  MyGame.UI.asmdef
  MyGame.Editor.asmdef
  MyGame.Tests.asmdef
```

---

## Output Guidelines

1. Always include `using` directives.
2. Wrap in a namespace matching the folder path.
3. Use `[Header]` and `[Tooltip]` on serialized fields.
4. Add XML doc comments on public members.
5. Use block-scoped namespaces (C# 9 default) unless the user explicitly uses file-scoped.
6. Generate complete, compilable scripts.
7. Save `.cs` files to `/mnt/user-data/outputs/`.
