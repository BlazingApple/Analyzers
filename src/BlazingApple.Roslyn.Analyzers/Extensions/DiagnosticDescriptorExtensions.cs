namespace BlazingApple.Roslyn.Analyzers;

/// <summary>Extensions for <see cref="DiagnosticDescriptor" /></summary>
internal static class DiagnosticDescriptorExtensions
{
	/// <summary>Report a diagnostic</summary>
	/// <param name="descriptor">Descriptor to report</param>
	/// <param name="context">Current analyzer context</param>
	/// <param name="location">Location to report</param>
	/// <param name="args">Additional arguments for the message in <paramref name="descriptor" /></param>
	public static void Report(this DiagnosticDescriptor descriptor, OperationAnalysisContext context, Location location, params object[] args)
	{
		Diagnostic diagnostic = Diagnostic.Create(descriptor, location, args);
		context.ReportDiagnostic(diagnostic);
	}

	/// <inheritdoc cref="Report(DiagnosticDescriptor, OperationAnalysisContext, Location, object[])" />
	public static void Report(this DiagnosticDescriptor descriptor, SyntaxNodeAnalysisContext context, Location location, params object[] args)
	{
		Diagnostic diagnostic = Diagnostic.Create(descriptor, location, args);
		context.ReportDiagnostic(diagnostic);
	}

	/// <inheritdoc cref="Report(DiagnosticDescriptor, OperationAnalysisContext, Location, object[])" />
	public static void Report(this DiagnosticDescriptor descriptor, SymbolAnalysisContext context, Location location, params object[] args)
	{
		Diagnostic diagnostic = Diagnostic.Create(descriptor, location, args);
		context.ReportDiagnostic(diagnostic);
	}

	/// <inheritdoc cref="Report(DiagnosticDescriptor, OperationAnalysisContext, Location, object[])" />
	public static void Report(this DiagnosticDescriptor descriptor, OperationBlockAnalysisContext context, Location location, params object[] args)
	{
		Diagnostic diagnostic = Diagnostic.Create(descriptor, location, args);
		context.ReportDiagnostic(diagnostic);
	}
}
