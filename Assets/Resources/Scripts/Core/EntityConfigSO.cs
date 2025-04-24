using System;
using System.Collections.Generic;
using ComponentUtils;
using ComponentUtils.ComponentUtils.Scripts;
using Tcp4.Resources.Scripts.Systems.CollisionCasters;
using Tcp4.Resources.Scripts.Systems.Interaction;
using Tcp4.Resources.Scripts.Types;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Core
{
       [CreateAssetMenu(fileName = "NewEntityConfig", menuName = "Entity/Config")]
    public class EntityConfigSO : ScriptableObject
    {
        [Serializable]
        public class ComponentConfiguration
        {
            public ComponentType Type;
            public SearchScope Scope = SearchScope.Self;
        }

        public List<ComponentConfiguration> ComponentsToSetup;

        public void SetupComponents(GameObject entity, ServiceLocator serviceLocator)
        {
            foreach (var config in ComponentsToSetup)
            {
                var component = FindOrCreateComponent(entity, config);
                if (component != null)
                {
                    serviceLocator.RegisterService(component);
                }
            }
        }

        private Component FindOrCreateComponent(GameObject entity, ComponentConfiguration config)
        {
            var component = FindComponent(entity, config);
            if (component == null)
            {
                component = CreateComponent(entity, config);
            }
            return component;
        }

        private Component FindComponent(GameObject entity, ComponentConfiguration config)
        {
            return config.Scope switch
            {
                SearchScope.Self => entity.GetComponent(GetComponentType(config.Type)),
                SearchScope.InChildren => entity.GetComponentInChildren(GetComponentType(config.Type)),
                SearchScope.InParent => entity.GetComponentInParent(GetComponentType(config.Type)),
                _ => null
            };
        }

        private Component CreateComponent(GameObject entity, ComponentConfiguration config)
        {
            var componentType = GetComponentType(config.Type);
            return componentType != null ? entity.AddComponent(componentType) : null;
        }

        private Type GetComponentType(ComponentType componentType)
        {
            return componentType switch
            {
                ComponentType.StatusComponent => typeof(StatusComponent),
                ComponentType.Animator => typeof(Animator),
                ComponentType.SpriteRenderer => typeof(SpriteRenderer),
                ComponentType.Rigidbody => typeof(Rigidbody),
                ComponentType.Collider => typeof(Collider),
                ComponentType.CollisionComponent => typeof(CollisionComponent),
                ComponentType.InteractableHandler => typeof(InteractableHandler),
                _ => null
            };
        }

        public Component GetComponent(GameObject entity, ComponentType type, SearchScope scope)
        {
            return scope switch
            {
                SearchScope.Self => entity.GetComponent(GetComponentType(type)),
                SearchScope.InChildren => entity.GetComponentInChildren(GetComponentType(type)),
                SearchScope.InParent => entity.GetComponentInParent(GetComponentType(type)),
                _ => null
            };
        }
    }
}