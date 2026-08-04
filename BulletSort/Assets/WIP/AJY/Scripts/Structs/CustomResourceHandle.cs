using System;
using UnityEngine;

namespace Util.Custom
{
    public enum CustomResourceStatus {None, Succeeded, Failed}

    public class CustomResourceHandle<T> where T : UnityEngine.Object
    {
        private readonly ResourceRequest _request;
        // 로드 완료된 에셋 캐싱용
        private T _cachedResult;
        // 콜백 중복 실행 방지
        private bool _isCompleted;

        // 내부 콜백 대리자
        private Action<CustomResourceHandle<T>> _completedAction;
        public event Action<CustomResourceHandle<T>> Completed
        {
            add
            {
                // 완료된 시점 콜백 등록시
                if (_isCompleted)
                {
                    // 저장하지 않고 즉시 실행
                    value(this);
                }
                else
                {
                    // 완료 전이라면 이벤트 리스트에 추가
                    _completedAction += value;
                }
            }
            remove => _completedAction -= value;
        }
        
        public CustomResourceStatus Status { get; private set; }
        
        public bool IsDone => _request != null && _request.isDone;

        public T Result
        {
            get
            {
                if (_request == null) return null;
                
                // 이미 캐싱했다면 즉시 반환
                if(_cachedResult != null) return _cachedResult;
                
                // 완료된 에셋을 가져와 캐싱 및 상태 업데이트
                _cachedResult = _request.asset as T;

                // 완료처리가 안 됐으면 강제 처리
                if (!_isCompleted)
                {
                    TriggerCompletion();
                }

                return _cachedResult;
            }
        }

        public CustomResourceHandle(ResourceRequest request)
        {
            _request = request;
            Status = CustomResourceStatus.None;
            _completedAction = null;
            _cachedResult = null;
            _isCompleted = false;

            if (_request != null)
            {
                _request.completed += OnLoadCompleted;
            }
        }

        private void OnLoadCompleted(AsyncOperation op)
        {
            Debug.Log("리소스 로드 완료 이벤트 발생");
            // 완료처리 되었다면 리턴
            if(_isCompleted) return;
            
            _cachedResult = _request.asset as T;
            TriggerCompletion();
        }
        
        // 완료 처리 메서드
        private void TriggerCompletion()
        {
            Debug.Log("로드 완료처리");
            _isCompleted = true;
            Status = (_cachedResult != null) ? CustomResourceStatus.Succeeded : CustomResourceStatus.Failed;
            _completedAction?.Invoke(this);
            _completedAction = null;
        }
    }
}
