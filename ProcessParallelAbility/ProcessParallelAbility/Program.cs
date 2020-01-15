// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Ace Olszowka">
// Copyright (c) 2020 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace ProcessParallelAbility
{
    using System;
    using System.Collections.Generic;
    using DotGraphUtilities;

    class Program
    {
        static void Main(string[] args)
        {
            string[] targetGraphs =
                new string[]
                {

                };

            foreach (string targetGraph in targetGraphs)
            {
                PrintLevelsForGraph(targetGraph);
                Console.WriteLine();
            }
        }

        private static void PrintLevelsForGraph(string targetGraph)
        {
            IDictionary<string, SortedSet<string>> dotGraph = DotGraph.LoadDependencyGraph(targetGraph);

            // Get the Parallel Tree
            IDictionary<string, int> parallelTree = ParallelAbility.ResolveParallelTree(dotGraph);

            // Reorganize the Parallel Tree to show the number of levels
            IDictionary<int, List<string>> levels = ParallelAbility.ConvertParallelTreeToLevels(parallelTree);

            Console.WriteLine($"For {targetGraph}");
            foreach (KeyValuePair<int, List<string>> level in levels)
            {
                Console.WriteLine($"{level.Key}\t{level.Value.Count}");
            }
        }
    }
}
