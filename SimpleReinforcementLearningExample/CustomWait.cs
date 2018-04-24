using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace SimpleReinforcementLearningExample
{
    [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    [SuppressMessage("Microsoft.Security", "CA2108:ReviewDeclarativeSecurityOnValueTypes")]
    public struct Message
    {
        IntPtr hWnd;
        int msg;
        IntPtr wparam;
        IntPtr lparam;
        IntPtr result;
    }
    
    public static class CustomWait
    {
        [DllImport("user32.dll")]
        private static extern int PeekMessage(out Message lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax,
            int wRemoveMsg);

        [DllImport("user32.dll")]
        private static extern int TranslateMessage(ref Message lpMsg);

        [DllImport("user32.dll")]
        private static extern int DispatchMessage(ref Message lpMsg);
        
        public static void Wait(int ms)
        {
            if (ms <= 0)
            {
                return;
            }

            var message = new Message();

            var startTime = Stopwatch.GetTimestamp();
            var secsPerTick = 1000.0 / Stopwatch.Frequency;

            while ((Stopwatch.GetTimestamp() - startTime) * secsPerTick < ms)
            {
                if (PeekMessage(out message, IntPtr.Zero, 0, 0, 1) == 0) continue;
                TranslateMessage(ref message);
                DispatchMessage(ref message);
            }
        }
    }
}