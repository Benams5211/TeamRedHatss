using UnityEngine;

/// <summary>
/// Persistent tier completion tracking via a single bit flag int in PlayerPrefs.
/// Each bit = one tier's completion state. Supports up to 32 tiers.
/// </summary>
public static class TierProgress
{
    private const string FlagsKey = "TierCompletionFlags";
    private const string ScorePrefix = "Tier_BestScore_";

    public static TierFlags CompletedFlags
    {
        get => (TierFlags)PlayerPrefs.GetInt(FlagsKey, 0);
        private set
        {
            PlayerPrefs.SetInt(FlagsKey, (int)value);
            PlayerPrefs.Save();
        }
    }

    public static bool IsTierCompleted(int tier)
    {
        return (CompletedFlags & KanaDatabase.TierIndexToFlag(tier)) != 0;
    }

    public static bool IsTierCompleted(TierFlags tier)
    {
        return (CompletedFlags & tier) == tier;
    }

    public static void CompleteTier(int tier)
    {
        CompletedFlags |= KanaDatabase.TierIndexToFlag(tier);
    }

    public static void CompleteTier(TierFlags tier)
    {
        CompletedFlags |= tier;
    }

    /// <summary>
    /// Returns the highest tier index the player has unlocked.
    /// Tier 0 is always unlocked. Completing tier N unlocks tier N+1.
    /// </summary>
    public static int GetHighestUnlockedTier()
    {
        TierFlags flags = CompletedFlags;
        for (int i = 0; i < KanaDatabase.TierCount; i++)
        {
            if ((flags & KanaDatabase.TierIndexToFlag(i)) == 0)
                return i;
        }
        return KanaDatabase.TierCount - 1;
    }

    /// <summary>
    /// Returns TierFlags for all unlocked tiers (completed + the next one).
    /// </summary>
    public static TierFlags GetUnlockedFlags()
    {
        TierFlags flags = CompletedFlags;
        int next = GetHighestUnlockedTier();
        flags |= KanaDatabase.TierIndexToFlag(next);
        return flags;
    }

    public static int GetBestScore(int tier)
    {
        return PlayerPrefs.GetInt(ScorePrefix + tier, 0);
    }

    public static void SetBestScore(int tier, int score)
    {
        if (score > GetBestScore(tier))
        {
            PlayerPrefs.SetInt(ScorePrefix + tier, score);
            PlayerPrefs.Save();
        }
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(FlagsKey);
        for (int i = 0; i < KanaDatabase.TierCount; i++)
            PlayerPrefs.DeleteKey(ScorePrefix + i);
        PlayerPrefs.Save();
    }
}
