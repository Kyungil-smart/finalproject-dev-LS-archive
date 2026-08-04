using Core;
using Towers.Interface.Tower;
using UnityEngine;

namespace Towers.Factory
{
    public class TowerFactory : MonoBehaviour
    {
        [SerializeField] private GameObject _towerPrefab;
        
        public ITower CreateTower(int TowerID, Transform spawnTransform)
        {
            TowerData data = DataManager.Instance.GetData<TowerData>(TowerID);
            
            var pos = spawnTransform.position;
            // 풀방식으로 변경할 수도 있다.
            GameObject instance = Instantiate(_towerPrefab, pos, Quaternion.identity);
            instance.transform.SetParent(spawnTransform);
            instance.GetComponent<ITower>().SetData(data);
            
            return instance.GetComponent<ITower>();
        }
    }
}
