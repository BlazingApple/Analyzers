using BlazingApple.Roslyn.Analyzers.Analyzers;
using BlazingApple.Roslyn.Analyzers.Services;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace BlazingApple.Roslyn.Analyzers.Fixes;

/// <summary>Fix a warning by creating a new GitHub issue</summary>
[ExportCodeFixProvider(LanguageNames.CSharp)]
public class CreateIssueFix : CodeFixProvider
{
	private readonly IIssueRepository _issueRepository;

	/// <inheritdoc />
	public override sealed ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(TodoCommentAnalyzer.TodoDiagnosticId, TodoCommentAnalyzer.HackDiagnosticId);

	/// <summary>Default constructor</summary>
	public CreateIssueFix()
		: this(new GitHubIssueRepository()) { }

	/// <summary>DI constructor</summary>
	internal CreateIssueFix(IIssueRepository issueRepository)
		=> _issueRepository = issueRepository;

	/// <inheritdoc />
	public override sealed FixAllProvider GetFixAllProvider()
		=> WellKnownFixAllProviders.BatchFixer;

	/// <inheritdoc />
	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		Diagnostic diagnostic = context.Diagnostics.First();
		CreateIssueAction action = new(_issueRepository, context, diagnostic);
		context.RegisterCodeFix(action, diagnostic);
	}

	private class CreateIssueAction : CodeAction
	{
		private readonly Diagnostic _diagnostic;
		private readonly Document _document;
		private readonly IIssueRepository _issueRepository;
		private readonly Location _location;

		public override string? EquivalenceKey => nameof(Fixes.CreateIssueFix);

		public override string Title => "Create GitHub issue";

		public CreateIssueAction(IIssueRepository issueRepository, CodeFixContext context, Diagnostic diagnostic)
		{
			_issueRepository = issueRepository;
			_document = context.Document;
			_diagnostic = diagnostic;
			_location = diagnostic.Location;
		}

		protected override async Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(CancellationToken cancellationToken)
		{
			Document changedDocument = await UpdateDocument(false, cancellationToken);
			Solution changedSolution = changedDocument.Project.Solution;
			return new CodeActionOperation[] { new ApplyChangesOperation(changedSolution) };
		}

		protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
			=> await UpdateDocument(true, cancellationToken);

		private void CreateIssue(SourceText oldText)
		{
			FileLinePositionSpan lineSpan = _location.GetLineSpan();
			int lineNumber = lineSpan.StartLinePosition.Line + 1; // From 0-based to 1-based

			string fullText = oldText.GetSubText(_location.SourceSpan)
				.ToString()
				.TrimEnd(); // No need to trim start, always starts at TODO/HACK

			string todoText = fullText
				.Substring(4, fullText.Length - 4) // Skip TODO/HACK
				.TrimStart(':', '-', ',', ' ')
				.TrimStart();

			string fileName = Path.GetFileName(_document.Name);

			string title = $"{fileName} - {todoText}";
			string body = $@"{fullText}

{_document.Project.Name} - {fileName} - line {lineNumber}";

			_issueRepository.CreateIssue(title, body);
		}

		private async Task<Document> UpdateDocument(bool createIssue, CancellationToken cancellationToken)
		{
			SourceText oldText = await _document.GetTextAsync(cancellationToken);

			if(createIssue)
				CreateIssue(oldText);

			TextChange change = _diagnostic.Id switch
			{
				TodoCommentAnalyzer.TodoDiagnosticId => new(_location.SourceSpan, ""),
				TodoCommentAnalyzer.HackDiagnosticId => new(new TextSpan(_location.SourceSpan.Start + 4, 0), " (#XXX)"),
				_ => throw new NotSupportedException("Unexpected descriptor id"),
			};

			SourceText newText = oldText.WithChanges(change);
			return _document.WithText(newText);
		}
	}
}
