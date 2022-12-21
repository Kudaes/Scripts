use std::{collections::BTreeMap, ffi::c_void};

use bindings::Windows::Win32::{Foundation::{BOOL, HANDLE, HINSTANCE, PSTR}, Security::SECURITY_ATTRIBUTES, System::{Diagnostics::Debug::{IMAGE_DATA_DIRECTORY, IMAGE_OPTIONAL_HEADER32, IMAGE_SECTION_HEADER, MINIDUMP_CALLBACK_INFORMATION, MINIDUMP_EXCEPTION_INFORMATION, MINIDUMP_USER_STREAM_INFORMATION}, SystemServices::OVERLAPPED, Kernel::UNICODE_STRING, Threading::{RTL_USER_PROCESS_PARAMETERS}, WindowsProgramming::{OBJECT_ATTRIBUTES, IO_STATUS_BLOCK}}};
use winapi::{shared::ntdef::LARGE_INTEGER};

pub type PVOID = *mut c_void;
pub type DWORD = u32;
pub type EAT = BTreeMap<i64,String>;
pub type EntryPoint =  extern "system" fn (HINSTANCE, u32, *mut c_void) -> BOOL;
pub type LoadLibraryA = unsafe extern "system" fn (PSTR) -> HINSTANCE;
pub type OpenProcess = unsafe extern "system" fn (u32, i32, u32) -> HANDLE;
pub type QueryFullProcessImageNameW = unsafe extern "system" fn (HANDLE, u32, *mut u16, *mut u32) -> i32;
pub type MiniDumpWriteDump = unsafe extern "system" fn (HANDLE, u32, HANDLE, u32, *mut MINIDUMP_EXCEPTION_INFORMATION,
    *mut MINIDUMP_USER_STREAM_INFORMATION, *mut MINIDUMP_CALLBACK_INFORMATION) -> i32;
pub type CreateFileA = unsafe extern "system" fn (*mut u8, u32, u32, *const SECURITY_ATTRIBUTES, u32, u32, HANDLE) -> HANDLE;
pub type ReadFile = unsafe extern "system" fn (HANDLE, PVOID, u32, *mut u32, *mut OVERLAPPED) -> i32; 
pub type CreateTransaction = unsafe extern "system" fn (*mut SECURITY_ATTRIBUTES, *mut GUID, u32, u32, u32, u32, *mut u16) -> HANDLE;
pub type CreateFileTransactedA = unsafe extern "system" fn (*mut u8, u32, u32, *const SECURITY_ATTRIBUTES, u32, u32, HANDLE,
    HANDLE, *const u32, PVOID) -> HANDLE;
