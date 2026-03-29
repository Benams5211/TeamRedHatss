using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks per-character performance data across tiers.
/// Records: correct/incorrect counts, total time for each, fastest correct time.
/// All stats are keyed by the character being asked (not what was pressed).
/// Persisted as a single JSON blob in PlayerPrefs.
/// </summary>
public class KanaStatsTracker : MonoBehaviour
{
    private const string PrefsKey = "KanaStats";

    [Serializable]
    public class CharacterStats
    {
        public int correctCount;
        public int incorrectCount;
        public float totalCorrectTime;
        public float totalIncorrectTime;
        public float fastestCorrectTime = float.MaxValue;

        public float AverageCorrectTime => correctCount > 0 ? totalCorrectTime / correctCount : 0f;
        public float AverageIncorrectTime => incorrectCount > 0 ? totalIncorrectTime / incorrectCount : 0f;
        public float Accuracy => (correctCount + incorrectCount) > 0
            ? (float)correctCount / (correctCount + incorrectCount)
            : 0f;
    }

    [Serializable]
    private class StatsEntry
    {
        public string key;
        public CharacterStats stats;
    }

    [Serializable]
    private class StatsData
    {
        public List<StatsEntry> entries = new List<StatsEntry>();
    }

    private Dictionary<string, CharacterStats> _stats = new Dictionary<string, CharacterStats>();

    private static KanaStatsTracker _instance;
    public static KanaStatsTracker Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    /// <summary>
    /// Record an answer for the character that was being asked about.
    /// </summary>
    /// <param name="tier">Which tier the question came from</param>
    /// <param name="kana">The kana/word the player was trying to identify</param>
    /// <param name="correct">Whether they chose the right answer</param>
    /// <param name="responseTime">Seconds from question shown to answer selected</param>
    public void RecordAnswer(int tier, string kana, bool correct, float responseTime)
    {
        string key = tier + "_" + kana;

        if (!_stats.TryGetValue(key, out CharacterStats entry))
        {
            entry = new CharacterStats();
            _stats[key] = entry;
        }

        if (correct)
        {
            entry.correctCount++;
            entry.totalCorrectTime += responseTime;
            if (responseTime < entry.fastestCorrectTime)
                entry.fastestCorrectTime = responseTime;
        }
        else
        {
            entry.incorrectCount++;
            entry.totalIncorrectTime += responseTime;
        }

        Save();
    }

    /// <summary>
    /// Get stats for a specific character in a specific tier.
    /// Returns null if no data recorded yet.
    /// </summary>
    public CharacterStats GetStats(int tier, string kana)
    {
        string key = tier + "_" + kana;
        _stats.TryGetValue(key, out CharacterStats entry);
        return entry;
    }

    /// <summary>
    /// Get all recorded stats for a given tier.
    /// Returns kana -> CharacterStats pairs.
    /// </summary>
    public Dictionary<string, CharacterStats> GetTierStats(int tier)
    {
        Dictionary<string, CharacterStats> result = new Dictionary<string, CharacterStats>();
        string prefix = tier + "_";

        foreach (KeyValuePair<string, CharacterStats> kvp in _stats)
        {
            if (kvp.Key.StartsWith(prefix))
                result[kvp.Key.Substring(prefix.Length)] = kvp.Value;
        }

        return result;
    }

    /// <summary>
    /// Get aggregated totals across all tiers.
    /// </summary>
    public CharacterStats GetGlobalStats()
    {
        CharacterStats total = new CharacterStats();

        foreach (CharacterStats s in _stats.Values)
        {
            total.correctCount += s.correctCount;
            total.incorrectCount += s.incorrectCount;
            total.totalCorrectTime += s.totalCorrectTime;
            total.totalIncorrectTime += s.totalIncorrectTime;
            if (s.fastestCorrectTime < total.fastestCorrectTime)
                total.fastestCorrectTime = s.fastestCorrectTime;
        }

        return total;
    }

    public void ResetAll()
    {
        _stats.Clear();
        PlayerPrefs.DeleteKey(PrefsKey);
        PlayerPrefs.Save();
    }

    private void Save()
    {
        StatsData data = new StatsData();
        foreach (KeyValuePair<string, CharacterStats> kvp in _stats)
        {
            data.entries.Add(new StatsEntry { key = kvp.Key, stats = kvp.Value });
        }
        PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        _stats.Clear();
        string json = PlayerPrefs.GetString(PrefsKey, "");
        if (string.IsNullOrEmpty(json)) return;

        StatsData data = JsonUtility.FromJson<StatsData>(json);
        if (data?.entries == null) return;

        foreach (StatsEntry entry in data.entries)
        {
            _stats[entry.key] = entry.stats;
        }
    }
}
