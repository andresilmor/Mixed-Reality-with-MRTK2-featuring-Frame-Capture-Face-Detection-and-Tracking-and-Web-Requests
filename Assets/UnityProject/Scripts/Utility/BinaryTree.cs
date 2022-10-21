using BestHTTP.Decompression.Zlib;
using System;
using System.Text.RegularExpressions;

public class BinaryTree 
{

    public BinaryTree()
    {
        Root = null;
    }

    public Node Root { get; set; }

    public class Node
    {
        public Node LeftNode { get; set; }
        public Node RightNode { get; set; }
        public string index { get; set; }

        public NodeType data;

    }

    abstract public class NodeType { }

    public int HexStringCompare(string value1, string value2)
    {
        string InvalidHexExp = @"[^\dabcdef]";
        string HexPaddingExp = @"^(0x)?0*";
        //Remove whitespace, "0x" prefix if present, and leading zeros.  
        //Also make all characters lower case.
        string Value1 = Regex.Replace(value1.Trim().ToLower(), HexPaddingExp, "");
        string Value2 = Regex.Replace(value2.Trim().ToLower(), HexPaddingExp, "");

        //validate that values contain only hex characters
        if (Regex.IsMatch(Value1, InvalidHexExp))
        {
            throw new ArgumentOutOfRangeException("Value1 is not a hex string");
        }
        if (Regex.IsMatch(Value2, InvalidHexExp))
        {
            throw new ArgumentOutOfRangeException("Value2 is not a hex string");
        }

        int Result = Value1.Length.CompareTo(Value2.Length);
        if (Result == 0)
        {
            Result = Value1.CompareTo(Value2);
        }

        return Result;
    }

    public bool Add(string value, NodeType data)
    {
        Node before = null, after = this.Root;
        while (after != null)
        {
            before = after;
            //if (value < after.index) //Is new node in left tree? 
            if (this.HexStringCompare(value, after.index) < 0)
                after = after.LeftNode;
           //else if (value > after.index) //Is new node in right tree?
            else if (this.HexStringCompare(value, after.index) > 0) //Is new node in right tree?
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
            //if (value < before.index)
            if (this.HexStringCompare(value, before.index) < 0)
                before.LeftNode = newNode;
            else
                before.RightNode = newNode;
        }

        return true;
    }

    public Node Find(string value)
    {
        return this.Find(value, this.Root);
    }

    public void Remove(string value)
    {
        this.Root = Remove(this.Root, value);
    }

    private Node Remove(Node parent, string key)
    {
        if (parent == null) return parent;

        //if (key < parent.index) parent.LeftNode = Remove(parent.LeftNode, key);
        if (this.HexStringCompare(key, parent.index) < 0) parent.LeftNode = Remove(parent.LeftNode, key);
        //else if (key > parent.index)
        else if (this.HexStringCompare(key, parent.index) > 0)
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

    private string MinValue(Node node)
    {
        string minv = node.index;
        while (node.LeftNode != null)
        {
            minv = node.LeftNode.index;
            node = node.LeftNode;
        }
        return minv;
    }

    private Node Find(string value, Node parent)
    {
        if (parent != null)
        {
            //if (value == parent.index) return parent;
            if (this.HexStringCompare(value, parent.index) == 0) return parent;
            //if (value < parent.index)
            if (this.HexStringCompare(value, parent.index) < 0)
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
