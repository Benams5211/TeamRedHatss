# World and Environments

## Level Structure

Each level is a song. Each song takes place in a distinct environment within the Futuristic Cavemen world. Environments define the visual backdrop, ambient sounds, and thematic note skins.

---

## Biomes

### 1. Crystal Caverns (ほらあな / horaana)
The starting biome. Underground caves with crystal formations.

**Visual**: Dark stone walls with amber circuit engravings. Large crystal clusters serve as light sources, casting prismatic light. Stalactites drip with glowing liquid. The note highway is a path of floating stone slabs.

**Palette**: Cave Dark (#1a1a2e) + Amber Glow (#ff9f1c) + Bone Ivory (#f2e9de).

**Ambient sounds**: Dripping water, echoing footsteps, low crystal hum.

**Creatures**: Compys scurry in the background. Crystalbacks sit on ledges. Glowwings flutter near crystal clusters.

**Difficulty range**: Easy to Normal (tutorial and early songs).

### 2. Primordial Jungle (もり / mori)
Dense prehistoric forest with massive ferns and towering trees.

**Visual**: Enormous tree trunks with bioluminescent moss. Vines with teal energy lines hanging everywhere. Shafts of light break through the canopy. The note highway weaves between trees.

**Palette**: Jungle Green (#2d6a4f) + Spirit Teal (#06d6a0) + Molten Orange (#e85d04).

**Ambient sounds**: Insect chorus, distant dinosaur calls, rustling leaves, bird-like pteranodon cries.

**Creatures**: Brachiosaurus visible through gaps in the canopy. Parasaurolophus calls between songs. Raptors dart between trees.

**Difficulty range**: Normal to Hard.

### 3. Volcanic Wasteland (かざん / kazan)
Lava flows, ash clouds, and scorched earth.

**Visual**: Black basalt ground cracked with glowing orange lava. Ash particles drift through the air. Bone structures of long-dead giants serve as landmarks. The note highway floats over a lava river.

**Palette**: Obsidian (#0f0f23) + Volcanic Red (#d00000) + Molten Orange (#e85d04).

**Ambient sounds**: Bubbling lava, distant rumbles, cracking rock, wind through bone structures.

**Creatures**: T-Rex silhouette in the ash clouds. Carnotaurus charges in the background. Fire-adapted compys with red glow.

**Difficulty range**: Hard to Expert.

### 4. Coastal Ruins (うみべ / umibe)
Ancient structures half-submerged on a prehistoric coastline.

**Visual**: Massive stone pillars and arches covered in barnacles and coral. Bioluminescent tide pools. A full moon with pteranodons silhouetted. The note highway runs along a stone bridge over shallow water.

**Palette**: Midnight Blue (#0f0f23 with blue tint) + Bioluminescent Cyan (#00f5d4) + Sandstone (#c9b99a).

**Ambient sounds**: Waves crashing, whale-like calls from marine reptiles, wind, distant thunder.

**Creatures**: Spinosaurus wading in the shallows. Mosasaur breaching in the distance. Schools of glowing fish.

**Difficulty range**: Normal to Hard.

### 5. Sky Peaks (そら / sora)
Mountain tops above the clouds.

**Visual**: Stone platforms floating above a cloud sea. Ancient carved obelisks with dense circuit patterns. Aurora-like energy ribbons in the sky. The note highway is a chain of floating stone steps.

**Palette**: Deep purple (#1a0a2e) + Amber Glow (#ff9f1c) + Danger Magenta (#ff006e).

**Ambient sounds**: High-altitude wind, crystalline chimes, distant thunder, pteranodon wing beats.

**Creatures**: Pteranodon flocks circling. Quetzalcoatlus as a massive boss encounter. Glowwings form constellations.

**Difficulty range**: Expert (endgame content).

### 6. The Core (しんぶ / shinbu)
The deepest point in the world where the "ancient power" originates.

**Visual**: A massive underground chamber with a pulsing energy sphere at the center. Walls are pure crystal, refracting light into rainbows. Fossilized dinosaur skeletons are embedded in the walls, their bones glowing. The note highway spirals around the energy core.

**Palette**: All energy colors at once, shifting. Base is Obsidian (#0f0f23).

**Ambient sounds**: Deep bass throb, harmonic resonance, the sound of the "ancient power" itself.

**Creatures**: Spectral dinosaurs made of pure energy. The final boss is a massive T-Rex skeleton that reforms from the cave walls.

**Difficulty range**: Expert (final level).

---

## Environment Art Guidelines for Quest

### Geometry Budget
| Element | Triangle Budget |
|---------|----------------|
| Entire scene (visible) | 150K-300K total |
| Skybox / backdrop | Shader-based or 2D planes |
| Ground / terrain | 5K-15K |
| Major landmarks (per object) | 1K-5K |
| Vegetation (per instance) | 100-500 |
| Note highway | 500-2K |
| Background creatures | 500-2K each |

### Optimization Rules
1. Use baked lighting for all static environment geometry.
2. Environment circuits/glow: use emissive textures, not real-time lights.
3. Distant elements: flat planes or billboards, not 3D geometry.
4. Fog: use URP's built-in fog or a custom fog shader to hide draw distance and add atmosphere.
5. Particle ambient (dust, fireflies, ash): keep under 50 particles per emitter.
6. Scrolling UV for lava, water, energy flows -- no real-time fluid simulation.

### Skybox Per Biome
Use a procedural or cubemap skybox per biome. Gradient skyboxes are cheapest on Quest:
- Caverns: no skybox (fully enclosed), use dark ambient.
- Jungle: green-to-blue gradient with volumetric light shafts (faked via billboards).
- Volcanic: orange-to-black gradient with particle ash.
- Coastal: dark blue-to-purple gradient with a moon sprite.
- Sky Peaks: deep purple-to-black with aurora shader.
- The Core: radial gradient from the energy sphere, all colors.
