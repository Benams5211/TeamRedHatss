# Profiling Workflow

## Tools

### Unity Profiler (Built-in)
The primary tool. Window > Analysis > Profiler.

**Setup for Quest profiling:**
1. Build Settings: check **Development Build** and **Autoconnect Profiler**.
2. Build and Run to Quest.
3. The Profiler window connects automatically.
4. If it does not connect, use the dropdown at the top of the Profiler to select the device manually.

**Key modules:**
- **CPU Usage**: Shows per-frame script execution time. Look for spikes and high-cost methods.
- **GPU Usage**: Shows rendering cost. Only available on-device (not in Editor).
- **Rendering**: Draw calls, batches, triangles, set pass calls.
- **Memory**: Managed heap size, GC allocations, native memory.
- **Physics**: Collision checks, rigidbody count, physics step time.

### Frame Debugger (Built-in)
Window > Analysis > Frame Debugger.

Shows every draw call in the current frame, in order. Use this to understand why batching is breaking or why a material is causing an extra pass.

### Meta Quest Developer Hub (MQDH)
External tool from Meta. Provides real-time GPU metrics, thermal state, clock speeds, and memory usage. Install from the Meta developer site.

### OVR Metrics Tool
Overlay that runs on-device showing frame rate, GPU/CPU time, and thermal throttling state. Enable via MQDH or install the APK directly.

## Profiling Strategy

### Step 1: Identify the Constraint
Run the game on-device with the Profiler attached. Play through a representative gameplay section (not menus).

Check:
- Is CPU frame time > 11.1 ms? -> CPU-bound.
- Is GPU frame time > 11.1 ms? -> GPU-bound.
- Are both under budget but frames still drop? -> Likely GC spikes or thermal throttling.

### Step 2: Drill Down

**If CPU-bound:**
1. Open CPU Usage module.
2. Switch to Hierarchy view.
3. Sort by **Self ms** (time spent in that method, excluding callees).
4. Look for:
   - Scripts taking > 1 ms per frame.
   - `GC.Alloc` entries (managed allocations triggering garbage collection).
   - Physics step time > 2 ms.
   - Animation evaluation > 2 ms.

**If GPU-bound:**
1. Check Rendering module: draw calls, triangles, set pass calls.
2. Open Frame Debugger to see what each draw call renders.
3. On-device: use OVR Metrics Tool to confirm GPU time.
4. Look for:
   - Overdraw (transparent objects, particles).
   - Too many materials (each unique material = separate draw call).
   - Post-processing passes.
   - Complex shaders.

### Step 3: Fix and Measure
Make ONE change at a time. Re-profile after each change. Verify the metric improved. It is easy to make changes that feel right but have no measurable effect.

## Common Profiler Readings and Fixes

| Profiler Reading | Likely Cause | Fix |
|-----------------|--------------|-----|
| High GC.Alloc in Update | Per-frame allocations | Cache, pool, avoid LINQ/string ops |
| Rendering.DrawBatches > 200 (Quest 2) | Too many materials / no batching | Atlas textures, GPU instancing, static batching |
| Physics.Simulate > 3 ms | Too many colliders or complex meshes | Simplify colliders, reduce physics layers |
| Camera.Render > 8 ms | Overdraw or complex shaders | Reduce transparency, simplify shaders |
| Scripts > 5 ms | Expensive per-frame logic | Move to Jobs, reduce frequency, cache results |
| GC spike (> 5 ms) | Large allocation triggering collection | Pool objects, pre-allocate collections |

## Deep Profile Mode

Enable Deep Profile for detailed per-method breakdown. Warning: it adds significant overhead and slows the game down, so timings are not accurate -- only useful for finding WHICH methods are expensive, not HOW expensive they are.

For accurate timings, use normal (non-deep) profiling and add `Profiler.BeginSample("MyMethod")` / `Profiler.EndSample()` around suspected hot spots.
