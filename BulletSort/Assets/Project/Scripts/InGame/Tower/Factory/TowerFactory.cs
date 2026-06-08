using Towers.Factory.Type;
using Towers.Interface.Tower;
using UnityEngine;

namespace Towers.Factory
{
    public class TowerFactory : MonoBehaviour
    {
        [SerializeField] private GameObject _basicPrefab;
        [SerializeField] private GameObject _nonBasicPrefab;
        [SerializeField] private GameObject _shoutGunPrefab;

        public ITower CreateTower(ETowerType type, Transform spawnTransform)
        {
            GameObject prefab = type switch
            {
                ETowerType.Basic => _basicPrefab,
                ETowerType.NonBasic => _nonBasicPrefab,
                ETowerType.Shotgun => _shoutGunPrefab,
                _ => throw new System.ArgumentException($"잘못된 타입 : {type}")
            };

            var pos = spawnTransform.position;
            // 풀방식으로 변경할 수도 있다.
            GameObject instance = Instantiate(prefab, pos, Quaternion.identity);
            instance.transform.SetParent(spawnTransform);
            
            return instance.GetComponent<ITower>();
        }
    }
}
