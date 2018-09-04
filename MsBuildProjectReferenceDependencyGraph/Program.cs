// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Ace Olszowka">
// Copyright (c) 2018 Ace Olszowka.
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
    using System.Xml.Linq;

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
            bool anonymizeNames = args.Any(current => Regex.IsMatch(current, @"^[-\/]+([a]{1}|anonymize)$"));

            string targetArgument = args.First();

            List<string> projectsToEvaluate = new List<string>();

            if (Path.GetExtension(targetArgument).Equals(".sln", StringComparison.InvariantCultureIgnoreCase))
            {
                IEnumerable<string> projectsInSolution = MSBuildUtilities.GetProjectsFromSolution(targetArgument);
                projectsToEvaluate.AddRange(projectsInSolution);
            }
            else
            {
                // Assume its just a single project
                projectsToEvaluate.Add(targetArgument);
            }

            Dictionary<string, IEnumerable<string>> projectReferenceDependencies = ResolveProjectReferenceDependencies(projectsToEvaluate);

            string output = CreateDOTGraph(projectReferenceDependencies, anonymizeNames);

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
                    string[] projectDependencies = ProjectDependencies(currentProject).ToArray();

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
        /// <returns>A string that represents a DOT Graph</returns>
        private static string CreateDOTGraph(Dictionary<string, IEnumerable<string>> projectReferenceDependencies, bool anonymizeNames)
        {
            // If we are going to use a anonymizer initialize it
            Anonymizer<string> anonymizer = null;
            if (anonymizeNames)
            {
                anonymizer = new Anonymizer<string>();
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("digraph {");

            foreach (KeyValuePair<string, IEnumerable<string>> kvp in projectReferenceDependencies)
            {
                string projectName = Path.GetFileName(kvp.Key);

                if (anonymizeNames)
                {
                    projectName = anonymizer.Anonymoize(projectName);
                }

                foreach (string projectDependency in kvp.Value)
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
        /// Given a path to a file that is assumed to be an MSBuild Type Project file, Return all ProjectReference Paths as fully qualified paths.
        /// </summary>
        /// <param name="targetProject">The project to load.</param>
        /// <returns>An IEnumerable that contains all the fully qualified ProjectReference paths.</returns>
        static IEnumerable<string> ProjectDependencies(string targetProject)
        {
            XNamespace msbuildNS = "http://schemas.microsoft.com/developer/msbuild/2003";

            XDocument projXml = XDocument.Load(targetProject);

            IEnumerable<XElement> projectReferences = projXml.Descendants(msbuildNS + "ProjectReference");

            foreach (XElement projectReference in projectReferences)
            {
                string relativeProjectPath = projectReference.Attribute("Include").Value;
                string resolvedPath = PathUtilities.ResolveRelativePath(Path.GetDirectoryName(targetProject), relativeProjectPath);
                yield return resolvedPath;
            }
        }
    }
}
