using BlazingApple.Roslyn.Analyzers.Analyzers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Verifier = BlazingApple.Tests.Roslyn.Shared.Analyzers.CSharpAnalyzerVerifier<BlazingApple.Roslyn.Analyzers.Analyzers.AsyncVoidAnalyzer>;

namespace BlazingApple.Tests.Roslyn.Analyzers;

/// <summary>Tests for <see cref="AsyncVoidAnalyzer" /></summary>
[TestClass]
public class AsyncVoidAnalyzerTests
{
	[TestMethod]
	public async Task Verify_AsyncReturnsTask_NoWarning()
	{
		string code = GetCode(true, "Task", false);
		await Verifier.VerifyAnalyzerAsync(code);
	}

	[TestMethod]
	public async Task Verify_AsyncReturnsVoid_HasWarning()
	{
		string code = GetCode(true, "void", true);
		await Verifier.VerifyAnalyzerAsync(code);
	}

	[TestMethod]
	public async Task Verify_NotAsync_NoWarning()
	{
		string code = GetCode(false, "void", false);
		await Verifier.VerifyAnalyzerAsync(code);
	}

	private static string GetCode(bool isAsync, string returnType, bool hasWarning)
	{
		string taggedMethodName = "MyMethod";
		if(hasWarning)
			taggedMethodName = $"{{|{AsyncVoidAnalyzer.DiagnosticId}:{taggedMethodName}|}}";

		string asyncKeyword = isAsync ? "async" : "";

		return $@"
	using System.Threading.Tasks;
	#nullable enable
	class MyClass
	{{
		{asyncKeyword} {returnType} {taggedMethodName}() {{ }}
	}}
	";
	}
}
