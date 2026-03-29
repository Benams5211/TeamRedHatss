/// <summary>
/// Static state that carries the player's tier selection between scenes.
/// Set before loading a tutorial or gameplay scene, read on Start() in that scene.
/// </summary>
public static class ActiveTierSelection
{
    /// <summary>Which tier index was selected (0 = vowels, 1 = consonants, etc.)</summary>
    public static int SelectedTier { get; set; } = 0;

    /// <summary>Whether the player chose tutorial or gameplay for the selected tier.</summary>
    public static PlayMode Mode { get; set; } = PlayMode.Tutorial;

    public enum PlayMode
    {
        Tutorial,
        Gameplay,
    }
}
