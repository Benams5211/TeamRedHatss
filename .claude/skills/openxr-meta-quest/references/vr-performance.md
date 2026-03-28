# VR Performance and Application SpaceWarp

## Frame Rate Targets

| Device | Display Refresh Rate | Frame Budget |
|--------|---------------------|--------------|
| Quest 2 | 72/90/120 Hz | 13.8 / 11.1 / 8.3 ms |
| Quest 3 | 72/90/120 Hz | 13.8 / 11.1 / 8.3 ms |
| Quest 3S | 72/90/120 Hz | 13.8 / 11.1 / 8.3 ms |
| Quest Pro | 72/90 Hz | 13.8 / 11.1 ms |

Missing frames causes judder and motion sickness. Target 90 Hz (11.1 ms) as the baseline.

## Application SpaceWarp (ASW)

Meta's frame extrapolation technology. Generates intermediate frames from depth and motion vectors, effectively halving your GPU workload by rendering at half the display refresh rate while maintaining perceived smoothness.

### How It Works
Each rendered frame produces motion vectors with three channels:
- R: horizontal movement (screen space)
- G: vertical movement (screen space)
- B: depth change (forward/backward)

The OpenXR runtime uses these to extrapolate the next frame.

### When to Use
- Your app struggles to hit 90 Hz consistently.
- You have heavy GPU-bound scenes.
- The visual quality tradeoff (occasional artifacts on fast-moving objects) is acceptable.

### Enabling SpaceWarp
Enable via the Meta Quest Display Utilities feature in the OpenXR feature group. Then in code:

```csharp
using UnityEngine;
using UnityEngine.XR;

namespace MyGame.VR
{
    public class SpaceWarpController : MonoBehaviour
    {
        [SerializeField] private bool _enableSpaceWarp = true;

        private void Start()
        {
            // Request half-rate rendering with SpaceWarp extrapolation
            if (_enableSpaceWarp)
            {
                // Set target frame rate to half of display refresh rate
                // The runtime will extrapolate the missing frames
                XRDisplaySubsystem display = null;
                var displays = new System.Collections.Generic.List<XRDisplaySubsystem>();
                SubsystemManager.GetSubsystems(displays);
                if (displays.Count > 0)
                    display = displays[0];
            }
        }
    }
}
```

## Symmetric Projection

Optimizes multiview rendering by using symmetric, aligned eye buffers instead of asymmetric frustums. Reduces GPU overhead for stereo rendering.

Enable in OpenXR > Meta Quest Support settings.

## General VR Optimization Checklist

### Rendering
- Use **Forward** rendering path (not Deferred).
- **MSAA 4x** instead of post-process anti-aliasing.
- Disable **HDR** on the URP Asset.
- Disable or minimize **post-processing** (bloom, DOF, SSAO are expensive).
- Use **baked lighting** where possible; limit real-time lights to 1-2.
- Shadows: single cascade, short distance (10-15m), or bake them.
- Texture compression: **ASTC** for Android.
- Use **GPU Instancing** for repeated meshes.
- Use **Static Batching** for non-moving geometry.
- LODs on all 3D models (3-4 levels).

### Scripting
- Avoid allocations in Update (no LINQ, no string concat, no GetComponent).
- Pool frequently instantiated objects (projectiles, effects).
- Use Jobs + Burst for heavy computation.
- Profile with the Unity Profiler connected to the Quest (Development Build + Autoconnect Profiler).

### Draw Calls
- Quest 2 budget: ~100-150 draw calls.
- Quest 3 budget: ~200-300 draw calls.
- Use texture atlases.
- Merge static meshes where possible.
- Minimize material count.

### Memory
- Quest 2: ~1.5 GB available for apps.
- Quest 3: ~2.5 GB available for apps.
- Compress textures aggressively.
- Use Addressables for on-demand asset loading.
- Unload unused assets between scenes.

## Profiling on Quest

1. Enable Development Build in Build Settings.
2. Check Autoconnect Profiler.
3. Build and Run.
4. Open Window > Analysis > Profiler in the Editor.
5. The profiler connects to the Quest automatically.

Key metrics to watch:
- CPU frame time (must stay under frame budget).
- GPU frame time (check via OVR Metrics Tool or Meta Quest Developer Hub).
- GC allocations per frame (target zero in steady state).
- Draw call count.
- Triangle count.
