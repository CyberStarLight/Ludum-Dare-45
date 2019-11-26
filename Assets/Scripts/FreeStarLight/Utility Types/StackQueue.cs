using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackQueue<T> : LinkedList<T>
{
    public void Push(T obj)
    {
        AddFirst(obj);
    }

    public void Enqueue(T obj)
    {
        AddLast(obj);
    }

    public T Pop()
    {
        T obj = First.Value;
        RemoveFirst();
        return obj;
    }

    public T Dequeue()
    {
        T obj = Last.Value;
        RemoveLast();
        return obj;
    }

    public T PeekStack()
    {
        return First.Value;
    }

    public T PeekQueue()
    {
        return Last.Value;
    }
}