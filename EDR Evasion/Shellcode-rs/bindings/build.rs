fn main() {
    windows::build!(
        Windows::Win32::System::{Memory::{VIRTUAL_ALLOCATION_TYPE, PAGE_PROTECTION_FLAGS,VirtualAlloc,VirtualFree},
        SystemServices::LPTHREAD_START_ROUTINE}, Windows::Win32::{Foundation::HANDLE, System::{Memory::VIRTUAL_FREE_TYPE, Threading::{CreateThread,ResumeThread,WaitForSingleObject,THREAD_CREATION_FLAGS}}}
    );
}