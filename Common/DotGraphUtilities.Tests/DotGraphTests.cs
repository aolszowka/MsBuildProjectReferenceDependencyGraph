// -----------------------------------------------------------------------
// <copyright file="DotGraphTests.cs" company="Ace Olszowka">
// Copyright (c) 2019-2020 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace DotGraphUtilities.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using NUnit.Framework;

    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class DotGraphTests
    {
        [TestCaseSource(typeof(LoadDependencyGraphTests))]
        public void LoadDependencyGraph(string inputFile, IDictionary<string, SortedSet<string>> expected)
        {
            IDictionary<string, SortedSet<string>> actual = DotGraph.LoadDependencyGraph(inputFile);

            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }

    internal class LoadDependencyGraphTests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new
                TestCaseData
                (
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "DotGraphTests", "TestCase1.g"),
                    new Dictionary<string, SortedSet<string>>()
                    {
                        { "A", new SortedSet<string>() { "B", "C", "D"} },
                        { "B", new SortedSet<string>() { "C" } },
                        { "C", new SortedSet<string>() { } },
                        { "D", new SortedSet<string>() { "B" } },
                    }
                ).SetArgDisplayNames("TestCase1.g");
            yield return new
                TestCaseData
                (
                Path.Combine(TestContext.CurrentContext.TestDirectory, "DotGraphTests", "TestCase2.g"),
                    new Dictionary<string, SortedSet<string>>()
                    {
                        { "A", new SortedSet<string>() { "B", "C"} },
                        { "B", new SortedSet<string>() { } },
                        { "C", new SortedSet<string>() { } },
                    }
                ).SetArgDisplayNames("TestCase2.g");
            yield return new
                 TestCaseData
                 (
                 Path.Combine(TestContext.CurrentContext.TestDirectory, "DotGraphTests", "TestCase3.g"),
                     new Dictionary<string, SortedSet<string>>()
                     {
                        { "A", new SortedSet<string>() { } },
                        { "B", new SortedSet<string>() { } },
                     }
                 ).SetArgDisplayNames("TestCase3.g");
        }
    }
}
