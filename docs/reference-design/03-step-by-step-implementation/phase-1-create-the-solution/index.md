# Create the solution

Create the Bolero solution and project structure.

## Goal

```text
a solution that builds
Bolero app (client + server)
Community.Shared shared contract project
a test project
```

## Steps

1. Scaffold the official Bolero template (`dotnet new bolero-app`).
2. Split shared contracts into `Community.Shared`.
3. Add a test project.
4. Verify with `dotnet build` and `dotnet test`.

## Note

The template output is the baseline; later phases reshape the structure per
this design without rewriting the framework machinery.