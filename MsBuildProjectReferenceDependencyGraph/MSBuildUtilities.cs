// -----------------------------------------------------------------------
// <copyright file="MSBuildUtilities.cs" company="Ace Olszowka">
// Copyright (c) 2018-2020 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace MsBuildProjectReferenceDependencyGraph
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    using Microsoft.Build.Construction;

    /// <summary>
    /// Class to hold all MSBuild Specific Logic
    /// </summary>
    public class MSBuildUtilities
    {
        private static readonly XNamespace msbuildNS = "http://schemas.microsoft.com/developer/msbuild/2003";

        /// <summary>
        ///     Gets an IEnumerable of strings representing the fully qualified
        /// paths to all of the projects referenced by the given solution.
        /// </summary>
        /// <param name="targetSolutionFile">The solution to parse.</param>
        /// <returns>The fully qualified paths to all of the projects in the solution.</returns>
        public static IEnumerable<string> GetProjectsFromSolution(string targetSolutionFile)
        {
            string solutionFolder = Path.GetDirectoryName(targetSolutionFile);
            SolutionFile solution = SolutionFile.Parse(targetSolutionFile);

            return
                solution
                .ProjectsInOrder
                .Where(project => IsSupportedProjectType(project))
                .Select(project => project.RelativePath)
                .Select(projectRelativePath => Path.GetFullPath(projectRelativePath, solutionFolder));
        }

        /// <summary>
        /// Determines if the Project Type is Known Supported
        /// </summary>
        /// <param name="project">The project to evaluate.</param>
        /// <returns><c>true</c> if this project is supported by this tool; otherwise, <c>false</c>.</returns>
        private static bool IsSupportedProjectType(ProjectInSolution project)
        {
            bool supportedType = false;

            if (project.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat)
            {
                supportedType = true;
            }
            else
            {
                // You can add additional supported types here
                string[] knownMsBuildProjectTypes = new string[]
                {
                    ".csproj",
                    ".sqlproj",
                    ".synproj",
                    ".vbproj",
                };

                string filePath = Path.GetExtension(project.AbsolutePath);

                supportedType = knownMsBuildProjectTypes.Any(knownProjectType => filePath.Equals(knownProjectType, System.StringComparison.InvariantCultureIgnoreCase));
            }

            return supportedType;
        }

        /// <summary>
        /// Given a path to a file that is assumed to be an MSBuild Type Project file, Return all Assembly References.
        /// </summary>
        /// <param name="targetProject">The project to load.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains all of the Assembly References.</returns>
        public static IEnumerable<string> AssemblyReferences(string targetProject)
        {
            XDocument projXml = XDocument.Load(targetProject);

            IEnumerable<XElement> assemblyReferences = projXml.Descendants(msbuildNS + "Reference");

            foreach (XElement assemblyReferenceElement in assemblyReferences)
            {
                string result = assemblyReferenceElement.Attribute("Include").Value;

                // For simplicity sake right now we will assume that the
                // project contains all equivalent assembly references
                // however it is possible that various projects could
                // reference different versions of the target assembly
                // in order to support this you'd need to return a more
                // complex object out of here to do further processing to
                // understand if the projects were referencing the same
                // version of the dependent assembly.
                try
                {
                    AssemblyName assemblyName = new AssemblyName(result);
                    result = assemblyName.Name;
                }
                catch (FileLoadException)
                {
                    // This was not a .NET Assembly; IE was a Synergy Library
                }

                yield return result;
            }
        }

        /// <summary>
        /// Given a path to a file that is assumed to be an MSBuild Type Project file, Return all Package References.
        /// </summary>
        /// <param name="targetProject">The project to load.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains all of the Package References.</returns>
        public static IEnumerable<string> PackageReferences(string targetProject)
        {
            XDocument projXml = XDocument.Load(targetProject);

            IEnumerable<XElement> packageReferences = projXml.Descendants(msbuildNS + "PackageReference");

            foreach (XElement packageReferenceElement in packageReferences)
            {
                string packageName = packageReferenceElement.Attribute("Include").Value;

                // Similar to the issue with Assembly References for simplicity
                // sake we will assume that all projects reference the same
                // package version; however if you need to start supporting
                // multiple versions you'll need to improve this area.
                yield return packageName;
            }
        }

        /// <summary>
        /// Given a path to a file that is assumed to be an MSBuild Type Project file, Return all ProjectReference Paths as fully qualified paths.
        /// </summary>
        /// <param name="targetProject">The project to load.</param>
        /// <returns>An IEnumerable that contains all the fully qualified ProjectReference paths.</returns>
        public static IEnumerable<string> ProjectDependencies(string targetProject)
        {
            XDocument projXml = null;
            try
            {
                projXml = XDocument.Load(targetProject);
            }
            catch (DirectoryNotFoundException)
            {
                Console.Error.WriteLine($@"ERROR: Could not load directory for {targetProject}");
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine($@"ERROR: Could not load file for {targetProject}");
            }

            if (projXml == null) yield break;

            IEnumerable<XElement> projectReferences = projXml.Descendants(msbuildNS + "ProjectReference");

            foreach (XElement projectReference in projectReferences)
            {
                string relativeProjectPath = projectReference.Attribute("Include").Value;
                string resolvedPath = Path.GetFullPath(UrlDecodePaths(relativeProjectPath),
                    Path.GetDirectoryName(targetProject));
                yield return resolvedPath;
            }
        }

        private static string UrlDecodePaths(string path) => path.Replace("%20", " ");
    }
}
