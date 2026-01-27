#nullable enable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConvMVVM3.SourceGenerator
{
    [Generator]
    public class ConvMVVM3Generator : IIncrementalGenerator
    {
        // FullyQualifiedFormat 기본값은 nullable 표기를 빼는 경우가 있어서 옵션을 포함해둠
        private static readonly SymbolDisplayFormat s_fqFormat =
            SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            System.Diagnostics.Debug.WriteLine("ConvMVVM3 IncrementalGenerator Initialize called");

            // 1. 클래스 선언 필터링
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(static m => m is not null);

            // 2. 컴파일ation과 결합
            var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

            // 3. 소스 코드 생성
            context.RegisterSourceOutput(
                compilationAndClasses,
                static (spc, source) => Execute(source.Left, source.Right!, spc));
        }

        static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            // 클래스에 ObservableProperty 필드나 RelayCommand 메서드가 있는지 확인
            var hasObservableFields = false;
            var hasRelayCommands = false;

            foreach (var member in classDeclaration.Members)
            {
                if (member is FieldDeclarationSyntax field && field.AttributeLists.Any())
                {
                    foreach (var attrList in field.AttributeLists)
                    {
                        foreach (var attr in attrList.Attributes)
                        {
                            var attrName = attr.Name.ToString();
                            if (attrName.Contains("ObservableProperty"))
                            {
                                hasObservableFields = true;
                                System.Diagnostics.Debug.WriteLine(
                                    $"Found ObservableProperty on field: {field.Declaration.Variables.FirstOrDefault()?.Identifier.Text}");
                                break;
                            }
                        }
                    }
                }
                else if (member is MethodDeclarationSyntax method && method.AttributeLists.Any())
                {
                    foreach (var attrList in method.AttributeLists)
                    {
                        foreach (var attr in attrList.Attributes)
                        {
                            var attrName = attr.Name.ToString();
                            if (attrName.Contains("RelayCommand"))
                            {
                                hasRelayCommands = true;
                                System.Diagnostics.Debug.WriteLine($"Found RelayCommand on method: {method.Identifier.Text}");
                                break;
                            }
                        }
                    }
                }
            }

            if (hasObservableFields || hasRelayCommands)
            {
                System.Diagnostics.Debug.WriteLine($"Class {classDeclaration.Identifier.Text} has attributed members");
                return classDeclaration;
            }

            return null;
        }

        static void Execute(Compilation compilation, IReadOnlyList<ClassDeclarationSyntax> classes, SourceProductionContext context)
        {
            System.Diagnostics.Debug.WriteLine($"ConvMVVM3 IncrementalGenerator Execute called with {classes.Count} classes");

            // 항상 실행되었음을 증명하는 파일 생성
            var alwaysGeneratedCode = $@"// ConvMVVM3 IncrementalGenerator ALWAYS EXECUTED
namespace GeneratedTest
{{
    public static class AlwaysExecuted
    {{
        public const string Message = ""IncrementalGenerator Execute method was called!"";
        public const string Timestamp = ""{DateTime.Now:O}"";
        public const string AssemblyName = ""{compilation.AssemblyName}"";
        public const int ClassesCount = {classes.Count};
    }}
}}";
            context.AddSource("AlwaysExecuted.g.cs", alwaysGeneratedCode);

            if (classes.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No classes with attributes found");
                return;
            }

            foreach (var classDecl in classes)
            {
                try
                {
                    var semanticModel = compilation.GetSemanticModel(classDecl.SyntaxTree);
                    var source = GenerateForClass(classDecl, semanticModel);

                    if (!string.IsNullOrEmpty(source))
                    {
                        var fileName = $"{classDecl.Identifier.Text}_Generated.g.cs";
                        context.AddSource(fileName, source);
                        System.Diagnostics.Debug.WriteLine($"Generated source for {classDecl.Identifier.Text}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error generating for {classDecl.Identifier.Text}: {ex.Message}");
                }
            }
        }

        static string GenerateForClass(ClassDeclarationSyntax classDecl, SemanticModel semanticModel)
        {
            var sb = new StringBuilder();
            var ns = GetNamespace(classDecl);
            var className = classDecl.Identifier.Text;

            // 원본 클래스 modifiers 유지(접근제한자/abstract/sealed/partial 등)
            var modifiers = string.Join(" ", classDecl.Modifiers.Select(m => m.Text));
            if (!modifiers.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Contains("partial"))
                modifiers = string.IsNullOrWhiteSpace(modifiers) ? "partial" : (modifiers + " partial");

            // 생성된 코드가 원본 파일 using에 의존하지 않도록,
            // 필드 타입은 semanticModel을 통해 global::... FullyQualified로 출력한다.
            sb.AppendLine("using ConvMVVM3.Core.Mvvm;");
            sb.AppendLine("using ConvMVVM3.Core.Mvvm.Commands;");
            sb.AppendLine("using ConvMVVM3.Core.Mvvm.Attributes;");

            // ✅ 전역 네임스페이스면 namespace 블록 자체를 생성하지 않음 (namespace global 방지)
            if (!string.IsNullOrWhiteSpace(ns))
            {
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"    {modifiers} class {className}");
            sb.AppendLine("    {");

            // 필드 및 메서드 수집
            var observableFields = new List<FieldDeclarationSyntax>();
            var relayCommands = new List<MethodDeclarationSyntax>();

            foreach (var member in classDecl.Members)
            {
                if (member is FieldDeclarationSyntax field && HasObservablePropertyAttribute(field))
                    observableFields.Add(field);
                else if (member is MethodDeclarationSyntax method && (HasRelayCommandAttribute(method) || HasAsyncRelayCommandAttribute(method)))
                    relayCommands.Add(method);
            }

            // ObservableProperty로부터 프로퍼티 생성
            foreach (var field in observableFields)
            {
                // 같은 FieldDeclarationSyntax에 변수가 여러개일 수 있음: private int _a, _b;
                foreach (var variable in field.Declaration.Variables)
                {
                    var fieldName = variable.Identifier.Text;
                    var propertyName = fieldName.TrimStart('_');
                    if (propertyName.Length > 0)
                        propertyName = char.ToUpper(propertyName[0]) + propertyName.Substring(1);

                    // ✅ 핵심: using 누락으로 인한 타입 해석 실패 방지(ObservableCollection 등)
                    var fieldType = GetFullyQualifiedTypeName(field.Declaration.Type, semanticModel);

                    // 속성 변경 알림을 위한 속성 확인
                    var notifyChangedAttrs = GetNotifyAttributes(field, "NotifyPropertyChangedFor");
                    var notifyChangingAttrs = GetNotifyAttributes(field, "NotifyPropertyChangingFor");
                    var notifyCanExecuteAttrs = GetNotifyAttributes(field, "NotifyCanExecuteChangedFor");

                    sb.AppendLine($"        partial void On{propertyName}Changed({fieldType} oldValue, {fieldType} newValue);");
                    sb.AppendLine($"        partial void On{propertyName}Changing({fieldType} oldValue, {fieldType} newValue);");
                    sb.AppendLine($"        public {fieldType} {propertyName}");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            get {{ return {fieldName}; }}");
                    sb.AppendLine("            set");
                    sb.AppendLine("            {");
                    sb.AppendLine($"                var __oldValue = {fieldName};");
                    sb.AppendLine($"                On{propertyName}Changing(__oldValue, value);");
                    sb.AppendLine($"                if (SetProperty(ref {fieldName}, value))");
                    sb.AppendLine("                {");
                    sb.AppendLine($"                    On{propertyName}Changed(__oldValue, value);");

                    // NotifyPropertyChangingFor 속성 처리
                    foreach (var propName in notifyChangingAttrs)
                        sb.AppendLine($"                    OnPropertyChanging(\"{propName}\");");

                    // NotifyPropertyChangedFor 속성 처리
                    foreach (var propName in notifyChangedAttrs)
                        sb.AppendLine($"                    OnPropertyChanged(\"{propName}\");");

                    // NotifyCanExecuteChangedFor 속성 처리
                    foreach (var commandName in notifyCanExecuteAttrs)
                        sb.AppendLine($"                    {commandName}Command?.NotifyCanExecuteChanged();");

                    sb.AppendLine("                }");
                    sb.AppendLine("            }");
                    sb.AppendLine("        }");
                    sb.AppendLine();
                }
            }

            // RelayCommand로부터 Command 프로퍼티 생성
            foreach (var method in relayCommands)
            {
                var methodName = method.Identifier.Text;
                var isAsync = HasAsyncRelayCommandAttribute(method);
                var commandType = isAsync ? "AsyncRelayCommand" : "RelayCommand";
                var commandPropertyName = methodName + "Command";
                var backingFieldName = "_" + char.ToLower(methodName[0]) + methodName.Substring(1) + "Command";

                sb.AppendLine($"        private {commandType}? {backingFieldName};");
                sb.AppendLine($"        public {commandType} {commandPropertyName}");
                sb.AppendLine("        {");
                sb.AppendLine("            get");
                sb.AppendLine("            {");
                sb.AppendLine($"                if ({backingFieldName} == null)");
                sb.AppendLine("                {");
                sb.AppendLine($"                    {backingFieldName} = new {commandType}({methodName});");
                sb.AppendLine("                }");
                sb.AppendLine($"                return {backingFieldName};");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            sb.AppendLine("    }");

            if (!string.IsNullOrWhiteSpace(ns))
                sb.AppendLine("}");

            return sb.ToString();
        }

        static bool HasObservablePropertyAttribute(FieldDeclarationSyntax field)
        {
            return field.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString().Contains("ObservableProperty"));
        }

        static bool HasRelayCommandAttribute(MethodDeclarationSyntax method)
        {
            return method.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString().Contains("RelayCommand"));
        }

        static bool HasAsyncRelayCommandAttribute(MethodDeclarationSyntax method)
        {
            return method.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString().Contains("AsyncRelayCommand"));
        }

        static List<string> GetNotifyAttributes(FieldDeclarationSyntax field, string attributeName)
        {
            var result = new List<string>();

            foreach (var attrList in field.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    if (attr.Name.ToString().Contains(attributeName))
                    {
                        var args = attr.ArgumentList?.Arguments;
                        if (args != null)
                        {
                            foreach (var arg in args)
                            {
                                var propName = arg.ToString().Trim('"');
                                if (!string.IsNullOrEmpty(propName))
                                    result.Add(propName);
                            }
                        }
                    }
                }
            }

            return result;
        }

        // ✅ 파일스코프(namespace A.B;) + 블록(namespace A.B { }) 모두 지원
        // ✅ 전역 네임스페이스면 null 반환 (namespace global 생성 방지)
        static string? GetNamespace(ClassDeclarationSyntax classDecl)
        {
            var parts = classDecl.Ancestors()
                .OfType<BaseNamespaceDeclarationSyntax>()
                .Select(n => n.Name.ToString())
                .Reverse()
                .ToArray();

            if (parts.Length == 0)
                return null; // global namespace

            return string.Join(".", parts);
        }

        // ✅ using 누락으로 인한 타입 미해결 방지: global::... fully qualified 타입명 생성
        static string GetFullyQualifiedTypeName(TypeSyntax typeSyntax, SemanticModel semanticModel)
        {
            var typeSymbol = semanticModel.GetTypeInfo(typeSyntax).Type;
            if (typeSymbol == null)
                return typeSyntax.ToString();

            return typeSymbol.ToDisplayString(s_fqFormat);
        }
    }
}
