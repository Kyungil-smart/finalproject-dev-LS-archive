using Towers.Interface.Tower;
using Towers.Struct.TowerInfo;
using UnityEngine;

namespace Towers.Factory
{
    public class BasicTower : MonoBehaviour, ITower
    {
        private STowerInfo _towerInfo;
        //private TargetDetector _targetDetector;

        private void Awake()
        {
            // 테스트용 하드코드
            _towerInfo = new STowerInfo("기본형 타워");
            //_targetDetector = gameObject.GetComponent<TargetDetector>();
        }

        private void Start()
        {
            _towerInfo.ShowData();
            //_targetDetector.SetDetectRange();
        }

        // SO데이터 불러오기 구현 필요
        private void Init()
        {
            
        }

        public void Attack()
        {
            // 투사체 생성
        }
    }
}
