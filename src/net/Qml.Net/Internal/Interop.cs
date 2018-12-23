using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AdvancedDLSupport;
using Qml.Net.Internal.Qml;
using Qml.Net.Internal.Types;

[assembly: InternalsVisibleTo("DLSupportDynamicAssembly")]

namespace Qml.Net.Internal
{
    internal static class Interop
    {
        static readonly CallbacksImpl DefaultCallbacks = new CallbacksImpl(new DefaultCallbacks());
        
        static Interop()
        {
            //Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", "/home/pknopf/git/qmlnet/src/native/build-QmlNet-Desktop_Qt_5_12_0_GCC_64bit2-Debug");
            
            string pluginsDirectory = null;
            string qmlDirectory = null;
            string libDirectory = null;

            ILibraryPathResolver pathResolver = null;
            
            if (Host.GetExportedSymbol != null)
            {
                // We are loading exported functions from the currently running executable.
                var member = (FieldInfo)typeof(NativeLibraryBase).GetMember("PlatformLoader", BindingFlags.Static | BindingFlags.NonPublic).First();
                member.SetValue(null, new Host.Loader());
                pathResolver = new Host.Loader();
            }
            else
            {
                var internalType = Type.GetType("AdvancedDLSupport.DynamicLinkLibraryPathResolver, AdvancedDLSupport");
                if (internalType != null)
                {
                    pathResolver = (ILibraryPathResolver) Activator.CreateInstance(internalType, new object[] {true});

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // This custom path resolver attempts to do a DllImport to get the path that .NET decides.
                        // It may load a special dll from a NuGet package.
                        pathResolver = new WindowsDllImportLibraryPathResolver(pathResolver);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        pathResolver = new MacDllImportLibraryPathResolver(pathResolver);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        pathResolver = new LinuxDllImportLibraryPathResolver(pathResolver);
                    }

                    var resolveResult = pathResolver.Resolve("QmlNet");

                    if (resolveResult.IsSuccess)
                    {
                        libDirectory = Path.GetDirectoryName(resolveResult.Path);
                        if (!string.IsNullOrEmpty(libDirectory))
                        {
                            // If this library has a plugins/qml directory below it, set it.
                            var potentialPlugisDirectory = Path.Combine(libDirectory, "plugins");
                            if (Directory.Exists(potentialPlugisDirectory))
                            {
                                pluginsDirectory = potentialPlugisDirectory;
                            }

                            var potentialQmlDirectory = Path.Combine(libDirectory, "qml");
                            if (Directory.Exists(potentialQmlDirectory))
                            {
                                qmlDirectory = potentialQmlDirectory;
                            }
                        }
                    }
                }
            }


            var builder = new NativeLibraryBuilder(pathResolver: pathResolver);
            
            var interop = builder.ActivateInterface<ICombined>("QmlNet");

            QtWebEngine = interop;
            
            if(!string.IsNullOrEmpty(pluginsDirectory))
            {
                Qt.PutEnv("QT_PLUGIN_PATH", pluginsDirectory);
            }
            if(!string.IsNullOrEmpty(qmlDirectory))
            {
                Qt.PutEnv("QML2_IMPORT_PATH", qmlDirectory);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!string.IsNullOrEmpty(libDirectory) && Directory.Exists(libDirectory))
                {
                    // Even though we opened up the native dll correctly, we need to add
                    // the folder to the path. The reason is because QML plugins aren't
                    // in the same directory and have trouble finding dependencies
                    // that are within our lib folder.
                    Environment.SetEnvironmentVariable("PATH",
                        Environment.GetEnvironmentVariable("PATH") + $";{libDirectory}");
                }
            }

