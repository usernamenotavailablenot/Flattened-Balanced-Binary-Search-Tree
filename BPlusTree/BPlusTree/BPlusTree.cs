namespace BTree
{
    internal enum DeleteAction
    {
        None,
        MergeIntoPre,
        MergeFromNext,
        BorrowFromPre,
        BorrowFromNext,
    }
    internal abstract class BTreeNode<K> where K : IComparable
    {
        private ushort _len;
        public K?[] IndexValues;
        public int Len 
        {
            get { return _len; }
            set
            {
                _len = (ushort)value;
            }
        }
        public BTreeNode<K>? Pre = null;
        public BTreeNode<K>? Next = null;
        public BTreeIndexNode<K>? Parent = null;
        public int ParentIndex = 0;
        public BTreeNode(int max)
        {
            IndexValues = new K?[max];
        }
        public virtual void Shrink(int len)
        {
            for (int i = len; i < Len; ++i)
            {
                IndexValues[i] = default;
            }
            Len = len;
        }
    }
    internal class BTreeIndexNode<K> : BTreeNode<K> where K : IComparable
    {
        public BTreeNode<K>?[] Indexes;
        public BTreeIndexNode(int max) : base(max)
        {
            Indexes = new BTreeNode<K>?[max];
        }
        public override void Shrink(int len)
        {
            for (int i = len; i < Len; ++i)
            {
                Indexes[i] = default;
            }
            base.Shrink(len);
        }
    }
    internal class BTreeLeafNode<K, V> : BTreeNode<K> where K : IComparable
    {
        public V?[] Values;
        public BTreeLeafNode(int max) : base(max)
        {
            Values = new V?[max];
        }
        public override void Shrink(int len)
        {
            for (int i = len; i < Len; ++i)
            {
                Values[i] = default;
            }
            base.Shrink(len);
        }
    }
    public class BPlusTree<K, V> where K : IComparable, new()
    {
        private const int LEAST = 2;
        private readonly K SENTINEL = Activator.CreateInstance<K>();
        private readonly int MIN;
        private readonly int MAX;
        private readonly int CUTOFF_BORROW_MERGE;
        private readonly int HALF;
        private BTreeLeafNode<K, V> _head;
        private BTreeLeafNode<K, V> _tail;
        private BTreeNode<K> _root;

        public V? Inserted;
        public V? Deleted;
        public BPlusTree(int min)
        {
            MIN = min < LEAST ? LEAST : (min > 4096 ? 4096 : min); // at least 2, such that upper level guaranteed to be at most half
            MAX = 4 * MIN;
            CUTOFF_BORROW_MERGE = 2 * MIN; // merge or borrow upon deletion
            HALF = 2 * MIN;
            _head = new BTreeLeafNode<K, V>(MAX);
            _head.ParentIndex = 0;
            _head.IndexValues[0] = SENTINEL;
            _head.Len = 1;
            _root = _head;
            _tail = _head;
        }
        public void Add(K key, V val)
        {
            BTreeNode<K> node = _root;
            Inserted = val;
            Deleted = default;
            // go to leaf
            while (node is BTreeIndexNode<K> nd)
            {
                int i = FindIndex(node, key, out _);
                node = nd.Indexes[i]!;
            }
            // at leaf
            var leafNode = (BTreeLeafNode<K, V>)node;
            bool found;
            int indexAfter = FindIndex(leafNode, key, out found);
            if (found) // update
            {
                Deleted = leafNode.Values[indexAfter];
                leafNode.Values[indexAfter] = val;
                return;
            }
            // insert
            if (leafNode.Len < MAX) // not full, shift, insert, and done
            {
                for (int i = leafNode.Len; i > indexAfter + 1; --i)
                {
                    leafNode.Values[i] = leafNode.Values[i - 1];
                    leafNode.IndexValues[i] = leafNode.IndexValues[i - 1];
                }
                leafNode.Values[indexAfter + 1] = val;
                leafNode.IndexValues[indexAfter + 1] = key;
                leafNode.Len += 1;
                return;
            }

            // full, split
            BTreeNode<K>? newNode = SplitLeafNode(leafNode, indexAfter + 1, val, key);

            // move up
            while (newNode?.Parent != null)
            {
                BTreeIndexNode<K> nd = newNode.Parent!;
                int idx = newNode.ParentIndex;
                if (nd.Len < MAX) // not full, insert
                {
                    for (int i = nd.Len; i > idx; --i)
                    {
                        nd.IndexValues[i] = nd.IndexValues[i - 1];
                        nd.Indexes[i] = nd.Indexes[i - 1];
                        nd.Indexes[i]!.ParentIndex = i;
                    }
                    nd.IndexValues[idx] = newNode.IndexValues[0];
                    nd.Indexes[idx] = newNode;
                    nd.Len += 1;
                    newNode = null;
                    break;
                }
                else // full split
                {
                    newNode = SplitIndexNode(nd, idx, newNode, newNode.IndexValues[0]!);
                }
            }
            // new root
            if (newNode != null)
            {
                this._root = CreateNewRoot(newNode);
            }
        }
        public void Remove(K key)
        {
            BTreeNode<K> node = _root;
            Inserted = default;
            Deleted = default;
            // go to leaf
            while (node is BTreeIndexNode<K> nd)
            {
                int i = FindIndex(node, key, out _);
                node = nd.Indexes[i]!;
            }
            // at leaf
            var leafNode = (BTreeLeafNode<K, V>)node;
            bool found;
            int indexAt = FindIndex(leafNode!, key, out found);
            if (!found)
            {
                return; // key not exists, do nothing
            }
            // delete
            // Hold a pointer to the Value to be deleted for Key/Value pair
            Deleted = leafNode.Values[indexAt]!;
            for (int i = indexAt; i < leafNode.Len; ++i)
            {
                leafNode.Values[i] = (i + 1 < MAX) ? leafNode.Values[i + 1] : default;
                leafNode.IndexValues[i] = (i + 1 < MAX) ? leafNode.IndexValues[i + 1] : default;
                if (i == leafNode.Len - 1)
                {
                    break;
                }
            }

            leafNode.Shrink(leafNode.Len - 1);
            int shift;
            DeleteAction dAction = GetDeleteAction(leafNode, indexAt, out shift);
            BTreeNode<K> childNode = TakeDeleteAction(leafNode, dAction, shift);
            if (childNode == _tail
                && (dAction == DeleteAction.MergeIntoPre || dAction == DeleteAction.MergeFromNext))
            {
                _tail = (BTreeLeafNode<K, V>)(_tail.Pre!);
            }
            while (childNode.Parent != null && dAction != DeleteAction.None)
            {
                BTreeIndexNode<K> nd = childNode.Parent;
                int idx = childNode.ParentIndex;
                switch (dAction)
                {
                    case DeleteAction.BorrowFromPre:
                    case DeleteAction.BorrowFromNext:
                        nd.IndexValues[idx] = childNode.IndexValues[0];
                        if (idx == 0)
                        {
                            dAction = DeleteAction.BorrowFromPre;
                        }
                        else
                        {
                            dAction = DeleteAction.None;
                        }
                        childNode = nd;
                        break;
                    case DeleteAction.MergeIntoPre:
                    case DeleteAction.MergeFromNext:
                        for (int i = idx; i < nd.Len; ++i) // delete this index
                        {
                            nd.IndexValues[i] = (i + 1 < MAX) ? nd.IndexValues[i + 1] : default;
                            nd.Indexes[i] = (i + 1 < MAX) ? nd.Indexes[i + 1] : default;
                            if (nd.Indexes[i] != null)
                            {
                                nd.Indexes[i]!.ParentIndex = i;
                            }
                            if (i == nd.Len - 1)
                            {
                                break;
                            }
                        }
                        nd.Shrink(nd.Len - 1);
                        dAction = GetDeleteAction(nd, idx, out shift);
                        childNode = TakeDeleteAction(nd, dAction, shift);
                        break;
                    default:
                        throw new Exception("Unsupported delete action when Delete."); // never
                }
            }
            if (_root.Len == 1 && _root is BTreeIndexNode<K> tmp) // empty root
            {
                _root = tmp.Indexes[0]!;
                _root.Parent = null;
                tmp.Indexes[0] = null;
            }
        }
        public bool HasKey(K key)
        {
            bool found;
            GetLeafNode(key, out found, out _);
            return found;
        }
        private DeleteAction GetDeleteAction(BTreeNode<K> node, int indexAt, out int shift) // merge or borrow
        {
            if (node.Pre == null && node.Next == null)
            {
                shift = 0;
                return DeleteAction.None;
            }
            if (node.Len >= MIN)
            {
                shift = 0;
                if (indexAt == 0)
                {
                    return DeleteAction.BorrowFromPre; // update index
                }
                return DeleteAction.None;
            }
            if (node.Pre == null) // The node contains sentinel. The 0th can not be deleted.
            {
                int total = node.Len + node.Next!.Len;
                if (total % 2 == 1)
                {
                    total += 1;
                }
                if (node.Next!.Len > CUTOFF_BORROW_MERGE)
                {
                    shift = total / 2 - node.Len;
                    return DeleteAction.BorrowFromNext;
                }
                else
                {
                    shift = 0;
                    return DeleteAction.MergeFromNext;
                }
            }
            // Only deal with predecessor. If deal with next and 0th is deleted, then we need to handle two nodes.
            if (node.Pre.Len > CUTOFF_BORROW_MERGE)
            {
                int total = node.Len + node.Pre!.Len;
                shift = total / 2 - node.Len;
                return DeleteAction.BorrowFromPre;
            }
            else
            {
                shift = 0;
                return DeleteAction.MergeIntoPre;
            }
        }
        private BTreeNode<K> TakeDeleteAction(BTreeNode<K> node, DeleteAction dAction, int shift)
        {
            switch (dAction)
            {
                case DeleteAction.None:
                    return node;
                case DeleteAction.BorrowFromPre:
                    {
                        RebalancePreToNextDelete(node.Pre!.IndexValues, node.IndexValues, node.Pre!.Len, node.Len, shift);
                        if (node is BTreeLeafNode<K, V> leaf)
                        {
                            BTreeLeafNode<K, V> pre = (leaf.Pre as BTreeLeafNode<K, V>)!;
                            RebalancePreToNextDelete(pre.Values, leaf.Values, pre.Len, leaf.Len, shift);
                        }
                        else if (node is BTreeIndexNode<K> nd)
                        {
                            BTreeIndexNode<K> pre = (nd.Pre as BTreeIndexNode<K>)!;
                            RebalancePreToNextDelete(pre.Indexes, nd.Indexes, pre.Len, nd.Len, shift);
                            for (int i = 0; i < nd.Len + shift; ++i)
                            {
                                nd.Indexes[i]!.Parent = nd;
                                nd.Indexes[i]!.ParentIndex = i;
                            }
                        }
                        node.Len += shift;
                        node.Pre!.Shrink(node.Pre!.Len - shift);
                        return node;
                    }
                case DeleteAction.BorrowFromNext: // only for the node containing setinel
                    {
                        RebalanceNextToPreDelete(node.IndexValues, node.Next!.IndexValues, node.Len, node.Next!.Len, shift);
                        if (node is BTreeLeafNode<K, V> leaf)
                        {
                            BTreeLeafNode<K, V> next = (leaf.Next as BTreeLeafNode<K, V>)!;
                            RebalanceNextToPreDelete(leaf.Values, next.Values, leaf.Len, next.Len, shift);
                        }
                        else if (node is BTreeIndexNode<K> nd)
                        {
                            BTreeIndexNode<K> next = (nd.Next as BTreeIndexNode<K>)!;
                            RebalanceNextToPreDelete(nd.Indexes, next.Indexes, nd.Len, next.Len, shift);
                            for (int i = node.Len; i < node.Len + shift; ++i)
                            {
                                nd.Indexes[i]!.Parent = nd;
                                nd.Indexes[i]!.ParentIndex = i;
                            }
                            for (int i = 0; i < next.Len - shift; ++i)
                            {
                                next.Indexes[i]!.ParentIndex = i;
                            }
                        }
                        node.Len += shift;
                        node.Next!.Shrink(node.Next!.Len - shift);
                        return node.Next; // to be modified
                    }
                case DeleteAction.MergeIntoPre:
                    {
                        int totalLen = node.Pre!.Len + node.Len;
                        ArrayMergeIntoPre(node.Pre!.IndexValues, node.IndexValues, node.Pre!.Len, node.Len);
                        if (node is BTreeLeafNode<K, V> leaf)
                        {
                            BTreeLeafNode<K, V> pre = (leaf.Pre as BTreeLeafNode<K, V>)!;
                            ArrayMergeIntoPre(pre.Values, leaf.Values, pre.Len, leaf.Len);
                        }
                        else if (node is BTreeIndexNode<K> nd)
                        {
                            BTreeIndexNode<K> pre = (nd.Pre as BTreeIndexNode<K>)!;
                            ArrayMergeIntoPre(pre.Indexes, nd.Indexes, pre.Len, nd.Len);
                            for (int i = 0; i < node.Len; ++i)
                            {
                                nd.Indexes[i]!.Parent = pre;
                                nd.Indexes[i]!.ParentIndex = pre.Len + i;
                            }
                        }
                        node.Pre!.Len = totalLen;
                        node.Pre.Next = node.Next;
                        if (node.Next != null)
                        {
                            node.Next.Pre = node.Pre;
                        }
                        return node; // to be deleted
                    }
                case DeleteAction.MergeFromNext: // only for the node containing setinel
                    {
                        int totalLen = node.Len + node.Next!.Len;
                        ArrayMergeIntoPre(node.IndexValues, node.Next!.IndexValues, node.Len, node.Next!.Len);
                        if (node is BTreeLeafNode<K, V> leaf)
                        {
                            BTreeLeafNode<K, V> next = (leaf.Next as BTreeLeafNode<K, V>)!;
                            ArrayMergeIntoPre(leaf.Values, next.Values, leaf.Len, next.Len);
                        }
                        else if (node is BTreeIndexNode<K> nd)
                        {
                            BTreeIndexNode<K> next = (nd.Next as BTreeIndexNode<K>)!;
                            ArrayMergeIntoPre(nd.Indexes, next.Indexes, nd.Len, next.Len);
                            for (int i = 0; i < next.Len; ++i)
                            {
                                next.Indexes[i]!.Parent = nd;
                                next.Indexes[i]!.ParentIndex = nd.Len + i;
                            }
                        }
                        node.Len = totalLen;
                        var tmp = node.Next;
                        node.Next = tmp.Next;
                        if (tmp.Next != null)
                        {
                            tmp.Next.Pre = node;
                        }
                        return tmp; // to be deleted
                    }
                default:
                    throw new Exception("Unsupported delete action when TakeDeleteAction."); // never
            }
        }
        /// <summary>
        /// should insert key after the index, 
        /// find the greatest index whose index value is less than or equal the key using binary search
        /// </summary>
        private int FindIndex(BTreeNode<K> node, K key, out bool found)
        {
            if (node.Pre == null && node.Len == 1)
            {
                // empty
                found = false;
                return 0;
            }
            if (node.Pre == null && key.CompareTo(node.IndexValues[1]) < 0)
            {
                // less than least
                found = false;
                return 0;
            }
            int lo = 0;
            if (node.Pre == null) // node with sentinel
            {
                lo = 1;
            }
            int hi = node.Len - 1;
            if (key.CompareTo(node.IndexValues[hi]) > 0)
            {
                // greater than greatest
                found = false;
                return hi;
            }
            int mid = (lo + hi) / 2;
            int comp = key.CompareTo(node.IndexValues[mid]);
            while (true)
            {
                if (comp >= 0 && (mid + 1 >= node.Len || key.CompareTo(node.IndexValues[mid + 1]) < 0))
                {
                    break;
                }
                if (comp < 0)
                {
                    hi = mid - 1;
                }
                else
                {
                    lo = mid + 1;
                }
                mid = (lo + hi) / 2;
                comp = key.CompareTo(node.IndexValues[mid]);
            }
            found = comp == 0;
            return mid;
        }
        /// <summary>
        /// insert at idxAt at old, the original idxAt at old shift right
        /// </summary>
        private void RebalanceArraysSplit<T>(T[] oldArr, T[] newArr, int idxAt, T val)
        {
            if (idxAt <= HALF) // in the old array
            {
                for (int i = HALF, j = 0; i < MAX; ++i, ++j)
                {
                    newArr[j] = oldArr[i];
                }
                // shift old node and insert
                for (int i = HALF; i > idxAt; --i)
                {
                    oldArr[i] = oldArr[i - 1];
                }
                oldArr[idxAt] = val;
            }
            else // in the new array
            {
                int j = 0, i = HALF + 1;
                for (; i < idxAt; ++i, ++j)
                {
                    newArr[j] = oldArr[i];
                }
                newArr[j] = val;
                ++j;
                for (; i < MAX; ++i, ++j)
                {
                    newArr[j] = oldArr[i];
                }
            }
        }
        private void RebalanceNextToPreDelete<T>(T[] pre, T[] next, int preLen, int nextLen, int offset)
        {
            for (int i = preLen, j = 0; j < offset; ++i, ++j)
            {
                pre[i] = next[j];
            }
            ArrayShiftLeft(next, nextLen, offset);
        }
        private void RebalancePreToNextDelete<T>(T[] pre, T[] next, int preLen, int nextLen, int offset)
        {

            ArrayShiftRight(next, nextLen, offset);
            for (int i = preLen - offset, j = 0; j < offset; ++i, ++j)
            {
                next[j] = pre[i];
            }
        }
        private void ArrayShiftLeft<T>(T[] arr, int curLen, int shiftLeft)
        {
            for (int i = 0; i < curLen - shiftLeft; ++i)
            {
                arr[i] = arr[i + shiftLeft];
            }
        }
        private void ArrayShiftRight<T>(T[] arr, int curLen, int shiftRight)
        {
            for (int i = curLen - 1; i >= 0; --i)
            {
                arr[i + shiftRight] = arr[i];
            }
        }
        private void ArrayMergeIntoPre<T>(T[] pre, T[] next, int preLen, int nextLen)
        {
            for (int i = preLen, j = 0; j < nextLen; ++i, ++j)
            {
                pre[i] = next[j];
            }
        }
        private BTreeNode<K> CreateNewNode(BTreeNode<K> node) // new node after existing node
        {
            BTreeNode<K> newNode = (Activator.CreateInstance(node.GetType(), MAX) as BTreeNode<K>)!;
            newNode.Pre = node;
            newNode.Next = node.Next;
            node.Next = newNode;
            if (newNode.Next != null)
            {
                newNode.Next.Pre = newNode;
            }
            newNode.Parent = node.Parent;
            newNode.ParentIndex = node.ParentIndex + 1;
            return newNode;
        }
        private BTreeNode<K> CreateNewRoot(BTreeNode<K> newNode)
        {
            BTreeIndexNode<K> newRoot = new BTreeIndexNode<K>(MAX);
            newRoot.Len = 2;
            newRoot.IndexValues[0] = newNode.Pre!.IndexValues[0];
            newRoot.IndexValues[1] = newNode.IndexValues[0];
            newRoot.Indexes[0] = newNode.Pre!;
            newRoot.Indexes[1] = newNode;
            newRoot.Indexes[0]!.Parent = newRoot;
            newRoot.Indexes[1]!.Parent = newRoot;
            newRoot.Indexes[0]!.ParentIndex = 0;
            newRoot.Indexes[1]!.ParentIndex = 1;
            return newRoot;
        }

        // split node, new node after existing node
        private BTreeLeafNode<K, V> SplitLeafNode(BTreeLeafNode<K, V> node, int idxAt, V value, K indexValue)
        {
            BTreeLeafNode<K, V> newNode = (BTreeLeafNode<K, V>)CreateNewNode(node);
            RebalanceArraysSplit<K>(node.IndexValues!, newNode.IndexValues!, idxAt, indexValue);
            var newNd = (newNode as BTreeLeafNode<K, V>)!;
            RebalanceArraysSplit<V>(node.Values!, newNd.Values!, idxAt, value);
            node.Shrink(HALF + 1);
            newNode.Len = HALF;
            if (node == this._tail)
            {
                this._tail = newNode;
            }
            return newNode;
        }
        private BTreeNode<K> SplitIndexNode(BTreeIndexNode<K> node, int idxAt, BTreeNode<K> child, K indexValue)
        {
            BTreeIndexNode<K> newNode = (BTreeIndexNode<K>)CreateNewNode(node);
            RebalanceArraysSplit(node.IndexValues!, newNode.IndexValues!, idxAt, indexValue);
            RebalanceArraysSplit(node.Indexes!, newNode.Indexes!, idxAt, child);
            node.Shrink(HALF + 1);
            newNode.Len = HALF;
            for (int i = 0; i < newNode.Len; ++i)
            {
                newNode.Indexes[i]!.Parent = newNode;
                newNode.Indexes[i]!.ParentIndex = i;
            }
            for (int i = idxAt + 1; i < node.Len; ++i)
            {
                node.Indexes[i]!.ParentIndex = i;
            }
            return newNode;
        }
        private BTreeNode<K> GetLeafNode(K indexValue, out bool found, out int indexAfter)
        {
            BTreeNode<K> node = _root;
            while (node is BTreeIndexNode<K> nd)
            {
                int i = FindIndex(node, indexValue, out _);
                node = nd.Indexes[i]!;
            }
            var leafNode = node;
            indexAfter = FindIndex(leafNode!, indexValue, out found);
            return leafNode;
        }
    }
}
