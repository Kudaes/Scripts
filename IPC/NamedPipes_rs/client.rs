#![crate_type = "cdylib"]

use std::{ptr, mem::size_of, ops::Add};

use bindings::Windows::Win32::{Foundation::HANDLE, System::{SystemServices::OVERLAPPED}};
use data::{CreateFileA, GENERIC_READ, GENERIC_WRITE, PVOID, GetLastError, CloseHandle};

//cargo rustc -- --crate-type cdylib 

static ADDR1: i64 = 12345678;
static ADDR2: i64 = 87654321;
static PATCH1: [u8;13] = [1;13];
static PATCH2: [u8;13] = [2;13];

fn main() {
  
}

#[no_mangle]
pub unsafe extern "Rust" fn get_addr1 () -> i32
{   
    unsafe
    {   
        let k = "kernel32.dll";
        let kernel32 = dinvoke::get_module_base_address(k);

        let f:data::CreateFileA;
        let r: Option<HANDLE>;
        let mut pipe_name: Vec<u16> = "\\\\.\\pipe\\test111".to_string().encode_utf16().collect();
        pipe_name.push(0);        

        dinvoke::dynamic_invoke!(
            kernel32,
            "CreateFileW",
            f,
            r,
            pipe_name.as_ptr(),
            GENERIC_READ | GENERIC_WRITE,
            0,
            ptr::null(),
            3, // OPEN_EXISTING
            0,
            HANDLE::default()
        );

        let pipe_handle = r.unwrap();

        let buffer = "hola que tal".as_ptr() as *mut u8;
        let buffer: PVOID = std::mem::transmute(buffer);
        let bytes_written: *mut u32 = std::mem::transmute(&u32::default());
        let over: Vec<u8> = vec![0u8; size_of::<OVERLAPPED>()];
        let over: *mut OVERLAPPED = std::mem::transmute(over.as_ptr());
        let r = dinvoke::write_file(pipe_handle, buffer, 13, bytes_written, over);

        1
    }
}


#[no_mangle]
pub fn get_addr2 () -> i64
{   
    unsafe
    {
        let r: *mut i64 = std::mem::transmute(&ADDR2);
        r as i64
    }
}

#[no_mangle]
pub fn get_addr3 () -> i64
{   
    unsafe
    {
        let r: *mut i64 = std::mem::transmute(&PATCH1);
        r as i64
    }
}

#[no_mangle]
pub fn get_addr4 () -> i64
{   
    unsafe
    {
        let r: *mut i64 = std::mem::transmute(&PATCH2);
        r as i64
    }
}
