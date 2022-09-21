namespace BlazingApple.Roslyn.Analyzers.Analyzers;

/// <summary><see cref="DiagnosticAnalyzer" /> that checks that base classes are abstract</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AbstractBaseAnalyzer : DiagnosticAnalyzer
{
	/// <summary>Id for a non-abstract base class</summary>
	public const string DiagnosticId = "BA0013";

	private static readonly DiagnosticDescriptor _descriptor = new DiagnosticDescriptor(
		DiagnosticId,
		"Base class should be abstract",
		"{0} should be abstract",
		"Design",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "Either make the class abstract, or change the class name to not say 'Base'.");

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_descriptor);

	/// <inheritdoc />
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSymbolAction(CheckNamedType, SymbolKind.NamedType);
	}

	private static void CheckNamedType(SymbolAnalysisContext context)
	{
		// Class declaration
		INamedTypeSymbol namedType = (INamedTypeSymbol)context.Symbol;
		if(namedType.TypeKind != TypeKind.Class)
			return;

		// Named Base
		if(!namedType.Name.EndsWith("Base"))
			return;

		// Not abstract
		if(namedType.IsAbstract)
			return;

		_descriptor.Report(context, namedType.GetLocation(), namedType.Name);
	}
}
