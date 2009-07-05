﻿//Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack
{
    /// <summary>
    /// Safe Window Handle
    /// </summary>
    public class SafeWindowHandle : ZeroInvalidHandle
    {
        /// <summary>
        /// Release the handle
        /// </summary>
        /// <returns>true if handled is release successfully, false otherwise</returns>
        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
            {
                return true;
            }

            if (CoreNativeMethods.DestroyWindow(handle) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}