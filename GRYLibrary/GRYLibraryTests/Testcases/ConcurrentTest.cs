using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class ConcurrentTest
    {
        [TestMethod]
        public void TestConcurrentFunctions1()
        {
            ISet<Tuple<int, int>> testFunctionDetails = new HashSet<Tuple<int, int>>();
            int resultValueOfFirstResult = 42;
            testFunctionDetails.Add(new Tuple<int, int>(3, 21));
            testFunctionDetails.Add(new Tuple<int, int>(1, resultValueOfFirstResult));
            testFunctionDetails.Add(new Tuple<int, int>(2, 97));
            for (int i = 0; i < 100; i++)
            {
                testFunctionDetails.Add(new Tuple<int, int>(2, 100 + i));
            }
            for (int i = 0; i < 100; i++)
            {
                testFunctionDetails.Add(new Tuple<int, int>(600, 200 + i));
            }
            ISet<Func<int>> input = this.GetTestFunction(testFunctionDetails);
            int result = Core.Misc.Utilities.RunAllConcurrentAndReturnFirstResult(input, 8);
            Assert.AreEqual(resultValueOfFirstResult, result);
        }

        private ISet<Func<T>> GetTestFunction<T>(ISet<Tuple<int, T>> functionDetails)
        {
            ISet<Func<T>> result = new HashSet<Func<T>>();
            foreach (Tuple<int, T> item in functionDetails)
            {
                result.Add(() =>
                {
                    for (int i = 0; i < item.Item1; i++)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                    return item.Item2;
                });
            }
            return result;
        }
    }
}