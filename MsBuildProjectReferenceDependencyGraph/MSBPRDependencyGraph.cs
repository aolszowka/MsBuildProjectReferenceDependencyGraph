// -----------------------------------------------------------------------
// <copyright file="MSBPRDependencyGraph.cs" company="Ace Olszowka">
// Copyright (c) 2018-2020 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace MsBuildProjectReferenceDependencyGraph
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Class to contain all of the Business Logic for MsBuildProjectReferenceDependencyGraph
    /// </summary>
    class MSBPRDependencyGraph
    {
        /// <summary>
        /// Given a IEnumerable of Target Project Files, Resolve All N-Order ProjectReference Dependencies.
        /// </summary>
        /// <param name="targetProjects">An IEnumerable of strings that represent MSBuild Projects.</param>
        /// <returns>A Dictionary in which the Key is the Project, and the Value is an IEnumerable of all its Project Reference projects</returns>
        internal static Dictionary<string, IEnumerable<string>> ResolveProjectReferenceDependencies(IEnumerable<string> targetProjects)
        {
            Stack<string> unresolvedProjects = new Stack<string>();
            Dictionary<string, IEnumerable<string>> resolvedProjects = new Dictionary<string, IEnumerable<string>>(StringComparer.InvariantCultureIgnoreCase);

            // Load up the initial projects to the stack
            foreach (string targetProject in targetProjects.Distinct())
            {
                unresolvedProjects.Push(targetProject);
            }

            while (unresolvedProjects.Count > 0)
            {
                string currentProject = unresolvedProjects.Pop();

                // First check just to make sure it wasn't already resolved.
                if (!resolvedProjects.ContainsKey(currentProject))
                {
                    // Get all this projects references
                    string[] projectDependencies = MSBuildUtilities.ProjectDependencies(currentProject).ToArray();

                    resolvedProjects.Add(currentProject, projectDependencies);

                    foreach (string projectDependency in projectDependencies)
                    {
                        // Save the stack by not resolving already resolved projects
                        if (!resolvedProjects.ContainsKey(projectDependency))
                        {
                            unresolvedProjects.Push(projectDependency);
                        }
                    }
                }
            }

            return resolvedProjects;
        }

        /// <summary>
        /// Given a Dictionary in which the Key Represents the Project and the Value represents the list Project Dependencies, generate a DOT Graph.
        /// </summary>
        /// <param name="projectReferenceDependencies">The dictionary to generate the graph for.</param>
        /// <param name="anonymizeNames">Determines if the names should be anonymized.</param>
        /// <param name="sortProjects">Determines if the output of the DOT Graph should be sorted.</param>
        /// <param name="showAssemblyReferences">Determines if Assembly/PackageReferences should be shown on the graph.</param>
        /// <returns>A string that represents a DOT Graph</returns>
        internal static string CreateDOTGraph(IDictionary<string, IEnumerable<string>> projectReferenceDependencies, MSBPROptions options)
        {
            // If we are going to use a anonymizer initialize it
            Anonymizer<string> anonymizer = null;
            if (options.AnonymizeNames)
            {
                anonymizer = new Anonymizer<string>();
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("digraph {");

            // If we need to sort the projects do so at this time
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> projectReferenceDependenciesToPrint = projectReferenceDependencies;
            if (options.SortProjects)
            {
                projectReferenceDependenciesToPrint = projectReferenceDependencies.OrderBy(kvp => Path.GetFileName(kvp.Key));
            }

            // Perform the ProjectReference Results
            foreach (KeyValuePair<string, IEnumerable<string>> kvp in projectReferenceDependenciesToPrint)
            {
                string projectName = Path.GetFileName(kvp.Key);

                if (options.AnonymizeNames)
                {
                    projectName = anonymizer.Anonymoize(projectName);
                }

                IEnumerable<string> projectReferences = kvp.Value;

                if (options.SortProjects)
                {
                    projectReferences = projectReferences.OrderBy(filePath => Path.GetFileName(filePath));
                }

                sb.AppendLine($"\"{projectName}\"");

                foreach (string projectDependency in projectReferences)
                {
                    string projectDependencyName = Path.GetFileName(projectDependency);

                    if (options.AnonymizeNames)
                    {
                        projectDependencyName = anonymizer.Anonymoize(projectDependencyName);
                    }

                    sb.AppendLine($"\"{projectName}\" -> \"{projectDependencyName}\"");
                }
            }

            // If we need to show assembly references find them
            if (options.ShowAssemblyReferences)
            {
                sb.AppendLine("//--------------------------");
                sb.AppendLine("// AssemblyReference Section");
                sb.AppendLine("//--------------------------");
                Dictionary<string, IEnumerable<string>> assemblyReferenceDependencies = ResolveAssemblyReferenceDependencies(projectReferenceDependencies.Keys);
                IEnumerable<string> assemblyReferenceSection = GenerateAssemblyReferenceSection(anonymizer, assemblyReferenceDependencies);

                if (options.SortProjects)
                {
                    assemblyReferenceSection = assemblyReferenceSection.OrderBy(x => x);
                }

                foreach (string line in assemblyReferenceSection)
                {
                    sb.AppendLine(line);
                }
            }

            if(options.ShowPackageReferences)
            {
                sb.AppendLine("//--------------------------");
                sb.AppendLine("// PackageReference Section");
                sb.AppendLine("//--------------------------");
                Dictionary<string, IEnumerable<string>> packageReferenceDependencies = ResolvePackageReferenceDependencies(projectReferenceDependencies.Keys);
                IEnumerable<string> packageReferenceSection = GeneratePackageReferenceSection(anonymizer, packageReferenceDependencies);

                if (options.SortProjects)
                {
                    packageReferenceSection = packageReferenceSection.OrderBy(x => x);
                }

                foreach (string line in packageReferenceSection)
                {
                    sb.AppendLine(line);
                }
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Generates the AssemblyReference Section of the DOT Graph.
        /// </summary>
        /// <param name="anonymizer">The Anonymizer (if used) to anonymize names</param>
        /// <param name="assemblyReferences">The Dictionary from <see cref="ResolveAssemblyReferenceDependencies(IEnumerable{string})"/></param>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains the lines to add to the DOT Graph</returns>
        internal static IEnumerable<string> GenerateAssemblyReferenceSection(Anonymizer<string> anonymizer, Dictionary<string, IEnumerable<string>> assemblyReferences)
        {
            // First we need to create nodes for each of the Assemblies
            IEnumerable<string> distinctAssemblyReferences = assemblyReferences.SelectMany(kvp => kvp.Value).Distinct();
            foreach (string distinctAssemblyReference in distinctAssemblyReferences)
            {
                string assemblyName = distinctAssemblyReference;

                if (anonymizer != null)
                {
                    assemblyName = anonymizer.Anonymoize(assemblyName);
                }

                yield return $"\"{assemblyName}\" [class=\"AssemblyReference\"]";
            }

            // Now Create the Connections
            foreach (KeyValuePair<string, IEnumerable<string>> kvp in assemblyReferences)
            {
                string projectName = Path.GetFileName(kvp.Key);

                if (anonymizer != null)
                {
                    projectName = anonymizer.Anonymoize(projectName);
                }

                IEnumerable<string> assemblyReferencesForCurrentProject = kvp.Value;

                foreach (string assemblyReference in assemblyReferencesForCurrentProject)
                {
                    string assemblyReferenceName = assemblyReference;

                    if (anonymizer != null)
                    {
                        assemblyReferenceName = anonymizer.Anonymoize(assemblyReferenceName);
                    }

                    yield return $"\"{projectName}\" -> \"{assemblyReferenceName}\"";
                }
            }
        }

        /// <summary>
        /// Generates the PackageReference Section of the DOT Graph.
        /// </summary>
        /// <param name="anonymizer">The Anonymizer (if used) to anonymize names</param>
        /// <param name="packageReferences">The Dictionary from <see cref="ResolvePackageReferenceDependencies(IEnumerable{string})"/></param>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains the lines to add to the DOT Graph</returns>
        internal static IEnumerable<string> GeneratePackageReferenceSection(Anonymizer<string> anonymizer, Dictionary<string, IEnumerable<string>> packageReferences)
        {
            // First we need to create the nodes for each of the Packages
            IEnumerable<string> distinctPackageReferences = packageReferences.SelectMany(kvp => kvp.Value).Distinct();
            foreach (string distinctPackageReference in distinctPackageReferences)
            {
                string packageReference = distinctPackageReference;

                if (anonymizer != null)
                {
                    packageReference = anonymizer.Anonymoize(packageReference);
                }

                yield return $"\"{packageReference}\" [class=\"PackageReference\"]";
            }

            // Now Create the Connections
            foreach (KeyValuePair<string, IEnumerable<string>> kvp in packageReferences)
            {
                string projectName = Path.GetFileName(kvp.Key);

                if (anonymizer != null)
                {
                    projectName = anonymizer.Anonymoize(projectName);
                }

                IEnumerable<string> packageReferencesForCurrentProject = kvp.Value;

                foreach (string packageReference in packageReferencesForCurrentProject)
                {
                    string packageReferenceName = packageReference;

                    if (anonymizer != null)
                    {
                        packageReferenceName = anonymizer.Anonymoize(packageReferenceName);
                    }

                    yield return $"\"{projectName}\" -> \"{packageReferenceName}\"";
                }
            }
        }

        /// <summary>
        /// Given a IEnumerable of Target Project Files, return all Assembly References.
        /// </summary>
        /// <param name="targetProjects">An IEnumerable of strings that represent MSBuild Projects.</param>
        /// <returns>A Dictionary in which the Key is the Project Path, and the Value is an IEnumerable of all its referenced Assemblies</returns>
        internal static Dictionary<string, IEnumerable<string>> ResolveAssemblyReferenceDependencies(IEnumerable<string> targetProjects)
        {
            ConcurrentBag<KeyValuePair<string, IEnumerable<string>>> resolvedAssemblyReferences = new ConcurrentBag<KeyValuePair<string, IEnumerable<string>>>();

            // Because of the large number of projects that could be scanned perform this in parallel.
            Parallel.ForEach(targetProjects, targetProject =>
            {
                IEnumerable<string> currentProjectAssemblyReferences = MSBuildUtilities.AssemblyReferences(targetProject);
                KeyValuePair<string, IEnumerable<string>> currentResult = new KeyValuePair<string, IEnumerable<string>>(targetProject, currentProjectAssemblyReferences);
                resolvedAssemblyReferences.Add(currentResult);
            }
            );

            // Convert this into the Dictionary
            Dictionary<string, IEnumerable<string>> result = new Dictionary<string, IEnumerable<string>>(StringComparer.InvariantCultureIgnoreCase);
            foreach (KeyValuePair<string, IEnumerable<string>> kvp in resolvedAssemblyReferences)
            {
                result.Add(kvp.Key, kvp.Value);
            }

            return result;
        }

        /// <summary>
        /// Given a IEnumerable of Target Project Files, return all Package References.
        /// </summary>
        /// <param name="targetProjects">An IEnumerable of strings that represent MSBuild Projects.</param>
        /// <returns>A Dictionary in which the Key is the Project Path, and the Value is an IEnumerable of all its referenced Packages</returns>
        internal static Dictionary<string, IEnumerable<string>> ResolvePackageReferenceDependencies(IEnumerable<string> targetProjects)
        {
            ConcurrentBag<KeyValuePair<string, IEnumerable<string>>> resolvedPackageReferences = new ConcurrentBag<KeyValuePair<string, IEnumerable<string>>>();

            // Because of the large number of projects that could be scanned perform this in parallel.
            Parallel.ForEach(targetProjects, targetProject =>
            {
                IEnumerable<string> currentProjectPackageReferences = MSBuildUtilities.PackageReferences(targetProject);
                KeyValuePair<string, IEnumerable<string>> currentResult = new KeyValuePair<string, IEnumerable<string>>(targetProject, currentProjectPackageReferences);
                resolvedPackageReferences.Add(currentResult);
            }
            );

            // Convert this into the Dictionary
            Dictionary<string, IEnumerable<string>> result = new Dictionary<string, IEnumerable<string>>(StringComparer.InvariantCultureIgnoreCase);
            foreach (KeyValuePair<string, IEnumerable<string>> kvp in resolvedPackageReferences)
            {
                result.Add(kvp.Key, kvp.Value);
            }

            return result;
        }
    }
}
