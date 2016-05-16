using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using log4net;

namespace WinService.Workers
{
    public class CompositeWorker : IWorker
    {
        private readonly IWorker[] _workers;
        private readonly ILog _logger;

        public CompositeWorker(
            IWorker[] services,
            ILog logger
            )
        {
            _workers = services;
            _logger = logger;
        }

        public void Start()
        {
            foreach (var worker in _workers)
            {
                worker.Start();
                _logger.Debug(worker.GetType().Name + " started.");
            }
            _logger.Info(string.Format("All {0} workers started successfully.", _workers.Count()));
        }

        public void Stop()
        {
            foreach (var worker in _workers)
            {
                worker.Stop();
                _logger.Info(worker.GetType().Name + " stopped.");
            }
            _logger.Info(string.Format("All {0} workers stopped successfully.", _workers.Count()));
        }
    }
}