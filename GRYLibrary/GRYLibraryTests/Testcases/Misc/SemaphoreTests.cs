using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using GSemaphore = GRYLibrary.Core.Misc.Semaphore;

namespace GRYLibrary.Tests.Testcases.Misc
{
    [TestClass]
    public class SemaphoreTests
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void LockAndUnlockChangeTheState()
        {
            // arrange
            GSemaphore semaphore = new();

            // act & assert
            Assert.IsFalse(semaphore.IsLocked());
            semaphore.Lock();
            Assert.IsTrue(semaphore.IsLocked());
            semaphore.Unlock();
            Assert.IsFalse(semaphore.IsLocked());
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void UnlockingAnUnlockedSemaphoreIsNotAllowed()
        {
            // arrange
            GSemaphore semaphore = new();

            // act & assert
            Assert.Throws<GRYLibrary.Core.Exceptions.InternalAlgorithmException>(semaphore.Unlock);
        }

        /// <remarks>
        /// This testcase ensures that <see cref="GSemaphore.Lock"/> does not block <see cref="GSemaphore.Unlock"/> while it is waiting.
        /// </remarks>
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void LockWaitsUntilTheSemaphoreIsUnlockedByAnotherThread()
        {
            // arrange
            GSemaphore semaphore = new();
            semaphore.Lock();

            // act
            Task waitingTask = Task.Run(semaphore.Lock);
            Assert.IsFalse(waitingTask.Wait(TimeSpan.FromMilliseconds(200)), "The semaphore was locked, so the second lock-operation must not succeed.");
            semaphore.Unlock();

            // assert
            Assert.IsTrue(waitingTask.Wait(TimeSpan.FromSeconds(10)), "The second lock-operation must succeed after the semaphore was unlocked.");
            Assert.IsTrue(semaphore.IsLocked());
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SemaphoreEnsuresThatOnlyOneThreadIsInTheCriticalSectionAtTheSameTime()
        {
            // arrange
            GSemaphore semaphore = new();
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
                    semaphore.Lock();
                    try
                    {
                        int amountOfThreadsInCriticalSection = Interlocked.Increment(ref currentAmountOfThreadsInCriticalSection);
                        maximalAmountOfThreadsInCriticalSection = Math.Max(maximalAmountOfThreadsInCriticalSection, amountOfThreadsInCriticalSection);
                        counter = counter + 1;
                        Interlocked.Decrement(ref currentAmountOfThreadsInCriticalSection);
                    }
                    finally
                    {
                        semaphore.Unlock();
                    }
                }
            });

            // assert
            Assert.AreEqual(amountOfThreads * amountOfIncrementsPerThread, counter);
            Assert.AreEqual(1, maximalAmountOfThreadsInCriticalSection);
        }
    }
}
