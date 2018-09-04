namespace MsBuildProjectReferenceDependencyGraph
{
    using System.Collections.Generic;
    using System.Linq;
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
    }
}
