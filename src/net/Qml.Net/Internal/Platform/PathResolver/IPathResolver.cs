namespace Qml.Net.Internal.Platform.PathResolver
{
    internal interface IPathResolver
    {
        ResolvePathResult Resolve(string library);
    }
}