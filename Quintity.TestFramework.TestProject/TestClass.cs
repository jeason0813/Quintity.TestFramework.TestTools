using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Quintity.TestFramework.Core;

namespace Quintity.TestFramework.TestProject
{
    [DataContract]
    public class MyClass
    {
        [DataMember]
        public string Name
        { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", "MyClass", Name);
        }
    }

    [TestClass]
    public class TestClass : TestClassBase
    {
        [TestMethod]
        public TestVerdict TestMethod()
        {
            try
            {
                //var tasks = new List<Task<int>>();

                //// Define a delegate that prints and returns the system tick count
                //Func<object, int> action = (object obj) =>
                //{
                //    int i = (int)obj;

                //    // Make each thread sleep a different time in order to return a different tick count
                //    Thread.Sleep(i * 1000);

                //    // The tasks that receive an argument between 2 and 5 throw exceptions
                //    //if (2 <= i && i <= 5)
                //    //{
                //    //    throw new InvalidOperationException("SIMULATED EXCEPTION");
                //    //}

                //    int tickCount = Environment.TickCount;
                //    TestTrace.Trace("Task={0}, i={1}, TickCount={2}, Thread={3}", Task.CurrentId, i, tickCount, Thread.CurrentThread.ManagedThreadId);

                //    return tickCount;
                //};

                //// Construct started tasks
                //for (int i = 0; i < 10; i++)
                //{
                //    int index = i;
                //    tasks.Add(Task<int>.Factory.StartNew(action, index));
                //}

                //try
                //{
                //    // Wait for all the tasks to finish.
                //    Task.WaitAll(tasks.ToArray());

                //    // We should never get to this point
                //    TestTrace.Trace("WaitAll() has not thrown exceptions. THIS WAS NOT EXPECTED.");
                //}
                //catch (AggregateException e)
                //{
                //    Console.WriteLine("\nThe following exceptions have been thrown by WaitAll(): (THIS WAS EXPECTED)");
                //    for (int j = 0; j < e.InnerExceptions.Count; j++)
                //    {
                //        TestTrace.Trace("\n-------------------------------------------------\n{0}", e.InnerExceptions[j].ToString());
                //    }
                //}




                TestVerdict = TestVerdict.Pass;
                TestMessage = "Success!";
            }
            catch (Exception e)
            {
                TestMessage += e.ToString();
                TestVerdict = TestVerdict.Error;
            }
            finally
            { }

            return TestVerdict;
        }

        [TestMethod]
        public TestVerdict SuppressExecution(bool bob, string stringValue, int intValue, bool boolValue)
        {
            try
            {
                TestVerdict = TestVerdict.Pass;
                TestMessage = "Success!";
            }
            catch (Exception e)
            {
                TestMessage += e.ToString();
                TestVerdict = TestVerdict.Error;
            }
            finally
            { }

            return TestVerdict;
        }

    }
}
