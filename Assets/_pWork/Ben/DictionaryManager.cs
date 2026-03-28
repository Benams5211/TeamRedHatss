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
    [SerializeField] private TextMeshProUGUI[] textMeshProUGUIs = null;
    [SerializeField] private Transform boxesParent = null;

    private void Awake()
    {
        FillTextArray();
    }

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

    private void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            GenerateQuestion();
        }
    }

    private void FillTextArray()
    {
        List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();

        foreach (Transform box in boxesParent)
        {
            TextMeshProUGUI tmp = box.GetComponentInChildren<TextMeshProUGUI>();

            if (tmp != null)
            {
                texts.Add(tmp);
            }
        }

        textMeshProUGUIs = texts.ToArray();
    }

    private void GenerateQuestion()
    {
        PickRandomKana();

        // Show the romaji the player needs to find
        toFind.text = romaji;

        // Build answer choices
        List<string> answerChoices = new List<string>();
        answerChoices.Add(randomKana); // correct answer

        // Add wrong answers
        List<string> availableKana = new List<string>(kanaKeys);
        availableKana.Remove(randomKana);

        while (answerChoices.Count < textMeshProUGUIs.Length && availableKana.Count > 0)
        {
            int randomIndex = Random.Range(0, availableKana.Count);
            answerChoices.Add(availableKana[randomIndex]);
            availableKana.RemoveAt(randomIndex);
        }

        // Shuffle answer choices
        ShuffleList(answerChoices);

        // Assign to text boxes
        for (int i = 0; i < textMeshProUGUIs.Length; i++)
        {
            textMeshProUGUIs[i].text = answerChoices[i];
        }
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
            string temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}