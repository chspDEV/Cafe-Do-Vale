using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;

namespace Tcp4
{
    public class CollectableSettings : MonoBehaviour
    {
        TimeManager tm;

        [SerializeField] bool onlyNight;

        private void Start()
        {
            tm = TimeManager.Instance;
            

            if (onlyNight)
            {
                gameObject.SetActive(false);
                InvokeRepeating(nameof(CheckNight), 0f, 5f);
            }

        }
        void Update()
        {
        
        }

        void CheckNight()
        {
            if (!tm.isDay)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
