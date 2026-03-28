# Art Direction and Visual Style

## Core Aesthetic: Primal-Tech Fusion

Every visual element blends two layers:
1. **Primal base**: rough, organic, handmade, natural materials.
2. **Tech overlay**: glowing lines, energy pulses, holographic projections.

Neither layer should dominate. A stone wall is still clearly a stone wall -- it just has faintly glowing circuit-patterns carved into it. A dinosaur is still a dinosaur -- it just has bioluminescent markings along its spine.

## Color Rules

### Energy Colors (Emissive)
Use these for glowing elements, UI highlights, note skins, and effects.

| Name | Hex | Usage |
|------|-----|-------|
| Amber Glow | #ff9f1c | Primary energy, note highlights, warm power |
| Molten Orange | #e85d04 | Intense hits, fire, volcanic areas |
| Bioluminescent Cyan | #00f5d4 | Secondary energy, water areas, cool power |
| Spirit Teal | #06d6a0 | Healing, nature, passive effects |
| Danger Magenta | #ff006e | Warnings, miss feedback, boss attacks |
| Volcanic Red | #d00000 | Damage, lava, critical states |

### Base Colors (Non-emissive)
Use these for geometry, environment, and grounding elements.

| Name | Hex | Usage |
|------|-----|-------|
| Cave Dark | #1a1a2e | Deep cave backgrounds, shadows |
| Obsidian | #0f0f23 | Dark stone surfaces, night sky |
| Hide Brown | #2d1b0e | Leather, wood, dried earth |
| Sandstone | #c9b99a | Light stone, desert surfaces |
| Bone Ivory | #f2e9de | Skeletal structures, UI backgrounds |
| Jungle Green | #2d6a4f | Dense foliage, moss |
| Fern Green | #40916c | Lighter vegetation, vines |

### Color Pairing Rule
Every scene should have:
- One dominant base color (sets the biome feel).
- One primary energy color (sets the mood).
- One accent energy color (for contrast and emphasis).

Example: Cave level = Cave Dark base + Amber Glow primary + Bioluminescent Cyan accent.

## Shader Patterns

### Glowing Circuit Lines
Apply to stone, bone, and wood surfaces. The "ancient circuit" effect:

1. Use a secondary UV channel with a hand-drawn circuit pattern texture.
2. Apply emissive color to this channel.
3. Animate emission intensity with a slow pulse (0.5-1 Hz, sine wave).
4. Optional: scroll UV slowly for a "flowing energy" effect.

In URP, use the Lit shader with an emission map, or write a simple Shader Graph:
- Base Color: stone/bone texture.
- Emission: circuit-pattern texture * energy color * pulse animation.

### Bioluminescence on Creatures
For dinosaur markings and glowing plants:

1. Paint emission masks on the model's UV (stripes, spots, spine ridges).
2. Animate emission intensity slightly offset per body part for organic feel.
3. Use subsurface-scattering approximation: tint the base color near emission areas.

### Crystal / Hologram Effect
For "screens" and "displays" in the world:

1. Use a semi-transparent material with rim lighting.
2. Add scanline effect (horizontal lines scrolling slowly).
3. Apply a slight vertex wobble for the "unstable hologram" look.
4. Color: use the primary energy color at low saturation.

## UI Design

### Panel Style: Stone Tablet
- Background: rounded rectangle with stone texture and subtle circuit engravings.
- Border: bone-colored edge with small glow on inner rim.
- Drop shadow: subtle, warm-tinted (not black).

### Button Style: Bone Toggle
- Idle: bone-ivory colored, slightly 3D (beveled edges).
- Hover/Focus: circuit lines on the button surface light up.
- Pressed: button sinks, brief amber flash.
- Disabled: cracks appear on the surface, no glow.

### Text Style
- Primary font: chunky, angular sans-serif that evokes carved stone (or Hiragana characters).
- Color: bone ivory on dark backgrounds, cave dark on light backgrounds.
- Important text: add a subtle glow halo in the primary energy color.
- Avoid thin or delicate fonts -- everything should feel heavy, carved, substantial.

### Transitions
- Scene transitions: stone slabs slide across the screen (left-right or top-bottom).
- Menu open: crystal lights up, panels "grow" from the crystal outward.
- Menu close: energy drains downward, panels shrink back into the crystal.
- Loading: an animated cave painting of a running dinosaur.

## VFX Budget (Quest)

Keep VFX lightweight. VR on Quest has tight GPU budgets.

| Effect | Max Particles | Lifetime | Notes |
|--------|--------------|----------|-------|
| Note hit burst | 20-30 | 0.3-0.5s | Opaque or additive, no alpha blending |
| Miss dust | 10-15 | 0.5s | Opaque small quads |
| Combo milestone | 30-50 | 0.8s | Only on 10/25/50/100 combo |
| Environment ambient | 5-10 per emitter | 2-3s | Floating dust motes, fireflies |
| Energy flow (note highway) | Shader-based | N/A | Use scrolling UV, not particles |

Prefer shader-based effects over particle systems where possible. A scrolling emissive texture is cheaper than 100 particle quads.
