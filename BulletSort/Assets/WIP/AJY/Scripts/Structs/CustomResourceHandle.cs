using System;
using UnityEngine;

namespace Util.Custom
{
    public enum CustomResourceStatus {None, Succeeded, Failed}

    public struct CustomResourceHandle<T> where T : UnityEngine.Object
    {
        private readonly ResourceRequest _request;
        // 로드 완료된 에셋 캐싱용
        private T _cachedResult;
        // 콜백 중복 실행 방지
        private bool _isCompleted;
        
        public event Action<CustomResourceHandle<T>> Completed;
        
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
            Completed = null;
            _cachedResult = null;
            _isCompleted = false;

            if (_request != null)
            {
                _request.completed += OnLoadCompleted;
            }
        }

        private void OnLoadCompleted(AsyncOperation op)
        {
            // 완료처리 되었다면 리턴
            if(_isCompleted) return;
            
            _cachedResult = _request.asset as T;
            TriggerCompletion();
        }
        
        // 완료 처리 메서드
        private void TriggerCompletion()
        {
            _isCompleted = true;
            Status = (_cachedResult != null) ? CustomResourceStatus.Succeeded : CustomResourceStatus.Failed;
            Completed?.Invoke(this);
        }
    }
}
