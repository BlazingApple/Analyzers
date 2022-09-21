namespace BlazingApple.Roslyn.Analyzers.Analyzers;

/// <summary><see cref="DiagnosticAnalyzer" /> that validates nullable reference type usage</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NullCheckAnalyzer : DiagnosticAnalyzer
{
	/// <summary>Id for this warning</summary>
	public const string DiagnosticId = "BA0001";

	private static readonly DiagnosticDescriptor _descriptor = new DiagnosticDescriptor(
		DiagnosticId,
		"Non-nullable value should not be treated as nullable",
		"Non-nullable value '{0}' should not be treated as nullable",
		"Syntax",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "Non-nullable value should not be treated as nullable.");

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_descriptor);

	/// <inheritdoc />
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterOperationAction(CheckOperationIsPattern, OperationKind.IsPattern);
		context.RegisterOperationAction(CheckOperationBinary, OperationKind.Binary);
		context.RegisterOperationAction(CheckOperationConditional, OperationKind.ConditionalAccess);
		context.RegisterSyntaxNodeAction(CheckSyntaxSuppressNullable, SyntaxKind.SuppressNullableWarningExpression);
	}

	private static void CheckOperationBinary(OperationAnalysisContext context)
	{
		IBinaryOperation operation = (IBinaryOperation)context.Operation;
		if(operation.OperatorKind is not (BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals))
			return;

		IOperation operand;
		if(IsNullConstant(operation.RightOperand.ConstantValue))
			operand = operation.LeftOperand;
		else if(IsNullConstant(operation.LeftOperand.ConstantValue))
			operand = operation.RightOperand;
		else
			return;

		if(operand.Type?.NullableAnnotation != NullableAnnotation.NotAnnotated)
			return;

		_descriptor.Report(context, operand.GetLocation(), operand);
	}

	private static void CheckOperationConditional(OperationAnalysisContext context)
	{
		IConditionalAccessOperation operation = (IConditionalAccessOperation)context.Operation;

		if(operation.Operation.Type?.NullableAnnotation != NullableAnnotation.NotAnnotated)
			return;

		_descriptor.Report(context, operation.Operation.GetLocation(), operation.Operation);
	}

	private static void CheckOperationIsPattern(OperationAnalysisContext context)
	{
		IIsPatternOperation operation = (IIsPatternOperation)context.Operation;
		IOperation leftValue = operation.Value;
		if(IsNullable(leftValue.SemanticModel!, leftValue.Syntax))
			return;

		IPatternOperation patternOperation = operation.Pattern;
		if(patternOperation.Kind == OperationKind.NegatedPattern)
			patternOperation = ((INegatedPatternOperation)patternOperation).Pattern;

		if(patternOperation.Kind != OperationKind.ConstantPattern)
			return;

		IConstantPatternOperation pattern = (IConstantPatternOperation)patternOperation;
		if(!IsNullConstant(pattern.Value.ConstantValue))
			return;

		_descriptor.Report(context, leftValue.GetLocation(), leftValue.Syntax);
	}

	private static void CheckSyntaxSuppressNullable(SyntaxNodeAnalysisContext context)
	{
		PostfixUnaryExpressionSyntax node = (PostfixUnaryExpressionSyntax)context.Node;
		ExpressionSyntax operand = node.Operand;
		if(IsNullable(context.SemanticModel, operand))
			return;

		_descriptor.Report(context, operand.GetLocation(), operand);
	}

	private static bool IsNullable(SemanticModel semanticModel, SyntaxNode syntax)
	{
		int position = syntax.GetLocation().SourceSpan.Start;
		TypeInfo type = semanticModel.GetSpeculativeTypeInfo(position, syntax, SpeculativeBindingOption.BindAsExpression);
		if(type.Type?.TypeKind == TypeKind.Dynamic)
			return true;
		else
			return type.Nullability.Annotation == NullableAnnotation.Annotated;
	}

	private static bool IsNullConstant(Optional<object?> value)
		=> value.HasValue && value.Value is null;
}
