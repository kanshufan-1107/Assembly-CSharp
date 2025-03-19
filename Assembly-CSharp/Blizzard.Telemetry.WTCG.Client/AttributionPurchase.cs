using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class AttributionPurchase : IProtoBuf
{
	public class PaymentInfo : IProtoBuf
	{
		public bool HasCurrencyCode;

		private string _CurrencyCode;

		public bool HasIsVirtualCurrency;

		private bool _IsVirtualCurrency;

		public bool HasAmount;

		private float _Amount;

		public string CurrencyCode
		{
			get
			{
				return _CurrencyCode;
			}
			set
			{
				_CurrencyCode = value;
				HasCurrencyCode = value != null;
			}
		}

		public bool IsVirtualCurrency
		{
			get
			{
				return _IsVirtualCurrency;
			}
			set
			{
				_IsVirtualCurrency = value;
				HasIsVirtualCurrency = true;
			}
		}

		public float Amount
		{
			get
			{
				return _Amount;
			}
			set
			{
				_Amount = value;
				HasAmount = true;
			}
		}

		public override int GetHashCode()
		{
			int hash = GetType().GetHashCode();
			if (HasCurrencyCode)
			{
				hash ^= CurrencyCode.GetHashCode();
			}
			if (HasIsVirtualCurrency)
			{
				hash ^= IsVirtualCurrency.GetHashCode();
			}
			if (HasAmount)
			{
				hash ^= Amount.GetHashCode();
			}
			return hash;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is PaymentInfo other))
			{
				return false;
			}
			if (HasCurrencyCode != other.HasCurrencyCode || (HasCurrencyCode && !CurrencyCode.Equals(other.CurrencyCode)))
			{
				return false;
			}
			if (HasIsVirtualCurrency != other.HasIsVirtualCurrency || (HasIsVirtualCurrency && !IsVirtualCurrency.Equals(other.IsVirtualCurrency)))
			{
				return false;
			}
			if (HasAmount != other.HasAmount || (HasAmount && !Amount.Equals(other.Amount)))
			{
				return false;
			}
			return true;
		}

		public void Deserialize(Stream stream)
		{
			Deserialize(stream, this);
		}

		public static PaymentInfo Deserialize(Stream stream, PaymentInfo instance)
		{
			return Deserialize(stream, instance, -1L);
		}

		public static PaymentInfo DeserializeLengthDelimited(Stream stream)
		{
			PaymentInfo instance = new PaymentInfo();
			DeserializeLengthDelimited(stream, instance);
			return instance;
		}

		public static PaymentInfo DeserializeLengthDelimited(Stream stream, PaymentInfo instance)
		{
			long limit = ProtocolParser.ReadUInt32(stream);
			limit += stream.Position;
			return Deserialize(stream, instance, limit);
		}

		public static PaymentInfo Deserialize(Stream stream, PaymentInfo instance, long limit)
		{
			BinaryReader br = new BinaryReader(stream);
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
					instance.CurrencyCode = ProtocolParser.ReadString(stream);
					continue;
				default:
				{
					Key key = ProtocolParser.ReadKey((byte)keyByte, stream);
					switch (key.Field)
					{
					case 0u:
						throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
					case 20u:
						if (key.WireType == Wire.Varint)
						{
							instance.IsVirtualCurrency = ProtocolParser.ReadBool(stream);
						}
						break;
					case 30u:
						if (key.WireType == Wire.Fixed32)
						{
							instance.Amount = br.ReadSingle();
						}
						break;
					default:
						ProtocolParser.SkipKey(stream, key);
						break;
					}
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

		public static void Serialize(Stream stream, PaymentInfo instance)
		{
			BinaryWriter bw = new BinaryWriter(stream);
			if (instance.HasCurrencyCode)
			{
				stream.WriteByte(82);
				ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.CurrencyCode));
			}
			if (instance.HasIsVirtualCurrency)
			{
				stream.WriteByte(160);
				stream.WriteByte(1);
				ProtocolParser.WriteBool(stream, instance.IsVirtualCurrency);
			}
			if (instance.HasAmount)
			{
				stream.WriteByte(245);
				stream.WriteByte(1);
				bw.Write(instance.Amount);
			}
		}

		public uint GetSerializedSize()
		{
			uint size = 0u;
			if (HasCurrencyCode)
			{
				size++;
				uint byteCount10 = (uint)Encoding.UTF8.GetByteCount(CurrencyCode);
				size += ProtocolParser.SizeOfUInt32(byteCount10) + byteCount10;
			}
			if (HasIsVirtualCurrency)
			{
				size += 2;
				size++;
			}
			if (HasAmount)
			{
				size += 2;
				size += 4;
			}
			return size;
		}
	}

	public bool HasApplicationId;

	private string _ApplicationId;

	public bool HasDeviceType;

	private string _DeviceType;

	public bool HasFirstInstallDate;

	private ulong _FirstInstallDate;

	public bool HasBundleId;

	private string _BundleId;

	public bool HasPurchaseType;

	private string _PurchaseType;

	public bool HasTransactionId;

	private string _TransactionId;

	public bool HasQuantity;

	private int _Quantity;

	private List<PaymentInfo> _Payments = new List<PaymentInfo>();

	public bool HasAmount;

	private float _Amount;

	public bool HasCurrency;

	private string _Currency;

	public string ApplicationId
	{
		get
		{
			return _ApplicationId;
		}
		set
		{
			_ApplicationId = value;
			HasApplicationId = value != null;
		}
	}

	public string DeviceType
	{
		get
		{
			return _DeviceType;
		}
		set
		{
			_DeviceType = value;
			HasDeviceType = value != null;
		}
	}

	public ulong FirstInstallDate
	{
		get
		{
			return _FirstInstallDate;
		}
		set
		{
			_FirstInstallDate = value;
			HasFirstInstallDate = true;
		}
	}

	public string BundleId
	{
		get
		{
			return _BundleId;
		}
		set
		{
			_BundleId = value;
			HasBundleId = value != null;
		}
	}

	public string PurchaseType
	{
		get
		{
			return _PurchaseType;
		}
		set
		{
			_PurchaseType = value;
			HasPurchaseType = value != null;
		}
	}

	public string TransactionId
	{
		get
		{
			return _TransactionId;
		}
		set
		{
			_TransactionId = value;
			HasTransactionId = value != null;
		}
	}

	public int Quantity
	{
		get
		{
			return _Quantity;
		}
		set
		{
			_Quantity = value;
			HasQuantity = true;
		}
	}

	public List<PaymentInfo> Payments
	{
		get
		{
			return _Payments;
		}
		set
		{
			_Payments = value;
		}
	}

	public float Amount
	{
		get
		{
			return _Amount;
		}
		set
		{
			_Amount = value;
			HasAmount = true;
		}
	}

	public string Currency
	{
		get
		{
			return _Currency;
		}
		set
		{
			_Currency = value;
			HasCurrency = value != null;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasApplicationId)
		{
			hash ^= ApplicationId.GetHashCode();
		}
		if (HasDeviceType)
		{
			hash ^= DeviceType.GetHashCode();
		}
		if (HasFirstInstallDate)
		{
			hash ^= FirstInstallDate.GetHashCode();
		}
		if (HasBundleId)
		{
			hash ^= BundleId.GetHashCode();
		}
		if (HasPurchaseType)
		{
			hash ^= PurchaseType.GetHashCode();
		}
		if (HasTransactionId)
		{
			hash ^= TransactionId.GetHashCode();
		}
		if (HasQuantity)
		{
			hash ^= Quantity.GetHashCode();
		}
		foreach (PaymentInfo i in Payments)
		{
			hash ^= i.GetHashCode();
		}
		if (HasAmount)
		{
			hash ^= Amount.GetHashCode();
		}
		if (HasCurrency)
		{
			hash ^= Currency.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is AttributionPurchase other))
		{
			return false;
		}
		if (HasApplicationId != other.HasApplicationId || (HasApplicationId && !ApplicationId.Equals(other.ApplicationId)))
		{
			return false;
		}
		if (HasDeviceType != other.HasDeviceType || (HasDeviceType && !DeviceType.Equals(other.DeviceType)))
		{
			return false;
		}
		if (HasFirstInstallDate != other.HasFirstInstallDate || (HasFirstInstallDate && !FirstInstallDate.Equals(other.FirstInstallDate)))
		{
			return false;
		}
		if (HasBundleId != other.HasBundleId || (HasBundleId && !BundleId.Equals(other.BundleId)))
		{
			return false;
		}
		if (HasPurchaseType != other.HasPurchaseType || (HasPurchaseType && !PurchaseType.Equals(other.PurchaseType)))
		{
			return false;
		}
		if (HasTransactionId != other.HasTransactionId || (HasTransactionId && !TransactionId.Equals(other.TransactionId)))
		{
			return false;
		}
		if (HasQuantity != other.HasQuantity || (HasQuantity && !Quantity.Equals(other.Quantity)))
		{
			return false;
		}
		if (Payments.Count != other.Payments.Count)
		{
			return false;
		}
		for (int i = 0; i < Payments.Count; i++)
		{
			if (!Payments[i].Equals(other.Payments[i]))
			{
				return false;
			}
		}
		if (HasAmount != other.HasAmount || (HasAmount && !Amount.Equals(other.Amount)))
		{
			return false;
		}
		if (HasCurrency != other.HasCurrency || (HasCurrency && !Currency.Equals(other.Currency)))
		{
			return false;
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static AttributionPurchase Deserialize(Stream stream, AttributionPurchase instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static AttributionPurchase DeserializeLengthDelimited(Stream stream)
	{
		AttributionPurchase instance = new AttributionPurchase();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static AttributionPurchase DeserializeLengthDelimited(Stream stream, AttributionPurchase instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static AttributionPurchase Deserialize(Stream stream, AttributionPurchase instance, long limit)
	{
		BinaryReader br = new BinaryReader(stream);
		if (instance.Payments == null)
		{
			instance.Payments = new List<PaymentInfo>();
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
				instance.PurchaseType = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				instance.TransactionId = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				instance.Quantity = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 50:
				instance.Payments.Add(PaymentInfo.DeserializeLengthDelimited(stream));
				continue;
			case 21:
				instance.Amount = br.ReadSingle();
				continue;
			case 26:
				instance.Currency = ProtocolParser.ReadString(stream);
				continue;
			default:
			{
				Key key = ProtocolParser.ReadKey((byte)keyByte, stream);
				switch (key.Field)
				{
				case 0u:
					throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
				case 100u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.ApplicationId = ProtocolParser.ReadString(stream);
					}
					break;
				case 101u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.DeviceType = ProtocolParser.ReadString(stream);
					}
					break;
				case 102u:
					if (key.WireType == Wire.Varint)
					{
						instance.FirstInstallDate = ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 103u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.BundleId = ProtocolParser.ReadString(stream);
					}
					break;
				default:
					ProtocolParser.SkipKey(stream, key);
					break;
				}
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

	public static void Serialize(Stream stream, AttributionPurchase instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasApplicationId)
		{
			stream.WriteByte(162);
			stream.WriteByte(6);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ApplicationId));
		}
		if (instance.HasDeviceType)
		{
			stream.WriteByte(170);
			stream.WriteByte(6);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DeviceType));
		}
		if (instance.HasFirstInstallDate)
		{
			stream.WriteByte(176);
			stream.WriteByte(6);
			ProtocolParser.WriteUInt64(stream, instance.FirstInstallDate);
		}
		if (instance.HasBundleId)
		{
			stream.WriteByte(186);
			stream.WriteByte(6);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.BundleId));
		}
		if (instance.HasPurchaseType)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.PurchaseType));
		}
		if (instance.HasTransactionId)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.TransactionId));
		}
		if (instance.HasQuantity)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.Quantity);
		}
		if (instance.Payments.Count > 0)
		{
			foreach (PaymentInfo i6 in instance.Payments)
			{
				stream.WriteByte(50);
				ProtocolParser.WriteUInt32(stream, i6.GetSerializedSize());
				PaymentInfo.Serialize(stream, i6);
			}
		}
		if (instance.HasAmount)
		{
			stream.WriteByte(21);
			bw.Write(instance.Amount);
		}
		if (instance.HasCurrency)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.Currency));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasApplicationId)
		{
			size += 2;
			uint byteCount100 = (uint)Encoding.UTF8.GetByteCount(ApplicationId);
			size += ProtocolParser.SizeOfUInt32(byteCount100) + byteCount100;
		}
		if (HasDeviceType)
		{
			size += 2;
			uint byteCount101 = (uint)Encoding.UTF8.GetByteCount(DeviceType);
			size += ProtocolParser.SizeOfUInt32(byteCount101) + byteCount101;
		}
		if (HasFirstInstallDate)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64(FirstInstallDate);
		}
		if (HasBundleId)
		{
			size += 2;
			uint byteCount103 = (uint)Encoding.UTF8.GetByteCount(BundleId);
			size += ProtocolParser.SizeOfUInt32(byteCount103) + byteCount103;
		}
		if (HasPurchaseType)
		{
			size++;
			uint byteCount104 = (uint)Encoding.UTF8.GetByteCount(PurchaseType);
			size += ProtocolParser.SizeOfUInt32(byteCount104) + byteCount104;
		}
		if (HasTransactionId)
		{
			size++;
			uint byteCount105 = (uint)Encoding.UTF8.GetByteCount(TransactionId);
			size += ProtocolParser.SizeOfUInt32(byteCount105) + byteCount105;
		}
		if (HasQuantity)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)Quantity);
		}
		if (Payments.Count > 0)
		{
			foreach (PaymentInfo payment in Payments)
			{
				size++;
				uint size6 = payment.GetSerializedSize();
				size += size6 + ProtocolParser.SizeOfUInt32(size6);
			}
		}
		if (HasAmount)
		{
			size++;
			size += 4;
		}
		if (HasCurrency)
		{
			size++;
			uint byteCount106 = (uint)Encoding.UTF8.GetByteCount(Currency);
			size += ProtocolParser.SizeOfUInt32(byteCount106) + byteCount106;
		}
		return size;
	}
}
