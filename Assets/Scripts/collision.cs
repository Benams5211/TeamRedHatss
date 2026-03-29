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

        if (detect)
        {
            if (other.gameObject.CompareTag("correct"))
            {
                Debug.Log("Correct");
                _pointSystem.getPoints();
                RecordStat(true);
                _manager.ShowAnswerFeedback(other.gameObject, true);

                // Route to tutorial or gameplay
                if (_tutorialManager != null)
                    _tutorialManager.OnCorrectAnswer();
                else
                    _manager.GenerateQuestion();
            }
            else if (other.gameObject.CompareTag("incorrect"))
            {
                Debug.Log("Incorrect");
                _pointSystem.loseLife();
                RecordStat(false);
                _manager.ShowAnswerFeedback(other.gameObject, false);

                if (_tutorialManager != null)
                    _tutorialManager.OnIncorrectAnswer();
            }
        }
        detect = false;
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
