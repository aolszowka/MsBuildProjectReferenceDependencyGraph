// -----------------------------------------------------------------------
// <copyright file="MSBuildUtilities.cs" company="Ace Olszowka">
// Copyright (c) 2018-2019 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace MsBuildProjectReferenceDependencyGraph
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Build.Construction;

    /// <summary>
    /// Class to hold all MSBuild Specific Logic
    /// </summary>
    public class MSBuildUtilities
    {
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
                .Select(projectRelativePath => PathUtilities.ResolveRelativePath(solutionFolder, projectRelativePath));
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
                    ".vbproj",
                    ".synproj"
                };

                string filePath = Path.GetExtension(project.AbsolutePath);

                supportedType = knownMsBuildProjectTypes.Any(knownProjectType => filePath.Equals(knownProjectType, System.StringComparison.InvariantCultureIgnoreCase));
            }

            return supportedType;
        }

        /// <summary>
        /// Given a path to a file that is assumed to be an MSBuild Type Project file, Return all ProjectReference Paths as fully qualified paths.
        /// </summary>
        /// <param name="targetProject">The project to load.</param>
        /// <returns>An IEnumerable that contains all the fully qualified ProjectReference paths.</returns>
        public static IEnumerable<string> ProjectDependencies(string targetProject)
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
