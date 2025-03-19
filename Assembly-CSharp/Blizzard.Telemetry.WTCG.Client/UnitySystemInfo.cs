using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blizzard.Telemetry.WTCG.Client;

public class UnitySystemInfo : IProtoBuf
{
	public enum BatteryStatusEnum
	{
		UnknownStatus = 1,
		Charging,
		Discharging,
		NotCharging,
		FullCharged
	}

	public enum CopyTextureSupportEnum
	{
		None = 1,
		Basic = 2,
		Copy3D = 3,
		DifferentTypes = 4,
		TextureToRT = 9,
		RTToTexture = 17
	}

	public enum DeviceTypeEnum
	{
		Unknown = 1,
		Handheld,
		Console,
		Desktop
	}

	public enum GraphicsDeviceTypeEnum
	{
		OpenGL2 = 1,
		Direct3D9 = 2,
		Direct3D11 = 3,
		PlayStation3 = 4,
		Null = 5,
		XBox360 = 7,
		OpenGLES2 = 9,
		OpenGLES3 = 12,
		PlayStationVita = 13,
		PlayStation4 = 14,
		XboxOne = 15,
		PlayStationMobile = 16,
		Metal = 17,
		OpenGLCore = 18,
		Direct3D12 = 19,
		N3DS = 20,
		Vulkan = 22,
		Switch = 23,
		XboxOneD3D12 = 24,
		GameCoreXboxOne = 25,
		GameCoreXboxSeries = 26,
		PlayStation5 = 27
	}

	public enum NPOTSupportEnum
	{
		NotSupported = 1,
		Restricted,
		Full
	}

	public enum OperatingSystemFamilyEnum
	{
		Other = 1,
		MacOSX,
		Windows,
		Linux
	}

	public enum RenderingThreadingModeEnum
	{
		Direct = 1,
		SingleThreaded,
		MultiThreaded,
		LegacyJobified,
		NativeGraphicsJobs,
		NativeGraphicsJobsWithoutRenderThread
	}

	public bool HasBatteryLevel;

	private float _BatteryLevel;

	public bool HasBatteryStatus;

	private BatteryStatusEnum _BatteryStatus;

	public bool HasCopyTextureSupport;

	private CopyTextureSupportEnum _CopyTextureSupport;

	public bool HasDeviceModel;

	private string _DeviceModel;

	public bool HasDeviceName;

	private string _DeviceName;

	public bool HasDeviceType;

	private DeviceTypeEnum _DeviceType;

	public bool HasDeviceUniqueIdentifier;

	private string _DeviceUniqueIdentifier;

	public bool HasGraphicsDeviceID;

	private long _GraphicsDeviceID;

	public bool HasGraphicsDeviceName;

	private string _GraphicsDeviceName;

	public bool HasGraphicsDeviceType;

	private GraphicsDeviceTypeEnum _GraphicsDeviceType;

	public bool HasGraphicsDeviceVendor;

	private string _GraphicsDeviceVendor;

	public bool HasGraphicsDeviceVendorID;

	private long _GraphicsDeviceVendorID;

	public bool HasGraphicsDeviceVersion;

	private string _GraphicsDeviceVersion;

	public bool HasGraphicsMemorySize;

	private int _GraphicsMemorySize;

	public bool HasGraphicsMultiThreaded;

	private bool _GraphicsMultiThreaded;

	public bool HasGraphicsShaderLevel;

	private int _GraphicsShaderLevel;

	public bool HasGraphicsUVStartsAtTop;

	private bool _GraphicsUVStartsAtTop;

	public bool HasHasDynamicUniformArrayIndexingInFragmentShaders;

	private bool _HasDynamicUniformArrayIndexingInFragmentShaders;

	public bool HasHasHiddenSurfaceRemovalOnGPU;

	private bool _HasHiddenSurfaceRemovalOnGPU;

	public bool HasHasMipMaxLevel;

	private bool _HasMipMaxLevel;

	public bool HasMaxComputeBufferInputsCompute;

	private int _MaxComputeBufferInputsCompute;

	public bool HasMaxComputeBufferInputsDomain;

	private int _MaxComputeBufferInputsDomain;

	public bool HasMaxComputeBufferInputsFragment;

	private int _MaxComputeBufferInputsFragment;

	public bool HasMaxComputeBufferInputsGeometry;

	private int _MaxComputeBufferInputsGeometry;

	public bool HasMaxComputeBufferInputsHull;

	private int _MaxComputeBufferInputsHull;

	public bool HasMaxComputeBufferInputsVertex;

	private int _MaxComputeBufferInputsVertex;

	public bool HasMaxComputeWorkGroupSize;

	private int _MaxComputeWorkGroupSize;

	public bool HasMaxComputeWorkGroupSizeX;

	private int _MaxComputeWorkGroupSizeX;

	public bool HasMaxComputeWorkGroupSizeY;

	private int _MaxComputeWorkGroupSizeY;

	public bool HasMaxComputeWorkGroupSizeZ;

	private int _MaxComputeWorkGroupSizeZ;

	public bool HasMaxCubemapSize;

	private int _MaxCubemapSize;

	public bool HasMaxTextureSize;

	private int _MaxTextureSize;

	public bool HasMinConstantBufferOffsetAlignment;

	private bool _MinConstantBufferOffsetAlignment;

	public bool HasNpotSupport;

	private NPOTSupportEnum _NpotSupport;

	public bool HasOperatingSystem;

	private string _OperatingSystem;

	public bool HasOperatingSystemFamily;

	private OperatingSystemFamilyEnum _OperatingSystemFamily;

	public bool HasProcessorCount;

	private int _ProcessorCount;

	public bool HasProcessorFrequency;

	private int _ProcessorFrequency;

	public bool HasProcessorType;

	private string _ProcessorType;

	public bool HasRenderingThreadingMode;

	private RenderingThreadingModeEnum _RenderingThreadingMode;

	public bool HasSupportedRandomWriteTargetCount;

	private int _SupportedRandomWriteTargetCount;

	public bool HasSupportedRenderTargetCount;

	private int _SupportedRenderTargetCount;

	public bool HasSupports2DArrayTextures;

	private bool _Supports2DArrayTextures;

	public bool HasSupports32bitsIndexBuffer;

	private bool _Supports32bitsIndexBuffer;

	public bool HasSupports3DRenderTextures;

	private bool _Supports3DRenderTextures;

	public bool HasSupports3DTextures;

	private bool _Supports3DTextures;

	public bool HasSupportsAccelerometer;

	private bool _SupportsAccelerometer;

	public bool HasSupportsAsyncCompute;

	private bool _SupportsAsyncCompute;

	public bool HasSupportsAsyncGPUReadback;

	private bool _SupportsAsyncGPUReadback;

	public bool HasSupportsAudio;

	private bool _SupportsAudio;

	public bool HasSupportsComputeShaders;

	private bool _SupportsComputeShaders;

	public bool HasSupportsCubemapArrayTextures;

	private bool _SupportsCubemapArrayTextures;

	public bool HasSupportsGeometryShaders;

	private bool _SupportsGeometryShaders;

	public bool HasSupportsGraphicsFence;

	private bool _SupportsGraphicsFence;

	public bool HasSupportsGyroscope;

	private bool _SupportsGyroscope;

	public bool HasSupportsHardwareQuadTopology;

	private bool _SupportsHardwareQuadTopology;

	public bool HasSupportsInstancing;

	private bool _SupportsInstancing;

	public bool HasSupportsLocationService;

	private bool _SupportsLocationService;

	public bool HasSupportsMipStreaming;

	private bool _SupportsMipStreaming;

	public bool HasSupportsMotionVectors;

	private bool _SupportsMotionVectors;

	public bool HasSupportsMultisampleAutoResolve;

	private bool _SupportsMultisampleAutoResolve;

	public bool HasSupportsMultisampledTextures;

	private bool _SupportsMultisampledTextures;

	public bool HasSupportsRawShadowDepthSampling;

	private bool _SupportsRawShadowDepthSampling;

	public bool HasSupportsRayTracing;

	private bool _SupportsRayTracing;

	public bool HasSupportsSeparatedRenderTargetsBlend;

	private bool _SupportsSeparatedRenderTargetsBlend;

	public bool HasSupportsSetConstantBuffer;

	private bool _SupportsSetConstantBuffer;

	public bool HasSupportsShadows;

	private bool _SupportsShadows;

	public bool HasSupportsSparseTextures;

	private bool _SupportsSparseTextures;

	public bool HasSupportsStoreAndResolveAction;

	private bool _SupportsStoreAndResolveAction;

	public bool HasSupportsTessellationShaders;

	private bool _SupportsTessellationShaders;

	public bool HasSupportsTextureWrapMirrorOnce;

	private bool _SupportsTextureWrapMirrorOnce;

	public bool HasSupportsVibration;

	private bool _SupportsVibration;

	public bool HasSystemMemorySize;

	private int _SystemMemorySize;

	public bool HasUnsupportedIdentifier;

	private string _UnsupportedIdentifier;

	public bool HasUsesLoadStoreActions;

	private bool _UsesLoadStoreActions;

	public bool HasUsesReversedZBuffer;

	private bool _UsesReversedZBuffer;

	public bool HasGraphicsFormatForLDR;

	private string _GraphicsFormatForLDR;

	public bool HasGraphicsFormatForHDR;

	private string _GraphicsFormatForHDR;

	private List<string> _SupportedRenderTextureFormats = new List<string>();

	private List<string> _SupportedTextureFormats = new List<string>();

	public float BatteryLevel
	{
		get
		{
			return _BatteryLevel;
		}
		set
		{
			_BatteryLevel = value;
			HasBatteryLevel = true;
		}
	}

	public BatteryStatusEnum BatteryStatus
	{
		get
		{
			return _BatteryStatus;
		}
		set
		{
			_BatteryStatus = value;
			HasBatteryStatus = true;
		}
	}

	public CopyTextureSupportEnum CopyTextureSupport
	{
		get
		{
			return _CopyTextureSupport;
		}
		set
		{
			_CopyTextureSupport = value;
			HasCopyTextureSupport = true;
		}
	}

	public string DeviceModel
	{
		get
		{
			return _DeviceModel;
		}
		set
		{
			_DeviceModel = value;
			HasDeviceModel = value != null;
		}
	}

	public string DeviceName
	{
		get
		{
			return _DeviceName;
		}
		set
		{
			_DeviceName = value;
			HasDeviceName = value != null;
		}
	}

	public DeviceTypeEnum DeviceType
	{
		get
		{
			return _DeviceType;
		}
		set
		{
			_DeviceType = value;
			HasDeviceType = true;
		}
	}

	public string DeviceUniqueIdentifier
	{
		get
		{
			return _DeviceUniqueIdentifier;
		}
		set
		{
			_DeviceUniqueIdentifier = value;
			HasDeviceUniqueIdentifier = value != null;
		}
	}

	public long GraphicsDeviceID
	{
		get
		{
			return _GraphicsDeviceID;
		}
		set
		{
			_GraphicsDeviceID = value;
			HasGraphicsDeviceID = true;
		}
	}

	public string GraphicsDeviceName
	{
		get
		{
			return _GraphicsDeviceName;
		}
		set
		{
			_GraphicsDeviceName = value;
			HasGraphicsDeviceName = value != null;
		}
	}

