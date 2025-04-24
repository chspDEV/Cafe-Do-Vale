using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.DayNightCycle
{
    public class AdvanceTimingTest : MonoBehaviour
    {
        [SerializeField] private TimeManager timeManager;
        private void OnTriggerEnter(Collider other)
        { 
            if (other.CompareTag("Player")) timeManager.AdvanceTime(2);
        }
    }
}
