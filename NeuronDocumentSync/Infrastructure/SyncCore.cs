using System;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Interfaces;

namespace NeuronDocumentSync.Infrastructure
{
    public class SyncCore : ISyncCore
    {
        private readonly INeuronLogger _logger;
        public SyncCore(INeuronLogger logger)
        {
            _logger = logger;
        }
        private SyncCoreStatus _status = SyncCoreStatus.Stopped;
        public SyncCoreStatus GetStatus()
        {
            return _status;
        }

        public void OnStart(string[] args)
        {
            _status = SyncCoreStatus.Started;
            Console.WriteLine("OnStart");
        }

        public void OnStop()
        {
            Console.WriteLine("OnStop");
        }
    }
}