using ComponentUtils;
using ComponentUtils.ComponentUtils.Scripts;
using Tcp4.Resources.Scripts.Systems.CollisionCasters;
using Tcp4.Resources.Scripts.Systems.Interaction;
using Tcp4.Resources.Scripts.Types;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Core
{
    public abstract class BaseEntity : MonoBehaviour
    {
        public BaseEntitySO baseStatus;   
        public EntityConfigSO entityConfig;
        public ServiceLocator ServiceLocator { get; private set; }

        // Componentes 
        public StatusComponent StatusComp { get; private set; }
        public Animator Anim { get; private set; }
        public Rigidbody Rb { get; private set; }
        public Collider Coll { get; private set; }
        public CollisionComponent Checker { get; private set; }
        public InteractableHandler InteractableHandler { get; private set; }
        public virtual void Awake()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            ServiceLocator = new ServiceLocator();
            entityConfig?.SetupComponents(gameObject, ServiceLocator);
            ServiceLocator?.RegisterService(baseStatus);
            SetupComponents();
        }
        
        private void SetupComponents()
        {
            StatusComp = Get<StatusComponent>(ComponentType.StatusComponent);
            Anim = Get<Animator>(ComponentType.Animator, SearchScope.InChildren);
            Rb = Get<Rigidbody>(ComponentType.Rigidbody);
            Coll = Get<Collider>(ComponentType.Collider);
            Checker = Get<CollisionComponent>(ComponentType.CollisionComponent);
            InteractableHandler = Get<InteractableHandler>(ComponentType.InteractableHandler);
        }
        
        public T Get<T>(ComponentType type, SearchScope scope = SearchScope.Self) where T : Component => 
            entityConfig.GetComponent(gameObject, (ComponentType)type, (SearchScope)scope) as T;
    }

}