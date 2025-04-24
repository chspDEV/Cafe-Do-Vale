using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.DayNightCycle
{
    public class SleepTesting : MonoBehaviour
    {
        [SerializeField] private TimeManager timeManager;
        
        private void OnTriggerEnter(Collider other)
        { 
            if (other.CompareTag("Player")) timeManager.Sleep();
        }
    }
}
