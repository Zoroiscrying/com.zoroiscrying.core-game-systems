using System.Collections.Generic;
using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.AudioSystem
{
    public abstract class ComponentPool<T> where T:Component
    {
        private readonly T prefab;
    
        private readonly Queue<T> pool = new Queue<T>();
        private readonly LinkedList<T> inuse = new LinkedList<T>();
        private readonly Queue<LinkedListNode<T>> nodePool = new Queue<LinkedListNode<T>>();

        private Transform poolParentTransform;
 
        private int lastCheckFrame = -1;
 
        protected ComponentPool(T prefab, Transform poolParentTransform = null)
        {
            this.prefab = prefab;
            this.poolParentTransform = poolParentTransform;
        }
 
        private void CheckInUse()
        {
            var node = inuse.First;
            while (node != null)
            {
                var current = node;
                node = node.Next;
 
                if (!IsActive(current.Value))
                {
                    current.Value.gameObject.SetActive(false);
                    pool.Enqueue(current.Value);
                    inuse.Remove(current);
                    nodePool.Enqueue(current);
                }
            }
        }
 
        protected T Get()
        {
            T item;
 
            if (lastCheckFrame != Time.frameCount)
            {
                lastCheckFrame = Time.frameCount;
                CheckInUse();
            }
 
            if (pool.Count == 0)
            {
                //Debug.Log("No Room Available");
                item = GameObject.Instantiate(prefab, poolParentTransform, true);
            }
            else
                item = pool.Dequeue();
 
            if (nodePool.Count == 0)
                inuse.AddLast(item);
            else
            {
                var node = nodePool.Dequeue();
                node.Value = item;
                inuse.AddLast(node);
            }
         
            item.gameObject.SetActive(true);
 
            return item;
        }
 
        protected abstract bool IsActive(T component);
    }
}