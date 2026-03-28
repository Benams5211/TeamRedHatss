# Creatures and Characters

## Player Character

The player is a **Caveman Drummer** -- a member of an ancient tribe that discovered rhythm is the key to controlling the "いにしえ の ちから" (ancient power) flowing through the earth.

### Appearance
- Stocky, athletic build.
- Animal hide clothing with glowing stitching.
- Face paint in geometric, circuit-like patterns (bioluminescent pigment).
- Hair tied back with bone pins.
- Wears a crystal pendant that glows brighter with higher combo.

The player does not see their full body in VR (first person), but their hands/controllers are visible as:
- **Bone clubs** (default): dinosaur femurs with crystal tips embedded in the striking end.
- **Stone mallets** (unlockable): obsidian head, wood handle, amber energy lines.
- **Claw gauntlets** (unlockable): raptor claws on hide gloves, teal energy trails.

---

## Dinosaurs

Dinosaurs serve multiple roles: environment, spectacle, rhythm elements, and characters.

### Friendly / Neutral Dinosaurs

| Dinosaur | Role in Game | Visual Notes |
|----------|-------------|--------------|
| Brachiosaurus | Background scenery, living mountain | Moss-covered, bioluminescent spine ridges (teal), slow-moving |
| Triceratops | Stage platform / drum surface | Frill has crystal nodes that light up on beat |
| Parasaurolophus | Sound effects, horn blasts on song transitions | Crest glows amber when calling |
| Ankylosaurus | Resting spot, level select "vehicle" | Club tail taps the ground in rhythm |
| Compsognathus (compys) | UI helpers, menu mascots | Tiny, curious, run in packs, glow green |
| Pteranodon | Score display carrier, flies overhead | Wingspan has energy membrane, carries HUD elements |
| Stegosaurus | Note highway support | Back plates light up sequentially like a progress bar |

### Threatening / Boss Dinosaurs

| Dinosaur | Role | Visual Notes |
|----------|------|--------------|
| T-Rex | Boss encounter, must drum the right pattern to calm | Magenta energy lines, volcanic red eyes, shaking ground = bass hits |
| Velociraptor pack | Speed challenge, fast note patterns | Neon cyan claw marks in the air, quick and agile |
| Spinosaurus | Water level boss | Sail glows with shifting colors, splashes = beat drops |
| Dilophosaurus | Hazard, spits at missed notes | Frill flashes danger magenta before attack |
| Carnotaurus | Chase sequence, charging toward player | Amber energy horns, tremor footsteps |

### Fantastical / Hybrid Creatures

Since this is a fantasy setting, some creatures can be invented:

| Creature | Description |
|----------|-------------|
| Crystalback | Turtle-like creature with a shell made of resonating crystals. Tapping its shell produces different notes. |
| Thundermaw | A large quadruped with a jaw that cracks like thunder when it snaps. Used as a bass drum. |
| Glowwing | Moth-like insects that swarm in patterns matching the music visualizer. |
| Boneweaver | Spider-like creature that builds webs from glowing bone fiber. Its web vibrates with the music. |

---

## Tribe Members (NPCs)

If the game includes a hub world or cutscenes:

| Character | Role | Visual |
|-----------|------|--------|
| Elder (おおさま / oosama) | Tutorial guide, lore keeper | Oldest tribe member, full-body circuit paint, bone staff with large crystal |
| Scout (はやあし / hayaashi) | Unlocks new levels | Young, agile, rides a raptor, minimal gear |
| Crafter (つくりて / tsukurite) | Upgrades/skins shop | Muscular, covered in bone dust, carries tools |
| Shaman (まつり / matsuri) | Calibration screen host, rhythm teacher | Wears a dinosaur skull mask, dances constantly |

---

## Creature Animation Guidelines

For Quest optimization, keep creature animations simple:

- Idle animations: 2-3 second loops, 30 fps baked.
- Reaction animations (on beat hits): short, 0.5-1s, triggered by events.
- Boss animations: slightly more complex but limit bone count (40-60 per creature).
- Background creatures: use simple shader-based animation (vertex displacement for breathing, UV scrolling for glowing) instead of skeletal animation.

### LOD for Creatures

| Distance | Detail |
|----------|--------|
| Close (< 5m) | Full mesh, skeletal animation, emission maps |
| Medium (5-15m) | Reduced mesh (50%), simplified animation, emission maps |
| Far (15-30m) | Low-poly silhouette, no animation, flat emissive color |
| Very far (> 30m) | Billboard sprite or culled |
