using UnityEngine;
using TMPro;

public class GameOver : MonoBehaviour
{
    public GameObject GameOverUI;
    public PointSystem pointSystem;

    public TMP_Text points;
    public TMP_Text lives;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameOverUI.SetActive(true);
        points.text = "Points: " + pointSystem.points;
        lives.text = "Lives Left: " + pointSystem.lives;
    }

    public void restartGame() {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
