# Hit Detection and Scoring

## Hit Window Design

Hit windows define the time tolerance around each note. The window is symmetric: a note at time 5.0 with a Perfect window of 35ms can be hit between 4.965 and 5.035.

### Recommended Windows

| Judgment | Window (+/-) | Description |
|----------|-------------|-------------|
| Perfect | 35 ms | Frame-perfect or near-perfect |
| Great | 70 ms | Solid timing |
| Good | 110 ms | Noticeably off but still counts |
| Miss | 150 ms | Beyond this, auto-miss |

These values are inspired by established rhythm games. Tune them during playtesting -- VR adds physical movement latency that may warrant slightly wider windows (especially for full-arm swings).

### Asymmetric Windows (Optional)

Some games use tighter early windows and looser late windows because hitting early feels worse than hitting late:

```csharp
public static Judgment JudgeAsymmetric(double hitTime, double noteTime)
{
    double delta = hitTime - noteTime; // negative = early, positive = late
    double absDelta = System.Math.Abs(delta);

    // Slightly more forgiving for late hits
    double earlyScale = delta < 0 ? 1.0 : 1.2;

    if (absDelta <= 0.035 * earlyScale) return Judgment.Perfect;
    if (absDelta <= 0.070 * earlyScale) return Judgment.Great;
    if (absDelta <= 0.110 * earlyScale) return Judgment.Good;
    return Judgment.Miss;
}
```

## Hit Detection Flow

```
Player swings controller
  -> Controller hitbox overlaps note trigger collider (OnTriggerEnter)
  -> HitDetector records hit time from AudioManager.SongTime
  -> HitDetector asks NoteSpawner for the closest hittable note in this lane
  -> JudgmentSystem compares hit time vs note time
  -> ScoreManager updates score and combo
  -> NoteObject plays hit VFX + haptic feedback
  -> NoteObject returned to pool
```

### HitDetector Script

```csharp
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MyGame.RhythmCore
{
    public class HitDetector : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private int _lane;
        [SerializeField] private ActionBasedController _controller;

        [Header("References")]
        [SerializeField] private NoteSpawner _spawner;

        [Header("Haptics")]
        [SerializeField] private float _hapticAmplitude = 0.5f;
        [SerializeField] private float _hapticDuration = 0.05f;

        // Event for UI / effects
        public event System.Action<Judgment, NoteData> OnNoteJudged;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Note")) return;

            double hitTime = AudioManager.Instance.SongTime;
            NoteObject note = _spawner.GetHittableNote(_lane, hitTime);

            if (note == null) return; // No hittable note in this lane

            Judgment judgment = HitWindows.Judge(hitTime, note.Data.Time);

            if (judgment == Judgment.Miss) return; // Too far off

            // Register the hit
            note.OnHit(judgment);
            _spawner.ReturnNote(note);

            // Haptic feedback
            if (_controller != null)
                _controller.SendHapticImpulse(_hapticAmplitude, _hapticDuration);

            OnNoteJudged?.Invoke(judgment, note.Data);
        }
    }
}
```

## Scoring System

### Base Score Per Judgment

| Judgment | Base Score | Combo Multiplier |
|----------|-----------|-----------------|
| Perfect | 300 | Yes |
| Great | 200 | Yes |
| Good | 100 | Yes |
| Miss | 0 | Resets combo |

### Combo Multiplier

The combo multiplier rewards sustained accuracy:

```
finalScore = baseScore * (1 + combo / 10)
```

At combo 10: 2x multiplier. At combo 50: 6x. At combo 100: 11x.

### Accuracy Calculation

```
accuracy = (perfect * 1.0 + great * 0.66 + good * 0.33) / totalNotes
```

### Letter Grade Thresholds

| Grade | Accuracy |
|-------|---------|
| S | >= 95% |
| A | >= 90% |
| B | >= 80% |
| C | >= 70% |
| D | >= 60% |
| F | < 60% |

## Results Screen Data

After a song ends, display:
- Final score
- Max combo
- Accuracy percentage
- Letter grade
- Per-judgment counts (Perfect: X, Great: X, Good: X, Miss: X)
- Whether it is a new high score

Store high scores per chart + difficulty in PlayerPrefs or a save file:

```csharp
string key = $"HighScore_{chartId}_{difficulty}";
int highScore = PlayerPrefs.GetInt(key, 0);
if (score > highScore)
{
    PlayerPrefs.SetInt(key, score);
    PlayerPrefs.Save();
}
```

## Anti-Spam Protection

Prevent players from just wildly swinging to hit everything:

1. **One hit per note**: Once a note is hit, remove it from the hittable pool immediately.
2. **Cooldown per lane**: After a hit, ignore further hits in the same lane for ~100ms.
3. **Penalty for empty swings** (optional): Deduct score or break combo for swinging when no note is present.

```csharp
private double _lastHitTime;
private const double HitCooldown = 0.1; // 100ms

private void OnTriggerEnter(Collider other)
{
    double hitTime = AudioManager.Instance.SongTime;
    if (hitTime - _lastHitTime < HitCooldown) return;
    _lastHitTime = hitTime;
    // ... proceed with hit detection
}
```
