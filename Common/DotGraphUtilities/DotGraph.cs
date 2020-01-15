// -----------------------------------------------------------------------
// <copyright file="DotGraph.cs" company="Ace Olszowka">
// Copyright (c) 2019-2020 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace DotGraphUtilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Class Library for dealing with DotGraph Files
    /// </summary>
    public static class DotGraph
    {
        public static IDictionary<string, SortedSet<string>> LoadDependencyGraph(string targetFile)
        {
            IDictionary<string, SortedSet<string>> result = new SortedDictionary<string, SortedSet<string>>();

            IEnumerable<string> validDigraphLines = ParseForValidDigraphLines(targetFile);

            foreach (string line in validDigraphLines)
            {
                string sourceProject = string.Empty;
                string dependentProject = string.Empty;

                if (line.Contains("->"))
                {
                    string[] splitLine = line.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
                    sourceProject = splitLine[0].Trim();
                    dependentProject = splitLine[1].Trim();
                }
                else
                {
                    // This is a "naked" file reference
                    sourceProject = line;
                }

                if (!result.ContainsKey(sourceProject))
                {
                    // We have a new source project add it
                    result.Add(sourceProject, new SortedSet<string>());
                }

                if (!string.IsNullOrEmpty(dependentProject))
                {
                    // We have a dependency that we need to add
                    result[sourceProject].Add(dependentProject);

                    // Create the dependent project at this time as a new source project
                    if(!result.ContainsKey(dependentProject))
                    {
                        result.Add(dependentProject, new SortedSet<string>());
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///   Given a DOT Graph File; strip any lines that do not describe
        /// the graph relationship (Comments/File Definitions/etc). Returning
        /// the relevant lines.
        /// </summary>
        /// <param name="targetFile">The DOT Graph File to Parse</param>
        /// <returns>An <see cref="IEnumerable{string}"/> of the graph relationship</returns>
        public static IEnumerable<string> ParseForValidDigraphLines(string targetFile)
        {
            IEnumerable<string> dotGraphLines = File.ReadLines(targetFile);

            foreach (string line in dotGraphLines)
            {
                string trimmedLine = line.Trim();

                if (
                    trimmedLine.StartsWith("digraph") ||
                    trimmedLine.StartsWith("}") ||
                    trimmedLine.StartsWith("//") ||
                    trimmedLine.StartsWith("label")
                    )
                {
                    // These are to be ignored
                }
                else
                {
                    string doubleQuoteSanitized = trimmedLine.Replace("\"", string.Empty);
                    string returnedLine = Regex.Replace(doubleQuoteSanitized, "\\[.*\\]", string.Empty).Trim();

                    if (!string.IsNullOrWhiteSpace(returnedLine))
                    {
                        yield return returnedLine;
                    }
                }
            }
        }
    }
}
