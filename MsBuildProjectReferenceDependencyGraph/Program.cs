// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Ace Olszowka">
// Copyright (c) 2018-2020 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace MsBuildProjectReferenceDependencyGraph
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using MsBuildProjectReferenceDependencyGraph.Properties;

    using NDesk.Options;

    /// <summary>
    /// Toy program to generate a DOT Graph of all ProjectReference dependencies of a project.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string targetFile = string.Empty;
            bool anonymizeNames = false;
            bool showHelp = false;
            bool sortOutput = false;

            OptionSet p = new OptionSet()
            {
                { "<>", Strings.TargetArgumentDescription, v => targetFile = v },
                { "a|anonymize", Strings.AnonymizeDescription, v => anonymizeNames = v != null },
                { "s|sort", Strings.SortDescription, v => sortOutput = v != null },
                { "?|h|help", Strings.HelpDescription, v => showHelp = v != null },
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException)
            {
                Console.WriteLine(Strings.ShortUsageMessage);
                Console.WriteLine($"Try `{Strings.ProgramName} --help` for more information.");
                return;
            }

            if (showHelp || string.IsNullOrEmpty(targetFile))
            {
                Environment.ExitCode = ShowUsage(p);
            }
            else if (!File.Exists(targetFile))
            {
                Console.WriteLine(Strings.InvalidTargetArgument, targetFile);
                Environment.ExitCode = 9009;
            }
            else
            {
                List<string> projectsToEvaluate = new List<string>();

                if (Path.GetExtension(targetFile).Equals(".sln", StringComparison.InvariantCultureIgnoreCase))
                {
                    IEnumerable<string> projectsInSolution = MSBuildUtilities.GetProjectsFromSolution(targetFile);

                    // These come back as relative paths; we need to "expand" them
                    // otherwise we'll get duplicates when we go to resolve.
                    projectsInSolution = PathUtilities.ResolveRelativePaths(projectsInSolution);

                    projectsToEvaluate.AddRange(projectsInSolution);
                }
                else
                {
                    // Assume its just a single project
                    projectsToEvaluate.Add(targetFile);
                }

                Dictionary<string, IEnumerable<string>> projectReferenceDependencies = MSBPRDependencyGraph.ResolveProjectReferenceDependencies(projectsToEvaluate);

                // Create an Options Object
                MSBPROptions options =
                    new MSBPROptions()
                    {
                        AnonymizeNames = anonymizeNames,
                        ShowAssemblyReferences = false,
                        SortProjects = sortOutput,
                    };

                string output = MSBPRDependencyGraph.CreateDOTGraph(projectReferenceDependencies, options);

                Console.WriteLine(output);
            }
        }

        private static int ShowUsage(OptionSet p)
        {
            Console.WriteLine(Strings.ShortUsageMessage);
            Console.WriteLine();
            Console.WriteLine(Strings.LongDescription);
            Console.WriteLine();
            Console.WriteLine($"               <>            {Strings.TargetArgumentDescription}");
            p.WriteOptionDescriptions(Console.Out);
            return 21;
        }
    }
}
