using System.Collections.Generic;
using UnityEngine;

namespace GameResources.Project.Scripts.Utilities.Audio
{
    public class PlayRandomSFX : MonoBehaviour
    {
        [Tooltip("names whitout prefix")]
        public List<string> sfx_Names;
        void Start()
        {
            var clip = Random.Range(0, sfx_Names.Count);
            SoundEvent.RequestSound(new SoundEventArgs
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = sfx_Names[clip],
                Position = transform.position,
                VolumeScale = 1f
            });
        }
    }
}
