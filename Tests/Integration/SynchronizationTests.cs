using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.Infrastructure.Synchronization;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Integration
{
    [Category("Integration")]
    internal class SynchronizationTests : TestsBase
    {
        private static IUnityContainer _container;
        const string ServiceToSynchronise = "testService";
        
        public override void BeforeAll()
        {
            base.BeforeAll();
            _container = new ApplicationContainerFactory().CreateWithRegisteredTypes();
        }

        /// <summary>
        ///  Check that service lock DB and next instance work after previouse
        ///  Time when next tread start work after previouse finished
        /// </summary>

        [Test]
        public void Check_that_next_instance_start_after_than_previouse_finished()
        {
            const int maxThreads = 2;
            var timeLog = new ConcurrentBag<TimeLog>();
            var timeToWait = new TimeSpan(0, 0, 0, 0, 100);

            var scenario = new Action(() =>
            {
                var synchronizationService = _container.Resolve<SynchronizationService>();

                synchronizationService.Execute(ServiceToSynchronise, new Action(() =>
                {
                    var time = new TimeLog { Start = DateTime.Now };
                    Trace.WriteLine(string.Format("Service To Synchronise: {0} in thread #{1}", ServiceToSynchronise, Thread.CurrentThread.ManagedThreadId));
                    Thread.Sleep(timeToWait);
                    time.Stop = DateTime.Now;
                    timeLog.Add(time);
                }));
            });

            var options = new ParallelOptions { MaxDegreeOfParallelism = maxThreads };
            Parallel.Invoke(options, Enumerable.Repeat(scenario, maxThreads).ToArray());

            var times = timeLog.OrderBy(x => x.Start).ToArray();

            //Check that time of start next instance greater than stop of previouse instance
            //and they not overlaped in the time
            for (int i = 0; i < maxThreads - 1; i++)
            {
                Assert.That(times[i].Stop <= times[i + 1].Start);
            }
        }
    }

    class TimeLog
    {
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
    }
}
