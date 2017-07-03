﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData.Tests.Domain;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NUnit.Framework;

namespace DynamicData.Tests.List
{
    
    public class FromAsyncFixture
    {
        private TestScheduler _scheduler;

        [SetUp]
        public void SetUp()
        {
            _scheduler = new TestScheduler();
        }

        [Test]
        public void CanLoadFromTask()
        {
            Task<IEnumerable<Person>> Loader()
            {
                var items = Enumerable.Range(1, 100)
                    .Select(i => new Person("Person" + i, 1))
                    .ToArray()
                    .AsEnumerable();

                return Task.FromResult(items);
            }

            var data = Observable.FromAsync((Func<Task<IEnumerable<Person>>>) Loader)
                .ToObservableChangeSet()
                .AsObservableList();

            data.Count.Should().Be(100);
        }

        [Test]
        public void HandlesErrorsInObservable()
        {
            Task<IEnumerable<Person>> Loader()
            {
                Task.Delay(100);
                throw new Exception("Broken");
            }

            Exception error = null;

            var data = Observable.FromAsync((Func<Task<IEnumerable<Person>>>) Loader)
                .ToObservableChangeSet()
                .Subscribe((changes) => { }, ex => error = ex);;

            error.Should().NotBeNull();
        }

        [Test]
        public void HandlesErrorsObservableList()
        {
            Func<Task<IEnumerable<Person>>> loader = () =>
            {
                Task.Delay(100);
                throw new Exception("Broken");
            };

            Exception error = null;

            var data = Observable.FromAsync(loader)
                .ToObservableChangeSet()
                .AsObservableList();

            var subscribed = data.Connect()
                .Subscribe(changes => { }, ex => error = ex);


            error.Should().NotBeNull();
        }

    }
}
