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
    using System.Linq;

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
            bool showHelp = false;

            // Create an Options Object
            MSBPROptions options = new MSBPROptions();

            OptionSet p = new OptionSet()
            {
                { "<>", Strings.TargetArgumentDescription, v => targetFile = v },
                { "a|anonymize", Strings.AnonymizeDescription, v => options.AnonymizeNames = v != null },
                { "sA|ShowAllReferences", Strings.ShowAllReferencesDescription, v => { if(v != null) { options.ShowAssemblyReferences = true; options.ShowPackageReferences = true; } } },
                { "sar|ShowAssemblyReferences", Strings.ShowAssemblyReferencesDescription, v => options.ShowAssemblyReferences = v != null },
                { "sfp|ShowFullPath", Strings.ShowFullPathDescription, v=> options.ShowFullPath = v != null },
                { "spr|ShowPackageReferences", Strings.ShowPackageReferencesDescription, v => options.ShowPackageReferences = v != null },
                { "s|sort", Strings.SortDescription, v => options.SortProjects = v != null },
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
                    projectsInSolution = projectsInSolution.Select(relativeProjectPath => Path.GetFullPath(relativeProjectPath));

                    projectsToEvaluate.AddRange(projectsInSolution);
                }
                else
                {
                    // Assume its just a single project
                    FileInfo fi = new FileInfo(targetFile);
                    projectsToEvaluate.Add(fi.FullName);
                }

                Dictionary<string, IEnumerable<string>> projectReferenceDependencies = MSBPRDependencyGraph.ResolveProjectReferenceDependencies(projectsToEvaluate);

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
