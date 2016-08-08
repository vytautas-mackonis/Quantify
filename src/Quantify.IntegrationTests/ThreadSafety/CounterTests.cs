using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quantify.Metrics;
using Xunit;

namespace Quantify.IntegrationTests.ThreadSafety
{
    public class CounterTests
    {
        private const int NumThreads = 1000;
        private readonly Counter _sut = new Counter();

        [Fact]
        public async Task NoParameterIncrementIsThreadSafe()
        {
            var tasks = Enumerable.Repeat(0, NumThreads)
                .Select(x => new Task(() =>
                {
                    _sut.Increment();
                    _sut.Increment();
                    _sut.Increment();
                    _sut.Increment();
                    _sut.Increment();
                }))
                .ToArray();

            foreach (var task in tasks)
            {
                task.Start();
            }

            await Task.WhenAll(tasks);

            Assert.Equal(NumThreads * 5, _sut.Value.Count);
        }

        [Fact]
        public async Task ValueIncrementIsThreadSafe()
        {
            var tasks = Enumerable.Range(0, NumThreads)
                .Select(x => new Task(() =>
                {
                    _sut.Increment(2);
                    _sut.Increment(2);
                    _sut.Increment(2);
                    _sut.Increment(2);
                    _sut.Increment(2);
                }))
                .ToArray();

            foreach (var task in tasks)
            {
                task.Start();
            }

            await Task.WhenAll(tasks);

            Assert.Equal(NumThreads * 2 * 5, _sut.Value.Count); 
        }

        [Fact]
        public async Task NoParameterDecrementIsThreadSafe()
        {
            var tasks = Enumerable.Repeat(0, NumThreads)
                .Select(x => new Task(() =>
                {
                    _sut.Decrement();
                    _sut.Decrement();
                    _sut.Decrement();
                    _sut.Decrement();
                    _sut.Decrement();
                }))
                .ToArray();

            foreach (var task in tasks)
            {
                task.Start();
            }

            await Task.WhenAll(tasks);

            Assert.Equal(-NumThreads * 5, _sut.Value.Count);
        }

        [Fact]
        public async Task ValueDecrementIsThreadSafe()
        {
            var tasks = Enumerable.Range(0, NumThreads)
                .Select(x => new Task(() =>
                {
                    _sut.Decrement(2);
                    _sut.Decrement(2);
                    _sut.Decrement(2);
                    _sut.Decrement(2);
                    _sut.Decrement(2);
                }))
                .ToArray();

            foreach (var task in tasks)
            {
                task.Start();
            }

            await Task.WhenAll(tasks);

            Assert.Equal(-NumThreads * 2 * 5, _sut.Value.Count);
        }
    }
}
