# CLAUDE.md -- Project Instructions

## Project Overview

This is a **VR Rhythm Game** called **Futuristic Cavemen**, built with **Unity 6000.4.0f1** targeting **Meta Quest** devices via **OpenXR**. The theme blends prehistoric cavemen and dinosaurs with futuristic glowing technology.

## Skills

Six custom skills are installed in `.claude/skills/`. Each skill has a `SKILL.md` with trigger conditions and reference files. **Always check and apply the relevant skill(s) before responding to any task.** Multiple skills often apply simultaneously.

### Skill Index

| Skill | Path | Use When |
|-------|------|----------|
| **Unity 6000.4** | `.claude/skills/unity-6000-4/` | Any Unity C# code, MonoBehaviour, physics, async, Input System, project setup |
| **OpenXR Meta Quest** | `.claude/skills/openxr-meta-quest/` | VR setup, controllers, hand tracking, passthrough, XR Interaction Toolkit, Quest builds |
| **Unity Optimization** | `.claude/skills/unity-optimization/` | Frame rate, draw calls, GC, profiling, pooling, Jobs/Burst, Quest performance budgets |
| **Rhythm Game VR** | `.claude/skills/rhythm-game-vr/` | Audio sync, DSP timing, note charts, hit detection, scoring, combos, beat mapping |
| **Hiragana Japanese** | `.claude/skills/hiragana-japanese/` | Japanese text for UI, menus, item names, translations, Hiragana characters |
| **Futuristic Cavemen** | `.claude/skills/futuristic-cavemen/` | Theme, art direction, lore, naming, creatures, environments, visual identity |

### How to Use Skills

1. **Read the `SKILL.md`** in each relevant skill folder before starting work on a task.
2. **Read the `references/` files** within each skill as directed by the SKILL.md's reference table.
3. **Layer skills together.** For example, writing a note-spawning script involves:
   - `unity-6000-4` (C# patterns, lifecycle, naming conventions)
   - `rhythm-game-vr` (note system, DSP timing)
   - `unity-optimization` (object pooling, avoiding allocations)
   - `futuristic-cavemen` (note visuals fit the theme)
4. When in doubt, consult the skill -- it's better to have the guidance and not need it.

## Code Conventions

- **C# 9** (Unity 6000.4 default)
- `[SerializeField] private` over public fields
- `_camelCase` for private fields, `PascalCase` for public members
- Cache `GetComponent` results in `Awake()`
- Use `Rigidbody.linearVelocity` (not `.velocity`) in Unity 6
- Use `Awaitable` async over coroutines for new code
- Use new Input System, never legacy `Input.GetKey()`
- All timing in rhythm code uses `double` and `AudioSettings.dspTime`
- Namespace everything: `namespace MyGame.*`
- Assembly Definitions for all non-prototype code

## VR-Specific Rules

- Rendering: URP Forward, no post-processing, MSAA 4x, HDR off
- Graphics API: Vulkan (required for passthrough)
- Scripting Backend: IL2CPP, ARM64
- Target 90 Hz (11.1ms frame budget)
- Pool all runtime objects -- no Instantiate/Destroy during gameplay
- Haptic feedback on every successful hit

## Theme Quick Reference

- Aesthetic: Primal materials (stone, bone, hide) with glowing energy circuits
- Colors: Amber/orange primary, cyan/teal secondary, magenta accents on dark backgrounds
- All in-game text pairs Hiragana with romaji in code comments
- UI looks like stone tablets and stretched hide
