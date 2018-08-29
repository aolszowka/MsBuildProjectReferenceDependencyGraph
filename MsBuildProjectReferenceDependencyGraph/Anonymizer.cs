// -----------------------------------------------------------------------
// <copyright file="Anonymizer.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2015-2018. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace MsBuildProjectReferenceDependencyGraph
{
    using System;
    using System.Collections.Generic;

    internal class Anonymizer<T> : IDisposable
    {
        private IEnumerator<string> permutationGenerator;
        private IDictionary<T, string> knownItems;
        private bool disposed;

        public Anonymizer()
        {
            this.permutationGenerator = GeneratePermutations(4, "0123456789".ToCharArray()).GetEnumerator();

            // Spend the first value of this enumeration
            this.permutationGenerator.MoveNext();

            this.knownItems = new Dictionary<T,string>();
        }

        public string Anonymoize(T target)
        {
            if (!knownItems.ContainsKey(target))
            {
                if (this.permutationGenerator.MoveNext())
                {
                    knownItems.Add(target, this.permutationGenerator.Current.TrimStart('0'));
                }
                else
                {
                    throw new OverflowException("Not enough anonymous representations available");
                }
            }

            return knownItems[target];
        }

        public void Dispose()
        {
            if (!disposed)
            {
                this.permutationGenerator.Dispose();
                this.disposed = true;
            }
        }

        private static IEnumerable<string> GeneratePermutations(int slots, IEnumerable<char> possibleValues)
        {
            foreach (char value in possibleValues)
            {
                if (slots > 1)
                {
                    foreach (string otherSlotValue in GeneratePermutations(slots - 1, possibleValues))
                    {
                        yield return value + otherSlotValue;
                    }
                }
                else
                {
                    yield return value.ToString();
                }
            }
        }
    }
}
