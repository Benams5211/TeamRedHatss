using System;
using System.Collections.Generic;

/// <summary>
/// Bit flags for tier completion. Each tier is a single bit.
/// Use bitwise ops to check/set: (flags & TierFlags.Vowels) != 0
/// </summary>
[Flags]
public enum TierFlags
{
    None       = 0,
    Vowels     = 1 << 0, // Tier 0
    Consonants = 1 << 1, // Tier 1
    Words      = 1 << 2, // Tier 2
    Katakana   = 1 << 3, // Tier 3
    All        = Vowels | Consonants | Words | Katakana,
}

/// <summary>
/// Central source of truth for all kana/word data organized by tier.
/// Tier 0: Vowels (あいうえお)
/// Tier 1: Consonants (ka-row through wa-row)
/// Tier 2: Simple words
/// </summary>
public static class KanaDatabase
{
    /// <summary>
    /// Convert a tier index (0, 1, 2...) to its TierFlags bit.
    /// </summary>
    public static TierFlags TierIndexToFlag(int index) => (TierFlags)(1 << index);

    public struct KanaEntry
    {
        public string kana;
        public string romaji;

        public KanaEntry(string kana, string romaji)
        {
            this.kana = kana;
            this.romaji = romaji;
        }
    }

    public struct Tier
    {
        public string name;
        public KanaEntry[] entries;

        public Tier(string name, KanaEntry[] entries)
        {
            this.name = name;
            this.entries = entries;
        }
    }

    public static readonly Tier[] Tiers = new Tier[]
    {
        // --- Tier 1: Vowels ---
        new Tier("Vowels", new KanaEntry[]
        {
            new KanaEntry("あ", "a"),
            new KanaEntry("い", "i"),
            new KanaEntry("う", "u"),
            new KanaEntry("え", "e"),
            new KanaEntry("お", "o"),
        }),

        // --- Tier 2: Consonants ---
        new Tier("Consonants", new KanaEntry[]
        {
            // Ka-row
            new KanaEntry("か", "ka"),
            new KanaEntry("き", "ki"),
            new KanaEntry("く", "ku"),
            new KanaEntry("け", "ke"),
            new KanaEntry("こ", "ko"),
            // Sa-row
            new KanaEntry("さ", "sa"),
            new KanaEntry("し", "shi"),
            new KanaEntry("す", "su"),
            new KanaEntry("せ", "se"),
            new KanaEntry("そ", "so"),
            // Ta-row
            new KanaEntry("た", "ta"),
            new KanaEntry("ち", "chi"),
            new KanaEntry("つ", "tsu"),
            new KanaEntry("て", "te"),
            new KanaEntry("と", "to"),
            // Na-row
            new KanaEntry("な", "na"),
            new KanaEntry("に", "ni"),
            new KanaEntry("ぬ", "nu"),
            new KanaEntry("ね", "ne"),
            new KanaEntry("の", "no"),
            // Ha-row
            new KanaEntry("は", "ha"),
            new KanaEntry("ひ", "hi"),
            new KanaEntry("ふ", "fu"),
            new KanaEntry("へ", "he"),
            new KanaEntry("ほ", "ho"),
            // Ma-row
            new KanaEntry("ま", "ma"),
            new KanaEntry("み", "mi"),
            new KanaEntry("む", "mu"),
            new KanaEntry("め", "me"),
            new KanaEntry("も", "mo"),
            // Ya-row
            new KanaEntry("や", "ya"),
            new KanaEntry("ゆ", "yu"),
            new KanaEntry("よ", "yo"),
            // Ra-row
            new KanaEntry("ら", "ra"),
            new KanaEntry("り", "ri"),
            new KanaEntry("る", "ru"),
            new KanaEntry("れ", "re"),
            new KanaEntry("ろ", "ro"),
            // Wa-row
            new KanaEntry("わ", "wa"),
            new KanaEntry("を", "wo"),
            new KanaEntry("ん", "n"),
        }),

        // --- Tier 2: Simple Words ---
        new Tier("Simple Words", new KanaEntry[]
        {
            new KanaEntry("いぬ", "inu"),       // dog
            new KanaEntry("ねこ", "neko"),       // cat
            new KanaEntry("さかな", "sakana"),   // fish
            new KanaEntry("みず", "mizu"),       // water
            new KanaEntry("やま", "yama"),       // mountain
            new KanaEntry("かわ", "kawa"),       // river
            new KanaEntry("そら", "sora"),       // sky
            new KanaEntry("ひと", "hito"),       // person
            new KanaEntry("て", "te"),           // hand
            new KanaEntry("め", "me"),           // eye
        }),

        // --- Tier 3: Katakana ---
        new Tier("Katakana", new KanaEntry[]
        {
            // Vowels
            new KanaEntry("ア", "a"),
            new KanaEntry("イ", "i"),
            new KanaEntry("ウ", "u"),
            new KanaEntry("エ", "e"),
            new KanaEntry("オ", "o"),
            // Ka-row
            new KanaEntry("カ", "ka"),
            new KanaEntry("キ", "ki"),
            new KanaEntry("ク", "ku"),
            new KanaEntry("ケ", "ke"),
            new KanaEntry("コ", "ko"),
            // Sa-row
            new KanaEntry("サ", "sa"),
            new KanaEntry("シ", "shi"),
            new KanaEntry("ス", "su"),
            new KanaEntry("セ", "se"),
            new KanaEntry("ソ", "so"),
            // Ta-row
            new KanaEntry("タ", "ta"),
            new KanaEntry("チ", "chi"),
            new KanaEntry("ツ", "tsu"),
            new KanaEntry("テ", "te"),
            new KanaEntry("ト", "to"),
            // Na-row
            new KanaEntry("ナ", "na"),
            new KanaEntry("ニ", "ni"),
            new KanaEntry("ヌ", "nu"),
            new KanaEntry("ネ", "ne"),
            new KanaEntry("ノ", "no"),
            // Ha-row
            new KanaEntry("ハ", "ha"),
            new KanaEntry("ヒ", "hi"),
            new KanaEntry("フ", "fu"),
            new KanaEntry("ヘ", "he"),
            new KanaEntry("ホ", "ho"),
            // Ma-row
            new KanaEntry("マ", "ma"),
            new KanaEntry("ミ", "mi"),
            new KanaEntry("ム", "mu"),
            new KanaEntry("メ", "me"),
            new KanaEntry("モ", "mo"),
            // Ya-row
            new KanaEntry("ヤ", "ya"),
            new KanaEntry("ユ", "yu"),
            new KanaEntry("ヨ", "yo"),
            // Ra-row
            new KanaEntry("ラ", "ra"),
            new KanaEntry("リ", "ri"),
            new KanaEntry("ル", "ru"),
            new KanaEntry("レ", "re"),
            new KanaEntry("ロ", "ro"),
            // Wa-row
            new KanaEntry("ワ", "wa"),
            new KanaEntry("ヲ", "wo"),
            new KanaEntry("ン", "n"),
        }),
    };

