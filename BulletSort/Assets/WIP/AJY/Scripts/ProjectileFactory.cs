using System;
using Core.ObjectPool;
using Towers;
using UnityEngine;

public class ProjectileFactory : MonoBehaviour
{
   [SerializeField] private GameObject _normalPrefab;
   [SerializeField] private GameObject _explosivePrefab;
   [SerializeField] private GameObject _snipePrefab;
   
   public GameObject CreateProjectile(EProjectileType type, int count)
   {
      GameObject prefab = type switch
      {
         EProjectileType.Normal => _normalPrefab,
         EProjectileType.Explosive => _explosivePrefab,
         EProjectileType.Snipe => _snipePrefab,
         _ => throw new SystemException($"잘못된 타입 : {type}")
      };
      
      PoolManager.Instance.CreatePool(prefab, count);
      return prefab;
   }
}
