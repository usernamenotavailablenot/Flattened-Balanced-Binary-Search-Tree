namespace FBBST
{
    public class FBBSTree<K, V> where K : IComparable
    {
        private class FBBSTreeNode
        {
            private short _len; // one bit for color, other 15 bits for length
            public K[] Keys;
            public V[] Values;
            public FBBSTreeNode? Left;
            public FBBSTreeNode? Right;
            public bool IsRed
            {
                get => _len < 0;
                set
                {
                    if (value)
                    {
                        // red
                        _len = (short)(_len | 0b1000000000000000);
                    }
                    else
                    {
                        // black
                        _len = (short)(_len & 0b0111111111111111);
                    }
                }
            }
            public int Len
            {
                get => _len & 0b0111111111111111;
                set
                {
                    if (IsRed)
                    {
                        _len = (short)(value | 0b1000000000000000);
                    }
                    else
                    {
                        _len = (short)value;
                    }
                }
            }
            public FBBSTreeNode(int capacity)
            {
                Keys = new K[capacity];
                Values = new V[capacity];
                _len = -32768; // 1000000000000000, Red and Len = 0
            }
            public void Shrink(int newLen)
            {
                for (int i = newLen; i < this.Len; ++i)
                {
                    this.Keys[i] = default!;
                    this.Values[i] = default!;
                }
                this.Len = newLen;
            }
        }

        private readonly int MAX; // capacity
        private readonly int MIN; // quarter of capacity
        private readonly int HALF; // half of capacity
        private FBBSTreeNode _root;
        private bool _isDB = false; // label for double black
        private bool _handleLeaf = false; // predecessor/successor of a leaf node is its parent
        public V? Deleted = default!;
        public FBBSTree(int min)
        {
            MIN = min < 1 ? 1 : (min > 4096 ? 4096 : min);
            HALF = 2 * MIN;
            MAX = 4 * MIN;
            _root = new FBBSTreeNode(MAX);
            _root.IsRed = false;
        }
        public V GetValue(K key)
        {
            return GetValueRecur(key, _root);
        }
        public bool HasKey(K key)
        {
            return HasValueRecur(key, _root);
        }
        public void Add(K key, V value)
        {
            _root = AddRecur(key, value, _root);
            _root.IsRed = false;
        }
        public void Remove(K key)
        {
            _handleLeaf = false;
            _isDB = false;
            Deleted = default!;
            _root = RemoveRecur(key, _root);
            _handleLeaf = false;
            _isDB = false;
            _root.IsRed = false;
        }
        private static V GetValueRecur(K key, FBBSTreeNode node)
        {
            if (node.Left != null && key.CompareTo(node.Keys[0]) < 0)
            {
                return GetValueRecur(key, node.Left);
            }
            if (node.Right != null && key.CompareTo(node.Keys[node.Len - 1]) > 0)
            {
                return GetValueRecur(key, node.Right);
            }
            bool found = false;
            int idx = FindIndex(node, key, out found);
            if (found)
            {
                return node.Values[idx];
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }
        private static bool HasValueRecur(K key, FBBSTreeNode node)
        {
            if (node.Left != null && key.CompareTo(node.Keys[0]) < 0)
            {
                return HasValueRecur(key, node.Left);
            }
            if (node.Right != null && key.CompareTo(node.Keys[node.Len - 1]) > 0)
            {
                return HasValueRecur(key, node.Right);
            }
            bool found = false;
            _ = FindIndex(node, key, out found);
            return found;
        }
        private FBBSTreeNode AddRecur(K key, V value, FBBSTreeNode node)
        {
            if (node.Left != null && key.CompareTo(node.Keys[0]) < 0)
            {
                node.Left = AddRecur(key, value, node.Left);
                node = ResolveDoubleRed(node);
                return node;
            }
            if (node.Right != null && key.CompareTo(node.Keys[node.Len - 1]) > 0)
            {
                node.Right = AddRecur(key, value, node.Right);
                node = ResolveRightRed(node);
                return node;
            }
            int len = node.Len;
            bool found = false;
            int idx = FindIndex(node, key, out found);
            if (found)
            {
                // update
                node.Values[idx] = value;
                return node;
            }
            if (len < MAX)
            {
                // insert
                for (int i = len; i > idx; --i)
                {
                    node.Keys[i] = node.Keys[i - 1];
                    node.Values[i] = node.Values[i - 1];
                }
                node.Keys[idx] = key;
                node.Values[idx] = value;
                node.Len += 1;
                return node;
            }
            // split
            FBBSTreeNode newNode = SplitNode(node, idx, key, value);
            node.Right = Prepend(newNode, node.Right);
            node = ResolveRightRed(node);
            return node;
        }
        private FBBSTreeNode SplitNode(FBBSTreeNode node, int idxAt, K key, V value)
        {
            FBBSTreeNode newNode = new(MAX);
            if (idxAt <= HALF) // in old node
            {
                for (int i = HALF, j = 0; i < MAX; ++i, ++j)
                {
                    newNode.Keys[j] = node.Keys[i];
                    newNode.Values[j] = node.Values[i];
                }
                for (int i = HALF; i > idxAt; --i)
                {
                    node.Keys[i] = node.Keys[i - 1];
                    node.Values[i] = node.Values[i - 1];
                }
                node.Keys[idxAt] = key;
                node.Values[idxAt] = value;
            }
            else // in new node
            {
                int j = 0;
                for (int i = HALF + 1; i < idxAt; ++i, ++j)
                {
                    newNode.Keys[j] = node.Keys[i];
                    newNode.Values[j] = node.Values[i];
                }
                newNode.Keys[j] = key;
                newNode.Values[j] = value;
                ++j;
                for (int i = idxAt; i < MAX; ++i, ++j)
                {
                    newNode.Keys[j] = node.Keys[i];
                    newNode.Values[j] = node.Values[i];
                }
            }
            node.Shrink(HALF + 1);
            newNode.Len = HALF;
            return newNode;
        }
        private FBBSTreeNode RemoveRecur(K key, FBBSTreeNode node)
        {
            if (node.Left != null && key.CompareTo(node.Keys[0]) < 0)
            {
                node.Left = RemoveRecur(key, node.Left);
                if (_handleLeaf)
                {
                    HandleLeftLeafOnDelete(node);
                    _handleLeaf = false;
                }
                node = ResolveDB(node, node.Left);
                return node;
            }
            if (node.Right != null && key.CompareTo(node.Keys[node.Len - 1]) > 0)
            {
                node.Right = RemoveRecur(key, node.Right);
                if (_handleLeaf)
                {
                    HandleRightLeafOnDelete(node);
                    _handleLeaf = false;
                }
                node = ResolveDB(node, node.Right);
                return node;
            }
            bool found = false;
            int idx = FindIndex(node, key, out found);
            if (!found)
            {
                // search miss, do nothing
                return node;
            }
            this.Deleted = node.Values[idx];
            for (int i = idx; i < node.Len - 1; ++i)
            {
                node.Keys[i] = node.Keys[i + 1];
                node.Values[i] = node.Values[i + 1];
            }
            node.Shrink(node.Len - 1);
            if (node.Len >= MIN)
            {
                return node;
            }
            // Node contains too few elements.
            // Leaf
            if (node.Left == null && node.Right == null)
            {
                _handleLeaf = true;
                return node;
            }
            // One child, only possibility is Left Red for LL RB tree.
            if (node.Left != null && node.Right == null)
            {
                HandleLeftLeafOnDelete(node);
                return node;
            }
            // two children, borrow or merge with its successor, the min of the right sub tree
            node.Right = HandleWithNextOnDelete(node, node.Right!);
            node = ResolveDB(node, node.Right);
            return node;
        }
        // Left leaf is short or the parent is short
        private void HandleLeftLeafOnDelete(FBBSTreeNode node)
        {
            int sumLen = node.Len + node.Left!.Len;
            if (sumLen < HALF + MIN)
            {
                // merge from Left child
                MergeFromPre(node, node.Left);
                _isDB = (!node.Left.IsRed);
                node.Left = null;
            }
            else
            {
                if (node.Len < MIN)
                {
                    // node borrows from Left
                    BorrowFromPre(node, node.Left);
                }
                else
                {
                    // Left borrows from node
                    BorrowFromNext(node.Left, node);
                }
                _isDB = false;
            }
        }
        // Right leaf is short.
        private void HandleRightLeafOnDelete(FBBSTreeNode node)
        {
            if (node.Len < HALF)
            {
                // merge right child into this node by append
                MergeFromNext(node, node.Right!);
                _isDB = true; // Right leaf must be BLACK
                node.Right = null;
            }
            else
            {
                // Right leaf borrows from this node
                BorrowFromPre(node.Right!, node);
                _isDB = false;
            }
        }
        private FBBSTreeNode? HandleWithNextOnDelete(FBBSTreeNode pNode, FBBSTreeNode curNode)
        {
            if (curNode.Left == null)
            {
                if (curNode.Len < HALF)
                {
                    // merge
                    MergeFromNext(pNode, curNode);
                    _isDB = (!curNode.IsRed);
                    return null;
                }
                else
                {
                    // pNode borrow from curNode
                    BorrowFromNext(pNode, curNode);
                    _isDB = false;
                    return curNode;
                }
            }
            curNode.Left = HandleWithNextOnDelete(pNode, curNode.Left!);
            curNode = ResolveDB(curNode, curNode.Left);
            return curNode;
        }
        // Add a new node as the smallest to the root
        private static FBBSTreeNode Prepend(FBBSTreeNode newNode, FBBSTreeNode? root)
        {
            if (root == null)
            {
                return newNode;
            }
            root.Left = Prepend(newNode, root.Left);
            root = ResolveDoubleRed(root);
            return root;
        }
        private FBBSTreeNode ResolveDB(FBBSTreeNode parent, FBBSTreeNode? child)
        {
            if (!_isDB)
            {
                return parent;
            }
            if (IsRed(child))
            {
                child!.IsRed = false;
                _isDB = false;
                return parent;
            }
            // Black and DB
            if (parent.Left == child)
            {
                bool pc = parent.IsRed;
                bool px = IsRed(parent.Right!.Left);
                FBBSTreeNode nd = LeftRotation(parent);
                if (!px)
                {
                    nd.Left!.IsRed = true;
                    _isDB = (!pc);
                    return nd;
                }
                else
                {
                    nd.Left = LeftRotation(nd.Left!);
                    nd = RightRotation(nd);
                    nd.Left!.IsRed = false;
                    nd.IsRed = pc;
                    _isDB = false;
                    return nd;
                }
            }
            else // right
            {
                if (parent.IsRed)
                {
                    if (IsRed(parent.Left!.Left))
                    {
                        var nd = RightRotation(parent);
                        nd.Left!.IsRed = false;
                        nd.Right!.IsRed = false;
                        nd.IsRed = true;
                        _isDB = false;
                        return nd;
                    }
                    else
                    {
                        parent.IsRed = false;
                        parent.Left.IsRed = true;
                        _isDB = false;
                        return parent;
                    }
                }
                else // parent is black
                {
                    if (!parent.Left!.IsRed)
                    {
                        if (!IsRed(parent.Left.Left))
                        {
                            parent.Left.IsRed = true;
                            _isDB = true;
                            return parent;
                        }
                        else
                        {
                            var nd = RightRotation(parent);
                            nd.Left!.IsRed = false;
                            _isDB = false;
                            return nd;
                        }
                    }
                    else
                    {
                        var nd = RightRotation(parent);
                        nd.IsRed = false;
                        nd.Right!.Left!.IsRed = true;
                        if (IsDoubleRed(nd.Right.Left))
                        {
                            nd.Right = RightRotation(nd.Right);
                            nd.Right.Left!.IsRed = false;
                            nd = LeftRotation(nd);
                            nd.IsRed = false;
                            nd.Left!.IsRed = true;
                        }
                        _isDB = false;
                        return nd;
                    }
                }
            }
        }
        private static int FindIndex(FBBSTreeNode node, K key, out bool found)
        {
            int lo = 0;
            int hi = node.Len - 1;
            int mid = (lo + hi) / 2;
            while (true)
            {
                if (lo > hi)
                {
                    found = false;
                    return lo;
                }
                int comp = key.CompareTo(node.Keys[mid]);
                if (comp == 0)
                {
                    found = true;
                    return mid;
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
            }
        }
        private static FBBSTreeNode LeftRotation(FBBSTreeNode node)
        {
            var tmp = node.Right;
            node.Right = node.Right!.Left;
            tmp!.Left = node;
            return tmp;
        }
        private static FBBSTreeNode RightRotation(FBBSTreeNode node)
        {
            var tmp = node.Left;
            node.Left = node.Left!.Right;
            tmp!.Right = node;
            return tmp;
        }
        private static bool IsRed(FBBSTreeNode? node)
        {
            if (node == null)
            {
                return false;
            }
            return node.IsRed;
        }
        private static bool IsDoubleRed(FBBSTreeNode? node)
        {
            if (IsRed(node) && IsRed(node!.Left))
            {
                return true;
            }
            return false;
        }
        private static FBBSTreeNode ResolveDoubleRed(FBBSTreeNode node)
        {
            if (IsDoubleRed(node.Left))
            {
                node = RightRotation(node);
                node.Left!.IsRed = false;
            }
            return node;
        }
        private static FBBSTreeNode ResolveRightRed(FBBSTreeNode node)
        {
            if (IsRed(node.Right))
            {
                if (IsRed(node.Left)) // flip color
                {
                    node.Left!.IsRed = false;
                    node.Right!.IsRed = false;
                    node.IsRed = true;
                }
                else
                {
                    bool c = node.IsRed;
                    node = LeftRotation(node);
                    node.Left!.IsRed = true;
                    node.IsRed = c;
                }
            }
            return node;
        }
        private static void ArrayShiftLeft<T>(T[] arr, int curLen, int shiftLeft)
        {
            for (int i = 0; i < curLen - shiftLeft; ++i)
            {
                arr[i] = arr[i + shiftLeft];
            }
        }
        private static void ArrayShiftRight<T>(T[] arr, int curLen, int shiftRight)
        {
            for (int i = curLen - 1; i >= 0; --i)
            {
                arr[i + shiftRight] = arr[i];
            }
        }
        private static void BorrowFromPre(FBBSTreeNode thisNode, FBBSTreeNode pre)
        {
            int sumLen = pre.Len + thisNode.Len;
            int rightShift = sumLen / 2 - thisNode.Len;
            ArrayShiftRight(thisNode.Keys, thisNode.Len, rightShift);
            ArrayShiftRight(thisNode.Values, thisNode.Len, rightShift);
            for (int i = 0, j = pre.Len - rightShift; i < rightShift; ++i, ++j)
            {
                thisNode.Keys[i] = pre.Keys[j];
                thisNode.Values[i] = pre.Values[j];
            }
            thisNode.Len += rightShift;
            pre.Shrink(pre.Len - rightShift);
        }
        private static void BorrowFromNext(FBBSTreeNode thisNode, FBBSTreeNode next)
        {
            int sumLen = next.Len + thisNode.Len;
            int leftShift = sumLen / 2 - thisNode.Len;
            for (int i = thisNode.Len, j = 0; j < leftShift; ++i, ++j)
            {
                thisNode.Keys[i] = next.Keys[j];
                thisNode.Values[i] = next.Values[j];
            }
            ArrayShiftLeft(next.Keys, next.Len, leftShift);
            ArrayShiftLeft(next.Values, next.Len, leftShift);
            thisNode.Len += leftShift;
            next.Shrink(next.Len - leftShift);
        }
        private static void MergeFromPre(FBBSTreeNode thisNode, FBBSTreeNode pre)
        {
            int rightShift = pre.Len;
            ArrayShiftRight(thisNode.Keys, thisNode.Len, rightShift);
            ArrayShiftRight(thisNode.Values, thisNode.Len, rightShift);
            for (int i = 0; i < rightShift; ++i)
            {
                thisNode.Keys[i] = pre.Keys[i];
                thisNode.Values[i] = pre.Values[i];
            }
            thisNode.Len += rightShift;
        }
        private static void MergeFromNext(FBBSTreeNode thisNode, FBBSTreeNode next)
        {
            for (int i = thisNode.Len, j = 0; j < next.Len; ++i, ++j)
            {
                thisNode.Keys[i] = next.Keys[j];
                thisNode.Values[i] = next.Values[j];
            }
            thisNode.Len += next.Len;
        }
    }
}
