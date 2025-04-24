using UnityEngine;

namespace Tcp4
{
    public abstract class AnimationStateParameter
    {
        public string ParameterName { get; private set; }

        protected AnimationStateParameter(string parameterName)
        {
            ParameterName = parameterName;
        }

        public virtual void ApplyEnter(Animator animator) { }
        public virtual void ApplyExit(Animator animator) { }
    }

    public class BoolStateAnimationParameter : AnimationStateParameter
    {
        private bool enterValue;
        private bool exitValue;

        public BoolStateAnimationParameter(string parameterName, bool enterValue, bool exitValue)
            : base(parameterName)
        {
            this.enterValue = enterValue;
            this.exitValue = exitValue;
        }

        public override void ApplyEnter(Animator animator)
        {
            animator.SetBool(ParameterName, enterValue);
        }

        public override void ApplyExit(Animator animator)
        {
            animator.SetBool(ParameterName, exitValue);
        }
    }

    public class FloatStateAnimationParameter : AnimationStateParameter
    {
        private float enterValue;
        private float exitValue;

        public FloatStateAnimationParameter(string parameterName, float enterValue, float exitValue)
            : base(parameterName)
        {
            this.enterValue = enterValue;
            this.exitValue = exitValue;
        }

        public override void ApplyEnter(Animator animator)
        {
            animator.SetFloat(ParameterName, enterValue);
        }

        public override void ApplyExit(Animator animator)
        {
            animator.SetFloat(ParameterName, exitValue);
        }
    }

    public class TriggerStateAnimationParameter : AnimationStateParameter
    {
        public TriggerStateAnimationParameter(string parameterName)
            : base(parameterName)
        {
        }

        public override void ApplyEnter(Animator animator)
        {
            animator.SetTrigger(ParameterName);
        }

        public override void ApplyExit(Animator animator)
        {
            animator.ResetTrigger(ParameterName);
        }
    }
}
