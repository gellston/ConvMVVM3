using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class RelayCommandAttribute : Attribute
    {
        #region Public Property
        /// <summary>
        /// (선택) 생성될 Command 프로퍼티 이름을 강제로 지정.
        /// null이면 제너레이터 규칙(보통 메서드명 + "Command")을 따른다.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// (선택) CanExecute 메서드 이름.
        /// 예: nameof(CanSave)
        /// </summary>
        public string CanExecute { get; set; }

        /// <summary>
        /// (선택) Command 실행 중 예외를 삼킬지/전파할지 등 정책을 원하면 여기서 확장 가능.
        /// 지금은 제너레이터가 무시해도 되고, 나중에 쓰기 위한 자리.
        /// </summary>
        public bool CatchExceptions { get; set; }
        #endregion
    }
}
