﻿namespace StyleCop.Analyzers.Settings
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using StyleCop.Analyzers.DocumentationRules;
    using StyleCop.Analyzers.Helpers;

    /// <summary>
    /// Implements a code fix that will generate a StyleCop settings file if it does not exist yet.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SettingsFileCodeFixProvider))]
    [Shared]
    public class SettingsFileCodeFixProvider : CodeFixProvider
    {
        private const string StyleCopSettingsFileName = "stylecop.json";
        private const string DefaultSettingsFileContent = @"{
  ""settings"": {
    ""documentationRules"": {
      ""companyName"": ""PlaceholderCompany""
    }
  }
}
";

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(
                SA1633FileMustHaveHeader.DiagnosticId,
                SA1634FileHeaderMustShowCopyright.DiagnosticId,
                SA1635FileHeaderMustHaveCopyrightText.DiagnosticId,
                SA1636FileHeaderCopyrightTextMustMatch.DiagnosticId,
                SA1637FileHeaderMustContainFileName.DiagnosticId,
                SA1638FileHeaderFileNameDocumentationMustMatchFileName.DiagnosticId,
                SA1639FileHeaderMustHaveSummary.DiagnosticId,
                SA1640FileHeaderMustHaveValidCompanyText.DiagnosticId,
                SA1641FileHeaderCompanyNameTextMustMatch.DiagnosticId,
                SA1649FileHeaderFileNameDocumentationMustMatchTypeName.DiagnosticId);

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var project = context.Document.Project;
            var workspace = project.Solution.Workspace;

            // check if the settings file already exists
            if (project.AdditionalDocuments.Any(IsStyleCopSettingsDocument))
            {
                return SpecializedTasks.CompletedTask;
            }

            // check if we are allowed to add it
            if (!workspace.CanApplyChange(ApplyChangesKind.AddAdditionalDocument))
            {
                return SpecializedTasks.CompletedTask;
            }

            foreach (var diagnostic in context.Diagnostics.Where(d => this.FixableDiagnosticIds.Contains(d.Id)))
            {
                context.RegisterCodeFix(CodeAction.Create(SettingsResources.SettingsFileCodeFix, token => GetTransformedSolutionAsync(context.Document, diagnostic, token), equivalenceKey: nameof(SettingsFileCodeFixProvider)), diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            // Added this to make it explicitly clear that this code fix does not support fix all actions.
            return null;
        }

        private static bool IsStyleCopSettingsDocument(TextDocument document)
        {
            return string.Equals(document.Name, StyleCopSettingsFileName, StringComparison.OrdinalIgnoreCase);
        }

        private static Task<Solution> GetTransformedSolutionAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var project = document.Project;
            var solution = project.Solution;

            var newDocumentId = DocumentId.CreateNewId(project.Id);

            var newSolution = solution.AddAdditionalDocument(newDocumentId, StyleCopSettingsFileName, DefaultSettingsFileContent);

            return Task.FromResult(newSolution);
        }
    }
}