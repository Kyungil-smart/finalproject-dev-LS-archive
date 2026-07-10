using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    [Tooltip("Audio Mixer에 설정된 파라미터 이름 (ex: Master, BGM, SFX)")]
    [SerializeField] private string _parameterName;
    
    private Slider _slider;
    
    private void Awake()
    {
        _slider = GetComponent<Slider>();
        
        // 슬라이더 기본 세팅 (공식에 맞게 최소값을 0.0001로 설정)
        _slider.minValue = 0.0001f;
        _slider.maxValue = 1.0f;
    }
    
    private void Start()
    {
        // 매니저에서 저장된 값 가져와서 슬라이더 갱신 (씬 진입 시)
        if (SoundManager.Instance != null)
        {
            _slider.value = SoundManager.Instance.Volume.GetVolume(_parameterName);
        }
        
        // 믹서 로드가 아직 안 끝났을 수 있으므로 대기 후 초기값 반영
        StartCoroutine(InitSliderWhenReady());
    }
    
    // SoundManager 믹서 로드 완료 후 저장된 볼륨으로 슬라이더 초기화
    private IEnumerator InitSliderWhenReady()
    {
        // Instance 생성 대기
        while (SoundManager.Instance == null)
            yield return null;
        
        // Addressable 믹서 로드 완료 대기
        while (!SoundManager.Instance.IsReady)
            yield return null;
        
        // 이벤트 발생 방지를 위해 리스너 해제 후 값 설정
        _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        _slider.value = SoundManager.Instance.Volume.GetVolume(_parameterName);
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
    }
    
    private void OnSliderValueChanged(float value)
    {
        // 매니저로 값 전달
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Volume.SetVolume(_parameterName, value);
        }
    }
    
    private void OnDestroy()
    {
        _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }
}
