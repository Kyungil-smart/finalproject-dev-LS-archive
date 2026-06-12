using Core;

namespace InGame.Slot.Data
{
    // 슬롯 정적 데이터 조회 — DataManager가 보유한 SlotData 테이블에 위임.
    // PieceQuery와 대칭: 데이터 보유는 DataManager(싱글톤, Resources 로드), 도메인 조회는 여기.
    // 슬롯은 SlotDataID로 조회한다
    //   (슬롯 표시는 가동 포탑 타입에 따라 달라지고, 슬롯별 정적값도 차등될 수 있어
    //    단일 기본값으로 묶지 않음. 호출 측이 자기 SlotDataID로 조회)
    // 전제: DataManager가 GetData<T>(int id) 단건 조회를 제공.
    // 작성자: 이성규
    public static class SlotQuery
    {
        // SlotData ID로 조회. 없으면 null (호출 측에서 폴백 처리).
        public static SlotData Get(int slotDataID)
        {
            return DataManager.Instance.GetData<SlotData>(slotDataID);
        }
    }
}