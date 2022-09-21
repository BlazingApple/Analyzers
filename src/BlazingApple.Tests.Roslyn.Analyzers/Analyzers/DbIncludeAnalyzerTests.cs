using BlazingApple.Roslyn.Analyzers.Analyzers;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Verifier = BlazingApple.Tests.Roslyn.Shared.Analyzers.CSharpAnalyzerVerifier<BlazingApple.Roslyn.Analyzers.Analyzers.DbIncludeAnalyzer>;

namespace BlazingApple.Tests.Roslyn.Analyzers;

/// <summary>Tests for <see cref="DbIncludeAnalyzer" /></summary>
[TestClass]
public class DbIncludeAnalyzerTests
{
	[TestMethod]
	public async Task EntityNamespace_Include_Mapped_NoWarning()
	{
		string fullCode = GetCode("Mapped", null, null);
		await RunTest(fullCode);
	}

	[TestMethod]
	public async Task EntityNamespace_Include_Unmapped_HasWarning()
	{
		string fullCode = GetCode("Unmapped", null, DbIncludeAnalyzer.DiagnosticId);
		await RunTest(fullCode);
	}

	[TestMethod]
	public async Task EntityNamespace_ThenInclude_Mapped_NoWarning()
	{
		string fullCode = GetCode("Mapped", "Mapped", null);
		await RunTest(fullCode);
	}

	[TestMethod]
	public async Task EntityNamespace_ThenInclude_Unmapped_HasWarning()
	{
		string fullCode = GetCode("Mapped", "Unmapped", DbIncludeAnalyzer.DiagnosticId);
		await RunTest(fullCode);
	}

	private static string GetCode(string propName, string? thenIncludePropName, string? diagnosticId)
	{
		if(diagnosticId is not null)
		{
			if(thenIncludePropName is null)
				propName = $"{{|{diagnosticId}:{propName}|}}";
			else
				thenIncludePropName = $"{{|{diagnosticId}:{thenIncludePropName}|}}";
		}

		string thenIncludeOperation = thenIncludePropName == null ? "" : $".ThenInclude(d => d.{thenIncludePropName})";

		return $@"
	using System.Linq;
	using Microsoft.EntityFrameworkCore;

	#nullable enable

	class MyTest
	{{
		public static void DoTest()
		{{
			MyContext context = new();
			_ = context.MyDatas.Include(d => d.{propName}!){thenIncludeOperation}.Count();
		}}
	}}
	";
	}

	private static async Task RunTest(string code)
	{
		const string sharedCode = @"
	using Microsoft.EntityFrameworkCore;
	using System.ComponentModel.DataAnnotations.Schema;

	#nullable enable

	public class MyData
	{
		public MyData? Mapped { get; set; }

		[NotMapped]
		public MyData? Unmapped { get; set; }
	}

	public class MyContext : DbContext
	{
		public DbSet<MyData> MyDatas { get; set; } = null!;
	}
";

		Verifier.Test test = Verifier.CreateTest(code, null, typeof(DbContext));
		test.TestState.Sources.Add(sharedCode);
		await test.RunAsync();
	}
}
