using UnityEngine;

namespace GameResources.Project.Scripts.Utilities.Audio
{
    public class PlayMusic : MonoBehaviour
    {
        [Tooltip("name whitout prefix")]
        public string ostName;
        void Start()
        {
            SoundEvent.RequestSound(new SoundEventArgs
            {
                Category = SoundEventArgs.SoundCategory.Music,
                AudioID = ostName
            });
        }
    }
}
