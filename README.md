# MsBuildProjectReferenceDependencyGraph
Utility to take a MsBuild Project File or Visual Studio Solution file and generate a DOT Graph of all its ProjectReference Elements

## Usage
`MsBuildProjectReferenceDependencyGraph.exe  MyProject.csproj > out.g`

Because the output is simply piped to Standard Out we redirect it to an output file for further processing by other utilities.

For example you can use Webgraphviz (http://www.webgraphviz.com/) to produce a graph online or download and install GraphViz (https://graphviz.gitlab.io/)

### Extended Help
```text
Usage: MsBuildProjectReferenceDependencyGraph MyProject.sln [-s][-a][-sar][-spr][-sA]

Takes either an MsBuild Project File or Visual Studio Solution File and generate
a DOT Graph of all its ProjectReference Elements.

               <>            The Project or Solution to evaluate
  -a, --anonymize            Anonymizes the names of all references
      --sA, --ShowAllReferences
                             Show both "Assembly" and "PackageReference" 
                               References in graph
      --sar, --ShowAssemblyReferences
                             Show "Assembly" References in graph
      --spr, --ShowPackageReferences
                             Show "PackageReference" References in graph
  -s, --sort                 Sort the output of this tool
  -?, -h, --help             Show this message and exit

```


### Anonymize (-a | --anonymize)
Produce an anonymized version of your graph. This is useful for when you wish to share the general shape of your dependency tree without exposing any privileged information.

`MsBuildProjectReferenceDependencyGraph.exe  MyProject.csproj -a > out.g`

```text
digraph {
"1"
"2"
"3"
"3" -> "1"
"3" -> "2"
}
```

### Show* Flags
This program was originally designed only to trace `<ProjectReference>` References, however it has been extended to support showing other similar References such as `<Assembly>` and `<PackageReference>` directives. You must pass the appropriate flag to have the tool follow these.

They are each put into their own section in the generated DotGraph and are also appended with a `[class=""]` attribute to allow for any custom CSS applied to the DotGraph to call these out in a different style.

Be warned that this can generate __extremely complex__ graphs.

#### ShowAllReferences (-sA | --ShowAllReferences)
This combines all of the flags described below into a single argument.

`MsBuildProjectReferenceDependencyGraph.exe  ProjectD.csproj -sA > out.g`

```text
digraph {
"ProjectD.csproj"
//--------------------------
// AssemblyReference Section
//--------------------------
"Moq" [class="AssemblyReference"]
"ProjectD.csproj" -> "Moq"
"ProjectD.csproj" -> "System"
"System" [class="AssemblyReference"]
//--------------------------
// PackageReference Section
//--------------------------
"NUnit" [class="PackageReference"]
"ProjectD.csproj" -> "NUnit"
}
```

#### ShowAssemblyReferences (-sar | --ShowAssemblyReferences)
In addition to displaying `<ProjectReference>` a new section will be generated in the DotGraph to show `<Assembly>` References.

__WARNING__ Assembly References are considered "best effort" they are truncated down to just the assembly name. This means that if you have projects with references like:

Project 1
```xml
<Reference Include="Moq, Version=4.0.10827.0, Culture=neutral, PublicKeyToken=69f491c39445e920, processorArchitecture=MSIL">
    <HintPath>..\..\..\..\..\ProductDependencies\Moq\Moq.dll</HintPath>
</Reference>
```

Project 2
```xml
<Reference Include="Moq, Version=5.0.0, Culture=neutral, PublicKeyToken=69f491c39445e920, processorArchitecture=MSIL" />
```

Project 3
```xml
<Reference Include="Moq" />
```

This tooling treats all them as the same reference, even though MSBuild will not. This was done to simplify the processing of the dependencies.

`MsBuildProjectReferenceDependencyGraph.exe  MoqProject.csproj -sA > out.g`

```text
digraph {
"MoqProject.csproj"
//--------------------------
// AssemblyReference Section
//--------------------------
"Moq" [class="AssemblyReference"]
"MoqProject.csproj" -> "Moq"
}
```

#### ShowPackageReferences (-spr | --ShowPackageReferences)
In addition to displaying `<ProjectReference>` a new section will be generated in the DotGraph to show `<PackageReference>` References.

__WARNING__ Package References are considered "best effort" they are based on just the package name and do not account for the version. This means that if you have projects with references like:

Project 1
```xml
<PackageReference Include="NUnit">
    <Version>3.11.0</Version>
</PackageReference>
```

Project 2
```xml
<PackageReference Include="NUnit">
    <Version>3.12.0</Version>
</PackageReference>
```

This tooling treats all them as the same reference, even though MSBuild will not. This was done to simplify the processing of the dependencies.

`MsBuildProjectReferenceDependencyGraph.exe  NUnitProject.csproj -sA > out.g`

```text
digraph {
"NUnitProject.csproj"
//--------------------------
// PackageReference Section
//--------------------------
"NUnit" [class="PackageReference"]
"NUnitProject.csproj" -> "NUnit"
}
```

### Sort (-s | --sort)
Sort the output (based on filename) if you need the output to be idempotent. For example consider using the tool to dump trees in a tight loop and committing the changes (if any) into a version control system.

`MsBuildProjectReferenceDependencyGraph.exe  MyProject.csproj -s > out.g`

## License
This is licensed under the MIT License.

## Third Party Licenses
This project uses other open source contributions see [LICENSES.md](LICENSES.md) for a comprehensive listing.

## Bugs/Feature Requests
I accept pull requests and am responsive on GitHub, Let me know!
