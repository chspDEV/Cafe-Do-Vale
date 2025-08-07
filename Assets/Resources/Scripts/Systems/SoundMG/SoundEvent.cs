using System;
using UnityEngine;

namespace GameResources.Project.Scripts.Utilities.Audio
{
    public static class SoundEvent
    {
        public static event EventHandler<SoundEventArgs> OnSoundRequested;

        public static void RequestSound(SoundEventArgs args)
        {
            OnSoundRequested?.Invoke(null, args);
        }
    }

    public class SoundEventArgs : EventArgs
    {
        public enum SoundCategory
        {
            Music,
            SFX
        }

        public SoundCategory Category { get; set; }
        public string AudioID { get; set; }
        public Vector3 Position { get; set; }
        public float VolumeScale { get; set; } = 1f;
        public Transform TargetTransform { get; set; }
        public float Pitch { get; set; } = 1f;
        public bool Loop { get; set; } = false;
    }
}
