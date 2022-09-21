namespace BlazingApple.Roslyn.Analyzers;

/// <summary>Extensions for <see cref="ISymbol" /></summary>
internal static class SymbolExtensions
{
	private static readonly SymbolDisplayFormat _fullNameFormat = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

	/// <summary>Get the fully-qualified identifier name (including namespace)</summary>
	/// <param name="symbol"><see cref="ISymbol" /></param>
	/// <returns>Fully-qualified name</returns>
	public static string GetFullName(this ISymbol symbol)
		=> symbol.ToDisplayString(_fullNameFormat);

	/// <summary>Get the <see cref="Location" /> for a <see cref="ISymbol" /></summary>
	/// <param name="symbol"><see cref="ISymbol" /></param>
	/// <returns><see cref="Location" /> of <paramref name="symbol" /></returns>
	public static Location GetLocation(this ISymbol symbol)
		=> symbol.Locations[0];
}