pub type GetLastError = unsafe extern "system" fn () -> u32;
pub type CloseHandle = unsafe extern "system" fn (HANDLE) -> i32;
pub type LdrGetProcedureAddress = unsafe extern "system" fn (PVOID, *mut String, u32, *mut PVOID) -> i32;
pub type NtCreateUserProcess = unsafe extern "system" fn (*mut HANDLE, *mut HANDLE,u32, u32, *mut OBJECT_ATTRIBUTES,*mut OBJECT_ATTRIBUTES, u32, u32, PVOID, *mut PS_CREATE_INFO, *mut PS_ATTRIBUTE_LIST) -> i32;
pub type NtWriteVirtualMemory = unsafe extern "system" fn (HANDLE, PVOID, PVOID, usize, *mut usize) -> i32;
pub type NtProtectVirtualMemory = unsafe extern "system" fn (HANDLE, *mut PVOID, *mut usize, u32, *mut u32) -> i32;
pub type NtAllocateVirtualMemory = unsafe extern "system" fn (HANDLE, *mut PVOID, usize, *mut usize, u32, u32) -> i32;
pub type NtQueryInformationProcess = unsafe extern "system" fn (HANDLE, u32, PVOID, u32, *mut u32) -> i32;
pub type NtQuerySystemInformation = unsafe extern "system" fn (u32, PVOID, u32, *mut u32) -> i32;
pub type NtDuplicateObject = unsafe extern "system" fn (HANDLE, HANDLE, HANDLE, *mut HANDLE, u32, u32, u32) -> i32;
pub type NtQueryObject = unsafe extern "system" fn (HANDLE, u32, PVOID, u32, *mut u32) -> i32;
pub type NtOpenFile = unsafe extern "system" fn (*mut HANDLE, u32, *mut OBJECT_ATTRIBUTES, *mut IO_STATUS_BLOCK, u32, u32) -> i32;
pub type NtCreateSection = unsafe extern "system" fn (*mut HANDLE, u32, *mut OBJECT_ATTRIBUTES, *mut LARGE_INTEGER, u32, u32, HANDLE) -> i32;
pub type NtMapViewOfSection = unsafe extern "system" fn (HANDLE, HANDLE, *mut PVOID, usize, usize, *mut LARGE_INTEGER, *mut usize, u32, u32, u32) -> i32;
pub type RtlAdjustPrivilege = unsafe extern "system" fn (u32, u8, u8, *mut u8) -> i32;
pub type RtlInitUnicodeString = unsafe extern "system" fn (*mut UNICODE_STRING, *const u16) -> () ;
pub type RtlZeroMemory = unsafe extern "system" fn (PVOID, usize) -> ();
pub type RtlCreateProcessParametersEx = unsafe extern "system" fn (*mut *mut RTL_USER_PROCESS_PARAMETERS, *mut UNICODE_STRING, *mut UNICODE_STRING, *mut UNICODE_STRING, *mut UNICODE_STRING, PVOID, *mut UNICODE_STRING, *mut UNICODE_STRING, *mut UNICODE_STRING, *mut UNICODE_STRING, u32) -> i32;
 
pub const DLL_PROCESS_DETACH: u32 = 0;
pub const DLL_PROCESS_ATTACH: u32 = 1;
pub const DLL_THREAD_ATTACH: u32 = 2;
pub const DLL_THREAD_DETACH: u32 = 3;

pub const PAGE_READONLY: u32 = 0x2;
pub const PAGE_READWRITE: u32 = 0x4;
pub const PAGE_EXECUTE_READWRITE: u32 = 0x40;
pub const PAGE_EXECUTE_READ: u32 = 0x20;
pub const PAGE_EXECUTE: u32 = 0x10;

pub const MEM_COMMIT: u32 = 0x1000;
pub const MEM_RESERVE: u32 = 0x2000;

pub const SECTION_MEM_READ: u32 = 0x40000000;
pub const SECTION_MEM_WRITE: u32 = 0x80000000;
pub const SECTION_MEM_EXECUTE: u32 = 0x20000000;

// Access mask
pub const GENERIC_READ: u32 = 0x80000000;
pub const GENERIC_WRITE: u32 = 0x40000000;
pub const GENERIC_EXECUTE: u32 = 0x20000000;
pub const GENERIC_ALL: u32 = 0x10000000;
pub const SECTION_ALL_ACCESS: u32 = 0x10000000;

//File share flags
pub const FILE_SHARE_NONE: u32 = 0x0;
pub const FILE_SHARE_READ: u32 = 0x1;
pub const FILE_SHARE_WRITE: u32 = 0x2;
pub const FILE_SHARE_DELETE: u32 = 0x4;

//File access flags
pub const DELETE: u32 = 0x10000;
pub const FILE_READ_DATA: u32 = 0x1;
pub const FILE_READ_ATTRIBUTES: u32 = 0x80;
pub const FILE_READ_EA: u32 = 0x8;
pub const READ_CONTROL: u32 = 0x20000;
pub const FILE_WRITE_DATA: u32 = 0x2;
pub const FILE_WRITE_ATTRIBUTES: u32 = 0x100;
pub const FILE_WRITE_EA: u32 = 0x10;
pub const FILE_APPEND_DATA: u32 = 0x4;
pub const WRITE_DAC: u32 = 0x40000;
pub const WRITE_OWNER: u32 = 0x80000;
pub const SYNCHRONIZE: u32 = 0x100000;
pub const FILE_EXECUTE: u32 = 0x20;

