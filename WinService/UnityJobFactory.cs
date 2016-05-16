using System;
using Microsoft.Practices.Unity;
using Quartz;
using Quartz.Spi;

namespace AFT.RegoV2.WinService
{
    public class UnityJobFactory : IJobFactory
    {
        private readonly IUnityContainer _container;

        public UnityJobFactory(IUnityContainer container)
        {
            _container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle)
        {
            return (IJob) _container.Resolve(bundle.JobDetail.JobType);
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return (IJob) _container.Resolve(bundle.JobDetail.JobType);
        }

        public void ReturnJob(IJob job)
        {
            throw new NotImplementedException();
        }
    }
}