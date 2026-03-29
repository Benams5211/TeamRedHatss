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

    [Header("Tier Selection")]
    [SerializeField] private TierFlags _activeTiers = TierFlags.Vowels;
    [SerializeField] private bool _includeAllUnlockedTiers = false;

    [Header("Choice Spawning")]
    [SerializeField] private GameObject _choicePrefab;
    [SerializeField] private int _choiceCount = 4;

    [Header("Answer Animation")]
    [SerializeField] private AnswerLerpManager _lerpManager;

    private readonly List<GameObject> _activeChoices = new List<GameObject>();

    /// <summary>The kana/word currently being asked about.</summary>
    public string CurrentKana => randomKana;
    /// <summary>The tier index for the current question pool.</summary>
    public int CurrentTierIndex => _currentTierIndex;
    /// <summary>Seconds elapsed since the current question was shown.</summary>
    public float ResponseTime => Time.time - _questionStartTime;

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

        // Tutorial scene: TutorialManager.Start() calls LoadTier + GenerateQuestion itself,
        // so only auto-generate if we're not in tutorial mode
        if (ActiveTierSelection.Mode != ActiveTierSelection.PlayMode.Tutorial)
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
        // Destroy leftover choices (hit choice is already removed from this list)
        for (int i = _activeChoices.Count - 1; i >= 0; i--)
        {
            if (_activeChoices[i] != null)
                Destroy(_activeChoices[i]);
        }
        _activeChoices.Clear();

        PickRandomKana();
        toFind.text = romaji;
        _questionStartTime = Time.time;

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
    }

    /// <summary>
    /// Call from collision to flash the hit answer green/red.
    /// The hit choice is removed from the active list so it survives
    /// the next GenerateQuestion call, then self-destructs after the flash.
    /// </summary>
    public void ShowAnswerFeedback(GameObject hitChoice, bool isCorrect)
    {
        if (_lerpManager == null) return;

        _activeChoices.Remove(hitChoice);

        TextMeshProUGUI hitText = hitChoice.GetComponentInChildren<TextMeshProUGUI>();
        if (hitText != null)
            _lerpManager.ShowFeedback(hitText, isCorrect, hitChoice);
    }

    private void PickRandomKana()
    {
        int randomIndex = Random.Range(0, kanaKeys.Count);
        randomKana = kanaKeys[randomIndex];
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
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            GenerateQuestion();
        }
    }
}
