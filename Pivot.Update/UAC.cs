using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Diagnostics;

namespace Pivot.Update
{
    internal static class UAC
    {
        [DllImport("user32")]
        public static extern UInt32 SendMessage
            (IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);

        internal const int BCM_FIRST = 0x1600; //Normal button
        internal const int BCM_SETSHIELD = (BCM_FIRST + 0x000C); //Elevated button

        internal static bool IsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        internal static bool RunElevated(string path, string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = path;
            startInfo.Arguments = args;
            startInfo.Verb = "runas";
            try
            {
                Process p = Process.Start(startInfo);
                p.WaitForExit();
                return (p.ExitCode == 0);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return false;
            }
        }
    }
}
