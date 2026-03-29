using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class collision : MonoBehaviour
{
    public PointSystem pointSystem;
    public DictionaryManager manager;

    public bool detect = true;

    public AudioSource audioSource1;

    void OnTriggerEnter(Collider other) 
    {
        if (detect)
        {
            if (other.gameObject.tag == "correct")
            {
                audioSource1.Play();
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

    private void Start()
    {
        //audioSource1 = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            audioSource1.Play();
        }
    }
}
