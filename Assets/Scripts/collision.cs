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
                pointSystem.getPoints();

                Debug.Log("Points: " + pointSystem.points);
                Debug.Log("Lives: " + pointSystem.lives);

                manager.ShowAnswerFeedback(other.gameObject, true);
                manager.GenerateQuestion();
            }
            else if (other.gameObject.tag == "incorrect")
            {
                Debug.Log("Incorrect");
                pointSystem.loseLife();
                if (pointSystem.lives == 0)
                {
                    pointSystem.lives = 0;
                }

                Debug.Log("Points: " + pointSystem.points);
                Debug.Log("Lives: " + pointSystem.lives);

                manager.ShowAnswerFeedback(other.gameObject, false);
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
