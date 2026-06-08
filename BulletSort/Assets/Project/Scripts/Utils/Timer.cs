using UnityEngine;

/*
 *  내용 요약: 타이머 기능
 *  작성자 : 안정연
 */

public class Timer
{
    // 시작시간
    private float _timeStart;
    // 현재시간
    private float _timeCurrent;
    // 끝나는 시간
    private float _timeEnd;
    
    public bool IsEnabled { get; private set; }
    
    public Timer(float maxTime)
    {
        ResetTimer(maxTime);
    }

    // 타이머 업데이트
    public void UpdateTimer()
    {
        // 현재시간에서 시작시간을 빼 경과한 시간을 계산
        _timeCurrent = Time.time - _timeStart;
        
        // 경과시간이 설정한 시간을 넘으면 타이머 종료
        if (_timeCurrent >= _timeEnd)
        {
            EndTimer();
        }
    }

    // 타이머 초기화
    public void ResetTimer(float timeMax)
    {
        _timeStart = Time.time;
        _timeCurrent = 0;
        _timeEnd = timeMax;
        IsEnabled = false;
    }
    
    // 타이머 종료
    private void EndTimer()
    {
        _timeCurrent = _timeEnd;
        IsEnabled = true;
    }
}
