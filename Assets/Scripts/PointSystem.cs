using System;
using System.IO;
using TMPro;
using UnityEngine;

public class PointSystem : MonoBehaviour
{
    [Serializable]
    private class HighScoreSaveData
    {
        public int highScore;
    }

    private const string SaveFileName = "highscore.json";

    public int maxLives = 3;
    public int lives;
    public int points = 0;

    public TextMeshProUGUI livesText;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI highScoreText;

    private int highScore;
    public GameObject GameOverUI;
    public Transform targetPos;

    public static bool gameOver = false;

    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private void Awake()
    {
        LoadHighScore();
    }

    private void Update()
    {
        if(gameOver)
        {
            GameOverUI.SetActive(true);
        }
        else
        {
            GameOverUI.SetActive(false);
        }
    }

    public void loseLife()
    {
        if (gameOver) return;
        lives -= 1;
        if (lives == 0)
        {
            gameOver = true;
            Debug.Log("YOU DEAD TWIN");
            //Vector3 spawnPos = new Vector3(-9f, 6f, 0f);
            //GameObject instance = Instantiate(GameOverUI, spawnPos, Quaternion.identity);
            //instance.SetActive(true);
            //SpawnIn spawnScript = instance.GetComponent<SpawnIn>();
            //if (spawnScript != null)
            //{
            //    spawnScript.target = targetPos;
            //    spawnScript.speed = 25f;
            //}
        }
        RefreshUI();
    }

    private void Start()
    {
        lives = maxLives;
        RefreshUI();
    }

    public void getPoints()
    {
        points += 1;
        TrySetHighScore();
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (pointsText != null)
            pointsText.text = "Score:\n" + points;

        if (highScoreText != null)
            highScoreText.text = "High Score:\n" + highScore;

        if (livesText != null)
            livesText.text = "Lives:\n" + lives;
    }

    private void TrySetHighScore()
    {
        if (points <= highScore)
            return;

        highScore = points;
        SaveHighScore();
    }

    private void LoadHighScore()
    {
        if (!File.Exists(SavePath))
        {
            highScore = 0;
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            HighScoreSaveData saveData = JsonUtility.FromJson<HighScoreSaveData>(json);
            highScore = Mathf.Max(0, saveData != null ? saveData.highScore : 0);
        }
        catch (Exception exception)
        {
            highScore = 0;
            Debug.LogWarning($"Failed to load high score from {SavePath}: {exception.Message}");
        }
    }

    private void SaveHighScore()
    {
        try
        {
            HighScoreSaveData saveData = new HighScoreSaveData { highScore = highScore };
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Failed to save high score to {SavePath}: {exception.Message}");
        }
    }
}
