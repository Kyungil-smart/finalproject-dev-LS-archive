using UnityEngine;

namespace Core.ObjectPool.Interface
{
    public interface IPoolable
    {
        public GameObject KeyObject { get; set; }
        
        // 풀에서 꺼내질 때
        void OnSpawn();

        // 풀에 들어갈 때
        void OnDespawn();
    }
}
    

