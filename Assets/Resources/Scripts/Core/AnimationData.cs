namespace Tcp4.Assets.Resources.Scripts.Core
{
    public class AnimationData
    {
        public string StateName { get; private set; }
        public float TransitionDuration { get; private set; }
        public int Layer { get; private set; }

        public AnimationData(
            string stateName,
            float transitionDuration = 0.05f,
            int layer = 0)
        {
            StateName = stateName;
            TransitionDuration = transitionDuration;
            Layer = layer;
        }
    }
}