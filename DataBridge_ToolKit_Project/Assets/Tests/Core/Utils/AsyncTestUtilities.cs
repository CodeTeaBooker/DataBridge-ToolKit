using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace DevToolkit.Tests.Core.Utils
{
    public static class AsyncTestUtilities
    {
        /// <summary>
        /// Executes an async method within a Unity coroutine, allowing async methods to be tested in Unity's Test Framework.
        /// </summary>
        /// <param name="testMethod">The async method to be tested.</param>
        /// <returns>An IEnumerator that can be used as a Unity coroutine.</returns>
        public static IEnumerator RunAsyncTest(Func<Task> testMethod, int timeoutMilliseconds = 5000)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var task = testMethod();
            var timeoutTask = Task.Delay(timeoutMilliseconds, cancellationTokenSource.Token);

            while (!task.IsCompleted && !timeoutTask.IsCompleted)
            {
                yield return null;
            }

            if (timeoutTask.IsCompleted && !task.IsCompleted)
            {
                throw new TimeoutException("The async test method timed out.");
            }

            cancellationTokenSource.Cancel();

            if (task.IsFaulted)
            {
                throw task.Exception.GetBaseException();
            }

            yield return null;
        }
    }
}

