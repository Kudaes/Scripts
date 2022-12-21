use bindings::Windows::Win32::System::Kernel::UNICODE_STRING;
use bindings::Windows::Win32::System::Threading::RTL_USER_PROCESS_PARAMETERS;
use bindings::Windows::Win32::Foundation::{HANDLE,};
use data::{PS_ATTRIBUTE_LIST, PS_ATTRIBUTE_u, PS_ATTRIBUTE, PS_CREATE_INFO, PVOID};
use std::mem::{transmute, size_of};
use std::ptr;

fn main() {
  
    unsafe 
    {
        let nt_image_path:UNICODE_STRING = UNICODE_STRING::default();
        let ptr_image_path: *mut UNICODE_STRING = transmute(&nt_image_path);
        let path = "\\??\\C:\\Windows\\System32\\mmc.exe".to_string();
        let mut path_utf16: Vec<u16> = path.encode_utf16().collect();
        path_utf16.push(0);
        dinvoke::rtl_init_unicode_string(ptr_image_path,path_utf16.as_ptr());

        let unused = vec![0u8;size_of::<RTL_USER_PROCESS_PARAMETERS>()];
        let process_parameters: *mut RTL_USER_PROCESS_PARAMETERS = transmute(unused.as_ptr());
        let parameters_ptr: *mut *mut RTL_USER_PROCESS_PARAMETERS = transmute(&process_parameters);
        let r = dinvoke::rtl_create_process_parameters_ex(
            parameters_ptr, 
            ptr_image_path, 
            ptr::null_mut(), 
            ptr::null_mut(), 
            ptr::null_mut(), 
            ptr::null_mut(), 
            ptr::null_mut(), 
            ptr::null_mut(), 
            ptr::null_mut(), 
            ptr::null_mut(), 
            0x01
        );

        if r != 0
        {
            println!("[X] RtlCreateProcessParametersEx failed.");
            return;
        }

        let mut create_info: PS_CREATE_INFO = PS_CREATE_INFO { size: 0, unused: [0;80] };
        create_info.size = size_of::<PS_CREATE_INFO>();

        let ps_attribute_u: PS_ATTRIBUTE_u = PS_ATTRIBUTE_u { value: 0 };
        let ps_attribute_u2: PS_ATTRIBUTE_u = PS_ATTRIBUTE_u { value: 0 };

        let ps_atribute: PS_ATTRIBUTE = PS_ATTRIBUTE { attribute: 0, size: 0, union: ps_attribute_u, return_length: 0 as *mut usize };
        let ps_atribute2: PS_ATTRIBUTE = PS_ATTRIBUTE { attribute: 0, size: 0, union: ps_attribute_u2, return_length: 0 as *mut usize };

        let mut attribut_list: PS_ATTRIBUTE_LIST = PS_ATTRIBUTE_LIST { total_length: 0, attributes: [ps_atribute,ps_atribute2] };
        attribut_list.total_length = size_of::<PS_ATTRIBUTE_LIST>(); // -  size_of::<PS_ATTRIBUTE>();
        attribut_list.attributes[0].attribute = 0x20005; // PS_ATTRIBUTE_IMAGE_NAME 
        attribut_list.attributes[0].size = nt_image_path.Length as usize; 
        attribut_list.attributes[0].union.value =  transmute(nt_image_path.Buffer.0);

        let aa =  0x00000001u64 << 44; // BlockDlls
        attribut_list.attributes[1].attribute = 0x20010; // PS_ATTRIBUTE_MITIGATION_OPTIONS  
        attribut_list.attributes[1].size = 8; 
        attribut_list.attributes[1].union.value =  transmute(&aa);

        let h = HANDLE::default();
        let t = HANDLE::default();
        let process: *mut HANDLE = transmute(&h);
        let thread: *mut HANDLE = transmute(&t);
        let parameters: PVOID = transmute(*parameters_ptr);
        let create: *mut PS_CREATE_INFO = transmute(&create_info);
        let attribute: *mut PS_ATTRIBUTE_LIST = transmute(&attribut_list);
        let r2 = dinvoke::nt_create_user_process(
            process, 
            thread, 
            (0x000F0000) |  (0x00100000) | 0xFFFF, //PROCESS_ALL_ACCESS
            (0x000F0000) |  (0x00100000) | 0xFFFF, //THREAD_ALL_ACCESS
            ptr::null_mut(), 
            ptr::null_mut(), 
            0, 
            0, 
            parameters, 
            create, 
            attribute
        );

        println!("NTSTATUS: {:x}", r2);


    }
}
