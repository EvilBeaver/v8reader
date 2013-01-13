using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace V8Reader.Core
{
    static class UnpackWrapper
    {

        static public int UnpackToDir(String InputFile, String Directory)
        {
            return Unpack(InputFile, Directory);
        }
        
        [DllImport("unpack.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Unpack([MarshalAs(UnmanagedType.LPStr)] String name, [MarshalAs(UnmanagedType.LPStr)] String path_out);


    }

    class UnpackException : Exception
    {
        public UnpackException() : base("Unpack module exception") { }
        public UnpackException(int exitCode) : base(String.Format("Unpack module exception. Exit code: {0}", exitCode)) { }
    }

}
