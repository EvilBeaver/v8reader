using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace V8Reader.Comparison
{

    class ExternalTextDiffViewer : IDiffViewer
    {

        public ExternalTextDiffViewer(string left, string right)
        {
            m_LeftContent = left;
            m_RightContent = right;

            m_LeftFile = Path.GetTempFileName();
            WriteFileContent(m_LeftFile, m_LeftContent);

            m_RightFile = Path.GetTempFileName();
            WriteFileContent(m_RightFile, m_RightContent);

            Utils.TempFileCleanup.RegisterTempFile(m_LeftFile);
            Utils.TempFileCleanup.RegisterTempFile(m_RightFile);

        }

        public void ShowDifference()
        {
            ShowDifference("", "");
        }

        public void ShowDifference(string NameCurrent, string NameComparand)
        {
            EnsureExistence(m_LeftFile, m_LeftContent);
            EnsureExistence(m_RightFile, m_RightContent);

            var diffPath = Properties.Settings.Default.DiffCmdLine;
            bool OperationAllowed = !(diffPath == String.Empty);
            bool cancel = false;

            do
            {
                if (!OperationAllowed)
                {

                    var mbr = System.Windows.MessageBox.Show(
                        "Командная строка для diff не указана. Задать её сейчас?",
                        "V8 Reader",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Question);

                    if (mbr == System.Windows.MessageBoxResult.Yes)
                    {
                        var mainWnd = new SettingsWindow();
                        mainWnd.ShowDialog();
                    }
                    else if (mbr == System.Windows.MessageBoxResult.No)
                    {
                        cancel = true;
                    }
                }

                diffPath = Properties.Settings.Default.DiffCmdLine;
                OperationAllowed = !(diffPath == String.Empty);

            }
            while (!OperationAllowed && !cancel);

            if (!OperationAllowed)
                return;

            try
            {
                var bldr = new StringBuilder(diffPath);

                System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
                bldr.Replace("%1", m_LeftFile);
                bldr.Replace("%2", m_RightFile);
                bldr.Replace("%name1", DefaultTitle(m_LeftFile, NameCurrent));
                bldr.Replace("%name2", DefaultTitle(m_RightFile, NameComparand));

                string[] args = CommandLineToArgs(bldr.ToString());
                info.FileName = args[0];

                bldr.Clear();
                for (int i = 1; i < args.Length; i++)
                {
                    bldr.Append(' ');
                    bldr.Append(args[i]);
                }
                info.Arguments = bldr.ToString();

                using (var proc = new System.Diagnostics.Process())
                {
                    proc.StartInfo = info;
                    proc.Start();
                }
            }
            catch (Exception exc)
            {
                Utils.UIHelper.DefaultErrHandling(exc);
            }

        }

        private string DefaultTitle(string Filename, string Title)
        {
            if (Title != null && Title != String.Empty)
            {
                return Title;
            }
            else
            {
                return System.IO.Path.GetFileName(Filename);
            }
        }

        [DllImport("shell32.dll", SetLastError = true)]
        extern static IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        public string[] CommandLineToArgs(string commandLine)
        {
            int argc;
            var argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

        private void WriteFileContent(string filename, string content)
        {
            using (var fs = new FileStream(filename, FileMode.OpenOrCreate | FileMode.Truncate))
            {
                using (var writer = new StreamWriter(fs))
                {
                    writer.Write(content);
                }
            }
        }

        private void EnsureExistence(string filename, string content)
        {
            if (!File.Exists(filename))
            {
                WriteFileContent(filename, content);
            }
        }

        private string m_LeftFile;
        private string m_RightFile;
        private string m_LeftContent;
        private string m_RightContent;

    }
}
