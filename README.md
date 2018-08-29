# MsBuildProjectReferenceDependencyGraph
Utility to take a MsBuild Project File and generate a DOT Graph of all its ProjectReference Elements

## Usage
`MsBuildProjectReferenceDependencyGraph.exe  MyProject.csproj > out.g`

Because the output is simply piped to Standard Out we redirect it to an output file for further processing by other utilities.

For example you can use Webgraphviz (http://www.webgraphviz.com/) to produce a graph online or download and install GraphViz (https://graphviz.gitlab.io/)

You can also use the optional Anonymize Tag (-a,-anonymize,/a,/anonymize are all valid) to produce an anonymized version of your graph.

`MsBuildProjectReferenceDependencyGraph.exe  MyProject.csproj -anonymize > out.g`
`MsBuildProjectReferenceDependencyGraph.exe  MyProject.csproj -a > out.g`

## License
This is licensed under the MIT License.

## Bugs/Feature Requests
I accept pull requests and am responsive on GitHub, Let me know!
