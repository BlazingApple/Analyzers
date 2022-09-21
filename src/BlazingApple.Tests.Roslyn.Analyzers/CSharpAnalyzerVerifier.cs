﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace BlazingApple.Tests.Roslyn.Shared.Analyzers;

/// <summary>Runs a C# code analyzer</summary>
/// <remarks>This file was condensed from classes generated by MS</remarks>
/// <typeparam name="TAnalyzer">Analyzer to test</typeparam>
internal static class CSharpAnalyzerVerifier<TAnalyzer>
	where TAnalyzer : DiagnosticAnalyzer, new()
{
	/// <summary>
	///     By default, the compiler reports diagnostics for nullable reference types at <see cref="DiagnosticSeverity.Warning" />, and the analyzer
	///     test framework defaults to only validating diagnostics at <see cref="DiagnosticSeverity.Error" />. This map contains all compiler
	///     diagnostic IDs related to nullability mapped to <see cref="ReportDiagnostic.Error" />, which is then used to enable all of these warnings
	///     for default validation during analyzer and code fix tests.
	/// </summary>
	private static ImmutableDictionary<string, ReportDiagnostic> NullableWarnings { get; } = GetNullableWarningsFromCompiler();

	public static CSharpAnalyzerVerifier<TAnalyzer>.Test CreateTest(string source, DiagnosticResult? expected = null, params Type[] referencedAssemblyTypes)
	{
		Test test = new();
		test.TestState.Sources.Add(source);

		foreach(Type referencedAssemblyType in referencedAssemblyTypes)
			test.TestState.AdditionalReferences.Add(referencedAssemblyType.Assembly);

		if(expected.HasValue)
			test.ExpectedDiagnostics.Add(expected.Value);

		return test;
	}

	/// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic()" />
	public static DiagnosticResult Diagnostic()
		=> CSharpAnalyzerVerifier<TAnalyzer, MSTestVerifier>.Diagnostic();

	/// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(string)" />
	public static DiagnosticResult Diagnostic(string diagnosticId)
		=> CSharpAnalyzerVerifier<TAnalyzer, MSTestVerifier>.Diagnostic(diagnosticId);

	/// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)" />
	public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
		=> CSharpAnalyzerVerifier<TAnalyzer, MSTestVerifier>.Diagnostic(descriptor);

	/// <summary>Set default values for all tests</summary>
	/// <param name="test">Test to update</param>
	public static void SetTestDefaults(AnalyzerTest<MSTestVerifier> test)
	{
		test.SolutionTransforms.Add((solution, projectId) =>
		{
			CompilationOptions? compilationOptions = solution.GetProject(projectId)?.CompilationOptions;
			if(compilationOptions is null)
				throw new InvalidOperationException("Missing compilation options");

			ImmutableDictionary<string, ReportDiagnostic> items = compilationOptions.SpecificDiagnosticOptions.SetItems(NullableWarnings);
			compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(items);
			solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

			return solution;
		});

		test.ReferenceAssemblies = ReferenceAssemblies.Net.Net60;
	}

	public static async Task VerifyAnalyzerAsync(string source, params Type[] referencedAssemblyTypes)
		=> await VerifyAnalyzerAsync(source, null, referencedAssemblyTypes);

	public static async Task VerifyAnalyzerAsync(string source, DiagnosticResult? expected, params Type[] referencedAssemblyTypes)
	{
		CSharpAnalyzerVerifier<TAnalyzer>.Test test = CreateTest(source, expected, referencedAssemblyTypes);
		await test.RunAsync();
	}

	private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
	{
		string[] args = { "/warnaserror:nullable" };
		CSharpCommandLineArguments commandLineArguments = CSharpCommandLineParser.Default.Parse(args, Environment.CurrentDirectory, Environment.CurrentDirectory);
		ImmutableDictionary<string, ReportDiagnostic> nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

		// Workaround for https://github.com/dotnet/roslyn/issues/41610
		nullableWarnings = nullableWarnings
			.SetItem("CS8632", ReportDiagnostic.Error)
			.SetItem("CS8669", ReportDiagnostic.Error);

		return nullableWarnings;
	}

	public class Test : CSharpAnalyzerTest<TAnalyzer, MSTestVerifier>
	{
		public Test()
			=> SetTestDefaults(this);
	}
}
