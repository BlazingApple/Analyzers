using BlazingApple.Roslyn.Analyzers.Analyzers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Verifier = BlazingApple.Tests.Roslyn.Shared.Analyzers.CSharpAnalyzerVerifier<BlazingApple.Roslyn.Analyzers.Analyzers.AbstractBaseAnalyzer>;

namespace BlazingApple.Tests.Roslyn.Analyzers;

/// <summary>Tests for <see cref="AbstractBaseAnalyzer" /></summary>
[TestClass]
public class AbstractBaseAnalyzerTests
{
	[TestMethod]
	public async Task Verify_AbstractBase_NoWarning()
	{
		string code = GetCode(true, "MyBase", false, null);
		await Verifier.VerifyAnalyzerAsync(code);
	}

	[TestMethod]
	public async Task Verify_AbstractNonbase_NoWarning()
	{
		string code = GetCode(true, "MyStuff", false, null);
		await Verifier.VerifyAnalyzerAsync(code);
	}

	[DataRow(true)]
	[DataRow(false)]
	[DataTestMethod]
	public async Task Verify_ConcreteBase_HasWarning(bool isGeneric)
	{
		string code = GetCode(false, "MyBase", isGeneric, AbstractBaseAnalyzer.DiagnosticId);
		await Verifier.VerifyAnalyzerAsync(code);
	}

	[TestMethod]
	public async Task Verify_ConcreteNonbase_NoWarning()
	{
		string code = GetCode(false, "MyStuff", false, null);
		await Verifier.VerifyAnalyzerAsync(code);
	}

	private static string GetCode(bool isAbstract, string className, bool isGeneric, string? expectedDiagnosticId)
	{
		if(expectedDiagnosticId is not null)
			className = $"{{|{expectedDiagnosticId}:{className}|}}";

		string abstractKeyword = isAbstract ? "abstract " : "";
		string genericParams = isGeneric ? "<T>" : "";

		return $@"{abstractKeyword}class {className}{genericParams} {{ }}";
	}
}
