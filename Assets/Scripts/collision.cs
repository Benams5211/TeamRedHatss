using UnityEngine;
using UnityEngine.SceneManagement;

public class collision : MonoBehaviour
{
    public bool detect = true;

    private PointSystem _pointSystem;
    private DictionaryManager _manager;
    private TutorialManager _tutorialManager;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindReferences();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindReferences();
    }

    private void FindReferences()
    {
        _pointSystem = FindAnyObjectByType<PointSystem>();
        _manager = FindAnyObjectByType<DictionaryManager>();
        _tutorialManager = FindAnyObjectByType<TutorialManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_manager == null || _pointSystem == null)
            FindReferences();

        if (_manager == null || _pointSystem == null) return;

        // The collider is on Cube (child), but the tag is on box (parent).
        // Walk up one level from the collider to the box root.
        GameObject root = other.transform.parent != null
            ? other.transform.parent.gameObject
            : other.gameObject;

        Debug.Log("[Collision] Hit: " + other.gameObject.name
            + " | Parent: " + (root != null ? root.name : "null")
            + " | Tag: " + root.tag
            + " | detect: " + detect
            + " | manager: " + (_manager != null ? _manager.gameObject.name : "null")
            + " | tutorial: " + (_tutorialManager != null));

        if (detect)
        {
            if (root.CompareTag("correct"))
            {
                Debug.Log("Correct");
                _pointSystem.getPoints();
                RecordStat(true);
                _manager.ShowAnswerFeedback(root, true);

                if (_tutorialManager != null)
                    _tutorialManager.OnCorrectAnswer();
                else
                    _manager.GenerateQuestion();
            }
            else if (root.CompareTag("incorrect"))
            {
                Debug.Log("Incorrect");
                _pointSystem.loseLife();
                RecordStat(false);
                _manager.ShowAnswerFeedback(root, false);

                if (_tutorialManager != null)
                    _tutorialManager.OnIncorrectAnswer();
                else
                    _manager.GenerateQuestion();
            }
        }
        // Re-enable after a short delay since the choice may be destroyed
        // before OnTriggerExit fires (destroying the collider skips the exit callback)
        detect = false;
        Invoke(nameof(ResetDetect), 0.2f);
    }

    private void ResetDetect()
    {
        detect = true;
    }

    private void RecordStat(bool correct)
    {
        if (KanaStatsTracker.Instance != null)
        {
            KanaStatsTracker.Instance.RecordAnswer(
                _manager.CurrentTierIndex,
                _manager.CurrentKana,
                correct,
                _manager.ResponseTime
            );
        }
    }

    private void OnTriggerExit()
    {
        detect = true;
    }
}
