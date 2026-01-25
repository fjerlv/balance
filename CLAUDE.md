# CLAUDE.md

This file provides guidance to Claude Code when working with this repository.

## Build and Test

After any code change, run:

```bash
dotnet build
dotnet test
```

## Version Management

When making a commit, increment `ModuleVersion` in `Balance.cs` (line 27):

```csharp
public override string ModuleVersion => "0.0.3";
```

Use semantic versioning (MAJOR.MINOR.PATCH). Increment the patch version for small changes.
