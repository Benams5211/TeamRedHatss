using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DictionaryManager : MonoBehaviour
{
    private Dictionary<string, string> kanaLib = new Dictionary<string, string>();
    private List<string> kanaKeys = new List<string>();

    private string randomKana;
    private string romaji;
    private int _currentTierIndex;
    private float _questionStartTime;

    [SerializeField] private TextMeshProUGUI toFind;
    [Tooltip("Root transform of the toFind display object (the whole cube).")]
    [SerializeField] private Transform _toFindRoot;

    [Header("Tier Selection")]
    [SerializeField] private TierFlags _activeTiers = TierFlags.Vowels;
    [SerializeField] private bool _includeAllUnlockedTiers = false;

    [Header("Choice Spawning")]
    [SerializeField] private GameObject _choicePrefab;
    [SerializeField] private int _choiceCount = 4;
    public int ChoiceCount => _choiceCount;

    [Header("Answer Animation")]
    [SerializeField] private AnswerLerpManager _lerpManager;

    private readonly List<GameObject> _activeChoices = new List<GameObject>();

    /// <summary>The kana/word currently being asked about.</summary>
    public string CurrentKana => randomKana;
    /// <summary>The tier index for the current question pool.</summary>
    public int CurrentTierIndex => _currentTierIndex;
    /// <summary>Seconds elapsed since the current question was shown.</summary>
    public float ResponseTime => Time.time - _questionStartTime;

    private string _pinnedKana;

    /// <summary>
    /// Pin a specific kana as the correct answer for all subsequent questions
    /// until cleared. Distractors are still randomized from the pool.
    /// Pass null to return to random selection.
    /// </summary>
    /// <summary>
    /// Spin the toFind display and clear the text at the midpoint.
    /// </summary>
    public void SpinToFindEmpty()
    {
        if (_lerpManager != null && _toFindRoot != null && toFind != null)
            _lerpManager.SpinToFindDisplay(_toFindRoot, toFind, "");
    }

    public void PinCorrectAnswer(string kana)
    {
        _pinnedKana = kana;
    }

    private void Start()
    {
        // If coming from tier select in Gameplay mode, use that tier
        if (ActiveTierSelection.Mode == ActiveTierSelection.PlayMode.Gameplay)
        {
            LoadTier(ActiveTierSelection.SelectedTier);
        }
        else if (_includeAllUnlockedTiers)
        {
            LoadTiers(TierProgress.GetUnlockedFlags());
        }
        else
        {
            LoadTiers(_activeTiers);
        }

        // Only auto-generate if there's no TutorialManager in the scene.
        // TutorialManager.Start() drives LoadTier + GenerateQuestion itself.
        if (FindAnyObjectByType<TutorialManager>() == null)
            GenerateQuestion();
    }

    /// <summary>
    /// Load characters for the given tier flags into the active pool.
    /// Example: LoadTiers(TierFlags.Vowels | TierFlags.Consonants)
    /// </summary>
    public void LoadTiers(TierFlags flags)
    {
        List<KanaDatabase.KanaEntry> entries = KanaDatabase.GetEntries(flags);
        ApplyEntries(entries);

        // Store the lowest set tier index for stats tracking
        for (int i = 0; i < KanaDatabase.TierCount; i++)
        {
            if ((flags & KanaDatabase.TierIndexToFlag(i)) != 0)
            {
                _currentTierIndex = i;
                break;
            }
        }
    }

    /// <summary>
    /// Load a single tier by index into the active pool.
    /// </summary>
    public void LoadTier(int tier)
    {
        _currentTierIndex = tier;
        LoadTiers(KanaDatabase.TierIndexToFlag(tier));
    }

    /// <summary>
    /// Load a custom set of entries (used by TutorialManager for progressive practice).
    /// </summary>
    public void LoadEntries(List<KanaDatabase.KanaEntry> entries)
    {
        ApplyEntries(entries);
    }

    private void ApplyEntries(IEnumerable<KanaDatabase.KanaEntry> entries)
    {
        kanaLib.Clear();
        foreach (KanaDatabase.KanaEntry e in entries)
        {
            kanaLib[e.kana] = e.romaji;
        }
        kanaKeys = new List<string>(kanaLib.Keys);
    }

    public void GenerateQuestion()
    {
        // Previous choices are cleaned up by ShowAnswerFeedback's flash timer.
        // Only destroy if there are stale leftovers (e.g. first question, or no answer was given).
        for (int i = _activeChoices.Count - 1; i >= 0; i--)
        {
            if (_activeChoices[i] != null)
                Destroy(_activeChoices[i]);
        }
        _activeChoices.Clear();

        PickRandomKana();
        _questionStartTime = Time.time;

        // Spin the entire toFind object and swap text at the midpoint
        if (_lerpManager != null && _toFindRoot != null && toFind != null)
        {
            _lerpManager.SpinToFindDisplay(_toFindRoot, toFind, romaji);
        }
        else if (toFind != null)
        {
            toFind.text = romaji;
        }

        // Build answer choices
        List<string> answerChoices = new List<string> { randomKana };

        List<string> availableKana = new List<string>(kanaKeys);
        availableKana.Remove(randomKana);

        while (answerChoices.Count < _choiceCount && availableKana.Count > 0)
        {
            int randomIndex = Random.Range(0, availableKana.Count);
            answerChoices.Add(availableKana[randomIndex]);
            availableKana.RemoveAt(randomIndex);
        }

        ShuffleList(answerChoices);

        // Spawn prefabs and configure
        Transform[] boxes = new Transform[answerChoices.Count];

        for (int i = 0; i < answerChoices.Count; i++)
        {
            GameObject choice = Instantiate(_choicePrefab);
            _activeChoices.Add(choice);

            TextMeshProUGUI tmp = choice.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = answerChoices[i];

            choice.tag = answerChoices[i] == randomKana ? "correct" : "incorrect";

            boxes[i] = choice.transform;
        }

        if (_lerpManager != null)
            _lerpManager.AnimateAnswers(boxes);

        Debug.Log("[GenerateQuestion] Spawned " + _activeChoices.Count + " choices on " + gameObject.name + " (instance: " + GetInstanceID() + ")");
    }

    /// <summary>
    /// Flash ALL active choices: correct = green, incorrect = red.
    /// All choices are destroyed together after the flash duration.
    /// New choices are already flying in during this time.
    /// </summary>
    public void ShowAnswerFeedback(GameObject hitChoice, bool isCorrect)
    {
        if (_lerpManager == null) return;

        // Hand off all choices to the flash routine — it owns their lifecycle now
        _lerpManager.ShowAllFeedback(new List<GameObject>(_activeChoices), "correct");
        _activeChoices.Clear();
    }

    private void PickRandomKana()
    {
        if (!string.IsNullOrEmpty(_pinnedKana) && kanaLib.ContainsKey(_pinnedKana))
        {
            randomKana = _pinnedKana;
        }
        else
        {
            int randomIndex = Random.Range(0, kanaKeys.Count);
            randomKana = kanaKeys[randomIndex];
        }

        romaji = kanaLib[randomKana];

        Debug.Log("Kana: " + randomKana);
        Debug.Log("Romaji: " + romaji);
    }

    private void ShuffleList(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.pKey.wasPressedThisFrame)
            GenerateQuestion();

        // Debug: ] = simulate hitting the correct answer
        if (Keyboard.current.rightBracketKey.wasPressedThisFrame)
            SimulateHit("correct");

        // Debug: [ = simulate hitting a wrong answer
        if (Keyboard.current.leftBracketKey.wasPressedThisFrame)
            SimulateHit("incorrect");
    }

    private void SimulateHit(string tag)
    {
        // Find a matching active choice
        GameObject target = null;
        foreach (GameObject choice in _activeChoices)
        {
            if (choice != null && choice.CompareTag(tag))
            {
                target = choice;
                break;
            }
        }

        if (target == null)
        {
            Debug.Log("[SimulateHit] No active choice with tag: " + tag);
            return;
        }

        TextMeshProUGUI tmp = target.GetComponentInChildren<TextMeshProUGUI>();
        string choiceText = tmp != null ? tmp.text : "?";
        Debug.Log("[SimulateHit] Pressed " + (tag == "correct" ? "]" : "[")
            + " | Target: " + choiceText
            + " | Expected: " + randomKana + " (" + romaji + ")"
            + " | Result: " + tag.ToUpper());

        bool isCorrect = tag == "correct";

        // Try to route through collision if it exists (XR Rig spawned)
        collision col = FindAnyObjectByType<collision>();
        if (col != null)
        {
            Collider hitCollider = target.GetComponent<Collider>();
            if (hitCollider != null)
                col.SendMessage("OnTriggerEnter", hitCollider);
            return;
        }

        // No XR Rig — handle directly (editor testing without Boot scene)
        if (KanaStatsTracker.Instance != null)
            KanaStatsTracker.Instance.RecordAnswer(_currentTierIndex, randomKana, isCorrect, ResponseTime);

        ShowAnswerFeedback(target, isCorrect);

        TutorialManager tutorial = FindAnyObjectByType<TutorialManager>();
        if (tutorial != null)
        {
            if (isCorrect)
                tutorial.OnCorrectAnswer();
            else
                tutorial.OnIncorrectAnswer();
        }
        else
        {
            if (isCorrect)
                GenerateQuestion();
        }
    }
}
