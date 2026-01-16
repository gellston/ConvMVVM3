using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using ConvMVVM3.SourceGenerator;

namespace ConvMVVM3.Tests
{
    public class SyntaxReceiverUnitTests
    {
        [Fact]
        public void SyntaxReceiver_ShouldDetectObservablePropertyAttributes()
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
            var receiver = new ConvMVVM3SyntaxReceiver();

            // Act
            var walker = new AttributeDetectionWalker(receiver);
            walker.Visit(syntaxTree.GetRoot());

            // Assert
            Assert.NotNull(receiver);
            Assert.Equal(2, receiver.ObservableFields.Count);
            Assert.Contains(receiver.ObservableFields, f => 
                f.Declaration.Variables.Any(v => v.Identifier.Text == "_name"));
            Assert.Contains(receiver.ObservableFields, f => 
                f.Declaration.Variables.Any(v => v.Identifier.Text == "_age"));
        }

        [Fact]
        public void SyntaxReceiver_ShouldDetectRelayCommandAttributes()
        {
            // Arrange
            var sourceCode = @"
using ConvMVVM3.Core.Mvvm.Attributes;

public class TestViewModel
{
    [RelayCommand]
    private void SaveCommand()
    {
    }
    
    [RelayCommand]
    private void CancelCommand()
    {
    }
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var receiver = new ConvMVVM3SyntaxReceiver();

            // Act
            var walker = new AttributeDetectionWalker(receiver);
            walker.Visit(syntaxTree.GetRoot());

            // Assert
            Assert.NotNull(receiver);
            Assert.Equal(2, receiver.RelayCommandMethods.Count);
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "SaveCommand");
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "CancelCommand");
        }

        [Fact]
        public void SyntaxReceiver_ShouldIgnoreFieldsWithoutAttributes()
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
            var receiver = new ConvMVVM3SyntaxReceiver();

            // Act
            var walker = new AttributeDetectionWalker(receiver);
            walker.Visit(syntaxTree.GetRoot());

            // Assert
            Assert.NotNull(receiver);
            Assert.Empty(receiver.ObservableFields);
            Assert.Empty(receiver.RelayCommandMethods);
        }

        [Fact]
        public void SyntaxReceiver_ShouldHandleFullAttributeNames()
        {
            // Arrange
            var sourceCode = @"
using ConvMVVM3.Core.Mvvm.Attributes;

public class TestViewModel
{
    [ObservablePropertyAttribute]
    private string _name;
    
    [RelayCommandAttribute]
    private void SaveCommand()
    {
    }
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var receiver = new ConvMVVM3SyntaxReceiver();

            // Act
            var walker = new AttributeDetectionWalker(receiver);
            walker.Visit(syntaxTree.GetRoot());

            // Assert
            Assert.NotNull(receiver);
            Assert.Single(receiver.ObservableFields);
            Assert.Single(receiver.RelayCommandMethods);
            Assert.Equal("_name", receiver.ObservableFields.First().Declaration.Variables.First().Identifier.Text);
            Assert.Equal("SaveCommand", receiver.RelayCommandMethods.First().Identifier.Text);
        }
    }

    // Helper walker that calls OnVisitSyntaxNode for each node
    public class AttributeDetectionWalker
    {
        private readonly ConvMVVM3SyntaxReceiver _receiver;

        public AttributeDetectionWalker(ConvMVVM3SyntaxReceiver receiver)
        {
            _receiver = receiver;
        }

        public void Visit(SyntaxNode node)
        {
            if (node != null)
            {
                _receiver.OnVisitSyntaxNode(node);
                foreach (var child in node.ChildNodes())
                {
                    Visit(child);
                }
            }
        }
    }
}