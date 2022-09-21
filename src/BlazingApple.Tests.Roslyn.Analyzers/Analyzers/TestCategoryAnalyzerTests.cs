using BlazingApple.Roslyn.Analyzers.Analyzers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Verifier = BlazingApple.Tests.Roslyn.Shared.Analyzers.CSharpAnalyzerVerifier<BlazingApple.Roslyn.Analyzers.Analyzers.TestCategoryAnalyzer>;

namespace BlazingApple.Tests.Roslyn.Analyzers;

/// <summary>Tests for <see cref="TestCategoryAnalyzer" /></summary>
[TestClass]
public class TestCategoryAnalyzerTests
{
	[TestMethod]
	public async Task Verify_MultipleCategories_MultipleWarning()
	{
		string fullCode = GetCode("Cat1, Cat2", TestCategoryAnalyzer.MultipleDiagnosticId);
		await RunTest(fullCode);
	}

	[TestMethod]
	public async Task Verify_NonTest_NoWarning()
	{
		string fullCode = GetCode("Other", null, false);
		await RunTest(fullCode);
	}

	[TestMethod]
	public async Task Verify_OneCategory_NoWarning()
	{
		string fullCode = GetCode("Cat1", null);
		await RunTest(fullCode);
	}

	[TestMethod]
	public async Task Verify_Uncategorized_MissingWarning()
	{
		string fullCode = GetCode("Other", TestCategoryAnalyzer.MissingDiagnosticId);
		await RunTest(fullCode);
	}

	private static string GetCode(string attributes, string? diagnosticId, bool isTest = true)
	{
		string methodName = "MyTest";
		if(diagnosticId is not null)
			methodName = $"{{|{diagnosticId}:{methodName}|}}";

		string testMethodAttrib = isTest ? "[TestMethod]" : "";

		return $@"
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	#nullable enable

	[TestClass]
	public class MyClass
	{{
		{testMethodAttrib}
		[{attributes}]
		public void {methodName}() {{ }}
	}}
	";
	}

	private static async Task RunTest(string code)
	{
		const string sharedCode = @"
	using System;
	using System.Collections.Generic;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	#nullable enable

	public abstract class TestCategoryBase : TestCategoryBaseAttribute
	{
		public override IList<string> TestCategories { get; } = new List<string>() { ""Unit Test"" };
	}

	public class Cat1Attribute : TestCategoryBase { }

	public class Cat2Attribute : TestCategoryBase { }

	public class OtherAttribute : Attribute { }
"
		;

		Verifier.Test test = Verifier.CreateTest(code, null, typeof(TestMethodAttribute));
		test.TestState.Sources.Add(sharedCode);
		await test.RunAsync();
	}
}
