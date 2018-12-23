using System;
using System.Runtime.InteropServices;
using AdvancedDLSupport;
using Qml.Net.Internal.Platform.Loader.Native;

namespace Qml.Net.Internal.Platform.Loader
{
    internal abstract class UnixPlatformLoader : PlatformLoaderBase
    {
        protected abstract bool UseCLibrary { get; }
        private readonly Action _resetErrorAction;
        private readonly Func<string, SymbolFlag, IntPtr> _openLibraryFunc;
        private readonly Func<IntPtr, string, IntPtr> _openSymbolFunc;
        private readonly Func<IntPtr, int> _closeLibraryFunc;
        private readonly Func<IntPtr> _getErrorFunc;

        protected UnixPlatformLoader()
        {
            _resetErrorAction = () => dl.ResetError(UseCLibrary);
            _openLibraryFunc = (s, flags) => dl.open(s, flags, UseCLibrary);
            _openSymbolFunc = (ptr, s) => dl.sym(ptr, s, UseCLibrary);
            _closeLibraryFunc = ptr => dl.close(ptr, UseCLibrary);
            _getErrorFunc = () => dl.error(UseCLibrary);
        }

        private IntPtr LoadLibrary(string path, SymbolFlag flags)
        {
            _resetErrorAction();

            var libraryHandle = _openLibraryFunc(path, flags);
            if (libraryHandle != IntPtr.Zero)
            {
                return libraryHandle;
            }

            var errorPtr = _getErrorFunc();
            if (errorPtr == IntPtr.Zero)
            {
                throw new LibraryLoadingException("Library could not be loaded, and error information from dl library could not be found.", path);
            }

            throw new LibraryLoadingException(string.Format("Library could not be loaded: {0}", Marshal.PtrToStringAnsi(errorPtr)), path);
        }

        protected override IntPtr LoadLibraryInternal(string path) => LoadLibrary(path, SymbolFlag.RTLD_DEFAULT);

        public override IntPtr LoadSymbol(IntPtr library, string symbolName)
        {
            _resetErrorAction();

            var symbolHandle = _openSymbolFunc(library, symbolName);
            if (symbolHandle != IntPtr.Zero)
            {
                return symbolHandle;
            }

            var errorPtr = _getErrorFunc();
            if (errorPtr == IntPtr.Zero)
            {
                throw new SymbolLoadingException($"Symbol could not be loaded, and error information from dl could not be found. Symbol name: {symbolName}", symbolName);
            }

            var msg = Marshal.PtrToStringAnsi(errorPtr);
            throw new SymbolLoadingException($"Symbol could not be loaded: {msg}. Symbol name: {symbolName}", symbolName);
        }

        public override bool CloseLibrary(IntPtr library)
        {
            return _closeLibraryFunc(library) <= 0;
        }
    }
}