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
            SolutionFile solution = SolutionFile.Parse(targetSolutionFile);

            return solution.ProjectsInOrder.Select(project => project.AbsolutePath);
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
