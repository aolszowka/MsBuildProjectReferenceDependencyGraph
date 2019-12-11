// -----------------------------------------------------------------------
// <copyright file="IdentifyRequiredProjects.cs" company="Ace Olszowka">
// Copyright (c) 2019 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace ProcessDependencyGraph
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Class to Identify Required Projects in a DependencyGraph
    /// </summary>
    internal static class IdentifyRequiredProjects
    {
        /// <summary>
        ///     Generate a DOT Graph of the given Dependency Graph that
        /// highlights the projects that are required by the
        /// <paramref name="targetProject"/>.
        /// </summary>
        /// <param name="targetProject">The project that you wish to build.</param>
        /// <param name="dependencyGraph">A Dependency Graph where the Key is the Project and the Value is the Direct References of that project.</param>
        /// <returns>
        ///     A string that contains a DOT Graph that is formated such that
        /// projects that are required by <paramref name="targetProject"/>
        /// are highlighted in Green.
        /// </returns>
        internal static string Execute(string targetProject, IDictionary<string, SortedSet<string>> dependencyGraph)
        {
            HashSet<string> directNOrderDependencies = GenerateNOrderDependencies(targetProject, dependencyGraph);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("digraph {");

            foreach (KeyValuePair<string, SortedSet<string>> kvp in dependencyGraph)
            {
                if (targetProject.Equals(kvp.Key))
                {
                    // If the project is the target project highlight it
                    sb.AppendLine($"\"{kvp.Key}\" [style=filled, fillcolor=yellow, fontname=\"consolas\", fontcolor=black]");
                }
                else if (directNOrderDependencies.Contains(kvp.Key))
                {
                    // If the project is an N-Order Dependency it will need to be included in order to build this project
                    sb.AppendLine($"\"{kvp.Key}\" [style=filled, fillcolor=green, fontname=\"consolas\", fontcolor=black]");
                }
                else
                {
                    // Otherwise this project is unrelated to this
                    sb.AppendLine($"\"{kvp.Key}\" [fontname=\"consolas\", fontcolor=black]");
                }

                foreach (string dependency in kvp.Value)
                {
                    sb.AppendLine($"\"{kvp.Key}\" -> \"{dependency}\"");
                }
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        ///    Identify the N-Order Dependencies of a given Project based on
        /// the current dependency graph.
        /// </summary>
        /// <param name="targetProject">The project for which to identify N-Order Dependencies.</param>
        /// <param name="dependencyGraph">A Dependency Graph where the Key is the Project and the Value is the Direct References of that project.</param>
        /// <returns>The N-Order dependencies required by this project.</returns>
        /// <remarks>
        ///     It is helpful to think of this as returning a list of projects
        /// which are required to build 
        /// </remarks>
        internal static HashSet<string> GenerateNOrderDependencies(string targetProject, IDictionary<string, SortedSet<string>> dependencyGraph)
        {
            HashSet<string> result = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            if (!dependencyGraph.ContainsKey(targetProject))
            {
                string exception = $"Could not find project `{targetProject}` in given dictionary.";
                throw new NotSupportedException(exception);
            }

            Stack<string> resolver = new Stack<string>(dependencyGraph[targetProject]);

            while (resolver.Count != 0)
            {
                string currentDependency = resolver.Pop();
                if (result.Contains(currentDependency))
                {
                    // Do not attempt to resolve something that has already been resolved
                }
                else
                {
                    // First add the dependency to the list of resolved
                    result.Add(currentDependency);

                    // Now find its Direct Dependencies and add them to the list
                    IEnumerable<string> currentDependencyDirectDepends = dependencyGraph[currentDependency];
                    foreach (string directDependency in currentDependencyDirectDepends)
                    {
                        resolver.Push(directDependency);
                    }
                }
            }

            return result;
        }
    }
}
