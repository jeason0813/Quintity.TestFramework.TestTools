using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Quintity.TestFramework.Core;

namespace Quintity.TestFramework.TestEngineer
{
    public partial class TestEngineer
    {
        #region Partial class data members

        delegate void OnTestExecutorExecutionBeginDelegate(TestExecutor testExecutor);
        delegate void OnTestExecutorExecutionCompleteDelegate(TestExecutor testExecutor, TestExecutionCompleteArgs args);
        delegate void OnTestScriptObjectExecutionCompleteDelegate(TestScriptObject testScriptObject, TestScriptResult testScriptObjectResult);
        delegate void OnTestScriptObjectExecutionBeginDelegate(TestScriptObject testScriptObject);
        delegate void OnTestTraceDelegate(string traceMessage);
        delegate void OnTestWarningDelegate(TestWarning testWarning);
        delegate void OnTestCheckDelegate(TestCheck testCheck);

        #endregion

        #region Partial class private methods

        private void registerRuntimeEvents()
        {
            // TestExecutor events
            TestExecutor.OnExecutionBegin += TestExecutor_OnExecutionBegin;
            TestExecutor.OnExecutionComplete += TestExecutor_OnExecutionComplete;

            // TestSuite events
            TestSuite.OnExecutionBegin += TestSuite_OnExecutionBegin;
            TestSuite.OnTestPreprocessorBegin += TestSuite_OnTestPreprocessorBegin;
            TestSuite.OnTestPreprocessorComplete += TestSuite_OnTestPreprocessorComplete;
            TestSuite.OnTestPostprocessorBegin += TestSuite_OnTestPostprocessorBegin;
            TestSuite.OnTestPostprocessorComplete += TestSuite_OnTestPostprocessorComplete;
            TestSuite.OnExecutionComplete += TestSuite_OnExecutionComplete;

            // TestCase events
            TestCase.OnExecutionBegin += TestCase_OnExecutionBegin;
            TestCase.OnExecutionComplete += TestCase_OnExecutionComplete;

            // TestStep events
            TestStep.OnExecutionBegin += TestStep_OnExecutionBegin;
            TestStep.OnExecutionComplete += TestStep_OnExecutionComplete;

            // Runtime events
            TestCheck.OnTestCheck += TestCheck_OnTestCheck;
            TestWarning.OnTestWarning += TestWarning_OnTestWarning;
            TestTrace.OnTestTrace += TestTrace_OnTestTrace;

            
        }

        private void unregisterRuntimeEvents()
        {
            // TestExecutor events
            // TestExecutor events
            TestExecutor.OnExecutionBegin -= TestExecutor_OnExecutionBegin;
            TestExecutor.OnExecutionComplete -= TestExecutor_OnExecutionComplete;

            // TestSuite events
            TestSuite.OnExecutionBegin -= TestSuite_OnExecutionBegin;
            TestSuite.OnTestPreprocessorBegin -= TestSuite_OnTestPreprocessorBegin;
            TestSuite.OnTestPreprocessorComplete -= TestSuite_OnTestPreprocessorComplete;
            TestSuite.OnTestPostprocessorBegin -= TestSuite_OnTestPostprocessorBegin;
            TestSuite.OnTestPostprocessorComplete -= TestSuite_OnTestPostprocessorComplete;
            TestSuite.OnExecutionComplete -= TestSuite_OnExecutionComplete;

            // TestCase events
            TestCase.OnExecutionBegin -= TestCase_OnExecutionBegin;
            TestCase.OnExecutionComplete -= TestCase_OnExecutionComplete;

            // TestStep events
            TestStep.OnExecutionBegin -= TestStep_OnExecutionBegin;
            TestStep.OnExecutionComplete -= TestStep_OnExecutionComplete;

            // Runtime events
            TestCheck.OnTestCheck -= TestCheck_OnTestCheck;
            TestWarning.OnTestWarning -= TestWarning_OnTestWarning;
            TestTrace.OnTestTrace -= TestTrace_OnTestTrace;
        }

        #endregion

        #region Runtime event handlers

