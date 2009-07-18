//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;

namespace Microsoft.WindowsAPICodePack
{
    /// <summary>
    /// 
    /// </summary>
    public class CommonDllNames
    {
		static CommonDllNames ()
		{
			ComCtl32 = (Environment.OSVersion.Version.Major > 5) ? "comctl32.dll" : "XTaskDlg.dll";
		}

        /// <summary>
        /// 
        /// </summary>
        public static readonly string ComCtl32;
        /// <summary>
        /// 
        /// </summary>
        public const string Kernel32 = "kernel32.dll";
        /// <summary>
        /// 
        /// </summary>
        public const string ComDlg32 = "comdlg32.dll";
        /// <summary>
        /// 
        /// </summary>
        public const string User32 = "user32.dll";
        /// <summary>
        /// 
        /// </summary>
        public const string Shell32 = "shell32.dll";
    }
}
