# ScribanCSharpGenerator

A C# source generator that renders [Scriban](https://github.com/scriban/scriban)
scripts.

## Installation

Install the NuGet package `ScribanCSharpGenerator`:
```xml
<ItemGroup>
    <PackageReference Include="ScribanCSharpGenerator" Version="0.1.0" />
</ItemGroup>
```

Or clone repository, reference project `ScribanCSharpGenerator.csproj` and set properties `OtputItemType="Analyzer"` and `ReferenceOutputAssembly="false"`:

```xml
<ItemGroup>
    <ProjectReference
        Include="..\ScribanCSharpGenerator\ScribanCSharpGenerator.csproj"
        OutputItemType="Analyzer"
        ReferenceOutputAssembly="false"
    />
</ItemGroup>
```

# Code generation

To be rendered by the generator Scriban scripts need to have `.scriban` extension and be included
as `AdditionalFiles` in the project file `.csproj`:
```xml
<ItemGroup>
    <AdditionalFiles Include="ScribanTemplates\*" />
</ItemGroup>
```

Generator will render all of the project's additional files with the filename extension `.scriban`, except files with names that start with
the underscore `_`. Files with names that start with the underscore `_` will not be rendered,
but can be included in other scripts, [`include`](https://github.com/scriban/scriban/blob/master/doc/language.md#911-include-name-arg1argn)
statement is supported.

To save the generated files set properties `EmitCompilerGeneratedFiles` and `CompilerGeneratedFilesOutputPath`
in your project file `.csproj`. For example:
```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>
        $(BaseIntermediateOutputPath)\GeneratedFiles
    </CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

Refer to [Sample project](./Sample) for a complete example.