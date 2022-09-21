namespace BlazingApple.Roslyn.Analyzers.Analyzers;

/// <summary><see cref="DiagnosticAnalyzer" /> that makes sure every <c>switch</c> has a default exception case</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SwitchDefaultAnalyzer : DiagnosticAnalyzer
{
	/// <summary>Id for this warning</summary>
	public const string DiagnosticId = "BA0003";

	private static readonly DiagnosticDescriptor _descriptor = new DiagnosticDescriptor(
		DiagnosticId,
		"Default case for switch should throw an exception",
		"Default case for switch should throw an exception",
		"Syntax",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "Default case for switch should throw an exception.");

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_descriptor);

	/// <inheritdoc />
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(CheckSyntaxSwitchStatement, SyntaxKind.SwitchStatement);
		context.RegisterSyntaxNodeAction(CheckSyntaxSwitchExpression, SyntaxKind.SwitchExpression);
	}

	private static void CheckSyntaxSwitchExpression(SyntaxNodeAnalysisContext context)
	{
		SwitchExpressionSyntax node = (SwitchExpressionSyntax)context.Node;
		SwitchExpressionArmSyntax? defaultArm = node.Arms.SingleOrDefault(a => a.Pattern.IsKind(SyntaxKind.DiscardPattern));

		// Default arm isn't a throw expression
		if(defaultArm?.Expression.IsKind(SyntaxKind.ThrowExpression) ?? false)
			return;

		_descriptor.Report(context, node.SwitchKeyword.GetLocation());
	}

	private static void CheckSyntaxSwitchStatement(SyntaxNodeAnalysisContext context)
	{
		SwitchStatementSyntax node = (SwitchStatementSyntax)context.Node;
		SwitchSectionSyntax? defaultCase = node.Sections.SingleOrDefault(s => s.Labels.Any(l => l.IsKind(SyntaxKind.DefaultSwitchLabel)));

		// Default case doesn't contain a throw expression
		if(defaultCase is not null && defaultCase.Statements.Count == 1)
		{
			StatementSyntax statement = defaultCase.Statements[0];
			if(statement.IsKind(SyntaxKind.ThrowStatement))
				return;
		}

		_descriptor.Report(context, node.SwitchKeyword.GetLocation());
	}
}
