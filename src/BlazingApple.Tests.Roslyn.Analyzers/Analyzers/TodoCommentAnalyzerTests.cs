using BlazingApple.Roslyn.Analyzers.Analyzers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Verifier = BlazingApple.Tests.Roslyn.Shared.Analyzers.CSharpAnalyzerVerifier<BlazingApple.Roslyn.Analyzers.Analyzers.TodoCommentAnalyzer>;

namespace BlazingApple.Tests.Roslyn.Analyzers;

/// <summary>Tests for <see cref="TodoCommentAnalyzer" /></summary>
[TestClass]
public class TodoCommentAnalyzerTests
{
	public static IEnumerable<object?[]> TodoCommentHeaders
	{
		get
		{
			yield return new object?[] { "todo", TodoCommentAnalyzer.TodoDiagnosticId };
			yield return new object?[] { "TODO:", TodoCommentAnalyzer.TodoDiagnosticId };
			yield return new object?[] { "hack", TodoCommentAnalyzer.HackDiagnosticId };
			yield return new object?[] { "HACK:", TodoCommentAnalyzer.HackDiagnosticId };
		}
	}

	public static IEnumerable<object?[]> ValidComments
	{
		get
		{
			yield return new object?[] { "BLAH: hack todo" };
			yield return new object?[] { "HACK (#123): stuff" };
		}
	}

	[DynamicData(nameof(TodoCommentHeaders))]
	[DataTestMethod]
	public async Task Verify_Plaintext_MultiLine_MultipleTodos_HasWarnings(string header, string diagnosticId)
	{
		string comment = $@"/* {Tag($"{header} other stuff 1", diagnosticId)}
some text
			{Tag($"{header} other stuff 2", diagnosticId)}
more text
{Tag($"{header} other stuff 3 ", diagnosticId)}*/";
		await Verify(comment);
	}

	[DynamicData(nameof(ValidComments))]
	[DataTestMethod]
	public async Task Verify_Plaintext_MultiLine_Normal_NoWarning(string commentText)
	{
		string comment = $@"/*
			* {commentText}
			*/";
		await Verify(comment);
	}

	[DynamicData(nameof(TodoCommentHeaders))]
	[DataTestMethod]
	public async Task Verify_Plaintext_MultiLine_Todo_HasWarning(string header, string diagnosticId)
	{
		string comment = $@"/*
			* {Tag($"{header} other stuff", diagnosticId)}
			*/";
		await Verify(comment);
	}

	[DynamicData(nameof(ValidComments))]
	[DataTestMethod]
	public async Task Verify_Plaintext_SingleLine_Normal_NoWarning(string commentText)
	{
		string comment = $"// {commentText}";
		await Verify(comment);
	}

	[DynamicData(nameof(TodoCommentHeaders))]
	[DataTestMethod]
	public async Task Verify_Plaintext_SingleLine_Todo_HasWarning(string header, string diagnosticId)
	{
		string comment = $"//{Tag($"{header} other stuff", diagnosticId)}";
		await Verify(comment);
	}

	[DynamicData(nameof(TodoCommentHeaders))]
	[DataTestMethod]
	public async Task Verify_XmlDoc_MultiLine_MultipleTodos_HasWarnings(string header, string diagnosticId)
	{
		// Don't use multi-line string because VS tries to reformat it
		string comment = "/// <summary>\r\n	///		<para>"
						+ Tag($"{header} other stuff", diagnosticId)
						+ "</para>\r\n	/// <para>"
						+ Tag($"{header} other stuff", diagnosticId)
						+ "</para>\r\n	/// </summary>";
		await Verify(comment);
	}

	[DynamicData(nameof(TodoCommentHeaders))]
	[DataTestMethod]
	public async Task Verify_XmlDoc_MultiLine_NestedTodo_HasWarning(string header, string diagnosticId)
	{
		// Don't use multi-line string because VS tries to reformat it
		string comment = "/// <summary>\r\n	///		<para>"
						+ Tag($"{header} other stuff", diagnosticId)
						+ "</para>\r\n	/// </summary>";
		await Verify(comment);
	}

	[DynamicData(nameof(ValidComments))]
	[DataTestMethod]
	public async Task Verify_XmlDoc_MultiLine_Normal_NoWarning(string commentText)
	{
		// Don't use multi-line string because VS tries to reformat it
		string comment = "/// <summary>\r\n"
						+ $"	///		{commentText}\r\n"
						+ "	/// </summary>";
		await Verify(comment);
	}

	[DynamicData(nameof(TodoCommentHeaders))]
	[DataTestMethod]
	public async Task Verify_XmlDoc_MultiLine_Todo_HasWarning(string header, string diagnosticId)
	{
		// Don't use multi-line string because VS tries to reformat it
		string comment = "/// <summary>\r\n	///		"
						+ Tag($"{header} other stuff", diagnosticId)
						+ "\r\n	/// </summary>\r\n";
		await Verify(comment);
	}

	[DynamicData(nameof(TodoCommentHeaders))]
	[DataTestMethod]
	public async Task Verify_XmlDoc_SingleLine_NestedTodo_HasWarning(string header, string diagnosticId)
	{
		string comment = $"/// <summary><para>{Tag($"{header} other stuff", diagnosticId)}</para></summary>";
		await Verify(comment);
	}

	[DynamicData(nameof(ValidComments))]
	[DataTestMethod]
	public async Task Verify_XmlDoc_SingleLine_Normal_NoWarning(string commentText)
	{
		string comment = $"/// <summary>{commentText}</summary>";
		await Verify(comment);
	}

	[DynamicData(nameof(TodoCommentHeaders))]
	[DataTestMethod]
	public async Task Verify_XmlDoc_SingleLine_Todo_HasWarning(string header, string diagnosticId)
	{
		string comment = $"/// <summary>{Tag($"{header} other stuff", diagnosticId)}</summary>";
		await Verify(comment);
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

	private static async Task Verify(string commentCode)
	{
		string code = GetCode(commentCode);
		await Verifier.VerifyAnalyzerAsync(code);
	}
}
