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

    [SerializeField] private TextMeshProUGUI toFind;

    [Header("Choice Spawning")]
    [SerializeField] private GameObject _choicePrefab;
    [SerializeField] private int _choiceCount = 4;

    [Header("Answer Animation")]
    [SerializeField] private AnswerLerpManager _lerpManager;

    private readonly List<GameObject> _activeChoices = new List<GameObject>();

    [SerializeField] private GameObject explosionPrefab;

    private void Start()
    {
        kanaLib.Add("あ", "a");
        kanaLib.Add("い", "i");
        kanaLib.Add("う", "u");
        kanaLib.Add("え", "e");
        kanaLib.Add("お", "o");

        kanaKeys = new List<string>(kanaLib.Keys);

        GenerateQuestion();
    }
    public void ExplodeBox(GameObject box)
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, box.transform.position, Quaternion.identity);
        }

        Destroy(box);
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

            // Set answer text (prefab structure: Choice > Canvas > Image > Text (TMP))
            TextMeshProUGUI tmp = choice.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = answerChoices[i];

            // Tag the root object for collision detection
            choice.tag = answerChoices[i] == randomKana ? "correct" : "incorrect";

            boxes[i] = choice.transform;
        }

        // Animate from start points to end points
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

        // Pull it out so GenerateQuestion won't destroy it mid-flash
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
