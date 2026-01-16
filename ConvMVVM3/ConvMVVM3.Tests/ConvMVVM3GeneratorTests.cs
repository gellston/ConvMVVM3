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
    public class ConvMVVM3GeneratorTests
    {
        [Fact]
        public void Generator_ShouldDetectObservablePropertyAttributes()
        {
            // Arrange
            var sourceCode = @"
using ConvMVVM3.Core.Mvvm.Attributes;

public class TestViewModel
{
    [ObservableProperty]
    private string _name;
    
    [ObservableProperty]
    private int _age;
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var compilation = CSharpCompilation.Create("TestAssembly")
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var generator = new ConvMVVM3Generator();
            var receiver = new ConvMVVM3SyntaxReceiver();

            // Act
            var syntaxWalker = new CSharpSyntaxWalker(delegate(SyntaxNode node)
            {
                receiver.OnVisitSyntaxNode(node);
            });

            syntaxWalker.Visit(syntaxTree.GetRoot());

            // Assert
            Assert.NotNull(receiver);
            Assert.Equal(2, receiver.ObservableFields.Count);
            Assert.Contains(receiver.ObservableFields, f => 
                f.Declaration.Variables.Any(v => v.Identifier.Text == "_name"));
            Assert.Contains(receiver.ObservableFields, f => 
                f.Declaration.Variables.Any(v => v.Identifier.Text == "_age"));
        }

        [Fact]
        public void Generator_ShouldDetectRelayCommandAttributes()
        {
            // Arrange
            var sourceCode = @"
using ConvMVVM3.Core.Mvvm.Attributes;

public class TestViewModel
{
    [RelayCommand]
    private void Save()
    {
    }
    
    [RelayCommand]
    private void Cancel()
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
            Assert.Equal(2, receiver.RelayCommandMethods.Count);
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "Save");
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "Cancel");
        }

        [Fact]
        public void Generator_ShouldDetectMixedAttributes()
        {
            // Arrange
            var sourceCode = @"
using ConvMVVM3.Core.Mvvm.Attributes;

public class TestViewModel
{
    [ObservableProperty]
    private string _name;
    
    [ObservableProperty]
    private int _age;
    
    [RelayCommand]
    private void Save()
    {
    }
    
    [RelayCommand]
    private void Cancel()
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
            Assert.Equal(2, receiver.ObservableFields.Count);
            Assert.Equal(2, receiver.RelayCommandMethods.Count);
            
            // Check ObservableProperty fields
            Assert.Contains(receiver.ObservableFields, f => 
                f.Declaration.Variables.Any(v => v.Identifier.Text == "_name"));
            Assert.Contains(receiver.ObservableFields, f => 
                f.Declaration.Variables.Any(v => v.Identifier.Text == "_age"));
            
            // Check RelayCommand methods
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "Save");
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "Cancel");
        }

        [Fact]
        public void Generator_ShouldIgnoreFieldsWithoutAttributes()
        {
            // Arrange
            var sourceCode = @"
public class TestViewModel
{
    private string _name;
    private int _age;
    private void SaveCommand()
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
            Assert.Empty(receiver.ObservableFields);
            Assert.Empty(receiver.RelayCommandMethods);
        }

        [Fact]
        public void Generator_ShouldHandlePartialAttributeNames()
        {
            // Arrange
            var sourceCode = @"
using ConvMVVM3.Core.Mvvm.Attributes;

public class TestViewModel
{
    [ObservablePropertyAttribute]
    private string _name;
    
    [RelayCommandAttribute]
    private void Save()
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
            Assert.Single(receiver.ObservableFields);
            Assert.Single(receiver.RelayCommandMethods);
            Assert.Equal("_name", receiver.ObservableFields.First().Declaration.Variables.First().Identifier.Text);
            Assert.Equal("Save", receiver.RelayCommandMethods.First().Identifier.Text);
        }
    }

    // Helper delegate for syntax walker
    public delegate void SyntaxNodeVisitor(SyntaxNode node);

    // Simple syntax walker for testing
    public class CSharpSyntaxWalker
    {
        private readonly SyntaxNodeVisitor _visitor;

        public CSharpSyntaxWalker(SyntaxNodeVisitor visitor)
        {
            _visitor = visitor;
        }

        public void Visit(SyntaxNode node)
        {
            if (node != null)
            {
                _visitor(node);
                foreach (var child in node.ChildNodes())
                {
                    Visit(child);
                }
            }
        }
    }
}