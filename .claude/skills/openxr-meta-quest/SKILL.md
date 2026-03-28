---
name: openxr-meta-quest
description: >
  OpenXR with Meta Quest feature group for Unity 6 VR development. Use this skill whenever
  the user mentions OpenXR, Meta Quest, Quest 2, Quest 3, Quest 3S, Quest Pro, XR Plug-in
  Management, XR Interaction Toolkit, VR setup, VR controllers, hand tracking, passthrough,
  head-mounted display configuration, or any Meta/Oculus VR development in Unity. Also
  trigger when the user references com.unity.xr.openxr, com.unity.xr.meta-openxr,
  OVRPlugin, Meta XR Core SDK, XROrigin, TrackedPoseDriver, or building APKs for Quest
  devices. Covers project setup, feature group configuration, interaction profiles,
  controller input, hand tracking, passthrough, Application SpaceWarp, performance
  settings, and common VR pitfalls. This skill is specifically tuned for Unity 6000.4.0f1
  with the recommended OpenXR path (not the deprecated Oculus XR Plugin).
---

# OpenXR Meta Quest VR Development

Target: **Unity 6000.4.0f1** with **com.unity.xr.openxr** (1.14+) and **com.unity.xr.meta-openxr** (2.1.0+).

The OpenXR path is Meta's recommended development path as of SDK v74+. The Oculus XR Plugin is deprecated for new projects. All new platform features ship on the OpenXR backend only.

## When to Read Reference Files

| Topic | File | When |
|-------|------|------|
| Full project setup | `references/project-setup-vr.md` | New VR project or adding Quest support |
| Controller and hand input | `references/input-vr.md` | Any controller/hand interaction code |
| Performance and SpaceWarp | `references/vr-performance.md` | Optimization, frame rate, SpaceWarp |

---

## Package Installation

Install these packages via Package Manager (Window > Package Manager):

1. **XR Plug-in Management** (usually pre-installed).
2. **OpenXR Plugin** (`com.unity.xr.openxr`) -- 1.14+ for feature parity with Oculus XR Plugin.
3. **Unity OpenXR: Meta** (`com.unity.xr.meta-openxr`) -- 2.1.0+ for Depth API, passthrough, anchors.
4. **XR Interaction Toolkit** (`com.unity.xr.interaction.toolkit`) -- high-level interaction framework.
5. **XR Hands** (`com.unity.xr.hands`) -- if using hand tracking.
6. **AR Foundation** (`com.unity.xr.arfoundation`) -- if using passthrough, plane detection, anchors.

## Enable Meta Quest Feature Group

1. Edit > Project Settings > XR Plug-in Management.
2. **Android tab**: check **OpenXR**.
3. Under OpenXR, enable the **Meta Quest** feature group.
4. Fix all validation errors/warnings (the OpenXR settings panel lists them).
5. Under Interaction Profiles, add **Oculus Touch Controller Profile**.

Only one XR plug-in provider can be active per build target. If you also target ARCore, disable it before building for Quest.

## Scene Setup

```
XR Origin (XR Rig)
  Camera Offset
    Main Camera
      - TrackedPoseDriver (centerEyePosition [XR HMD])
      - Camera: Clear Flags = Solid Color, Background alpha = 0 (for passthrough)
    Left Controller
      - XR Controller (Action-based)
      - XR Ray Interactor or XR Direct Interactor
    Right Controller
      - XR Controller (Action-based)
      - XR Ray Interactor or XR Direct Interactor
```

Use the **XR Interaction Setup** prefab from XR Interaction Toolkit samples as a starting point.

## Target Devices

In OpenXR settings, select which Quest devices to support:
- Quest 2
- Quest 3
- Quest 3S
- Quest Pro

The runtime adjusts behavior based on this list. Only check devices you have tested on.

## URP Configuration for Quest

Quest requires specific URP settings for best performance, especially with passthrough:

- Graphics API: **Vulkan** (required for passthrough in dev builds; recommended always).
- URP Asset: disable HDR, set Anti-Aliasing to MSAA 4x, disable SRPBatcher if causing issues.
- URP Renderer Data: disable Post-processing, set Intermediate Texture to Auto.
- Rendering Path: Forward (not Deferred -- too expensive for mobile VR).

## Passthrough

Passthrough lets users see the real world behind virtual content.

Requirements:
- Camera Clear Flags: Solid Color with alpha = 0.
- Vulkan as Graphics API.
- `com.unity.xr.meta-openxr` 2.1.0+ installed.
- Meta Quest Passthrough feature enabled in OpenXR feature group.

Passthrough only works on-device -- it does not appear in the Unity Editor or via Quest Link during development.

## Common VR Pitfalls

| Problem | Cause | Fix |
|---------|-------|-----|
| Black screen on Quest | OpenXR not enabled or wrong build target | Switch to Android, enable OpenXR + Meta Quest feature group |
| Controllers not tracked | Missing interaction profile | Add Oculus Touch Controller Profile in OpenXR settings |
| Passthrough not working | Wrong camera settings or Graphics API | Set Clear Flags to Solid Color, alpha 0, use Vulkan |
| Low frame rate | Post-processing, shadows, or deferred rendering | Disable post-processing, use Forward, reduce shadow quality |
| App crashes on launch | IL2CPP not selected or wrong architecture | Set Scripting Backend to IL2CPP, Target Architecture to ARM64 |
| Input not responding | Using legacy Input Manager | Use Input System + XR Interaction Toolkit action-based controllers |

## Output Guidelines

1. All VR scripts should use the `UnityEngine.XR.Interaction.Toolkit` namespace when referencing XR Interaction Toolkit types.
2. Use `InputActionReference` for controller input bindings, not hard-coded button names.
3. Always include the full `using` block at the top of scripts.
4. Wrap in a project namespace (e.g., `namespace MyGame.VR`).
5. Generate complete, compilable scripts and save to `/mnt/user-data/outputs/`.
