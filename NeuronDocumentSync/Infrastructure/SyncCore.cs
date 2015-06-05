using System;
using System.ComponentModel;
using System.Threading;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;
using NeuronDocumentSync.Resources;
using Ninject;

namespace NeuronDocumentSync.Infrastructure
{
    public class SyncCore : ISyncCore
    {
        private bool _isInternalError = false;
        private readonly INeuronLogger _logger;

        private object cfgLocker = new object();

        private BackgroundWorker _worker;
        public SyncCore(INeuronLogger logger, IGeneralConfig cfg)
        {
            _logger = logger;

            InitializeWorker();
        }

        #region Worker
        private int _syncOperationBreakValue = 500000;
        private bool _isServiceTimeEnable = false;
        private TimeSpan _serviceTimeStart = new TimeSpan(0);
        private TimeSpan _serviceTimeEnd = new TimeSpan(0);

        private void InitializeWorker()
        {
            _worker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
            _worker.DoWork += WorkerOnDoWork;
        }

        private void SyncNeuronDocuments()
        {
            _status = SyncCoreStatus.SyncStep;
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            _status = SyncCoreStatus.Started;
            while (true)
            {
                if (CheckServiceTime())
                {                    
                    if (_worker.CancellationPending) break;
                    SyncNeuronDocuments();
                }
                if (_worker.CancellationPending) break;
                EngageBrake();
                if (_worker.CancellationPending) break;
            }

            _status = SyncCoreStatus.Stopped;
        }

        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {

        }

        private void EngageBrake()
        {
            _status = SyncCoreStatus.BreakUp;

            int spendedMs = 0;
            int tmpSyncOperationBreakValue = _syncOperationBreakValue;                

            while (spendedMs < tmpSyncOperationBreakValue)
            {
                if (_worker.CancellationPending)
                {
                    return;
                }
                spendedMs += 1000;
                Thread.Sleep(1000);
            }           
        }

        private bool CheckServiceTime()
        {
            TimeSpan tmpServiceTimeStart;
            TimeSpan tmpServiceTimeEnd;
            bool tmpIsServiceTimeEnable;
            lock (cfgLocker)
            {
                tmpIsServiceTimeEnable = _isServiceTimeEnable;
                tmpServiceTimeStart = _serviceTimeStart;
                tmpServiceTimeEnd = _serviceTimeEnd;
            }
            if (tmpIsServiceTimeEnable)
            {
                var currTime = DateTime.Now.TimeOfDay;
                if (tmpServiceTimeStart > tmpServiceTimeEnd)
                {
                    if ((currTime >= tmpServiceTimeStart) || (currTime <= tmpServiceTimeEnd))
                        return false;
                }
                else
                {
                    if ((currTime >= tmpServiceTimeStart) && (currTime <= tmpServiceTimeEnd))
                        return false;
                }
            }

            return true;
        }
        #endregion


        private SyncCoreStatus _status = SyncCoreStatus.Stopped;     
        public SyncCoreStatus GetStatus()
        {
            if (_isInternalError)
                return SyncCoreStatus.Error;
            return _status;
        }
        public void OnStart(string[] args)
        {          

            StartWorker();

        }

        public void OnStop()
        {
            StopWorker();
        }

        private bool StopWorker()
        {
            var result = true;
            if (_worker.IsBusy)
            {
                _worker.CancelAsync();
            }

            var timeout = true;
            for (int i = 0; i < 40; i++)
            {
                if (!_worker.IsBusy)
                {
                    timeout = false;
                    break;
                }
                Thread.Sleep(500);
            }
            if (timeout)
            {
                result = false;
                _isInternalError = true;
                _logger.AddLog(MainMessages.rs_SyncCoreWorkerDidntStop);
            }         
            return result;
        }
        private bool StartWorker()
        {
            if (_isInternalError)
            {
                _logger.AddLog(MainMessages.rs_SyncCoreStartWithInternalError);
                return false;
            }

            if (_worker.IsBusy)
            {
                _logger.AddLog(MainMessages.rs_SyncCoreStartWhileBusy);
                return false;               
            }

            _worker.RunWorkerAsync();
            return true;
        }

        [Inject]
        public void UpdatePreferences(IGeneralConfig cfg, bool withRebootWorker = false)
        {
            lock (cfgLocker)
            {
                _isServiceTimeEnable = cfg.IsServiceTimeEnable;
                _serviceTimeStart = cfg.ServiceTimeStart;
                _serviceTimeEnd = cfg.ServiceTimeEnd;
                _syncOperationBreakValue = cfg.SyncOperationBreakValue;
            }
            if (withRebootWorker && _worker.IsBusy)
            {
                if (StopWorker())
                {
                    StartWorker();
                }
            }
        }

    }
}