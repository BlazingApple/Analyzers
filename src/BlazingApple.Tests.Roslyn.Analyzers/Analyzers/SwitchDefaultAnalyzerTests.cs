using BlazingApple.Roslyn.Analyzers.Analyzers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Verifier = BlazingApple.Tests.Roslyn.Shared.Analyzers.CSharpAnalyzerVerifier<BlazingApple.Roslyn.Analyzers.Analyzers.SwitchDefaultAnalyzer>;

namespace BlazingApple.Tests.Roslyn.Analyzers;

/// <summary>Tests for <see cref="SwitchDefaultAnalyzer" /></summary>
[TestClass]
public class SwitchDefaultAnalyzerTests
{
	[DataRow(true, "0")]
	[DataRow(true, null)]
	[DataRow(false, "throw new System.Exception()")]
	[DataTestMethod]
	public async Task Verify_Expression(bool hasWarning, string? defaultStatement)
	{
		string code = GetExpressionCode(hasWarning, defaultStatement);
		await Verifier.VerifyAnalyzerAsync(code);
	}

	[DataRow(true, "return 0")]
	[DataRow(true, null)]
	[DataRow(false, "throw new System.Exception()")]
	[DataTestMethod]
	public async Task Verify_Statement(bool hasWarning, string? defaultStatement)
	{
		string code = GetStatementCode(hasWarning, defaultStatement);
		await Verifier.VerifyAnalyzerAsync(code);
	}

	private static string GetExpressionCode(bool hasWarning, string? defaultExpression)
	{
		string taggedSwitch = "switch";
		if(hasWarning)
			taggedSwitch = $"{{|{SwitchDefaultAnalyzer.DiagnosticId}:{taggedSwitch}|}}";

		string defaultCase = "";
		if(defaultExpression is not null)
			defaultCase = $"_ => {defaultExpression},";

		return $@"
	#nullable enable
	class MyClass
	{{
		int Test(int p)
		{{
			return p {taggedSwitch}
			{{
				0 => 0,
				{defaultCase}
			}};
		}}
	}}
	";
	}

	private static string GetStatementCode(bool hasWarning, string? defaultStatement)
	{
		string taggedSwitch = "switch";
		if(hasWarning)
			taggedSwitch = $"{{|{SwitchDefaultAnalyzer.DiagnosticId}:{taggedSwitch}|}}";

		string defaultCase = "";
		if(defaultStatement is not null)
			defaultCase = $"default: {defaultStatement};";

		return $@"
	#nullable enable
	class MyClass
	{{
		int Test(int p)
		{{
			{taggedSwitch}(p)
			{{
				case 0: return 0;
				{defaultCase}
			}}
			return 0;
		}}
	}}
	";
	}
}
