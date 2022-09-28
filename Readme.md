# ScribanCSharpGenerator

A C# source generator that renders [Scriban](https://github.com/scriban/scriban) scripts.


## Installation

Install the NuGet package `ScribanCSharpGenerator`:

```xml
<ItemGroup>
    <PackageReference Include="ScribanCSharpGenerator" Version="0.1.0"/>
</ItemGroup>
```

Or reference project `ScribanCSharpGenerator.csproj` and set properties `OtputItemType="Analyzer"`
and `ReferenceOutputAssembly="false"`:

```xml
<ItemGroup>
    <ProjectReference
            Include="..\ScribanCSharpGenerator\ScribanCSharpGenerator.csproj"
            OutputItemType="Analyzer"
            ReferenceOutputAssembly="false"
    />
</ItemGroup>
```

## Code generation

Scriban files need to have `.scriban` extension and be included as `AdditionalFiles`
in the project to be processed by the generator. For example:

```xml
<ItemGroup>
    <AdditionalFiles Include="ScribanTemplates\*"/>
</ItemGroup>
```

If Scriban file name starts with underscore then it will not be rendered, but can be
included in other scripts using
[`include`](https://github.com/scriban/scriban/blob/master/doc/language.md#911-include-name-arg1argn)
statement.

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


## Limitations

Passing parameters to Scriban scripts is not supported.