using System.Diagnostics;
using System.Web;

namespace BlazingApple.Roslyn.Analyzers.Services;

/// <summary><see cref="IIssueRepository" /> for GitHub</summary>
internal class GitHubIssueRepository : IIssueRepository
{
	/// <inheritdoc />
	public void CreateIssue(string title, string body)
	{
		// Can't use Flurl here since we can't reference external packages
		string titleEnc = HttpUtility.UrlEncode(title);
		string bodyEnc = HttpUtility.UrlEncode(body);
		string url = $"https://github.com/BlazingApple/Analyzers/issues/new?title={titleEnc}&body={bodyEnc}";

		ProcessStartInfo command = new(url)
		{
			UseShellExecute = true,
		};
		Process.Start(command);
	}
}
