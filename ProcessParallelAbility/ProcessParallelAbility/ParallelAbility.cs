// -----------------------------------------------------------------------
// <copyright file="ParallelAbility.cs" company="Ace Olszowka">
// Copyright (c) 2020 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace ProcessParallelAbility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class ParallelAbility
    {
        /// <summary>
        /// Given the result of <see cref="ResolveParallelTree(IDictionary{string, SortedSet{string}})"/> convert it to a structure for output.
        /// </summary>
        /// <param name="parallelTree">The structure returned by <see cref="ResolveParallelTree(IDictionary{string, SortedSet{string}})"/>.</param>
        /// <returns>A structure in which the Key is the "Level" or depth of the project and the Value is the list of projects at that level.</returns>
        internal static IDictionary<int, List<string>> ConvertParallelTreeToLevels(IDictionary<string, int> parallelTree)
        {
            IDictionary<int, List<string>> levels = new Dictionary<int, List<string>>();

            foreach (KeyValuePair<string, int> entry in parallelTree)
            {
                if (!levels.ContainsKey(entry.Value))
                {
                    levels.Add(entry.Value, new List<string>());
                }

                levels[entry.Value].Add(entry.Key);
            }

            return levels;
        }

        /// <summary>
        /// Given a Dependency Tree, return a structure that represents the depth within the dependency tree.
        /// </summary>
        /// <param name="dependencyTree">The dependency tree to evaluate.</param>
        /// <returns>A structure in which the Key is the project and the value is the depth within the given dependency tree.</returns>
        internal static IDictionary<string, int> ResolveParallelTree(IDictionary<string, SortedSet<string>> dependencyTree)
        {
            IDictionary<string, int> parallelBuildTree = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

            foreach (KeyValuePair<string, SortedSet<string>> dotGraphEntry in dependencyTree)
            {
                if (parallelBuildTree.ContainsKey(dotGraphEntry.Key))
                {
                    // We can Skip this because it was resolved
                }
                else
                {
                    // Push all of the projects as well as ourself
                    Stack<string> projectsToResolve = new Stack<string>(dotGraphEntry.Value);
                    projectsToResolve.Push(dotGraphEntry.Key);

                    while (projectsToResolve.Count != 0)
                    {
                        string currentProjectName = projectsToResolve.Pop();
                        if (parallelBuildTree.ContainsKey(currentProjectName))
                        {
                            // Skip it because it is resolved
                        }
                        else
                        {
                            try
                            {
                                string[] unresolvedDependencies = dependencyTree[currentProjectName].Where(dependency => !parallelBuildTree.ContainsKey(dependency)).ToArray();

                                if (unresolvedDependencies.Any())
                                {
                                    // We still need to resolve more N-Order Dependencies
                                    projectsToResolve.Push(currentProjectName);
                                    foreach (string dependency in unresolvedDependencies)
                                    {
                                        projectsToResolve.Push(dependency);
                                    }
                                }
                                else
                                {
                                    // Everything was resolved now we need to figure out at what level we should insert
                                    if (dependencyTree[currentProjectName].Count == 0)
                                    {
                                        // If we do not have any dependencies we're the deepest project
                                        parallelBuildTree.Add(currentProjectName, 0);
                                    }
                                    else
                                    {
                                        // Otherwise we need to find out what the deepest dependency is, and go one level deeper
                                        int deepestDependency = dependencyTree[currentProjectName].Select(dependency => parallelBuildTree[dependency]).Max();
                                        parallelBuildTree.Add(currentProjectName, deepestDependency + 1);
                                    }
                                }
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }
                }
            }

            return parallelBuildTree;
        }
    }
}
