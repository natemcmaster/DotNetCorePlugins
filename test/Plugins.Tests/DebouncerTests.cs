using System;
using System.Threading.Tasks;
using McMaster.NETCore.Plugins.Internal;
using Xunit;

namespace McMaster.NETCore.Plugins.Tests
{
    public class DebouncerTests
    {
        [Fact]
        public async Task InvocationIsDelayed()
        {
            var executionCounter = 0;

            var debouncer = new Debouncer(TimeSpan.FromSeconds(.1));
            debouncer.Execute(() => executionCounter++);

            Assert.Equal(0, executionCounter);

            await Task.Delay(TimeSpan.FromSeconds(.5));

            Assert.Equal(1, executionCounter);
        }

        [Fact]
        public async Task ActionsAreDebounced()
        {
            var executionCounter = 0;

            var debouncer = new Debouncer(TimeSpan.FromSeconds(.1));
            debouncer.Execute(() => executionCounter++);
            debouncer.Execute(() => executionCounter++);
            debouncer.Execute(() => executionCounter++);

            await Task.Delay(TimeSpan.FromSeconds(.5));

            Assert.Equal(1, executionCounter);
        }

        [Fact]
        public async Task OnlyLastActionIsInvoked()
        {
            string? invokedAction = null;

            var debouncer = new Debouncer(TimeSpan.FromSeconds(.1));
            foreach (var action in new[]{"a", "b", "c"})
            {
                debouncer.Execute(() => invokedAction = action);
            }

            await Task.Delay(TimeSpan.FromSeconds(.5));

            Assert.NotNull(invokedAction);
            Assert.Equal("c", invokedAction);
        }
    }
}
