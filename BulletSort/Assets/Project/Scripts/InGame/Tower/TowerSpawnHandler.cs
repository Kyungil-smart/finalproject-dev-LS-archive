using Core.Manager.SpawnManager;
using InGame.Slot;
using Towers.Factory.Type;
using UnityEngine;

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
        
            // 2. pieceID → ETowerType 변환 ( 임시 매핑 추후 데이터 SO로)
            ETowerType type = (ETowerType)pieceID;

            // 3. 소환
            SpawnManager.Instance.SpawnTower(type, slot);
        }
    }
}
