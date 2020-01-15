// -----------------------------------------------------------------------
// <copyright file="MSBuildUtilitiesTests.cs" company="Ace Olszowka">
// Copyright (c) 2019-2020 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace MsBuildProjectReferenceDependencyGraph.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using NUnit.Framework;

    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    class MSBuildUtilitiesTests
    {
        [TestCaseSource(typeof(AssemblyReferencesTestCases))]
        public void AssemblyReferences(string targetProject, IEnumerable<string> expected)
        {
            IEnumerable<string> actual = MSBuildUtilities.AssemblyReferences(targetProject);
            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }

    internal class AssemblyReferencesTestCases : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new
                TestCaseData
                (
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "TestProjects", "ProjectA.csproj"),
                    new string[]
                    {
                        "log4net",
                        "Moq",
                        "System",
                        "System.Core",
                        "System.Data",
                        "System.Data.DataSetExtensions",
                        "System.Data.Linq",
                        "System.Drawing",
                        "System.Transactions",
                        "System.Web.Services",
                        "System.Windows.Forms",
                        "System.Xml",
                        "System.Xml.Linq",
                        "xfnlnet"
                    }
                ).SetName("{m}_TestCase1");
            yield return new
                TestCaseData
                (
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "TestProjects", "ProjectB.csproj"),
                    new string[]
                    {
                        "Microsoft.VisualBasic",
                        "System",
                        "System.Core",
                        "System.Data",
                        "System.Data.DataSetExtensions",
                        "System.Data.Linq",
                        "System.Drawing",
                        "System.EnterpriseServices",
                        "System.Runtime.Serialization",
                        "System.Transactions",
                        "System.Web",
                        "System.Web.Services",
                        "System.Windows.Forms",
                        "System.Xml",
                        "System.Xml.Linq",
                        "xfnlnet"
                    }
                ).SetName("{m}_TestCase2");
        }
    }
}
