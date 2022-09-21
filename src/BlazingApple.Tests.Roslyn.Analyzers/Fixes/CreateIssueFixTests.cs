using BlazingApple.Roslyn.Analyzers.Analyzers;
using BlazingApple.Roslyn.Analyzers.Fixes;
using BlazingApple.Roslyn.Analyzers.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Verifier = BlazingApple.Tests.Roslyn.Shared.Analyzers.CSharpCodeFixVerifier<BlazingApple.Roslyn.Analyzers.Analyzers.TodoCommentAnalyzer, BlazingApple.Roslyn.Analyzers.Fixes.CreateIssueFix>;

namespace BlazingApple.Tests.Roslyn.Shared.Analyzers.Fixes;

/// <summary>Tests for <see cref="CreateIssueFix" /></summary>
[TestClass]
public class CreateIssueFixTests
{
	[TestMethod]
	public async Task Verify_Plaintext_SingleLine_Hack_UpdatesComment()
	{
		string oldComment = $"// {Tag($"HACK: other stuff", TodoCommentAnalyzer.HackDiagnosticId)}";
		string newComment = $"// {Tag($"HACK (#XXX): other stuff", TodoCommentAnalyzer.HackDiagnosticId)}";
		await Verify(oldComment, newComment);
	}

	[TestMethod]
	public async Task Verify_Plaintext_SingleLine_Todo_RemovesComment()
	{
		string comment = $"// {Tag($"TODO: other stuff", TodoCommentAnalyzer.TodoDiagnosticId)}";
		await Verify(comment, "// ");
	}

	private static string GetCode(string commentCode)
	{
		return $@"
			class MyClass
			{{
				{commentCode}
				void MyMethod() {{ }}
			}}
		";
	}

	private static string Tag(string code, string diagnosticId)
		=> $"{{|{diagnosticId}:{code}|}}";

	private static async Task Verify(string badComment, string fixedComment)
	{
		string badCode = GetCode(badComment);
		string fixedCode = GetCode(fixedComment);

		FakeIssueRepository issueRepository = new();
		Verifier verifier = new()
		{
			CreateCodeFix = () => new CreateIssueFix(issueRepository),
		};

		await verifier.VerifyCodeFixAsync(badCode, fixedCode);

		// Multiple issues may be created since the verifier runs multiple tests
		Assert.IsTrue(issueRepository.Issues.Count <= 1);
	}

	private class FakeIssueRepository : IIssueRepository
	{
		public List<string> Issues { get; } = new();

		public void CreateIssue(string title, string body)
			=> Issues.Add(title);
	}
}
