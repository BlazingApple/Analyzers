using System.Data;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Text;

namespace BlazingApple.Roslyn.Analyzers.Analyzers;

/// <summary><see cref="DiagnosticAnalyzer" /> to find TODO/HACK comments</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TodoCommentAnalyzer : DiagnosticAnalyzer
{
	/// <summary>Id HACK comments</summary>
	public const string HackDiagnosticId = "BA0006";

	/// <summary>Id TODO comments</summary>
	public const string TodoDiagnosticId = "BA0005";

	private static readonly Regex _commentRegex = new(@"^\s*(\*\s*)?(?<header>TODO|HACK)\b(\s\(#(?<issue>\d+)\))?", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

	private static readonly DiagnosticDescriptor _hackDescriptor = new DiagnosticDescriptor(
		HackDiagnosticId,
		"Add issue number to HACK comment",
		"Add issue number to HACK comment",
		"Documentation",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "HACK comments should include an issue number.");

	private static readonly DiagnosticDescriptor _todoDescriptor = new DiagnosticDescriptor(
		TodoDiagnosticId,
		"Move TODO comment to issue",
		"Move TODO comment to issue",
		"Documentation",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "TODO comments should be moved to issues outside of the code unless the code is actively under development.");

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_todoDescriptor, _hackDescriptor);

	/// <inheritdoc />
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxTreeAction(CheckTree);
	}

	private static Diagnostic? CheckComment(TextLocation textLocation)
	{
		Match match = _commentRegex.Match(textLocation.Text);
		if(!match.Success)
			return null;

		Capture header = match.Groups["header"];
		string headerText = header.Value.ToUpper();

		DiagnosticDescriptor descriptor = headerText switch
		{
			"HACK" => _hackDescriptor,
			"TODO" => _todoDescriptor,
			_ => throw new DataException("Unexpected regex match"),
		};

		if(descriptor == _hackDescriptor && match.Groups["issue"].Success)
			return null;

		Location originalLocation = textLocation.GetLocation();
		TextSpan originalSpan = originalLocation.SourceSpan;
		TextSpan matchSpan = TextSpan.FromBounds(originalSpan.Start + header.Index, originalSpan.End);
		Location matchLocation = Location.Create(originalLocation.SourceTree!, matchSpan);

		return Diagnostic.Create(descriptor, matchLocation);
	}

	private static void CheckTree(SyntaxTreeAnalysisContext context)
	{
		CompilationUnitSyntax root = context.Tree.GetCompilationUnitRoot();

		foreach(SyntaxTrivia trivia in root.DescendantTrivia())
		{
			IEnumerable<TextLocation> textLocations = trivia.Kind() switch
			{
				SyntaxKind.SingleLineCommentTrivia => GetCommentSingleLine(trivia),
				SyntaxKind.MultiLineCommentTrivia => GetCommentMultiLine(trivia),
				SyntaxKind.SingleLineDocumentationCommentTrivia => GetCommentXml(trivia),
				SyntaxKind.MultiLineDocumentationCommentTrivia => GetCommentXml(trivia),
				_ => Array.Empty<TextLocation>(),
			};

			foreach(TextLocation textLocation in textLocations)
			{
				Diagnostic? warning = CheckComment(textLocation);
				if(warning is not null)
					context.ReportDiagnostic(warning);
			}
		}
	}

	private static IEnumerable<TextLocation> GetCommentMultiLine(SyntaxTrivia trivia)
	{
		TextSpan span = TextSpan.FromBounds(trivia.Span.Start + 2, trivia.Span.End - 2); // [2..^2]
		SourceText allText = trivia.SyntaxTree!.GetText();
		SourceText commentText = allText.GetSubText(span);

		foreach(TextLine line in commentText.Lines)
		{
			Location GetLocation() => Location.Create(trivia.SyntaxTree, new TextSpan(span.Start + line.Span.Start, line.Span.Length));
			yield return new TextLocation(line.ToString(), GetLocation);
		}
	}

	private static IEnumerable<TextLocation> GetCommentSingleLine(SyntaxTrivia trivia)
	{
		TextSpan span = TextSpan.FromBounds(trivia.Span.Start + 2, trivia.Span.End); // [2..]
		SourceText allText = trivia.SyntaxTree!.GetText();
		SourceText commentText = allText.GetSubText(span);

		Location GetLocation() => Location.Create(trivia.SyntaxTree!, span);
		yield return new TextLocation(commentText.ToString(), GetLocation);
	}

	private static IEnumerable<TextLocation> GetCommentXml(SyntaxTrivia trivia)
	{
		DocumentationCommentTriviaSyntax? syntax = (DocumentationCommentTriviaSyntax?)trivia.GetStructure();
		if(syntax is null)
			yield break;

		foreach(SyntaxToken token in syntax.DescendantTokens())
		{
			if(token.IsKind(SyntaxKind.XmlTextLiteralToken))
				yield return new TextLocation(token.ToString(), token.GetLocation);
		}
	}

	private class TextLocation
	{
		public Func<Location> GetLocation { get; }

		public string Text { get; }

		public TextLocation(string text, Func<Location> getLocation)
		{
			Text = text;
			GetLocation = getLocation;
		}
	}
}
