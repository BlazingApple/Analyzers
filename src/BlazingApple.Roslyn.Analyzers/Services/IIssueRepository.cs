namespace BlazingApple.Roslyn.Analyzers.Services;

/// <summary>Repository for storing code issues</summary>
internal interface IIssueRepository
{
	/// <summary>Create a new issue</summary>
	/// <param name="title">Short title</param>
	/// <param name="body">Descriptive issue body</param>
	void CreateIssue(string title, string body);
}
