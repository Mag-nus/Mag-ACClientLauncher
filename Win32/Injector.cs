using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Mag_ACClientLauncher.Win32
{
    static class Injector
    {
        /// <summary>
        /// This will start an application using Process.Start() and inject the dll.<para />
        /// If dllFunctionToExecute is defined, it will be called after the dll has been injected.
        /// </summary>
        public static bool RunWithInject(ProcessStartInfo processStartInfo, string pathOfDllToInject, string dllFunctionToExecute = null)
        {
            var process = Process.Start(processStartInfo);

            if (process == null || process.Handle == IntPtr.Zero)
                return false;

            return Inject(process, pathOfDllToInject, dllFunctionToExecute);
        }

        /// <summary>
        /// This will start an application using kernel32.CreateProcess() suspended, inject the dll and then resume the process.<para />
        /// If dllFunctionToExecute is defined, it will be called after the dll has been injected and before the process is resumed.
        /// </summary>
        /// <remarks>
        /// This function was cleverly named by parad0x, one of the developers of Decal for Asheron's Call.
        /// </remarks>
        public static bool RunSuspendedCommaInjectCommaAndResume(string fileName, string arguments, string pathOfDllToInject, string dllFunctionToExecute = null)
        {
            // Reference: https://docs.microsoft.com/en-us/windows/desktop/procthread/process-creation-flags
            const uint CREATE_SUSPENDED = 0x00000004;

            kernel32.SECURITY_ATTRIBUTES pSec = new kernel32.SECURITY_ATTRIBUTES();
            pSec.nLength = Marshal.SizeOf(pSec);
            kernel32.SECURITY_ATTRIBUTES tSec = new kernel32.SECURITY_ATTRIBUTES();
            tSec.nLength = Marshal.SizeOf(tSec);
            kernel32.STARTUPINFO sInfo = new kernel32.STARTUPINFO();

            if (!kernel32.CreateProcess(null, fileName + " " + arguments, ref pSec, ref tSec, false, CREATE_SUSPENDED, IntPtr.Zero, Path.GetDirectoryName(fileName), ref sInfo, out var pInfo))
                return false;

            try
            {
                return Inject(pInfo.hProcess, pathOfDllToInject, dllFunctionToExecute);
            }
            finally
            {
                kernel32.ResumeThread(pInfo.hThread);
            }
        }

        /// <summary>
        /// This will inject a dll into an existing process defined by applicationName.<para />
        /// If dllFunctionToExecute is defined, it will be called after the dll has been injected.
        /// </summary>
        public static bool Inject(string applicationName, string pathOfDllToInject, string dllFunctionToExecute = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This will inject a dll into an existing process defined by process.<para />
        /// If dllFunctionToExecute is defined, it will be called after the dll has been injected.
        /// </summary>
        public static bool Inject(Process process, string pathOfDllToInject, string dllFunctionToExecute = null)
        {
            return Inject(process.Handle, pathOfDllToInject, dllFunctionToExecute);
        }

        /// <summary>
        /// This will inject a dll into an existing process defined by processHandle.<para />
        /// If dllFunctionToExecute is defined, it will be called after the dll has been injected.
        /// </summary>
        public static bool Inject(IntPtr processHandle, string pathOfDllToInject, string dllFunctionToExecute = null)
        {
            // Alocating some memory on the target process - enough to store the name of the dll and storing its address in a pointer
            var dwSize = (uint)((pathOfDllToInject.Length + 1) * Marshal.SizeOf(typeof(char)));

            var allocMemAddress = kernel32.VirtualAllocEx(processHandle, IntPtr.Zero, dwSize, kernel32.AllocationType.Commit | kernel32.AllocationType.Reserve, kernel32.MemoryProtection.ReadWrite);

            if (allocMemAddress == IntPtr.Zero)
                return false;

            try
            {
                // Writing the name of the dll there
                if (!kernel32.WriteProcessMemory(processHandle, allocMemAddress, Encoding.Default.GetBytes(pathOfDllToInject), (uint) ((pathOfDllToInject.Length + 1) * Marshal.SizeOf(typeof(char))), out _))
                    return false;

                // Searching for the address of LoadLibraryA and storing it in a pointer
                var kernel32Handle = kernel32.GetModuleHandle("kernel32.dll");
                var loadLibraryAddr = kernel32.GetProcAddress(kernel32Handle, "LoadLibraryA");

                // Inject the DLL
                var remoteThreadHandle = kernel32.CreateRemoteThread(processHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);

                if (remoteThreadHandle == IntPtr.Zero)
                    return false;

                try
                {
                    kernel32.WaitForSingleObject(remoteThreadHandle, kernel32.INFINITE);

                    kernel32.GetExitCodeThread(remoteThreadHandle, out var injectedDllAddress);

                    if (injectedDllAddress != 0)
                    {
                        // If we have a function to execute, lets do it
                        if (!String.IsNullOrEmpty(dllFunctionToExecute))
                            return Execute(processHandle, injectedDllAddress, pathOfDllToInject, dllFunctionToExecute);

                        return true;
                    }

                    return false;
                }
                finally
                {
                    kernel32.CloseHandle(remoteThreadHandle);
                }
            }
            finally
            {
                kernel32.VirtualFreeEx(processHandle, allocMemAddress, dwSize, kernel32.AllocationType.Release);
            }
        }

        /// <summary>
        /// This will find the address offset of dllFunctionToExecute in pathOfDllToInject, and add that to injectedDllAddress;
        /// It will then use kernel32.CreateRemoteThread() to call that address on processHandle.
        /// </summary>
        public static bool Execute(IntPtr processHandle, uint injectedDllAddress, string pathOfDllToInject, string dllFunctionToExecute)
        {
            var libraryAddress = kernel32.LoadLibrary(pathOfDllToInject);

            if (libraryAddress == IntPtr.Zero)
                return false;

            try
            {
                var functionAddress = kernel32.GetProcAddress(libraryAddress, dllFunctionToExecute);

                if (functionAddress == IntPtr.Zero)
                    return false;

                var functionAddressOffset = functionAddress.ToInt64() - libraryAddress.ToInt64();

                var addressToExecute = injectedDllAddress + functionAddressOffset;

                return Execute(processHandle, (IntPtr)addressToExecute);
            }
            finally
            {
                kernel32.FreeLibrary(libraryAddress);
            }
        }

        /// <summary>
        /// This will use kernel32.CreateRemoteThread() to call addressToExecute on processHandle.
        /// </summary>
        public static bool Execute(IntPtr processHandle, IntPtr addressToExecute)
        {
            var remoteThreadHandle = kernel32.CreateRemoteThread(processHandle, IntPtr.Zero, 0, addressToExecute, IntPtr.Zero, 0, IntPtr.Zero);

            if (remoteThreadHandle == IntPtr.Zero)
                return false;

            try
            {
                kernel32.WaitForSingleObject(remoteThreadHandle, kernel32.INFINITE);

                kernel32.GetExitCodeThread(remoteThreadHandle, out var exitCode);

                if (exitCode != 0)
                    return true;

                return false;
            }
            finally
            {
                kernel32.CloseHandle(remoteThreadHandle);
            }
        }
    }
}
