using System.Collections.Generic;
using UnityEngine;
using Core.ObjectPool.Interface;

namespace Core.ObjectPool
{
    public class PoolManager : Singleton<PoolManager>
    {
        private Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
        
        protected override void Init()
        {
            // 풀에 미리 생성하고 싶을시 CreatePool(prefab, count) 사용
        }

        // 풀에 count만큼 미리 생성해두는 메서드
        public void CreatePool(GameObject prefab, int count)
        {
            if (!_pools.ContainsKey(prefab))
            {
                _pools.Add(prefab, new Queue<GameObject>());
            }
            
            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.name = prefab.name;
                obj.SetActive(false);
                _pools[prefab].Enqueue(obj);
            }
        }

        // 풀에서 꺼내오는 메서드
        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            // _pools에 prefeb으로 등록된 키가 없으면 Queue 새로 만듬 
            if (!_pools.ContainsKey(prefab))
            {
                _pools.Add(prefab, new Queue<GameObject>());
            }

            GameObject obj;

            if (_pools[prefab].Count > 0)
            {
                obj = _pools[prefab].Dequeue();
            }
            else
            {
                // ObjectPoolManager 자식으로 Instantiate
                obj = Instantiate(prefab, transform);
                obj.name = prefab.name;
            }

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);

            if (obj.TryGetComponent<IPoolable>(out IPoolable poolable))
            {
                poolable.OnSpawn();
            }

            return obj;
        }

        // 풀에 넣어놓는 메서드
        public void Release(GameObject prefab, GameObject obj)
        {
            if (!_pools.ContainsKey(prefab))
            {
                _pools.Add(prefab, new Queue<GameObject>());
            }

            if (obj.TryGetComponent<IPoolable>(out IPoolable poolable))
            {
                poolable.OnDespawn();
            }

            SetActiveFalse(obj);
        
            _pools[prefab].Enqueue(obj);
        }
        
        // 디버그 코스
        private void SetActiveFalse(GameObject obj)
        {
            obj.SetActive(false);
            // Debug.Log($"{obj.name} 비활성화");
        }
    }
}    


