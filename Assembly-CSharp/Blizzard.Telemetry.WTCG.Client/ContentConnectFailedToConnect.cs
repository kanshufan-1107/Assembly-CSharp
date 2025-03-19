using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class ContentConnectFailedToConnect : IProtoBuf
{
	public bool HasDeviceInfo;

	private DeviceInfo _DeviceInfo;

	public bool HasUrl;

	private string _Url;

	public bool HasHttpErrorcode;

	private int _HttpErrorcode;

	public bool HasServerErrorcode;

	private int _ServerErrorcode;

	public DeviceInfo DeviceInfo
	{
		get
		{
			return _DeviceInfo;
		}
		set
		{
			_DeviceInfo = value;
			HasDeviceInfo = value != null;
		}
	}

	public string Url
	{
		get
		{
			return _Url;
		}
		set
		{
			_Url = value;
			HasUrl = value != null;
		}
	}

	public int HttpErrorcode
	{
		get
		{
			return _HttpErrorcode;
		}
		set
		{
			_HttpErrorcode = value;
			HasHttpErrorcode = true;
		}
	}

	public int ServerErrorcode
	{
		get
		{
			return _ServerErrorcode;
		}
		set
		{
			_ServerErrorcode = value;
			HasServerErrorcode = true;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasDeviceInfo)
		{
			hash ^= DeviceInfo.GetHashCode();
		}
		if (HasUrl)
		{
			hash ^= Url.GetHashCode();
		}
		if (HasHttpErrorcode)
		{
			hash ^= HttpErrorcode.GetHashCode();
		}
		if (HasServerErrorcode)
		{
			hash ^= ServerErrorcode.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ContentConnectFailedToConnect other))
		{
			return false;
		}
		if (HasDeviceInfo != other.HasDeviceInfo || (HasDeviceInfo && !DeviceInfo.Equals(other.DeviceInfo)))
		{
			return false;
		}
		if (HasUrl != other.HasUrl || (HasUrl && !Url.Equals(other.Url)))
		{
			return false;
		}
		if (HasHttpErrorcode != other.HasHttpErrorcode || (HasHttpErrorcode && !HttpErrorcode.Equals(other.HttpErrorcode)))
		{
			return false;
		}
		if (HasServerErrorcode != other.HasServerErrorcode || (HasServerErrorcode && !ServerErrorcode.Equals(other.ServerErrorcode)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static ContentConnectFailedToConnect Deserialize(Stream stream, ContentConnectFailedToConnect instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static ContentConnectFailedToConnect DeserializeLengthDelimited(Stream stream)
	{
		ContentConnectFailedToConnect instance = new ContentConnectFailedToConnect();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static ContentConnectFailedToConnect DeserializeLengthDelimited(Stream stream, ContentConnectFailedToConnect instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static ContentConnectFailedToConnect Deserialize(Stream stream, ContentConnectFailedToConnect instance, long limit)
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
				if (instance.DeviceInfo == null)
				{
					instance.DeviceInfo = DeviceInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					DeviceInfo.DeserializeLengthDelimited(stream, instance.DeviceInfo);
				}
				continue;
			case 18:
				instance.Url = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				instance.HttpErrorcode = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				instance.ServerErrorcode = (int)ProtocolParser.ReadUInt64(stream);
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

	public static void Serialize(Stream stream, ContentConnectFailedToConnect instance)
	{
		if (instance.HasDeviceInfo)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.DeviceInfo.GetSerializedSize());
			DeviceInfo.Serialize(stream, instance.DeviceInfo);
		}
		if (instance.HasUrl)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Url));
		}
		if (instance.HasHttpErrorcode)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.HttpErrorcode);
		}
		if (instance.HasServerErrorcode)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ServerErrorcode);
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasDeviceInfo)
		{
			size++;
			uint size2 = DeviceInfo.GetSerializedSize();
			size += size2 + ProtocolParser.SizeOfUInt32(size2);
		}
		if (HasUrl)
		{
			size++;
			uint byteCount2 = (uint)Encoding.UTF8.GetByteCount(Url);
			size += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
		}
		if (HasHttpErrorcode)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)HttpErrorcode);
		}
		if (HasServerErrorcode)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)ServerErrorcode);
		}
		return size;
	}
}
