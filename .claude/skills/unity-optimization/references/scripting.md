# Scripting Optimization

## Zero-Allocation Hot Paths

The single most important scripting optimization for VR: **zero managed allocations in Update, FixedUpdate, and LateUpdate**. Every allocation eventually triggers a garbage collection pause, which causes a visible frame drop.

### Common Allocation Sources and Fixes

| Source | Fix |
|--------|-----|
| `GetComponent<T>()` per frame | Cache in Awake |
| `Find*()` per frame | Cache reference, use events |
| `string + string` | Use `StringBuilder` or avoid |
| LINQ (`.Where()`, `.Select()`) | Use `for` loops |
| `foreach` on non-generic IEnumerable | Use `for` with index |
| `new List<T>()` per frame | Pre-allocate, reuse |
| Lambda/closure allocation | Cache delegate, avoid closures |
| Boxing (value type -> object) | Use generic collections |
| `ToString()` on value types | Cache or avoid in hot path |
| `Instantiate()` / `Destroy()` | Object pooling |

### Pre-allocate Collections

```csharp
// BAD: allocates a new list every frame
void Update()
{
    var nearby = new List<Enemy>();
    // ...
}

// GOOD: reuse a pre-allocated list
private readonly List<Enemy> _nearbyCache = new(32);

void Update()
{
    _nearbyCache.Clear();
    // ... fill _nearbyCache
}
```

### Cache Delegates

```csharp
// BAD: allocates a new delegate each call
_enemies.Sort((a, b) => a.Distance.CompareTo(b.Distance));

// GOOD: cache the comparison
private static readonly Comparison<Enemy> _distanceCompare =
    (a, b) => a.Distance.CompareTo(b.Distance);

_enemies.Sort(_distanceCompare);
```

## Object Pooling

Never Instantiate/Destroy during gameplay. Pool everything:
- Projectiles, particles, hit effects.
- Enemy spawns (recycle dead enemies).
- UI elements (damage numbers, score popups).
- Audio sources for one-shot sounds.

Use `UnityEngine.Pool.ObjectPool<T>` (built-in) or write a custom pool (see main SKILL.md).

## Update Frequency Reduction

Not every script needs to run every frame.

```csharp
// Run expensive logic every N frames
private int _frameCounter;

void Update()
{
    if (++_frameCounter % 5 != 0) return;
    ExpensiveEnemyAI();
}
```

Or use a timer:

```csharp
private float _timer;
private const float Interval = 0.2f; // 5 times per second

void Update()
{
    _timer += Time.deltaTime;
    if (_timer < Interval) return;
    _timer -= Interval;
    ExpensiveLogic();
}
```

## Jobs and Burst

For CPU-heavy work that cannot be reduced or skipped, offload to worker threads using the Jobs system with Burst compilation.

### When to Use Jobs
- Processing > 1000 items per frame (distance checks, visibility, sorting).
- Procedural generation (mesh, terrain, noise).
- Custom physics or spatial queries.
- Audio DSP processing.

### Pattern

```csharp
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
struct ProcessNotesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float> NoteTimes;
    [ReadOnly] public float CurrentTime;
    [ReadOnly] public float HitWindow;
    public NativeArray<int> Results; // 0=pending, 1=hittable, 2=missed

    public void Execute(int i)
    {
        float delta = NoteTimes[i] - CurrentTime;
        if (delta > HitWindow) Results[i] = 0;
        else if (delta >= -HitWindow) Results[i] = 1;
        else Results[i] = 2;
    }
}
```

Schedule in Update, complete before you need results:

```csharp
void Update()
{
    var job = new ProcessNotesJob { /* ... */ };
    _handle = job.Schedule(_noteCount, 64);
}

void LateUpdate()
{
    _handle.Complete();
    // Read results
}
```

### Rules
- `NativeArray<T>` for data (disposed manually with `.Dispose()`).
- `[ReadOnly]` on data you do not write.
- `[BurstCompile]` on the job struct for native SIMD compilation.
- No managed objects in jobs (no strings, no classes, no Unity API calls).
- Inner loop batch size for IJobParallelFor: 32-128 depending on work per item.

## String Optimization

Strings are immutable in C#. Every concatenation allocates a new string.

```csharp
// BAD (3 allocations per frame)
_scoreText.text = "Score: " + score + " x" + combo;

// GOOD (zero allocation if using a cached StringBuilder)
_sb.Clear();
_sb.Append("Score: ").Append(score).Append(" x").Append(combo);
_scoreText.SetText(_sb);

// BEST for TextMeshPro (zero allocation)
_scoreText.SetText("Score: {0} x{1}", score, combo);
```

## Physics Optimization

- Use simple colliders (Box, Sphere, Capsule) over MeshColliders.
- Use Layer-based collision matrix to skip unnecessary checks (Edit > Project Settings > Physics > Layer Collision Matrix).
- Reduce FixedUpdate frequency if physics fidelity allows: 0.02 (50 Hz) instead of 0.01111 (90 Hz).
- Use `Physics.OverlapSphereNonAlloc()` instead of `Physics.OverlapSphere()` to avoid allocation.

```csharp
private readonly Collider[] _hitBuffer = new Collider[16];

void CheckHits()
{
    int count = Physics.OverlapSphereNonAlloc(
        transform.position, _radius, _hitBuffer, _hitLayer);
    for (int i = 0; i < count; i++)
    {
        // Process _hitBuffer[i]
    }
}
```
