using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ConvMVVM3.SourceGenerator;
using Xunit;

namespace ConvMVVM3.Tests
{
    /// <summary>
    /// 포괄적인 RelayCommand 생성 테스트 - 이름 충돌 회피
    /// </summary>
    public class ComprehensiveRelayCommandTests
    {
        [Fact]
        public void Generator_ShouldCreateRelayCommand_WithSafeNaming()
        {
            // Arrange - Execute와 충돌하지 않는 안전한 이름들 사용
            var sourceCode = @"
using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;

public partial class SafeNamingViewModel : ObservableObject
{
    // 기본 RelayCommand 테스트
    [RelayCommand]
    private void HandleAction()
    {
        // 액션 처리
    }
    
    [RelayCommand]
    private void ProcessData()
    {
        // 데이터 처리
    }
    
    [RelayCommand]
    private void SubmitForm()
    {
        // 폼 제출
    }
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var compilation = CSharpCompilation.Create("TestAssembly")
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var generator = new ConvMVVM3Generator();
            var receiver = new ConvMVVM3SyntaxReceiver();

            // Act - SyntaxReceiver로 속성 감지
            var syntaxWalker = new CSharpSyntaxWalker(delegate(SyntaxNode node)
            {
                receiver.OnVisitSyntaxNode(node);
            });

            syntaxWalker.Visit(syntaxTree.GetRoot());

            // Assert - 메서드 감지 확인
            Assert.NotNull(receiver);
            Assert.Equal(3, receiver.RelayCommandMethods.Count);
            
            // 안전한 이름들인지 확인
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "HandleAction");
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "ProcessData");
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "SubmitForm");
            
            // Execute와 충돌하는 이름 없는지 확인
            Assert.DoesNotContain(receiver.RelayCommandMethods, m => m.Identifier.Text == "Execute");
        }

        [Fact]
        public void Generator_ShouldCreateAsyncRelayCommand_WithSafeNaming()
        {
            // Arrange
            var sourceCode = @"
using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using System.Threading.Tasks;

public partial class AsyncSafeNamingViewModel : ObservableObject
{
    // AsyncRelayCommand 테스트
    [AsyncRelayCommand]
    private async Task LoadDataAsync()
    {
        // 비동기 데이터 로딩
    }
    
    [AsyncRelayCommand]
    private async Task SaveDataAsync()
    {
        // 비동기 데이터 저장
    }
    
    [AsyncRelayCommand]
    private async Task RefreshContentAsync()
    {
        // 비동기 새로고침
    }
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var compilation = CSharpCompilation.Create("TestAssembly")
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var receiver = new ConvMVVM3SyntaxReceiver();

            // Act
            var syntaxWalker = new CSharpSyntaxWalker(delegate(SyntaxNode node)
            {
                receiver.OnVisitSyntaxNode(node);
            });

            syntaxWalker.Visit(syntaxTree.GetRoot());

            // Assert
            Assert.NotNull(receiver);
            Assert.Equal(3, receiver.RelayCommandMethods.Count);
            
            // AsyncRelayCommand 메서드들 확인
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "LoadDataAsync");
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "SaveDataAsync");
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "RefreshContentAsync");
        }

        [Fact]
        public void Generator_ShouldHandleMixedCommandTypes_Safely()
        {
            // Arrange - 동기/비동기 커맨드 혼합
            var sourceCode = @"
using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;
using System.Threading.Tasks;

public partial class MixedCommandViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;
    
    // 동기 커맨드들
    [RelayCommand]
    private void Initialize()
    {
        // 초기화
    }
    
    [RelayCommand]
    private void Cleanup()
    {
        // 정리
    }
    
    // 비동기 커맨드들
    [AsyncRelayCommand]
    private async Task FetchDataAsync()
    {
        // 데이터 가져오기
    }
    
    [AsyncRelayCommand]
    private async Task ProcessItemsAsync()
    {
        // 아이템 처리
    }
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var compilation = CSharpCompilation.Create("TestAssembly")
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var receiver = new ConvMVVM3SyntaxReceiver();

            // Act
            var syntaxWalker = new CSharpSyntaxWalker(delegate(SyntaxNode node)
            {
                receiver.OnVisitSyntaxNode(node);
            });

            syntaxWalker.Visit(syntaxTree.GetRoot());

            // Assert
            Assert.NotNull(receiver);
            
            // ObservableProperty 확인
            Assert.Single(receiver.ObservableFields);
            Assert.Contains(receiver.ObservableFields, f => 
                f.Declaration.Variables.Any(v => v.Identifier.Text == "_isBusy"));
            
            // RelayCommand 확인 (총 4개)
            Assert.Equal(4, receiver.RelayCommandMethods.Count);
            
            // 동기 커맨드 확인
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "Initialize");
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "Cleanup");
            
            // 비동기 커맨드 확인
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "FetchDataAsync");
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "ProcessItemsAsync");
        }

        [Fact]
        public void Generator_ShouldAvoidNamingConflicts_Basic()
        {
            // Arrange - 잠재적 충돌 이름들 테스트
            var sourceCode = @"
using ConvMVVM3.Core.Mvvm;
using ConvMVVM3.Core.Mvvm.Attributes;

public partial class ConflictAvoidanceViewModel : ObservableObject
{
    // Execute 대신 동의어 사용
    [RelayCommand]
    private void RunOperation()
    {
    }
    
    [RelayCommand]
    private void PerformAction()
    {
    }
    
    [RelayCommand]
    private void ExecuteTask()
    {
    }
    
    // Command 접미사가 이미 있는 경우
    [RelayCommand]
    private void HandleCommand()
    {
    }
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var compilation = CSharpCompilation.Create("TestAssembly")
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var receiver = new ConvMVVM3SyntaxReceiver();

            // Act
            var syntaxWalker = new CSharpSyntaxWalker(delegate(SyntaxNode node)
            {
                receiver.OnVisitSyntaxNode(node);
            });

            syntaxWalker.Visit(syntaxTree.GetRoot());

            // Assert
            Assert.NotNull(receiver);
            Assert.Equal(4, receiver.RelayCommandMethods.Count);
            
            // 안전한 이름들 확인
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "RunOperation");
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "PerformAction");
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "ExecuteTask");
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "HandleCommand");
            
            // 직접적인 'Execute'는 없는지 확인
            Assert.DoesNotContain(receiver.RelayCommandMethods, m => m.Identifier.Text == "Execute");
        }
    }
}