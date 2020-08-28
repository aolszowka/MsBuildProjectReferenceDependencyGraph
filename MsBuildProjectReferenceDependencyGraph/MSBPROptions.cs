// -----------------------------------------------------------------------
// <copyright file="MSBPROptions.cs" company="Ace Olszowka">
// Copyright (c) 2020 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace MsBuildProjectReferenceDependencyGraph
{
    /// <summary>
    /// Class to store options that control how MsBuildProjectReferenceDependencyGraph operates.
    /// </summary>
    public class MSBPROptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to anonymize names within the graph.
        /// </summary>
        public bool AnonymizeNames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show Assembly References
        /// </summary>
        public bool ShowAssemblyReferences { get; set; }

        public bool ShowFullPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show PackageReferences
        /// </summary>
        public bool ShowPackageReferences { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to sort the graph.
        /// </summary>
        public bool SortProjects { get; set; }
    }
}
