using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using AdvancedDLSupport;
using Qml.Net.Internal.Platform.Loader.Native;

namespace Qml.Net.Internal.Platform.Loader
{
    internal class WindowsPlatformLoader : PlatformLoaderBase
    {
        protected override IntPtr LoadLibraryInternal(string path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path), "null library names or paths are not supported on Windows.");
            }

            var libraryHandle = kernel32.LoadLibrary(path);
            if (libraryHandle == IntPtr.Zero)
            {
                throw new LibraryLoadingException("Library loading failed.", path, new Win32Exception(Marshal.GetLastWin32Error()));
            }

            return libraryHandle;
        }

        /// <inheritdoc />
        public override IntPtr LoadSymbol(IntPtr library, string symbolName)
        {
            var symbolHandle = kernel32.GetProcAddress(library, symbolName);
            if (symbolHandle == IntPtr.Zero)
            {
                throw new SymbolLoadingException($"Symbol loading failed. Symbol name: {symbolName}", symbolName, new Win32Exception(Marshal.GetLastWin32Error()));
            }

            return symbolHandle;
        }

        /// <inheritdoc />
        public override bool CloseLibrary(IntPtr library)
        {
            return kernel32.FreeLibrary(library) > 0;
        }
    }
}