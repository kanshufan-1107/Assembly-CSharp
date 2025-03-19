using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class DeletedInvalidBundles : IProtoBuf
{
	private List<string> _Bundles = new List<string>();

	public bool HasCounts;

	private int _Counts;

	public List<string> Bundles
	{
		get
		{
			return _Bundles;
		}
		set
		{
			_Bundles = value;
		}
	}

	public int Counts
	{
		get
		{
			return _Counts;
		}
		set
		{
			_Counts = value;
			HasCounts = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		foreach (string i in Bundles)
		{
			hash ^= i.GetHashCode();
		}
		if (HasCounts)
		{
			hash ^= Counts.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is DeletedInvalidBundles other))
		{
			return false;
		}
		if (Bundles.Count != other.Bundles.Count)
		{
			return false;
		}
		for (int i = 0; i < Bundles.Count; i++)
		{
			if (!Bundles[i].Equals(other.Bundles[i]))
			{
				return false;
			}
		}
		if (HasCounts != other.HasCounts || (HasCounts && !Counts.Equals(other.Counts)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static DeletedInvalidBundles Deserialize(Stream stream, DeletedInvalidBundles instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static DeletedInvalidBundles DeserializeLengthDelimited(Stream stream)
	{
		DeletedInvalidBundles instance = new DeletedInvalidBundles();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static DeletedInvalidBundles DeserializeLengthDelimited(Stream stream, DeletedInvalidBundles instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static DeletedInvalidBundles Deserialize(Stream stream, DeletedInvalidBundles instance, long limit)
	{
		if (instance.Bundles == null)
		{
			instance.Bundles = new List<string>();
		}
		while (true)
		{
			if (limit >= 0 && stream.Position >= limit)
			{
				if (stream.Position == limit)
				{
					break;
				}
				throw new ProtocolBufferException("Read past max limit");
			}
			int keyByte = stream.ReadByte();
			switch (keyByte)
			{
			case -1:
				break;
			case 10:
				instance.Bundles.Add(ProtocolParser.ReadString(stream));
				continue;
			case 24:
				instance.Counts = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			default:
			{
				Key key = ProtocolParser.ReadKey((byte)keyByte, stream);
				if (key.Field == 0)
				{
					throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
				}
				ProtocolParser.SkipKey(stream, key);
				continue;
			}
			}
			if (limit < 0)
			{
				break;
			}
			throw new EndOfStreamException();
		}
		return instance;
	}

	public void Serialize(Stream stream)
	{
		Serialize(stream, this);
	}

	public static void Serialize(Stream stream, DeletedInvalidBundles instance)
	{
		if (instance.Bundles.Count > 0)
		{
			foreach (string i1 in instance.Bundles)
			{
				stream.WriteByte(10);
				ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(i1));
			}
		}
		if (instance.HasCounts)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Counts);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (Bundles.Count > 0)
		{
			foreach (string i1 in Bundles)
			{
				size++;
				uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(i1);
				size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
			}
		}
		if (HasCounts)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Counts);
		}
		return size;
	}
}
