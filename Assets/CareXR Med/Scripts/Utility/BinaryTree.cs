using BestHTTP.Decompression.Zlib;
using System;
using System.Text.RegularExpressions;

using Debug = XRDebug;

public class BinaryTree 
{
    public BinaryTree()
    {
        Root = null;
    }

    public Node Root { get; private set; }

    public class Node
    {
        public Node LeftNode { get; set; }
        public Node RightNode { get; set; }
        public string key { get; set; }

        public object data;

    }


    public int HexStringCompare(string keyOne, string keyTwo)
    {
        keyOne = keyOne.Replace("-", string.Empty);
        keyTwo = keyTwo.Replace("-", string.Empty);

        string InvalidHexExp = @"[^\dabcdef]";
        string HexPaddingExp = @"^(0x)?0*";
        //Remove whitespace, "0x" prefix if present, and leading zeros.  
        //Also make all characters lower case.
        string Value1 = Regex.Replace(keyOne.Trim().ToLower(), HexPaddingExp, "");
        string Value2 = Regex.Replace(keyTwo.Trim().ToLower(), HexPaddingExp, "");

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

    public bool Add(string key, object data)
    {
        Node before = null, after = this.Root;
        while (after != null)
        {
            before = after;
            //if (key < after.key) //Is new node in left tree? 
            if (this.HexStringCompare(key, after.key) < 0)
                after = after.LeftNode;
           //else if (key > after.key) //Is new node in right tree?
            else if (this.HexStringCompare(key, after.key) > 0) //Is new node in right tree?
                after = after.RightNode;
            else
            {
                //Exist same key
                return false;
            }
        }

        Node newNode = new Node();
        newNode.key = key;
        newNode.data = data;
     
        if (this.Root == null)//Tree ise empty
            this.Root = newNode;
        else
        {
            //if (key < before.key)
            if (this.HexStringCompare(key, before.key) < 0)
                before.LeftNode = newNode;
            else
                before.RightNode = newNode;
        }

        return true;
    }

    public Node Find(string key)
    {
        return this.Find(key, this.Root);
    }

    public void Remove(string key)
    {
        this.Root = Remove(this.Root, key);
    }

    private Node Remove(Node parent, string key)
    {
        if (parent == null) return parent;

        //if (key < parent.key) parent.LeftNode = Remove(parent.LeftNode, key);
        if (this.HexStringCompare(key, parent.key) < 0) parent.LeftNode = Remove(parent.LeftNode, key);
        //else if (key > parent.key)
        else if (this.HexStringCompare(key, parent.key) > 0)
            parent.RightNode = Remove(parent.RightNode, key);

        // if key is same as parent's key, then this is the node to be deleted  
        else
        {
            // node with only one child or no child  
            if (parent.LeftNode == null)
                return parent.RightNode;
            else if (parent.RightNode == null)
                return parent.LeftNode;

            // node with two children: Get the inorder successor (smallest in the right subtree)  
            parent.key = MinValue(parent.RightNode);

            // Delete the inorder successor  
            parent.RightNode = Remove(parent.RightNode, parent.key);
        }

        return parent;
    }

    private string MinValue(Node node)
    {
        string minv = node.key;
        while (node.LeftNode != null)
        {
            minv = node.LeftNode.key;
            node = node.LeftNode;
        }
        return minv;
    }

    private Node Find(string key, Node parent)
    {
        if (parent != null)
        {
            //if (key == parent.key) return parent;
            if (this.HexStringCompare(key, parent.key) == 0) return parent;
            //if (key < parent.key)
            if (this.HexStringCompare(key, parent.key) < 0)
                    return Find(key, parent.LeftNode);
            else
                return Find(key, parent.RightNode);
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
            Console.Write(parent.key + " ");
            TraversePreOrder(parent.LeftNode);
            TraversePreOrder(parent.RightNode);
        }
    }

    public void TraverseInOrder(Node parent)
    {
        if (parent != null)
        {
            TraverseInOrder(parent.LeftNode);
            Console.Write(parent.key + " ");
            TraverseInOrder(parent.RightNode);
        }
    }

    public void TraversePostOrder(Node parent)
    {
        if (parent != null)
        {
            TraversePostOrder(parent.LeftNode);
            TraversePostOrder(parent.RightNode);
            Console.Write(parent.key + " ");
        }
    }
}
