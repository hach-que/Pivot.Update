using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace Pivot.Update.Service
{
    public static class WindowsNative
    {
        public static bool LaunchPathAsUser(string path)
        {
            unsafe
            {
                IntPtr token;
                string dir = new FileInfo(path).Directory.FullName;
                STARTUPINFO startup = new STARTUPINFO
                {
                    cb = 0, // Probably incredibly dangerous and unsafe, but you can't use sizeof(STARTUPINFO).
                    lpReserved = null,
                    lpDesktop = "winsta0\\default",
                    lpTitle = null,
                    dwFlags = 0,
                    cbReserved2 = 0,
                    lpReserved2 = IntPtr.Zero
                };
                PROCESS_INFORMATION procinfo;
                WTSQueryUserToken(WTSGetActiveConsoleSessionId(), out token);
                return CreateProcessAsUser(
                    token,
                    path,
                    null,
                    null,
                    null,
                    false,
                    0,
                    IntPtr.Zero,
                    dir,
                    ref startup,
                    out procinfo
                    );
            }
        }

        #region P/Invoke Imports

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSQueryUserToken(uint sessionId, out IntPtr Token);

        [DllImport("kernel32.dll")]
        private static extern uint WTSGetActiveConsoleSessionId();

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        unsafe static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            SECURITY_ATTRIBUTES* lpProcessAttributes,
            SECURITY_ATTRIBUTES* lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        #endregion
    }
}
