// -----------------------------------------------------------------------
// <copyright file="ParallelAbilityTests.cs" company="Ace Olszowka">
// Copyright (c) 2020 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace ProcessParallelAbility.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ParallelAbilityTests
    {
        [TestCaseSource(typeof(ResolveParallelTreeTests))]
        public void ResolveParallelTree(IDictionary<string, SortedSet<string>> dependencyTree, IDictionary<string, int> expected)
        {
            IDictionary<string, int> actual = ParallelAbility.ResolveParallelTree(dependencyTree);

            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }

    internal class ResolveParallelTreeTests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData
                (
                new Dictionary<string, SortedSet<string>>()
                {
                    // This is this tree
                    // digraph {
                    // A -> B
                    // B -> C
                    // D -> C
                    // E
                    // F -> B
                    // }
                    { "A", new SortedSet<string>() {"B"} },
                    { "B", new SortedSet<string>() {"C"} },
                    { "C", new SortedSet<string>() },
                    { "D", new SortedSet<string>() {"C"} },
                    { "E", new SortedSet<string>() },
                    { "F", new SortedSet<string>() {"B"} },
                },
                new Dictionary<string, int>
                {
                    { "A", 2 },
                    { "B", 1 },
                    { "C", 0 },
                    { "D", 1 },
                    { "E", 0 },
                    { "F", 2 },
                }
                ).SetName("{m}(ComplexDependencyTest)");
            yield return new TestCaseData
                (
                new Dictionary<string, SortedSet<string>>()
                {
                                // This is this tree
                                // digraph {
                                // A -> B
                                // }
                                { "A", new SortedSet<string>() {"B"} },
                                { "B", new SortedSet<string>() },
                },
                new Dictionary<string, int>
                {
                                { "A", 1 },
                                { "B", 0 },
                }
                ).SetName("{m}(SimpleDependencyTest)");
           yield return new TestCaseData
                (
                new Dictionary<string, SortedSet<string>>()
                {
                                // This is this tree
                                // digraph {
                                // A
                                // B
                                // }
                                { "A", new SortedSet<string>() },
                                { "B", new SortedSet<string>() },
                },
                new Dictionary<string, int>
                {
                                { "A", 0 },
                                { "B", 0 },
                }
                ).SetName("{m}(SimpleTest)");
        }
    }
}
