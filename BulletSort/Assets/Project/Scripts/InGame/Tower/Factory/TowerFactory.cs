using Core;
using InGame.Tower.Data;
using Towers.Factory.Type;
using Towers.Interface.Tower;
using UnityEngine;

namespace Towers.Factory
{
    public class TowerFactory : MonoBehaviour
    {
        [SerializeField] private GameObject _basicPrefab;
        [SerializeField] private GameObject _shoutGunPrefab;
        [SerializeField] private GameObject _longRangePrefab;
        [SerializeField] private GameObject _tankPrefab;
        [SerializeField] private GameObject _widePrefab;
        [SerializeField] private GameObject _bufferPrefab;

        
        
        public ITower CreateTower(int TowerID, Transform spawnTransform)
        {
            TowerData data = DataManager.Instance.GetData<TowerData>(TowerID);
            
            ETowerType type = (ETowerType)data.TowerType;
            
            //타워 아이디에 따른 프리팹호출
            GameObject prefab = type switch
            {
                ETowerType.Basic => _basicPrefab,
                ETowerType.Shotgun => _shoutGunPrefab,
                ETowerType.LongRange => _longRangePrefab,
                ETowerType.Tank => _tankPrefab,
                ETowerType.Wide => _widePrefab,
                ETowerType.Buffer => _bufferPrefab,
                _ => throw new System.ArgumentException($"잘못된 타입 : {type}")
            };

            var pos = spawnTransform.position;
            // 풀방식으로 변경할 수도 있다.
            GameObject instance = Instantiate(prefab, pos, Quaternion.identity);
            instance.transform.SetParent(spawnTransform);
            instance.GetComponent<ITower>().SetData(data);
            
            return instance.GetComponent<ITower>();
        }
    }
}
