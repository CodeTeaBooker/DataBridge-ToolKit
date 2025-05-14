using DataBridgeToolKit.Storage.Implementations;
using DataBridgeToolKit.Tests.Core.Utils;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.TestTools;

namespace DataBridgeToolKit.Storage.Implementations.Tests
{
    [TestFixture]
    public class ConcurrentLockManagerTests
    {
        #region Test Setup
        private ConcurrentLockManager _lockManager;

        [SetUp]
        public void SetUp()
        {
            _lockManager = new ConcurrentLockManager(
                defaultTimeout: TimeSpan.FromSeconds(1),
                cleanupInterval: TimeSpan.FromSeconds(10),
                inactiveTimeout: TimeSpan.FromMinutes(5));
        }

        [TearDown]
        public void TearDown()
        {
            _lockManager?.Dispose();
        }

        #endregion

        #region Constructor Tests

        [Test]
        public void Constructor_WithDefaultValues_InitializesCorrectly()
        {
            Assert.DoesNotThrow(() =>
            {
                using var manager = new ConcurrentLockManager();
                Assert.NotNull(manager);
            });
        }

        [Test]
        public void Constructor_WithCustomValues_InitializesCorrectly()
        {
            using var manager = new ConcurrentLockManager(
                defaultTimeout: TimeSpan.FromSeconds(2),
                cleanupInterval: TimeSpan.FromSeconds(20),
                inactiveTimeout: TimeSpan.FromMinutes(10));
            Assert.NotNull(manager);
        }

        #endregion

        #region Basic Lock Operations