	public GraphicsDeviceTypeEnum GraphicsDeviceType
	{
		get
		{
			return _GraphicsDeviceType;
		}
		set
		{
			_GraphicsDeviceType = value;
			HasGraphicsDeviceType = true;
		}
	}

	public string GraphicsDeviceVendor
	{
		get
		{
			return _GraphicsDeviceVendor;
		}
		set
		{
			_GraphicsDeviceVendor = value;
			HasGraphicsDeviceVendor = value != null;
		}
	}

	public long GraphicsDeviceVendorID
	{
		get
		{
			return _GraphicsDeviceVendorID;
		}
		set
		{
			_GraphicsDeviceVendorID = value;
			HasGraphicsDeviceVendorID = true;
		}
	}

	public string GraphicsDeviceVersion
	{
		get
		{
			return _GraphicsDeviceVersion;
		}
		set
		{
			_GraphicsDeviceVersion = value;
			HasGraphicsDeviceVersion = value != null;
		}
	}

	public int GraphicsMemorySize
	{
		get
		{
			return _GraphicsMemorySize;
		}
		set
		{
			_GraphicsMemorySize = value;
			HasGraphicsMemorySize = true;
		}
	}

	public bool GraphicsMultiThreaded
	{
		get
		{
			return _GraphicsMultiThreaded;
		}
		set
		{
			_GraphicsMultiThreaded = value;
			HasGraphicsMultiThreaded = true;
		}
	}

	public int GraphicsShaderLevel
	{
		get
		{
			return _GraphicsShaderLevel;
		}
		set
		{
			_GraphicsShaderLevel = value;
			HasGraphicsShaderLevel = true;
		}
	}

	public bool GraphicsUVStartsAtTop
	{
		get
		{
			return _GraphicsUVStartsAtTop;
		}
		set
		{
			_GraphicsUVStartsAtTop = value;
			HasGraphicsUVStartsAtTop = true;
		}
	}

	public bool HasDynamicUniformArrayIndexingInFragmentShaders
	{
		get
		{
			return _HasDynamicUniformArrayIndexingInFragmentShaders;
		}
		set
		{
			_HasDynamicUniformArrayIndexingInFragmentShaders = value;
			HasHasDynamicUniformArrayIndexingInFragmentShaders = true;
		}
	}

	public bool HasHiddenSurfaceRemovalOnGPU
	{
		get
		{
			return _HasHiddenSurfaceRemovalOnGPU;
		}
		set
		{
			_HasHiddenSurfaceRemovalOnGPU = value;
			HasHasHiddenSurfaceRemovalOnGPU = true;
		}
	}

	public bool HasMipMaxLevel
	{
		get
		{
			return _HasMipMaxLevel;
		}
		set
		{
			_HasMipMaxLevel = value;
			HasHasMipMaxLevel = true;
		}
	}

	public int MaxComputeBufferInputsCompute
	{
		get
		{
			return _MaxComputeBufferInputsCompute;
		}
		set
		{
			_MaxComputeBufferInputsCompute = value;
			HasMaxComputeBufferInputsCompute = true;
		}
	}

	public int MaxComputeBufferInputsDomain
	{
		get
		{
			return _MaxComputeBufferInputsDomain;
		}
		set
		{
			_MaxComputeBufferInputsDomain = value;
			HasMaxComputeBufferInputsDomain = true;
		}
	}

	public int MaxComputeBufferInputsFragment
	{
		get
		{
			return _MaxComputeBufferInputsFragment;
		}
		set
		{
			_MaxComputeBufferInputsFragment = value;
			HasMaxComputeBufferInputsFragment = true;
		}
	}

	public int MaxComputeBufferInputsGeometry
	{
		get
		{
			return _MaxComputeBufferInputsGeometry;
		}
		set
		{
			_MaxComputeBufferInputsGeometry = value;
			HasMaxComputeBufferInputsGeometry = true;
		}
	}

	public int MaxComputeBufferInputsHull
	{
		get
		{
			return _MaxComputeBufferInputsHull;
		}
		set
		{
			_MaxComputeBufferInputsHull = value;
			HasMaxComputeBufferInputsHull = true;
		}
	}

	public int MaxComputeBufferInputsVertex
	{
		get
		{
			return _MaxComputeBufferInputsVertex;
		}
		set
		{
			_MaxComputeBufferInputsVertex = value;
			HasMaxComputeBufferInputsVertex = true;
		}
	}

	public int MaxComputeWorkGroupSize
	{
		get
		{
			return _MaxComputeWorkGroupSize;
		}
		set
		{
			_MaxComputeWorkGroupSize = value;
			HasMaxComputeWorkGroupSize = true;
		}
	}

	public int MaxComputeWorkGroupSizeX
	{
		get
		{
			return _MaxComputeWorkGroupSizeX;
		}
		set
		{
			_MaxComputeWorkGroupSizeX = value;
			HasMaxComputeWorkGroupSizeX = true;
		}
	}

	public int MaxComputeWorkGroupSizeY
	{
		get
		{
			return _MaxComputeWorkGroupSizeY;
		}
		set
		{
			_MaxComputeWorkGroupSizeY = value;
			HasMaxComputeWorkGroupSizeY = true;
		}
	}

	public int MaxComputeWorkGroupSizeZ
	{
		get
		{
			return _MaxComputeWorkGroupSizeZ;
		}
		set
		{
			_MaxComputeWorkGroupSizeZ = value;
			HasMaxComputeWorkGroupSizeZ = true;
		}
	}

	public int MaxCubemapSize
	{
		get
		{
			return _MaxCubemapSize;
		}
		set
		{
			_MaxCubemapSize = value;
			HasMaxCubemapSize = true;
		}
	}

	public int MaxTextureSize
	{
		get
		{
			return _MaxTextureSize;
		}
		set
		{
			_MaxTextureSize = value;
			HasMaxTextureSize = true;
		}
	}

	public bool MinConstantBufferOffsetAlignment
	{
		get
		{
			return _MinConstantBufferOffsetAlignment;
		}
		set
		{
			_MinConstantBufferOffsetAlignment = value;
			HasMinConstantBufferOffsetAlignment = true;
		}
	}

	public NPOTSupportEnum NpotSupport
	{
		get
		{
			return _NpotSupport;
		}
		set
		{
			_NpotSupport = value;
			HasNpotSupport = true;
		}
	}

	public string OperatingSystem
	{
		get
		{
			return _OperatingSystem;
		}
		set
		{
			_OperatingSystem = value;
			HasOperatingSystem = value != null;
		}
	}

	public OperatingSystemFamilyEnum OperatingSystemFamily
	{
		get
		{
			return _OperatingSystemFamily;
		}
		set
		{
			_OperatingSystemFamily = value;
			HasOperatingSystemFamily = true;
		}
	}

	public int ProcessorCount
	{
		get
		{
			return _ProcessorCount;
		}
		set
		{
			_ProcessorCount = value;
			HasProcessorCount = true;
		}
	}

	public int ProcessorFrequency
	{
		get
		{
			return _ProcessorFrequency;
		}
		set
		{
			_ProcessorFrequency = value;
			HasProcessorFrequency = true;
		}
	}

	public string ProcessorType
	{
		get
		{
			return _ProcessorType;
		}
		set
		{
			_ProcessorType = value;
			HasProcessorType = value != null;
		}
	}

	public RenderingThreadingModeEnum RenderingThreadingMode
	{
		get
		{
			return _RenderingThreadingMode;
		}
		set
		{
			_RenderingThreadingMode = value;
			HasRenderingThreadingMode = true;
		}
	}

	public int SupportedRandomWriteTargetCount
	{
		get
		{
			return _SupportedRandomWriteTargetCount;
		}
		set
		{
			_SupportedRandomWriteTargetCount = value;
			HasSupportedRandomWriteTargetCount = true;
		}
	}

	public int SupportedRenderTargetCount
	{
		get
		{
			return _SupportedRenderTargetCount;
		}
		set
		{
			_SupportedRenderTargetCount = value;
			HasSupportedRenderTargetCount = true;
		}
	}

	public bool Supports2DArrayTextures
	{
		get
		{
			return _Supports2DArrayTextures;
		}
		set
		{
			_Supports2DArrayTextures = value;
			HasSupports2DArrayTextures = true;
		}
	}

	public bool Supports32bitsIndexBuffer
	{
		get
		{
			return _Supports32bitsIndexBuffer;
		}
		set
		{
			_Supports32bitsIndexBuffer = value;
			HasSupports32bitsIndexBuffer = true;
		}
	}

	public bool Supports3DRenderTextures
	{
		get
		{
			return _Supports3DRenderTextures;
		}
		set
		{
			_Supports3DRenderTextures = value;
			HasSupports3DRenderTextures = true;
		}
	}

	public bool Supports3DTextures
	{
		get
		{
			return _Supports3DTextures;
		}
		set
		{
			_Supports3DTextures = value;
			HasSupports3DTextures = true;
		}
	}

	public bool SupportsAccelerometer
	{
		get
		{
			return _SupportsAccelerometer;
		}
		set
		{
			_SupportsAccelerometer = value;
			HasSupportsAccelerometer = true;
		}
	}

	public bool SupportsAsyncCompute
	{
		get
		{
			return _SupportsAsyncCompute;
		}
		set
		{
			_SupportsAsyncCompute = value;
			HasSupportsAsyncCompute = true;
		}
	}

	public bool SupportsAsyncGPUReadback
	{
		get
		{
			return _SupportsAsyncGPUReadback;
		}
		set
		{
			_SupportsAsyncGPUReadback = value;
			HasSupportsAsyncGPUReadback = true;
		}
	}

	public bool SupportsAudio
	{
		get
		{
			return _SupportsAudio;
		}
		set
		{
			_SupportsAudio = value;
			HasSupportsAudio = true;
		}
	}

	public bool SupportsComputeShaders
	{
		get
		{
			return _SupportsComputeShaders;
		}
		set
		{
			_SupportsComputeShaders = value;
			HasSupportsComputeShaders = true;
		}
	}

	public bool SupportsCubemapArrayTextures
	{
		get
		{
			return _SupportsCubemapArrayTextures;
		}
		set
		{
			_SupportsCubemapArrayTextures = value;
			HasSupportsCubemapArrayTextures = true;
		}
	}

	public bool SupportsGeometryShaders
	{
		get
		{
			return _SupportsGeometryShaders;
		}
		set
		{
			_SupportsGeometryShaders = value;
			HasSupportsGeometryShaders = true;
		}
	}

	public bool SupportsGraphicsFence
	{
		get
		{
			return _SupportsGraphicsFence;
		}
		set
		{
			_SupportsGraphicsFence = value;
			HasSupportsGraphicsFence = true;
		}
	}

	public bool SupportsGyroscope
	{
		get
		{
			return _SupportsGyroscope;
		}
		set
		{
			_SupportsGyroscope = value;
			HasSupportsGyroscope = true;
		}
	}

	public bool SupportsHardwareQuadTopology
	{
		get
		{
			return _SupportsHardwareQuadTopology;
		}
		set
		{
			_SupportsHardwareQuadTopology = value;
			HasSupportsHardwareQuadTopology = true;
		}
	}

	public bool SupportsInstancing
	{
		get
		{
			return _SupportsInstancing;
		}
		set
		{
			_SupportsInstancing = value;
			HasSupportsInstancing = true;
		}
	}

