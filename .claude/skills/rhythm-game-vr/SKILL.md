---
name: rhythm-game-vr
description: >
  Audio-based VR rhythm game development in Unity. Use this skill whenever the user
  mentions rhythm game, beat game, music game, note charts, audio sync, beat mapping,
  hit detection, hit windows, timing accuracy, audio latency, DSP time, AudioSettings,
  note spawning, note highways, score systems, combo mechanics, beat saber-style gameplay,
  taiko-style drums, or any game where player actions are synchronized to music. Also
  trigger when the user discusses AudioSource management, audio mixing, beat detection,
  BPM, tempo, song charts, note patterns, VR controller hit zones, haptic feedback on
  beat, or visual-audio synchronization. Covers the AudioManager singleton, DSP-based
  timing, note data structures, hit detection with configurable windows, scoring/combo
  systems, VR-specific hitbox design, and performance considerations for audio-driven
  games on Quest. This skill is tuned for Unity 6000.4.0f1 with OpenXR Meta Quest but
  the patterns apply to any VR rhythm game.
---

# VR Rhythm Game Development

Target: **Unity 6000.4.0f1** on **Meta Quest** via OpenXR.

Rhythm games live or die on audio-visual synchronization. A 20ms timing error is perceptible. A 50ms error makes the game feel broken. This skill provides the architecture and patterns to keep everything tight.

## When to Read Reference Files

| Topic | File | When |
|-------|------|------|
| Audio system and timing | `references/audio-timing.md` | Audio playback, DSP sync, latency |
| Note system and charts | `references/note-system.md` | Note data, spawning, scrolling |
| Hit detection and scoring | `references/hit-scoring.md` | Hit windows, judgment, combos, scoring |
| VR-specific hitbox design | `references/vr-hitboxes.md` | Controller hit zones, haptics, spatial notes |

---

## Architecture Overview

```
AudioManager (Singleton)
  - Owns the AudioSource for the song
  - Provides SongTime (DSP-accurate current position)
  - Handles calibration offset

ChartManager
  - Loads NoteChart data (ScriptableObject or JSON)
  - Feeds notes to the NoteSpawner based on SongTime

NoteSpawner
  - Instantiates/pools note GameObjects
  - Positions notes on the "highway" based on time-to-hit

NoteObject (per note)
  - Moves along the highway
  - Handles its own visual state (approaching, hittable, missed)

HitDetector (per hand/controller)
  - Checks overlap between controller hitbox and note hitboxes
  - Reports hits to the JudgmentSystem

JudgmentSystem
  - Compares hit time vs note time
  - Determines judgment (Perfect, Great, Good, Miss)
  - Feeds results to ScoreManager

ScoreManager
  - Tracks score, combo, max combo, accuracy
  - Broadcasts events for UI updates
```

## The Golden Rule: Use DSP Time, Not deltaTime

`Time.time` and `Time.deltaTime` are frame-based and accumulate drift. Audio playback runs on a separate clock (the DSP clock). For rhythm games, all timing must derive from `AudioSettings.dspTime`.

```csharp
// The only reliable way to know "where in the song are we?"
public double SongTime => AudioSettings.dspTime - _songStartDspTime + _calibrationOffset;
```

Use `double` for all timing values. `float` loses precision after ~3 minutes of playback.

## Quick Start: Minimal AudioManager

```csharp
using UnityEngine;

namespace MyGame.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private double _calibrationOffsetSeconds;

        private double _songStartDspTime;
        private bool _isPlaying;

        /// <summary>
        /// Current position in the song, in seconds. DSP-accurate.
        /// </summary>
        public double SongTime
        {
            get
            {
                if (!_isPlaying) return 0.0;
                return AudioSettings.dspTime - _songStartDspTime
                       + _calibrationOffsetSeconds;
            }
        }

        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Start playing the song with a countdown delay.
        /// </summary>
        public void PlaySong(AudioClip clip, double delaySeconds = 3.0)
        {
            _musicSource.clip = clip;
            _songStartDspTime = AudioSettings.dspTime + delaySeconds;
            _musicSource.PlayScheduled(_songStartDspTime);
            _isPlaying = true;
        }

        public void StopSong()
        {
            _musicSource.Stop();
            _isPlaying = false;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
```

