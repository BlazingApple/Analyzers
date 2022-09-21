using BlazingApple.Roslyn.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Verifier = BlazingApple.Tests.Roslyn.Shared.Analyzers.CSharpAnalyzerVerifier<BlazingApple.Roslyn.Analyzers.Analyzers.SyncQueryAnalyzer>;

namespace BlazingApple.Tests.Roslyn.Analyzers;

/// <summary>Tests for <see cref="SyncQueryAnalyzer" /></summary>
[TestClass]
public class SyncQueryAnalyzerTests
{
	[DataRow("IEnumerable")]
	[DataRow("IEnumerable<string>")]
	[DataTestMethod]
	public async Task Verify_AssignmentCast_Sync_HasWarning(string convertType)
	{
		string code = GetCode(true, $"{convertType} data2 = ", "data");
		await RunTest(code, SyncQueryAnalyzer.DescriptorCast, "IEnumerable");
	}

	[TestMethod]
	public async Task Verify_ForEach_Async_NoWarning()
	{
		string code = GetCode(false, "await foreach(var item in ", "data", ".AsAsyncEnumerable()) { }");
		await RunTest(code);
	}

	[TestMethod]
	public async Task Verify_ForEach_Sync_HasWarning()
	{
		string code = GetCode(true, "foreach(var item in ", "data", ") { }");
		await RunTest(code, SyncQueryAnalyzer.DescriptorProjection, "foreach");
	}

	[TestMethod]
	public async Task Verify_GetEnumerator_HasWarning()
	{
		string code = GetCode(true, null, "data", $".GetEnumerator();");
		await RunTest(code, SyncQueryAnalyzer.DescriptorProjection, "GetEnumerator");
	}

	[TestMethod]
	public async Task Verify_NestedQuery_NoWarning()
	{
		string code = GetCode(false, "IQueryable<string> query = ", "data", ".Where(d1 => data.Any(d2 => d1 == d2));");
		await RunTest(code);
	}

	[DataRow("ToList{0}")]
	[DataTestMethod]
	public async Task Verify_ProjectionCast_SyncWarning(string methodPattern)
	{
		// Sync: methods for IEnumerable
		string syncMethodName = string.Format(methodPattern, "");
		string code = GetCode(true, null, "data", $".{syncMethodName}();");
		await RunTest(code, SyncQueryAnalyzer.DescriptorCast, "IEnumerable");

		// Async: methods for IQueryable that return Task
		string asyncMethodName = string.Format(methodPattern, "Async");
		code = GetCode(false, null, "data", $".{asyncMethodName}();");
		await RunTest(code);
	}

	[DataRow("Single", "")]
	[DataRow("First", "")]
	[DataRow("SingleOrDefault", "")]
	[DataRow("FirstOrDefault", "")]
	[DataRow("Count", "")]
	[DataRow("Any", "")]
	[DataRow("All", "x => true")]
	[DataTestMethod]
	public async Task Verify_ProjectionReturn_SyncWarning(string methodName, string args)
	{
		// Sync: methods for IQueryable that return sync results (not one of the allowed types)
		string code = GetCode(true, null, $"data.{methodName}({args})");
		await RunTest(code, SyncQueryAnalyzer.DescriptorProjection, methodName);

		// Async: methods for IQueryable that return Task
		code = GetCode(false, null, $"data.{methodName}Async({args})");
		await RunTest(code);
	}

	[DataRow("data.Where(x => true)")]
	[DataRow("data.OrderBy(x => x)")]
	[DataRow("data.AsAsyncEnumerable()")]
	[DataTestMethod]
	public async Task Verify_Query_NoWarning(string testCode)
	{
		// Methods for IQueryable that return IQueryable or IAsyncEnumerable
		string code = GetCode(false, null, testCode);
		await RunTest(code);
	}

	private static string GetCode(bool hasWarning, string? codePrefix, string testCode, string codeSuffix = ";")
	{
		string taggedCode = testCode;
		if(hasWarning)
			taggedCode = $"{{|#0:{taggedCode}|}}";

		return $@"
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
#nullable enable
class MyClass
{{
	async Task MyMethod()
	{{
		IQueryable<string> data = new MyData();
		{codePrefix}{taggedCode}{codeSuffix}
	}}
}}

class MyData : DbSet<string>
{{
	public override IEntityType EntityType {{ get; }} = null!;
}}
	";
	}

	private static async Task RunTest(string code)
		=> await Verifier.VerifyAnalyzerAsync(code, typeof(DbContext));

	private static async Task RunTest(string code, DiagnosticDescriptor descriptor, string message)
	{
		DiagnosticResult expected = Verifier.Diagnostic(descriptor)
			.WithLocation(0) // Referenced in code markup
			.WithArguments(message);

		await Verifier.VerifyAnalyzerAsync(code, expected, typeof(DbContext));
	}
}
