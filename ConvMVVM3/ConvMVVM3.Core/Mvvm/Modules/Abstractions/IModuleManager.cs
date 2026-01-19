using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Core.Mvvm.Modules.Abstractions
{
    public interface IModuleManager
    {
        #region Public Property
        IReadOnlyCollection<ModuleDescriptor> Modules { get; }
        #endregion


        #region Public Functions

        // 자동 발견(특정 경로)
        void DiscoverFromDirectory(string directory);

        void AddBlockName(string blockName);

        // 수동 추가
        void Add(IModule module);

        // 시작 모듈(WhenAvailable) 초기화
        void InitializeStartupModules();

        // OnDemand 로드
        void LoadModule(string name);

        bool IsLoaded(string name);
        #endregion
    }
}
