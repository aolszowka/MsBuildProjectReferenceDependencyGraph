// -----------------------------------------------------------------------
// <copyright file="IdentifyAffectedProjects.cs" company="Ace Olszowka">
// Copyright (c) 2019 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace ProcessDependencyGraph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class to Identify Affected Projects in a DependencyGraph
    /// </summary>
    internal static class IdentifyAffectedProjects
    {
        /// <summary>
        ///     Generate a DOT Graph of the given Dependency Graph that
        /// highlights the projects that would be affected by a change
        /// to the <paramref name="targetProject"/>.
        /// </summary>
        /// <param name="targetProject">The project that would be changed.</param>
        /// <param name="dependencyGraph">A Dependency Graph where the Key is the Project and the Value is the Direct References of that project.</param>
        /// <returns>
        ///     A string that contains a DOT Graph that is formated such that
        /// projects that would be affected by a change to the
        /// <paramref name="targetProject"/> are highlighted in Red.
        /// </returns>
        internal static string Execute(string targetProject, IDictionary<string, SortedSet<string>> dependencyGraph)
        {
            HashSet<string> affectedProjects = GenerateNOrderDependsOnMe(targetProject, dependencyGraph);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("digraph {");

            foreach (KeyValuePair<string, SortedSet<string>> kvp in dependencyGraph)
            {
                if (targetProject.Equals(kvp.Key))
                {
                    // If the project is the target project highlight it
                    sb.AppendLine($"\"{kvp.Key}\" [style=filled, fillcolor=yellow, fontname=\"consolas\", fontcolor=black, label=\"{kvp.Key} ({affectedProjects.Count})\"]");
                }
                else if (affectedProjects.Contains(kvp.Key))
                {
                    // If this project would be affected by a change to the target project
                    sb.AppendLine($"\"{kvp.Key}\" [style=filled, fillcolor=red, fontname=\"consolas\", fontcolor=black]");
                }
                else
                {
                    // Otherwise this project would remain untouched by changes
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
        /// Identify the N-Order Projects which Depend on this Project.
        /// </summary>
        /// <param name="targetProject">The project for which to identify projects that depend on it.</param>
        /// <param name="dependencyGraph">A Dependency Graph where the Key is the Project and the Value is the Direct References of that project.</param>
        /// <returns></returns>
        /// <remarks>
        ///     It is helpful to think of this as returning a list of projects
        /// which, if this project were modified, would need to be rebuilt.
        /// </remarks>
        internal static HashSet<string> GenerateNOrderDependsOnMe(string targetProject, IDictionary<string, SortedSet<string>> dependencyGraph)
        {
            HashSet<string> result = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            if (!dependencyGraph.ContainsKey(targetProject))
            {
                string exception = $"Could not find project `{targetProject}` in given dictionary.";
                throw new NotSupportedException(exception);
            }

            Stack<string> resolver = new Stack<string>();

            resolver.Push(targetProject);

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

                    // Now spin though the entire list looking for projects to push
                    ParallelQuery<string> currentDependsOnMe =
                        dependencyGraph
                        .AsParallel()
                        .Where(kvp => kvp.Value.Contains(currentDependency))
                        .Select(kvp => kvp.Key);

                    // Resolve these "recursively"
                    foreach (string directDependency in currentDependsOnMe)
                    {
                        resolver.Push(directDependency);
                    }
                }
            }

            return result;
        }
    }
}
