using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tcp4
{
    public class SetSelectedGameObject : MonoBehaviour
    {
        class MyUIController : MonoBehaviour
        {
            public GameObject firstButton;

            void OnEnable()
            {
                EventSystem.current.SetSelectedGameObject(firstButton);
            }
        }
    }
}
