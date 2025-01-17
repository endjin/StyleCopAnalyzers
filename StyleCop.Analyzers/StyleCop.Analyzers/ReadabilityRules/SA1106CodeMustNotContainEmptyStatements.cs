﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.ReadabilityRules
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// The C# code contains an extra semicolon.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs when the code contain an extra semicolon. Syntactically, this results in
    /// an extra, empty statement in the code.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SA1106CodeMustNotContainEmptyStatements : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="SA1106CodeMustNotContainEmptyStatements"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "SA1106";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(ReadabilityResources.SA1106Title), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(ReadabilityResources.SA1106MessageFormat), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(ReadabilityResources.SA1106Description), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly string HelpLink = "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1106.md";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.ReadabilityRules, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink, WellKnownDiagnosticTags.Unnecessary);

        private static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnosticsValue =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return SupportedDiagnosticsValue;
            }
        }

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(HandleCompilationStart);
        }

        private static void HandleCompilationStart(CompilationStartAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionHonorExclusions(HandleEmptyStatementSyntax, SyntaxKind.EmptyStatement);
            context.RegisterSyntaxNodeActionHonorExclusions(HandleTypeSyntax, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeActionHonorExclusions(HandleTypeSyntax, SyntaxKind.StructDeclaration);
            context.RegisterSyntaxNodeActionHonorExclusions(HandleTypeSyntax, SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeActionHonorExclusions(HandleTypeSyntax, SyntaxKind.EnumDeclaration);
            context.RegisterSyntaxNodeActionHonorExclusions(HandleNamespaceSyntax, SyntaxKind.NamespaceDeclaration);
        }

        private static void HandleTypeSyntax(SyntaxNodeAnalysisContext context)
        {
            var declaration = (BaseTypeDeclarationSyntax)context.Node;

            if (declaration.SemicolonToken.IsKind(SyntaxKind.SemicolonToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.SemicolonToken.GetLocation()));
            }
        }

        private static void HandleNamespaceSyntax(SyntaxNodeAnalysisContext context)
        {
            var declaration = (NamespaceDeclarationSyntax)context.Node;

            if (declaration.SemicolonToken.IsKind(SyntaxKind.SemicolonToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.SemicolonToken.GetLocation()));
            }
        }

        private static void HandleEmptyStatementSyntax(SyntaxNodeAnalysisContext context)
        {
            EmptyStatementSyntax syntax = (EmptyStatementSyntax)context.Node;

            LabeledStatementSyntax labeledStatementSyntax = syntax.Parent as LabeledStatementSyntax;
            if (labeledStatementSyntax != null)
            {
                BlockSyntax blockSyntax = labeledStatementSyntax.Parent as BlockSyntax;
                if (blockSyntax != null)
                {
                    for (int i = blockSyntax.Statements.Count - 1; i >= 0; i--)
                    {
                        StatementSyntax statement = blockSyntax.Statements[i];

                        // allow an empty statement to be used for a label, but only if no non-empty statements exist
                        // before the end of the block
                        if (blockSyntax.Statements[i] == labeledStatementSyntax)
                        {
                            return;
                        }

                        if (!statement.IsKind(SyntaxKind.EmptyStatement))
                        {
                            break;
                        }
                    }
                }
            }

            // Code must not contain empty statements
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, syntax.GetLocation()));
        }
    }
}