    public static int TierCount => Tiers.Length;

    /// <summary>
    /// Get all entries for a single tier (0-indexed).
    /// </summary>
    public static KanaEntry[] GetTierEntries(int tier)
    {
        if (tier < 0 || tier >= Tiers.Length)
            return new KanaEntry[0];
        return Tiers[tier].entries;
    }

    /// <summary>
    /// Get combined entries for tiers 0 through tierInclusive.
    /// Useful for gameplay that includes all unlocked content.
    /// </summary>
    public static List<KanaEntry> GetEntriesUpToTier(int tierInclusive)
    {
        List<KanaEntry> result = new List<KanaEntry>();
        for (int i = 0; i <= tierInclusive && i < Tiers.Length; i++)
        {
            result.AddRange(Tiers[i].entries);
        }
        return result;
    }

    /// <summary>
    /// Get combined entries for all tiers matching the given flags.
    /// Example: GetEntries(TierFlags.Vowels | TierFlags.Consonants)
    /// </summary>
    public static List<KanaEntry> GetEntries(TierFlags flags)
    {
        List<KanaEntry> result = new List<KanaEntry>();
        for (int i = 0; i < Tiers.Length; i++)
        {
            if ((flags & TierIndexToFlag(i)) != 0)
                result.AddRange(Tiers[i].entries);
        }
        return result;
    }

    /// <summary>
    /// Build a kana-to-romaji dictionary from a list of entries.
    /// </summary>
    public static Dictionary<string, string> ToDictionary(IEnumerable<KanaEntry> entries)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        foreach (KanaEntry e in entries)
        {
            dict[e.kana] = e.romaji;
        }
        return dict;
    }
}
