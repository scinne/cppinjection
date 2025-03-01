
# C++ File Injection (WIP) (Vape Lite)

A Project to investigate file injection methods for Vape Lite related bypasses such as Word RunPE, Elite Loader etc.

## Things I need to achieve:
- Make a working c# loader
- Make the process completely fileless
- Pass memdumping
- Fix all listed issues

## Issues I currently have with the project:
- Fileless doesn't work?/Windows doesn't allow it to work?/Need to revert Windows update?/Only works on certain Windows versions?
- Creating a C# Loader for Vape lite (All previous attempts have failed so far)
- Achieving true Fileless. No temp files or anything on disk at all, everything to memory.
- Figuring out whether a C# loader is even needed for Vape lite
- Basically just questioning myself and if I am even sane
- I need help.
- Picking the right method for memory execution. I think Reflection Assembly is the way to go but I'm not sure how to get it to WORK...
  
Currently working temp file memory execution method (Main.ps1):
```
Add-Type @"
using System;
using System.Runtime.InteropServices;
using System.Text;

public class Win32 {
    // Define CreateProcess function
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern bool CreateProcess(
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation
    );

    // Define STARTUPINFO structure
    [StructLayout(LayoutKind.Sequential)]
    public struct STARTUPINFO {
        public uint cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public ushort wShowWindow;
        public ushort cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    // Define PROCESS_INFORMATION structure
    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }
}
"@

# URL of Vape Lite, From Github Release
$fileUrl = "https://github.com/scinne/cppinjection/releases/download/test/578d2bfa.exe"

# Download File Contents of Vape Lite into Memory
$fileContent = Invoke-WebRequest -Uri $fileUrl -Method Get

# The content is now available in $fileContent.Content as a byte array
$byteArray = $fileContent.Content

# Write the content to a temporary file
$tempFilePath = [System.IO.Path]::GetTempFileName() + ".exe"
[System.IO.File]::WriteAllBytes($tempFilePath, $byteArray)

#Runs Vape Lite within Memory Via $tempFilePath Method
Invoke-Expression -Command $tempFilePath
