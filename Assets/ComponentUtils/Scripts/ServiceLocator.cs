using System;
using System.Collections.Generic;

namespace ComponentUtils.ComponentUtils.Scripts
{
    public class ServiceLocator
    {
        // Dicion�rio que armazena servi�os por tipo, espec�fico para uma inst�ncia
        private Dictionary<Type, object> services = new Dictionary<Type, object>();

        // Registra um novo servi�o para a inst�ncia
        public void RegisterService<T>(T service)
        {
            var type = typeof(T);
            if (services.ContainsKey(type))
            {
             //   Debug.Log($"ServiceLocator: Servi�o do tipo {type} j� registrado. Substituindo pelo novo.");
                services[type] = service;  // Substitui o servi�o existente
            }
            else
            {
                services.Add(type, service);
              //  Debug.Log($"ServiceLocator: Servi�o do tipo {type} registrado com sucesso.");
            }
        }

        // Remove um servi�o para a inst�ncia
        public void UnregisterService<T>()
        {
            var type = typeof(T);
            if (services.ContainsKey(type))
            {
                services.Remove(type);
            }
        }

        // Retorna um servi�o registrado para a inst�ncia
        public T GetService<T>()
        {
            var type = typeof(T);
            if (services.ContainsKey(type))
            {
                return (T)services[type];
            }
            else
            {
                throw new Exception($"Servi�o do tipo {type} n�o est� registrado para esta entidade.");
            }
        }

        // Limpa todos os servi�os registrados para a inst�ncia
        public void ClearAllServices()
        {
            services.Clear();
        }
    }
}
