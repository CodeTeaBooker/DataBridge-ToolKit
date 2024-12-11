using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevToolkit.Tests.Core.Utils
{
    public static class MultipleAssert
    {
        private const int EstimatedFailureMessageLength = 256;
        private const string NullAssertionsMessage = "Assertion delegate cannot be null";
        private const string NullAssertionArrayMessage = "Assertion array cannot be null";
        private const string NullSingleAssertionMessage = "Single assertion cannot be null";

        private readonly struct AssertionFailure
        {
            public string Message { get; }
            public string StackTrace { get; }
            public string Context { get; }

            public AssertionFailure(string message, string stackTrace, string context)
            {
                Message = message ?? string.Empty;
                StackTrace = stackTrace ?? string.Empty;
                Context = context ?? string.Empty;
            }

            public bool HasStackTrace => !string.IsNullOrEmpty(StackTrace);
            public bool HasContext => !string.IsNullOrEmpty(Context);
        }

        /// <summary>
        /// Executes multiple assertions and throws a combined exception after collecting all failures.
        /// </summary>
        /// <param name="assertions">Delegate containing multiple assertions.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assertions"/> is null.</exception>
        public static void Multiple(Action assertions)
        {
            if (assertions == null)
            {
                throw new ArgumentNullException(nameof(assertions), NullAssertionsMessage);
            }

            ExecuteMultipleAssertions(new[] { assertions });
        }

        /// <summary>
        /// Executes multiple assertions and throws a combined exception after collecting all failures.
        /// </summary>
        /// <param name="assertions">Array of assertion delegates to execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assertions"/> is null or contains null elements.</exception>
        public static void Multiple(params Action[] assertions)
        {
            if (assertions == null)
            {
                throw new ArgumentNullException(nameof(assertions), NullAssertionArrayMessage);
            }

            ExecuteMultipleAssertions(assertions);
        }

        private static void ExecuteMultipleAssertions(Action[] assertions)
        {
            var failures = new List<AssertionFailure>();
            var context = GetCurrentTestContext();

            for (int i = 0; i < assertions.Length; i++)
            {
                var assertion = assertions[i];
                if (assertion == null)
                {
                    throw new ArgumentNullException(nameof(assertion), NullSingleAssertionMessage);
                }

                try
                {
                    assertion();
                }
                catch (AssertionException ex)
                {
                    failures.Add(new AssertionFailure(ex.Message, ex.StackTrace, context));
                }
                // Let other exceptions propagate directly
            }

            if (failures.Count > 0)
            {
                ThrowCombinedAssertionException(failures);
            }
        }

        private static void ThrowCombinedAssertionException(IReadOnlyList<AssertionFailure> failures)
        {
            if (failures == null)
            {
                throw new ArgumentNullException(nameof(failures));
            }

            if (failures.Count == 0)
            {
                return;
            }

            var messageBuilder = new StringBuilder(failures.Count * EstimatedFailureMessageLength);
            BuildExceptionMessage(messageBuilder, failures);
            Assert.Fail(messageBuilder.ToString());
        }

        private static void BuildExceptionMessage(StringBuilder messageBuilder, IReadOnlyList<AssertionFailure> failures)
        {
            var firstFailure = failures[0];
            if (firstFailure.HasContext)
            {
                messageBuilder.AppendLine($"Test Context: {firstFailure.Context}");
                messageBuilder.AppendLine();
            }

            messageBuilder.AppendLine($"Multiple assertions failed ({failures.Count} failures in total):");
            messageBuilder.AppendLine();

            for (int i = 0; i < failures.Count; i++)
            {
                var failure = failures[i];
                messageBuilder.AppendLine($"[{i + 1}] {failure.Message}");

                if (failure.HasStackTrace)
                {
                    messageBuilder.AppendLine("Stack Trace:");
                    messageBuilder.AppendLine(failure.StackTrace);
                    messageBuilder.AppendLine();
                }
            }
        }

        private static string GetCurrentTestContext()
        {
            try
            {
                var testContext = TestContext.CurrentContext;
                if (testContext?.Test == null)
                {
                    return string.Empty;
                }

                return BuildTestContextString(testContext);
            }
            catch (Exception ex)
            {
                TestContext.Error.WriteLine($"Error occurred while getting test context: {ex}");
                return string.Empty;
            }
        }

        private static string BuildTestContextString(TestContext testContext)
        {
            var contextBuilder = new StringBuilder();
            contextBuilder.Append($"Test: {testContext.Test.Name}");

            if (!string.IsNullOrEmpty(testContext.Test.ClassName))
            {
                contextBuilder.Append($" in {testContext.Test.ClassName}");
            }

            return contextBuilder.ToString();
        }
    }
}

