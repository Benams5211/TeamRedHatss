using UnityEngine;
using TMPro;
public class PointSystem : MonoBehaviour
{
    public int maxLives = 3;
    public int lives;
    public int points = 0;

    public TextMeshProUGUI livesText;
    public TextMeshProUGUI pointsText;

    public GameObject GameOverUI;
    public Transform targetPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lives = maxLives;
    }

    public void getPoints() 
    {
        points += 1;
    }

    public void loseLife() 
    {
        lives -= 1;
        if(lives == 0) {
            Vector3 spawnPos = new Vector3(-1f, 5f, 20f);
                GameObject instance = GameOverUI = Instantiate(GameOverUI, spawnPos, Quaternion.identity);

                SpawnIn spawnScript = instance.GetComponent<SpawnIn>();
                if (spawnScript != null)
                {
                    spawnScript.target = targetPos;
                    spawnScript.speed = 25f;
                }
        }
    }

    // Update is called once per frame
    void Update()
    {
        pointsText.text = "Score:\n" + points;
        livesText.text = "Lives:\n" + lives;
    }
}
