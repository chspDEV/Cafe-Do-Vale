using Tcp4.Resources.Scripts.Core;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.Utility
{
    public class DebugComponent : MonoBehaviour
    {
        private DynamicEntity dynamicEntity;
        private StaticEntity staticEntity;

        [Header("Debug GUI Settings")]
        public bool isPlayer = false;
        public Vector2 guiOffset = new Vector2(0, 1.5f);
        public int textSize = 20;
        public Color textColor = Color.white;

        private void Start()
        {
            TryGetComponent<DynamicEntity>(out dynamicEntity);
            TryGetComponent<StaticEntity>(out staticEntity);
        }

        private void OnGUI()
        {
            GUI.skin.label.fontSize = textSize;

            if (dynamicEntity != null)
            {
                DisplayDynamicEntityInfo(dynamicEntity);
            }
            else if (staticEntity != null)
            {
                DisplayStaticEntityInfo(staticEntity);
            }
            else
            {
                Debug.LogWarning("Nenhuma entidade v�lida encontrada.");
            }
        }

        private void DisplayDynamicEntityInfo(DynamicEntity entity)
        {
            if (isPlayer)
            {
                GUI.color = textColor;
                GUI.Label(new Rect(10, 10, 300, textSize + 5), $"State: {entity.Machine.CurrentState.GetType().Name}");
                //GUI.Label(new Rect(10, 10 + textSize + 5, 300, textSize + 5), $"FPS: {Mathf.Round(1 / Time.deltaTime)}");
                foreach (var field in entity.GetType().GetFields())
                {
                    GUI.Label(new Rect(10, 30 + (textSize + 5) * field.MetadataToken, 300, textSize + 5), $"{field.Name}: {field.GetValue(entity)}");
                }
            }
            else
            {
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(entity.transform.position + new Vector3(guiOffset.x, guiOffset.y, 0));
                GUI.color = textColor;
                GUI.Label(new Rect(screenPosition.x - 50, Screen.height - screenPosition.y - 50, 300, textSize + 5), $"State: {entity.Machine.CurrentState.GetType().Name}");
            }
        }

        private void DisplayStaticEntityInfo(StaticEntity entity)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(entity.transform.position + new Vector3(guiOffset.x, guiOffset.y, 0));
            GUI.color = textColor;
            GUI.Label(new Rect(screenPosition.x - 50, Screen.height - screenPosition.y - 50, 300, textSize + 5), $"Static Entity: {entity.name}");
        }
    }
}
