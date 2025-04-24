using UnityEngine;
using UnityEngine.VFX;

namespace Tcp4
{
    public class FeelEffects : MonoBehaviour
    {
        public VisualEffect footFX; 
        
        public void Footeffect()
        {
            footFX.Play();
        }
    }
}
