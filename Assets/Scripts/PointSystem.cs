using UnityEngine;

public class PointSystem : MonoBehaviour
{
    public int maxLives = 3;
    public int lives;
    public int points = 0;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
