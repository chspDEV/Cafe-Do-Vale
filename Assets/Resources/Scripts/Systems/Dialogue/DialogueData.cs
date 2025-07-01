using System.Collections.Generic;
using UnityEngine;

namespace Tcp4
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    }
}
