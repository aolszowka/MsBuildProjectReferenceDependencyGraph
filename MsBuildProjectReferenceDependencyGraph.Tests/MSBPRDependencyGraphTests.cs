// -----------------------------------------------------------------------
// <copyright file="MSBPRDependencyGraphTests.cs" company="Ace Olszowka">
// Copyright (c) 2019 Ace Olszowka.
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
        [TestCaseSource(typeof(ResolveAssemblyReferenceTests))]
        public void ResolveAssemblyReferenceDependencies(IEnumerable<string> targetProjects, Dictionary<string,IEnumerable<string>> expected)
        {
            Dictionary<string, IEnumerable<string>> actual = MSBPRDependencyGraph.ResolveAssemblyReferenceDependencies(targetProjects);

            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }

    internal class ResolveAssemblyReferenceTests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new
                TestCaseData
                (
                    new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory,"TestProjects", "ProjectA.csproj") },
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
