using System;
using System.Reactive.Subjects;
using FluentAssertions;
using DynamicData.Tests.Domain;
using Microsoft.Reactive.Testing;
using NUnit.Framework;

namespace DynamicData.Tests.List
{
    
    public class BatchIfWithTimeOutFixture: IDisposable
    {
        private ISourceList<Person> _source;
        private ChangeSetAggregator<Person> _results;
        private TestScheduler _scheduler;

        private ISubject<bool> _pausingSubject = new Subject<bool>();

        [SetUp]
        public void MyTestInitialize()
        {
            _pausingSubject = new Subject<bool>();
            _scheduler = new TestScheduler();
            _source = new SourceList<Person>();
            _results = _source.Connect().BufferIf(_pausingSubject, TimeSpan.FromMinutes(1), _scheduler).AsAggregator();
        }

        public void Dispose()
        {
            _results.Dispose();
            _source.Dispose();
            _pausingSubject.OnCompleted();
        }

        [Test]
        public void WillApplyTimeout()
        {
            _pausingSubject.OnNext(true);

            //should timeout 
            _scheduler.AdvanceBy(TimeSpan.FromSeconds(61).Ticks);

            _source.Add(new Person("A", 1));

            //go forward an arbitary amount of time
            // _scheduler.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
            _results.Messages.Count.Should().Be(1, "There should be no messages");
        }

        [Test]
        public void NoResultsWillBeReceivedIfPaused()
        {
            _pausingSubject.OnNext(true);
            //advance otherwise nothing happens
            _scheduler.AdvanceBy(TimeSpan.FromMilliseconds(10).Ticks);

            _source.Add(new Person("A", 1));

            _results.Messages.Count.Should().Be(0, "There should be no messages");
        }

        [Test]
        public void ResultsWillBeReceivedIfNotPaused()
        {
            _source.Add(new Person("A", 1));

            //go forward an arbitary amount of time
            _scheduler.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
            _results.Messages.Count.Should().Be(1, "Should be 1 update");
        }

        [Test]
        public void CanToggleSuspendResume()
        {
            _pausingSubject.OnNext(true);
            ////advance otherwise nothing happens
            _scheduler.AdvanceBy(TimeSpan.FromMilliseconds(10).Ticks);

            _source.Add(new Person("A", 1));

            //go forward an arbitary amount of time
            _results.Messages.Count.Should().Be(0, "There should be no messages");

            _pausingSubject.OnNext(false);
            _scheduler.AdvanceBy(TimeSpan.FromMilliseconds(10).Ticks);

            _source.Add(new Person("B", 1));

            _results.Messages.Count.Should().Be(2, "There should be no messages");
        }
    }
}
