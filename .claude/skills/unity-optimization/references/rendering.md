# Rendering Optimization

## Draw Call Reduction

Every unique combination of mesh + material + shader variant = a draw call. Quest 2 budget is 100-150 draw calls.

### Static Batching
For geometry that never moves. Unity combines meshes at build time.
- Mark GameObjects as **Static** in the Inspector (or just Batching Static).
- Tradeoff: increases memory (each static batch stores a copy of the mesh data).

### GPU Instancing
For many copies of the same mesh+material (trees, rocks, grass).
- On the material: check **Enable GPU Instancing**.
- Shader must support instancing (URP/Lit does by default).
- Each unique material still requires a separate batch.

### SRP Batcher
URP's SRP Batcher reduces CPU cost of draw calls (not the count, but the cost per call).
- Enabled by default in URP. Verify in URP Asset > Advanced > SRP Batcher.
- Shaders must be SRP Batcher compatible (all URP/Lit shaders are).

### Dynamic Batching
Combines small meshes at runtime. Generally not worth it for VR -- the CPU cost of combining often exceeds the GPU savings. Disable it in URP Asset unless profiling shows a clear benefit.

## Texture Optimization

### Compression
- **ASTC** is the standard for Quest (Android). Use ASTC 6x6 for most textures, ASTC 4x4 for important textures needing higher quality.
- Set via texture import settings > Format > ASTC.

### Resolution
| Object Type | Max Texture Size (Quest 2) | Max Texture Size (Quest 3) |
|-------------|---------------------------|---------------------------|
| Hero objects (hands, weapons) | 1024 | 2048 |
| Environment (walls, floors) | 512-1024 | 1024-2048 |
| Distant objects | 256-512 | 512-1024 |
| UI elements | 512 | 1024 |

### Texture Atlasing
Combine multiple small textures into one atlas. This allows multiple objects to share one material, enabling batching.

Tools:
- Unity Sprite Atlas (for 2D / UI).
- Third-party tools (ProBuilder, TexturePacker) for 3D texture atlases.
- Manual UV remapping in modeling software.

### Mipmaps
Enable mipmaps for 3D scene textures (reduces aliasing and bandwidth). Disable for UI textures and textures only viewed at fixed distance.

## Shader Optimization

### Use URP/Lit or URP/Simple Lit
- URP/Lit: physically based, good quality. Fine for Quest 3.
- URP/Simple Lit: cheaper, Blinn-Phong. Better for Quest 2.
- URP/Unlit: cheapest. Use for objects with baked lighting or stylized art.

### Avoid
- Custom shaders with excessive texture samples (keep under 4 per pass).
- Alpha testing (discard/clip) -- breaks early-z rejection on mobile GPUs. Use alpha blending sparingly.
- Complex math in fragment shaders. Move calculations to vertex shader when possible.

### Shader Variants
Each keyword combination creates a separate variant. Reduce variants by:
- Stripping unused keywords in URP Asset > Shader Stripping settings.
- Avoiding per-material keyword toggles unless necessary.

## Lighting

### Baked Lighting
- Use Lightmaps for static geometry. Bake with GPU Lightmapper for speed.
- Use Light Probes for dynamic objects moving through baked environments.
- Baked lighting is "free" at runtime (just a texture lookup).

### Real-Time Lights
| Light Type | Cost | Recommendation |
|-----------|------|----------------|
| Directional (main) | Low-Medium | 1 allowed, per-pixel |
| Point/Spot (additional) | High | Per-vertex or disabled on Quest 2 |
| Area | Very High | Bake only, never real-time on Quest |

### Shadows
- Real-time shadows are expensive. Use 1 cascade, max distance 10-15m.
- Consider baked shadows (shadow masks) or no shadows + ambient occlusion baked into lightmaps.
- Shadow resolution: 512-1024 for Quest 2, 1024-2048 for Quest 3.

## LOD (Level of Detail)

Add LOD Groups to all 3D models:

| LOD Level | Distance | Triangle Target |
|-----------|----------|----------------|
| LOD 0 | Close (0-5m) | Full model |
| LOD 1 | Medium (5-15m) | 50% triangles |
| LOD 2 | Far (15-30m) | 25% triangles |
| LOD 3 / Culled | Very far (30m+) | Billboard or cull |

Set LOD bias in Quality Settings to control transition distances globally.

## Occlusion Culling

Unity's built-in occlusion culling prevents rendering objects hidden behind other objects.

Setup:
1. Mark large occluders (walls, terrain) as **Occluder Static**.
2. Mark all renderable objects as **Occludee Static**.
3. Window > Rendering > Occlusion Culling > Bake.

Effective in indoor environments and levels with walls. Less useful in open outdoor scenes.

## Overdraw

Overdraw = rendering the same pixel multiple times. Common culprits:
- Transparent / alpha-blended materials (particles, glass, foliage).
- Overlapping UI elements.
- Skybox behind opaque geometry (the skybox always renders).

Visualize overdraw: Scene View > Overdraw draw mode.

Fix:
- Reduce particle count and size.
- Use opaque materials whenever possible.
- Sort transparent objects back-to-front (Unity does this, but complex scenes can still cause issues).
- For foliage, use alpha-to-coverage instead of alpha blending on supported hardware.
