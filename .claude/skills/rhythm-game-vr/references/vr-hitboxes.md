# VR-Specific Hitbox Design

## Controller Hit Zones

In VR rhythm games, the player swings physical controllers to hit virtual notes. The hitbox design directly affects how satisfying the game feels.

### Hitbox Shape Options

| Shape | Use Case | Pros | Cons |
|-------|----------|------|------|
| Sphere | Drum hits, punching | Simple, rotation-independent | Hard to make directional |
| Capsule | Saber/club swings | Matches elongated weapons | Needs rotation tracking |
| Box | Paddle/shield blocks | Clear directional facing | Feels rigid |

### Recommended Sizes

For drum/punch style (futuristic caveman clubs):
- Controller hitbox: Sphere, radius 0.08-0.12m.
- Note target: Sphere, radius 0.15-0.20m.

For saber/slash style:
- Controller hitbox: Capsule, length 0.6-0.8m, radius 0.05m.
- Note target: Box, 0.3 x 0.3 x 0.15m.

Generous hitboxes feel better in VR because depth perception is imperfect and players lack physical feedback for "contact."

### Hitbox Placement

```csharp
using UnityEngine;

namespace MyGame.VR
{
    /// <summary>
    /// Attach to the controller GameObject. The hitbox follows the controller
    /// with an optional offset for weapon length.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class ControllerHitbox : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float _radius = 0.1f;
        [SerializeField] private Vector3 _offset = new(0f, 0f, 0.05f);

        private SphereCollider _collider;

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;
            _collider.radius = _radius;
            _collider.center = _offset;
        }
    }
}
```

## Note Hitbox Setup

Notes need trigger colliders that the controller hitbox overlaps with.

```csharp
using UnityEngine;

namespace MyGame.RhythmCore
{
    [RequireComponent(typeof(SphereCollider))]
    public class NoteHitbox : MonoBehaviour
    {
        [SerializeField] private float _radius = 0.18f;

        private void Awake()
        {
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = _radius;
        }
    }
}
```

Tag notes with "Note" so the HitDetector can filter collision events.

## Layer Setup

Use physics layers to prevent unwanted collisions:

| Layer | Description |
|-------|-------------|
| Default | Environment, non-interactive |
| ControllerHitbox | Controller trigger zones |
| NoteTarget | Note trigger zones |
| Hand | Hand tracking colliders (if used) |

In Edit > Project Settings > Physics > Layer Collision Matrix:
- ControllerHitbox <-> NoteTarget: **enabled**
- Everything else involving these layers: **disabled**

This prevents notes from colliding with the environment or each other.

## Spatial Note Layouts

### Drum Circle (Taiko-style)
Notes appear on fixed positions in front of the player. Player hits them like drums.

```
            [Left]     [Center]     [Right]
              O           O           O
                    
              Player (standing)
```

Good for: seated play, accessible, low fatigue.

### Highway (Beat Saber-style)
Notes scroll toward the player along lanes.

```
  Lane 0    Lane 1    Lane 2    Lane 3
    |         |         |         |
    O         |         O         |      <- approaching
    |         O         |         |
    |         |         |         O
    ===================================  <- hit line
              Player
```

Good for: high energy, visual spectacle, difficulty scaling via lane count.

### Orbital (360-degree)
Notes appear around the player in a sphere or ring.

Good for: immersion, room-scale, but can cause motion sickness if too fast.

## Haptic Feedback Patterns

Haptics make hits feel physical. Different judgments should feel different.

```csharp
public static class HapticPatterns
{
    // Amplitude, Duration
    public static readonly (float amp, float dur) Perfect = (0.8f, 0.06f);
    public static readonly (float amp, float dur) Great   = (0.5f, 0.05f);
    public static readonly (float amp, float dur) Good    = (0.3f, 0.04f);
    public static readonly (float amp, float dur) Miss    = (0.1f, 0.02f);
}
```

Apply via the XR Interaction Toolkit controller:

```csharp
var pattern = judgment switch
{
    Judgment.Perfect => HapticPatterns.Perfect,
    Judgment.Great   => HapticPatterns.Great,
    Judgment.Good    => HapticPatterns.Good,
    _                => HapticPatterns.Miss,
};
_controller.SendHapticImpulse(pattern.amp, pattern.dur);
```

## Visual Feedback

Layer visual feedback to make hits feel impactful:

1. **Scale pop**: Note briefly scales up 1.2x then disappears.
2. **Color flash**: Hit zone flashes the judgment color (gold = Perfect, blue = Great, green = Good).
3. **Particle burst**: Small particle system at the hit point. Use a pooled particle system.
4. **Judgment text**: Floating text ("かんぺき!", "すごい!", "いい!") that fades upward.
5. **Combo counter**: Large number in the player's peripheral vision.

Keep VFX lightweight -- every particle system and transparency layer costs GPU time on Quest.

## Comfort Considerations

- Notes should approach from the front, not from behind or below the player.
- Avoid requiring the player to look straight up or down for extended periods.
- Keep the active play area within a comfortable arm-reach zone (~0.5-0.8m from the body).
- Provide seated and standing mode options.
- If notes come from multiple directions, use audio cues (panning) to telegraph them.
- Avoid strobing or rapid color changes that could trigger photosensitive reactions.
