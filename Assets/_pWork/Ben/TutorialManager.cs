using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Tutorial scene controller. Walks the player through each character
/// in the current tier, then offers the option to start that tier's level.
///
/// Flow:
/// 1. Show a character (kana) and its romaji side by side as introduction
/// 2. Player confirms they're ready (via button/trigger)
/// 3. Quiz the player on that character mixed with previously learned ones
/// 4. After all characters in the tier are practiced, mark tier tutorial complete
/// 5. Fire OnTutorialComplete so UI can show "Start Level" button
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DictionaryManager _dictionaryManager;
    [SerializeField] private TextMeshProUGUI _promptText;
    [SerializeField] private TextMeshProUGUI _kanaDisplayText;
    [SerializeField] private TextMeshProUGUI _romajiDisplayText;
    [SerializeField] private TextMeshProUGUI _progressText;

    [Header("Tutorial Settings")]
    [Tooltip("Overridden at runtime by ActiveTierSelection if set.")]
    [SerializeField] private int _tier = 0;
    [SerializeField] private int _practiceRoundsPerCharacter = 3;

    [Header("Events")]
    public UnityEvent OnTutorialComplete;

    private KanaDatabase.KanaEntry[] _tierEntries;
    private int _currentCharIndex;
    private int _practiceCount;
    private bool _inIntroPhase;
    private bool _tutorialFinished;

    private void Start()
    {
        // Use the tier selected from the tier select screen if available
        _tier = ActiveTierSelection.SelectedTier;
        _tierEntries = KanaDatabase.GetTierEntries(_tier);

        if (_tierEntries.Length == 0)
        {
            Debug.LogError("TutorialManager: No entries for tier " + _tier);
            return;
        }

        _currentCharIndex = 0;
        _tutorialFinished = false;

        // Load this tier into DictionaryManager so it uses the right character set
        _dictionaryManager.LoadTier(_tier);

        ShowIntro();
    }

    /// <summary>
    /// Show the current character and its romaji as an introduction.
    /// Call ConfirmIntro() when the player is ready to practice.
    /// </summary>
    private void ShowIntro()
    {
        _inIntroPhase = true;
        _practiceCount = 0;

        KanaDatabase.KanaEntry entry = _tierEntries[_currentCharIndex];

        if (_kanaDisplayText != null)
            _kanaDisplayText.text = entry.kana;

        if (_romajiDisplayText != null)
            _romajiDisplayText.text = entry.romaji;

        if (_promptText != null)
            _promptText.text = "New character! Study it, then tap to practice.";

        UpdateProgressText();
    }

    /// <summary>
    /// Call from a UI button or XR interaction to move from intro to practice.
    /// </summary>
    public void ConfirmIntro()
    {
        if (!_inIntroPhase || _tutorialFinished) return;

        _inIntroPhase = false;

        // Hide intro display
        if (_kanaDisplayText != null)
            _kanaDisplayText.text = "";

        if (_romajiDisplayText != null)
            _romajiDisplayText.text = "";

        if (_promptText != null)
            _promptText.text = "Find the correct kana!";

        // Build the practice pool: all characters learned so far (0..currentCharIndex)
        List<KanaDatabase.KanaEntry> practicePool = new List<KanaDatabase.KanaEntry>();
        for (int i = 0; i <= _currentCharIndex; i++)
        {
            practicePool.Add(_tierEntries[i]);
        }
        _dictionaryManager.LoadEntries(practicePool);
        _dictionaryManager.GenerateQuestion();
    }

    /// <summary>
    /// Called by collision/game logic when the player answers correctly during tutorial.
    /// Tracks practice rounds and advances to the next character when ready.
    /// </summary>
    public void OnCorrectAnswer()
    {
        if (_tutorialFinished) return;

        _practiceCount++;

        if (_practiceCount >= _practiceRoundsPerCharacter)
        {
            _currentCharIndex++;

            if (_currentCharIndex >= _tierEntries.Length)
            {
                CompleteTutorial();
            }
            else
            {
                ShowIntro();
            }
        }
        else
        {
            _dictionaryManager.GenerateQuestion();
        }
    }

    /// <summary>
    /// Called when the player answers incorrectly. Tutorial continues from same state.
    /// </summary>
    public void OnIncorrectAnswer()
    {
        // Don't advance practice count, just generate a new question
        if (!_tutorialFinished && !_inIntroPhase)
        {
            _dictionaryManager.GenerateQuestion();
        }
    }

    private void CompleteTutorial()
    {
        _tutorialFinished = true;
        TierProgress.CompleteTier(_tier);

        if (_promptText != null)
            _promptText.text = "Tutorial complete! Ready to start Level " + (_tier + 1) + "?";

        if (_kanaDisplayText != null)
            _kanaDisplayText.text = "";

        if (_romajiDisplayText != null)
            _romajiDisplayText.text = "";

        OnTutorialComplete?.Invoke();
    }

    private void UpdateProgressText()
    {
        if (_progressText != null)
            _progressText.text = (_currentCharIndex + 1) + " / " + _tierEntries.Length;
    }
}
