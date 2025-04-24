using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Core;
using Tcp4.Resources.Scripts.Core;

namespace Tcp4.Resources.Scripts.FSM
{
    public abstract class State<T> : IInitializeState<T> where T : BaseEntity
    {
        protected T Entity;
        protected AnimationData StateAnimation;
        protected List<string> AdditionalAnimations;

        public virtual void Initialize(T e) // e = entity
        {
            this.Entity = e;
            AdditionalAnimations = new List<string>();
            ConfigureAnimation();
        }

        public virtual void DoEnterLogic()
        {
            DoChecks();
            PlayStateAnimation();
        }

        public virtual void DoExitLogic()
        {
            ResetValues();
        }

        protected virtual void ConfigureAnimation() { }
        public virtual void DoFrameUpdateLogic() { }
        public virtual void DoPhysicsLogic() { DoChecks(); }
        public virtual void DoChecks() { }
        public virtual void ResetValues() { }
     
        protected void PlayStateAnimation()
        {
            if (StateAnimation != null && Entity.Anim != null)
            {
                Entity.Anim.CrossFade(
                    StateAnimation.StateName,
                    StateAnimation.TransitionDuration,
                    StateAnimation.Layer
                );
            }
        }
        
        protected void PlayAdditionalAnimation(string animationName, float transitionDuration = 0.1f, int layer = 0)
        {
            Entity.Anim.CrossFadeInFixedTime(animationName, transitionDuration, layer);
            if (!AdditionalAnimations.Contains(animationName))
            {
                AdditionalAnimations.Add(animationName);
            }
        }
    }
}