	public bool SupportsLocationService
	{
		get
		{
			return _SupportsLocationService;
		}
		set
		{
			_SupportsLocationService = value;
			HasSupportsLocationService = true;
		}
	}

	public bool SupportsMipStreaming
	{
		get
		{
			return _SupportsMipStreaming;
		}
		set
		{
			_SupportsMipStreaming = value;
			HasSupportsMipStreaming = true;
		}
	}

	public bool SupportsMotionVectors
	{
		get
		{
			return _SupportsMotionVectors;
		}
		set
		{
			_SupportsMotionVectors = value;
			HasSupportsMotionVectors = true;
		}
	}

	public bool SupportsMultisampleAutoResolve
	{
		get
		{
			return _SupportsMultisampleAutoResolve;
		}
		set
		{
			_SupportsMultisampleAutoResolve = value;
			HasSupportsMultisampleAutoResolve = true;
		}
	}

	public bool SupportsMultisampledTextures
	{
		get
		{
			return _SupportsMultisampledTextures;
		}
		set
		{
			_SupportsMultisampledTextures = value;
			HasSupportsMultisampledTextures = true;
		}
	}

	public bool SupportsRawShadowDepthSampling
	{
		get
		{
			return _SupportsRawShadowDepthSampling;
		}
		set
		{
			_SupportsRawShadowDepthSampling = value;
			HasSupportsRawShadowDepthSampling = true;
		}
	}

	public bool SupportsRayTracing
	{
		get
		{
			return _SupportsRayTracing;
		}
		set
		{
			_SupportsRayTracing = value;
			HasSupportsRayTracing = true;
		}
	}

	public bool SupportsSeparatedRenderTargetsBlend
	{
		get
		{
			return _SupportsSeparatedRenderTargetsBlend;
		}
		set
		{
			_SupportsSeparatedRenderTargetsBlend = value;
			HasSupportsSeparatedRenderTargetsBlend = true;
		}
	}

	public bool SupportsSetConstantBuffer
	{
		get
		{
			return _SupportsSetConstantBuffer;
		}
		set
		{
			_SupportsSetConstantBuffer = value;
			HasSupportsSetConstantBuffer = true;
		}
	}

	public bool SupportsShadows
	{
		get
		{
			return _SupportsShadows;
		}
		set
		{
			_SupportsShadows = value;
			HasSupportsShadows = true;
		}
	}

	public bool SupportsSparseTextures
	{
		get
		{
			return _SupportsSparseTextures;
		}
		set
		{
			_SupportsSparseTextures = value;
			HasSupportsSparseTextures = true;
		}
	}

	public bool SupportsStoreAndResolveAction
	{
		get
		{
			return _SupportsStoreAndResolveAction;
		}
		set
		{
			_SupportsStoreAndResolveAction = value;
			HasSupportsStoreAndResolveAction = true;
		}
	}

	public bool SupportsTessellationShaders
	{
		get
		{
			return _SupportsTessellationShaders;
		}
		set
		{
			_SupportsTessellationShaders = value;
			HasSupportsTessellationShaders = true;
		}
	}

	public bool SupportsTextureWrapMirrorOnce
	{
		get
		{
			return _SupportsTextureWrapMirrorOnce;
		}
		set
		{
			_SupportsTextureWrapMirrorOnce = value;
			HasSupportsTextureWrapMirrorOnce = true;
		}
	}

	public bool SupportsVibration
	{
		get
		{
			return _SupportsVibration;
		}
		set
		{
			_SupportsVibration = value;
			HasSupportsVibration = true;
		}
	}

	public int SystemMemorySize
	{
		get
		{
			return _SystemMemorySize;
		}
		set
		{
			_SystemMemorySize = value;
			HasSystemMemorySize = true;
		}
	}

	public string UnsupportedIdentifier
	{
		get
		{
			return _UnsupportedIdentifier;
		}
		set
		{
			_UnsupportedIdentifier = value;
			HasUnsupportedIdentifier = value != null;
		}
	}

	public bool UsesLoadStoreActions
	{
		get
		{
			return _UsesLoadStoreActions;
		}
		set
		{
			_UsesLoadStoreActions = value;
			HasUsesLoadStoreActions = true;
		}
	}

	public bool UsesReversedZBuffer
	{
		get
		{
			return _UsesReversedZBuffer;
		}
		set
		{
			_UsesReversedZBuffer = value;
			HasUsesReversedZBuffer = true;
		}
	}

	public string GraphicsFormatForLDR
	{
		get
		{
			return _GraphicsFormatForLDR;
		}
		set
		{
			_GraphicsFormatForLDR = value;
			HasGraphicsFormatForLDR = value != null;
		}
	}

	public string GraphicsFormatForHDR
	{
		get
		{
			return _GraphicsFormatForHDR;
		}
		set
		{
			_GraphicsFormatForHDR = value;
			HasGraphicsFormatForHDR = value != null;
		}
	}

	public List<string> SupportedRenderTextureFormats
	{
		get
		{
			return _SupportedRenderTextureFormats;
		}
		set
		{
			_SupportedRenderTextureFormats = value;
		}
	}

	public List<string> SupportedTextureFormats
	{
		get
		{
			return _SupportedTextureFormats;
		}
		set
		{
			_SupportedTextureFormats = value;
		}
	}

