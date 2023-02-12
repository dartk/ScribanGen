# CSharp.SourceGen.Scriban

A C# source generator that renders [Scriban](https://github.com/scriban/scriban) templates.

## Installation

Install the NuGet package `Dartk.CSharp.SourceGen.Scriban` and set property `PrivateAssets="All"`:

```xml

<ItemGroup>
    <PackageReference Include="Dartk.CSharp.SourceGen.Scriban" Version="0.1.0-alpha1"
                      PrivateAssets="All" />
</ItemGroup>
```

## Source generation

Include scriban template files with `.scriban` extension to the project as `AdditionalFiles`.

> **Warning**: If a scriban template's name starts with an underscore `_`, then it will not be rendered but can be included in other templates using the [`include`](https://github.com/scriban/scriban/blob/master/doc/language.md#911-include-name-arg1argn) statement.

For example, to render all scriban templates in the `ScribanTemplates` folder add this to the project file:

```xml

<ItemGroup>
    <AdditionalFiles Include="ScribanTemplates\*" />
</ItemGroup>
```

## Saving generated files

To save the generated source files set properties `EmitCompilerGeneratedFiles` and `CompilerGeneratedFilesOutputPath`:

```xml

<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <!--Generated files will be saved to 'obj\GeneratedFiles' folder-->
    <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)\GeneratedFiles</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

## Limitations

Passing parameters to scriban templates is not supported.