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

            Dictionary<string, IEnumerable<string>> projectReferenceDependencies = MSBPRDependencyGraph.ResolveProjectReferenceDependencies(projectsToEvaluate);

            string output = MSBPRDependencyGraph.CreateDOTGraph(projectReferenceDependencies, anonymizeNames, sortOutput, false);

            Console.WriteLine(output);
        }
    }
}
