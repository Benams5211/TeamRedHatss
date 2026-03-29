using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Tutorial scene controller. Auto-starts on scene load.
/// Pins each character as the correct answer for practiceRoundsPerCharacter
/// rounds, then advances to the next. No intro pause — transitions are seamless.
/// Supports tier selection via Inspector, ActiveTierSelection, or keyboard (1-4).
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DictionaryManager _dictionaryManager;
    [SerializeField] private TextMeshProUGUI _promptText;
    [SerializeField] private TextMeshProUGUI _kanaDisplayText;
    [SerializeField] private TextMeshProUGUI _romajiDisplayText;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private TextMeshProUGUI _tierNameText;

    [Header("Tutorial Settings")]
    [Tooltip("Overridden at runtime by ActiveTierSelection.")]
    [SerializeField] private int _tier = 0;
    [SerializeField] private int _practiceRoundsPerCharacter = 3;

    [Header("Events")]
    public UnityEvent OnTutorialComplete;

    private KanaDatabase.KanaEntry[] _tierEntries;
    private int _currentCharIndex;
    private int _practiceCount;
    private bool _tutorialFinished;

    private void Start()
    {
        _tier = ActiveTierSelection.SelectedTier;
        BeginTier(_tier);
    }

    /// <summary>
    /// Start or restart the tutorial for a given tier.
    /// </summary>
    public void BeginTier(int tier)
    {
        _tier = tier;
        _tierEntries = KanaDatabase.GetTierEntries(_tier);

        if (_tierEntries.Length == 0)
        {
            Debug.LogError("TutorialManager: No entries for tier " + _tier);
            return;
        }

        _currentCharIndex = 0;
        _tutorialFinished = false;

        if (_promptText != null)
            _promptText.text = "Find the correct kana!";

        if (_tierNameText != null)
            _tierNameText.text = KanaDatabase.Tiers[_tier].name;

        _dictionaryManager.LoadTier(_tier);
        StartPractice();
    }

    private void StartPractice()
    {
        _practiceCount = 0;

        KanaDatabase.KanaEntry entry = _tierEntries[_currentCharIndex];

        if (_kanaDisplayText != null)
            _kanaDisplayText.text = entry.kana;

        if (_romajiDisplayText != null)
            _romajiDisplayText.text = entry.romaji;

        List<KanaDatabase.KanaEntry> practicePool = new List<KanaDatabase.KanaEntry>();
        for (int i = 0; i <= _currentCharIndex; i++)
        {
            practicePool.Add(_tierEntries[i]);
        }
        _dictionaryManager.LoadEntries(practicePool);
        _dictionaryManager.PinCorrectAnswer(entry.kana);

        UpdateProgressText();
        _dictionaryManager.GenerateQuestion();
    }

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
                StartPractice();
            }
        }
        else
        {
            _dictionaryManager.GenerateQuestion();
        }
    }

    public void OnIncorrectAnswer()
    {
        if (!_tutorialFinished)
        {
            _dictionaryManager.GenerateQuestion();
        }
    }

    private void CompleteTutorial()
    {
        _tutorialFinished = true;
        _dictionaryManager.PinCorrectAnswer(null);
        _dictionaryManager.SpinToFindEmpty();
        TierProgress.CompleteTier(_tier);

        if (_promptText != null)
            _promptText.text = "Tutorial complete! Ready to start Level " + (_tier + 1) + "?";

        if (_kanaDisplayText != null)
            _kanaDisplayText.text = "";

        if (_romajiDisplayText != null)
            _romajiDisplayText.text = "";

        UpdateProgressText();
        OnTutorialComplete?.Invoke();
    }

    private void UpdateProgressText()
    {
        if (_progressText != null)
            _progressText.text = (_currentCharIndex + 1) + " / " + _tierEntries.Length;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Debug tier select: 1-4 keys restart tutorial on that tier
        if (Keyboard.current.digit1Key.wasPressedThisFrame && KanaDatabase.TierCount > 0)
            BeginTier(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame && KanaDatabase.TierCount > 1)
            BeginTier(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame && KanaDatabase.TierCount > 2)
            BeginTier(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame && KanaDatabase.TierCount > 3)
            BeginTier(3);
    }
}
