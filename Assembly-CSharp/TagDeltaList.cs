using System.Collections.Generic;

public class TagDeltaList
{
	private List<TagDelta> m_deltas = new List<TagDelta>();

	public int Count => m_deltas.Count;

	public TagDelta this[int index] => m_deltas[index];

	public void Add(int tag, int prev, int curr)
	{
		TagDelta delta = new TagDelta();
		delta.tag = tag;
		delta.oldValue = prev;
		delta.newValue = curr;
		m_deltas.Add(delta);
	}
}
