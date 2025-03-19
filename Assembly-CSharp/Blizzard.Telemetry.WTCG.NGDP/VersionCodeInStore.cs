using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.NGDP;

public class VersionCodeInStore : IProtoBuf
{
	public bool HasVersionCode;

	private string _VersionCode;

	public bool HasCountryCode;

	private string _CountryCode;

	public string VersionCode
	{
		get
		{
			return _VersionCode;
		}
		set
		{
			_VersionCode = value;
			HasVersionCode = value != null;
		}
	}

	public string CountryCode
	{
		get
		{
			return _CountryCode;
		}
		set
		{
			_CountryCode = value;
			HasCountryCode = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasVersionCode)
		{
			hash ^= VersionCode.GetHashCode();
		}
		if (HasCountryCode)
		{
			hash ^= CountryCode.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is VersionCodeInStore other))
		{
			return false;
		}
		if (HasVersionCode != other.HasVersionCode || (HasVersionCode && !VersionCode.Equals(other.VersionCode)))
		{
			return false;
		}
		if (HasCountryCode != other.HasCountryCode || (HasCountryCode && !CountryCode.Equals(other.CountryCode)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static VersionCodeInStore Deserialize(Stream stream, VersionCodeInStore instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static VersionCodeInStore DeserializeLengthDelimited(Stream stream)
	{
		VersionCodeInStore instance = new VersionCodeInStore();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static VersionCodeInStore DeserializeLengthDelimited(Stream stream, VersionCodeInStore instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static VersionCodeInStore Deserialize(Stream stream, VersionCodeInStore instance, long limit)
	{
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
				instance.VersionCode = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				instance.CountryCode = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, VersionCodeInStore instance)
	{
		if (instance.HasVersionCode)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.VersionCode));
		}
		if (instance.HasCountryCode)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.CountryCode));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasVersionCode)
		{
			size++;
			uint byteCount1 = (uint)Encoding.UTF8.GetByteCount(VersionCode);
			size += ProtocolParser.SizeOfUInt32(byteCount1) + byteCount1;
		}
		if (HasCountryCode)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(CountryCode);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		return size;
	}
}
