namespace BlazingApple.Roslyn.Analyzers.Analyzers;

/// <summary><see cref="DiagnosticAnalyzer" /> that prevents using <c>async void</c></summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AsyncVoidAnalyzer : DiagnosticAnalyzer
{
	/// <summary>Id for this warning</summary>
	public const string DiagnosticId = "BA0002";

	private static readonly DiagnosticDescriptor _descriptor = new DiagnosticDescriptor(
		DiagnosticId,
		"Async method should not return void",
		"Async method '{0}' should not return void",
		"Syntax",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "Async method should not return void.");

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_descriptor);

	/// <inheritdoc />
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(CheckSyntaxMethod, SyntaxKind.MethodDeclaration);
	}

	private static void CheckSyntaxMethod(SyntaxNodeAnalysisContext context)
	{
		// Returns predefined type
		MethodDeclarationSyntax node = (MethodDeclarationSyntax)context.Node;
		if(node.ReturnType is not PredefinedTypeSyntax returnType)
			return;

		// Returns void
		if(returnType.Keyword.ToString() != "void")
			return;

		// Has async keyword
		if(!node.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)))
			return;

		_descriptor.Report(context, node.Identifier.GetLocation(), node.Identifier);
	}
}
