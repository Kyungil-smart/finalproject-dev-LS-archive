using UnityEngine;
using UnityEngine.Audio;

namespace Sound
{
    public class MixerVolumeController
    {
        private AudioMixer _mixer;
        private const float MIN_SLIDER_VALUE = 0.0001f;
    
        // PlayerPrefs 영구 저장 대신, 런타임 동안 유지될 볼륨 상태 변수
        public float Master_Volume { get; private set; } = 1.0f;
        public float BGM_Volume { get; private set; } = 1.0f;
        public float SFX_Volume { get; private set; } = 1.0f;
    
        public void Init(AudioMixer mixer)
        {
            _mixer = mixer;
            SetVolume("Master", Master_Volume);
            SetVolume("BGM", BGM_Volume);
            SetVolume("SFX", SFX_Volume);
        }
    
        // 유니티 오디오 믹서 볼륨 공식
        // 공식: dB = 20 * log10(SliderValue)
        public void SetVolume(string parameterName, float sliderValue)
        {
            if (_mixer == null) return;
        
            float clamped = Mathf.Max(sliderValue, MIN_SLIDER_VALUE);
            float db = Mathf.Log10(clamped) * 20f;
            _mixer.SetFloat(parameterName, db);

            switch (parameterName)
            {
                case "Master": Master_Volume = sliderValue; break;
                case "BGM":    BGM_Volume    = sliderValue; break;
                case "SFX":    SFX_Volume    = sliderValue; break;
            }
        }
    
        public float GetVolume(string parameterName)
        {
            switch (parameterName)
            {
                case "Master": return Master_Volume;
                case "BGM":    return BGM_Volume;
                case "SFX":    return SFX_Volume;
                default:       return 1.0f;
            }
        }
    }
}
