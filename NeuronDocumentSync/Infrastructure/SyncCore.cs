using System;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;

namespace NeuronDocumentSync.Infrastructure
{
    public class SyncCore : ISyncCore
    {
        private readonly INeuronLogger _logger;
        private readonly IGeneralConfig _cfg;
        private readonly FbDbConnectionConfig _fbDbConfig;
        public SyncCore(INeuronLogger logger, IGeneralConfig cfg, FbDbConnectionConfig fbDbConfig)
        {
            _cfg = cfg;
            _fbDbConfig = fbDbConfig;
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
            _cfg.Save();
            _fbDbConfig.Save();
            Console.WriteLine("OnStart");
        }

        public void OnStop()
        {
            Console.WriteLine("OnStop");
        }
    }
}