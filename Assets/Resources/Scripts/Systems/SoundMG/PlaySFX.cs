using UnityEngine;

namespace GameResources.Project.Scripts.Utilities.Audio
{
    public class PlaySFX : MonoBehaviour
    {
        [Tooltip("name whitout prefix")]
        public string sfx_Name;
        void Start()
        {
            SoundEvent.RequestSound(new SoundEventArgs
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = sfx_Name,
                Position = transform.position,
                VolumeScale = 1f
            });
        }
    }
}
