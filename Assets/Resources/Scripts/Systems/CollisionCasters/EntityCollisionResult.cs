using Tcp4.Resources.Scripts.Core;
using Tcp4.Resources.Scripts.Interfaces;

namespace Tcp4.Resources.Scripts.Systems.CollisionCasters
{
    public class EntityCollisionResult : ICollisionResult
    {
        private BaseEntity entity;
        public BaseEntity Entity => entity;

        public void SetEntity(BaseEntity e)
        {
            this.entity = e;
        }

        public void Reset()
        {
            entity = null;
        }
    }
}