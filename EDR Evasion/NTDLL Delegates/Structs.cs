
public class Structs
{
    [Flags]
    public enum ProcessAccessFlags : uint
    {
        PROCESS_ALL_ACCESS = 0x001F0FFF,
        PROCESS_CREATE_PROCESS = 0x0080,
        PROCESS_CREATE_THREAD = 0x0002,
        PROCESS_DUP_HANDLE = 0x0040,
        PROCESS_QUERY_INFORMATION = 0x0400,
        PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
        PROCESS_SET_INFORMATION = 0x0200,
        PROCESS_SET_QUOTA = 0x0100,
        PROCESS_SUSPEND_RESUME = 0x0800,
        PROCESS_TERMINATE = 0x0001,
        PROCESS_VM_OPERATION = 0x0008,
        PROCESS_VM_READ = 0x0010,
        PROCESS_VM_WRITE = 0x0020,
        SYNCHRONIZE = 0x00100000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OBJECT_ATTRIBUTES
    {
        public int Length;
        public IntPtr RootDirectory;
        public IntPtr objectName;
        public uint Attributes;
        public IntPtr SecurityDescriptor;
        public IntPtr SecurityQualityOfService;

        public OBJECT_ATTRIBUTES(string name, uint attrs)
        {
            Length = 0;
            RootDirectory = IntPtr.Zero;
            objectName = IntPtr.Zero;
            Attributes = attrs;
            SecurityDescriptor = IntPtr.Zero;
            SecurityQualityOfService = IntPtr.Zero;
            Length = Marshal.SizeOf(this);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CLIENT_ID
    {
        public IntPtr UniqueProcess;
        public IntPtr UniqueThread;
    }

    [Flags()]
    public enum TOKEN_ACCESS_FLAGS : int
    {
        StandardRightsRequired = 0x000F0000,
        StandardRightsRead = 0x00020000,
        TokenAssignPrimary = 0x0001,
        TokenDuplicate = 0x0002,
        TokenImpersonate = 0x0004,
        TokenQuery = 0x0008,
        TokenQuerySource = 0x0010,
        TokenAdjustPrivileges = 0x0020,
        TokenAdjustGroups = 0x0040,
        TokenAdjustDefault = 0x0080,
        TokenAdjustSessionId = 0x0100,
        TokenRead = (StandardRightsRead | TokenQuery),
        TokenAllAccess = (StandardRightsRequired | TokenAssignPrimary |
            TokenDuplicate | TokenImpersonate | TokenQuery | TokenQuerySource |
            TokenAdjustPrivileges | TokenAdjustGroups | TokenAdjustDefault |
            TokenAdjustSessionId)
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct OBJECT_ATTRIBUTES
    {
        public ulong Length;
        public IntPtr RootDirectory;
        public IntPtr ObjectName;
        public ulong Attributes;
        public IntPtr SecurityDescriptor;
        public IntPtr SecurityQualityOfService;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IO_STATUS_BLOCK
    {
        public uint Status;
        public IntPtr Information;
    }

    [Flags]
    public enum ACCESS_MASK : uint
    {
        DELETE = 0x00010000,
        READ_CONTROL = 0x00020000,
        WRITE_DAC = 0x00040000,
        WRITE_OWNER = 0x00080000,
        SYNCHRONIZE = 0x00100000,
        STANDARD_RIGHTS_REQUIRED = 0x000F0000,
        STANDARD_RIGHTS_READ = 0x00020000,
        STANDARD_RIGHTS_WRITE = 0x00020000,
        STANDARD_RIGHTS_EXECUTE = 0x00020000,
        STANDARD_RIGHTS_ALL = 0x001F0000,
        SPECIFIC_RIGHTS_ALL = 0x0000FFF,
        ACCESS_SYSTEM_SECURITY = 0x01000000,
        MAXIMUM_ALLOWED = 0x02000000,
        GENERIC_READ = 0x80000000,
        GENERIC_WRITE = 0x40000000,
        GENERIC_EXECUTE = 0x20000000,
        GENERIC_ALL = 0x10000000,
        DESKTOP_READOBJECTS = 0x00000001,
        DESKTOP_CREATEWINDOW = 0x00000002,
        DESKTOP_CREATEMENU = 0x00000004,
        DESKTOP_HOOKCONTROL = 0x00000008,
        DESKTOP_JOURNALRECORD = 0x00000010,
        DESKTOP_JOURNALPLAYBACK = 0x00000020,
        DESKTOP_ENUMERATE = 0x00000040,
        DESKTOP_WRITEOBJECTS = 0x00000080,
        DESKTOP_SWITCHDESKTOP = 0x00000100,
        WINSTA_ENUMDESKTOPS = 0x00000001,
        WINSTA_READATTRIBUTES = 0x00000002,
        WINSTA_ACCESSCLIPBOARD = 0x00000004,
        WINSTA_CREATEDESKTOP = 0x00000008,
        WINSTA_WRITEATTRIBUTES = 0x00000010,
        WINSTA_ACCESSGLOBALATOMS = 0x00000020,
        WINSTA_EXITWINDOWS = 0x00000040,
        WINSTA_ENUMERATE = 0x00000100,
        WINSTA_READSCREEN = 0x00000200,
        WINSTA_ALL_ACCESS = 0x0000037F,
        SECTION_ALL_ACCESS = 0x10000000,
        SECTION_QUERY = 0x0001,
        SECTION_MAP_WRITE = 0x0002,
        SECTION_MAP_READ = 0x0004,
        SECTION_MAP_EXECUTE = 0x0008,
        SECTION_EXTEND_SIZE = 0x0010
    };

    public enum PROCESSINFOCLASS : int
    {
        ProcessBasicInformation = 0x00,
        ProcessQuotaLimits = 0x01,
        ProcessIoCounters = 0x02,
        ProcessVmCounters = 0x03,
        ProcessTimes = 0x04,
        ProcessBasePriority = 0x05,
        ProcessRaisePriority = 0x06,
        ProcessDebugPort = 0x07,
        ProcessExceptionPort = 0x08,
        ProcessAccessToken = 0x09,
        ProcessLdtInformation = 0x0A,
        ProcessLdtSize = 0x0B,
        ProcessDefaultHardErrorMode = 0x0C,
        ProcessIoPortHandlers = 0x0D,
        ProcessPooledUsageAndLimits = 0x0E,
        ProcessWorkingSetWatch = 0x0F,
        ProcessUserModeIOPL = 0x10,
        ProcessEnableAlignmentFaultFixup = 0x11,
        ProcessPriorityClass = 0x12,
        ProcessWx86Information = 0x13,
        ProcessHandleCount = 0x14,
        ProcessAffinityMask = 0x15,
        ProcessPriorityBoost = 0x16,
        ProcessDeviceMap = 0x17,
        ProcessSessionInformation = 0x18,
        ProcessForegroundInformation = 0x19,
        ProcessWow64Information = 0x1A,
        ProcessImageFileName = 0x1B,
        ProcessLUIDDeviceMapsEnabled = 0x1C,
        ProcessBreakOnTermination = 0x1D,
        ProcessDebugObjectHandle = 0x1E,
        ProcessDebugFlags = 0x1F,
        ProcessHandleTracing = 0x20,
        ProcessIoPriority = 0x21,
        ProcessExecuteFlags = 0x22,
        ProcessResourceManagement = 0x23,
        ProcessCookie = 0x24,
        ProcessImageInformation = 0x25,
        ProcessCycleTime = 0x26,
        ProcessPagePriority = 0x27,
        ProcessInstrumentationCallback = 0x28,
        ProcessThreadStackAllocation = 0x29,
        ProcessWorkingSetWatchEx = 0x2A,
        ProcessImageFileNameWin32 = 0x2B,
        ProcessImageFileMapping = 0x2C,
        ProcessAffinityUpdateMode = 0x2D,
        ProcessMemoryAllocationMode = 0x2E,
        ProcessGroupInformation = 0x2F,
        ProcessTokenVirtualizationEnabled = 0x30,
        ProcessConsoleHostProcess = 0x31,
        ProcessWindowInformation = 0x32,
        ProcessHandleInformation = 0x33,
        ProcessMitigationPolicy = 0x34,
        ProcessDynamicFunctionTableInformation = 0x35,
        ProcessHandleCheckingMode = 0x36,
        ProcessKeepAliveCount = 0x37,
        ProcessRevokeFileHandles = 0x38,
        ProcessWorkingSetControl = 0x39,
        ProcessHandleTable = 0x3A,
        ProcessCheckStackExtentsMode = 0x3B,
        ProcessCommandLineInformation = 0x3C,
        ProcessProtectionInformation = 0x3D,
        ProcessMemoryExhaustion = 0x3E,
        ProcessFaultInformation = 0x3F,
        ProcessTelemetryIdInformation = 0x40,
        ProcessCommitReleaseInformation = 0x41,
        ProcessDefaultCpuSetsInformation = 0x42,
        ProcessAllowedCpuSetsInformation = 0x43,
        ProcessSubsystemProcess = 0x44,
        ProcessJobMemoryInformation = 0x45,
        ProcessInPrivate = 0x46,
        ProcessRaiseUMExceptionOnInvalidHandleClose = 0x47,
        ProcessIumChallengeResponse = 0x48,
        ProcessChildProcessInformation = 0x49,
        ProcessHighGraphicsPriorityInformation = 0x4A,
        ProcessSubsystemInformation = 0x4B,
        ProcessEnergyValues = 0x4C,
        ProcessActivityThrottleState = 0x4D,
        ProcessActivityThrottlePolicy = 0x4E,
        ProcessWin32kSyscallFilterInformation = 0x4F,
        ProcessDisableSystemAllowedCpuSets = 0x50,
        ProcessWakeInformation = 0x51,
        ProcessEnergyTrackingState = 0x52,
        ProcessManageWritesToExecutableMemory = 0x53,
        ProcessCaptureTrustletLiveDump = 0x54,
        ProcessTelemetryCoverage = 0x55,
        ProcessEnclaveInformation = 0x56,
        ProcessEnableReadWriteVmLogging = 0x57,
        ProcessUptimeInformation = 0x58,
        ProcessImageSection = 0x59,
        ProcessDebugAuthInformation = 0x5A,
        ProcessSystemResourceManagement = 0x5B,
        ProcessSequenceNumber = 0x5C,
        ProcessLoaderDetour = 0x5D,
        ProcessSecurityDomainInformation = 0x5E,
        ProcessCombineSecurityDomainsInformation = 0x5F,
        ProcessEnableLogging = 0x60,
        ProcessLeapSecondInformation = 0x61,
        ProcessFiberShadowStackAllocation = 0x62,
        ProcessFreeFiberShadowStackAllocation = 0x63,
        MaxProcessInfoClass = 0x64
    };

    [Flags]
    public enum ThreadAccess : uint
    {
        Terminate = 0x0001,
        SuspendResume = 0x0002,
        Alert = 0x0004,
        GetContext = 0x0008,
        SetContext = 0x0010,
        SetInformation = 0x0020,
        QueryInformation = 0x0040,
        SetThreadToken = 0x0080,
        Impersonate = 0x0100,
        DirectImpersonation = 0x0200,
        SetLimitedInformation = 0x0400,
        QueryLimitedInformation = 0x0800,
        All = StandardRights.Required | StandardRights.Synchronize | 0x3ff
    }

    public enum MEMORYINFOCLASS : int
    {
        MemoryBasicInformation = 0,
        MemoryWorkingSetList = 1,
        MemorySectionName = 2,
        MemoryBasicVlmInformation = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OSVERSIONINFOEX
    {
        public uint OSVersionInfoSize;
        public uint MajorVersion;
        public uint MinorVersion;
        public uint BuildNumber;
        public uint PlatformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string CSDVersion;
        public ushort ServicePackMajor;
        public ushort ServicePackMinor;
        public ushort SuiteMask;
        public byte ProductType;
        public byte Reserved;
    }
}
