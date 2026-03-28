# Project Setup Checklist (Unity 6000.4.0f1)

## New Project Creation

1. Open Unity Hub, select Unity 6000.4.0f1.
2. Choose **3D (URP)** template for VR projects (Meta Quest uses URP).
3. Project name: PascalCase, no spaces (e.g., `FuturisticCavemen`).

## Essential Settings

### Player Settings (Edit > Project Settings > Player)

**Company Name / Product Name**: Set early -- they affect build output paths.

**Android tab** (for Quest):
- Minimum API Level: **Android 10.0 (API 29)** or higher.
- Target API Level: **Automatic (highest installed)**.
- Scripting Backend: **IL2CPP** (required for Quest).
- Target Architectures: **ARM64** only.
- Install Location: **Automatic**.
- Internet Access: **Require** if any online features; otherwise **Auto**.

### Quality Settings
- Create a "Quest" quality level.
- Disable VSync (the VR compositor handles sync).
- Disable shadows or use only a single cascade.
- Texture Quality: Half or Quarter for Quest 2; Full for Quest 3.

### Graphics Settings
- Use Vulkan as the primary Graphics API for Android (required for Passthrough).
- Remove OpenGLES if Vulkan is available.

### Time Settings
- Fixed Timestep: `0.01111` (90 Hz) for Quest. Match the target refresh rate.

## Folder Structure

```
Assets/
  _Project/
    Scripts/
      Core/           -> MyGame.Core.asmdef
      Gameplay/        -> MyGame.Gameplay.asmdef
      Audio/           -> MyGame.Audio.asmdef
      UI/              -> MyGame.UI.asmdef
      VR/              -> MyGame.VR.asmdef
      Editor/          -> MyGame.Editor.asmdef (Editor platform only)
    Prefabs/
    ScriptableObjects/
    Materials/
    Shaders/
    Audio/
    Scenes/
    Art/
      Models/
      Textures/
      Animations/
  Plugins/
  StreamingAssets/
```

## Build Settings

1. File > Build Settings.
2. Switch platform to **Android**.
3. Texture Compression: **ASTC** (standard for Quest).
4. Build System: **Gradle**.
5. Export Project: unchecked for direct APK builds.
6. Development Build: check during development for profiler access.
