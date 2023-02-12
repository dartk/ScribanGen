# CSharp.SourceGen.Scriban

A C# source generator that renders [Scriban](https://github.com/scriban/scriban) templates.


## Installation

Install the NuGet package `Dartk.CSharp.SourceGen.Scriban`:

* Using dotnet tool:

    ```
    dotnet add package Dartk.CSharp.SourceGen.Scriban
    ```
  
* Editing .csproj project file:
    ```xml
    <ItemGroup>
        <PackageReference Include="Dartk.CSharp.SourceGen.Scriban" Version="0.1.0-alpha1"
                          PrivateAssets="All" />
    </ItemGroup>
        ```

## Source generation

Include scriban template files with `.scriban` extension to the project as `AdditionalFiles`.

> **Note**
> If Scriban file name starts with an underscore, then it will not be rendered but can be included in other scripts using [`include`](https://github.com/scriban/scriban/blob/master/doc/language.md#911-include-name-arg1argn) statement.

For example, to render all scriban templates in the `ScribanTemplates` folder add this to the project file:

```xml
<ItemGroup>
    <AdditionalFiles Include="ScribanTemplates\*"/>
</ItemGroup>
```


## Saving generated files

To save the generated source files set properties `EmitCompilerGeneratedFiles` and `CompilerGeneratedFilesOutputPath`. For Example:

```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>
        $(BaseIntermediateOutputPath)\GeneratedFiles
    </CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

## Limitations

Passing parameters to Scriban scripts is not supported.