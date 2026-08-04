using UnityEngine;

public partial class WaveData : ScriptableObject
{
    public int this[int index]
    {
        get
        {
            return index switch
            {
                0 => WavePattern_1,
                1 => WavePattern_2,
                2 => WavePattern_3,
                3 => WavePattern_4,
                4 => WavePattern_5,
                5 => WavePattern_6,
                6 => WavePattern_7,
                7 => WavePattern_8,
                8 => WavePattern_9,
                _ => throw new System.IndexOutOfRangeException($"Index Out Of Bound in Wave Data {index}")
            };
        }
    }
}
