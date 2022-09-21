// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project. Project-level suppressions either have
// no target or are given a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// HACK (#692): Workaround known issue: https://github.com/dotnet/roslyn-analyzers/issues/5890#issuecomment-1046043775
[assembly: SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = ".net issue", Scope = "namespaceanddescendants", Target = "~N:BlazingApple.Analyzers.Analyzers")]