	public override int GetHashCode()
	{
		int hash = GetType().GetHashCode();
		if (HasBatteryLevel)
		{
			hash ^= BatteryLevel.GetHashCode();
		}
		if (HasBatteryStatus)
		{
			hash ^= BatteryStatus.GetHashCode();
		}
		if (HasCopyTextureSupport)
		{
			hash ^= CopyTextureSupport.GetHashCode();
		}
		if (HasDeviceModel)
		{
			hash ^= DeviceModel.GetHashCode();
		}
		if (HasDeviceName)
		{
			hash ^= DeviceName.GetHashCode();
		}
		if (HasDeviceType)
		{
			hash ^= DeviceType.GetHashCode();
		}
		if (HasDeviceUniqueIdentifier)
		{
			hash ^= DeviceUniqueIdentifier.GetHashCode();
		}
		if (HasGraphicsDeviceID)
		{
			hash ^= GraphicsDeviceID.GetHashCode();
		}
		if (HasGraphicsDeviceName)
		{
			hash ^= GraphicsDeviceName.GetHashCode();
		}
		if (HasGraphicsDeviceType)
		{
			hash ^= GraphicsDeviceType.GetHashCode();
		}
		if (HasGraphicsDeviceVendor)
		{
			hash ^= GraphicsDeviceVendor.GetHashCode();
		}
		if (HasGraphicsDeviceVendorID)
		{
			hash ^= GraphicsDeviceVendorID.GetHashCode();
		}
		if (HasGraphicsDeviceVersion)
		{
			hash ^= GraphicsDeviceVersion.GetHashCode();
		}
		if (HasGraphicsMemorySize)
		{
			hash ^= GraphicsMemorySize.GetHashCode();
		}
		if (HasGraphicsMultiThreaded)
		{
			hash ^= GraphicsMultiThreaded.GetHashCode();
		}
		if (HasGraphicsShaderLevel)
		{
			hash ^= GraphicsShaderLevel.GetHashCode();
		}
		if (HasGraphicsUVStartsAtTop)
		{
			hash ^= GraphicsUVStartsAtTop.GetHashCode();
		}
		if (HasHasDynamicUniformArrayIndexingInFragmentShaders)
		{
			hash ^= HasDynamicUniformArrayIndexingInFragmentShaders.GetHashCode();
		}
		if (HasHasHiddenSurfaceRemovalOnGPU)
		{
			hash ^= HasHiddenSurfaceRemovalOnGPU.GetHashCode();
		}
		if (HasHasMipMaxLevel)
		{
			hash ^= HasMipMaxLevel.GetHashCode();
		}
		if (HasMaxComputeBufferInputsCompute)
		{
			hash ^= MaxComputeBufferInputsCompute.GetHashCode();
		}
		if (HasMaxComputeBufferInputsDomain)
		{
			hash ^= MaxComputeBufferInputsDomain.GetHashCode();
		}
		if (HasMaxComputeBufferInputsFragment)
		{
			hash ^= MaxComputeBufferInputsFragment.GetHashCode();
		}
		if (HasMaxComputeBufferInputsGeometry)
		{
			hash ^= MaxComputeBufferInputsGeometry.GetHashCode();
		}
		if (HasMaxComputeBufferInputsHull)
		{
			hash ^= MaxComputeBufferInputsHull.GetHashCode();
		}
		if (HasMaxComputeBufferInputsVertex)
		{
			hash ^= MaxComputeBufferInputsVertex.GetHashCode();
		}
		if (HasMaxComputeWorkGroupSize)
		{
			hash ^= MaxComputeWorkGroupSize.GetHashCode();
		}
		if (HasMaxComputeWorkGroupSizeX)
		{
			hash ^= MaxComputeWorkGroupSizeX.GetHashCode();
		}
		if (HasMaxComputeWorkGroupSizeY)
		{
			hash ^= MaxComputeWorkGroupSizeY.GetHashCode();
		}
		if (HasMaxComputeWorkGroupSizeZ)
		{
			hash ^= MaxComputeWorkGroupSizeZ.GetHashCode();
		}
		if (HasMaxCubemapSize)
		{
			hash ^= MaxCubemapSize.GetHashCode();
		}
		if (HasMaxTextureSize)
		{
			hash ^= MaxTextureSize.GetHashCode();
		}
		if (HasMinConstantBufferOffsetAlignment)
		{
			hash ^= MinConstantBufferOffsetAlignment.GetHashCode();
		}
		if (HasNpotSupport)
		{
			hash ^= NpotSupport.GetHashCode();
		}
		if (HasOperatingSystem)
		{
			hash ^= OperatingSystem.GetHashCode();
		}
		if (HasOperatingSystemFamily)
		{
			hash ^= OperatingSystemFamily.GetHashCode();
		}
		if (HasProcessorCount)
		{
			hash ^= ProcessorCount.GetHashCode();
		}
		if (HasProcessorFrequency)
		{
			hash ^= ProcessorFrequency.GetHashCode();
		}
		if (HasProcessorType)
		{
			hash ^= ProcessorType.GetHashCode();
		}
		if (HasRenderingThreadingMode)
		{
			hash ^= RenderingThreadingMode.GetHashCode();
		}
		if (HasSupportedRandomWriteTargetCount)
		{
			hash ^= SupportedRandomWriteTargetCount.GetHashCode();
		}
		if (HasSupportedRenderTargetCount)
		{
			hash ^= SupportedRenderTargetCount.GetHashCode();
		}
		if (HasSupports2DArrayTextures)
		{
			hash ^= Supports2DArrayTextures.GetHashCode();
		}
		if (HasSupports32bitsIndexBuffer)
		{
			hash ^= Supports32bitsIndexBuffer.GetHashCode();
		}
		if (HasSupports3DRenderTextures)
		{
			hash ^= Supports3DRenderTextures.GetHashCode();
		}
		if (HasSupports3DTextures)
		{
			hash ^= Supports3DTextures.GetHashCode();
		}
		if (HasSupportsAccelerometer)
		{
			hash ^= SupportsAccelerometer.GetHashCode();
		}
		if (HasSupportsAsyncCompute)
		{
			hash ^= SupportsAsyncCompute.GetHashCode();
		}
		if (HasSupportsAsyncGPUReadback)
		{
			hash ^= SupportsAsyncGPUReadback.GetHashCode();
		}
		if (HasSupportsAudio)
		{
			hash ^= SupportsAudio.GetHashCode();
		}
		if (HasSupportsComputeShaders)
		{
			hash ^= SupportsComputeShaders.GetHashCode();
		}
		if (HasSupportsCubemapArrayTextures)
		{
			hash ^= SupportsCubemapArrayTextures.GetHashCode();
		}
		if (HasSupportsGeometryShaders)
		{
			hash ^= SupportsGeometryShaders.GetHashCode();
		}
		if (HasSupportsGraphicsFence)
		{
			hash ^= SupportsGraphicsFence.GetHashCode();
		}
		if (HasSupportsGyroscope)
		{
			hash ^= SupportsGyroscope.GetHashCode();
		}
		if (HasSupportsHardwareQuadTopology)
		{
			hash ^= SupportsHardwareQuadTopology.GetHashCode();
		}
		if (HasSupportsInstancing)
		{
			hash ^= SupportsInstancing.GetHashCode();
		}
		if (HasSupportsLocationService)
		{
			hash ^= SupportsLocationService.GetHashCode();
		}
		if (HasSupportsMipStreaming)
		{
			hash ^= SupportsMipStreaming.GetHashCode();
		}
		if (HasSupportsMotionVectors)
		{
			hash ^= SupportsMotionVectors.GetHashCode();
		}
		if (HasSupportsMultisampleAutoResolve)
		{
			hash ^= SupportsMultisampleAutoResolve.GetHashCode();
		}
		if (HasSupportsMultisampledTextures)
		{
			hash ^= SupportsMultisampledTextures.GetHashCode();
		}
		if (HasSupportsRawShadowDepthSampling)
		{
			hash ^= SupportsRawShadowDepthSampling.GetHashCode();
		}
		if (HasSupportsRayTracing)
		{
			hash ^= SupportsRayTracing.GetHashCode();
		}
		if (HasSupportsSeparatedRenderTargetsBlend)
		{
			hash ^= SupportsSeparatedRenderTargetsBlend.GetHashCode();
		}
		if (HasSupportsSetConstantBuffer)
		{
			hash ^= SupportsSetConstantBuffer.GetHashCode();
		}
		if (HasSupportsShadows)
		{
			hash ^= SupportsShadows.GetHashCode();
		}
		if (HasSupportsSparseTextures)
		{
			hash ^= SupportsSparseTextures.GetHashCode();
		}
		if (HasSupportsStoreAndResolveAction)
		{
			hash ^= SupportsStoreAndResolveAction.GetHashCode();
		}
		if (HasSupportsTessellationShaders)
		{
			hash ^= SupportsTessellationShaders.GetHashCode();
		}
		if (HasSupportsTextureWrapMirrorOnce)
		{
			hash ^= SupportsTextureWrapMirrorOnce.GetHashCode();
		}
		if (HasSupportsVibration)
		{
			hash ^= SupportsVibration.GetHashCode();
		}
		if (HasSystemMemorySize)
		{
			hash ^= SystemMemorySize.GetHashCode();
		}
		if (HasUnsupportedIdentifier)
		{
			hash ^= UnsupportedIdentifier.GetHashCode();
		}
		if (HasUsesLoadStoreActions)
		{
			hash ^= UsesLoadStoreActions.GetHashCode();
		}
		if (HasUsesReversedZBuffer)
		{
			hash ^= UsesReversedZBuffer.GetHashCode();
		}
		if (HasGraphicsFormatForLDR)
		{
			hash ^= GraphicsFormatForLDR.GetHashCode();
		}
		if (HasGraphicsFormatForHDR)
		{
			hash ^= GraphicsFormatForHDR.GetHashCode();
		}
		foreach (string i in SupportedRenderTextureFormats)
		{
			hash ^= i.GetHashCode();
		}
		foreach (string i2 in SupportedTextureFormats)
		{
			hash ^= i2.GetHashCode();
		}
		return hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is UnitySystemInfo other))
		{
			return false;
		}
		if (HasBatteryLevel != other.HasBatteryLevel || (HasBatteryLevel && !BatteryLevel.Equals(other.BatteryLevel)))
		{
			return false;
		}
		if (HasBatteryStatus != other.HasBatteryStatus || (HasBatteryStatus && !BatteryStatus.Equals(other.BatteryStatus)))
		{
			return false;
		}
		if (HasCopyTextureSupport != other.HasCopyTextureSupport || (HasCopyTextureSupport && !CopyTextureSupport.Equals(other.CopyTextureSupport)))
		{
			return false;
		}
		if (HasDeviceModel != other.HasDeviceModel || (HasDeviceModel && !DeviceModel.Equals(other.DeviceModel)))
		{
			return false;
		}
		if (HasDeviceName != other.HasDeviceName || (HasDeviceName && !DeviceName.Equals(other.DeviceName)))
		{
			return false;
		}
		if (HasDeviceType != other.HasDeviceType || (HasDeviceType && !DeviceType.Equals(other.DeviceType)))
		{
			return false;
		}
		if (HasDeviceUniqueIdentifier != other.HasDeviceUniqueIdentifier || (HasDeviceUniqueIdentifier && !DeviceUniqueIdentifier.Equals(other.DeviceUniqueIdentifier)))
		{
			return false;
		}
		if (HasGraphicsDeviceID != other.HasGraphicsDeviceID || (HasGraphicsDeviceID && !GraphicsDeviceID.Equals(other.GraphicsDeviceID)))
		{
			return false;
		}
		if (HasGraphicsDeviceName != other.HasGraphicsDeviceName || (HasGraphicsDeviceName && !GraphicsDeviceName.Equals(other.GraphicsDeviceName)))
		{
			return false;
		}
		if (HasGraphicsDeviceType != other.HasGraphicsDeviceType || (HasGraphicsDeviceType && !GraphicsDeviceType.Equals(other.GraphicsDeviceType)))
		{
			return false;
		}
		if (HasGraphicsDeviceVendor != other.HasGraphicsDeviceVendor || (HasGraphicsDeviceVendor && !GraphicsDeviceVendor.Equals(other.GraphicsDeviceVendor)))
		{
			return false;
		}
		if (HasGraphicsDeviceVendorID != other.HasGraphicsDeviceVendorID || (HasGraphicsDeviceVendorID && !GraphicsDeviceVendorID.Equals(other.GraphicsDeviceVendorID)))
		{
			return false;
		}
		if (HasGraphicsDeviceVersion != other.HasGraphicsDeviceVersion || (HasGraphicsDeviceVersion && !GraphicsDeviceVersion.Equals(other.GraphicsDeviceVersion)))
		{
			return false;
		}
		if (HasGraphicsMemorySize != other.HasGraphicsMemorySize || (HasGraphicsMemorySize && !GraphicsMemorySize.Equals(other.GraphicsMemorySize)))
		{
			return false;
		}
		if (HasGraphicsMultiThreaded != other.HasGraphicsMultiThreaded || (HasGraphicsMultiThreaded && !GraphicsMultiThreaded.Equals(other.GraphicsMultiThreaded)))
		{
			return false;
		}
		if (HasGraphicsShaderLevel != other.HasGraphicsShaderLevel || (HasGraphicsShaderLevel && !GraphicsShaderLevel.Equals(other.GraphicsShaderLevel)))
		{
			return false;
		}
		if (HasGraphicsUVStartsAtTop != other.HasGraphicsUVStartsAtTop || (HasGraphicsUVStartsAtTop && !GraphicsUVStartsAtTop.Equals(other.GraphicsUVStartsAtTop)))
		{
			return false;
		}
		if (HasHasDynamicUniformArrayIndexingInFragmentShaders != other.HasHasDynamicUniformArrayIndexingInFragmentShaders || (HasHasDynamicUniformArrayIndexingInFragmentShaders && !HasDynamicUniformArrayIndexingInFragmentShaders.Equals(other.HasDynamicUniformArrayIndexingInFragmentShaders)))
		{
			return false;
		}
		if (HasHasHiddenSurfaceRemovalOnGPU != other.HasHasHiddenSurfaceRemovalOnGPU || (HasHasHiddenSurfaceRemovalOnGPU && !HasHiddenSurfaceRemovalOnGPU.Equals(other.HasHiddenSurfaceRemovalOnGPU)))
		{
			return false;
		}
		if (HasHasMipMaxLevel != other.HasHasMipMaxLevel || (HasHasMipMaxLevel && !HasMipMaxLevel.Equals(other.HasMipMaxLevel)))
		{
			return false;
		}
		if (HasMaxComputeBufferInputsCompute != other.HasMaxComputeBufferInputsCompute || (HasMaxComputeBufferInputsCompute && !MaxComputeBufferInputsCompute.Equals(other.MaxComputeBufferInputsCompute)))
		{
			return false;
		}
		if (HasMaxComputeBufferInputsDomain != other.HasMaxComputeBufferInputsDomain || (HasMaxComputeBufferInputsDomain && !MaxComputeBufferInputsDomain.Equals(other.MaxComputeBufferInputsDomain)))
		{
			return false;
		}
		if (HasMaxComputeBufferInputsFragment != other.HasMaxComputeBufferInputsFragment || (HasMaxComputeBufferInputsFragment && !MaxComputeBufferInputsFragment.Equals(other.MaxComputeBufferInputsFragment)))
		{
			return false;
		}
		if (HasMaxComputeBufferInputsGeometry != other.HasMaxComputeBufferInputsGeometry || (HasMaxComputeBufferInputsGeometry && !MaxComputeBufferInputsGeometry.Equals(other.MaxComputeBufferInputsGeometry)))
		{
			return false;
		}
		if (HasMaxComputeBufferInputsHull != other.HasMaxComputeBufferInputsHull || (HasMaxComputeBufferInputsHull && !MaxComputeBufferInputsHull.Equals(other.MaxComputeBufferInputsHull)))
		{
			return false;
		}
		if (HasMaxComputeBufferInputsVertex != other.HasMaxComputeBufferInputsVertex || (HasMaxComputeBufferInputsVertex && !MaxComputeBufferInputsVertex.Equals(other.MaxComputeBufferInputsVertex)))
		{
			return false;
		}
		if (HasMaxComputeWorkGroupSize != other.HasMaxComputeWorkGroupSize || (HasMaxComputeWorkGroupSize && !MaxComputeWorkGroupSize.Equals(other.MaxComputeWorkGroupSize)))
		{
			return false;
		}
		if (HasMaxComputeWorkGroupSizeX != other.HasMaxComputeWorkGroupSizeX || (HasMaxComputeWorkGroupSizeX && !MaxComputeWorkGroupSizeX.Equals(other.MaxComputeWorkGroupSizeX)))
		{
			return false;
		}
		if (HasMaxComputeWorkGroupSizeY != other.HasMaxComputeWorkGroupSizeY || (HasMaxComputeWorkGroupSizeY && !MaxComputeWorkGroupSizeY.Equals(other.MaxComputeWorkGroupSizeY)))
		{
			return false;
		}
		if (HasMaxComputeWorkGroupSizeZ != other.HasMaxComputeWorkGroupSizeZ || (HasMaxComputeWorkGroupSizeZ && !MaxComputeWorkGroupSizeZ.Equals(other.MaxComputeWorkGroupSizeZ)))
		{
			return false;
		}
		if (HasMaxCubemapSize != other.HasMaxCubemapSize || (HasMaxCubemapSize && !MaxCubemapSize.Equals(other.MaxCubemapSize)))
		{
			return false;
		}
		if (HasMaxTextureSize != other.HasMaxTextureSize || (HasMaxTextureSize && !MaxTextureSize.Equals(other.MaxTextureSize)))
		{
			return false;
		}
		if (HasMinConstantBufferOffsetAlignment != other.HasMinConstantBufferOffsetAlignment || (HasMinConstantBufferOffsetAlignment && !MinConstantBufferOffsetAlignment.Equals(other.MinConstantBufferOffsetAlignment)))
		{
			return false;
		}
		if (HasNpotSupport != other.HasNpotSupport || (HasNpotSupport && !NpotSupport.Equals(other.NpotSupport)))
		{
			return false;
		}
		if (HasOperatingSystem != other.HasOperatingSystem || (HasOperatingSystem && !OperatingSystem.Equals(other.OperatingSystem)))
		{
			return false;
		}
		if (HasOperatingSystemFamily != other.HasOperatingSystemFamily || (HasOperatingSystemFamily && !OperatingSystemFamily.Equals(other.OperatingSystemFamily)))
		{
			return false;
		}
		if (HasProcessorCount != other.HasProcessorCount || (HasProcessorCount && !ProcessorCount.Equals(other.ProcessorCount)))
		{
			return false;
		}
		if (HasProcessorFrequency != other.HasProcessorFrequency || (HasProcessorFrequency && !ProcessorFrequency.Equals(other.ProcessorFrequency)))
		{
			return false;
		}
		if (HasProcessorType != other.HasProcessorType || (HasProcessorType && !ProcessorType.Equals(other.ProcessorType)))
		{
			return false;
		}
		if (HasRenderingThreadingMode != other.HasRenderingThreadingMode || (HasRenderingThreadingMode && !RenderingThreadingMode.Equals(other.RenderingThreadingMode)))
		{
			return false;
		}
		if (HasSupportedRandomWriteTargetCount != other.HasSupportedRandomWriteTargetCount || (HasSupportedRandomWriteTargetCount && !SupportedRandomWriteTargetCount.Equals(other.SupportedRandomWriteTargetCount)))
		{
			return false;
		}
		if (HasSupportedRenderTargetCount != other.HasSupportedRenderTargetCount || (HasSupportedRenderTargetCount && !SupportedRenderTargetCount.Equals(other.SupportedRenderTargetCount)))
		{
			return false;
		}
		if (HasSupports2DArrayTextures != other.HasSupports2DArrayTextures || (HasSupports2DArrayTextures && !Supports2DArrayTextures.Equals(other.Supports2DArrayTextures)))
		{
			return false;
		}
		if (HasSupports32bitsIndexBuffer != other.HasSupports32bitsIndexBuffer || (HasSupports32bitsIndexBuffer && !Supports32bitsIndexBuffer.Equals(other.Supports32bitsIndexBuffer)))
		{
			return false;
		}
		if (HasSupports3DRenderTextures != other.HasSupports3DRenderTextures || (HasSupports3DRenderTextures && !Supports3DRenderTextures.Equals(other.Supports3DRenderTextures)))
		{
			return false;
		}
		if (HasSupports3DTextures != other.HasSupports3DTextures || (HasSupports3DTextures && !Supports3DTextures.Equals(other.Supports3DTextures)))
		{
			return false;
		}
		if (HasSupportsAccelerometer != other.HasSupportsAccelerometer || (HasSupportsAccelerometer && !SupportsAccelerometer.Equals(other.SupportsAccelerometer)))
		{
			return false;
		}
		if (HasSupportsAsyncCompute != other.HasSupportsAsyncCompute || (HasSupportsAsyncCompute && !SupportsAsyncCompute.Equals(other.SupportsAsyncCompute)))
		{
			return false;
		}
		if (HasSupportsAsyncGPUReadback != other.HasSupportsAsyncGPUReadback || (HasSupportsAsyncGPUReadback && !SupportsAsyncGPUReadback.Equals(other.SupportsAsyncGPUReadback)))
		{
			return false;
		}
		if (HasSupportsAudio != other.HasSupportsAudio || (HasSupportsAudio && !SupportsAudio.Equals(other.SupportsAudio)))
		{
			return false;
		}
		if (HasSupportsComputeShaders != other.HasSupportsComputeShaders || (HasSupportsComputeShaders && !SupportsComputeShaders.Equals(other.SupportsComputeShaders)))
		{
			return false;
		}
		if (HasSupportsCubemapArrayTextures != other.HasSupportsCubemapArrayTextures || (HasSupportsCubemapArrayTextures && !SupportsCubemapArrayTextures.Equals(other.SupportsCubemapArrayTextures)))
		{
			return false;
		}
		if (HasSupportsGeometryShaders != other.HasSupportsGeometryShaders || (HasSupportsGeometryShaders && !SupportsGeometryShaders.Equals(other.SupportsGeometryShaders)))
		{
			return false;
		}
		if (HasSupportsGraphicsFence != other.HasSupportsGraphicsFence || (HasSupportsGraphicsFence && !SupportsGraphicsFence.Equals(other.SupportsGraphicsFence)))
		{
			return false;
		}
		if (HasSupportsGyroscope != other.HasSupportsGyroscope || (HasSupportsGyroscope && !SupportsGyroscope.Equals(other.SupportsGyroscope)))
		{
			return false;
		}
		if (HasSupportsHardwareQuadTopology != other.HasSupportsHardwareQuadTopology || (HasSupportsHardwareQuadTopology && !SupportsHardwareQuadTopology.Equals(other.SupportsHardwareQuadTopology)))
		{
			return false;
		}
		if (HasSupportsInstancing != other.HasSupportsInstancing || (HasSupportsInstancing && !SupportsInstancing.Equals(other.SupportsInstancing)))
		{
			return false;
		}
		if (HasSupportsLocationService != other.HasSupportsLocationService || (HasSupportsLocationService && !SupportsLocationService.Equals(other.SupportsLocationService)))
		{
			return false;
		}
		if (HasSupportsMipStreaming != other.HasSupportsMipStreaming || (HasSupportsMipStreaming && !SupportsMipStreaming.Equals(other.SupportsMipStreaming)))
		{
			return false;
		}
		if (HasSupportsMotionVectors != other.HasSupportsMotionVectors || (HasSupportsMotionVectors && !SupportsMotionVectors.Equals(other.SupportsMotionVectors)))
		{
			return false;
		}
		if (HasSupportsMultisampleAutoResolve != other.HasSupportsMultisampleAutoResolve || (HasSupportsMultisampleAutoResolve && !SupportsMultisampleAutoResolve.Equals(other.SupportsMultisampleAutoResolve)))
		{
			return false;
		}
		if (HasSupportsMultisampledTextures != other.HasSupportsMultisampledTextures || (HasSupportsMultisampledTextures && !SupportsMultisampledTextures.Equals(other.SupportsMultisampledTextures)))
		{
			return false;
		}
		if (HasSupportsRawShadowDepthSampling != other.HasSupportsRawShadowDepthSampling || (HasSupportsRawShadowDepthSampling && !SupportsRawShadowDepthSampling.Equals(other.SupportsRawShadowDepthSampling)))
		{
			return false;
		}
		if (HasSupportsRayTracing != other.HasSupportsRayTracing || (HasSupportsRayTracing && !SupportsRayTracing.Equals(other.SupportsRayTracing)))
		{
			return false;
		}
		if (HasSupportsSeparatedRenderTargetsBlend != other.HasSupportsSeparatedRenderTargetsBlend || (HasSupportsSeparatedRenderTargetsBlend && !SupportsSeparatedRenderTargetsBlend.Equals(other.SupportsSeparatedRenderTargetsBlend)))
		{
			return false;
		}
		if (HasSupportsSetConstantBuffer != other.HasSupportsSetConstantBuffer || (HasSupportsSetConstantBuffer && !SupportsSetConstantBuffer.Equals(other.SupportsSetConstantBuffer)))
		{
			return false;
		}
		if (HasSupportsShadows != other.HasSupportsShadows || (HasSupportsShadows && !SupportsShadows.Equals(other.SupportsShadows)))
		{
			return false;
		}
		if (HasSupportsSparseTextures != other.HasSupportsSparseTextures || (HasSupportsSparseTextures && !SupportsSparseTextures.Equals(other.SupportsSparseTextures)))
		{
			return false;
		}
		if (HasSupportsStoreAndResolveAction != other.HasSupportsStoreAndResolveAction || (HasSupportsStoreAndResolveAction && !SupportsStoreAndResolveAction.Equals(other.SupportsStoreAndResolveAction)))
		{
			return false;
		}
		if (HasSupportsTessellationShaders != other.HasSupportsTessellationShaders || (HasSupportsTessellationShaders && !SupportsTessellationShaders.Equals(other.SupportsTessellationShaders)))
		{
			return false;
		}
		if (HasSupportsTextureWrapMirrorOnce != other.HasSupportsTextureWrapMirrorOnce || (HasSupportsTextureWrapMirrorOnce && !SupportsTextureWrapMirrorOnce.Equals(other.SupportsTextureWrapMirrorOnce)))
		{
			return false;
		}
		if (HasSupportsVibration != other.HasSupportsVibration || (HasSupportsVibration && !SupportsVibration.Equals(other.SupportsVibration)))
		{
			return false;
		}
		if (HasSystemMemorySize != other.HasSystemMemorySize || (HasSystemMemorySize && !SystemMemorySize.Equals(other.SystemMemorySize)))
		{
			return false;
		}
		if (HasUnsupportedIdentifier != other.HasUnsupportedIdentifier || (HasUnsupportedIdentifier && !UnsupportedIdentifier.Equals(other.UnsupportedIdentifier)))
		{
			return false;
		}
		if (HasUsesLoadStoreActions != other.HasUsesLoadStoreActions || (HasUsesLoadStoreActions && !UsesLoadStoreActions.Equals(other.UsesLoadStoreActions)))
		{
			return false;
		}
		if (HasUsesReversedZBuffer != other.HasUsesReversedZBuffer || (HasUsesReversedZBuffer && !UsesReversedZBuffer.Equals(other.UsesReversedZBuffer)))
		{
			return false;
		}
		if (HasGraphicsFormatForLDR != other.HasGraphicsFormatForLDR || (HasGraphicsFormatForLDR && !GraphicsFormatForLDR.Equals(other.GraphicsFormatForLDR)))
		{
			return false;
		}
		if (HasGraphicsFormatForHDR != other.HasGraphicsFormatForHDR || (HasGraphicsFormatForHDR && !GraphicsFormatForHDR.Equals(other.GraphicsFormatForHDR)))
		{
			return false;
		}
		if (SupportedRenderTextureFormats.Count != other.SupportedRenderTextureFormats.Count)
		{
			return false;
		}
		for (int i = 0; i < SupportedRenderTextureFormats.Count; i++)
		{
			if (!SupportedRenderTextureFormats[i].Equals(other.SupportedRenderTextureFormats[i]))
			{
				return false;
			}
		}
		if (SupportedTextureFormats.Count != other.SupportedTextureFormats.Count)
		{
			return false;
		}
		for (int j = 0; j < SupportedTextureFormats.Count; j++)
		{
			if (!SupportedTextureFormats[j].Equals(other.SupportedTextureFormats[j]))
			{
				return false;
			}
		}
		return true;
	}

	public void Deserialize(Stream stream)
	{
		Deserialize(stream, this);
	}

	public static UnitySystemInfo Deserialize(Stream stream, UnitySystemInfo instance)
	{
		return Deserialize(stream, instance, -1L);
	}

	public static UnitySystemInfo DeserializeLengthDelimited(Stream stream)
	{
		UnitySystemInfo instance = new UnitySystemInfo();
		DeserializeLengthDelimited(stream, instance);
		return instance;
	}

	public static UnitySystemInfo DeserializeLengthDelimited(Stream stream, UnitySystemInfo instance)
	{
		long limit = ProtocolParser.ReadUInt32(stream);
		limit += stream.Position;
		return Deserialize(stream, instance, limit);
	}

	public static UnitySystemInfo Deserialize(Stream stream, UnitySystemInfo instance, long limit)
	{
		BinaryReader br = new BinaryReader(stream);
		instance.BatteryStatus = BatteryStatusEnum.UnknownStatus;
		instance.CopyTextureSupport = CopyTextureSupportEnum.None;
		instance.DeviceType = DeviceTypeEnum.Unknown;
		instance.GraphicsDeviceType = GraphicsDeviceTypeEnum.OpenGL2;
		instance.NpotSupport = NPOTSupportEnum.NotSupported;
		instance.OperatingSystemFamily = OperatingSystemFamilyEnum.Other;
		instance.RenderingThreadingMode = RenderingThreadingModeEnum.Direct;
		if (instance.SupportedRenderTextureFormats == null)
		{
			instance.SupportedRenderTextureFormats = new List<string>();
		}
		if (instance.SupportedTextureFormats == null)
		{
			instance.SupportedTextureFormats = new List<string>();
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
			case 13:
				instance.BatteryLevel = br.ReadSingle();
				continue;
			case 16:
				instance.BatteryStatus = (BatteryStatusEnum)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				instance.CopyTextureSupport = (CopyTextureSupportEnum)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				instance.DeviceModel = ProtocolParser.ReadString(stream);
				continue;
			case 42:
				instance.DeviceName = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				instance.DeviceType = (DeviceTypeEnum)ProtocolParser.ReadUInt64(stream);
				continue;
			case 58:
				instance.DeviceUniqueIdentifier = ProtocolParser.ReadString(stream);
				continue;
			case 64:
				instance.GraphicsDeviceID = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 74:
				instance.GraphicsDeviceName = ProtocolParser.ReadString(stream);
				continue;
			case 80:
				instance.GraphicsDeviceType = (GraphicsDeviceTypeEnum)ProtocolParser.ReadUInt64(stream);
				continue;
			case 90:
				instance.GraphicsDeviceVendor = ProtocolParser.ReadString(stream);
				continue;
			case 96:
				instance.GraphicsDeviceVendorID = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 106:
				instance.GraphicsDeviceVersion = ProtocolParser.ReadString(stream);
				continue;
			case 112:
				instance.GraphicsMemorySize = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 120:
				instance.GraphicsMultiThreaded = ProtocolParser.ReadBool(stream);
				continue;
			default:
			{
				Key key = ProtocolParser.ReadKey((byte)keyByte, stream);
				switch (key.Field)
				{
				case 0u:
					throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
				case 16u:
					if (key.WireType == Wire.Varint)
					{
						instance.GraphicsShaderLevel = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 17u:
					if (key.WireType == Wire.Varint)
					{
						instance.GraphicsUVStartsAtTop = ProtocolParser.ReadBool(stream);
					}
					break;
				case 18u:
					if (key.WireType == Wire.Varint)
					{
						instance.HasDynamicUniformArrayIndexingInFragmentShaders = ProtocolParser.ReadBool(stream);
					}
					break;
				case 19u:
					if (key.WireType == Wire.Varint)
					{
						instance.HasHiddenSurfaceRemovalOnGPU = ProtocolParser.ReadBool(stream);
					}
					break;
				case 20u:
					if (key.WireType == Wire.Varint)
					{
						instance.HasMipMaxLevel = ProtocolParser.ReadBool(stream);
					}
					break;
				case 21u:
					if (key.WireType == Wire.Varint)
					{
						instance.MaxComputeBufferInputsCompute = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 22u:
					if (key.WireType == Wire.Varint)
					{
						instance.MaxComputeBufferInputsDomain = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 23u:
					if (key.WireType == Wire.Varint)
					{
						instance.MaxComputeBufferInputsFragment = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 24u:
					if (key.WireType == Wire.Varint)
					{
						instance.MaxComputeBufferInputsGeometry = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 25u:
					if (key.WireType == Wire.Varint)
					{
						instance.MaxComputeBufferInputsHull = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 26u:
					if (key.WireType == Wire.Varint)
					{
						instance.MaxComputeBufferInputsVertex = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 27u:
					if (key.WireType == Wire.Varint)
					{
						instance.MaxComputeWorkGroupSize = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 28u:
					if (key.WireType == Wire.Varint)
					{
						instance.MaxComputeWorkGroupSizeX = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 29u:
					if (key.WireType == Wire.Varint)
					{
						instance.MaxComputeWorkGroupSizeY = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 30u:
					if (key.WireType == Wire.Varint)
					{
						instance.MaxComputeWorkGroupSizeZ = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 31u:
					if (key.WireType == Wire.Varint)
					{
						instance.MaxCubemapSize = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 32u:
					if (key.WireType == Wire.Varint)
					{
						instance.MaxTextureSize = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 33u:
					if (key.WireType == Wire.Varint)
					{
						instance.MinConstantBufferOffsetAlignment = ProtocolParser.ReadBool(stream);
					}
					break;
				case 34u:
					if (key.WireType == Wire.Varint)
					{
						instance.NpotSupport = (NPOTSupportEnum)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 35u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.OperatingSystem = ProtocolParser.ReadString(stream);
					}
					break;
				case 36u:
					if (key.WireType == Wire.Varint)
					{
						instance.OperatingSystemFamily = (OperatingSystemFamilyEnum)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 37u:
					if (key.WireType == Wire.Varint)
					{
						instance.ProcessorCount = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 38u:
					if (key.WireType == Wire.Varint)
					{
						instance.ProcessorFrequency = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 39u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.ProcessorType = ProtocolParser.ReadString(stream);
					}
					break;
				case 40u:
					if (key.WireType == Wire.Varint)
					{
						instance.RenderingThreadingMode = (RenderingThreadingModeEnum)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 41u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportedRandomWriteTargetCount = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 42u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportedRenderTargetCount = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 43u:
					if (key.WireType == Wire.Varint)
					{
						instance.Supports2DArrayTextures = ProtocolParser.ReadBool(stream);
					}
					break;
				case 44u:
					if (key.WireType == Wire.Varint)
					{
						instance.Supports32bitsIndexBuffer = ProtocolParser.ReadBool(stream);
					}
					break;
				case 45u:
					if (key.WireType == Wire.Varint)
					{
						instance.Supports3DRenderTextures = ProtocolParser.ReadBool(stream);
					}
					break;
				case 46u:
					if (key.WireType == Wire.Varint)
					{
						instance.Supports3DTextures = ProtocolParser.ReadBool(stream);
					}
					break;
				case 47u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsAccelerometer = ProtocolParser.ReadBool(stream);
					}
					break;
				case 48u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsAsyncCompute = ProtocolParser.ReadBool(stream);
					}
					break;
				case 49u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsAsyncGPUReadback = ProtocolParser.ReadBool(stream);
					}
					break;
				case 50u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsAudio = ProtocolParser.ReadBool(stream);
					}
					break;
				case 51u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsComputeShaders = ProtocolParser.ReadBool(stream);
					}
					break;
				case 52u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsCubemapArrayTextures = ProtocolParser.ReadBool(stream);
					}
					break;
				case 53u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsGeometryShaders = ProtocolParser.ReadBool(stream);
					}
					break;
				case 54u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsGraphicsFence = ProtocolParser.ReadBool(stream);
					}
					break;
				case 55u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsGyroscope = ProtocolParser.ReadBool(stream);
					}
					break;
				case 56u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsHardwareQuadTopology = ProtocolParser.ReadBool(stream);
					}
					break;
				case 57u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsInstancing = ProtocolParser.ReadBool(stream);
					}
					break;
				case 58u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsLocationService = ProtocolParser.ReadBool(stream);
					}
					break;
				case 59u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsMipStreaming = ProtocolParser.ReadBool(stream);
					}
					break;
				case 60u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsMotionVectors = ProtocolParser.ReadBool(stream);
					}
					break;
				case 61u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsMultisampleAutoResolve = ProtocolParser.ReadBool(stream);
					}
					break;
				case 62u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsMultisampledTextures = ProtocolParser.ReadBool(stream);
					}
					break;
				case 63u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsRawShadowDepthSampling = ProtocolParser.ReadBool(stream);
					}
					break;
				case 64u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsRayTracing = ProtocolParser.ReadBool(stream);
					}
					break;
				case 65u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsSeparatedRenderTargetsBlend = ProtocolParser.ReadBool(stream);
					}
					break;
				case 66u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsSetConstantBuffer = ProtocolParser.ReadBool(stream);
					}
					break;
				case 67u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsShadows = ProtocolParser.ReadBool(stream);
					}
					break;
				case 68u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsSparseTextures = ProtocolParser.ReadBool(stream);
					}
					break;
				case 69u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsStoreAndResolveAction = ProtocolParser.ReadBool(stream);
					}
					break;
				case 70u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsTessellationShaders = ProtocolParser.ReadBool(stream);
					}
					break;
				case 71u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsTextureWrapMirrorOnce = ProtocolParser.ReadBool(stream);
					}
					break;
				case 72u:
					if (key.WireType == Wire.Varint)
					{
						instance.SupportsVibration = ProtocolParser.ReadBool(stream);
					}
					break;
				case 73u:
					if (key.WireType == Wire.Varint)
					{
						instance.SystemMemorySize = (int)ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 74u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.UnsupportedIdentifier = ProtocolParser.ReadString(stream);
					}
					break;
				case 75u:
					if (key.WireType == Wire.Varint)
					{
						instance.UsesLoadStoreActions = ProtocolParser.ReadBool(stream);
					}
					break;
				case 76u:
					if (key.WireType == Wire.Varint)
					{
						instance.UsesReversedZBuffer = ProtocolParser.ReadBool(stream);
					}
					break;
				case 77u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.GraphicsFormatForLDR = ProtocolParser.ReadString(stream);
					}
					break;
				case 78u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.GraphicsFormatForHDR = ProtocolParser.ReadString(stream);
					}
					break;
				case 79u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.SupportedRenderTextureFormats.Add(ProtocolParser.ReadString(stream));
					}
					break;
				case 80u:
					if (key.WireType == Wire.LengthDelimited)
					{
						instance.SupportedTextureFormats.Add(ProtocolParser.ReadString(stream));
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

	public static void Serialize(Stream stream, UnitySystemInfo instance)
	{
		BinaryWriter bw = new BinaryWriter(stream);
		if (instance.HasBatteryLevel)
		{
			stream.WriteByte(13);
			bw.Write(instance.BatteryLevel);
		}
		if (instance.HasBatteryStatus)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.BatteryStatus);
		}
		if (instance.HasCopyTextureSupport)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.CopyTextureSupport);
		}
		if (instance.HasDeviceModel)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DeviceModel));
		}
		if (instance.HasDeviceName)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DeviceName));
		}
		if (instance.HasDeviceType)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.DeviceType);
		}
		if (instance.HasDeviceUniqueIdentifier)
		{
			stream.WriteByte(58);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.DeviceUniqueIdentifier));
		}
		if (instance.HasGraphicsDeviceID)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.GraphicsDeviceID);
		}
		if (instance.HasGraphicsDeviceName)
		{
			stream.WriteByte(74);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.GraphicsDeviceName));
		}
		if (instance.HasGraphicsDeviceType)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.GraphicsDeviceType);
		}
		if (instance.HasGraphicsDeviceVendor)
		{
			stream.WriteByte(90);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.GraphicsDeviceVendor));
		}
		if (instance.HasGraphicsDeviceVendorID)
		{
			stream.WriteByte(96);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.GraphicsDeviceVendorID);
		}
		if (instance.HasGraphicsDeviceVersion)
		{
			stream.WriteByte(106);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.GraphicsDeviceVersion));
		}
		if (instance.HasGraphicsMemorySize)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.GraphicsMemorySize);
		}
		if (instance.HasGraphicsMultiThreaded)
		{
			stream.WriteByte(120);
			ProtocolParser.WriteBool(stream, instance.GraphicsMultiThreaded);
		}
		if (instance.HasGraphicsShaderLevel)
		{
			stream.WriteByte(128);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.GraphicsShaderLevel);
		}
		if (instance.HasGraphicsUVStartsAtTop)
		{
			stream.WriteByte(136);
			stream.WriteByte(1);
			ProtocolParser.WriteBool(stream, instance.GraphicsUVStartsAtTop);
		}
		if (instance.HasHasDynamicUniformArrayIndexingInFragmentShaders)
		{
			stream.WriteByte(144);
			stream.WriteByte(1);
			ProtocolParser.WriteBool(stream, instance.HasDynamicUniformArrayIndexingInFragmentShaders);
		}
		if (instance.HasHasHiddenSurfaceRemovalOnGPU)
		{
			stream.WriteByte(152);
			stream.WriteByte(1);
			ProtocolParser.WriteBool(stream, instance.HasHiddenSurfaceRemovalOnGPU);
		}
		if (instance.HasHasMipMaxLevel)
		{
			stream.WriteByte(160);
			stream.WriteByte(1);
			ProtocolParser.WriteBool(stream, instance.HasMipMaxLevel);
		}
		if (instance.HasMaxComputeBufferInputsCompute)
		{
			stream.WriteByte(168);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MaxComputeBufferInputsCompute);
		}
		if (instance.HasMaxComputeBufferInputsDomain)
		{
			stream.WriteByte(176);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MaxComputeBufferInputsDomain);
		}
		if (instance.HasMaxComputeBufferInputsFragment)
		{
			stream.WriteByte(184);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MaxComputeBufferInputsFragment);
		}
		if (instance.HasMaxComputeBufferInputsGeometry)
		{
			stream.WriteByte(192);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MaxComputeBufferInputsGeometry);
		}
		if (instance.HasMaxComputeBufferInputsHull)
		{
			stream.WriteByte(200);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MaxComputeBufferInputsHull);
		}
		if (instance.HasMaxComputeBufferInputsVertex)
		{
			stream.WriteByte(208);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MaxComputeBufferInputsVertex);
		}
		if (instance.HasMaxComputeWorkGroupSize)
		{
			stream.WriteByte(216);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MaxComputeWorkGroupSize);
		}
		if (instance.HasMaxComputeWorkGroupSizeX)
		{
			stream.WriteByte(224);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MaxComputeWorkGroupSizeX);
		}
		if (instance.HasMaxComputeWorkGroupSizeY)
		{
			stream.WriteByte(232);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MaxComputeWorkGroupSizeY);
		}
		if (instance.HasMaxComputeWorkGroupSizeZ)
		{
			stream.WriteByte(240);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MaxComputeWorkGroupSizeZ);
		}
		if (instance.HasMaxCubemapSize)
		{
			stream.WriteByte(248);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MaxCubemapSize);
		}
		if (instance.HasMaxTextureSize)
		{
			stream.WriteByte(128);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.MaxTextureSize);
		}
		if (instance.HasMinConstantBufferOffsetAlignment)
		{
			stream.WriteByte(136);
			stream.WriteByte(2);
			ProtocolParser.WriteBool(stream, instance.MinConstantBufferOffsetAlignment);
		}
		if (instance.HasNpotSupport)
		{
			stream.WriteByte(144);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.NpotSupport);
		}
		if (instance.HasOperatingSystem)
		{
			stream.WriteByte(154);
			stream.WriteByte(2);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.OperatingSystem));
		}
		if (instance.HasOperatingSystemFamily)
		{
			stream.WriteByte(160);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.OperatingSystemFamily);
		}
		if (instance.HasProcessorCount)
		{
			stream.WriteByte(168);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ProcessorCount);
		}
		if (instance.HasProcessorFrequency)
		{
			stream.WriteByte(176);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ProcessorFrequency);
		}
		if (instance.HasProcessorType)
		{
			stream.WriteByte(186);
			stream.WriteByte(2);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.ProcessorType));
		}
		if (instance.HasRenderingThreadingMode)
		{
			stream.WriteByte(192);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.RenderingThreadingMode);
		}
		if (instance.HasSupportedRandomWriteTargetCount)
		{
			stream.WriteByte(200);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.SupportedRandomWriteTargetCount);
		}
		if (instance.HasSupportedRenderTargetCount)
		{
			stream.WriteByte(208);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.SupportedRenderTargetCount);
		}
		if (instance.HasSupports2DArrayTextures)
		{
			stream.WriteByte(216);
			stream.WriteByte(2);
			ProtocolParser.WriteBool(stream, instance.Supports2DArrayTextures);
		}
		if (instance.HasSupports32bitsIndexBuffer)
		{
			stream.WriteByte(224);
			stream.WriteByte(2);
			ProtocolParser.WriteBool(stream, instance.Supports32bitsIndexBuffer);
		}
		if (instance.HasSupports3DRenderTextures)
		{
			stream.WriteByte(232);
			stream.WriteByte(2);
			ProtocolParser.WriteBool(stream, instance.Supports3DRenderTextures);
		}
		if (instance.HasSupports3DTextures)
		{
			stream.WriteByte(240);
			stream.WriteByte(2);
			ProtocolParser.WriteBool(stream, instance.Supports3DTextures);
		}
		if (instance.HasSupportsAccelerometer)
		{
			stream.WriteByte(248);
			stream.WriteByte(2);
			ProtocolParser.WriteBool(stream, instance.SupportsAccelerometer);
		}
		if (instance.HasSupportsAsyncCompute)
		{
			stream.WriteByte(128);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsAsyncCompute);
		}
		if (instance.HasSupportsAsyncGPUReadback)
		{
			stream.WriteByte(136);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsAsyncGPUReadback);
		}
		if (instance.HasSupportsAudio)
		{
			stream.WriteByte(144);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsAudio);
		}
		if (instance.HasSupportsComputeShaders)
		{
			stream.WriteByte(152);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsComputeShaders);
		}
		if (instance.HasSupportsCubemapArrayTextures)
		{
			stream.WriteByte(160);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsCubemapArrayTextures);
		}
		if (instance.HasSupportsGeometryShaders)
		{
			stream.WriteByte(168);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsGeometryShaders);
		}
		if (instance.HasSupportsGraphicsFence)
		{
			stream.WriteByte(176);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsGraphicsFence);
		}
		if (instance.HasSupportsGyroscope)
		{
			stream.WriteByte(184);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsGyroscope);
		}
		if (instance.HasSupportsHardwareQuadTopology)
		{
			stream.WriteByte(192);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsHardwareQuadTopology);
		}
		if (instance.HasSupportsInstancing)
		{
			stream.WriteByte(200);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsInstancing);
		}
		if (instance.HasSupportsLocationService)
		{
			stream.WriteByte(208);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsLocationService);
		}
		if (instance.HasSupportsMipStreaming)
		{
			stream.WriteByte(216);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsMipStreaming);
		}
		if (instance.HasSupportsMotionVectors)
		{
			stream.WriteByte(224);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsMotionVectors);
		}
		if (instance.HasSupportsMultisampleAutoResolve)
		{
			stream.WriteByte(232);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsMultisampleAutoResolve);
		}
		if (instance.HasSupportsMultisampledTextures)
		{
			stream.WriteByte(240);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsMultisampledTextures);
		}
		if (instance.HasSupportsRawShadowDepthSampling)
		{
			stream.WriteByte(248);
			stream.WriteByte(3);
			ProtocolParser.WriteBool(stream, instance.SupportsRawShadowDepthSampling);
		}
		if (instance.HasSupportsRayTracing)
		{
			stream.WriteByte(128);
			stream.WriteByte(4);
			ProtocolParser.WriteBool(stream, instance.SupportsRayTracing);
		}
		if (instance.HasSupportsSeparatedRenderTargetsBlend)
		{
			stream.WriteByte(136);
			stream.WriteByte(4);
			ProtocolParser.WriteBool(stream, instance.SupportsSeparatedRenderTargetsBlend);
		}
		if (instance.HasSupportsSetConstantBuffer)
		{
			stream.WriteByte(144);
			stream.WriteByte(4);
			ProtocolParser.WriteBool(stream, instance.SupportsSetConstantBuffer);
		}
		if (instance.HasSupportsShadows)
		{
			stream.WriteByte(152);
			stream.WriteByte(4);
			ProtocolParser.WriteBool(stream, instance.SupportsShadows);
		}
		if (instance.HasSupportsSparseTextures)
		{
			stream.WriteByte(160);
			stream.WriteByte(4);
			ProtocolParser.WriteBool(stream, instance.SupportsSparseTextures);
		}
		if (instance.HasSupportsStoreAndResolveAction)
		{
			stream.WriteByte(168);
			stream.WriteByte(4);
			ProtocolParser.WriteBool(stream, instance.SupportsStoreAndResolveAction);
		}
		if (instance.HasSupportsTessellationShaders)
		{
			stream.WriteByte(176);
			stream.WriteByte(4);
			ProtocolParser.WriteBool(stream, instance.SupportsTessellationShaders);
		}
		if (instance.HasSupportsTextureWrapMirrorOnce)
		{
			stream.WriteByte(184);
			stream.WriteByte(4);
			ProtocolParser.WriteBool(stream, instance.SupportsTextureWrapMirrorOnce);
		}
		if (instance.HasSupportsVibration)
		{
			stream.WriteByte(192);
			stream.WriteByte(4);
			ProtocolParser.WriteBool(stream, instance.SupportsVibration);
		}
		if (instance.HasSystemMemorySize)
		{
			stream.WriteByte(200);
			stream.WriteByte(4);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.SystemMemorySize);
		}
		if (instance.HasUnsupportedIdentifier)
		{
			stream.WriteByte(210);
			stream.WriteByte(4);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.UnsupportedIdentifier));
		}
		if (instance.HasUsesLoadStoreActions)
		{
			stream.WriteByte(216);
			stream.WriteByte(4);
			ProtocolParser.WriteBool(stream, instance.UsesLoadStoreActions);
		}
		if (instance.HasUsesReversedZBuffer)
		{
			stream.WriteByte(224);
			stream.WriteByte(4);
			ProtocolParser.WriteBool(stream, instance.UsesReversedZBuffer);
		}
		if (instance.HasGraphicsFormatForLDR)
		{
			stream.WriteByte(234);
			stream.WriteByte(4);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.GraphicsFormatForLDR));
		}
		if (instance.HasGraphicsFormatForHDR)
		{
			stream.WriteByte(242);
			stream.WriteByte(4);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(instance.GraphicsFormatForHDR));
		}
		if (instance.SupportedRenderTextureFormats.Count > 0)
		{
			foreach (string i79 in instance.SupportedRenderTextureFormats)
			{
				stream.WriteByte(250);
				stream.WriteByte(4);
				ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(i79));
			}
		}
		if (instance.SupportedTextureFormats.Count <= 0)
		{
			return;
		}
		foreach (string i80 in instance.SupportedTextureFormats)
		{
			stream.WriteByte(130);
			stream.WriteByte(5);
			ProtocolParser.WriteBytes(stream, Encoding.UTF8.GetBytes(i80));
		}
	}

	public uint GetSerializedSize()
	{
		uint size = 0u;
		if (HasBatteryLevel)
		{
			size++;
			size += 4;
		}
		if (HasBatteryStatus)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)BatteryStatus);
		}
		if (HasCopyTextureSupport)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)CopyTextureSupport);
		}
		if (HasDeviceModel)
		{
			size++;
			uint byteCount4 = (uint)Encoding.UTF8.GetByteCount(DeviceModel);
			size += ProtocolParser.SizeOfUInt32(byteCount4) + byteCount4;
		}
		if (HasDeviceName)
		{
			size++;
			uint byteCount5 = (uint)Encoding.UTF8.GetByteCount(DeviceName);
			size += ProtocolParser.SizeOfUInt32(byteCount5) + byteCount5;
		}
		if (HasDeviceType)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)DeviceType);
		}
		if (HasDeviceUniqueIdentifier)
		{
			size++;
			uint byteCount7 = (uint)Encoding.UTF8.GetByteCount(DeviceUniqueIdentifier);
			size += ProtocolParser.SizeOfUInt32(byteCount7) + byteCount7;
		}
		if (HasGraphicsDeviceID)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)GraphicsDeviceID);
		}
		if (HasGraphicsDeviceName)
		{
			size++;
			uint byteCount9 = (uint)Encoding.UTF8.GetByteCount(GraphicsDeviceName);
			size += ProtocolParser.SizeOfUInt32(byteCount9) + byteCount9;
		}
		if (HasGraphicsDeviceType)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)GraphicsDeviceType);
		}
		if (HasGraphicsDeviceVendor)
		{
			size++;
			uint byteCount11 = (uint)Encoding.UTF8.GetByteCount(GraphicsDeviceVendor);
			size += ProtocolParser.SizeOfUInt32(byteCount11) + byteCount11;
		}
		if (HasGraphicsDeviceVendorID)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)GraphicsDeviceVendorID);
		}
		if (HasGraphicsDeviceVersion)
		{
			size++;
			uint byteCount13 = (uint)Encoding.UTF8.GetByteCount(GraphicsDeviceVersion);
			size += ProtocolParser.SizeOfUInt32(byteCount13) + byteCount13;
		}
		if (HasGraphicsMemorySize)
		{
			size++;
			size += ProtocolParser.SizeOfUInt64((ulong)GraphicsMemorySize);
		}
		if (HasGraphicsMultiThreaded)
		{
			size++;
			size++;
		}
		if (HasGraphicsShaderLevel)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)GraphicsShaderLevel);
		}
		if (HasGraphicsUVStartsAtTop)
		{
			size += 2;
			size++;
		}
		if (HasHasDynamicUniformArrayIndexingInFragmentShaders)
		{
			size += 2;
			size++;
		}
		if (HasHasHiddenSurfaceRemovalOnGPU)
		{
			size += 2;
			size++;
		}
		if (HasHasMipMaxLevel)
		{
			size += 2;
			size++;
		}
		if (HasMaxComputeBufferInputsCompute)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)MaxComputeBufferInputsCompute);
		}
		if (HasMaxComputeBufferInputsDomain)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)MaxComputeBufferInputsDomain);
		}
		if (HasMaxComputeBufferInputsFragment)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)MaxComputeBufferInputsFragment);
		}
		if (HasMaxComputeBufferInputsGeometry)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)MaxComputeBufferInputsGeometry);
		}
		if (HasMaxComputeBufferInputsHull)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)MaxComputeBufferInputsHull);
		}
		if (HasMaxComputeBufferInputsVertex)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)MaxComputeBufferInputsVertex);
		}
		if (HasMaxComputeWorkGroupSize)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)MaxComputeWorkGroupSize);
		}
		if (HasMaxComputeWorkGroupSizeX)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)MaxComputeWorkGroupSizeX);
		}
		if (HasMaxComputeWorkGroupSizeY)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)MaxComputeWorkGroupSizeY);
		}
		if (HasMaxComputeWorkGroupSizeZ)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)MaxComputeWorkGroupSizeZ);
		}
		if (HasMaxCubemapSize)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)MaxCubemapSize);
		}
		if (HasMaxTextureSize)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)MaxTextureSize);
		}
		if (HasMinConstantBufferOffsetAlignment)
		{
			size += 2;
			size++;
		}
		if (HasNpotSupport)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)NpotSupport);
		}
		if (HasOperatingSystem)
		{
			size += 2;
			uint byteCount35 = (uint)Encoding.UTF8.GetByteCount(OperatingSystem);
			size += ProtocolParser.SizeOfUInt32(byteCount35) + byteCount35;
		}
		if (HasOperatingSystemFamily)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)OperatingSystemFamily);
		}
		if (HasProcessorCount)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)ProcessorCount);
		}
		if (HasProcessorFrequency)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)ProcessorFrequency);
		}
		if (HasProcessorType)
		{
			size += 2;
			uint byteCount39 = (uint)Encoding.UTF8.GetByteCount(ProcessorType);
			size += ProtocolParser.SizeOfUInt32(byteCount39) + byteCount39;
		}
		if (HasRenderingThreadingMode)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)RenderingThreadingMode);
		}
		if (HasSupportedRandomWriteTargetCount)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)SupportedRandomWriteTargetCount);
		}
		if (HasSupportedRenderTargetCount)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)SupportedRenderTargetCount);
		}
		if (HasSupports2DArrayTextures)
		{
			size += 2;
			size++;
		}
		if (HasSupports32bitsIndexBuffer)
		{
			size += 2;
			size++;
		}
		if (HasSupports3DRenderTextures)
		{
			size += 2;
			size++;
		}
		if (HasSupports3DTextures)
		{
			size += 2;
			size++;
		}
		if (HasSupportsAccelerometer)
		{
			size += 2;
			size++;
		}
		if (HasSupportsAsyncCompute)
		{
			size += 2;
			size++;
		}
		if (HasSupportsAsyncGPUReadback)
		{
			size += 2;
			size++;
		}
		if (HasSupportsAudio)
		{
			size += 2;
			size++;
		}
		if (HasSupportsComputeShaders)
		{
			size += 2;
			size++;
		}
		if (HasSupportsCubemapArrayTextures)
		{
			size += 2;
			size++;
		}
		if (HasSupportsGeometryShaders)
		{
			size += 2;
			size++;
		}
		if (HasSupportsGraphicsFence)
		{
			size += 2;
			size++;
		}
		if (HasSupportsGyroscope)
		{
			size += 2;
			size++;
		}
		if (HasSupportsHardwareQuadTopology)
		{
			size += 2;
			size++;
		}
		if (HasSupportsInstancing)
		{
			size += 2;
			size++;
		}
		if (HasSupportsLocationService)
		{
			size += 2;
			size++;
		}
		if (HasSupportsMipStreaming)
		{
			size += 2;
			size++;
		}
		if (HasSupportsMotionVectors)
		{
			size += 2;
			size++;
		}
		if (HasSupportsMultisampleAutoResolve)
		{
			size += 2;
			size++;
		}
		if (HasSupportsMultisampledTextures)
		{
			size += 2;
			size++;
		}
		if (HasSupportsRawShadowDepthSampling)
		{
			size += 2;
			size++;
		}
		if (HasSupportsRayTracing)
		{
			size += 2;
			size++;
		}
		if (HasSupportsSeparatedRenderTargetsBlend)
		{
			size += 2;
			size++;
		}
		if (HasSupportsSetConstantBuffer)
		{
			size += 2;
			size++;
		}
		if (HasSupportsShadows)
		{
			size += 2;
			size++;
		}
		if (HasSupportsSparseTextures)
		{
			size += 2;
			size++;
		}
		if (HasSupportsStoreAndResolveAction)
		{
			size += 2;
			size++;
		}
		if (HasSupportsTessellationShaders)
		{
			size += 2;
			size++;
		}
		if (HasSupportsTextureWrapMirrorOnce)
		{
			size += 2;
			size++;
		}
		if (HasSupportsVibration)
		{
			size += 2;
			size++;
		}
		if (HasSystemMemorySize)
		{
			size += 2;
			size += ProtocolParser.SizeOfUInt64((ulong)SystemMemorySize);
		}
		if (HasUnsupportedIdentifier)
		{
			size += 2;
			uint byteCount74 = (uint)Encoding.UTF8.GetByteCount(UnsupportedIdentifier);
			size += ProtocolParser.SizeOfUInt32(byteCount74) + byteCount74;
		}
		if (HasUsesLoadStoreActions)
		{
			size += 2;
			size++;
		}
		if (HasUsesReversedZBuffer)
		{
			size += 2;
			size++;
		}
		if (HasGraphicsFormatForLDR)
		{
			size += 2;
			uint byteCount77 = (uint)Encoding.UTF8.GetByteCount(GraphicsFormatForLDR);
			size += ProtocolParser.SizeOfUInt32(byteCount77) + byteCount77;
		}
		if (HasGraphicsFormatForHDR)
		{
			size += 2;
			uint byteCount78 = (uint)Encoding.UTF8.GetByteCount(GraphicsFormatForHDR);
			size += ProtocolParser.SizeOfUInt32(byteCount78) + byteCount78;
		}
		if (SupportedRenderTextureFormats.Count > 0)
		{
			foreach (string i79 in SupportedRenderTextureFormats)
			{
				size += 2;
				uint byteCount79 = (uint)Encoding.UTF8.GetByteCount(i79);
				size += ProtocolParser.SizeOfUInt32(byteCount79) + byteCount79;
			}
		}
		if (SupportedTextureFormats.Count > 0)
		{
			foreach (string i80 in SupportedTextureFormats)
			{
				size += 2;
				uint byteCount80 = (uint)Encoding.UTF8.GetByteCount(i80);
				size += ProtocolParser.SizeOfUInt32(byteCount80) + byteCount80;
			}
		}
		return size;
	}
}
