
namespace ProcessDependencyGraph.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    using DotGraphUtilities;

    using NUnit.Framework;

    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class IdentifyDirectDependencyCountsTests
    {
        [TestCaseSource(typeof(ExecuteTests))]
        public void Execute(string targetProject, IDictionary<string, SortedSet<string>> dependencyGraph, string expected)
        {
            string actual = IdentifyDirectDependencyCounts.Execute(targetProject, dependencyGraph);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Walk()
        {
            string targetProject = "A";
            IDictionary<string, SortedSet<string>> dependencyGraph = DotGraph.LoadDependencyGraph(Path.Combine(TestContext.CurrentContext.TestDirectory, "IdentifyDirectDependencyCountsTests", "TestCase1.g"));

            (Dictionary<string, string> ColoringDictionary, Dictionary<string, SortedSet<string>> UniqueProjectsForDependency) expected =
                (
                new Dictionary<string, string>() { { "B", "#336699" }, { "C", "#99CCFF" }, { "E", "#336699" }, { "F", "#336699" }, { "D", "#99CCFF" } },
                new Dictionary<string, SortedSet<string>>() { { "B", new SortedSet<string>() { "E", "F" } }, { "C", new SortedSet<string>() { "D" } } }
                );

            (Dictionary<string, string> ColoringDictionary, Dictionary<string, SortedSet<string>> UniqueProjectsForDependency) actual =
                IdentifyDirectDependencyCounts.Walk(targetProject, dependencyGraph);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }

    internal class ExecuteTests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new
                TestCaseData
                (
                    "A",
                    DotGraph.LoadDependencyGraph(Path.Combine(TestContext.CurrentContext.TestDirectory, "IdentifyDirectDependencyCountsTests", "TestCase1.g")),
                    File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "IdentifyDirectDependencyCountsTests", "TestCase1_Result.g"))
                ).SetArgDisplayNames("TestCase1.g");
            yield return new
                TestCaseData
                (
                    "36",
                    DotGraph.LoadDependencyGraph(Path.Combine(TestContext.CurrentContext.TestDirectory, "IdentifyDirectDependencyCountsTests", "TestCase2.g")),
                    File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "IdentifyDirectDependencyCountsTests", "TestCase2_Result.g"))
                ).SetArgDisplayNames("TestCase2.g");
            yield return new
                TestCaseData
                (
                    "A",
                    DotGraph.LoadDependencyGraph(Path.Combine(TestContext.CurrentContext.TestDirectory, "IdentifyDirectDependencyCountsTests", "TestCase3.g")),
                    File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "IdentifyDirectDependencyCountsTests", "TestCase3_Result.g"))
                ).SetArgDisplayNames("TestCase3.g");
        }
    }
}
