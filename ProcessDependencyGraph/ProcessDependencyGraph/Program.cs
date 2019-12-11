// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Ace Olszowka">
// Copyright (c) 2019 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace ProcessDependencyGraph
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("You did not provide the required targetProject argument.");
                Environment.Exit(1);
            }

            // Get the DotGraph File
            string dotGraphFile = args.FirstOrDefault();

            // See if the target project flag has been set
            string targetProject = _ParseForTargetProjectFlag(args);

            if (string.IsNullOrWhiteSpace(dotGraphFile) || !File.Exists(dotGraphFile))
            {
                string missingDotGraphFile = "The Provided DotGraph Argument was not valid. This tool requires a valid DotGraph File.";
                Console.WriteLine(missingDotGraphFile);
                Environment.Exit(1);
            }

            if (string.IsNullOrWhiteSpace(targetProject))
            {
                string missingTargetProject = "You must provide a `-TargetProject` to this tool.";
                Console.WriteLine(missingTargetProject);
                Environment.Exit(1);
            }

            ProcessOperation operation = _ParseForOperation(args);

            IDictionary<string, SortedSet<string>> loadedGraph = DotGraph.LoadDependencyGraph(dotGraphFile);

            string consoleOutput = string.Empty;

            switch (operation)
            {
                case ProcessOperation.Default:
                case ProcessOperation.IdentifyRequiredProjects:
                    {
                        consoleOutput = IdentifyRequiredProjects.Execute(targetProject, loadedGraph);
                        break;
                    }
                case ProcessOperation.IdentifyAffectedProjects:
                    {
                        consoleOutput = IdentifyAffectedProjects.Execute(targetProject, loadedGraph);
                        break;
                    }
                case ProcessOperation.IdentifyDirectDependencyCounts:
                    {
                        consoleOutput = IdentifyDirectDependencyCounts.Execute(targetProject, loadedGraph);
                        break;
                    }
                default:
                    {
                        Console.WriteLine($"Unknown Operation {operation.ToString()}");
                        Environment.Exit(1);
                        break;
                    }
            }

            Console.WriteLine(consoleOutput);
        }

        /// <summary>
        /// Parses the Input Arguments for the Operation Flag
        /// </summary>
        /// <param name="args">The arguments sent to this program.</param>
        /// <returns>The value of the Operation Flag; or <see cref="ProcessOperation.Default"/> if one is not provided.</returns>
        private static ProcessOperation _ParseForOperation(string[] args)
        {
            ProcessOperation result = ProcessOperation.Default;

            if (args.Any(currentArg => Regex.IsMatch(currentArg, @"(?i)^[-\/]+(irp{1}|identifyrequiredprojects)$")))
            {
                result = ProcessOperation.IdentifyRequiredProjects;
            }

            if (args.Any(currentArg => Regex.IsMatch(currentArg, @"(?i)^[-\/]+(iap{1}|identifyaffectedprojects)$")))
            {
                result = ProcessOperation.IdentifyAffectedProjects;
            }

            if (args.Any(currentArg => Regex.IsMatch(currentArg, @"(?i)^[-\/]+(iddc{1}|identifydirectdependencycounts)$")))
            {
                result = ProcessOperation.IdentifyDirectDependencyCounts;
            }

            return result;
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

    internal enum ProcessOperation
    {
        Default,
        IdentifyRequiredProjects,
        IdentifyAffectedProjects,
        IdentifyDirectDependencyCounts,
    }
}
