using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace V8Reader.Comparison
{
    class FWDiffViewer : IDiffViewer
    {

        #region IDiffViewer Members

        public void ShowDifference()
        {
            
            string exeName = null;
            if (!Utils.UIHelper.AskForFileWorkshop(out exeName, null))
                return;
            
            using (Process FWProcess = new Process())
            {
                ProcessStartInfo sInfo = new ProcessStartInfo();
                sInfo.FileName = exeName;

                FWProcess.StartInfo = sInfo;


            }
        }

        private IntPtr GetWindowHandle(Process proc)
        {

            IntPtr result = IntPtr.Zero;
            
            foreach (ProcessThread pt in proc.Threads)
            {
                EnumThreadWindows((uint)pt.Id, new EnumThreadDelegate((hwnd,param)=>
                {
                    StringBuilder sb = new StringBuilder(300);
                    GetWindowText(hwnd, sb, sb.Capacity);
                    if (sb.ToString() == "1С:Предприятие - Работа с файлами")
                    {
                        result = hwnd;
                        return false;
                    }
                    else
                    {
                        return true;
                    }

                }), IntPtr.Zero);
            }

            return result;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumThreadWindows(uint dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        delegate bool EnumThreadDelegate (IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        #endregion
    }
}
