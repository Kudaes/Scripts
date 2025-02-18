#[macro_use]
extern crate litcrypt2;
use_litcrypt!();

use std::{ffi::{c_void, OsStr}, os::windows::ffi::OsStrExt, ptr::{self, null_mut}};

use windows::{ Win32::{Foundation::{HANDLE, UNICODE_STRING}, System::IO::IO_STATUS_BLOCK}};

pub type NtCreateNamedPipeFile = unsafe extern "system" fn(*mut HANDLE, u32, *mut OBJECT_ATTRIBUTES, *mut IO_STATUS_BLOCK, u32,u32,u32,u32,u32,u32,u32,u32,u32, *mut i64) -> i32;

#[repr(C)]
pub struct OBJECT_ATTRIBUTES {
    pub Length: u32,
    pub unknown: u32, // wtf
    pub RootDirectory: HANDLE,
    pub ObjectName: *const UNICODE_STRING,
    pub Attributes: u32,
    pub SecurityDescriptor: *const c_void,
    pub SecurityQualityOfService: *const c_void,
}

#[repr(C)]
struct SECURITY_DESCRIPTOR {
    Revision: u8,
    Sbz1: u8,
    Control: u16,
    Owner: *mut c_void,
    Group: *mut c_void,
    Sacl: *mut c_void,
    Dacl: *mut c_void,
}
const INVALID_HANDLE_VALUE: HANDLE = HANDLE{0:-1};


fn main() {
    unsafe 
    {   
        let pipe_name = &lc!(r"\??\pipe\3F2504E0-4F89-11D3-9A0C-0305E82C3301\pipe\srvsvc");
        let mut namedpipe_name_utf16: Vec<u16> = pipe_name.encode_utf16().collect();
        namedpipe_name_utf16.push(0);
    
        let u = UNICODE_STRING::default();
        let object_name: *mut UNICODE_STRING = std::mem::transmute(&u);
        dinvoke_rs::dinvoke::rtl_init_unicode_string(object_name, namedpipe_name_utf16.as_ptr());

        let mut object_attributes: OBJECT_ATTRIBUTES = std::mem::zeroed();
        object_attributes.Length = size_of::<OBJECT_ATTRIBUTES>() as u32;
        object_attributes.ObjectName = object_name;
        object_attributes.Attributes = 0x40; // Case Insensitive 
        object_attributes.RootDirectory = HANDLE{0:0}; // Local Machine


        let object_ptr: *mut OBJECT_ATTRIBUTES = std::mem::transmute(&object_attributes);

        let io = IO_STATUS_BLOCK::default();
        let io_ptr: *mut IO_STATUS_BLOCK = std::mem::transmute(&io);
        
        let default_timeout = -500000i64; // Sacado de IDA + Hook con dinvoke, con null peta
        let timeout_ptr: *mut i64 = std::mem::transmute(&default_timeout);
        let handle = HANDLE::default();
        let phandle: *mut HANDLE = std::mem::transmute(&handle);

        let r: Option<i32>;
        let f: NtCreateNamedPipeFile; 

        let desired_access   = 0x00000000C0100000u32; // Sacado de IDA

        dinvoke_rs::dinvoke::execute_syscall!(
            &lc!("NtCreateNamedPipeFile"),
            f,
            r,
            phandle,desired_access,object_ptr,io_ptr,0x3,0x3,0x20,0,0,0,0xA,0x800,0x800,timeout_ptr);
        
        println!("resultado: {:x}", r.unwrap() as i32);

    }


}