namespace GenericCollections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Set collection with red-black tree realization
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.Generic.ISet{T}" />
    public class Set<T> : ISet<T>
    {
        #region Fields and prop

        /// <summary>
        /// The root
        /// </summary>
        private Node _root;
        /// <summary>
        /// The version
        /// </summary>
        private int _version;
        /// <summary>
        /// The comparer
        /// </summary>
        private readonly IComparer<T> _comparer;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly => false;
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public int Version { get; private set; }
        #endregion

        #region Ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="Set{T}"/> class.
        /// </summary>
        public Set()
        {
            _comparer = Comparer<T>.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Set{T}"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        /// <exception cref="ArgumentNullException">comparer</exception>
        public Set(IComparer<T> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException($"{nameof(comparer)} can't be null");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Set{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="comparer">The comparer.</param>
        /// <exception cref="ArgumentNullException">collection</exception>
        public Set(IEnumerable<T> collection, IComparer<T> comparer = null)
        {
            if (collection == null)
            {
                throw new ArgumentNullException($"{nameof(collection)} can't be null");
            }

            if (comparer == null)
            {
                _comparer = Comparer<T>.Default;
            }

            foreach (var item in collection)
            {
                AddMember(item);
            }
        }

        #endregion

        #region IEnumerable<T>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            _version = Version;
            return InOrderIEnumerator(_root);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        void ICollection<T>.Add(T item)
        {
            AddMember(item);
        }

        /// <summary>
        /// Modifies the current set so that it contains all elements that are present in the current set, in the specified collection, or in both.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException">other</exception>
        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException($"{nameof(other)} can't be null");
            }

            AddAll(other);
        }

        /// <summary>
        /// Unions the specified LHS.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// rhs
        /// or
        /// rhs
        /// </exception>
        public static Set<T> Union(Set<T> lhs, Set<T> rhs)
        {
            if (rhs == null)
            {
                throw new ArgumentNullException($"{nameof(rhs)} can't be null");
            }

            if (lhs == null)
            {
                throw new ArgumentNullException($"{nameof(rhs)} can't be null");
            }

            T[] result = new T[lhs.Count + rhs.Count];

            lhs.CopyTo(result, 0);
            rhs.CopyTo(result, lhs.Count);

            return new Set<T>(result);
        }
        /// <summary>
        /// Modifies the current set so that it contains only elements that are also in a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException">other</exception>
        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException($"{nameof(other)} can't be null");
            }

            if (Count == 0)
            {
                return;
            }

            List<T> results = new List<T>();

            foreach (var item in other)
            {
                if (Contains(item))
                {
                    results.Add(item);
                }
            }

            Clear();
            AddAll(results);
        }

        /// <summary>
        /// Intersects the specified LHS.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// rhs
        /// or
        /// rhs
        /// </exception>
        public static Set<T> Intersect(Set<T> lhs, Set<T> rhs)
        {
            if (rhs == null)
            {
                throw new ArgumentNullException($"{nameof(rhs)} can't be null");
            }

            if (lhs == null)
            {
                throw new ArgumentNullException($"{nameof(rhs)} can't be null");
            }

            if (lhs.Count == 0 || rhs.Count == 0)
            {
                return new Set<T>();
            }

            List<T> results = new List<T>();

            var greaterCollection = lhs.Count > rhs.Count ? lhs : rhs;
            var smallerCollection = lhs.Count < rhs.Count ? lhs : rhs;

            foreach (var item in smallerCollection)
            {
                if (greaterCollection.Contains(item))
                {
                    results.Add(item);
                }
            }

            return new Set<T>(results);
        }
        /// <summary>
        /// Removes all elements in the specified collection from the current set.
        /// </summary>
        /// <param name="other">The collection of items to remove from the set.</param>
        /// <exception cref="ArgumentNullException">other</exception>
        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException($"{nameof(other)} can't be null");
            }

            if (Count == 0)
            {
                return;
            }

            foreach (var item in other)
            {
                if (Contains(item))
                {
                    Remove(item);
                }
            }
        }

        /// <summary>
        /// Modifies the current set so that it contains only elements that are present either in the current set or in the specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <exception cref="ArgumentNullException">other</exception>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException($"{nameof(other)} can't be null");
            }

            if (Count == 0)
            {
                UnionWith(other);
                return;
            }

            var intersect = Intersect(this, new Set<T>(other));
            var otherSet = new Set<T>(other);

            foreach (var item in intersect)
            {
                if (Contains(item))
                {
                    Remove(item);
                    otherSet.Remove(item);
                }
            }

            foreach (var item in otherSet)
            {
                AddMember(item);
            }
        }

        /// <summary>
        /// Determines whether a set is a subset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a subset of <paramref name="other" />; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">other</exception>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException($"{nameof(other)} can't be null");
            }

            var otherSet = new Set<T>(other);

            if (otherSet.IsSupersetOf(this))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the current set is a superset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a superset of <paramref name="other" />; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">other</exception>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException($"{nameof(other)} can't be null");
            }

            foreach (var item in other)
            {
                if (!Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the current set is a proper (strict) superset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a proper superset of <paramref name="other" />; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether the current set is a proper (strict) subset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a proper subset of <paramref name="other" />; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether the current set overlaps with the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set and <paramref name="other" /> share at least one common element; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool Overlaps(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether the current set and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is equal to <paramref name="other" />; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">other</exception>
        public bool SetEquals(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException($"{nameof(other)} can't be null");
            }

            var otherSet = new Set<T>(other);

            if (otherSet.IsSupersetOf(this))
            {
                if (IsSupersetOf(otherSet))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds an element to the current set and returns a value to indicate if the element was successfully added.
        /// </summary>
        /// <param name="item">The element to add to the set.</param>
        /// <returns>
        ///   <see langword="true" /> if the element is added to the set; <see langword="false" /> if the element is already in the set.
        /// </returns>
        bool ISet<T>.Add(T item)
        {
            return AddMember(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            _root = null;
            Count = 0;
            ++Version;
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />.
        /// </returns>
        public bool Contains(T item)
        {
            return FindNode(item) != null;
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex, Count);
        }
        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <exception cref="ArgumentNullException">array</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// index
        /// or
        /// count
        /// </exception>
        /// <exception cref="ArgumentException">index</exception>
        public void CopyTo(T[] array, int index, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException($"{nameof(array)} can't be null");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(index)} must be greater than 0");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(count)} must be greater than 0");

            }

            if (index > array.Length || count > array.Length - index)
            {
                throw new ArgumentException($"{nameof(index)} is wrong index");
            }

            count += index;

            foreach (var item in InOrder(_root))
            {
                if (index >= count)
                {
                    return;
                }

                array[index++] = item;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public bool Remove(T item)
        {
            return RemoveMember(item);
        }

        #region Private members

        /// <summary>
        /// Removes the member.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private bool RemoveMember(T item)
        {
            if (_root == null)
            {
                return false;
            }

            ++Version;

            Node node1 = _root;
            Node node2 = null;
            Node node3 = null;
            Node match = null;
            Node parentOfMatch = null;
            bool flag = false;

            while (node1 != null)
            {
                if (Is2Node(node1))
                {
                    if (node2 == null)
                    {
                        node1.IsRed = true;
                    }
                    else
                    {
                        Node node4 = GetSibling(node1, node2);

                        if (node4.IsRed)
                        {
                            if (node2.Right == node4)
                            {
                                RotateLeft(node2);
                            }
                            else
                            {
                                RotateRight(node2);
                            }

                            node2.IsRed = true;
                            node4.IsRed = false;
                            ReplaceChildOfNodeOrRoot(node3, node2, node4);
                            node3 = node4;

                            if (node2 == match)
                            {
                                parentOfMatch = node4;
                            }

                            node4 = node2.Left == node1 ? node2.Right : node2.Left;
                        }

                        if (Is2Node(node4))
                        {
                            Merge2Nodes(node2, node1, node4);
                        }
                        else
                        {
                            TreeRotation treeRotation = RotationNeeded(node2, node1, node4);
                            Node newChild = null;

                            switch (treeRotation)
                            {
                                case TreeRotation.LeftRotation:
                                    node4.Right.IsRed = false;
                                    newChild = RotateLeft(node2);
                                    break;
                                case TreeRotation.RightRotation:
                                    node4.Left.IsRed = false;
                                    newChild = RotateRight(node2);
                                    break;
                                case TreeRotation.RightLeftRotation:
                                    newChild = RotateRightLeft(node2);
                                    break;
                                case TreeRotation.LeftRightRotation:
                                    newChild = RotateLeftRight(node2);
                                    break;
                            }

                            newChild.IsRed = node2.IsRed;
                            node2.IsRed = false;
                            node1.IsRed = true;
                            ReplaceChildOfNodeOrRoot(node3, node2, newChild);

                            if (node2 == match)
                            {
                                parentOfMatch = newChild;
                            }
                        }
                    }
                }

                var comparer = flag ? -1 : _comparer.Compare(item, node1.Item);

                if (comparer == 0)
                {
                    flag = true;
                    match = node1;
                    parentOfMatch = node2;
                }

                node3 = node2;
                node2 = node1;
                node1 = comparer >= 0 ? node1.Right : node1.Left;
            }

            if (match != null)
            {
                ReplaceNode(match, parentOfMatch, node2, node3);
                --Count;
            }

            if (_root != null)
            {
                _root.IsRed = false;
            }

            return flag;
        }
        /// <summary>
        /// Adds the member.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private bool AddMember(T item)
        {
            if (_root == null)
            {
                _root = new Node(item, false);
                Count = 1;
                ++Version;

                return true;
            }

            Node node = _root;
            Node parent = null;
            Node grandParent = null;
            Node greatGrandParent = null;

            ++Version;
            int comparer = 0;

            while (node != null)
            {
                comparer = _comparer.Compare(item, node.Item);

                if (comparer == 0)
                {
                    _root.IsRed = false;
                    return false;
                }

                if (Is4Node(node))
                {
                    Split4Node(node);

                    if (IsRed(parent))
                    {
                        InsertionBalance(node, ref parent, grandParent, greatGrandParent);
                    }
                }

                greatGrandParent = grandParent;
                grandParent = parent;
                parent = node;
                node = comparer < 0 ? node.Left : node.Right;
            }

            Node current = new Node(item);

            if (comparer > 0)
            {
                parent.Right = current;
            }

            else
            {
                parent.Left = current;
            }

            if (parent.IsRed)
            {
                InsertionBalance(current, ref parent, grandParent, greatGrandParent);
            }

            _root.IsRed = false;
            ++Count;

            return true;
        }

        /// <summary>
        /// Adds all.
        /// </summary>
        /// <param name="collection">The collection.</param>
        private void AddAll(IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                if (!Contains(item))
                {
                    AddMember(item);
                }
            }
        }
        /// <summary>
        /// Finds the node.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private Node FindNode(T item)
        {
            var node = _root;

            while (node != null)
            {
                var counter = _comparer.Compare(item, node.Item);

                if (counter == 0)
                {
                    return node;
                }

                node = counter < 0 ? node.Left : node.Right;
            }

            return null;
        }

        /// <summary>
        /// Determines whether [is null or black] [the specified node].
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        ///   <c>true</c> if [is null or black] [the specified node]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsNullOrBlack(Node node)
        {
            if (node != null)
            {
                return !node.IsRed;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified node is black.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        ///   <c>true</c> if the specified node is black; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsBlack(Node node)
        {
            if (node != null)
            {
                return !node.IsRed;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified node is red.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        ///   <c>true</c> if the specified node is red; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsRed(Node node)
        {
            return node != null && node.IsRed;
        }
        /// <summary>
        /// Is2s the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static bool Is2Node(Node node)
        {
            if (IsBlack(node) && IsNullOrBlack(node.Left))
            {
                return IsNullOrBlack(node.Right);
            }

            return false;
        }

        /// <summary>
        /// Is4s the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static bool Is4Node(Node node)
        {
            return IsRed(node.Left) && IsRed(node.Right);
        }
        /// <summary>
        /// Split4s the node.
        /// </summary>
        /// <param name="node">The node.</param>
        private static void Split4Node(Node node)
        {
            node.IsRed = true;
            node.Left.IsRed = false;
            node.Right.IsRed = false;
        }
        /// <summary>
        /// Merge2s the nodes.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="child1">The child1.</param>
        /// <param name="child2">The child2.</param>
        private static void Merge2Nodes(Node parent, Node child1, Node child2)
        {
            parent.IsRed = false;
            child1.IsRed = true;
            child2.IsRed = true;
        }

        /// <summary>
        /// Insertions the balance.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="grandParent">The grand parent.</param>
        /// <param name="greatGrandParent">The great grand parent.</param>
        private void InsertionBalance(Node current, ref Node parent, Node grandParent, Node greatGrandParent)
        {
            bool flag1 = grandParent.Right == parent;
            bool flag2 = parent.Right == current;

            Node newChild;

            if (flag1 == flag2)
            {
                newChild = flag2 ? RotateLeft(grandParent) : RotateRight(grandParent);
            }
            else
            {
                newChild = flag2 ? RotateLeftRight(grandParent) : RotateRightLeft(grandParent);
                parent = greatGrandParent;
            }

            grandParent.IsRed = true;
            newChild.IsRed = false;
            ReplaceChildOfNodeOrRoot(greatGrandParent, grandParent, newChild);

        }

        /// <summary>
        /// Replaces the child of node or root.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="child">The child.</param>
        /// <param name="newChild">The new child.</param>
        private void ReplaceChildOfNodeOrRoot(Node parent, Node child, Node newChild)
        {
            if (parent != null)
            {
                if (parent.Left == child)
                {
                    parent.Left = newChild;
                }
                else
                {
                    parent.Right = newChild;
                }
            }
            else
            {
                _root = newChild;
            }
        }

        /// <summary>
        /// Replaces the node.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <param name="parentOfMatch">The parent of match.</param>
        /// <param name="succesor">The succesor.</param>
        /// <param name="parentOfSuccesor">The parent of succesor.</param>
        private void ReplaceNode(Node match, Node parentOfMatch, Node succesor, Node parentOfSuccesor)
        {
            if (succesor == match)
            {
                succesor = match.Left;
            }
            else
            {
                if (succesor.Right != null)
                {
                    succesor.Right.IsRed = false;
                }

                if (parentOfSuccesor != match)
                {
                    parentOfSuccesor.Left = succesor.Right;
                    succesor.Right = match.Right;
                }
                succesor.Left = match.Left;
            }

            if (succesor != null)
            {
                succesor.IsRed = match.IsRed;
            }

            ReplaceChildOfNodeOrRoot(parentOfMatch, match, succesor);
        }

        /// <summary>
        /// Gets the sibling.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        private static Node GetSibling(Node node, Node parent)
        {
            if (parent.Left == node)
            {
                return parent.Right;
            }

            return parent.Left;
        }
        /// <summary>
        /// Rotates the left.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static Node RotateLeft(Node node)
        {
            Node right = node.Right;

            node.Right = right.Left;
            right.Left = node;

            return right;
        }

        /// <summary>
        /// Rotates the left right.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static Node RotateLeftRight(Node node)
        {
            Node left = node.Left;
            Node right = left.Right;

            node.Left = right.Right;
            right.Right = node;
            left.Right = right.Left;
            right.Left = left;

            return right;
        }

        /// <summary>
        /// Rotates the right.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static Node RotateRight(Node node)
        {
            Node left = node.Left;

            node.Left = left.Right;
            left.Right = node;

            return left;
        }

        /// <summary>
        /// Rotates the right left.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static Node RotateRightLeft(Node node)
        {
            Node right = node.Right;
            Node left = right.Left;

            node.Right = left.Left;
            left.Left = node;
            right.Left = left.Right;
            left.Right = right;

            return left;
        }
        /// <summary>
        /// Rotations the needed.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="current">The current.</param>
        /// <param name="sibling">The sibling.</param>
        /// <returns></returns>
        private static TreeRotation RotationNeeded(Node parent, Node current, Node sibling)
        {
            if (IsRed(sibling.Left))
            {
                return parent.Left == current ? TreeRotation.RightLeftRotation : TreeRotation.RightRotation;
            }

            return parent.Left == current ? TreeRotation.LeftRotation : TreeRotation.LeftRightRotation;
        }

        /// <summary>
        /// Ins the order i enumerator.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private IEnumerator<T> InOrderIEnumerator(Node node)
        {
            if (_version != Version)
            {
                throw new InvalidOperationException($"Collection can't be change when enumerating");
            }

            return InOrder(_root).GetEnumerator();
        }
        /// <summary>
        /// Ins the order.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private IEnumerable<T> InOrder(Node node)
        {
            while (true)
            {
                if (node == null)
                {
                    yield break;
                }

                if (node.Left != null)
                {
                    foreach (var item in InOrder(node.Left))
                    {
                        yield return item;
                    }
                }

                yield return node.Item;

                if (node.Right == null) yield break;
                {
                    node = node.Right;
                }
            }
        }
        #endregion

        #region Internal members

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="System.Collections.Generic.ISet{T}" />
        internal class Node
        {
            /// <summary>
            /// The item
            /// </summary>
            public T Item;
            /// <summary>
            /// The left
            /// </summary>
            public Node Left;
            /// <summary>
            /// The right
            /// </summary>
            public Node Right;
            /// <summary>
            /// Gets or sets a value indicating whether this instance is red.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is red; otherwise, <c>false</c>.
            /// </value>
            public bool IsRed { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> class.
            /// </summary>
            /// <param name="item">The item.</param>
            public Node(T item)
            {
                Item = item;
                IsRed = true;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> class.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="isRed">if set to <c>true</c> [is red].</param>
            public Node(T item, bool isRed)
            {
                Item = item;
                IsRed = isRed;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        internal enum TreeRotation
        {
            /// <summary>
            /// The left rotation
            /// </summary>
            LeftRotation = 1,
            /// <summary>
            /// The right rotation
            /// </summary>
            RightRotation = 2,
            /// <summary>
            /// The right left rotation
            /// </summary>
            RightLeftRotation = 3,
            /// <summary>
            /// The left right rotation
            /// </summary>
            LeftRightRotation = 4,
        }

        #endregion
    }
}