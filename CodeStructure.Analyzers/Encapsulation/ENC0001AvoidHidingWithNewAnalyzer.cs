// // ENC0001AvoidHidingWithNewAnalyzer.cs
// // Copyright © 2012–Present Jackalope Technologies, Inc. and Doug Gerard.
// // Use subject to the MIT License.

#region Usings

using System.Collections.Immutable;
using CodeStructure.Analyzers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#endregion

namespace CodeStructure.Analyzers.Encapsulation;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Enc0001AvoidHidingWithNewAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [DiagnosticDescriptors.ENC0001];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMember, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMember, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMember, SyntaxKind.EventDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMember, SyntaxKind.EventFieldDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMember, SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeMember(SyntaxNodeAnalysisContext context)
    {
        var memberDeclaration = (MemberDeclarationSyntax) context.Node;

        if (memberDeclaration.Modifiers.Any(SyntaxKind.NewKeyword))
        {
            string name = GetMemberName(memberDeclaration);
            var diagnostic =
                Diagnostic.Create(DiagnosticDescriptors.ENC0001, memberDeclaration.GetLocation(), name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static string GetMemberName(MemberDeclarationSyntax memberDeclaration)
    {
        var name = memberDeclaration.Kind().ToString();

        if (memberDeclaration is MethodDeclarationSyntax methodDeclaration)
            name = methodDeclaration.Identifier.Text;
        else
        {
            if (memberDeclaration is PropertyDeclarationSyntax propertyDeclaration)
                name = propertyDeclaration.Identifier.Text;
            else
            {
                if (memberDeclaration is EventDeclarationSyntax eventDeclaration)
                    name = eventDeclaration.Identifier.Text;
                else
                {
                    if (memberDeclaration is FieldDeclarationSyntax fieldDeclaration)
                    {
                        if (fieldDeclaration.Declaration.Variables.Count > 0)
                            name = fieldDeclaration.Declaration.Variables[index: 0].Identifier.Text;
                    }
                }
            }
        }

        return name;
    }
}
