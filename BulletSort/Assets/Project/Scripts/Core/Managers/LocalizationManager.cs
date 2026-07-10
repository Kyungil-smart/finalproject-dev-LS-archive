using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Core
{
    // 언어 적용·저장 — Unity Localization 패키지의 SelectedLocale을 교체.
    //   Language enum ↔ Locale Code 매핑은 여기 한 곳에만. 다른 코드는 enum만 안다.
    //   테이블·Locale 에셋은 팀 공용(김시온 CSV 임포트) — 코드는 선택만 바꿈.
    // 저장은 PlayerPrefs — 재화·인벤토리와 달리 민감하지 않아 암호화 세이브 불필요.
    // 앱 시작 시 GameManager.Init에서 Restore() 코루틴으로 복원.
    // 작성자: 이성규
    public static class LocalizationManager
    {
        private const string PREF_KEY = "language";

        // enum → Locale Code. Localization 창의 Locale 식별자(ko/en/ja)와 일치해야 함.
        private static string ToCode(Language lang) => lang switch
        {
            Language.EN => "en",
            Language.JA => "ja",
            _ => "ko",
        };

        // Code → enum. 알 수 없는 값은 KO로 폴백(세이브 손상·구버전 대비).
        private static Language FromCode(string code) => code switch
        {
            "en" => Language.EN,
            "ja" => Language.JA,
            _ => Language.KO,
        };

        // 저장된 언어 — 없으면 KO. 설정창이 초기 선택값으로 사용.
        public static Language Current => FromCode(PlayerPrefs.GetString(PREF_KEY, "ko"));

        // 언어 적용 + 저장. SelectedLocale 대입 시점에 화면의 LocalizeStringEvent들이 갱신됨.
        public static void SetLanguage(Language lang)
        {
            PlayerPrefs.SetString(PREF_KEY, ToCode(lang));
            PlayerPrefs.Save();

            var locale = LocalizationSettings.AvailableLocales.GetLocale(ToCode(lang));
            if (locale == null)
            {
                Debug.LogWarning($"[Localization] Locale 없음: {ToCode(lang)} — Localization Settings에 등록 확인 필요.");
                return;
            }

            LocalizationSettings.SelectedLocale = locale;
        }

        // 앱 시작 시 저장된 언어 복원.
        //   AvailableLocales 조회 전에 초기화가 끝나야 하므로 InitializationOperation을 기다림.
        public static IEnumerator Restore()
        {
            yield return LocalizationSettings.InitializationOperation;
            SetLanguage(Current);
        }
    }
}