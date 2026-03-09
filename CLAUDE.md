# CLAUDE.md

This file provides guidance to Claude Code when working with this repository.

## Build and Test

After any code change, run:

```bash
dotnet build
dotnet test
```

To run tests with detailed output:

```bash
dotnet test --logger "console;verbosity=detailed"
```

Tests are located in `Balance.Tests/` and use xUnit. New tests should be added for any new functionality.

## Version Management

When making a commit, increment `ModuleVersion` in `Balance.cs` (line 27):

```csharp
public override string ModuleVersion => "0.0.7";
```

Use semantic versioning (MAJOR.MINOR.PATCH). Increment the patch version for small changes.
