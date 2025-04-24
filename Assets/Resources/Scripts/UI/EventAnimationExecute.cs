using UnityEngine;

namespace Tcp4
{
    public class AnimationExecute : MonoBehaviour
    {
        public Animator anim;

        public void ExecuteAnimation(string parameter)
        {
            if (!IsAnimationPlaying())
            {
                anim.Play(parameter);
            }
            else
            {
                Debug.Log($"Uma animação já está em execução. {gameObject.name}");
            }
        }

        private bool IsAnimationPlaying()
        {
            return anim.IsInTransition(0) || anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
        }
    }
}