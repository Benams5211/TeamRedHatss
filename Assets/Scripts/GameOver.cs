using UnityEngine;
using TMPro;

public class GameOver : MonoBehaviour
{
    public GameObject GameOverUI;
    //public PointSystem pointSystem;

    //public TMP_Text points;
    //public TMP_Text lives;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //points.text = "Points: " + pointSystem.points;
        //lives.text = "Lives Left: " + pointSystem.lives;

        Vector3 spawnPos = new Vector3(-7.86038f, 0f, 50.26112f);
        Instantiate(GameOverUI, spawnPos, Quaternion.identity);
    }

    public void restartGame() {
        Debug.Log("Restart game");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
