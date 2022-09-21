namespace BlazingApple.Roslyn.Analyzers.Analyzers;

/// <summary><see cref="DiagnosticAnalyzer" /> that verifies category attributes on unit tests</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestCategoryAnalyzer : DiagnosticAnalyzer
{
	/// <summary>Id for a test that doesn't have a category</summary>
	public const string MissingDiagnosticId = "BA0010";

	/// <summary>Id for a test with multiple category attributes</summary>
	public const string MultipleDiagnosticId = "BA0011";

	private const string _typeNameCategoryBase = "Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryBaseAttribute";

	private const string _typeNameTestMethod = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";

	/// <summary>Diagnostic added when</summary>
	private static readonly DiagnosticDescriptor _missingDescriptor = new DiagnosticDescriptor(
		MissingDiagnosticId,
		"Unit tests should be have a category attribute",
		"Test method '{0}' does not have a category attribute",
		"Design",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "Unit tests should be have exactly one category attribute.");

	/// <summary>Diagnostic added when</summary>
	private static readonly DiagnosticDescriptor _multipleDescriptor = new DiagnosticDescriptor(
		MultipleDiagnosticId,
		"Unit tests should only have one category attribute",
		"Test method '{0}' has multiple category attributes",
		"Design",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "Unit tests should be have exactly one category attribute.");

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_missingDescriptor, _multipleDescriptor);

	/// <inheritdoc />
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterOperationBlockAction(CheckOperationBlock);
	}

	private static void CheckOperationBlock(OperationBlockAnalysisContext context)
	{
		if(context.OwningSymbol.Kind != SymbolKind.Method)
			return;

		ISymbol method = context.OwningSymbol;
		ImmutableArray<AttributeData> attributes = method.GetAttributes();

		// Is a TestMethod
		if(CountAttribute(attributes, _typeNameTestMethod) == 0)
			return;

		// Without exactly one category
		int count = CountAttribute(attributes, _typeNameCategoryBase);
		DiagnosticDescriptor? descriptor = count switch
		{
			< 1 => _missingDescriptor,
			> 1 => _multipleDescriptor,
			_ => null,
		};

		descriptor?.Report(context, method.GetLocation(), method.Name);
	}

	private static int CountAttribute(ImmutableArray<AttributeData> attributes, string name)
		=> attributes.Count(a => a.AttributeClass.HasType(name));
}
