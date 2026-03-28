# VR Project Setup (Unity 6000.4.0f1 + OpenXR Meta Quest)

## Step-by-Step: New VR Project

### 1. Create Project
- Unity Hub > New Project > Unity 6000.4.0f1.
- Template: **3D (URP)**.
- Name: PascalCase, no spaces.

### 2. Install Packages
Open Window > Package Manager. Install in this order:
1. `com.unity.xr.openxr` (OpenXR Plugin)
2. `com.unity.xr.meta-openxr` (Unity OpenXR: Meta)
3. `com.unity.xr.interaction.toolkit` (XR Interaction Toolkit)
4. `com.unity.xr.hands` (XR Hands -- optional, for hand tracking)
5. `com.unity.inputsystem` (Input System -- usually auto-installed as dependency)

When prompted to use the new Input System backend, select **Yes** and allow the Editor to restart.

### 3. Configure XR Plug-in Management
Edit > Project Settings > XR Plug-in Management:
- **Android tab**: check **OpenXR**.
- **Standalone tab** (for Quest Link testing): check **OpenXR**.
- Click the OpenXR settings gear icon.
- Enable the **Meta Quest** feature group.
- Under Interaction Profiles, add **Oculus Touch Controller Profile**.
- Fix all validation warnings.

### 4. Player Settings (Android)
Edit > Project Settings > Player > Android tab:
- Other Settings:
  - Color Space: **Linear**.
  - Auto Graphics API: **uncheck**. Add **Vulkan** and remove OpenGLES.
  - Minimum API Level: **Android 10.0 (API 29)**.
  - Scripting Backend: **IL2CPP**.
  - Target Architectures: **ARM64** only (uncheck ARMv7).
  - Active Input Handling: **Input System Package (New)**.

### 5. URP Asset Configuration
Find the URP Asset (search `t:UniversalRenderPipelineAsset` in Project window):
- Quality:
  - HDR: **off**.
  - Anti-Aliasing (MSAA): **4x**.
  - Render Scale: **1.0** (adjust down if GPU-bound).
- Lighting:
  - Main Light: **Per Pixel**.
  - Additional Lights: **Per Vertex** or **Disabled** for Quest 2.
  - Cast Shadows: **off** or single cascade.
- Shadows:
  - Max Distance: **10-20m** (keep tight for VR).

Find the URP Renderer Data (`t:UniversalRendererData`):
- Post-processing: **off** (enable selectively later).
- Intermediate Texture: **Auto**.
- Rendering Path: **Forward**.

### 6. Quality Settings
Edit > Project Settings > Quality:
- Create a "Quest" quality level if needed.
- VSync Count: **Don't Sync** (VR compositor handles this).

### 7. Time Settings
Edit > Project Settings > Time:
- Fixed Timestep: `0.01111` (1/90 for 90 Hz Quest refresh rate).

### 8. Scene Setup
Delete the default Main Camera. Create an XR Origin:
- GameObject > XR > XR Origin (Action-based).
- This creates the rig with Camera Offset, Main Camera, and controllers.
- On Main Camera's TrackedPoseDriver, verify `centerEyePosition [XR HMD]` is set.

### 9. Build and Test
- File > Build Settings > Switch Platform to **Android**.
- Texture Compression: **ASTC**.
- Connect Quest via USB (enable Developer Mode in Oculus app first).
- Build and Run.

## Quest Link Testing (Editor)

For faster iteration, use Quest Link or Air Link:
1. Enable OpenXR on the **Standalone** tab in XR Plug-in Management.
2. Add Meta Quest feature group on Standalone as well.
3. Connect Quest via Link cable or enable Air Link.
4. Press Play in the Editor -- the game renders in the headset.

Limitations: Passthrough and plane detection do not work via Link. You must build an APK to test those features.

## Common Package Version Compatibility

| Package | Minimum Version | Notes |
|---------|----------------|-------|
| com.unity.xr.openxr | 1.14.0 | Feature parity with Oculus XR Plugin |
| com.unity.xr.meta-openxr | 2.1.0 | Depth API, passthrough, anchors |
| com.unity.xr.interaction.toolkit | 3.0.0+ | Action-based controllers |
| com.unity.xr.hands | 1.4.0+ | Hand tracking |
| com.unity.xr.arfoundation | 6.0.0+ | Plane detection, anchors, passthrough |

Version numbers of AR Foundation, ARCore XR Plugin, and ARKit XR Plugin must match if used together.
