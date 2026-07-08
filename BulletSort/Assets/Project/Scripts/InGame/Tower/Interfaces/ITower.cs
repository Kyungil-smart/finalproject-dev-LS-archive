using System.Collections;

namespace Towers.Interface.Tower
{
    public interface ITower
    {
        // 투사체 생성
        public IEnumerator Attack();
        public void StartAttack();

        // 데이터 저장
        public void SetData(TowerData towerData);

        // 탄배출
    }    
}

