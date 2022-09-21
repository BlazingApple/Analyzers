namespace BlazingApple.Roslyn.Analyzers.Analyzers;

/// <summary><see cref="DiagnosticAnalyzer" /> that prevents using synchronous queries</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SyncQueryAnalyzer : DiagnosticAnalyzer
{
	/// <summary>Id for this warning</summary>
	public const string DiagnosticId = "BA0004";

	/// <summary>Diagnostic added when casting a query to a synchronous enumeration type</summary>
	public static readonly DiagnosticDescriptor DescriptorCast = new DiagnosticDescriptor(
		DiagnosticId,
		"Queries should be asynchronous",
		"Query should not be cast to '{0}', use an async projection instead",
		"Performance",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "Queries should be asynchronous.");

	/// <summary>Diagnostic added when using a non-async projection on a query</summary>
	public static readonly DiagnosticDescriptor DescriptorProjection = new(
		DiagnosticId,
		"Queries should be asynchronous",
		"Query projection '{0}' should be asynchronous",
		"Performance",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "Queries should be asynchronous.");

	private const string _methodNameGetEnumerator = "GetEnumerator";
	private const string _typeNameExpression = "System.Linq.Expressions.Expression";
	private const string _typeNameIAsyncEnumerableGeneric = "System.Collections.Generic.IAsyncEnumerable";
	private const string _typeNameIEnumerable = "System.Collections.IEnumerable";
	private const string _typeNameIEnumerableGeneric = "System.Collections.Generic.IEnumerable";
	private const string _typeNameIQueryable = "System.Linq.IQueryable";
	private const string _typeNameTask = "System.Threading.Tasks.Task";

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DescriptorProjection, DescriptorCast);

	/// <inheritdoc />
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterOperationAction(CheckInvocation, OperationKind.Invocation);
		context.RegisterOperationAction(CheckOperationConversion, OperationKind.Conversion);
		context.RegisterOperationAction(CheckOperationArgument, OperationKind.Argument);
		context.RegisterOperationAction(CheckOperationLoop, OperationKind.Loop);
	}

	private static void CheckInvocation(OperationAnalysisContext context)
	{
		// Method is GetEnumerator
		IInvocationOperation operation = (IInvocationOperation)context.Operation;
		if(operation.Instance is null || operation.TargetMethod.OriginalDefinition.Name != _methodNameGetEnumerator)
			return;

		ITypeSymbol? type = operation.Instance.Type;
		if(type is null)
			return;

		// On IQueryable/IAsyncEnumerable
		if(!type.HasInterface(_typeNameIQueryable) && !type.HasInterface(_typeNameIAsyncEnumerableGeneric))
			return;

		DescriptorProjection.Report(context, operation.Instance.GetLocation(), operation.TargetMethod.Name);
	}

	private static void CheckOperationArgument(OperationAnalysisContext context)
	{
		IArgumentOperation operation = (IArgumentOperation)context.Operation;
		if(operation.Parent is null || operation.Parameter is null)
			return;

		// Method argument is IQueryable
		if(!operation.Parameter.Type.HasInterface(_typeNameIQueryable))
			return;

		// Method doesn't return Task or IQueryable
		IInvocationOperation invocation = (IInvocationOperation)operation.Parent;
		if(invocation.Type.HasType(_typeNameTask) || invocation.Type.HasInterface(_typeNameIQueryable) || invocation.Type.HasInterface(_typeNameIAsyncEnumerableGeneric))
			return;

		// Isn't part of a nested LINQ expression (i.e. nested query)
		if(IsNestedExpression(invocation))
			return;

		DescriptorProjection.Report(context, invocation.GetLocation(), invocation.TargetMethod.Name);
	}

	private static void CheckOperationConversion(OperationAnalysisContext context)
	{
		// Cast to IEnumerable
		IConversionOperation operation = (IConversionOperation)context.Operation;
		if(operation.Type?.GetFullName() is not (_typeNameIEnumerable or _typeNameIEnumerableGeneric))
			return;

		// From IQueryable
		if(!operation.Operand.Type.HasInterface(_typeNameIQueryable))
			return;

		DescriptorCast.Report(context, operation.GetLocation(), operation.Type.Name);
	}

	private static void CheckOperationLoop(OperationAnalysisContext context)
	{
		// Foreach loop
		ILoopOperation operation = (ILoopOperation)context.Operation;
		if(operation.LoopKind != LoopKind.ForEach)
			return;

		// Not async
		IForEachLoopOperation forEach = (IForEachLoopOperation)context.Operation;
		if(forEach.IsAsynchronous)
			return;

		// On an IQueryable
		if(!forEach.Collection.Type.HasInterface(_typeNameIQueryable))
			return;

		ForEachStatementSyntax syntax = (ForEachStatementSyntax)forEach.Syntax;
		DescriptorProjection.Report(context, forEach.Collection.GetLocation(), syntax.ForEachKeyword);
	}

	private static bool IsNestedExpression(IOperation? operation)
	{
		if(operation is null)
			return false;

		if(operation is IConversionOperation conversion && conversion.Type.HasType(_typeNameExpression))
			return true;

		return IsNestedExpression(operation.Parent);
	}
}
