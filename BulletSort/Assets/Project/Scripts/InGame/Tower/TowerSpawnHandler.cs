using Core.Manager.SpawnManager;
using InGame.Slot;
using Towers.Factory.Type;
using Towers.Interface.Tower;
using UnityEngine;
using Logger = Core.Logger;

namespace Towers.Spawner.Handler
{
    public class TowerSpawnHandler : MonoBehaviour
    {
        [SerializeField] private SlotBoardManager _slotBoardManager;
        private void OnEnable()
        {
            if (_slotBoardManager != null)
                _slotBoardManager.OnSortSuccess += HandleSortSuccess;
        }

        private void OnDisable()
        {
            if (_slotBoardManager != null)
                _slotBoardManager.OnSortSuccess -= HandleSortSuccess;
        }

        // 이벤트 시그니처 그대로 받기 - (SlotID, PieceID)
        private void HandleSortSuccess(int slotID, int pieceID)
        {
            // 1. slotID → 슬롯 데이터
            Slot slot = _slotBoardManager.GetSlotByID(slotID);
        
            // 2. pieceID → 연결 포탑 ID (PieceData.ConnectTower 조회)
            int towerID = _slotBoardManager.GetConnectTowerID(pieceID);
            
            var queue     = slot.TurretQueue;

            // 3. towerID → ETowerType (타워 영역 매핑 — 추후 타워 SO 조회로 교체)
            Logger.Instance?.LogInfo($"{pieceID} 기물의 {towerID.ToString()} 포탑 소환 요청");
            
            // 4. 소환 → 큐 등록(가동/대기/오버소팅 판단은 큐가)
            ITower turret = SpawnManager.Instance.SpawnTower(towerID, slot);
            queue.RegisterTurret(turret);
        }
    }
}
