using System;
using System.IO;
using System.Threading;
using System.Runtime.Serialization;
using Quintity.TestFramework.Core;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Quintity.TestFramework.Runtime
{
    class Program
    {
        #region Data members

        enum ExitCode : int
        {
            TestRunPass = 0,
            TestRunFailure = 1,
            TestPropertiesError = -2,
            TestSuiteError = -3,
            TestListenersError = -4,
            TestProfileError = -5,
            RuntimeError = -99
        }

        private static readonly log4net.ILog LogEvent =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string testEnvironments = String.Empty;
        private static ManualResetEvent manualReset = new ManualResetEvent(false);
        private static TestSuite initialTestSuite = null;
        private static ExitCode exitCode = ExitCode.TestRunPass;

        #endregion

        [STAThread]
        static int Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;

            TestTrace.OnTestTrace += TestTrace_OnTestTrace;
            TestExecutor.OnExecutionBegin += TestExecutor_OnExecutionBegin;
            TestExecutor.OnExecutionComplete += TestExecutor_OnExecutionComplete;
            TestExecutor.OnTestListenersComplete += TestExecutor_OnTestListenersComplete;
            TestSuite.OnExecutionComplete += TestSuite_OnExecutionComplete;

            string testPropertiesFile = string.Empty;
            string testSuiteFile = string.Empty;
            string testListenersFile = string.Empty;
            string testPerformanceFile = string.Empty;
            bool suppressExecution = false;

            if (args.Length != 0)
            {
                //Cycle through arguments.
                for (int index = 0; index < args.Length; index++)
                {
                    string arg = args[index].ToUpper().Trim();

                    suppressExecution = arg.ToUpper().Equals("/X") ? true : false;

                    arg = arg.Length > 2 ? arg.Substring(0, 2) : null;

                    switch (arg)
                    {
                        case "/p":
                        case "/P":
                            testPropertiesFile = extractUriFromArg(args[index]);
                            break;

                        case "/s":
                        case "/S":
                            testSuiteFile = extractUriFromArg(args[index]);
                            break;

                        case "/l":
                        case "/L":
                            testListenersFile = extractUriFromArg(args[index]);
                            break;

                        case "/e":
                        case "/E":
                            testEnvironments = extractUriFromArg(args[index]);
                            break;

                        case "/r":
                        case "/R":
                            testPerformanceFile = extractUriFromArg(args[index]);
                            break;
                        default:
                            break;
                    }
                }
            }

            executeTestSuite(testSuiteFile, testPropertiesFile, testPerformanceFile, testListenersFile, suppressExecution);

            return (int)exitCode;
        }

        private static void TestSuite_OnExecutionComplete(TestSuite testSuite, TestSuiteResult testSuiteResult)
        {
            // If this is the initial test suite, set exitcode based on overall test verdict.
            if (initialTestSuite.SystemID == testSuite.SystemID)
            {
                exitCode = (testSuiteResult.TestVerdict == TestVerdict.Pass) ? ExitCode.TestRunPass : ExitCode.TestRunFailure;
            }
        }

        private static void TestExecutor_OnTestListenersComplete(TestExecutor testExecutor)
        {
            LogEvent.Info("TestListeners executon complete");
        }

        private static void TestExecutor_OnExecutionBegin(TestExecutor testExecutor, TestExecutionBeginArgs args)
        {
            initialTestSuite = args.TestScriptObject as TestSuite;

            LogEvent.Info($"Beginnning execution of test suite \"{initialTestSuite.Title}\"");
        }

        private static void TestExecutor_OnExecutionComplete(TestExecutor testExecutor, TestExecutionCompleteArgs args)
        {
            LogEvent.Info("Test execution complete");

            manualReset.Set();
        }

        private static void TestTrace_OnTestTrace(string traceMessage)
        {
            LogEvent.Info(traceMessage);
        }

        static private string extractUriFromArg(string arg)
        {
            string file = null;

            string[] parts = arg.Split('=');

            if (parts.Length == 2)
            {
                file = parts[1].Trim();
            }

            return file;
        }

        private static void executeTestSuite(
            string testSuiteFile,
            string testPropertiesFile,
            string testPerformanceFile,
            string testListenersFile,
            bool suppressExecution)
        {
            initializeTestProperties(testPropertiesFile);

            var testSuite = initializeTestSuite(testSuiteFile);

            if (null == testSuite)
            {
                LogEvent.Info("Exiting Quintity TestFramework Runtime");

                exitCode = ExitCode.TestSuiteError;

                return;
            }

            if (!initializeTestListeners(testListenersFile))
            {
                LogEvent.Info("Exiting Quintity TestFramework Runtime");

                exitCode = ExitCode.TestSuiteError;

                return;
            }

            var testProfile = initializeTestProfile(testPerformanceFile);

            var executor = new Quintity.TestFramework.Core.TestExecutor();
            executor.ExecuteTestSuite(testSuite, null, testProfile, TestListeners.TestListenerCollection, suppressExecution);

            manualReset.WaitOne();

            LogEvent.Info("Exiting Quintity TestFramework Runtime");
        }

        private static TestSuite initializeTestSuite(string testSuiteFile)
        {
            return loadTestSuite(testSuiteFile);
        }

        private static TestSuite loadTestSuite(string testSuiteFile)
        {
            TestSuite testSuite = null;

            try
            {
                if (string.IsNullOrEmpty(testSuiteFile))
                {
                    throw new ArgumentException("The test suite cannot be a null or empty value");
                }

                testSuiteFile = TestProperties.ExpandString(testSuiteFile);
                testSuite = TestSuite.DeserializeFromFile(testSuiteFile);
            }
            catch (FileNotFoundException)
            {
                LogEvent.Fatal(
                    string.Format("Unable to locate test suite file \"{0}\".\r\n\r\nPlease verify the file path.", testSuiteFile));

                exitCode = ExitCode.TestSuiteError;
            }
            catch (SerializationException)
            {
                LogEvent.Fatal(
                    string.Format("Unable to read test suite \"{0}\".\r\n\r\nPlease verify the file is a valid test suite file.", testSuiteFile));

                exitCode = ExitCode.TestSuiteError;
            }
            catch (UriFormatException)
            {
                LogEvent.Fatal(
                    string.Format("Unable to read test suite \"{0}\".\r\n\r\nPlease verify the file is a valid test suite file.", testSuiteFile));

                exitCode = ExitCode.TestSuiteError;
            }
            catch (Exception e)
            {
                LogEvent.Fatal(e.Message);

                exitCode = ExitCode.TestSuiteError;
            }

            return testSuite;
        }

        private static void initializeTestProperties(string testPropertiesFile)
        {
            if (!string.IsNullOrEmpty(testPropertiesFile))
            {
                Uri testPropertiesUri = loadTestProperties(testPropertiesFile);

                if (testPropertiesUri == null)
                {
                    TestProperties.Initialize();
                }
            }
            else
            {
                TestProperties.Initialize();
            }

            if (!string.IsNullOrEmpty(testEnvironments))
            {
                var testPropertyOverrides = TestProperties.GetTestProperityOverrides(testEnvironments);

                if (testPropertyOverrides != null)
                {
                    TestProperties.ApplyTestPropertyOverrides(testPropertyOverrides);
                }
                else
                {
                    LogEvent.Info($"The test environment \"{testEnvironments }\" has been specified, " +
                        "however it was not located in the application configuration file.");

                    exitCode = ExitCode.TestPropertiesError;

                }
            }
        }

        private static Uri loadTestProperties(string testPropertiesFile)
        {
            Uri testPropertiesUri = null;

            try
            {
                testPropertiesUri = new Uri(testPropertiesFile);
                TestProperties.Initialize(testPropertiesUri.LocalPath);
            }
            catch (FileNotFoundException)
            {
                LogEvent.Fatal(
                    string.Format("Unable to locate test properties file \"{0}\".\r\n\r\nPlease verify the file path.", testPropertiesFile));

                testPropertiesUri = null;

                exitCode = ExitCode.TestPropertiesError;
            }
            catch (SerializationException)
            {
                LogEvent.Fatal(
                    string.Format("Unable to deserialize test properties \"{0}\".\r\n\r\nPlease verify the file is a valid test properties file.", testPropertiesFile));

                testPropertiesUri = null;

                exitCode = ExitCode.TestPropertiesError;
            }
            catch (UriFormatException)
            {
                LogEvent.Fatal(
                    string.Format("Unable to read test properties \"{0}\".\r\n\r\nPlease verify the file is a valid test properties file.", testPropertiesFile));

                exitCode = ExitCode.TestPropertiesError;
            }
            catch (Exception e)
            {
                LogEvent.Fatal(e.Message);

                testPropertiesUri = null;

                exitCode = ExitCode.TestPropertiesError;
            }

            return testPropertiesUri;
        }

        private static bool initializeTestListeners(string testListenersFile)
        {
            bool success = false;

            try
            {
                if (string.IsNullOrEmpty(testListenersFile))
                {
                    throw new ArgumentException("The test listener file cannot be a null or empty value");
                }

                var testListenersUri = new Uri(TestProperties.ExpandString(testListenersFile));
                TestListeners.ReadFromFile(testListenersUri.LocalPath);
                success = true;
            }
            catch (FileNotFoundException)
            {
                LogEvent.Fatal(
                    string.Format("Unable to locate test listeners file \"{0}\".\r\n\r\nPlease verify the file path.", testListenersFile));

                exitCode = ExitCode.TestListenersError;
            }
            catch (SerializationException)
            {
                LogEvent.Fatal(
                    string.Format("Unable to read test listeners \"{0}\".\r\n\r\nPlease verify the file is a valid test properties file.", testListenersFile));

                exitCode = ExitCode.TestListenersError;
            }
            catch (UriFormatException)
            {
                LogEvent.Fatal(
                    string.Format("Unable to read test listeners \"{0}\".\r\n\r\nPlease verify the file is a valid test properties file.", testListenersFile));

                exitCode = ExitCode.TestListenersError;
            }
            catch (Exception e)
            {
                LogEvent.Fatal(e.Message);

                exitCode = ExitCode.TestListenersError;
            }

            return success;
        }

        private static TestProfile initializeTestProfile(string testPerformanceFile)
        {
            TestProfile testProfile = null;

            try
            {
                if (!string.IsNullOrEmpty(testPerformanceFile))
                {
                    var testProfileUri = new Uri(TestProperties.ExpandString(testPerformanceFile));
                    testProfile = TestProfile.ReadFromFile(testProfileUri.LocalPath);
                }
            }
            catch (FileNotFoundException)
            {
                LogEvent.Fatal(
                    string.Format("Unable to locate test performance file \"{0}\".\r\n\r\nPlease verify the file path.", testPerformanceFile));

                exitCode = ExitCode.TestProfileError;
            }
            catch (SerializationException)
            {
                LogEvent.Fatal(
                    string.Format("Unable to read test performance \"{0}\".\r\n\r\nPlease verify the file is a valid test performance file.", testPerformanceFile));

                exitCode = ExitCode.TestProfileError;
            }
            catch (UriFormatException)
            {
                LogEvent.Fatal(
                    string.Format("Unable to read test performance \"{0}\".\r\n\r\nPlease verify the file is a valid test performance file.", testPerformanceFile));

                exitCode = ExitCode.TestProfileError;
            }
            catch (Exception e)
            {
                LogEvent.Fatal(e.Message);

                exitCode = ExitCode.TestProfileError;
            }

            return testProfile;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogEvent.Fatal("An unhandled exception has occurred.", (Exception)e.ExceptionObject);

            exitCode = ExitCode.RuntimeError;
        }
    }
}
