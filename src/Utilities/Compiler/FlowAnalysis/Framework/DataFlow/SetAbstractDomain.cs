﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow
{
    internal class SetAbstractDomain<T> : AbstractDomain<ImmutableHashSet<T>>
    {
        public static SetAbstractDomain<T> Default = new SetAbstractDomain<T>();

        public override ImmutableHashSet<T> Bottom => ImmutableHashSet<T>.Empty;

        public override int Compare(ImmutableHashSet<T> oldValue, ImmutableHashSet<T> newValue, bool assertMonotonicity)
        {
            Debug.Assert(oldValue != null);
            Debug.Assert(newValue != null);

            if (ReferenceEquals(oldValue, newValue))
            {
                return 0;
            }

            int result;
            // PERF: Avoid additional hash set allocation by using overload which takes
            // a set argument instead of IEnumerable argument.
            var isSubset = oldValue.IsSubsetOfSet(newValue);

            if (isSubset &&
                oldValue.Count == newValue.Count)
            {
                // oldValue == newValue
                result = 0;
            }
            else if (isSubset)
            {
                // oldValue < newValue
                result = -1;
            }
            else
            {
                // oldValue > newValue
                result = 1;
            }

            return result;
        }

        public override ImmutableHashSet<T> Merge(ImmutableHashSet<T> value1, ImmutableHashSet<T> value2) => MergeOrIntersect(value1, value2, merge: true);

        public ImmutableHashSet<T> Intersect(ImmutableHashSet<T> value1, ImmutableHashSet<T> value2) => MergeOrIntersect(value1, value2, merge: false);

        private static ImmutableHashSet<T> MergeOrIntersect(ImmutableHashSet<T> value1, ImmutableHashSet<T> value2, bool merge)
        {
            Debug.Assert(value1 != null);
            Debug.Assert(value2 != null);

            if (value1.IsEmpty)
            {
                return value2;
            }
            else if (value2.IsEmpty || ReferenceEquals(value1, value2))
            {
                return value1;
            }

            // PERF: Avoid additional allocations by using the overload that takes a set argument
            // instead of IEnumerable argument.
            return merge ? value1.AddRange(value2) : value1.IntersectSet(value2);
        }
    }
}