// File open flags
pub const FILE_SYNCHRONOUS_IO_NONALERT: u32 = 0x20;
pub const FILE_NON_DIRECTORY_FILE: u32 = 0x40;

pub const SEC_IMAGE: u32 = 0x1000000;

#[repr(C)]
pub struct PS_ATTRIBUTE_LIST {
    pub total_length: usize,
    pub attributes: [PS_ATTRIBUTE; 2],
}

#[repr(C)]
pub struct PS_CREATE_INFO {
    pub size: usize,
    pub unused: [u8;80],
}

#[repr(C)]
pub struct PS_ATTRIBUTE {
    pub attribute: usize,
    pub size: usize,
    pub union: PS_ATTRIBUTE_u,
    pub return_length: *mut usize,
}

#[repr(C)]
pub union PS_ATTRIBUTE_u {
    pub value: usize,
    pub value_ptr: PVOID,
}


#[derive(Clone,Copy,Default)]
#[repr(C)]
pub struct PS_CREATE_INFO_InitState {
    pub init_flags: u32,
    pub additional_file_access: u32,
}

#[derive(Clone,Copy)]
#[repr(C)]
pub struct PS_CREATE_INFO_u_SuccessState {
    pub output_flags: u32,
    pub file_handle: HANDLE,
    pub section_handle: HANDLE,
    pub user_process_parameters_native: u64,
    pub user_process_parameters_wow64: u32,
    pub current_parameter_flags: u32,
    pub peb_address_native: u64,
    pub peb_address_wow64: u32,
    pub manifest_address: u64,
    pub manifest_size: u32,
}

#[derive(Clone,Copy)]
#[repr(C)]
pub union PS_CREATE_INFO_u {
    pub init_state: PS_CREATE_INFO_InitState,
    pub file_handle: HANDLE,
    pub dll_characteristics: u16,
    pub ifeokey: HANDLE,
    pub success_state: PS_CREATE_INFO_u_SuccessState,
}


#[derive(Clone)]
#[repr(C)]
pub struct PeMetadata {
    pub pe: u32,
    pub is_32_bit: bool,
    pub image_file_header: IMAGE_FILE_HEADER,
    pub opt_header_32: IMAGE_OPTIONAL_HEADER32,
    pub opt_header_64: IMAGE_OPTIONAL_HEADER64,
    pub sections: Vec<IMAGE_SECTION_HEADER> 
}

impl Default for PeMetadata {
    fn default() -> PeMetadata {
        PeMetadata {
            pe: u32::default(),
            is_32_bit: false,
            image_file_header: IMAGE_FILE_HEADER::default(),
            opt_header_32: IMAGE_OPTIONAL_HEADER32::default(),
            opt_header_64: IMAGE_OPTIONAL_HEADER64::default(),
            sections: Vec::default(),  
        }
    }
}

#[repr(C)]
pub struct PeManualMap {
    pub decoy_module: String,
    pub base_address: i64,
    pub pe_info: PeMetadata,
}

#[repr(C)]
#[derive(Copy, Clone, Default, PartialEq, Debug, Eq)]
pub struct ApiSetNamespace {
    pub unused: [u8;12],
    pub count: i32, // offset 0x0C
    pub entry_offset: i32, // offset 0x10
}

#[repr(C)]
#[derive(Copy, Clone, Default, PartialEq, Debug, Eq)]
pub struct ApiSetNamespaceEntry {
    pub unused1: [u8;4],
    pub name_offset: i32, // offset 0x04
    pub name_length: i32, // offset 0x08
    pub unused2: [u8;4],
    pub value_offset: i32, // offset 0x10
    pub value_length: i32, // offset 0x14
}

#[repr(C)]
#[derive(Copy, Clone, Default, PartialEq, Debug, Eq)]
pub struct ApiSetValueEntry {
    pub flags: i32, // offset 0x00
    pub name_offset: i32, // offset 0x04
    pub name_count: i32, // offset 0x08
    pub value_offset: i32, // offset 0x0C
    pub value_count: i32, // offset 0x10
}

