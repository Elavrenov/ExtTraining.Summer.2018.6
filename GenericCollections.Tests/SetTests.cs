namespace GenericCollections.Tests
{
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class SetTests
    {
        private ICollection<int> set = new Set<int>();
        private ICollection<int> forOperationsSet = new Set<int>();


        [Test]
        public void SetCtorTest()
        {
            int[] collection = new int[] { 1, 2, 3, 4 };
            Set<int> newSet = new Set<int>(collection);

            int[] expected = new[] { 1, 2, 3, 4 };

            CompareItems(expected, newSet);
        }

        [TestCase(3)]
        public void ContainsClearTest(int checkedItem)
        {
            set.Add(5);
            set.Add(3);
            set.Add(65);

            if (set.Contains(checkedItem))
            {
                set.Clear();
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public void AddTestWithInOrderEnumerator()
        {
            set.Add(4);
            set.Add(43);
            set.Add(31);
            set.Add(-6);
            set.Add(0);

            int[] expected = new[] { -6, 0, 4, 31, 43 };

            CompareItems(expected, set);

            set.Clear();
        }

        [Test]
        public void RemoveTestWithInOrderEnumerator()
        {
            set.Add(4);
            set.Add(43);
            set.Add(31);
            set.Add(-6);
            set.Add(0);
            set.Remove(0);

            int[] expected = new[] { -6, 4, 31, 43 };

            CompareItems(expected, set);

            set.Clear();
        }

        [Test]
        public void CopyToTest()
        {
            set.Add(4);
            set.Add(43);
            set.Add(31);
            set.Add(-6);

            int[] copyArray = new int[4];

            set.CopyTo(copyArray, 0);

            int[] expected = new[] { -6, 4, 31, 43 };

            CompareItems(expected, copyArray);
        }

        [Test]
        public void UnionWithTest()
        {
            set.Add(4);
            set.Add(43);
            set.Add(31);
            set.Add(-6);

            forOperationsSet.Add(4);
            forOperationsSet.Add(2);
            forOperationsSet.Add(0);

            var newSet = set as Set<int>;

            newSet.UnionWith(forOperationsSet);

            int[] expected = new[] { -6, 0, 2, 4, 31, 43 };

            CompareItems(expected, newSet);
        }

        [Test]
        public void UnionTest()
        {
            set.Add(4);
            set.Add(43);
            set.Add(31);
            set.Add(-6);

            forOperationsSet.Add(4);
            forOperationsSet.Add(2);
            forOperationsSet.Add(0);

            Set<int> newSet = Set<int>.Union((Set<int>)set, (Set<int>)forOperationsSet);

            int[] expected = new[] { -6, 0, 2, 4, 31, 43 };

            CompareItems(expected, newSet);
        }

        [Test]
        public void IntersectWithTest()
        {
            set.Add(4);
            set.Add(43);
            set.Add(31);
            set.Add(-6);
            set.Add(0);

            forOperationsSet.Add(4);
            forOperationsSet.Add(2);
            forOperationsSet.Add(0);

            var newSet = set as Set<int>;

            newSet.IntersectWith(forOperationsSet);

            int[] expected = new[] { 0, 4 };

            CompareItems(expected, newSet);
        }

        [Test]
        public void IntersectTest1()
        {
            set.Add(4);
            set.Add(43);
            set.Add(31);
            set.Add(-6);
            set.Add(0);

            forOperationsSet.Add(4);
            forOperationsSet.Add(2);
            forOperationsSet.Add(0);

            Set<int> newSet = Set<int>.Intersect((Set<int>)set, (Set<int>)forOperationsSet);

            int[] expected = new[] { 0, 4 };

            CompareItems(expected, newSet);
        }
        [Test]
        public void IntersectTest2()
        {
            set.Add(4);
            set.Add(43);
            set.Add(31);
            set.Add(-6);
            set.Add(0);

            forOperationsSet.Add(4);
            forOperationsSet.Add(2);
            forOperationsSet.Add(0);

            Set<int> newSet = Set<int>.Intersect((Set<int>)forOperationsSet, (Set<int>)set);

            int[] expected = new[] { 0, 4 };

            CompareItems(expected, newSet);
        }

        [Test]
        public void ExceptWithTest()
        {
            set.Add(4);
            set.Add(43);
            set.Add(31);
            set.Add(-6);
            set.Add(0);

            forOperationsSet.Add(4);
            forOperationsSet.Add(2);
            forOperationsSet.Add(0);

            var newSet = set as Set<int>;

            newSet.ExceptWith(forOperationsSet);

            int[] expected = new[] { -6, 31, 43 };

            CompareItems(expected, newSet);
        }

        [Test]
        public void SymmetricExceptWithTest()
        {
            set.Add(4);
            set.Add(43);
            set.Add(31);
            set.Add(-6);
            set.Add(0);

            forOperationsSet.Add(4);
            forOperationsSet.Add(2);
            forOperationsSet.Add(0);

            var newSet = set as Set<int>;

            newSet.SymmetricExceptWith(forOperationsSet);

            int[] expected = new[] { -6, 2, 31, 43 };

            CompareItems(expected, newSet);
        }

        [Test]
        public void IsSupersetOfTest()
        {
            set.Clear();
            forOperationsSet.Clear();

            set.Add(4);
            set.Add(43);
            set.Add(31);
            set.Add(-6);
            set.Add(0);

            forOperationsSet.Add(4);
            forOperationsSet.Add(0);

            var newSet = set as Set<int>;

            if (newSet.IsSupersetOf(forOperationsSet))
            {
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public void IsSubsetOfTest()
        {
            set.Clear();
            forOperationsSet.Clear();

            set.Add(4);
            set.Add(43);
            set.Add(31);
            set.Add(-6);
            set.Add(0);

            forOperationsSet.Add(4);
            forOperationsSet.Add(0);

            var newSet = forOperationsSet as Set<int>;

            if (newSet.IsSubsetOf(set))
            {
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public void SetEqualsTest()
        {
            set.Clear();
            forOperationsSet.Clear();

            set.Add(4);
            set.Add(0);

            forOperationsSet.Add(0);
            forOperationsSet.Add(4);

            var newSet = forOperationsSet as Set<int>;

            if (newSet.SetEquals(set))
            {
                Assert.Pass();
            }

            Assert.Fail();
        }
        private void CompareItems(int[] expected, IEnumerable<int> collection)
        {
            int i = -1;

            foreach (var item in collection)
            {
                Assert.AreEqual(expected[++i], item);
            }
        }
    }
}
