using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Core
{
    // 요약 : 화면 오버레이 디버그 로거. Debug.Log와 달리 빌드에서도 화면에 로그가 보인다.
    // 작성자 : 이성규
    public class Logger : Singleton<Logger>
    {
        [SerializeField]
        private TextMeshProUGUI debugAreaText;
        [SerializeField]
        private bool enableDebug = true;
        [SerializeField]
        private int maxLines = 15;

        // 텍스트 데이터를 관리할 큐 (메모리 최적화)
        private Queue<string> _logQueue = new Queue<string>();

        protected override void Init()
        {
            // 릴리즈 빌드에서는 동작 금지
            if (!Debug.isDebugBuild)
            {
                gameObject.SetActive(false);
                return;
            }

            if (debugAreaText == null)
            {
                debugAreaText = GetComponent<TextMeshProUGUI>();
            }

            // 시작할 때 텍스트 초기화 및 상태 동기화
            if (debugAreaText != null)
            {
                debugAreaText.text = string.Empty;
                debugAreaText.enabled = enableDebug;
            }
            
            Log($"{GetType().Name} initialized.", "white");
        }

        // 로그 함수 (큐를 통한 디버그 텍스트 선입 선출)
        private void Log(string message, string color)
        {
            if (!enableDebug || debugAreaText == null) return;

            // 큐에 새로운 메시지 추가
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string formattedLine = $"<color=\"{color}\">{timestamp} {message}</color>";
            _logQueue.Enqueue(formattedLine);

            // 최대 줄 수 넘어가면 가장 오래된 줄 제거
            if (_logQueue.Count > maxLines)
            {
                _logQueue.Dequeue();
            }

            // UI 갱신 (큐의 내용을 합쳐서 출력)
            // string.Join 사용으로 줄나눔 및 로그 큐의 내용을 합치기
            debugAreaText.text = string.Join("\n", _logQueue);
        }

#if UNITY_EDITOR
        // 에디터상에서만 enableDebug 체크 상태 변경으로 변하는 동작 확인
        private void OnValidate()
        {
            if (debugAreaText != null)
            {
                debugAreaText.enabled = enableDebug;
            }
        }
#endif

        public void LogInfo(string message) => Log(message, "green");
        public void LogError(string message) => Log(message, "red");
        public void LogWarning(string message) => Log(message, "yellow");

        // 로그 클리어
        public void Clear()
        {
            _logQueue.Clear();
            if (debugAreaText != null) debugAreaText.text = string.Empty;
        }
    }
}