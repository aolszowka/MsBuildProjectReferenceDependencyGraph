// -----------------------------------------------------------------------
// <copyright file="MSBPRDependencyGraphTests.cs" company="Ace Olszowka">
// Copyright (c) 2019-2020 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace MsBuildProjectReferenceDependencyGraph.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    using NUnit.Framework;

    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class MSBPRDependencyGraphTests
    {
        [TestCaseSource(typeof(CreateDOTGraphTests))]
        public void CreateDOTGraph(IDictionary<string, IEnumerable<string>> projectReferenceDependencies, MSBPROptions options, string expected)
        {
            string actual = MSBPRDependencyGraph.CreateDOTGraph(projectReferenceDependencies, options);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(typeof(ResolveAssemblyReferenceTests))]
        public void ResolveAssemblyReferenceDependencies(IEnumerable<string> targetProjects, Dictionary<string, IEnumerable<string>> expected)
        {
            Dictionary<string, IEnumerable<string>> actual = MSBPRDependencyGraph.ResolveAssemblyReferenceDependencies(targetProjects);

            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }

    internal class CreateDOTGraphTests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new
                TestCaseData
                (
                    MSBPRDependencyGraph.ResolveProjectReferenceDependencies(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "TestProjects", "ProjectC.csproj") }),
                    new MSBPROptions { SortProjects = true },
                    $"digraph {{{Environment.NewLine}\"ProjectA.csproj\"{Environment.NewLine}\"ProjectB.csproj\"{Environment.NewLine}\"ProjectC.csproj\"{Environment.NewLine}\"ProjectC.csproj\" -> \"ProjectA.csproj\"{Environment.NewLine}\"ProjectC.csproj\" -> \"ProjectB.csproj\"{Environment.NewLine}}}{Environment.NewLine}"
                ).SetName("SortedOutput");
            yield return new
                TestCaseData
                (
                    MSBPRDependencyGraph.ResolveProjectReferenceDependencies(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "TestProjects", "ProjectC.csproj") }),
                    new MSBPROptions { SortProjects = true, AnonymizeNames = true },
                    $"digraph {{{Environment.NewLine}\"1\"{Environment.NewLine}\"2\"{Environment.NewLine}\"3\"{Environment.NewLine}\"3\" -> \"1\"{Environment.NewLine}\"3\" -> \"2\"{Environment.NewLine}}}{Environment.NewLine}"
                ).SetName("SortAnonymize");
            yield return new
                TestCaseData
                (
                    MSBPRDependencyGraph.ResolveProjectReferenceDependencies(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "TestProjects", "ProjectD.csproj") }),
                    new MSBPROptions { SortProjects = true, ShowAssemblyReferences = true },
                    $"digraph {{{Environment.NewLine}\"ProjectD.csproj\"{Environment.NewLine}//--------------------------{Environment.NewLine}// AssemblyReference Section{Environment.NewLine}//--------------------------{Environment.NewLine}\"Moq\" [class=\"AssemblyReference\"]{Environment.NewLine}\"ProjectD.csproj\" -> \"Moq\"{Environment.NewLine}\"ProjectD.csproj\" -> \"System\"{Environment.NewLine}\"System\" [class=\"AssemblyReference\"]{Environment.NewLine}}}{Environment.NewLine}"
                ).SetName("SortShowAssemblyReferences");
            yield return new
                TestCaseData
                (
                    MSBPRDependencyGraph.ResolveProjectReferenceDependencies(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "TestProjects", "ProjectD.csproj") }),
                    new MSBPROptions { SortProjects = true, ShowPackageReferences = true },
                    $"digraph {{{Environment.NewLine}\"ProjectD.csproj\"{Environment.NewLine}//--------------------------{Environment.NewLine}// PackageReference Section{Environment.NewLine}//--------------------------{Environment.NewLine}\"NUnit\" [class=\"PackageReference\"]{Environment.NewLine}\"ProjectD.csproj\" -> \"NUnit\"{Environment.NewLine}}}{Environment.NewLine}"
                ).SetName("SortShowPackageReferences");
            yield return new
                TestCaseData
                (
                    MSBPRDependencyGraph.ResolveProjectReferenceDependencies(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "TestProjects", "ProjectD.csproj") }),
                    new MSBPROptions { SortProjects = true, ShowAssemblyReferences = true, ShowPackageReferences = true },
                    $"digraph {{{Environment.NewLine}\"ProjectD.csproj\"{Environment.NewLine}//--------------------------{Environment.NewLine}// AssemblyReference Section{Environment.NewLine}//--------------------------{Environment.NewLine}\"Moq\" [class=\"AssemblyReference\"]{Environment.NewLine}\"ProjectD.csproj\" -> \"Moq\"{Environment.NewLine}\"ProjectD.csproj\" -> \"System\"{Environment.NewLine}\"System\" [class=\"AssemblyReference\"]{Environment.NewLine}//--------------------------{Environment.NewLine}// PackageReference Section{Environment.NewLine}//--------------------------{Environment.NewLine}\"NUnit\" [class=\"PackageReference\"]{Environment.NewLine}\"ProjectD.csproj\" -> \"NUnit\"{Environment.NewLine}}}{Environment.NewLine}"
                ).SetName("SortShowAssemblyShowPackageReferences");
        }
    }

    internal class ResolveAssemblyReferenceTests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new
                TestCaseData
                (
                    new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "TestProjects", "ProjectA.csproj") },
                    new Dictionary<string, IEnumerable<string>>
                    {
                        {
                            Path.Combine(TestContext.CurrentContext.TestDirectory,"TestProjects", "ProjectA.csproj"),
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
                        }
                    }
                ).SetName("{m}_SingleTestInput");
            yield return new
                TestCaseData
                (
                    new string[]
                    {
                        Path.Combine(TestContext.CurrentContext.TestDirectory,"TestProjects", "ProjectA.csproj"),
                        Path.Combine(TestContext.CurrentContext.TestDirectory,"TestProjects", "ProjectB.csproj")
                    },
                    new Dictionary<string, IEnumerable<string>>
                    {
                        {
                            Path.Combine(TestContext.CurrentContext.TestDirectory,"TestProjects", "ProjectA.csproj"),
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
                        },
                        {
                            Path.Combine(TestContext.CurrentContext.TestDirectory,"TestProjects", "ProjectB.csproj"),
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
                        }
                    }
                ).SetName("{m}_MultipleInputs");

        }
    }
}
