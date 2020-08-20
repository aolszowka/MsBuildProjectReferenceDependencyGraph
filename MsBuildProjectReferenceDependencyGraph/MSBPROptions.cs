// -----------------------------------------------------------------------
// <copyright file="MSBPROptions.cs" company="Ace Olszowka">
// Copyright (c) 2020 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace MsBuildProjectReferenceDependencyGraph
{
    internal class MSBPROptions
    {
        public bool AnonymizeNames { get; set; }

        public bool SortProjects { get; set; }

        public bool ShowAssemblyReferences { get; set; }
    }
}