## Note Data Structure

```csharp
using System;

namespace MyGame.RhythmCore
{
    [Serializable]
    public struct NoteData
    {
        /// <summary>Time in seconds when this note should be hit.</summary>
        public double Time;

        /// <summary>Which lane or position (0 = left, 1 = right, etc.).</summary>
        public int Lane;

        /// <summary>Type of note (tap, hold, swipe, etc.).</summary>
        public NoteType Type;

        /// <summary>Duration for hold notes, in seconds. 0 for taps.</summary>
        public double Duration;
    }

    public enum NoteType
    {
        Tap = 0,
        Hold = 1,
        Swipe = 2
    }
}
```

## Hit Windows

```csharp
namespace MyGame.RhythmCore
{
    public enum Judgment
    {
        Perfect,
        Great,
        Good,
        Miss
    }

    public static class HitWindows
    {
        // All values in seconds (half-window, +/-)
        public const double Perfect = 0.035; // +/- 35ms
        public const double Great   = 0.070; // +/- 70ms
        public const double Good    = 0.110; // +/- 110ms
        public const double Miss    = 0.150; // +/- 150ms (beyond this = auto-miss)

        public static Judgment Judge(double hitTime, double noteTime)
        {
            double delta = System.Math.Abs(hitTime - noteTime);
            if (delta <= Perfect) return Judgment.Perfect;
            if (delta <= Great)   return Judgment.Great;
            if (delta <= Good)    return Judgment.Good;
            return Judgment.Miss;
        }
    }
}
```

## Scoring Formula

```csharp
namespace MyGame.RhythmCore
{
    public class ScoreManager
    {
        public int Score { get; private set; }
        public int Combo { get; private set; }
        public int MaxCombo { get; private set; }
        public int PerfectCount { get; private set; }
        public int GreatCount { get; private set; }
        public int GoodCount { get; private set; }
        public int MissCount { get; private set; }
        public int TotalNotes { get; private set; }

        private const int BaseScorePerfect = 300;
        private const int BaseScoreGreat   = 200;
        private const int BaseScoreGood    = 100;

        public void RegisterHit(Judgment judgment)
        {
            TotalNotes++;
            switch (judgment)
            {
                case Judgment.Perfect:
                    PerfectCount++;
                    Combo++;
                    Score += BaseScorePerfect * (1 + Combo / 10);
                    break;
                case Judgment.Great:
                    GreatCount++;
                    Combo++;
                    Score += BaseScoreGreat * (1 + Combo / 10);
                    break;
                case Judgment.Good:
                    GoodCount++;
                    Combo++;
                    Score += BaseScoreGood * (1 + Combo / 10);
                    break;
                case Judgment.Miss:
                    MissCount++;
                    Combo = 0;
                    break;
            }
            if (Combo > MaxCombo) MaxCombo = Combo;
        }

        public float Accuracy
        {
            get
            {
                if (TotalNotes == 0) return 0f;
                float weighted = (PerfectCount * 1f)
                               + (GreatCount * 0.66f)
                               + (GoodCount * 0.33f);
                return weighted / TotalNotes;
            }
        }

        public void Reset()
        {
            Score = 0;
            Combo = 0;
            MaxCombo = 0;
            PerfectCount = 0;
            GreatCount = 0;
            GoodCount = 0;
            MissCount = 0;
            TotalNotes = 0;
        }
    }
}
```

## Output Guidelines

1. All timing values use `double`, never `float`.
2. All timing derives from `AudioSettings.dspTime`, never `Time.time`.
3. Note objects are pooled, never Instantiated/Destroyed mid-song.
4. Hit detection uses physics overlap (trigger colliders), not raycasts.
5. Haptic feedback is sent on every successful hit (short pulse, 0.05-0.1s).
6. Visual feedback (particle burst, color flash) triggers on judgment, not on physics contact.
7. Generate complete scripts with `using` directives and namespaces.
8. Save `.cs` files to `/mnt/user-data/outputs/`.
