using System.Collections.Generic;
using Unity.Mathematics;


namespace Core
{
    // 요약: 2차원 격자 데이터 구조체. (x, y) 좌표로 칸을 읽고 쓸 수 있는 단순 컨테이너.
    // 매치-3 프로젝트에서 작성, 본 프로젝트에선 보관용 (1차원 변형 SGrid1D를 슬롯에 사용).
    // 추후 덱 편성 보유 유닛 그리드 정렬 등에 활용 가능.
    // 작성자 : 이성규
    public struct SGrid2D<T>
    {
        // 내부 데이터
        T[] cells;
        int2 size;
    
        public SGrid2D(int2 size)
        {
            this.size = size;
            cells = new T[size.x * size.y];
        }
    
        // 외부 접근용 프로퍼티
        public int2 Size => size;
        public int SizeX => size.x;
        public int SizeY => size.y;
    
        // 상태 확인
        public bool IsUndefined => cells == null || cells.Length == 0;
        public bool AreValidCoordinates(int2 c) =>
            0 <= c.x && c.x < size.x && 0 <= c.y && c.y < size.y;
    
        // 인덱서: 2차원 좌표 (x, y)를 1차원 배열 인덱스(y * width + x)로 변환하여 접근
        public T this[int x, int y]
        {
            get { return cells[y * size.x + x]; }
            set => cells[y * size.x + x] = value;
        }
        public T this[int2 c]
        {
            get => cells[c.y * size.x + c.x];
            set => cells[c.y * size.x + c.x] = value;
        }
    
        // 두 좌표의 데이터를 서로 교체
        public void Swap(int2 a, int2 b) => (this[a], this[b]) = (this[b], this[a]);
    
        // 초기화 기능
        public void Clear()
        {
            System.Array.Clear(cells, 0, cells.Length);
        }
    
        // 보드 전체를 순회하며 (좌표, 값)을 반환하는 반복자
        public IEnumerable<(int2 pos, T value)> GetAllCells()
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    yield return (new int2(x, y), this[x, y]);
                }
            }
        }
    
        // 보드의 지정된 Y 범위만 순회하며 (좌표, 값)을 반환하는 반복자
        public IEnumerable<(int2 pos, T value)> GetCellsInRange(int startY, int endY)
        {
            for (int y = startY; y < endY; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    yield return (new int2(x, y), this[x, y]);
                }
            }
        }
    }
}