using System;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace V8Reader.Utils
{
    static class FileAssociation
    {
        // Associate file extension with progID, description, icon and application
        public static void Associate(string extension,
               string progID, string description, string icon, string application)
        {

            if (!IsAssociated(extension, progID))
            {
                AssociateExplicit(extension, progID, description, icon, application);
                UpdateShell();
            }

        }

        /// <summary>
        /// Explicitly associates file extension, even if it already associated
        /// </summary>
        public static void AssociateExplicit(string extension, string progID, string description, string icon, string application)
        {
            Registry.ClassesRoot.CreateSubKey(extension).SetValue("", progID);
            if (progID != null && progID.Length > 0)
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(progID))
                {
                    if (description != null)
                        key.SetValue("", description);
                    if (icon != null)
                        key.CreateSubKey("DefaultIcon").SetValue("", icon);
                    if (application != null)
                        key.CreateSubKey(@"Shell\Open\Command").SetValue("",
                                    application + " \"%1\"");

                }
        }

        // Return true if extension already associated in registry
        public static bool IsAssociated(string extension, string desiredProgID)
        {
            if (extension == null || desiredProgID == null)
                throw new ArgumentException();

            RegistryKey extKey = Registry.ClassesRoot.OpenSubKey(extension, false);
            if (extKey == null)
                return false;

            var progID = extKey.GetValue("") as string;
            if (progID == desiredProgID)
                return true;
            else
                return false;

        }

        /// <summary>
        /// Removes file association
        /// </summary>
        /// <param name="extension">extension to unregister</param>
        /// <param name="restoreToProgID">set association to another ProgID if needed</param>
        public static void RemoveAssociation(string extension, string progID, string restoreToProgID = null)
        {
            if (!IsAssociated(extension, progID))
            {
                return;
            }

            RegistryKey extKey = Registry.ClassesRoot.OpenSubKey(extension, true);

            if (extKey == null)
                return;

            if (restoreToProgID != null)
            {
                extKey.SetValue("", restoreToProgID);
            }
            else
            {
                extKey.DeleteValue("");
                Registry.ClassesRoot.DeleteSubKeyTree(progID, false);
            }

            UpdateShell();

        }

        /// <summary>
        /// Update explorer shell (icons, applications, etc)
        /// </summary>
        public static void UpdateShell()
        {
            SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("Kernel32.dll")]
        private static extern uint GetShortPathName(string lpszLongPath,
            [Out] StringBuilder lpszShortPath, uint cchBuffer);

        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        // Return short path format of a file name
        private static string ToShortPathName(string longName)
        {
            StringBuilder s = new StringBuilder(1000);
            uint iSize = (uint)s.Capacity;
            uint iRet = GetShortPathName(longName, s, iSize);
            return s.ToString();
        }
    }
}