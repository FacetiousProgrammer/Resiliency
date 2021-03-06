using System;
using Xunit;

namespace Resiliency.Tests.Functions
{
    using System.Threading;
    using System.Threading.Tasks;

    public class RetriesAreExhaustedTests
    {
        public RetriesAreExhaustedTests()
        {
            ResilientOperation.WaiterFactory = (cancellationToken) => new FakeWaiter(cancellationToken);
        }

        [Fact]
        public async Task ThrowsOnceRetryHandlersAreExhausted()
        {
            var resilientOperation = ResilientOperation.From(() =>
                {
                    throw new Exception();

#pragma warning disable CS0162 // Unreachable code detected
                    return Task.FromResult(42);
#pragma warning restore CS0162 // Unreachable code detected
                })
                .WhenExceptionIs<Exception>(async (op, ex) =>
                {
                    if (op.Total.AttemptsExhausted < 3)
                    {
                        await op.WaitThenRetryAsync(TimeSpan.FromMilliseconds(100));
                    }
                })
                .GetOperation();

            await Assert.ThrowsAsync<Exception>(async () => await resilientOperation(CancellationToken.None));
        }

        [Fact]
        public async Task ExtensionThrowsOnceRetryHandlersAreExhausted()
        {
            Func<Task<int>> asyncOperation = () =>
            {
                throw new Exception();

#pragma warning disable CS0162 // Unreachable code detected
                return Task.FromResult(42);
#pragma warning restore CS0162 // Unreachable code detected
            };

            var resilientOperation = asyncOperation
                .AsResilient()
                .WhenExceptionIs<Exception>(async (op, ex) =>
                {
                    if (op.Total.AttemptsExhausted < 3)
                    {
                        await op.WaitThenRetryAsync(TimeSpan.FromMilliseconds(100));
                    }
                })
                .GetOperation();

                await Assert.ThrowsAsync<Exception>(async () => await resilientOperation(CancellationToken.None));
        }
    }
}
