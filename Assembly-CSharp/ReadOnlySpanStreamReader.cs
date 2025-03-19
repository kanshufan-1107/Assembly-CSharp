using System;
using System.IO;

public class ReadOnlySpanStreamReader : IDisposable
{
	private StreamReader m_streamReader;

	private char[] m_readBuffer;

	private readonly int m_capacity;

	private int m_position;

	private int m_size;

	private bool m_fileHasMoreContent;

	public ReadOnlySpanStreamReader(string path)
	{
		m_capacity = 1024;
		Init(path);
	}

	public void Dispose()
	{
		m_streamReader.Dispose();
	}

	public bool ReadLine(ref ReadOnlySpan<char> line)
	{
		if (!m_fileHasMoreContent && m_position >= m_size)
		{
			return false;
		}
		if (m_position >= m_size)
		{
			m_size = 0;
			ReadFromFile();
		}
		if (!m_fileHasMoreContent && m_size == 0)
		{
			return false;
		}
		int newLineIndex = IndexOfNewLine();
		if (newLineIndex == -1)
		{
			int totalCharactersLeft = m_size - m_position;
			int i = m_position;
			int j = 0;
			while (i < m_size)
			{
				m_readBuffer[j] = m_readBuffer[i];
				i++;
				j++;
			}
			m_size = totalCharactersLeft;
			ReadFromFile();
			newLineIndex = IndexOfNewLine();
			if (newLineIndex == -1)
			{
				newLineIndex = m_size;
			}
		}
		int spanSize = newLineIndex - m_position;
		line = new ReadOnlySpan<char>(m_readBuffer, m_position, spanSize);
		m_position = newLineIndex;
		SkipNewline();
		return true;
	}

	private void Init(string path)
	{
		m_readBuffer = new char[m_capacity];
		m_streamReader = new StreamReader(path);
		m_fileHasMoreContent = true;
		m_size = 0;
		m_position = 0;
		ReadFromFile();
	}

	private void ReadFromFile()
	{
		int maxToRead = m_capacity - m_size;
		if (maxToRead <= 0)
		{
			m_size = 0;
			maxToRead = m_capacity;
		}
		int totalRead = m_streamReader.Read(m_readBuffer, m_size, maxToRead);
		m_fileHasMoreContent = m_streamReader.Peek() != -1;
		m_position = 0;
		m_size += Math.Max(0, totalRead);
		SkipNewline();
	}

	private int IndexOfNewLine()
	{
		for (int i = m_position; i < m_size; i++)
		{
			if (m_readBuffer[i] == '\r' || m_readBuffer[i] == '\n')
			{
				return i;
			}
		}
		return -1;
	}

	private void SkipNewline()
	{
		while (m_position < m_size && (m_readBuffer[m_position] == '\r' || m_readBuffer[m_position] == '\n'))
		{
			m_position++;
		}
	}
}
