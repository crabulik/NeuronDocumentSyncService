using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Infrastructure.Processors;
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
        private readonly NeuronRepository _repository;
        private readonly INeuronDocumentProcessor _processor;
        private string _lastStatusErrorInfo;

        private object cfgLocker = new object();

        private BackgroundWorker _worker;
        public SyncCore(INeuronLogger logger, NeuronRepository repository, INeuronDocumentProcessor processor)
        {
            _logger = logger;
            _repository = repository;
            _processor = processor;

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
            SetStatus(SyncCoreStatus.SyncStep);

            var list = _repository.GetUnhandledDocumentsInfo();

            foreach (var document in list)
            {
                if (_worker.CancellationPending)
                {
                    _logger.AddLog(MainMessages.rs_SyncNeuronDocumentsCanceled);
                    return;
                }
                if (_repository.FillDocumentData(document))
                {
                    if (ValidateDocument(document))
                    {

                        #region Processing document

                        switch (_processor.ProcessDocument(document))
                        {
                            case NeuronDocumentProcessorResult.Success:
                                _repository.SetDocumentHandled(document);
                                break;
                            case NeuronDocumentProcessorResult.Fail:
                            {
                                _logger.AddLog(string.Format(MainMessages.rs_SyncNeuronDocumentWasNotProcessed,
                                    document.Name, document.CreatDate, document.ID));
                                _repository.LogDocumentError(document);
                                break;
                            }
                            case NeuronDocumentProcessorResult.Error:
                            {
                                _logger.AddLog(string.Format(MainMessages.rs_SyncNeuronDocumentGlobalError,
                                    document.Name, document.CreatDate, document.ID));
                                _repository.LogDocumentError(document);
                                return;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        #endregion
                    }

                    else
                    {
                        
                        _logger.AddLog(string.Format(MainMessages.rs_ValidationModelErrorsLog,
                                    document.Name, document.CreatDate, document.ID, document.Errors));
                        _repository.SetDocumentUnhandleable(document);
                    }

                }
                else
                {
                    _logger.AddLog(string.Format(MainMessages.rs_SyncNeuronDocumentWasNotFilled,
                            document.Name, document.CreatDate, document.ID));                    
                }
            }
        }

        private bool ValidateDocument(NeuronDocument document)
        {
            var context = new ValidationContext(document, null, null);
            var results = new List<ValidationResult>();
            
            bool valid = Validator.TryValidateObject(document, context, results, true);
            if ((document.DeliveryPhone == string.Empty) && (document.DeliveryEMail == string.Empty))
            {
                results.Add(new ValidationResult(MainMessages.rs_DocumantDoesntHaveAnyDeliveryInformation));
            }

            if ((document.DocumentData == null) && (document.DocumentAdditionalData == null))
            {
                results.Add(new ValidationResult(MainMessages.rs_DocumentHasNoData));
            }

            if (results.Count > 0)
            {
                var sb = new StringBuilder(MainMessages.rs_ValidationModelErrors);
                foreach (var result in results)
                {
                    sb.AppendLine(result.ErrorMessage);
                }
                document.Errors = sb.ToString();
            }
            return valid && (results.Count == 0);
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            SetStatus(SyncCoreStatus.Started);
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

            SetStatus(SyncCoreStatus.Stopped);
        }

        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {

        }

        private void EngageBrake()
        {
            SetStatus(SyncCoreStatus.BreakUp);

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


        #region Status

        private SyncCoreStatus _status = SyncCoreStatus.Stopped;

        private void SetStatus(SyncCoreStatus status)
        {
            _status = status;
            OnStatusChanged();
        }

        public SyncCoreStatus GetStatus()
        {
            if (_isInternalError)
                return SyncCoreStatus.Error;
            return _status;
        }

        private void OnStatusChanged(int documentCount = -1, int documentNumber = -1, string documentInfo = "")
        {
            var state = GetStatus();
            ServiceStatusInfoArgs info;
            switch (state)
            {
                case SyncCoreStatus.SyncStep:
                    info = new ServiceStatusInfoArgs(state, string.Empty,
                        documentCount, documentNumber, documentInfo);
                    break;
                case SyncCoreStatus.Stopped:
                case SyncCoreStatus.Started:           
                case SyncCoreStatus.BreakUp:
                    info = new ServiceStatusInfoArgs(state);
                    break;
                case SyncCoreStatus.Error:
                    info = new ServiceStatusInfoArgs(state, _lastStatusErrorInfo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (StausChanged != null)
            {
                var delegat = StausChanged;
                delegat(this, info);
            }

        }
        
        public event EventHandler<ServiceStatusInfoArgs> StausChanged; 

        #endregion

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