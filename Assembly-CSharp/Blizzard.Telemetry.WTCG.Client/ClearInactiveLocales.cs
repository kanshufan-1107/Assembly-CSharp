using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class ClearInactiveLocales : IProtoBuf
{
	private List<string> _Locales = new List<string>();

	public bool HasSuccess;

	private bool _Success;

	public bool HasErrors;

	private string _Errors;

	public List<string> Locales
	{
		get
		{
			return _Locales;
		}
		set
		{
			_Locales = value;
		}
	}

	public bool Success
	{
		get
		{
			return _Success;
		}
		set
		{
			_Success = value;
			HasSuccess = true;
		}
	}

	public string Errors
	{
		get
		{
			return _Errors;
		}
		set
		{
			_Errors = value;
			HasErrors = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		foreach (string i in Locales)
		{
			hash ^= i.GetHashCode();
		}
		if (HasSuccess)
		{
			hash ^= Success.GetHashCode();
		}
		if (HasErrors)
		{
			hash ^= Errors.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ClearInactiveLocales other))
		{
			return false;
		}
		if (Locales.Count != other.Locales.Count)
		{
			return false;
		}
		for (int i = 0; i < Locales.Count; i++)
		{
			if (!Locales[i].Equals(other.Locales[i]))
			{
				return false;
			}
		}
		if (HasSuccess != other.HasSuccess || (HasSuccess && !Success.Equals(other.Success)))
		{
			return false;
		}
		if (HasErrors != other.HasErrors || (HasErrors && !Errors.Equals(other.Errors)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ClearInactiveLocales Deserialize(Stream stream, ClearInactiveLocales instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ClearInactiveLocales DeserializeLengthDelimited(Stream stream)
	{
		ClearInactiveLocales instance = new ClearInactiveLocales();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ClearInactiveLocales DeserializeLengthDelimited(Stream stream, ClearInactiveLocales instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ClearInactiveLocales Deserialize(Stream stream, ClearInactiveLocales instance, long limit)
	{
		if (instance.Locales == null)
		{
			instance.Locales = new List<string>();
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
				instance.Locales.Add(ProtocolParser.ReadString(stream));
				continue;
			case 24:
				instance.Success = ProtocolParser.ReadBool(stream);
				continue;
			case 42:
				instance.Errors = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, ClearInactiveLocales instance)
	{
		if (instance.Locales.Count > 0)
		{
			foreach (string i1 in instance.Locales)
			{
				stream.WriteByte(10);
				ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(i1));
			}
		}
		if (instance.HasSuccess)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.Success);
		}
		if (instance.HasErrors)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Errors));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (Locales.Count > 0)
		{
			foreach (string i1 in Locales)
			{
				size++;
				uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(i1);
				size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
			}
		}
		if (HasSuccess)
		{
			size++;
			size++;
		}
		if (HasErrors)
		{
			size++;
			uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(Errors);
			size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
		}
		return size;
	}
}
