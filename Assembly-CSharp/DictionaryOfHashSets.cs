using System.Collections.Generic;

public class DictionaryOfHashSets<TKey, TValue> : DictionaryOfSets<TKey, TValue, HashSet<TValue>, Dictionary<TKey, HashSet<TValue>>>
{
}