            {
                var pathLoader = new Platform.PathResolver.DynamicLinkLibraryPathResolver();
                var path = pathLoader.Resolve("QmlNet");
                var loader = Platform.Loader.PlatformLoaderBase.SelectPlatformLoader();
                var lib = loader.LoadLibrary(path.Path);

                Callbacks = LoadInteropType<CallbacksInterop>(lib, loader);
                NetTypeInfo = LoadInteropType<NetTypeInfoInterop>(lib, loader);
                NetJsValue = LoadInteropType<NetJsValueInterop>(lib, loader);
                NetMethodInfo = LoadInteropType<NetMethodInfoInterop>(lib, loader);
                NetPropertyInfo = LoadInteropType<NetPropertyInfoInterop>(lib, loader);
                NetTypeManager = LoadInteropType<NetTypeManagerInterop>(lib, loader);
                QGuiApplication = LoadInteropType<QGuiApplicationInterop>(lib, loader);
                QQmlApplicationEngine = LoadInteropType<QQmlApplicationEngineInterop>(lib, loader);
                NetVariant = LoadInteropType<NetVariantInterop>(lib, loader);
                NetReference = LoadInteropType<NetReferenceInterop>(lib, loader);
                NetVariantList = LoadInteropType<NetVariantListInterop>(lib, loader);
                NetTestHelper = LoadInteropType<NetTestHelperInterop>(lib, loader);
                NetSignalInfo = LoadInteropType<NetSignalInfoInterop>(lib, loader);
                QResource = LoadInteropType<QResourceInterop>(lib, loader);
                NetDelegate = LoadInteropType<NetDelegateInterop>(lib, loader);
                QQuickStyle = LoadInteropType<QQuickStyleInterop>(lib, loader);
                QtInterop = LoadInteropType<QtInterop>(lib, loader);
                Utilities = LoadInteropType<UtilitiesInterop>(lib, loader);
            }

            var cb = DefaultCallbacks.Callbacks();
            Callbacks.RegisterCallbacks(ref cb);
        }

        // ReSharper disable PossibleInterfaceMemberAmbiguity
        // ReSharper disable MemberCanBePrivate.Global
        internal interface ICombined :
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore PossibleInterfaceMemberAmbiguity
            IQtWebEngine
        {

        }
        
        public static CallbacksInterop Callbacks { get; }

        public static NetTypeInfoInterop NetTypeInfo { get; }
        
        public static NetMethodInfoInterop NetMethodInfo { get; }
        
        public static NetPropertyInfoInterop NetPropertyInfo { get; }
        
        public static NetTypeManagerInterop NetTypeManager { get; }
        
        public static QGuiApplicationInterop QGuiApplication { get; }
        
        public static QQmlApplicationEngineInterop QQmlApplicationEngine { get; }
        
        public static NetVariantInterop NetVariant { get; }
        
        public static NetReferenceInterop NetReference { get; }
        
        public static NetVariantListInterop NetVariantList { get; }
        
        public static NetTestHelperInterop NetTestHelper { get; }
        
        public static NetSignalInfoInterop NetSignalInfo { get; }
        
        public static QResourceInterop QResource { get; }
        
        public static NetDelegateInterop NetDelegate { get; }
        
        public static NetJsValueInterop NetJsValue { get; }
        
        public static QQuickStyleInterop QQuickStyle { get; }

        public static QtInterop QtInterop { get; }
        
        public static UtilitiesInterop Utilities { get; }
        
        public static IQtWebEngine QtWebEngine { get; }

        private static T LoadInteropType<T>(IntPtr library, Platform.Loader.IPlatformLoader loader) where T:new()
        {
            var result = new T();
            LoadDelegates(result, library, loader);
            return result;
        }
        
        private static void LoadDelegates(object o, IntPtr library, Platform.Loader.IPlatformLoader loader)
        {
            foreach (var property in o.GetType().GetProperties())
            {
                var entryName = property.GetCustomAttributes().OfType<NativeSymbolAttribute>().First().Entrypoint;
                var symbol = loader.LoadSymbol(library, entryName);
                property.SetValue(o, Marshal.GetDelegateForFunctionPointer(symbol, property.PropertyType));
            }
        }
    }
}