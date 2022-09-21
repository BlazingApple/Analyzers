namespace BlazingApple.Roslyn.Analyzers.Analyzers;

/// <summary><see cref="DiagnosticAnalyzer" /> that checks that database <c>Include</c> calls</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DbIncludeAnalyzer : DiagnosticAnalyzer
{
	/// <summary>Id for an <c>Include</c> on an invalid property</summary>
	public const string DiagnosticId = "BA0012";

	private const string _includeMethodDefinition = "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include<TEntity, TProperty>(System.Linq.IQueryable<TEntity>, System.Linq.Expressions.Expression<System.Func<TEntity, TProperty>>)";
	private const string _notMappedAttributeName = "System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute";
	private const string _thenIncludeMethodDefinition = "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ThenInclude<TEntity, TPreviousProperty, TProperty>(Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<TEntity, TPreviousProperty>, System.Linq.Expressions.Expression<System.Func<TPreviousProperty, TProperty>>)";

	private static readonly DiagnosticDescriptor _descriptor = new DiagnosticDescriptor(
		DiagnosticId,
		"Property cannot be included",
		"{0} cannot be included",
		"Design",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "This property is not part of the database and needs to be retrieved/computed separately by the caller.");

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_descriptor);

	/// <inheritdoc />
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterOperationAction(CheckInvocation, OperationKind.Invocation);
	}

	private static void CheckInvocation(OperationAnalysisContext context)
	{
		// Method call
		IInvocationOperation operation = (IInvocationOperation)context.Operation;
		string methodDefinition = operation.TargetMethod.OriginalDefinition.ToString();

		// Include or ThenInclude call
		if(methodDefinition is not (_includeMethodDefinition or _thenIncludeMethodDefinition))
			return;

		// For a property reference
		IArgumentOperation expressionArg = operation.Arguments[1];
		IPropertyReferenceOperation propRef = expressionArg
			.Descendants()
			.OfType<IPropertyReferenceOperation>()
			.SingleOrDefault();

		if(propRef is null)
			return;

		// Marked as [NotMapped]
		ImmutableArray<AttributeData> attributes = propRef.Property.GetAttributes();
		if(!attributes.Any(a => a.AttributeClass.HasType(_notMappedAttributeName)))
			return;

		MemberAccessExpressionSyntax memberSyntax = (MemberAccessExpressionSyntax)propRef.Syntax;
		SimpleNameSyntax propNameSyntax = memberSyntax.Name;
		_descriptor.Report(context, propNameSyntax.GetLocation(), propNameSyntax.ToString());
	}
}
