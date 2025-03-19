using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class PushRegistrationFailed : IProtoBuf
{
	public bool HasError;

	private string _Error;

	public string Error
	{
		get
		{
			return _Error;
		}
		set
		{
			_Error = value;
			HasError = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasError)
		{
			hash ^= Error.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PushRegistrationFailed other))
		{
			return false;
		}
		if (HasError != other.HasError || (HasError && !Error.Equals(other.Error)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static PushRegistrationFailed Deserialize(Stream stream, PushRegistrationFailed instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static PushRegistrationFailed DeserializeLengthDelimited(Stream stream)
	{
		PushRegistrationFailed instance = new PushRegistrationFailed();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static PushRegistrationFailed DeserializeLengthDelimited(Stream stream, PushRegistrationFailed instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static PushRegistrationFailed Deserialize(Stream stream, PushRegistrationFailed instance, long limit)
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
			case 82:
				instance.Error = ProtocolParser.ReadString(stream);
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

	public static void Serialize(Stream stream, PushRegistrationFailed instance)
	{
		if (instance.HasError)
		{
			stream.WriteByte(82);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Error));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasError)
		{
			size++;
			uint byteCount10 = (uint)Encoding.UTF8.GetByteCount(Error);
			size += ProtocolParser.SizeOfUInt32(byteCount10) + byteCount10;
		}
		return size;
	}
}
