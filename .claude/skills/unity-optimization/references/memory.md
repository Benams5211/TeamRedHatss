# Memory and Asset Optimization

## Memory Budgets

| Platform | Total Available | Recommended App Usage |
|----------|----------------|-----------------------|
| Quest 2 | ~3 GB system | ~1.5 GB for app |
| Quest 3 | ~6 GB system | ~2.5 GB for app |

Exceeding the budget causes the OS to kill your app without warning.

## Texture Memory

Textures are the largest memory consumer by far.

### Size Estimation
Formula: `width * height * bytesPerPixel * mipmapMultiplier`

| Format | Bytes Per Pixel |
|--------|----------------|
| RGBA32 (uncompressed) | 4 |
| ASTC 4x4 | 1 |
| ASTC 6x6 | 0.89 |
| ASTC 8x8 | 0.5 |

Mipmap multiplier: 1.33 (mipmaps add ~33% memory).

Example: 2048x2048 ASTC 6x6 with mipmaps = 2048 * 2048 * 0.89 * 1.33 = ~5 MB.

### Reduction Strategies
1. Downscale textures at import. Most objects do not need 2048 textures on Quest 2.
2. Use ASTC 6x6 or 8x8 compression for non-critical textures.
3. Share textures via atlases.
4. Disable mipmaps on textures viewed at fixed distance (UI, skybox).
5. Use Virtual Texturing for large open worlds (Unity 6 feature).

## Mesh Memory

- Use mesh compression (Low/Medium/High) in import settings.
- Remove unused UV channels, vertex colors, and blend shapes.
- Combine static meshes to reduce overhead per mesh.
- Use LODs -- lower LOD meshes use less memory when they replace higher ones.

## Audio Memory

- Use **Vorbis** compression for music and long clips (streaming mode).
- Use **ADPCM** for short sound effects (lower CPU decode cost).
- Load Type:
  - **Streaming**: for music tracks. Lowest memory, highest CPU.
  - **Compressed In Memory**: for frequently played effects. Medium memory.
  - **Decompress On Load**: for very short, frequently played clips. Highest memory, lowest latency.

| Clip Type | Format | Load Type |
|-----------|--------|-----------|
| Background music | Vorbis, quality 50-70% | Streaming |
| Sound effects (frequent) | ADPCM | Compressed In Memory |
| Sound effects (rare) | Vorbis | Compressed In Memory |
| UI clicks, short hits | PCM or ADPCM | Decompress On Load |

## Addressables

For projects with many scenes or large asset libraries, use Addressables to load and unload assets on demand.

Benefits:
- Assets not in the current scene are not loaded into memory.
- Async loading prevents frame drops during scene transitions.
- Remote catalogs enable over-the-air content updates (if needed).

Key patterns:
- Group assets by scene or usage pattern.
- Load groups before they are needed (preload during loading screens).
- Release handles when assets are no longer needed: `Addressables.Release(handle)`.
- Use `Addressables.LoadSceneAsync()` for scene loading.

## Build Size

Quest APK size limit is 1 GB (recommended under 500 MB for faster installs).

Reduction strategies:
1. Strip unused engine features: Edit > Project Settings > Player > Strip Engine Code.
2. Use IL2CPP with Managed Code Stripping set to Medium or High.
3. Compress textures (ASTC saves significant space).
4. Compress audio (Vorbis at 50-70% quality).
5. Remove unused packages from Package Manager.
6. Use Asset Bundles or Addressables for optional content.
7. Check the Build Report (Console > Build Report after building) to identify the largest assets.

## Runtime Memory Management

### Avoid Runtime GC
The managed heap in Unity grows but rarely shrinks. Every GC collection causes a frame spike.

Prevention:
- Pre-allocate all collections at startup.
- Pool all runtime-created objects.
- Use struct over class for small, short-lived data.
- Use NativeArray/NativeList (from Unity.Collections) for large datasets.

### Unload Between Scenes
```csharp
async Awaitable LoadSceneAsync(string sceneName)
{
    await SceneManager.LoadSceneAsync(sceneName);
    await Resources.UnloadUnusedAssets();
    System.GC.Collect();
}
```

Call `Resources.UnloadUnusedAssets()` during loading screens, never during gameplay.
