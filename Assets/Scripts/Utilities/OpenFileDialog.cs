using System;
using System.Runtime.InteropServices;

namespace Puzzled
{
    public static class OpenFileDialog
    {
        public const int OFN_EXPLORER = 0x00080000;
        public const int OFN_FILEMUSTEXIST = 0x00001000;
        public const int OFN_PATHMUSTEXIST = 0x00000800;
        public const int OFN_ALLOWMULTISELECT = 0x00000200;
        public const int OFN_NOCHANGEDIR = 0x00000008;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName
        {
            public int structSize = 0;
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;
            public string filter = null;
            public string customFilter = null;
            public int maxCustFilter = 0;
            public int filterIndex = 0;
            public string file = null;
            public int maxFile = 0;
            public string fileTitle = null;
            public int maxFileTitle = 0;
            public string initialDir = null;
            public string title = null;
            public int flags = 0;
            public short fileOffset = 0;
            public short fileExtension = 0;
            public string defExt = null;
            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;
            public string templateName = null;
            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }

        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto, EntryPoint = "GetOpenFileName")]
        private static extern bool Internal_GetOpenFileName([In, Out] OpenFileName ofn);

        public static bool GetOpenFileName([In, Out] OpenFileName ofn)
        {
            ofn.structSize = Marshal.SizeOf(ofn);
            return Internal_GetOpenFileName(ofn);
        }
    }
}
