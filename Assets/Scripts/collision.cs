using UnityEngine;

public class collision : MonoBehaviour
{
    public PointSystem pointSystem;
    public DictionaryManager manager;

    public bool detect = true;

    void OnTriggerEnter(Collider other) 
    {
        if (detect)
        {
            if (other.gameObject.tag == "correct")
            {
                Debug.Log("Correct");
                // add health, move on
                pointSystem.getPoints();    // increase points

                Debug.Log("Points: " + pointSystem.points);
                Debug.Log("Lives: " + pointSystem.lives);
                manager.GenerateQuestion();

            }
            else if (other.gameObject.tag == "incorrect")
            {
                Debug.Log("Incorrect");
                pointSystem.loseLife(); // decrease lives
                if (pointSystem.lives == 0)
                {
                    pointSystem.lives = 0;
                }

                Debug.Log("Points: " + pointSystem.points);
                Debug.Log("Lives: " + pointSystem.lives);

            }
        }
        detect = false;
    }

    private void OnTriggerExit()
    {
        detect = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
