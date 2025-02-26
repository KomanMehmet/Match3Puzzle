using UnityEngine;
using UnityEngine.Events;

namespace Events
{
    [CreateAssetMenu(fileName = "ScoreChangedEventChannel", menuName = "Events/ScoreChangedEventChannel")]
    public class ScoreChangedEventChannel : ScriptableObject
    {
        public UnityAction<int> OnScoreChanged = delegate { };

        public void RaiseEvent(int newScore) => OnScoreChanged?.Invoke(newScore);
    }
}