# Note System and Charts

## Chart Data Format

A chart defines all the notes in a song. Store it as a ScriptableObject for Editor convenience or as JSON for runtime loading.

### ScriptableObject Chart

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.RhythmCore
{
    [CreateAssetMenu(fileName = "NewChart", menuName = "Rhythm/Note Chart")]
    public class NoteChart : ScriptableObject
    {
        [Header("Song Info")]
        public string SongName;
        public AudioClip SongClip;
        public double BPM;
        public double AudioOffset; // seconds before first beat

        [Header("Difficulty")]
        public string DifficultyName; // e.g. "kantan", "futsuu", "muzukashii"
        public int DifficultyLevel;   // 1-10

        [Header("Notes")]
        public List<NoteData> Notes = new();

        /// <summary>
        /// Returns notes sorted by time. Call once at chart load.
        /// </summary>
        public List<NoteData> GetSortedNotes()
        {
            var sorted = new List<NoteData>(Notes);
            sorted.Sort((a, b) => a.Time.CompareTo(b.Time));
            return sorted;
        }
    }
}
```

### JSON Chart Format

For charts loaded at runtime or created by external tools:

```json
{
    "songName": "いにしえ の りずむ",
    "bpm": 130.0,
    "audioOffset": 0.25,
    "difficultyName": "むずかしい",
    "difficultyLevel": 7,
    "notes": [
        { "time": 1.0,  "lane": 0, "type": 0, "duration": 0.0 },
        { "time": 1.5,  "lane": 1, "type": 0, "duration": 0.0 },
        { "time": 2.0,  "lane": 0, "type": 1, "duration": 0.5 },
        { "time": 2.75, "lane": 1, "type": 2, "duration": 0.0 }
    ]
}
```

Parse with `JsonUtility.FromJson<ChartJson>(jsonText)` or use a serializable wrapper class.

## Note Spawning

Notes should be spawned ahead of time and scrolled toward the player. The "look-ahead" window determines how far ahead notes appear.

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.RhythmCore
{
    public class NoteSpawner : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float _lookAheadSeconds = 2f;
        [SerializeField] private float _scrollSpeed = 5f;
        [SerializeField] private Transform[] _laneOrigins;
        [SerializeField] private Transform _hitLine;

        [Header("Pooling")]
        [SerializeField] private NoteObject _notePrefab;
        [SerializeField] private int _poolSize = 32;

        private List<NoteData> _notes;
        private int _nextNoteIndex;
        private readonly Queue<NoteObject> _pool = new();
        private readonly List<NoteObject> _activeNotes = new();

        public void Initialize(List<NoteData> sortedNotes)
        {
            _notes = sortedNotes;
            _nextNoteIndex = 0;

            // Pre-warm pool
            for (int i = 0; i < _poolSize; i++)
            {
                var obj = Instantiate(_notePrefab, transform);
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        private void Update()
        {
            if (_notes == null) return;
            double songTime = AudioManager.Instance.SongTime;

            // Spawn upcoming notes
            while (_nextNoteIndex < _notes.Count
                   && _notes[_nextNoteIndex].Time <= songTime + _lookAheadSeconds)
            {
                SpawnNote(_notes[_nextNoteIndex]);
                _nextNoteIndex++;
            }

            // Update active notes
            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                var note = _activeNotes[i];
                double delta = note.Data.Time - songTime;

                // Position based on time delta
                float zPos = (float)(delta * _scrollSpeed);
                var lane = _laneOrigins[note.Data.Lane];
                note.transform.position = lane.position
                    + lane.forward * zPos;

                // Auto-miss if past the miss window
                if (delta < -HitWindows.Miss)
                {
                    note.OnMissed();
                    ReturnNote(note);
                    _activeNotes.RemoveAt(i);
                }
            }
        }

        private void SpawnNote(NoteData data)
        {
            NoteObject obj = _pool.Count > 0
                ? _pool.Dequeue()
                : Instantiate(_notePrefab, transform);

            obj.Initialize(data);
            obj.gameObject.SetActive(true);
            _activeNotes.Add(obj);
        }

        public void ReturnNote(NoteObject obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }

        /// <summary>
        /// Find the closest active note in the given lane within the hit window.
        /// </summary>
        public NoteObject GetHittableNote(int lane, double songTime)
        {
            NoteObject closest = null;
            double closestDelta = double.MaxValue;

            foreach (var note in _activeNotes)
            {
                if (note.Data.Lane != lane) continue;
                double delta = System.Math.Abs(note.Data.Time - songTime);
                if (delta <= HitWindows.Miss && delta < closestDelta)
                {
                    closest = note;
                    closestDelta = delta;
                }
            }
            return closest;
        }
    }
}
```

## NoteObject Component

```csharp
using UnityEngine;

namespace MyGame.RhythmCore
{
    public class NoteObject : MonoBehaviour
    {
        public NoteData Data { get; private set; }

        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _normalMaterial;
        [SerializeField] private Material _hittableMaterial;

        public void Initialize(NoteData data)
        {
            Data = data;
            _renderer.material = _normalMaterial;
        }

        public void OnHit(Judgment judgment)
        {
            // Trigger hit VFX (particle burst, scale pop, etc.)
            // The NoteSpawner will return this to the pool
        }

        public void OnMissed()
        {
            // Trigger miss VFX (fade out, red flash, etc.)
        }
    }
}
```

## Chart Authoring Tips

For hackathon-speed chart creation:
1. Use Audacity to find the BPM and first-beat offset of your song.
2. Define notes at beat positions: `time = audioOffset + beatNumber * (60.0 / bpm)`.
3. Start simple: notes on beats 1 and 3, then add subdivisions.
4. Playtest frequently. If it does not feel right, adjust the audio offset by 10-20ms.
5. For variety, alternate lanes and mix in hold notes on longer musical phrases.

## Scroll Speed and Approach Rate

Scroll speed controls how fast notes approach. Higher speed = less visual clutter but requires faster reactions.

| Difficulty | Scroll Speed Multiplier | Look-Ahead |
|-----------|------------------------|------------|
| Easy | 1.0x | 3.0s |
| Normal | 1.5x | 2.0s |
| Hard | 2.0x | 1.5s |
| Expert | 3.0x | 1.0s |

Let players adjust scroll speed in settings -- it is a personal preference.
