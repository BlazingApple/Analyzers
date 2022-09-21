namespace BlazingApple.Roslyn.Analyzers;

/// <summary>Extensions for <see cref="IOperation" /></summary>
internal static class OperationExtensions
{
	/// <summary>Get the <see cref="Location" /></summary>
	/// <param name="source"><see cref="Location" /> source</param>
	/// <returns><see cref="Location" /> of <paramref name="source" /></returns>
	public static Location GetLocation(this IOperation source)
		=> source.Syntax.GetLocation();
}
