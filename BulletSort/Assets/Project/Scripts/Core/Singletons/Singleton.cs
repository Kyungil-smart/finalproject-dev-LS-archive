using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 제네릭이 아닌 싱글톤 베이스.
    /// [RuntimeInitializeOnLoadMethod]는 제네릭 클래스에 둘 수 없으므로
    /// static 상태 리셋 로직을 이 비제네릭 클래스로 분리한다.
    /// </summary>
    public abstract class SingletonBase : MonoBehaviour
    {
        // 앱 전체의 종료 상태 (모든 싱글톤이 공유 — 의미상 전역이 맞음)
        protected static bool _isQuitting;
        public static bool IsQuitting => _isQuitting;

        // 각 Singleton<T>가 자기 _instance를 비우는 함수를 등록해 둔다.
        // 중복 없는 모음이므로 HashSet — Add가 자동으로 중복을 거른다.
        private static readonly HashSet<Action> _resetActions = new();

        protected static void RegisterResetAction(Action reset)
        {
            // HashSet.Add — 이미 있으면 false, 자동으로 중복 스킵.
            // (단, reset 델리게이트가 매번 같은 인스턴스여야 중복 판정이 동작)
            _resetActions.Add(reset);
        }

        // 도메인 리로드를 꺼도 플레이 진입 시 가장 먼저 실행되어 static을 초기화
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetAllStatics()
        {
            _isQuitting = false;
            foreach (var reset in _resetActions)
                reset();
        }
    }

    /// <summary>
    /// 제네릭 싱글톤 베이스. 단일 인스턴스 보장 · 씬 전환 생존 · 종료 안전성 제공.
    /// 자식은 Awake가 아니라 Init()을 오버라이드한다 — 중복 인스턴스 보호를 베이스가 강제한다.
    /// </summary>
    public class Singleton<T> : SingletonBase where T : Component
    {
        // UDR0001 경고: Singleton<T>는 제네릭이라 [RuntimeInitializeOnLoadMethod]를
        // 직접 둘 수 없다. static 리셋은 비제네릭 SingletonBase가 레지스트리로 처리한다.
        // → 진단기가 이 패턴을 인식 못 해 경고가 남지만, 의도된 설계이므로 무시.
#pragma warning disable UDR0001
        private static T _instance;
#pragma warning restore UDR0001

        // 이 타입의 _instance를 비우는 리셋 함수 (한 번만 만들어 재사용 — HashSet 중복 판정의 전제)
        private static readonly Action _resetInstance = () => _instance = null;

        public static T Instance
        {
            get
            {
                if (IsQuitting)
                {
                    Debug.LogWarning($"[Singleton] 인스턴스 '{typeof(T)}' 는 이미 파괴됨. 재생성 방지.");
                    return null;
                }

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        var obj = new GameObject
                        {
                            name = $"[Singleton] {typeof(T).Name}"
                        };
                        _instance = obj.AddComponent<T>();
                    }

                    // 인스턴스가 생긴 경로에서 리셋 함수 등록 보장
                    RegisterResetAction(_resetInstance);
                }
                return _instance;
            }
        }

        /// <summary>
        /// Unity 콜백. sealed로 막아 자식이 실수로 오버라이드하지 못하게 한다.
        /// 중복 인스턴스 보호를 여기서 처리하고, 초기화는 Init()으로 위임한다.
        /// </summary>
        protected void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;

                // 씬에 미리 배치된 인스턴스도 리셋 대상으로 등록
                RegisterResetAction(_resetInstance);

                if (transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }

                // 살아남은 인스턴스에서만 초기화 실행
                Init();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] {typeof(T).Name} 의 중복 인스턴스를 발견. 중복 삭제 실행.");
                Destroy(gameObject);
                // 여기서 Init()을 호출하지 않으므로 중복 인스턴스는 초기화되지 않는다.
            }
        }

        /// <summary>
        /// 자식 클래스의 초기화 진입점. Awake 대신 이 메서드를 오버라이드한다.
        /// 베이스가 "살아남은 단일 인스턴스"에서만 호출하므로,
        /// 중복 인스턴스 체크(if Instance != this return)를 자식이 직접 할 필요가 없다.
        /// </summary>
        protected virtual void Init() { }

        protected virtual void OnDestroy()
        {
            // 이 인스턴스가 현재 등록된 인스턴스라면 참조를 비운다.
            // (중복 인스턴스가 파괴되는 경우엔 _instance를 건드리지 않음)
            if (_instance == this)
                _instance = null;
        }

        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }
    }
}