using BlazingApple.Roslyn.Analyzers.Analyzers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Verifier = BlazingApple.Tests.Roslyn.Shared.Analyzers.CSharpAnalyzerVerifier<BlazingApple.Roslyn.Analyzers.Analyzers.NullCheckAnalyzer>;

namespace BlazingApple.Tests.Roslyn.Analyzers;

/// <summary>Tests for <see cref="NullCheckAnalyzer" /></summary>
[TestClass]
public class NullCheckAnalyzerTests
{
	[DataRow(" is null")]
	[DataRow(" is not null")]
	[DataRow(" == null")]
	[DataRow(" == null!")]
	[DataRow(" != null")]
	[DataRow("?.Trim() == \"test\"")]
	[DataRow("!.Trim() == \"test\"")]
	[DataTestMethod]
	public async Task Verify(string operation)
	{
		string code = GetCode(true, operation);
		await Verifier.VerifyAnalyzerAsync(code);

		code = GetCode(false, operation);
		await Verifier.VerifyAnalyzerAsync(code);
	}

	private static string GetCode(bool useAnnotation, string operation)
	{
		const string paramName = "value";
		string taggedParam = paramName;
		if(!useAnnotation)
			taggedParam = $"{{|{NullCheckAnalyzer.DiagnosticId}:{taggedParam}|}}";

		string paramType = "string";
		if(useAnnotation)
			paramType += "?";

		return $@"
	#nullable enable
	class MyClass
	{{
		int MyMethod({paramType} {paramName})
		{{
			if({taggedParam}{operation})
				return 0;
			else
				return 1;
		}}
	}}
	";
	}
}
