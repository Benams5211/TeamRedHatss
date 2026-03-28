---
name: unity-optimization
description: >
  Unity performance optimization for VR and mobile targets. Use this skill whenever the
  user mentions frame rate problems, lag, stuttering, draw calls, overdraw, GC spikes,
  memory pressure, profiling, batching, LODs, texture compression, object pooling, Jobs,
  Burst, occlusion culling, or any performance issue in Unity. Also trigger when the user
  asks about Quest performance budgets, GPU/CPU frame timing, reducing build size,
  optimizing shaders, reducing material count, atlas textures, mesh combining, or Asset
  Bundle / Addressables optimization. Covers both scripting-level optimization (C# hot
  paths, allocation avoidance, pooling, Jobs + Burst) and rendering-level optimization
  (draw calls, batching, LODs, shaders, lighting). Tuned for Unity 6000.4.0f1 targeting
  Meta Quest devices, but the principles apply broadly. When in doubt, use this skill --
  performance problems are easier to prevent than to fix.
---

# Unity Performance Optimization

Target: **Unity 6000.4.0f1**, primarily targeting **Meta Quest** (mobile VR).

Performance is the difference between a playable VR experience and one that causes motion sickness. Quest devices have strict budgets. This skill covers how to stay within them.

## When to Read Reference Files

| Topic | File | When |
|-------|------|------|
| Profiling workflow | `references/profiling.md` | Finding what is slow |
| Rendering optimization | `references/rendering.md` | Draw calls, shaders, batching, LODs |
| Scripting optimization | `references/scripting.md` | GC, pooling, Jobs, Burst, hot paths |
| Memory and assets | `references/memory.md` | Build size, textures, Addressables |

---

## Quest Performance Budgets

| Metric | Quest 2 | Quest 3 / 3S |
|--------|---------|--------------|
| Frame budget (90 Hz) | 11.1 ms | 11.1 ms |
| Draw calls | 100-150 | 200-300 |
| Triangles per frame | 100K-200K | 300K-750K |
| App memory | ~1.5 GB | ~2.5 GB |
| Texture memory | ~200-300 MB | ~400-500 MB |

These are guidelines, not hard limits. Profile on-device to find actual bottlenecks.

## The Three Questions

Before optimizing anything, answer:
1. **Is it GPU-bound or CPU-bound?** Check the Profiler. Optimizing rendering when the CPU is the bottleneck wastes time.
2. **Is it a spike or sustained?** Spikes (GC, scene load) need different fixes than sustained overhead (too many draw calls).
3. **Does the user notice?** Dropped frames during a loading screen matter less than dropped frames during gameplay.

## Top 10 Quick Wins

These fixes address the most common VR performance problems:

1. **Disable post-processing** on the URP Renderer. Bloom, SSAO, DOF are too expensive for mobile VR.
2. **Use Forward rendering**, not Deferred. Deferred is too heavy for Quest GPUs.
3. **Bake lighting**. Use Light Probes and Lightmaps instead of real-time lights.
4. **Limit real-time shadows** to one directional light, single cascade, 10-15m distance.
5. **Use ASTC texture compression** and downscale textures (1024 max for most objects on Quest 2).
6. **Enable GPU Instancing** on materials for repeated meshes.
7. **Add LODs** to all 3D models (3-4 levels). Use the LOD Group component.
8. **Pool objects** instead of Instantiate/Destroy at runtime.
9. **Eliminate per-frame allocations**. No LINQ, no string concat, no `GetComponent` in Update.
10. **Use Assembly Definitions** to speed up compile times during development.

## Rendering Pipeline Settings (URP for Quest)

### URP Asset
- HDR: **off**.
- MSAA: **4x** (hardware-accelerated on Quest, nearly free).
- Render Scale: **1.0** (lower to 0.8 if GPU-bound).
- Main Light: **Per Pixel**.
- Additional Lights: **Per Vertex** or **Disabled**.
- Shadows: off or single cascade, max distance 10-15m.
- Soft Shadows: **off**.

### URP Renderer
- Rendering Path: **Forward**.
- Post-processing: **off** by default.
- Intermediate Texture: **Auto**.
- Depth Priming: **Disabled** (for multi-pass stereo).

---

## Object Pooling Pattern

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Core
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _pool = new();

        public ObjectPool(T prefab, int preWarmCount, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < preWarmCount; i++)
            {
                T obj = Object.Instantiate(_prefab, _parent);
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public T Get()
        {
            T obj = _pool.Count > 0
                ? _pool.Dequeue()
                : Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}
```

Use Unity's built-in `UnityEngine.Pool.ObjectPool<T>` for simpler cases.

## Jobs + Burst for Heavy Computation

When computation is too heavy for the main thread (path finding, physics queries, procedural generation):

```csharp
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MyGame.Core
{
    [BurstCompile]
    public struct DistanceJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> Positions;
        public NativeArray<float> Distances;
        public Vector3 Origin;

        public void Execute(int index)
        {
            Distances[index] = Vector3.Distance(Origin, Positions[index]);
        }
    }
}
```

Key rules:
- Use `NativeArray<T>` (not managed arrays) for job data.
- Mark read-only data with `[ReadOnly]`.
- Add `[BurstCompile]` for SIMD-optimized native code.
- Schedule jobs in Update, complete in LateUpdate (or next frame if latency is acceptable).
- Dispose `NativeArray` when done.

## Output Guidelines

When providing optimization advice:
1. Always identify the bottleneck first (GPU vs CPU, spike vs sustained).
2. Provide concrete code or settings changes, not vague advice.
3. Include before/after expectations (e.g., "This should reduce draw calls from ~300 to ~80").
4. Prioritize by impact -- fix the biggest bottleneck first.
5. Note any visual quality tradeoffs.
