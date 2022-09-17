using System;

public class BinaryTree 
{
    public Node Root { get; set; }

    public class Node
    {
        public Node LeftNode { get; set; }
        public Node RightNode { get; set; }
        public int index { get; set; }

        public NodeType data;

    }

    abstract public class NodeType { }

    public bool Add(int value, NodeType data)
    {
        Node before = null, after = this.Root;

        while (after != null)
        {
            before = after;
            if (value < after.index) //Is new node in left tree? 
                after = after.LeftNode;
            else if (value > after.index) //Is new node in right tree?
                after = after.RightNode;
            else
            {
                //Exist same value
                return false;
            }
        }

        Node newNode = new Node();
        newNode.index = value;
        newNode.data = data;

        if (this.Root == null)//Tree ise empty
            this.Root = newNode;
        else
        {
            if (value < before.index)
                before.LeftNode = newNode;
            else
                before.RightNode = newNode;
        }

        return true;
    }

    public Node Find(int value)
    {
        return this.Find(value, this.Root);
    }

    public void Remove(int value)
    {
        this.Root = Remove(this.Root, value);
    }

    private Node Remove(Node parent, int key)
    {
        if (parent == null) return parent;

        if (key < parent.index) parent.LeftNode = Remove(parent.LeftNode, key);
        else if (key > parent.index)
            parent.RightNode = Remove(parent.RightNode, key);

        // if value is same as parent's value, then this is the node to be deleted  
        else
        {
            // node with only one child or no child  
            if (parent.LeftNode == null)
                return parent.RightNode;
            else if (parent.RightNode == null)
                return parent.LeftNode;

            // node with two children: Get the inorder successor (smallest in the right subtree)  
            parent.index = MinValue(parent.RightNode);

            // Delete the inorder successor  
            parent.RightNode = Remove(parent.RightNode, parent.index);
        }

        return parent;
    }

    private int MinValue(Node node)
    {
        int minv = node.index;

        while (node.LeftNode != null)
        {
            minv = node.LeftNode.index;
            node = node.LeftNode;
        }

        return minv;
    }

    private Node Find(int value, Node parent)
    {
        if (parent != null)
        {
            if (value == parent.index) return parent;
            if (value < parent.index)
                return Find(value, parent.LeftNode);
            else
                return Find(value, parent.RightNode);
        }

        return null;
    }

    public int GetTreeDepth()
    {
        return this.GetTreeDepth(this.Root);
    }

    private int GetTreeDepth(Node parent)
    {
        return parent == null ? 0 : Math.Max(GetTreeDepth(parent.LeftNode), GetTreeDepth(parent.RightNode)) + 1;
    }

    public void TraversePreOrder(Node parent)
    {
        if (parent != null)
        {
            Console.Write(parent.index + " ");
            TraversePreOrder(parent.LeftNode);
            TraversePreOrder(parent.RightNode);
        }
    }

    public void TraverseInOrder(Node parent)
    {
        if (parent != null)
        {
            TraverseInOrder(parent.LeftNode);
            Console.Write(parent.index + " ");
            TraverseInOrder(parent.RightNode);
        }
    }

    public void TraversePostOrder(Node parent)
    {
        if (parent != null)
        {
            TraversePostOrder(parent.LeftNode);
            TraversePostOrder(parent.RightNode);
            Console.Write(parent.index + " ");
        }
    }
}
