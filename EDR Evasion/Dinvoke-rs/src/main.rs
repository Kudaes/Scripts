use std::ptr;
use std::ffi::CString;
use winproc;
use winapi::{shared::minwindef::HINSTANCE__};
use bindings::{Windows::Win32::{Foundation::{HINSTANCE,PSTR}}};

fn main() {

    unsafe 
    {    

        let module_base_address = get_module_base_address("kernel32.dll"); 

        if module_base_address != 0
        {
            let function_address = get_function_address(module_base_address, "LoadLibraryA".to_string());

            let func_ptr: extern "system" fn (PSTR) -> HINSTANCE = std::mem::transmute(function_address); // Similar a los delegados de C#
            let name = CString::new("kernel32.dll").expect("CString::new failed");
            let function_name = PSTR{0: name.as_ptr() as *mut u8};

            let result = func_ptr(function_name);
            println!("Base address of kernel32.dll: 0x{:o}", result.0 as u32);

        } 
        else 
        {
            println!("Error obtaining kernel32.dll base address");
        }

    }
}

fn get_module_base_address (module_name: &str) -> i64
{
    let process = winproc::Process::current();
    let modules = process.module_list().unwrap();
    let handle: *mut HINSTANCE__;
    for m in modules
    {
        if m.name().unwrap().to_lowercase() == module_name.to_ascii_lowercase()
        {
            handle = m.handle();
            return handle as i64;

        }
    }

    0
}

fn get_function_address(module_base_address: i64, function: String) -> *mut i32 {
    
    let mut function_ptr:*mut i32 = ptr::null_mut();

    unsafe
    {

        let pe_header = *((module_base_address + 0x3C) as *mut i32);
        let opt_header: i64 = module_base_address + (pe_header as i64) + 0x18;
        let magic = *(opt_header as *mut i16);
        let p_export: i64;

        if magic == 0x010b 
        {
            p_export = opt_header + 0x60;
        } 
        else 
        {
            p_export = opt_header + 0x70;
        }

        let export_rva = *(p_export as *mut i32);
        let ordinal_base = *((module_base_address + export_rva as i64 + 0x10) as *mut i32);
        let number_of_names = *((module_base_address + export_rva as i64 + 0x18) as *mut i32);
        let functions_rva = *((module_base_address + export_rva as i64 + 0x1C) as *mut i32);
        let names_rva = *((module_base_address + export_rva as i64 + 0x20) as *mut i32);
        let ordinals_rva = *((module_base_address + export_rva as i64 + 0x24) as *mut i32);

        for x in 0..number_of_names 
        {

            let address = *((module_base_address + names_rva as i64 + x as i64 * 4) as *mut i32);
            let mut function_name_ptr = (module_base_address + address as i64) as *mut u8;
            let mut function_name: String = "".to_string();

            while *function_name_ptr as char != '\0' // null byte
            { 
                function_name.push(*function_name_ptr as char);
                function_name_ptr = function_name_ptr.add(1);
            }

            if function_name.to_lowercase() == function.to_lowercase() 
            {
                let function_ordinal = *((module_base_address + ordinals_rva as i64 + x as i64 * 2) as *mut i16) as i32 + ordinal_base;
                let function_rva = *(((module_base_address + functions_rva as i64 + (4 * (function_ordinal - ordinal_base)) as i64 )) as *mut i32);
                function_ptr = (module_base_address + function_rva as i64) as *mut i32;

                break;
            }

        }

    }

    function_ptr
}

