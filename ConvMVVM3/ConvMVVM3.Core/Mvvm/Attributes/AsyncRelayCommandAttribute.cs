using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class AsyncRelayCommandAttribute : Attribute
    {
        /// <summary>
        /// (선택) 생성될 Command 프로퍼티 이름 지정.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// (선택) CanExecute 메서드 이름.
        /// </summary>
        public string CanExecute { get; set; }

        /// <summary>
        /// 동시에 여러 번 실행 허용 여부.
        /// 보통 장비/IO는 false가 안전.
        /// </summary>
        public bool AllowConcurrentExecutions { get; set; }

        /// <summary>
        /// 실행 중에는 CanExecute가 false가 되게 만들지(제너레이터 구현 정책용).
        /// </summary>
        public bool DisableWhileRunning { get; set; }

        /// <summary>
        /// 예외 처리 정책용(원하면 제너레이터가 반영).
        /// </summary>
        public bool CatchExceptions { get; set; }

        public AsyncRelayCommandAttribute()
        {
            // 보수적 기본값(장비/IO 앱에 안전한 편)
            AllowConcurrentExecutions = false;
            DisableWhileRunning = true;
            CatchExceptions = false;
        }
    }
}
