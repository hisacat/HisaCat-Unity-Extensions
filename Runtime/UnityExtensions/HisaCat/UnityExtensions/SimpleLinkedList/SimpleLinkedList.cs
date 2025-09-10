using System;

namespace HisaCat.Collections
{
    public class SimpleLinkedList<T>
    {
        public Node First;
        public Node Last;
        public int Count = 0;

        public void AddFirst(Node node)
        {
            if (node == null) throw new ArgumentNullException("node cannot be null");
            if (node.List != null)
            {
                if (node.List == this)
                    throw new ArgumentException("this node already in list");
                else
                    throw new ArgumentException("this node already used in other list");
            }

            node.Prev = null;
            node.Next = this.First;

            if (this.Last == null && this.First == null)
            {
                this.First = node;
                this.Last = node;
            }
            else
            {
                this.First.Prev = node;
                this.First = node;
            }

            node.List = this;

            this.Count++;
        }
        public Node AddFirst(T data)
        {
            var newNode = new Node(data);
            AddFirst(newNode);
            return newNode;
        }
        public void AddLast(Node node)
        {
            if (node == null) throw new ArgumentNullException("node cannot be null");
            if (node.List != null)
            {
                if (node.List == this) throw new ArgumentException("this node already in list");
                if (node.List != this) throw new ArgumentException("this node already used in other list");
            }

            node.Prev = this.Last;
            node.Next = null;

            if (this.Last == null && this.First == null)
            {
                this.First = node;
                this.Last = node;
            }
            else
            {
                this.Last.Next = node;
                this.Last = node;
            }

            node.List = this;

            this.Count++;
        }
        public Node AddLast(T data)
        {
            var newNode = new Node(data);
            AddLast(newNode);
            return newNode;
        }
        public void Clear()
        {
            if (this.First == null)
                return;

            var head = this.First;

            this.First = null;
            this.Last = null;

            while (head != null)
            {
                var next = head.Next;
                head.List = null;
                head.Prev = null;
                head.Next = null;

                head = next;
            }

            this.Count = 0;
        }
        public void Remove(Node node)
        {
            if (node == null) throw new ArgumentNullException("node cannot be null");
            if (node.List != this) throw new ArgumentException("this node not in current list");

            if (this.First == node && this.Last == node)
            {
                this.First = this.Last = null;
            }
            else if (this.First == node)
            {
                this.First = node.Next;
                this.First.Prev = null;
            }
            else if (this.Last == node)
            {
                this.Last = node.Prev;
                this.Last.Next = null;
            }
            else
            {
                node.Prev.Next = node.Next;
                node.Next.Prev = node.Prev;
            }

            node.Prev = null;
            node.Next = null;
            node.List = null;

            this.Count--;
        }
        public void Remove(T value)
        {
            var node = this.First;
            while (node != null)
            {
                if (node.Value.Equals(value))
                {
                    Remove(node);
                    return;
                }
                node = node.Next;
            }
        }
        public void RemoveAll(T value)
        {
            var node = this.First;
            while (node != null)
            {
                var temp = node;
                node = node.Next;
                if (temp.Value.Equals(value))
                {
                    Remove(temp);
                    continue;
                }
            }
        }

        public class Node
        {
            public SimpleLinkedList<T> List { get; internal set; }
            public Node Prev { get; internal set; }
            public Node Next { get; internal set; }

            public T Value { get; set; }

            public Node(T value)
            {
                this.Value = value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }
    }
}
