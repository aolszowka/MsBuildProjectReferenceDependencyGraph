# MsBuildProjectReferenceDependencyGraph
Utility to take a MsBuild Project File or Visual Studio Solution file and generate a DOT Graph of all its ProjectReference Elements

## Usage
`MsBuildProjectReferenceDependencyGraph.exe  MyProject.csproj > out.g`

Because the output is simply piped to Standard Out we redirect it to an output file for further processing by other utilities.

For example you can use Webgraphviz (http://www.webgraphviz.com/) to produce a graph online or download and install GraphViz (https://graphviz.gitlab.io/)

## Flags
The first argument must always be the project/solution file to operate on.

### Anonymize
Optionally you can use the Anonymize Tag (-a,-anonymize,/a,/anonymize are all valid) to produce an anonymized version of your graph. This is useful for when you wish to share the general shape of your dependency tree without exposing any privilaged information.

`MsBuildProjectReferenceDependencyGraph.exe  MyProject.csproj -anonymize > out.g`
`MsBuildProjectReferenceDependencyGraph.exe  MyProject.csproj -a > out.g`

### Sort
Optionally you can use the Sort Tag (-s,-sort,/s/sort are all valid) to sort the output (based on filename) if you need the output to be idempotent. For example consider using the tool to dump trees in a tight loop and committing the changes (if any) into a version control system.

`MsBuildProjectReferenceDependencyGraph.exe  MyProject.csproj -s > out.g`

## License
This is licensed under the MIT License.

## Bugs/Feature Requests
I accept pull requests and am responsive on GitHub, Let me know!
