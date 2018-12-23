using System;

namespace Qml.Net.Internal.Platform.Loader
{
    public interface IPlatformLoader
    {
        IntPtr LoadLibrary(string path);

        IntPtr LoadSymbol(IntPtr library, string symbolName);

        bool CloseLibrary(IntPtr library);
    }
}