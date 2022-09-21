# :apple: BlazingApple.Roslyn.Analyzers
This repository supplies analyzers intended to speed development and prevent avoidable mistakes. We're all human, after all.


Here are some of the analyzers present. Fixes are supplied for most, if not all, of these:

| Analyzer              | Description                                                                                                       |
|-----------------------|-------------------------------------------------------------------------------------------------------------------|
| AbstractBaseAnalyzer  | Enforces that all classes that end with "Base" must be abstract.                                                  |
| AsyncVoidAnalyzer     | Enforces that all Tasks are awaited.                                                                              |
| NullCheckAnalyzer     | Ensures that non-nullable types are not checked for null (and visa-versa)                                         |
| SwitchDefaultAnalyzer | Ensures that all switch statements throw an exception in their default case.                                      |
| SyncQueryAnalyzer     | Ensures that all LINQ queries are executed asynchronously.                                                        |
| TestCategoryAnalyzer  | Ensures that all TestMethods have a TestCategory on them.                                                         |
| TodoCommentAnalyzer   | Ensures that a warning is present for all ToDo or HACK comments, and allows creating an issue in GitHub as a fix. |
