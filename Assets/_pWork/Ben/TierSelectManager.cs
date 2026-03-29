using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the tier selection screen. Shows each tier with its completion state.
/// Player picks a tier, then chooses Tutorial or Play.
/// Sets ActiveTierSelection and loads the target scene.
/// </summary>
public class TierSelectManager : MonoBehaviour
{
    [System.Serializable]
    public struct TierButton
    {
        public Button button;
        public TextMeshProUGUI label;
        public GameObject completedIndicator;
    }

    [Header("Tier Buttons (one per tier, in order)")]
    [SerializeField] private TierButton[] _tierButtons;

    [Header("Action Panel (shown after selecting a tier)")]
    [SerializeField] private GameObject _actionPanel;
    [SerializeField] private Button _tutorialButton;
    [SerializeField] private Button _playButton;
    [SerializeField] private TextMeshProUGUI _selectedTierText;

    [Header("Scene Names")]
    [SerializeField] private string _tutorialScene = "_tutorial";
    [SerializeField] private string _gameplayScene = "Main";

    private int _selectedTier = -1;

    private void Start()
    {
        if (_actionPanel != null)
            _actionPanel.SetActive(false);

        TierFlags completed = TierProgress.CompletedFlags;

        for (int i = 0; i < _tierButtons.Length && i < KanaDatabase.TierCount; i++)
        {
            TierButton tb = _tierButtons[i];
            int tierIndex = i; // capture for closure

            // Set label to tier name
            if (tb.label != null)
                tb.label.text = KanaDatabase.Tiers[i].name;

            // Show/hide completion indicator
            bool isCompleted = (completed & KanaDatabase.TierIndexToFlag(i)) != 0;
            if (tb.completedIndicator != null)
                tb.completedIndicator.SetActive(isCompleted);

            // Wire button
            if (tb.button != null)
                tb.button.onClick.AddListener(() => SelectTier(tierIndex));
        }

        // Wire action buttons
        if (_tutorialButton != null)
            _tutorialButton.onClick.AddListener(StartTutorial);

        if (_playButton != null)
            _playButton.onClick.AddListener(StartGameplay);
    }

    private void SelectTier(int tier)
    {
        _selectedTier = tier;

        if (_actionPanel != null)
            _actionPanel.SetActive(true);

        if (_selectedTierText != null)
            _selectedTierText.text = KanaDatabase.Tiers[tier].name;

        // Disable Play button if tier hasn't been completed via tutorial yet
        if (_playButton != null)
            _playButton.interactable = TierProgress.IsTierCompleted(tier);
    }

    private void StartTutorial()
    {
        if (_selectedTier < 0) return;

        ActiveTierSelection.SelectedTier = _selectedTier;
        ActiveTierSelection.Mode = ActiveTierSelection.PlayMode.Tutorial;
        SceneMgmt.Manager.LoadByName(_tutorialScene);
    }

    private void StartGameplay()
    {
        if (_selectedTier < 0) return;

        ActiveTierSelection.SelectedTier = _selectedTier;
        ActiveTierSelection.Mode = ActiveTierSelection.PlayMode.Gameplay;
        SceneMgmt.Manager.LoadByName(_gameplayScene);
    }
}
