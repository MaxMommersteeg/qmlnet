using System;
using System.Collections.Generic;
using System.Threading;
using AdvancedDLSupport;
using Qml.Net.Internal.Qml;
using Qml.Net.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Qml.Net.Sandbox
{
    class Sink : IMessageSink
    {
        public bool OnMessage(IMessageSinkMessage message)
        {
            switch (message)
            {
                case ITestAssemblyFinished _:
                    TestAssemblyFinished.Set();
                    break;
                case ITestPassed testPassed:
                    Console.WriteLine($"Passed: {testPassed.TestCase.DisplayName}");
                    break;
                case ITestFailed testFailed:
                    Console.WriteLine($"Failed: {testFailed.TestCase.DisplayName}");
                    break;
            }
            return true;
        }
        
        public readonly ManualResetEvent TestAssemblyFinished = new ManualResetEvent(false);
    }

    class SinkWithTypes : IMessageSinkWithTypes
    {
        public void Dispose()
        {
            
        }

        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
        {
            switch (message)
            {
                case IDiscoveryCompleteMessage _:
                    DiscoveryComplete.Set();
                    break;
                case ITestCaseDiscoveryMessage testCaseDiscoveryMessage:
                    if(testCaseDiscoveryMessage.TestCase.DisplayName.Contains("Does_unregister_signal_on_ref_destroy"))
                        TestCases.Add(testCaseDiscoveryMessage.TestCase);
                    break;
            }

            return true;
        }

        public readonly ManualResetEvent DiscoveryComplete = new ManualResetEvent(false);

        public readonly List<ITestCase> TestCases = new List<ITestCase>();
    }
    
    public class Program
    {
        static void Main()
        {
//            Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", "/home/pknopf/git/x3/abra/app/src/net/submodules/qmlnet/src/native/build-QmlNet-Desktop_Qt_5_12_0_GCC_64bit2-Debug");
//
//            var activator = new NativeLibraryBuilder();
//            var library = activator.ActivateInterface<IMainInterface>("QmlNet");
//            library.Create();
//            Environment.Exit(1);
            
            var config = ConfigReader.Load(typeof(BaseTests).Assembly.Location);
            var controller = new XunitFrontController(AppDomainSupport.Denied, typeof(BaseTests).Assembly.Location);
            var discoverOptions = TestFrameworkOptions.ForDiscovery(config);
            var executionOptions = TestFrameworkOptions.ForExecution(config);
            
            var sinkWithTypes = new SinkWithTypes();
            var sink = new Sink();
            
            // Discover the tests
            Console.WriteLine("Discovering...");
            controller.Find(false, sinkWithTypes, discoverOptions);
            sinkWithTypes.DiscoveryComplete.WaitOne();
            
            // Run the tests
            Console.WriteLine("Running...");
            controller.RunTests(sinkWithTypes.TestCases, sink, executionOptions);
            sink.TestAssemblyFinished.WaitOne();
        }
        
        
        public interface IMainInterface : ITestInterop
        {
            
        }
        
        public interface ITestInterop
        {   
            [NativeSymbol(Entrypoint = "net_variant_list_create")]
            IntPtr Create();
        }
    }
}