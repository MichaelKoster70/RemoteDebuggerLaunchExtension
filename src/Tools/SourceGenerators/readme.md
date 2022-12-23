# SourceGenerators
Implements a simple Roslyn [Source Generator](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) to generate version information used in the VS extension package registration.

## Rroject References
The extension project relies on Visual Studio provided assemblies which are available in public Azure DevOps Artifact feeds.
To prevent NuGet restore to fail with package version conflicts, the version of the project references must be kept in sync with the ones used in the extension.

The following references are affected:

| Nuget Package                   | Package Version | VS Version |
|:--------------------------------|:----------------|:-----------|
| Microsoft.CodeAnalysis.Analyzer | 3.3.3           | 17.2       |
| Microsoft.CodeAnalysis.CSharp   | 4.1.0-2.21558.8 | 17.2       |

## Compiler visible metadata
The generator relies on the MSBUID **RootNamespace** property beeing available in the generator. 
The consuming project must expose the property in a  **CompilerVisibleProperty** tag as followed:
```
<ItemGroup>
    <CompilerVisibleProperty Include="RootNamespace" />
</ItemGroup>
```

## Relevant Classes
The relevant classes in this project are listed below:
| Class                    | Implements |
|:-------------------------|:-----------|
| AssemblyVersionGenerator | The source generator producing the version information class |

## References
[Roslyn Source Generators Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md)
