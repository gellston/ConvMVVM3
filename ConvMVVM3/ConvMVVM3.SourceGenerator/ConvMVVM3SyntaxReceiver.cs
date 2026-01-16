using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace ConvMVVM3.SourceGenerator
{
    public class ConvMVVM3SyntaxReceiver : ISyntaxReceiver
    {
        public List<FieldDeclarationSyntax> ObservableFields { get; } = new();
        public List<MethodDeclarationSyntax> RelayCommandMethods { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is FieldDeclarationSyntax field && HasObservablePropertyAttribute(field))
            {
                ObservableFields.Add(field);
            }
            else if (syntaxNode is MethodDeclarationSyntax method && HasRelayCommandAttribute(method))
            {
                RelayCommandMethods.Add(method);
            }
        }

        private bool HasObservablePropertyAttribute(FieldDeclarationSyntax field)
        {
            return field.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString().Contains("ObservableProperty"));
        }

        private bool HasRelayCommandAttribute(MethodDeclarationSyntax method)
        {
            return method.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString().Contains("RelayCommand"));
        }
    }
}