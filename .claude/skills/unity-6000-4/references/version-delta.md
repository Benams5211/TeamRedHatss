# Version Delta: 6000.3.x LTS -> 6000.4.0f1

## Breaking or Behavioral Changes

### Artifact Database Version Bump
The artifact dependency was changed from `ImportResultID` to `ImportResultOutputID`. This triggers a **full reimport of all assets** the first time you open a 6000.3.x project in 6000.4.0f1. Plan for this -- it can take significant time on large projects.

The upside: if an asset's import output does not change, downstream assets that depend on it are no longer reimported. This reduces import churn during development.

### Editor C# Compilation Mode
Release builds of the Editor engine assemblies now compile C# in Release mode (previously Debug). Debug editor builds still use Debug. Player engine assemblies always compile in Release.

This means some debug-only code paths that previously ran in Editor release builds will no longer execute. If you relied on `#if DEBUG` checks in Editor code, verify they still behave as expected.

### Prismatic Articulation Joint
Setting `targetVelocity` to a positive value now correctly results in a positive linear velocity during simulation. If you had workarounds for the old inverted behavior, remove them.

### Light Probe Unloading
When you unload the last scene containing light probes, changes now apply automatically. You no longer need to call `LightProbes.Tetrahedralize()` manually afterward.

## New Features and Improvements

### Render Graph Performance
2-4% CPU gain on the main thread during the Render Graph recording step. No code changes needed -- this is an internal optimization.

### CAMetalDisplayLink (macOS)
Synchronizes frame rate with the display refresh rate more accurately on macOS. Enable in Player Settings for reduced stuttering and more stable `Time.deltaTime`.

### LOD Visualization
Mesh Renderer and Skinned Mesh Renderer now have an Inspector slider to visually scrub through LOD levels. Useful for verifying LOD transitions without moving the camera.

### Scene View Grid Transform
You can set custom position and rotation for the Scene View grid, making alignment easier for non-standard environments.

### Deep Linking on iOS
`Application.deepLinkActivated` now correctly fires on cold start and when the app is already running.

## Bug Fixes Worth Knowing

- **Rigidbody on prefabs**: No longer creates internal physics objects unintentionally.
- **MeshCollider Fast Midphase**: Stack overflow on raycast fixed.
- **UnityEvent memory leak**: Fixed in player builds.
- **ShaderVariantCollection prewarm crash**: Fixed.
- **Canvas shader uv1 channel**: Fixed initialization when not using masking.

## Migration Checklist

1. Back up your project before opening in 6000.4.0f1.
2. Expect a full asset reimport on first open.
3. Search for `#if DEBUG` in Editor scripts and verify behavior.
4. If using Prismatic Articulation Joints with targetVelocity, remove any sign-inversion workarounds.
5. If manually calling `LightProbes.Tetrahedralize()` after scene unload, that call is now unnecessary (but harmless).
6. Test macOS builds with CAMetalDisplayLink enabled for smoother frame pacing.
