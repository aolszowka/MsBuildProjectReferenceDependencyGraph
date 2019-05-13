// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Ace Olszowka">
// Copyright (c) 2018-2019 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace MsBuildProjectReferenceDependencyGraph
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Toy program to generate a DOT Graph of all ProjectReference dependencies of a project.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("You did not provide the required targetProject argument.");
                Environment.Exit(1);
            }

            // See if the anonymize flag has been sent
            bool anonymizeNames = args.Any(current => Regex.IsMatch(current.ToLowerInvariant(), @"^[-\/]+([a]{1}|anonymize)$"));

            // See if the Sort Flag has been set
            bool sortOutput = args.Any(current => Regex.IsMatch(current.ToLowerInvariant(), @"^[-\/]+([s]{1}|sort)$"));

            // See if the target project flag has been set
            string targetProject = _ParseForTargetProjectFlag(args);

            string targetArgument = args.First();

            List<string> projectsToEvaluate = new List<string>();

            if (Path.GetExtension(targetArgument).Equals(".sln", StringComparison.InvariantCultureIgnoreCase))
            {
                IEnumerable<string> projectsInSolution = MSBuildUtilities.GetProjectsFromSolution(targetArgument);

                // These come back as relative paths; we need to "expand" them
                // otherwise we'll get duplicates when we go to resolve.
                projectsInSolution = PathUtilities.ResolveRelativePaths(projectsInSolution);

                projectsToEvaluate.AddRange(projectsInSolution);
            }
            else
            {
                // Assume its just a single project
                projectsToEvaluate.Add(targetArgument);
            }

            Dictionary<string, IEnumerable<string>> projectReferenceDependencies = ResolveProjectReferenceDependencies(projectsToEvaluate);

            string output = CreateDOTGraph(projectReferenceDependencies, targetProject, anonymizeNames, sortOutput);

            Console.WriteLine(output);
        }

        /// <summary>
        /// Given a IEnumerable of Target Project Files, Resolve All N-Order ProjectReference Dependencies.
        /// </summary>
        /// <param name="targetProjects">An IEnumerable of strings that represent MSBuild Projects.</param>
        /// <returns>A Dictionary in which the Key is the Project, and the Value is an IEnumerable of all its Project Reference projects</returns>
        private static Dictionary<string, IEnumerable<string>> ResolveProjectReferenceDependencies(IEnumerable<string> targetProjects)
        {
            Stack<string> unresolvedProjects = new Stack<string>();
            Dictionary<string, IEnumerable<string>> resolvedProjects = new Dictionary<string, IEnumerable<string>>();

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
        /// <param name="targetProject">Determines the project (based on name) to be highlighted</param>
        /// <param name="anonymizeNames">Determines if the names should be anonymized.</param>
        /// <returns>A string that represents a DOT Graph</returns>
        private static string CreateDOTGraph(IDictionary<string, IEnumerable<string>> projectReferenceDependencies, string targetProject, bool anonymizeNames, bool sortProjects)
        {
            // If we are going to use a anonymizer initialize it
            Anonymizer<string> anonymizer = null;
            if (anonymizeNames)
            {
                anonymizer = new Anonymizer<string>();
            }

            // If we have a target project set a flag
            (string TargetProject, HashSet<string> NOrderDependencies) directNOrderDependencies = (string.Empty, new HashSet<string>());
            bool highlightNonNOrderDependencies = !string.IsNullOrWhiteSpace(targetProject);
            if (highlightNonNOrderDependencies)
            {
                directNOrderDependencies = GenerateDirectNOrderDependencies(targetProject, projectReferenceDependencies);
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("digraph {");

            // If we need to sort the projects do so at this time
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> projectReferenceDependenciesToPrint = projectReferenceDependencies;
            if (sortProjects)
            {
                projectReferenceDependenciesToPrint = projectReferenceDependencies.OrderBy(kvp => Path.GetFileName(kvp.Key));
            }

            foreach (KeyValuePair<string, IEnumerable<string>> kvp in projectReferenceDependenciesToPrint)
            {
                string projectName = Path.GetFileName(kvp.Key);

                if (anonymizeNames)
                {
                    projectName = anonymizer.Anonymoize(projectName);
                }

                IEnumerable<string> projectReferences = kvp.Value;

                if (sortProjects)
                {
                    projectReferences = projectReferences.OrderBy(filePath => Path.GetFileName(filePath));
                }

                if (highlightNonNOrderDependencies)
                {
                    // If we are being asked to highlight non-norder
                    // dependencies then we need to perform special
                    // formatting on the graph
                    if (directNOrderDependencies.TargetProject.Equals(kvp.Key))
                    {
                        sb.AppendLine($"\"{projectName}\" [style = filled, fillcolor = yellow, fontname=\"consolas\", fontcolor=black]");
                    }
                    else if (directNOrderDependencies.NOrderDependencies.Contains(kvp.Key))
                    {
                        sb.AppendLine($"\"{projectName}\" [style = filled, fillcolor = green, fontname=\"consolas\", fontcolor=black]");
                    }
                    else
                    {
                        sb.AppendLine($"\"{projectName}\" [style = filled, fillcolor = red, fontname=\"consolas\", fontcolor=black]");
                    }
                }
                else
                {
                    sb.AppendLine($"\"{projectName}\"");
                }

                foreach (string projectDependency in projectReferences)
                {
                    string projectDependencyName = Path.GetFileName(projectDependency);

                    if (anonymizeNames)
                    {
                        projectDependencyName = anonymizer.Anonymoize(projectDependencyName);
                    }

                    sb.AppendLine($"\"{projectName}\" -> \"{projectDependencyName}\"");
                }
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Identify the Direct N-Order Dependencies of a given Project
        /// </summary>
        /// <param name="targetProject">The project for which to identify Direct N-Order Dependencies.</param>
        /// <param name="projectReferenceDependencies">A Project Lookup Dictionary created by <see cref="ResolveProjectReferenceDependencies(IEnumerable{string})"/></param>
        /// <returns></returns>
        private static (string TargetProject, HashSet<string> NOrderDependencies) GenerateDirectNOrderDependencies(string targetProject, IDictionary<string, IEnumerable<string>> projectReferenceDependencies)
        {
            HashSet<string> result = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            string extendedTargetProject = string.Empty;
            IEnumerable<string> directNOrderReferences = new string[0];

            // We need to enumerate though the projectReferenceDictionary to
            // find our target project; we cannot simply perform a lookup
            // because the user input is not guaranteed to be fully qualified
            foreach (KeyValuePair<string, IEnumerable<string>> project in projectReferenceDependencies)
            {
                // For now we will just assume that the first guy in wins.
                // This will cause issues if you have a scenario where the
                // project name is duplicated in two paths; however this is
                // unlikely as this would cause a solution error.
                if (project.Key.EndsWith(targetProject, StringComparison.InvariantCultureIgnoreCase))
                {
                    extendedTargetProject = project.Key;
                    directNOrderReferences = project.Value;
                    break;
                }
            }

            Stack<string> resolver = new Stack<string>(directNOrderReferences);

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
                    IEnumerable<string> currentDependencyDirectDepends = projectReferenceDependencies[currentDependency];
                    foreach (string directDependency in currentDependencyDirectDepends)
                    {
                        resolver.Push(directDependency);
                    }
                }
            }

            return (extendedTargetProject, result);
        }

        /// <summary>
        /// Parses the Input Arguments for the TargetProject Flag
        /// </summary>
        /// <param name="args">The arguments sent to this program.</param>
        /// <returns>The value of the TargetProject Flag</returns>
        private static string _ParseForTargetProjectFlag(string[] args)
        {
            string targetProject = string.Empty;

            // If -TargetProject (-tp) 
            for (int i = 0; i < args.Length; i++)
            {
                // All flags are treated case insensitive
                string currentArg = args[i].ToLowerInvariant();
                bool targetProjectFlag = Regex.IsMatch(currentArg, @"^[-\/]+(tp{1}|targetproject)$");

                if (targetProjectFlag)
                {
                    // First ensure that we won't over index
                    if (i++ < args.Length)
                    {
                        // Then the next argument is assumed to be the target project
                        targetProject = args[i];

                        // Stop processing for more arguments
                        break;
                    }
                    else
                    {
                        Console.WriteLine("You must provide a target project with the -TargetProject flag");
                    }
                }
            }

            return targetProject;
        }
    }
}