#[derive(Copy, Clone, Default, PartialEq, Debug, Eq)]
#[repr(C)]
pub struct IMAGE_FILE_HEADER {
    pub machine: u16,
    pub number_of_sections: u16,
    pub time_data_stamp: u32,
    pub pointer_to_symbol_table: u32,
    pub number_of_symbols: u32,
    pub size_of_optional_header: u16,
    pub characteristics: u16,
}

#[derive(Copy, Clone,Default)]
#[repr(C)] // required to keep fields order, otherwise Rust may change that order randomly
pub struct IMAGE_OPTIONAL_HEADER64 {
        pub magic: u16, 
        pub major_linker_version: u8, 
        pub minor_linker_version: u8, 
        pub size_of_code: u32, 
        pub size_of_initialized_data: u32, 
        pub size_of_unitialized_data: u32, 
        pub address_of_entry_point: u32, 
        pub base_of_code: u32, 
        pub image_base: u64, 
        pub section_alignment: u32, 
        pub file_alignment: u32, 
        pub major_operating_system_version: u16, 
        pub minor_operating_system_version: u16, 
        pub major_image_version: u16,
        pub minor_image_version: u16, 
        pub major_subsystem_version: u16,
        pub minor_subsystem_version: u16, 
        pub win32_version_value: u32, 
        pub size_of_image: u32, 
        pub size_of_headers: u32, 
        pub checksum: u32, 
        pub subsystem: u16, 
        pub dll_characteristics: u16, 
        pub size_of_stack_reserve: u64, 
        pub size_of_stack_commit: u64, 
        pub size_of_heap_reserve: u64, 
        pub size_of_heap_commit: u64, 
        pub loader_flags: u32, 
        pub number_of_rva_and_sizes: u32, 
        pub datas_directory: [IMAGE_DATA_DIRECTORY; 16], 
}

#[derive(Copy, Clone, Default, PartialEq, Debug, Eq)]
#[repr(C)]
pub struct GUID
{
    pub data1: u32,
    pub data2: u16,
    pub data3: u16,
    pub data4: [u8; 8],
}

#[repr(C)]
pub struct SYSTEM_HANDLE_INFORMATION {
    pub number_of_handles: u32,
    pub handles: Vec<SYSTEM_HANDLE_TABLE_ENTRY_INFO>,
}

#[repr(C)]
pub struct SYSTEM_HANDLE_TABLE_ENTRY_INFO {
    pub process_id: u16,
    pub creator_back_trace_index: u16,
    pub object_type_index: u8,
    pub handle_attributes: u8,
    pub handle_value: u16,
    pub object: PVOID,
    pub granted_access: u32,
}

#[repr(C)]
pub struct CURR_DIR
{
    pub dos_path: UNICODE_STRING,
    pub handle: HANDLE
}

#[repr(C)]
pub struct RTL_DRIVE_LETTER_CURDIR
{
    pub flags: u16,
    pub len: u16,
    pub timestamp: u32,
    pub dos_path: UNICODE_STRING
}

#[repr(C)]
pub struct USER_PROCESS_PARAMETERS 
{
    pub max_len: u32,
    pub len: u32,
    pub flags: u32,
    pub debug_flags: u32,
    pub console_handle: HANDLE,
    pub console_flags: u32,
    pub stdin: HANDLE,
    pub stdout: HANDLE,
    pub stderr: HANDLE,
    pub cur_dir: CURR_DIR,
    pub dll_path: UNICODE_STRING,
    pub image_path: UNICODE_STRING,
    pub command_name: UNICODE_STRING,
    pub environment: *mut u8,
    pub startx: u32,
    pub starty: u32,
    pub countx: u32,
    pub county: u32,
    pub charsx: u32,
    pub charsy: u32,
    pub attribute: u32,
    pub window_flags: u32,
    pub show_flags: u32,
    pub title: UNICODE_STRING,
    pub desktop_info: UNICODE_STRING,
    pub shell_info: UNICODE_STRING,
    pub runtime_data: UNICODE_STRING,
    pub current_directories: [RTL_DRIVE_LETTER_CURDIR; 32],
    pub encironment_size: usize,
    pub environment_version: usize,
    pub dependecy_data: PVOID,
    pub group_id: u32,
    pub loader_threads: u32
}
