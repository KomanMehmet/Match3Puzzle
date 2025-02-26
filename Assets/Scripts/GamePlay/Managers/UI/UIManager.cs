using Events;
using TMPro;
using UnityEngine;

namespace GamePlay.Managers.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private ScoreChangedEventChannel scoreChangedEvent;

        private void OnEnable()
        {
            if (scoreChangedEvent != null)
            {
                scoreChangedEvent.OnScoreChanged += UpdateScore;
            }
        }

        private void OnDisable()
        {
            if (scoreChangedEvent != null)
            {
                scoreChangedEvent.OnScoreChanged -= UpdateScore;
            }
        }

        private void UpdateScore(int newScore)
        {
            if (scoreText != null)
            {
                scoreText.text = "Score: " + newScore;
            }
        }
    }
}