        [UnityTest]
        public IEnumerator AcquireLock_BasicOperation_Succeeds()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                using (var lockHandle = await _lockManager.AcquireLockAsync("test_key", TimeSpan.FromSeconds(1)))
                {
                    Assert.NotNull(lockHandle);
                }
            });
        }

        [UnityTest]
        public IEnumerator AcquireLock_WithZeroTimeout_UsesDefaultTimeout()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                using (var lockHandle = await _lockManager.AcquireLockAsync("test_key", TimeSpan.Zero))
                {
                    Assert.NotNull(lockHandle);
                }
            });
        }

        #endregion

        #region Lock Statistics

        //[UnityTest]
        //public IEnumerator LockStatistics_TracksMetricsCorrectly()
        //{
        //    yield return AsyncTestUtilities.RunAsyncTest(async () =>
        //    {
        //        string key = "stats_test_key";

        //        // Successful acquisition
        //        using (await _lockManager.AcquireLockAsync(key, TimeSpan.FromSeconds(1)))
        //        {
        //            // Create timeout scenario
        //            await AsyncAssert.ThrowsAsync<TimeoutException>(async () =>
        //            {
        //                await _lockManager.AcquireLockAsync(key, TimeSpan.FromMilliseconds(100));
        //            });
        //        }

        //        //var stats = _lockManager.GetLockStatistics(key);
        //        Assert.NotNull(stats);
        //        Assert.That(stats.AcquisitionCount, Is.EqualTo(1));
        //        Assert.That(stats.TimeoutCount, Is.EqualTo(1));
        //        Assert.That(stats.CurrentRefCount, Is.Zero);
        //    });
        //}

        #endregion

        #region Concurrency Tests

        [UnityTest]
        public IEnumerator AcquireLock_MultipleKeys_ConcurrentAccess()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                var tasks = new List<Task>();
                var lockCount = 10;

                for (int i = 0; i < lockCount; i++)
                {
                    string key = $"concurrent_key_{i}";
                    tasks.Add(Task.Run(async () =>
                    {
                        using var lockHandle = await _lockManager.AcquireLockAsync(key, TimeSpan.FromSeconds(1));
                        await Task.Delay(100);
                    }));
                }

                await Task.WhenAll(tasks);
            });
        }

        [UnityTest]
        public IEnumerator AcquireLock_SameKey_Sequential()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                const string key = "sequential_key";
                var acquiredCount = 0;
                var tasks = new List<Task>();

                for (int i = 0; i < 5; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        using (await _lockManager.AcquireLockAsync(key, TimeSpan.FromSeconds(2)))
                        {
                            Interlocked.Increment(ref acquiredCount);
                            await Task.Delay(100);
                        }
                    }));
                }

                await Task.WhenAll(tasks);
                Assert.That(acquiredCount, Is.EqualTo(5));
            });
        }

        #endregion

        #region Timeout and Cancellation

        [UnityTest]
        public IEnumerator AcquireLock_Timeout_ThrowsTimeoutException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                const string key = "timeout_key";

                using (await _lockManager.AcquireLockAsync(key, TimeSpan.FromSeconds(5)))
                {
                    await AsyncAssert.ThrowsAsync<TimeoutException>(async () =>
                    {
                        await _lockManager.AcquireLockAsync(key, TimeSpan.FromMilliseconds(100));
                    });
                }
            });
        }

        [UnityTest]
        public IEnumerator AcquireLock_Cancellation_ThrowsOperationCanceledException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                using var cts = new CancellationTokenSource();
                const string key = "cancel_key";

                using (await _lockManager.AcquireLockAsync(key, TimeSpan.FromSeconds(5)))
                {
                    var lockTask = _lockManager.AcquireLockAsync(key, TimeSpan.FromSeconds(5), cts.Token);
                    await Task.Delay(100);
                    cts.Cancel();

                    await AsyncAssert.ThrowsAsync<OperationCanceledException>(async () => await lockTask);
                }
            });
        }

        #endregion

        #region Cleanup and Resource Management

        [UnityTest]
        public IEnumerator CleanupInactiveLocks_RemovesInactiveLocks()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                var manager = new ConcurrentLockManager(
               defaultTimeout: TimeSpan.FromMilliseconds(100),
               cleanupInterval: TimeSpan.FromMilliseconds(500),
               inactiveTimeout: TimeSpan.FromSeconds(1));

                const string key = "cleanup_key";

                using (await manager.AcquireLockAsync(key, TimeSpan.FromMilliseconds(100)))
                {
                    await Task.Delay(50);
                }

                await Task.Delay(1500);

                using (var newLock = await manager.AcquireLockAsync(key, TimeSpan.FromSeconds(1)))
                {
                    Assert.NotNull(newLock);
                }

                manager.Dispose();
            });
        }

        [UnityTest]
        public IEnumerator MemoryUsage_UnderLoad_StaysReasonable()
        {
            return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                var initialMemory = GC.GetTotalMemory(true);

                const int iterations = 1000;
                const int uniqueKeys = 10;
                const int maxDegreeOfParallelism = 100;
                var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
                var tasks = new List<Task>();

                for (int i = 0; i < iterations; i++)
                {
                    string key = $"memory_key_{i % uniqueKeys}";
                    await semaphore.WaitAsync();

                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            using (await _lockManager.AcquireLockAsync(key, TimeSpan.FromMilliseconds(500)))
                            {
                                await Task.Delay(1);
                            }
                        }
                        catch (TimeoutException te)
                        {
                            Console.Error.WriteLine($"Timeout acquiring lock for key '{key}': {te.Message}");
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"Error acquiring lock for key '{key}': {ex}");
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                await Task.Delay(2000);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                var finalMemory = GC.GetTotalMemory(true);

                var memoryIncrease = finalMemory - initialMemory;

                Assert.That(memoryIncrease, Is.LessThan(2 * 1024 * 1024),
                    $"Memory increase exceeded 4MB threshold. Initial: {initialMemory}, Final: {finalMemory}");
            });
        }

        #endregion

        #region Error Cases

        [UnityTest]
        public IEnumerator AcquireLock_AfterDispose_ThrowsObjectDisposedException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                _lockManager.Dispose();

                await AsyncAssert.ThrowsAsync<ObjectDisposedException>(async () =>
                {
                    await _lockManager.AcquireLockAsync("disposed_key", TimeSpan.FromSeconds(1));
                });
            });
        }

        [UnityTest]
        public IEnumerator AcquireLock_InvalidKeys_ThrowsArgumentException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Null key
                await AsyncAssert.ThrowsAsync<ArgumentNullException>(async () =>
                {
                    await _lockManager.AcquireLockAsync(null, TimeSpan.FromSeconds(1));
                });

                // Empty key
                await AsyncAssert.ThrowsAsync<ArgumentNullException>(async () =>
                {
                    await _lockManager.AcquireLockAsync("", TimeSpan.FromSeconds(1));
                });

                // Too long key
                var longKey = new string('a', 2049);
                await AsyncAssert.ThrowsAsync<ArgumentException>(async () =>
                {
                    await _lockManager.AcquireLockAsync(longKey, TimeSpan.FromSeconds(1));
                });
            });
        }

        #endregion

        #region Performance Tests

        [UnityTest]
        public IEnumerator Performance_UnderLoad_MaintainsResponsiveness()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                const string key = "perf_test_key";
                const int iterations = 100;
                var latencies = new List<TimeSpan>(iterations);
                var tasks = new List<Task>();


                using var concurrencyLimiter = new SemaphoreSlim(5);

                for (int i = 0; i < iterations; i++)
                {
                    var task = Task.Run(async () =>
                    {
                        await concurrencyLimiter.WaitAsync();
                        try
                        {
                            var sw = Stopwatch.StartNew();
                            using (await _lockManager.AcquireLockAsync(
                                key,
                                TimeSpan.FromMilliseconds(100)))
                            {
                                sw.Stop();
                                latencies.Add(sw.Elapsed);
                                await Task.Delay(5);
                            }
                        }
                        finally
                        {
                            concurrencyLimiter.Release();
                        }
                    });
                    tasks.Add(task);


                    if (i > 0 && i % 5 == 0)
                    {
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                    }
                }


                await Task.WhenAll(tasks);

                var maxLatency = latencies.Max(l => l.TotalMilliseconds);
                var avgLatency = latencies.Average(l => l.TotalMilliseconds);
                var p95Latency = GetPercentileLatency(latencies, 0.95);

                MultipleAssert.Multiple(() =>
                {
                    Assert.That(avgLatency, Is.LessThan(50),
                        $"Average latency ({avgLatency:F2}ms) exceeded threshold");
                    Assert.That(maxLatency, Is.LessThan(200),
                        $"Maximum latency ({maxLatency:F2}ms) exceeded threshold");
                    Assert.That(p95Latency, Is.LessThan(100),
                        $"95th percentile latency ({p95Latency:F2}ms) exceeded threshold");
                });

            });
        }

        private static double GetPercentileLatency(List<TimeSpan> latencies, double percentile)
        {
            if (latencies == null || latencies.Count == 0)
                return 0;

            var sortedLatencies = latencies
                .Select(l => l.TotalMilliseconds)
                .OrderBy(l => l)
                .ToList();

            var index = (int)Math.Ceiling(percentile * (sortedLatencies.Count - 1));
            return sortedLatencies[index];
        }

        #endregion
    }
}

