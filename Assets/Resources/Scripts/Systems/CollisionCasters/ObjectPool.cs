using System.Collections.Generic;

namespace Tcp4.Resources.Scripts.Systems.CollisionCasters
{
    public class ObjectPool<T> where T : class
    {
        private readonly System.Func<T> createFunc;
        private readonly Stack<T> pool = new Stack<T>();
        private const int InitialSize = 10;

        public ObjectPool(System.Func<T> createFunc)
        {
            this.createFunc = createFunc;
            // Pré-aloca alguns objetos
            for (int i = 0; i < InitialSize; i++)
            {
                pool.Push(createFunc());
            }
        }

        public T Get()
        {
            return pool.Count > 0 ? pool.Pop() : createFunc();
        }

        public void Release(T item)
        {
            pool.Push(item);
        }
    }
}