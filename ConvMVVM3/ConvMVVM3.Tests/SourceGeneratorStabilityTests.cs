using ConvMVVM3.Core.Mvvm;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ConvMVVM3.Tests;

public class SourceGeneratorStabilityTests
{
    private const string Imports = """
        using ConvMVVM3.Core.Mvvm;
        using ConvMVVM3.Core.Mvvm.Attributes;
        """;

    [Fact]
    public void SameClassNameInDifferentNamespacesGeneratesBothTypes()
    {
        var result = Run(true, Imports + """
            namespace Sales
            {
                public partial class MainViewModel : ObservableObject
                {
                    [ObservableProperty] private int _count;
                }
            }
            namespace Inventory
            {
                public partial class MainViewModel : ObservableObject
                {
                    [ObservableProperty] private int _count;
                }
            }
            public class Consumer
            {
                public int Read() => new Sales.MainViewModel().Count + new Inventory.MainViewModel().Count;
            }
            """);

        Assert.Equal(2, result.GeneratedTrees.Length);
        Assert.Equal(2, result.Results.Single().GeneratedSources.Select(s => s.HintName).Distinct().Count());
    }

    [Fact]
    public void PartialDeclarationsAreMergedWithEachFilesSemanticModel()
    {
        var result = Run(true,
            Imports + """
            using Number = System.Int32;
            namespace Example;
            public partial class ViewModel : ObservableObject
            {
                [ObservableProperty] private Number _count;
            }
            """,
            Imports + """
            using Number = System.String;
            namespace Example;
            public partial class ViewModel
            {
                [ObservableProperty] private Number _title = "";
                [RelayCommand] private void Save() { }
                public void UseGeneratedMembers() { Count = 1; Title = "saved"; SaveCommand.Execute(null); }
            }
            """);

        Assert.Single(result.GeneratedTrees);
    }

    [Fact]
    public void NestedAndGenericTypesKeepTheirContainingTypeAndArity()
    {
        var result = Run(true, Imports + """
            namespace Example;
            public partial class First<T>
            {
                public partial class ViewModel : ObservableObject
                {
                    [ObservableProperty] private T _value = default!;
                }
            }
            public partial class Second
            {
                public partial class ViewModel : ObservableObject
                {
                    [ObservableProperty] private int _count;
                }
            }
            public partial class ViewModel : ObservableObject
            {
                [ObservableProperty] private int _count;
            }
            public partial class ViewModel<T> : ObservableObject
            {
                [ObservableProperty] private T _value = default!;
            }
            """);

        Assert.Equal(4, result.GeneratedTrees.Length);
    }

    [Fact]
    public void UnattributedCompilationProducesNoDiagnosticHelperType()
    {
        Assert.Empty(Run(true, "public class Plain { }").GeneratedTrees);
    }

    [Fact]
    public void IdenticalInputsProduceIdenticalGeneratedSources()
    {
        var source = Imports + """
            public partial class ViewModel : ObservableObject
            {
                [ObservableProperty] private int _count;
            }
            """;
        var first = Run(true, source).Results.Single().GeneratedSources;
        var second = Run(true, source).Results.Single().GeneratedSources;

        Assert.Equal(first.Select(s => (s.HintName, s.SourceText.ToString())),
            second.Select(s => (s.HintName, s.SourceText.ToString())));
        Assert.DoesNotContain("AlwaysExecuted", first.Single().SourceText.ToString());
        Assert.DoesNotContain("Timestamp", first.Single().SourceText.ToString());
    }

    [Fact]
    public void GenerationFailureReportsLocationAndContinuesWithOtherTypes()
    {
        var result = Run(false, Imports + """
            public partial class Broken : ObservableObject
            {
                [ObservableProperty] private int _;
            }
            public partial class Healthy : ObservableObject
            {
                [ObservableProperty] private int _count;
            }
            """);

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("CONVMVVM001", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.True(diagnostic.Location.IsInSource);
        Assert.Equal("Broken", diagnostic.Location.SourceTree!.GetText()
            .ToString(diagnostic.Location.SourceSpan));
        Assert.Contains("cannot produce a property name", diagnostic.GetMessage());
        Assert.Single(result.GeneratedTrees);
        Assert.Contains("class Healthy", result.GeneratedTrees.Single().ToString());
    }

    private static GeneratorDriverRunResult Run(bool assertCompiles, params string[] sources)
    {
        var platformAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator);
        var references = platformAssemblies.Append(typeof(ObservableObject).Assembly.Location)
            .Distinct().Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("StabilityTests",
            sources.Select(source => CSharpSyntaxTree.ParseText(source)), references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new SourceGenerator.ConvMVVM3Generator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        var result = driver.GetRunResult();

        Assert.All(result.Results, generator => Assert.Null(generator.Exception));
        if (assertCompiles)
        {
            Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
            Assert.Empty(output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        }
        return result;
    }
}
