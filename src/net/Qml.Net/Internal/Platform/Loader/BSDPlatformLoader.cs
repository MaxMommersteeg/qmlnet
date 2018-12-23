namespace Qml.Net.Internal.Platform.Loader
{
    internal sealed class BSDPlatformLoader : UnixPlatformLoader
    {
        protected override bool UseCLibrary => true;
    }
}