using UnityEngine;
namespace GameResources.Project.Scripts.Utilities.Audio
{
    public static class SoundController
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            System.Type type = typeof(SoundController);
            Debug.Log("SoundController inicializado automaticamente.");
        }

        static SoundController()
        {
            SoundEvent.OnSoundRequested += HandleSoundRequested;
        }

        private static void HandleSoundRequested(object sender, SoundEventArgs args)
        {
            switch (args.Category)
            {
                case SoundEventArgs.SoundCategory.Music:
                    SoundManager.PlayOST(args.AudioID);
                    break;
                case SoundEventArgs.SoundCategory.SFX:
                    if (args.TargetTransform != null)
                    {
                        SoundManager.PlaySFX(args.AudioID, args.TargetTransform, args.VolumeScale, args.Pitch);
                    }
                    else
                    {
                        SoundManager.PlaySFX(args.AudioID, args.Position, args.VolumeScale, args.Pitch);
                    }
                    break;
            }
        }
    }
}