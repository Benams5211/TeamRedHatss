using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Tutorial scene controller. Auto-starts on scene load.
/// For each character: shows a brief intro (kana + romaji), then spawns
/// practice rounds. After all characters in the tier are practiced,
/// marks the tier complete and fires OnTutorialComplete.
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
    [Tooltip("Overridden at runtime by ActiveTierSelection.")]
    [SerializeField] private int _tier = 0;
    [SerializeField] private int _practiceRoundsPerCharacter = 3;
    [SerializeField] private float _introDisplayTime = 2f;

    [Header("Events")]
    public UnityEvent OnTutorialComplete;

    private KanaDatabase.KanaEntry[] _tierEntries;
    private int _currentCharIndex;
    private int _practiceCount;
    private bool _inIntroPhase;
    private bool _tutorialFinished;

    private void Start()
    {
        _tier = ActiveTierSelection.SelectedTier;
        _tierEntries = KanaDatabase.GetTierEntries(_tier);

        if (_tierEntries.Length == 0)
        {
            Debug.LogError("TutorialManager: No entries for tier " + _tier);
            return;
        }

        _currentCharIndex = 0;
        _tutorialFinished = false;

        _dictionaryManager.LoadTier(_tier);

        StartCoroutine(RunIntro());
    }

    /// <summary>
    /// Shows the character intro for _introDisplayTime seconds, then auto-starts practice.
    /// </summary>
    private IEnumerator RunIntro()
    {
        _inIntroPhase = true;
        _practiceCount = 0;

        KanaDatabase.KanaEntry entry = _tierEntries[_currentCharIndex];

        // Show flashcard
        if (_kanaDisplayText != null)
            _kanaDisplayText.text = entry.kana;

        if (_romajiDisplayText != null)
            _romajiDisplayText.text = entry.romaji;

        if (_promptText != null)
            _promptText.text = "New character:";

        UpdateProgressText();

        // Brief pause so the player can see the character
        yield return new WaitForSeconds(_introDisplayTime);

        // Transition to practice
        _inIntroPhase = false;

        if (_kanaDisplayText != null)
            _kanaDisplayText.text = "";

        if (_romajiDisplayText != null)
            _romajiDisplayText.text = "";

        if (_promptText != null)
            _promptText.text = "Find the correct kana!";

        // Build practice pool: only learned characters
        List<KanaDatabase.KanaEntry> practicePool = new List<KanaDatabase.KanaEntry>();
        for (int i = 0; i <= _currentCharIndex; i++)
        {
            practicePool.Add(_tierEntries[i]);
        }
        _dictionaryManager.LoadEntries(practicePool);
        _dictionaryManager.GenerateQuestion();
    }

    /// <summary>
    /// Called by collision when the player answers correctly.
    /// </summary>
    public void OnCorrectAnswer()
    {
        if (_tutorialFinished || _inIntroPhase) return;

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
                StartCoroutine(RunIntro());
            }
        }
        else
        {
            _dictionaryManager.GenerateQuestion();
        }
    }

    /// <summary>
    /// Called by collision when the player answers incorrectly.
    /// </summary>
    public void OnIncorrectAnswer()
    {
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

        UpdateProgressText();
        OnTutorialComplete?.Invoke();
    }

    private void UpdateProgressText()
    {
        if (_progressText != null)
            _progressText.text = (_currentCharIndex + 1) + " / " + _tierEntries.Length;
    }
}
