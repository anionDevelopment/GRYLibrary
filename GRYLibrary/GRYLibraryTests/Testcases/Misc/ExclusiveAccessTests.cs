using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GRYLibrary.Tests.Testcases.Misc
{
    [TestClass]
    public class ExclusiveAccessTests
    {
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void RunReturnsTheResultOfTheGivenFunction()
        {
            // arrange
            using ExclusiveAccess exclusiveAccess = new ExclusiveAccess();

            // act
            int result = exclusiveAccess.Run(() => 42);

            // assert
            Assert.AreEqual(42, result);
            Assert.IsFalse(exclusiveAccess.IsOwnedByCurrentThread(), "The access must be released after the operation is finished.");
        }

        /// <remarks>
        /// This testcase ensures the property which a plain <see cref="SemaphoreSlim"/> does not have: an operation on the protected resource must be able to
        /// call another operation on the same resource without deadlocking itself.
        /// </remarks>
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void NestedRunOnTheSameThreadDoesNotBlock()
        {
            // arrange
            using ExclusiveAccess exclusiveAccess = new ExclusiveAccess();

            // act
            Task<int> task = Task.Run(() => exclusiveAccess.Run(() => exclusiveAccess.Run(() => exclusiveAccess.Run(() => 3))));

            // assert
            Assert.IsTrue(task.Wait(TimeSpan.FromSeconds(10)), "Nested access on the same thread must not block.");
            Assert.AreEqual(3, task.Result);
        }

        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void AccessIsReleasedAgainAfterANestedRun()
        {
            // arrange
            using ExclusiveAccess exclusiveAccess = new ExclusiveAccess();

            // act
            exclusiveAccess.Run(() => exclusiveAccess.Run(() => { }));

            // assert
            Assert.IsFalse(exclusiveAccess.IsOwnedByCurrentThread());
            Task waitingTask = Task.Run(() => exclusiveAccess.Run(() => { }));
            Assert.IsTrue(waitingTask.Wait(TimeSpan.FromSeconds(10)), "The access must be available again for another thread.");
        }

        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void RunOfAnotherThreadWaitsUntilTheAccessIsReleased()
        {
            // arrange
            using ExclusiveAccess exclusiveAccess = new ExclusiveAccess();
            using ManualResetEventSlim accessTaken = new ManualResetEventSlim(false);
            using ManualResetEventSlim releaseAccess = new ManualResetEventSlim(false);
            Task occupyingTask = Task.Run(() => exclusiveAccess.Run(() =>
            {
                accessTaken.Set();
                releaseAccess.Wait(TimeSpan.FromSeconds(30));
            }));
            Assert.IsTrue(accessTaken.Wait(TimeSpan.FromSeconds(10)));

            // act
            Task waitingTask = Task.Run(() => exclusiveAccess.Run(() => { }));

            // assert
            Assert.IsFalse(waitingTask.Wait(TimeSpan.FromMilliseconds(200)), "The access is occupied, so the second operation must wait.");
            releaseAccess.Set();
            Assert.IsTrue(waitingTask.Wait(TimeSpan.FromSeconds(10)), "The second operation must run after the access was released.");
            Assert.IsTrue(occupyingTask.Wait(TimeSpan.FromSeconds(10)));
        }

        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void AccessIsReleasedWhenTheOperationThrowsAnException()
        {
            // arrange
            using ExclusiveAccess exclusiveAccess = new ExclusiveAccess();

            // act
            Assert.Throws<InvalidOperationException>(() => exclusiveAccess.Run(() => throw new InvalidOperationException("Intended exception for this testcase.")));

            // assert
            Assert.IsFalse(exclusiveAccess.IsOwnedByCurrentThread());
            Task waitingTask = Task.Run(() => exclusiveAccess.Run(() => { }));
            Assert.IsTrue(waitingTask.Wait(TimeSpan.FromSeconds(10)), "The access must be available again after a failed operation.");
        }

        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void OnlyOneThreadIsInTheCriticalSectionAtTheSameTime()
        {
            // arrange
            using ExclusiveAccess exclusiveAccess = new ExclusiveAccess();
            int amountOfThreads = 8;
            int amountOfIncrementsPerThread = 50;
            int counter = 0;
            int maximalAmountOfThreadsInCriticalSection = 0;
            int currentAmountOfThreadsInCriticalSection = 0;

            // act
            Parallel.For(0, amountOfThreads, _ =>
            {
                for (int i = 0; i < amountOfIncrementsPerThread; i++)
                {
                    exclusiveAccess.Run(() =>
                    {
                        int amountOfThreadsInCriticalSection = Interlocked.Increment(ref currentAmountOfThreadsInCriticalSection);
                        maximalAmountOfThreadsInCriticalSection = Math.Max(maximalAmountOfThreadsInCriticalSection, amountOfThreadsInCriticalSection);
                        counter = counter + 1;
                        Interlocked.Decrement(ref currentAmountOfThreadsInCriticalSection);
                    });
                }
            });

            // assert
            Assert.AreEqual(amountOfThreads * amountOfIncrementsPerThread, counter);
            Assert.AreEqual(1, maximalAmountOfThreadsInCriticalSection);
        }
    }
}
