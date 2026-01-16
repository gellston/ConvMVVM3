using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using ConvMVVM3.SourceGenerator;

namespace ConvMVVM3.Tests
{
    public class ComprehensiveAttributeTests
    {
        [Fact]
        public void SyntaxReceiver_ShouldDetectAllAttributes()
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
    private void SaveCommand()
    {
    }
    
    [RelayCommand]
    private void CancelCommand()
    {
    }
    
    [NotifyPropertyChangedFor(""OtherProperty"")]
    private string _notifyingField;
    
    [NotifyCanExecuteChangedFor(""SaveCommand"", ""CancelCommand"")]
    private void UpdateCanExecute()
    {
    }
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var receiver = new ConvMVVM3SyntaxReceiver();

            // Act
            var walker = new ComprehensiveAttributeWalker(receiver);
            walker.Visit(syntaxTree.GetRoot());

            // Assert
            Assert.NotNull(receiver);
            
            // Check ObservableProperty detection
            Assert.Equal(2, receiver.ObservableFields.Count);
            Assert.Contains(receiver.ObservableFields, f => 
                f.Declaration.Variables.Any(v => v.Identifier.Text == "_name"));
            Assert.Contains(receiver.ObservableFields, f => 
                f.Declaration.Variables.Any(v => v.Identifier.Text == "_age"));
            
            // Check RelayCommand detection
            Assert.Equal(2, receiver.RelayCommandMethods.Count);
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "SaveCommand");
            Assert.Contains(receiver.RelayCommandMethods, m => m.Identifier.Text == "CancelCommand");
        }

        [Fact]
        public void SyntaxReceiver_ShouldDetectNotifyPropertyChangedForAttribute()
        {
            // Arrange
            var sourceCode = @"
using ConvMVVM3.Core.Mvvm.Attributes;

public class TestViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(""Name"", ""Age"")]
    private string _notifyingField;
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var receiver = new ConvMVVM3SyntaxReceiver();

            // Act
            var walker = new ComprehensiveAttributeWalker(receiver);
            walker.Visit(syntaxTree.GetRoot());

            // Assert
            Assert.NotNull(receiver);
            Assert.Single(receiver.ObservableFields);
            var field = receiver.ObservableFields.First();
            Assert.Equal("_notifyingField", field.Declaration.Variables.First().Identifier.Text);
        }

        [Fact]
        public void SyntaxReceiver_ShouldDetectNotifyCanExecuteChangedForAttribute()
        {
            // Arrange
            var sourceCode = @"
using ConvMVVM3.Core.Mvvm.Attributes;

public class TestViewModel
{
    [RelayCommand]
    [NotifyCanExecuteChangedFor(""SaveCommand"", ""CancelCommand"")]
    private void UpdateCanExecute()
    {
    }
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var receiver = new ConvMVVM3SyntaxReceiver();

            // Act
            var walker = new ComprehensiveAttributeWalker(receiver);
            walker.Visit(syntaxTree.GetRoot());

            // Assert
            Assert.NotNull(receiver);
            Assert.Single(receiver.RelayCommandMethods);
            var method = receiver.RelayCommandMethods.First();
            Assert.Equal("UpdateCanExecute", method.Identifier.Text);
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
    private void Save()
    {
    }
    
    [ObservablePropertyAttribute]
    [NotifyPropertyChangedForAttribute(""Name"")]
    private string _notifyingField;
    
    [RelayCommand]
    [NotifyCanExecuteChangedForAttribute(""SaveCommand"")]
    private void UpdateCanExecute()
    {
    }
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var receiver = new ConvMVVM3SyntaxReceiver();

            // Act
            var walker = new ComprehensiveAttributeWalker(receiver);
            walker.Visit(syntaxTree.GetRoot());

            // Assert
            Assert.NotNull(receiver);
            Assert.Equal(2, receiver.ObservableFields.Count); // _name, _notifyingField
            Assert.Equal(2, receiver.RelayCommandMethods.Count); // Save, UpdateCanExecute
        }

        [Fact]
        public void SyntaxReceiver_ShouldIgnoreNonAttributeMembers()
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
    private void UpdateCanExecute()
    {
    }
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var receiver = new ConvMVVM3SyntaxReceiver();

            // Act
            var walker = new ComprehensiveAttributeWalker(receiver);
            walker.Visit(syntaxTree.GetRoot());

            // Assert
            Assert.NotNull(receiver);
            Assert.Empty(receiver.ObservableFields);
            Assert.Empty(receiver.RelayCommandMethods);
        }
    }

    // Enhanced walker that properly visits all nodes
    public class ComprehensiveAttributeWalker
    {
        private readonly ConvMVVM3SyntaxReceiver _receiver;

        public ComprehensiveAttributeWalker(ConvMVVM3SyntaxReceiver receiver)
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