using System;
using Core.ObjectPool;
using Towers;
using UnityEngine;

public class ProjectileFactory : MonoBehaviour
{
   [SerializeField] private GameObject _normalPrefab;
   [SerializeField] private GameObject _snipePrefab;
   [SerializeField] private GameObject _tankPrefab;
   [SerializeField] private GameObject _explosivePrefab;
   [SerializeField] private GameObject _healPrefab;
   
   public GameObject CreateProjectile(EProjectileType type, int count)
   {
      GameObject prefab = type switch
      {
         EProjectileType.Normal => _normalPrefab,
         EProjectileType.Snipe => _snipePrefab,
         EProjectileType.Tank => _tankPrefab,
         EProjectileType.Heal => _healPrefab,
         EProjectileType.Explosive => _explosivePrefab,
         _ => throw new SystemException($"잘못된 타입 : {type}")
      };
      
      PoolManager.Instance.CreatePool(prefab, count); 
      return prefab;
   }
}
