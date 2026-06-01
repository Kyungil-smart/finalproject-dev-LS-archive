using System.Collections.Generic;

namespace Core
{
    // 요약: 1차원 격자 데이터 구조체. 인덱스로 칸을 읽고 쓰는 단순 컨테이너.
    // 기존 매치-3에서 사용했던 2차원 구조체 SGrid2D<T>를 1차원 셀 리스트에 맞게 개조함
    // 작성자 : 이성규
    public struct SGrid1D<T>
    {
        // 내부 데이터
        T[] cells;
        int size;

        // 생성자
        public SGrid1D(int size)
        {
            this.size = size;
            cells = new T[size];
        }

        // 외부 접근용 프로퍼티
        public int Size => size;

        // 상태 확인
        public bool IsUndefined => cells == null || cells.Length == 0;
        public bool IsValidIndex(int i) => i >= 0 && i < size;

        // 인덱서: 인덱스로 셀에 직접 접근
        public T this[int i]
        {
            get => cells[i];
            set => cells[i] = value;
        }

        // 두 인덱스의 데이터를 서로 교체
        public void Swap(int a, int b) => (this[a], this[b]) = (this[b], this[a]);
    
        // 초기화 기능
        public void Clear()
        {
            System.Array.Clear(cells, 0, cells.Length);
        }
    
        // 슬롯 내부를 순회하며 인덱스와 타입을 반환하는 반복자
        public IEnumerable<(int index, T value)> GetAllCells()
        {
            for (int i = 0; i < size; i++)
                yield return (i, this[i]);
        }
    }
}