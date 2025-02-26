using Events;
using UnityEngine;

namespace GamePlay.Managers.UI
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private int score;
        [SerializeField] private ScoreChangedEventChannel scoreChangedEvent;

        public int Score => score;
        
        public void AddScore(int amount)
        {
            score += amount;

            if (scoreChangedEvent != null)
            {
                scoreChangedEvent.RaiseEvent(score);
            }
        }

        public void ResetScore()
        {
            score = 0;
            
            if (scoreChangedEvent != null)
            {
                scoreChangedEvent.RaiseEvent(score);
            }
        }
        
    }
}