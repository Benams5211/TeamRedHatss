# Audio System and Timing

## Why DSP Time Matters

Unity runs two independent clocks:
1. **Frame clock** (`Time.time`, `Time.deltaTime`) -- ticks once per rendered frame. Frame rate varies.
2. **DSP clock** (`AudioSettings.dspTime`) -- ticks at the audio sample rate (typically 44100 or 48000 Hz). Rock-solid timing independent of frame rate.

If you move notes using `Time.deltaTime`, they drift relative to the audio. After 30 seconds of a song, you can accumulate 50-100ms of error. This makes hits feel wrong even when the player's timing is correct.

Rule: **All gameplay timing reads from AudioSettings.dspTime. Period.**

## AudioSettings Configuration

### Sample Rate
Quest default is 48000 Hz. Do not change unless you have a specific reason.

### DSP Buffer Size
Controls audio latency. Smaller buffer = lower latency but higher CPU cost.

```csharp
// Set before any AudioSource plays
AudioSettings.SetDSPBufferSize(256, 4);
// 256 samples at 48000 Hz = ~5.3ms latency per buffer
// 4 buffers = ~21ms total output latency
```

For rhythm games, use the smallest buffer your target device handles without crackling:
- Quest 2: 256-512 samples is usually safe.
- Quest 3: 256 samples.

Test on-device. The Editor may handle smaller buffers than the Quest.

### Calibration Offset

Even with DSP time, there is a pipeline delay between when audio leaves the DSP and when the player hears it (DAC, amplifier, headphone driver). This is device-specific and cannot be measured programmatically.

Provide a calibration screen:
1. Play a metronome click track.
2. Ask the player to tap on each beat.
3. Average the delta between their taps and the expected beat times.
4. Store this offset and apply it to `SongTime`.

```csharp
public double SongTime =>
    AudioSettings.dspTime - _songStartDspTime + _calibrationOffset;
```

A typical Quest offset is 0-30ms. Some Bluetooth headphones add 100-200ms (warn users to use wired audio).

## AudioSource Setup

```csharp
[SerializeField] private AudioSource _musicSource;

void ConfigureMusicSource()
{
    _musicSource.playOnAwake = false;
    _musicSource.loop = false;
    _musicSource.spatialBlend = 0f; // 2D (non-spatial) for music
    _musicSource.priority = 0;     // Highest priority
    _musicSource.volume = 1f;
}
```

### PlayScheduled for Precise Start

`AudioSource.Play()` starts on the next audio buffer boundary, which introduces up to one buffer of jitter. `PlayScheduled(dspTime)` starts at an exact DSP time.

```csharp
double startTime = AudioSettings.dspTime + 3.0; // 3 second countdown
_musicSource.PlayScheduled(startTime);
_songStartDspTime = startTime;
```

This guarantees the song starts at exactly `startTime`, and all subsequent `SongTime` calculations are accurate.

## Sound Effects

### Pooled AudioSources

Do not create a new AudioSource for every sound effect. Pool them.

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Audio
{
    public class SfxPool : MonoBehaviour
    {
        [SerializeField] private int _poolSize = 16;
        private readonly Queue<AudioSource> _pool = new();

        private void Awake()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                var go = new GameObject($"SFX_{i}");
                go.transform.SetParent(transform);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 1f; // 3D for positional SFX
                _pool.Enqueue(src);
            }
        }

        public void PlayOneShot(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (_pool.Count == 0) return; // All sources busy
            var src = _pool.Dequeue();
            src.transform.position = position;
            src.clip = clip;
            src.volume = volume;
            src.Play();
            // Return to pool after clip finishes
            StartCoroutine(ReturnAfterPlay(src));
        }

        private System.Collections.IEnumerator ReturnAfterPlay(AudioSource src)
        {
            yield return new WaitForSeconds(src.clip.length);
            _pool.Enqueue(src);
        }
    }
}
```

### Hit Sound Latency

Hit sounds must play with minimal latency. Pre-load all hit effect clips using **Decompress On Load** (see memory reference). Use `AudioSource.PlayOneShot()` for fire-and-forget playback.

For even lower latency, use a pre-allocated `AudioSource` per hand and swap clips:

```csharp
_leftHandSource.clip = _hitClip;
_leftHandSource.Play();
```

## Audio Clip Import Settings

| Clip Type | Format | Load Type | Compression |
|-----------|--------|-----------|-------------|
| Song/Music | Vorbis | Streaming | Quality 70% |
| Hit SFX | PCM or ADPCM | Decompress On Load | None or ADPCM |
| Miss SFX | ADPCM | Decompress On Load | ADPCM |
| UI sounds | ADPCM | Decompress On Load | ADPCM |
| Metronome (calibration) | PCM | Decompress On Load | None |

## Tempo and BPM

```csharp
public static class TempoUtils
{
    /// <summary>Seconds per beat at the given BPM.</summary>
    public static double SecondsPerBeat(double bpm) => 60.0 / bpm;

    /// <summary>Convert a beat number to seconds.</summary>
    public static double BeatToTime(double beat, double bpm, double offset)
        => offset + beat * SecondsPerBeat(bpm);

    /// <summary>Convert seconds to the current beat number.</summary>
    public static double TimeToBeat(double time, double bpm, double offset)
        => (time - offset) / SecondsPerBeat(bpm);
}
```
