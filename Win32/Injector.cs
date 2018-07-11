﻿using System;
using System.Diagnostics;
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
        /// This will inject a dll into an existing process defined by applicationName.<para />
        /// If dllFunctionToExecute is defined, it will be called after the dll has been injected.
        /// </summary>
        public static bool Inject(string applicationName, string pathOfDllToInject, string dllFunctionToExecute = null)
        {
            // todo
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
            var allocMemAddress = kernel32.VirtualAllocEx(processHandle, IntPtr.Zero, (uint)((pathOfDllToInject.Length + 1) * Marshal.SizeOf(typeof(char))), kernel32.AllocationType.Commit | kernel32.AllocationType.Reserve, kernel32.MemoryProtection.ReadWrite);

            if (allocMemAddress == IntPtr.Zero)
                return false;

            // Writing the name of the dll there
            if (!kernel32.WriteProcessMemory(processHandle, allocMemAddress, Encoding.Default.GetBytes(pathOfDllToInject), (uint)((pathOfDllToInject.Length + 1) * Marshal.SizeOf(typeof(char))), out _))
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
                }

                return false;
            }
            finally
            {
                kernel32.CloseHandle(remoteThreadHandle);
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

            return true;
        }
    }
}