        private void TestExecutor_OnExecutionBegin(TestExecutor testExecutor)
        {
            Debug.WriteLine("TestExecutor_OnExecutionBegin");

            if (this.InvokeRequired)
            {
                OnTestExecutorExecutionBeginDelegate d = new OnTestExecutorExecutionBeginDelegate(onBeginningTestExecution);
                BeginInvoke(d, new object[] { testExecutor });
            }
            else
            {
                onBeginningTestExecution(testExecutor);
            }
        }

        private void TestSuite_OnExecutionBegin(TestSuite testSuite)
        {
            Debug.WriteLine("TestSuite_OnExecutionBegin");

            if (this.InvokeRequired)
            {
                OnTestScriptObjectExecutionBeginDelegate d = new OnTestScriptObjectExecutionBeginDelegate(onBeginningTestScriptObjectExecution);
                BeginInvoke(d, new object[] { testSuite });
            }
            else
            {
                onBeginningTestScriptObjectExecution(testSuite);
            }
        }

        private void TestSuite_OnTestPreprocessorBegin(TestSuite testSuite)
        {
            Debug.WriteLine("TestSuite_OnTestPreprocessorBegin");
        }

        private void TestSuite_OnTestPreprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult)
        {
            Debug.WriteLine("TestSuite_OnTestPreprocessorComplete");
        }

        private void TestSuite_OnTestPostprocessorBegin(TestSuite testSuite)
        {
            Debug.WriteLine("TestSuite_OnTestPostprocessorBegin");
        }

