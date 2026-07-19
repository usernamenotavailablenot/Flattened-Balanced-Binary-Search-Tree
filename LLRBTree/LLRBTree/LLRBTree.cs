namespace RBTree
{
    public class RBTreeNode<K, V> where K : IComparable
    {
        public required K Key;
        public required V Value;
        public RBTreeNode<K, V>? Left;
        public RBTreeNode<K, V>? Right;
        public bool IsRed;
    }
    /// <summary>
    /// Left leaning RB tree
    /// </summary>
    public class LLRBTree<K, V> where K : IComparable 
    {
        public RBTreeNode<K, V>? Deleted;
        private bool _isDB; // whether Double Black
        private RBTreeNode<K, V>? _root;

        public void Add(K key, V value)
        {
            Deleted = default;
            _isDB = false;
            _root = AddToRBTreeRecur(key, value, _root);
            _root.IsRed = false;
        }
        public void Remove(K key)
        {
            Deleted = default;
            _isDB = false;
            _root = DeleteFromRbTreeRecur(key, _root);
            if (_root != null)
            {
                _root.IsRed = false;
            }
            _isDB = false;
        }
        public V GetValue(K key)
        {
            return GetValue(key, _root);
        }
        public bool HasKey(K key)
        {
            return HasKey(key, _root);
        }
        private static bool HasKey(K key, RBTreeNode<K, V>? root)
        {
            if (root == null)
            {
                return false;
            }
            int comp = key.CompareTo(root.Key);
            if (comp == 0)
            {
                return true;
            }
            if (comp < 0)
            {
                return HasKey(key, root.Left);
            }
            else
            {
                return HasKey(key, root.Right);
            }
        }
        private static V GetValue(K key, RBTreeNode<K, V>? root)
        {
            if (root == null)
            {
                throw new KeyNotFoundException();
            }
            int comp = key.CompareTo(root.Key);
            if (comp == 0)
            {
                return root.Value;
            }
            if (comp < 0)
            {
                return GetValue(key, root.Left);
            }
            else
            {
                return GetValue(key, root.Right);
            }

        }
        private static RBTreeNode<K, V> AddToRBTreeRecur(K key, V value, RBTreeNode<K, V>? node)
        {
            if (node == null)
            {
                return new RBTreeNode<K, V>() { Key = key, Value = value, IsRed = true };
            }
            int comp = key.CompareTo(node.Key);
            if (comp == 0)
            {
                node.Value = value; // update
            }
            else if (comp < 0)
            {
                node.Left = AddToRBTreeRecur(key, value, node.Left);
                if (IsDoubleRed(node.Left)) // Double Red violation
                {
                    node = RightRotation(node);
                    node.Left!.IsRed = false;
                }
            }
            else
            {
                node.Right = AddToRBTreeRecur(key, value, node.Right);
                if (IsRed(node.Right)) // Right Red violation
                {
                    if (IsRed(node.Left)) // flip color
                    {
                        node.Left!.IsRed = false;
                        node.Right.IsRed = false;
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
            }
            return node;
        }
        private static bool IsRed(RBTreeNode<K, V>? node)
        {
            if (node == null)
            {
                return false;
            }
            return node.IsRed;
        }
        private static bool IsDoubleRed(RBTreeNode<K, V>? node)
        {
            if (IsRed(node) && IsRed(node!.Left))
            {
                return true;
            }
            return false;
        }
        private static RBTreeNode<K, V> LeftRotation(RBTreeNode<K, V> node)
        {
            var tmp = node.Right;
            node.Right = node.Right!.Left;
            tmp!.Left = node;
            return tmp;
        }
        private static RBTreeNode<K, V> RightRotation(RBTreeNode<K, V> node)
        {
            var tmp = node.Left;
            node.Left = node.Left!.Right;
            tmp!.Right = node;
            return tmp;
        }
        private RBTreeNode<K, V>? DeleteFromRbTreeRecur(K key, RBTreeNode<K, V>? root)
        {
            if (root == null)
            {
                return null; // Key does not exist, search miss, do nothing
            }
            int comp = key.CompareTo(root.Key);
            RBTreeNode<K, V>? dRoot;
            if (comp == 0) // Search hit
            {
                if (root.Left == null || root.Right == null) // leaf or only one child
                {
                    Deleted = root;
                    _isDB = !root.IsRed;
                    dRoot = root.Left ?? root.Right;
                }
                else // has two children
                {
                    RBTreeNode<K, V>? dMinRoot = DelMin(root.Right);
                    RBTreeNode<K, V> newRoot = Deleted!;
                    Deleted = root;
                    newRoot.IsRed = root.IsRed;
                    newRoot.Left = root.Left;
                    newRoot.Right = dMinRoot;
                    dRoot = ResolveDb(newRoot, dMinRoot);
                }
                Deleted.Left = null;
                Deleted.Right = null;
            }
            else if (comp < 0)
            {
                root.Left = DeleteFromRbTreeRecur(key, root.Left);
                dRoot = ResolveDb(root, root.Left);
            }
            else
            {
                root.Right = DeleteFromRbTreeRecur(key, root.Right);
                dRoot = ResolveDb(root, root.Right);
            }
            return dRoot;
        }
        private RBTreeNode<K, V>? DelMin(RBTreeNode<K, V> node)
        {
            if (node.Left == null)
            {
                Deleted = node;
                _isDB = (!node.IsRed);
                return node.Right;
            }
            node.Left = DelMin(node.Left);
            RBTreeNode<K, V> dMinRoot = ResolveDb(node, node.Left);
            return dMinRoot;
        }
        private RBTreeNode<K, V> ResolveDb(RBTreeNode<K, V> parent, RBTreeNode<K, V>? child)
        {
            if (!_isDB) // Not Double Black, do nothing
            {
                return parent;
            }
            if (IsRed(child)) // Red and Double Black, flip color
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
                RBTreeNode<K, V> nd = LeftRotation(parent);
                if (!px) // Black
                {
                    nd.Left!.IsRed = true;
                    _isDB = !pc; // Black
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
                    if (!parent.Left!.IsRed) // Black
                    {
                        if (!IsRed(parent.Left.Left)) // Black
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
    }
}
