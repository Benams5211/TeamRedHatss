using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class collision : MonoBehaviour
{
    public PointSystem pointSystem;
    public DictionaryManager manager;

    public AudioManager audioManager;
    public SphereCollider sphereCol;

    [SerializeField] private float hitCooldown = 1.0f;
    private bool canHit = true;

    private void Start()
    {
        if (sphereCol == null)
            sphereCol = GetComponent<SphereCollider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;

        if (other.CompareTag("correct"))
        {
            canHit = false;

            audioManager.PlayCorrect();

            Debug.Log("Correct");
            pointSystem.getPoints();

            Debug.Log("Points: " + pointSystem.points);
            Debug.Log("Lives: " + pointSystem.lives);

            manager.ShowAnswerFeedback(other.gameObject, true);
            //manager.ExplodeBox(other.gameObject);
            manager.GenerateQuestion();

            StartCoroutine(DisableColliderTemporarily());
        }
        else if (other.CompareTag("incorrect"))
        {
            canHit = false;

            audioManager.PlayIncorrect();

            Debug.Log("Incorrect");
            pointSystem.loseLife();

            if (pointSystem.lives < 0)
                pointSystem.lives = 0;

            Debug.Log("Points: " + pointSystem.points);
            Debug.Log("Lives: " + pointSystem.lives);

            manager.ShowAnswerFeedback(other.gameObject, false);
            //manager.ExplodeBox(other.gameObject);
            manager.GenerateQuestion();

            StartCoroutine(DisableColliderTemporarily());
        }
    }

    private IEnumerator DisableColliderTemporarily()
    {
        if (sphereCol != null)
            sphereCol.enabled = false;

        yield return new WaitForSeconds(hitCooldown);

        if (sphereCol != null)
            sphereCol.enabled = true;

        canHit = true;
    }
}