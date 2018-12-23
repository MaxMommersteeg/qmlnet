namespace Qml.Net.Internal.Platform.Loader
{
    internal sealed class LinuxPlatformLoader : UnixPlatformLoader
    {
        /// <inheritdoc />
        protected override bool UseCLibrary => false;
    }
}