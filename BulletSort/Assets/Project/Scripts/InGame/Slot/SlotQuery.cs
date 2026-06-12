using Core;

namespace InGame.Slot.Data
{
    // 슬롯 정적 데이터 조회 — DataManager가 보유한 SlotData 테이블에 위임.
    // PieceQuery와 대칭: 데이터 보유는 DataManager(싱글톤, Resources 로드), 도메인 조회는 여기
    // 1차 기준 슬롯 정적값는 전 슬롯 공통 1개 → GetDefault로 조회.
    //   SlotID별 차등이 생기면 Get(slotID)로 확장(인터페이스는 미리 열어둠).
    // 전제: DataManager가 GetData<T>(int id) 단건 조회를 제공.
    // 작성자: 이성규
    public class SlotQuery
    {
        
    }
}
