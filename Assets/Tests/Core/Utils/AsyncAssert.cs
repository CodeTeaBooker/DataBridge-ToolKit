using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

public static class AsyncAssert
{
    private const int DefaultTimeoutMilliseconds = 5000;

    /// <summary>
    /// Asserts that the given async operation throws an exception of type TException,
    /// optionally verifying the exception message contains the expected substring.
    /// </summary>
    public static async Task<TException> ThrowsAsync<TException>(
        Func<Task> action,
        string expectedMessageContains = null,
        int timeoutMilliseconds = DefaultTimeoutMilliseconds) where TException : Exception
    {
        return (TException)await ThrowsAsyncInternal(typeof(TException), action, timeoutMilliseconds, expectedMessageContains);
    }

    /// <summary>
    /// Asserts that the given async operation throws an exception of the specified type,
    /// optionally verifying the exception message contains the expected substring.
    /// </summary>
    public static async Task<Exception> ThrowsAsync(
        Type expectedExceptionType,
        Func<Task> action,
        string expectedMessageContains = null,
        int timeoutMilliseconds = DefaultTimeoutMilliseconds)
    {
        if (expectedExceptionType == null)
            throw new ArgumentNullException(nameof(expectedExceptionType));
        if (!typeof(Exception).IsAssignableFrom(expectedExceptionType))
            throw new ArgumentException("Expected exception type must be a subclass of Exception.", nameof(expectedExceptionType));

        return await ThrowsAsyncInternal(expectedExceptionType, action, timeoutMilliseconds, expectedMessageContains);
    }

    private static async Task<Exception> ThrowsAsyncInternal(
        Type expectedExceptionType,
        Func<Task> action,
        int timeoutMilliseconds,
        string expectedMessageContains)
    {
        using var cts = new CancellationTokenSource(timeoutMilliseconds);

        try
        {
            var task = action();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeoutMilliseconds, cts.Token));

            if (completedTask != task)
                throw new TimeoutException($"Operation timed out after {timeoutMilliseconds}ms");

            await task; // Await to catch any exceptions thrown by the task
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            
            Exception currentException = ex;
            while (currentException != null)
            {
                if (expectedExceptionType.IsInstanceOfType(currentException))
                {
                    if (!string.IsNullOrEmpty(expectedMessageContains) && !currentException.Message.Contains(expectedMessageContains))
                    {
                        throw new AssertionException($"Expected exception message to contain '{expectedMessageContains}' but got '{currentException.Message}'.");
                    }
                    return currentException;
                }
                currentException = currentException.InnerException;
            }
            throw new AssertionException($"Expected exception of type {expectedExceptionType.Name} but got {ex.GetType().Name}.");
        }

        throw new AssertionException($"Expected exception of type {expectedExceptionType.Name} was not thrown.");
    }
}
