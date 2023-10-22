using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DictionaryTwoKey<F, S, T>
{
    public Dictionary<F, Container> DicionaryFirst { get; private set; }
    public Dictionary<S, Container> DictionarySecond { get; private set; }

    public int Count { get { return DicionaryFirst.Count; } }

    public struct Container
    {
        public F firstKey;
        public S secondKey;
        public T obj;
    }

    public DictionaryTwoKey()
    {
        this.DicionaryFirst = new Dictionary<F, Container>();
        this.DictionarySecond = new Dictionary<S, Container>();
    }

    public bool Contains(F key)
    {
        return DicionaryFirst.ContainsKey(key);
    }

    public bool Contains(S key)
    {
        return DictionarySecond.ContainsKey(key);
    }

    public void Remove(F key)
    {
        if (DicionaryFirst.ContainsKey(key))
        {
            Container container = DicionaryFirst[key];
            DicionaryFirst.Remove(key);
            if (DictionarySecond.ContainsKey(container.secondKey))
            {
                DictionarySecond.Remove(container.secondKey);
            }
        }
    }

    public void Remove(S key)
    {
        if (DictionarySecond.ContainsKey(key))
        {
            Container container = DictionarySecond[key];
            DictionarySecond.Remove(key);
            if (DicionaryFirst.ContainsKey(container.firstKey))
            {
                DicionaryFirst.Remove(container.firstKey);
            }
        }
    }

    public void Add(F firstKey, S secondKey, T obj)
    {
        if (DicionaryFirst.ContainsKey(firstKey) ||
            DictionarySecond.ContainsKey(secondKey))
        {
            return;
        }

        Container container = new Container
        {
            firstKey = firstKey,
            secondKey = secondKey,
            obj = obj
        };

        DicionaryFirst.Add(firstKey, container);
        DictionarySecond.Add(secondKey, container);
    }

    public T Get(F firstKey)
    {
        if (DicionaryFirst.ContainsKey(firstKey))
        {
            return DicionaryFirst[firstKey].obj;
        }

        return default;
    }

    public T Get(S secondKey)
    {
        if (DictionarySecond.ContainsKey(secondKey))
        {
            return DictionarySecond[secondKey].obj;
        }

        return default;
    }

    public T[] ToArray()
    {
        return DicionaryFirst.Values.Select(x => x.obj).ToArray();
    }
}
