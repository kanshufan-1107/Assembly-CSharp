using System;

public static class ReadOnlySpanExtensions
{
	public ref struct SplitEnumerator
	{
		private ReadOnlySpan<char> m_stringSpan;

		private ReadOnlySpan<char> m_delimiter;

		public ReadOnlySpan<char> Current { get; private set; }

		public SplitEnumerator(ReadOnlySpan<char> stringSpan, ReadOnlySpan<char> delimiters)
		{
			m_stringSpan = stringSpan;
			m_delimiter = delimiters;
			Current = default(ReadOnlySpan<char>);
		}

		public SplitEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			ReadOnlySpan<char> span = m_stringSpan;
			if (span.Length == 0)
			{
				return false;
			}
			int index = span.IndexOfAny(m_delimiter);
			if (index == -1)
			{
				m_stringSpan = ReadOnlySpan<char>.Empty;
				Current = span;
				return true;
			}
			Current = span.Slice(0, index);
			m_stringSpan = span.Slice(index + 1);
			return true;
		}
	}

	public static SplitEnumerator Split(this ReadOnlySpan<char> span, ReadOnlySpan<char> delimiters)
	{
		return new SplitEnumerator(span, delimiters);
	}

	public static bool HasNonSpaceCharacter(this ReadOnlySpan<char> span)
	{
		int spanLength = span.Length;
		bool hasNonSpaceCharacter = false;
		for (int i = 0; i < spanLength; i++)
		{
			if (span[i] != ' ')
			{
				hasNonSpaceCharacter = true;
				break;
			}
		}
		return hasNonSpaceCharacter;
	}
}
