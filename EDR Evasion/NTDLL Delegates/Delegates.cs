
public class Delegates
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NtOpenProcess(
        ref IntPtr hProcess, 
        Structs.ProcessAccessFlags desiredAccess, 
        ref Structs.OBJECT_ATTRIBUTES objectAttributes,
        ref Structs.CLIENT_ID clientId);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NtOpenProcessToken(
        IntPtr processHandle, 
        Structs.TOKEN_ACCESS_FLAGS desiredAccess, 
        out IntPtr tokenHandle);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NtReadVirtualMemory(
        IntPtr processHandle, 
        IntPtr baseAddress, 
        out IntPtr buffer, 
        uint numberOfBytesToRead, 
        out IntPtr numberOfBytesReaded);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int NtWriteVirtualMemory(
        IntPtr processHandle, 
        IntPtr address, 
        byte[] buffer, 
        UIntPtr size, 
        IntPtr bytesWrittenBuffer);

    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int NtCreateFile(
        out Microsoft.Win32.SafeHandles.SafeFileHandle fileHandle,
        int desiredAccess,
        ref Structs.OBJECT_ATTRIBUTES objectAttributes,
        out Structs.IO_STATUS_BLOCK ioStatusBlock,
        ref long allocationSize,
        uint fileAttributes,
        System.IO.FileShare shareAccess,
        uint createDisposition,
        uint createOptions,
        IntPtr eaBuffer,
        uint eaLength);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate NTSTATUS NtCreateThreadEx(
        out IntPtr threadHandle,
        Structs.ACCESS_MASK desiredAccess,
        IntPtr objectAttributes,
        IntPtr processHandle,
        IntPtr startAddress,
        IntPtr parameter,
        bool createSuspended,
        int stackZeroBits,
        int sizeOfStack,
        int maximumStackSize,
        IntPtr attributeList);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate NTSTATUS RtlCreateUserThread(
        IntPtr Process,
        IntPtr ThreadSecurityDescriptor,
        bool CreateSuspended,
        IntPtr ZeroBits,
        IntPtr MaximumStackSize,
        IntPtr CommittedStackSize,
        IntPtr StartAddress,
        IntPtr Parameter,
        ref IntPtr Thread,
        IntPtr ClientId);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate NTSTATUS NtCreateSection(
        ref IntPtr SectionHandle,
        uint DesiredAccess,
        IntPtr ObjectAttributes,
        ref ulong MaximumSize,
        uint SectionPageProtection,
        uint AllocationAttributes,
        IntPtr FileHandle);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate NTSTATUS NtUnmapViewOfSection(
        IntPtr hProc,
        IntPtr baseAddr);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate NTSTATUS NtMapViewOfSection(
        IntPtr SectionHandle,
        IntPtr ProcessHandle,
        out IntPtr BaseAddress,
        IntPtr ZeroBits,
        IntPtr CommitSize,
        IntPtr SectionOffset,
        out ulong ViewSize,
        uint InheritDisposition,
        uint AllocationType,
        uint Win32Protect);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint LdrLoadDll(
        IntPtr PathToFile,
        uint dwFlags,
        ref UNICODE_STRING ModuleFileName,
        ref IntPtr ModuleHandle);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void RtlInitUnicodeString(
        ref UNICODE_STRING DestinationString,
        [MarshalAs(UnmanagedType.LPWStr)]
        string SourceString);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void RtlZeroMemory(
        IntPtr Destination,
        int length);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtQueryInformationProcess(
        IntPtr processHandle,
        Structs.PROCESSINFOCLASS processInformationClass,
        IntPtr processInformation,
        int processInformationLength,
        ref uint returnLength);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtQueueApcThread(
        IntPtr ThreadHandle,
        IntPtr ApcRoutine,
        IntPtr ApcArgument1,
        IntPtr ApcArgument2,
        IntPtr ApcArgument3);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtOpenThread(
        ref IntPtr ThreadHandle,
        Structs.ThreadAccess DesiredAccess,
        ref Structs.OBJECT_ATTRIBUTES ObjectAttributes,
        ref Structs.CLIENT_ID ClientId);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtAllocateVirtualMemory(
        IntPtr ProcessHandle,
        ref IntPtr BaseAddress,
        IntPtr ZeroBits,
        ref IntPtr RegionSize,
        uint AllocationType,
        uint Protect);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtFreeVirtualMemory(
        IntPtr ProcessHandle,
        ref IntPtr BaseAddress,
        ref IntPtr RegionSize,
        uint FreeType);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtQueryVirtualMemory(
        IntPtr ProcessHandle,
        IntPtr BaseAddress,
        Structs.MEMORYINFOCLASS MemoryInformationClass,
        IntPtr MemoryInformation,
        uint MemoryInformationLength,
        ref uint ReturnLength);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtProtectVirtualMemory(
        IntPtr ProcessHandle,
        ref IntPtr BaseAddress,
        ref IntPtr RegionSize,
        uint NewProtect,
        ref uint OldProtect);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint RtlUnicodeStringToAnsiString(
                ref ANSI_STRING DestinationString,
                ref UNICODE_STRING SourceString,
                bool AllocateDestinationString);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint LdrGetProcedureAddress(
        IntPtr hModule,
        IntPtr FunctionName,
        IntPtr Ordinal,
        ref IntPtr FunctionAddress);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint RtlGetVersion(
        ref OSVERSIONINFOEX VersionInformation);

}
