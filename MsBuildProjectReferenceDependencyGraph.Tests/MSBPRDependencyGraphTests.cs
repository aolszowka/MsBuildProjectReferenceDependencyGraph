// -----------------------------------------------------------------------
// <copyright file="MSBPRDependencyGraphTests.cs" company="Ace Olszowka">
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
                    "digraph {\r\n\"ProjectA.csproj\"\r\n\"ProjectB.csproj\"\r\n\"ProjectC.csproj\"\r\n\"ProjectC.csproj\" -> \"ProjectA.csproj\"\r\n\"ProjectC.csproj\" -> \"ProjectB.csproj\"\r\n}\r\n"
                ).SetName("SortedOutput");
            yield return new
                TestCaseData
                (
                    MSBPRDependencyGraph.ResolveProjectReferenceDependencies(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "TestProjects", "ProjectC.csproj") }),
                    new MSBPROptions { SortProjects = true, AnonymizeNames = true },
                    "digraph {\r\n\"1\"\r\n\"2\"\r\n\"3\"\r\n\"3\" -> \"1\"\r\n\"3\" -> \"2\"\r\n}\r\n"
                ).SetName("SortAnonymize");
            yield return new
                TestCaseData
                (
                    MSBPRDependencyGraph.ResolveProjectReferenceDependencies(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "TestProjects", "ProjectD.csproj") }),
                    new MSBPROptions { SortProjects = true, ShowAssemblyReferences = true },
                    "digraph {\r\n\"ProjectD.csproj\"\r\n//--------------------------\r\n// AssemblyReference Section\r\n//--------------------------\r\n\"Moq\" [class=\"AssemblyReference\"]\r\n\"ProjectD.csproj\" -> \"Moq\"\r\n\"ProjectD.csproj\" -> \"System\"\r\n\"System\" [class=\"AssemblyReference\"]\r\n}\r\n"
                ).SetName("SortShowAssemblyReferences");
            yield return new
                TestCaseData
                (
                    MSBPRDependencyGraph.ResolveProjectReferenceDependencies(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "TestProjects", "ProjectD.csproj") }),
                    new MSBPROptions { SortProjects = true, ShowPackageReferences = true },
                    "digraph {\r\n\"ProjectD.csproj\"\r\n//--------------------------\r\n// PackageReference Section\r\n//--------------------------\r\n\"NUnit\" [class=\"PackageReference\"]\r\n\"ProjectD.csproj\" -> \"NUnit\"\r\n}\r\n"
                ).SetName("SortShowPackageReferences");
            yield return new
                TestCaseData
                (
                    MSBPRDependencyGraph.ResolveProjectReferenceDependencies(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "TestProjects", "ProjectD.csproj") }),
                    new MSBPROptions { SortProjects = true, ShowAssemblyReferences = true, ShowPackageReferences = true },
                    "digraph {\r\n\"ProjectD.csproj\"\r\n//--------------------------\r\n// AssemblyReference Section\r\n//--------------------------\r\n\"Moq\" [class=\"AssemblyReference\"]\r\n\"ProjectD.csproj\" -> \"Moq\"\r\n\"ProjectD.csproj\" -> \"System\"\r\n\"System\" [class=\"AssemblyReference\"]\r\n//--------------------------\r\n// PackageReference Section\r\n//--------------------------\r\n\"NUnit\" [class=\"PackageReference\"]\r\n\"ProjectD.csproj\" -> \"NUnit\"\r\n}\r\n"
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
