namespace ProcessDependencyGraph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal static class IdentifyDirectDependencyCounts
    {
        internal static string Execute(string targetProject, IDictionary<string, SortedSet<string>> dependencyGraph)
        {
            (Dictionary<string, string> ColoringDictionary, Dictionary<string, SortedSet<string>> UniqueProjectsForDependency) evaluatedGraph =
                Walk(targetProject, dependencyGraph);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("digraph {");

            foreach (KeyValuePair<string, SortedSet<string>> kvp in dependencyGraph)
            {
                if (targetProject.Equals(kvp.Key))
                {
                    // If the project is the target project highlight it
                    sb.AppendLine($"\"{kvp.Key}\" [style=filled, fillcolor=yellow, fontname=\"consolas\", fontcolor=black]");
                }
                else if (evaluatedGraph.UniqueProjectsForDependency.ContainsKey(kvp.Key))
                {
                    // If the project is an N-Order Dependency it will need to be included in order to build this project
                    sb.AppendLine($"\"{kvp.Key}\" [style=filled, fillcolor=\"{evaluatedGraph.ColoringDictionary[kvp.Key]}\", fontname=\"consolas\", fontcolor=black, label=\"{kvp.Key} ({evaluatedGraph.UniqueProjectsForDependency[kvp.Key].Count})\"]");
                }
                else
                {
                    if (evaluatedGraph.ColoringDictionary.ContainsKey(kvp.Key))
                    {
                        sb.AppendLine($"\"{kvp.Key}\" [style=filled, fillcolor=\"{evaluatedGraph.ColoringDictionary[kvp.Key]}\", fontname=\"consolas\", fontcolor=black]");
                    }
                    else
                    {
                        sb.AppendLine($"\"{kvp.Key}\" [fontname=\"consolas\", fontcolor=black]");
                    }
                }

                foreach (string dependency in kvp.Value)
                {
                    sb.AppendLine($"\"{kvp.Key}\" -> \"{dependency}\"");
                }
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        internal static (Dictionary<string, string> ColoringDictionary, Dictionary<string, SortedSet<string>> UniqueProjectsForDependency) Walk(string targetProject, IDictionary<string, SortedSet<string>> dependencyGraph)
        {
            // We are going to need to perform this for every direct dependency of the target project
            if (!dependencyGraph.ContainsKey(targetProject))
            {
                string exception = $"Could not find project `{targetProject}` in given dictionary.";
                throw new NotSupportedException(exception);
            }

            // If we do not have enough colors to represent all N-Order Dependencies we need to throw
            if (dependencyGraph[targetProject].Count > ColorScheme.Length)
            {
                string exception = $"There are not enough colors to properly render the graph. Need {dependencyGraph[targetProject].Count}; only have {ColorScheme.Length}";
                throw new NotSupportedException(exception);
            }

            Dictionary<string, string> coloringDictionary = new Dictionary<string, string>();
            Dictionary<string, SortedSet<string>> uniqueDependenciesForProject = new Dictionary<string, SortedSet<string>>();

            // Load the Initial Dependencies into the Color Dictionary
            string[] directDependencyProjects = dependencyGraph[targetProject].ToArray();
            for (int i = 0; i < directDependencyProjects.Length; i++)
            {
                coloringDictionary.Add(directDependencyProjects[i], ColorScheme[i]);
            }

            SortedSet<string> directDependenciesOfTargetProject = dependencyGraph[targetProject];

            foreach (string dependencyOfTargetProject in directDependenciesOfTargetProject)
            {
                // We need to remove the direct dependency from the existing
                // graph to determine what the remaining N-Order Dependencies
                // of the target project are. To do this we need a DEEP COPY
                // CLONE of the Original Dependency Graph. DO NOT USE THE
                // OVERLOAD WHICH IS ONLY A SHALLOW COPY!
                IDictionary<string, SortedSet<string>> modifiedDependencyGraph = CloneDependencyGraph(dependencyGraph);
                modifiedDependencyGraph[targetProject].Remove(dependencyOfTargetProject);
                HashSet<string> modifiedDependencies = IdentifyRequiredProjects.GenerateNOrderDependencies(targetProject, modifiedDependencyGraph);

                // Now using the original dependency graph get the N-Order
                // Dependencies of the current dependency
                HashSet<string> directDependencyDependencies = IdentifyRequiredProjects.GenerateNOrderDependencies(dependencyOfTargetProject, dependencyGraph);

                // Now if there are any dependencies that are ONLY dependencies
                // of the current dependency then those would "fall away" if we
                // trimmed out the reference.
                string[] distinctDependenciesOfCurrentDependency = directDependencyDependencies.Except(modifiedDependencies).ToArray();

                // Save this information
                uniqueDependenciesForProject.Add(dependencyOfTargetProject, new SortedSet<string>(distinctDependenciesOfCurrentDependency));

                // Update the coloring information as well
                foreach (string dependency in distinctDependenciesOfCurrentDependency)
                {
                    coloringDictionary.Add(dependency, coloringDictionary[dependencyOfTargetProject]);
                }
            }

            return (coloringDictionary, uniqueDependenciesForProject);
        }

        internal static IDictionary<string, SortedSet<string>> CloneDependencyGraph(IDictionary<string, SortedSet<string>> original)
        {
            Dictionary<string, SortedSet<string>> result = new Dictionary<string, SortedSet<string>>();

            foreach (KeyValuePair<string, SortedSet<string>> kvp in original)
            {
                result.Add(kvp.Key, new SortedSet<string>(kvp.Value));
            }

            return result;
        }

        internal static string[] ColorScheme =
            new string[]
            {
                "#336699",
                "#99CCFF",
                "#999933",
                "#666699",
                "#CC9933",
                "#006666",
                "#3399FF",
                "#993300",
                "#CCCC99",
                "#666666",
                "#FFCC66",
                "#6699CC",
                "#663366",
                "#9999CC",
                "#CCCCCC",
                "#669999",
                "#CCCC66",
                "#CC6600",
                "#9999FF",
                "#0066CC",
                "#99CCCC",
                "#999999",
                "#FFCC00",
                "#009999",
                "#99CC33",
                "#FF9900",
                "#999966",
                "#66CCCC",
                "#339966",
                "#CCCC33",
            };
    }
}
