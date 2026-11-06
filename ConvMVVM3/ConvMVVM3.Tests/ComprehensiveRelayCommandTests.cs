using ConvMVVM3.Core.Mvvm;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ConvMVVM3.Tests;

public class ComprehensiveRelayCommandTests
{
    [Theory]
    [InlineData("Execute", false)]
    [InlineData("HandleCommand", false)]
    [InlineData("LoadDataAsync", true)]
    [InlineData("Execute", true)]
    public void GeneratedCommand_CompilesAndInvokesAttributedMethod(string method, bool async)
    {
        var attribute = async ? "AsyncRelayCommand" : "RelayCommand";
        var returnType = async ? "System.Threading.Tasks.Task" : "void";
        var body = async ? "Calls++; return System.Threading.Tasks.Task.CompletedTask;" : "Calls++;";
        var source = $$"""
            using ConvMVVM3.Core.Mvvm.Attributes;
            public partial class GeneratedViewModel
            {
                public int Calls { get; private set; }
                [{{attribute}}] private {{returnType}} {{method}}() { {{body}} }
            }
            """;
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator).Append(typeof(ObservableObject).Assembly.Location)
            .Distinct().Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("Commands_" + Guid.NewGuid().ToString("N"),
            new[] { CSharpSyntaxTree.ParseText(source) }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SourceGenerator.ConvMVVM3Generator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.All(driver.GetRunResult().Results, result => Assert.Null(result.Exception));
        using var stream = new MemoryStream();
        var emitted = output.Emit(stream);
        Assert.True(emitted.Success, string.Join(Environment.NewLine, emitted.Diagnostics));
        var assembly = System.Reflection.Assembly.Load(stream.ToArray());
        var type = assembly.GetType("GeneratedViewModel")!;
        var vm = Activator.CreateInstance(type)!;
        var property = type.GetProperty(method + "Command")!;
        var command = Assert.IsAssignableFrom<System.Windows.Input.ICommand>(property.GetValue(vm));
        Assert.Same(command, property.GetValue(vm));
        command.Execute(null);
        Assert.Equal(1, type.GetProperty("Calls")!.GetValue(vm));
    }
}