        private void TestSuite_OnTestPostprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult)
        {
            Debug.WriteLine("TestSuite_OnTestPostprocessorComplete");
        }

        private void TestSuite_OnExecutionComplete(TestSuite testSuite, TestSuiteResult testSuiteResult)
        {
            Debug.WriteLine("TestSuite_OnExecutionComplete");

            if (this.InvokeRequired)
            {
                OnTestScriptObjectExecutionCompleteDelegate d = new OnTestScriptObjectExecutionCompleteDelegate(onTestScriptObjectExecutionComplete);
                BeginInvoke(d, new object[] { testSuite, testSuiteResult });
            }
            else
            {
                onTestScriptObjectExecutionComplete(testSuite, testSuiteResult);
            }
        }

        private void TestCase_OnExecutionBegin(TestCase testCase)
        {
            Debug.WriteLine("TestCase_OnExecutionBegin");

            if (this.InvokeRequired)
            {
                OnTestScriptObjectExecutionBeginDelegate d = new OnTestScriptObjectExecutionBeginDelegate(onBeginningTestScriptObjectExecution);
                BeginInvoke(d, new object[] { testCase });
            }
            else
            {
                onBeginningTestScriptObjectExecution(testCase);
            }
        }

        private void TestCase_OnExecutionComplete(TestCase testCase, TestCaseResult testCaseResult)
        {
            Debug.WriteLine("TestCase_OnExecutionComplete");

            if (this.InvokeRequired)
            {
                OnTestScriptObjectExecutionCompleteDelegate d = new OnTestScriptObjectExecutionCompleteDelegate(onTestScriptObjectExecutionComplete);
                BeginInvoke(d, new object[] { testCase, testCaseResult });
            }
            else
            {
                onTestScriptObjectExecutionComplete(testCase, testCaseResult);
            }
        }

        private void TestStep_OnExecutionBegin(TestStep testStep)
        {
            Debug.WriteLine("TestStep_OnExecutionBegin");

            if (this.InvokeRequired)
            {
                OnTestScriptObjectExecutionBeginDelegate d = new OnTestScriptObjectExecutionBeginDelegate(onBeginningTestScriptObjectExecution);
                BeginInvoke(d, new object[] { testStep });
            }
            else
            {
                onBeginningTestScriptObjectExecution(testStep);
            }
        }

        private void TestStep_OnExecutionComplete(TestStep testStep, TestStepResult testStepResult)
        {
            Debug.WriteLine("TestStep_OnExecutionComplete");

            if (this.InvokeRequired)
            {
                OnTestScriptObjectExecutionCompleteDelegate d = new OnTestScriptObjectExecutionCompleteDelegate(onTestScriptObjectExecutionComplete);
                BeginInvoke(d, new object[] { testStep, testStepResult });
            }
            else
            {
                onTestScriptObjectExecutionComplete(testStep, testStepResult);
            }
        }

        private void TestCheck_OnTestCheck(TestCheck testCheck)
        {
            Debug.WriteLine("TestCheck_OnTestCheck");

            if (this.InvokeRequired)
            {
                OnTestCheckDelegate d = new OnTestCheckDelegate(onTestCheck);
                BeginInvoke(d, new object[] { testCheck });
            }
            else
            {
                onTestCheck(testCheck);
            }
        }

        private void TestWarning_OnTestWarning(TestWarning testWarning)
        {
            Debug.WriteLine("TestWarning_OnTestWarning");

            if (this.InvokeRequired)
            {
                OnTestWarningDelegate d = new OnTestWarningDelegate(onTestWarning);
                BeginInvoke(d, new object[] { testWarning });
            }
            else
            {
                onTestWarning(testWarning);
            }
        }

        private void TestTrace_OnTestTrace(string traceMessage)
        {
            Debug.WriteLine("TestTrace_OnTestTrace");

            if (this.InvokeRequired)
            {
                OnTestTraceDelegate d = new OnTestTraceDelegate(onTestTrace);
                BeginInvoke(d, new object[] { traceMessage });
            }
            else
            {
                onTestTrace(traceMessage);
            }
        }

        private void TestExecutor_OnExecutionComplete(TestExecutor testExecutor, TestExecutionCompleteArgs args)
        {
            Debug.WriteLine("TestExecutor_OnExecutionComplete");

            if (this.InvokeRequired)
            {
                OnTestExecutorExecutionCompleteDelegate d = new OnTestExecutorExecutionCompleteDelegate(onTestExecutionComplete);
                BeginInvoke(d, new object[] { testExecutor, args });
            }
            else
            {
                onTestExecutionComplete(testExecutor, args);
            }
        }

        #endregion

        #region Runtime event methods

        private void onBeginningTestExecution(TestExecutor testExecutor)
        {
            m_testOutputViewer.AppendText("Beginning execution\n\n");
            m_testOutputViewer.ScrollToCaret();
            m_executionTimer.Start();
        }

        private void onTestExecutionComplete(TestExecutor testExecutor, TestExecutionCompleteArgs args)
        {
            m_testOutputViewer.AppendText("Execution complete\n\n");
            m_testOutputViewer.ScrollToCaret();
            m_treeView.SelectedNode = m_treeView.FindNode(args.TestScriptObject);
            m_tabCtrlResults.SelectedTab = this.m_tpTestResults;
            m_executionTimer.Stop();
        }

        private void onTestWarning(TestWarning testWarning)
        {
            //m_testOutputViewer.AppendText(string.Format("Test warning:  {0}\n", testWarning.Comment));
            //m_testOutputViewer.ScrollToCaret();
        }

        private void onTestCheck(TestCheck testCheck)
        {
            //m_testOutputViewer.AppendText(string.Format("Test check:  {0}\n", testCheck.Comment));
            //m_testOutputViewer.ScrollToCaret();
        }

        private void onTestTrace(string traceMessage)
        {
            m_testOutputViewer.AppendText(traceMessage + "\n\n");
            m_testOutputViewer.ScrollToCaret();
        }

        private void onBeginningTestScriptObjectExecution(TestScriptObject testScriptObject)
        {
            TestTreeNode node = m_treeView.FindNode(testScriptObject);

            m_testOutputViewer.AppendText(string.Format("Beginning execution of {0}\"{1}\"\n\n",
                node.GetTestScriptObjectType().ToLower(), node.TestScriptObject.Title));

            node.IsExecuting = true;
            node.RefreshUI();
            m_treeView.SelectedNode = node;
        }

        private void onTestScriptObjectExecutionComplete(TestScriptObject testScriptObject, TestScriptResult testScriptObjectResult)
        {
            TestTreeNode node = m_treeView.FindNode(testScriptObject);

            m_testOutputViewer.AppendText(string.Format("{0} \"{1}\" execution complete\n\n",
                node.GetTestScriptObjectType(), node.TestScriptObject.Title));

            node.IsExecuting = false;
            node.TestScriptResult = testScriptObjectResult;
            node.RefreshUI();
        }

        #endregion
    }
}
