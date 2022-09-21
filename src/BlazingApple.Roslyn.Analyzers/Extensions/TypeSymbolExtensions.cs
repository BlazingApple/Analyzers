namespace BlazingApple.Roslyn.Analyzers;

/// <summary>Extensions for <see cref="ITypeSymbol" /></summary>
internal static class TypeSymbolExtensions
{
	/// <summary>Check if the given type implements or is the given interface</summary>
	/// <param name="type"><see cref="ITypeSymbol" /> to check</param>
	/// <param name="name">Fully qualified interface name</param>
	/// <returns>
	///     Whether <paramref name="type" /> implements (or is) <paramref name="name" />, or <c>false</c> when <paramref name="type" /> is <c>null</c>
	/// </returns>
	public static bool HasInterface(this ITypeSymbol? type, string name)
	{
		if(type is null)
			return false;
		else if(type.GetFullName() == name)
			return true;
		else
			return type.AllInterfaces.Any(i => i.GetFullName() == name);
	}

	/// <summary>Check if the given type inherits from or is the given base class</summary>
	/// <param name="type"><see cref="ITypeSymbol" /> to check</param>
	/// <param name="name">Fully qualified class name</param>
	/// <returns>
	///     Whether <paramref name="type" /> inherits from (or is) <paramref name="name" />, or <c>false</c> when <paramref name="type" /> is <c>null</c>
	/// </returns>
	public static bool HasType(this ITypeSymbol? type, string name)
	{
		if(type is null)
			return false;
		else if(type.GetFullName() == name)
			return true;
		else
			return HasType(type.BaseType, name);
	}
}
