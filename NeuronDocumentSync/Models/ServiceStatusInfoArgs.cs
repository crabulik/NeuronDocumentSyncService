using System;
using NeuronDocumentSync.Enums;

namespace NeuronDocumentSync.Models
{
    public class ServiceStatusInfoArgs : EventArgs
    {
        public SyncCoreStatus Status { get; set; }

        public string StatusMessage { get; private set; }

        public int DocumentCount { get; private set; }
        public int DocumentNumber { get; private set; }
        public string DocumentInfo { get; private set; }

        public ServiceStatusInfoArgs(SyncCoreStatus status, string statusMessage,
            int documentCount, int documentNumber, string documentInfo)
        {
            Status = status;
            StatusMessage = statusMessage;
            DocumentCount = documentCount;
            DocumentNumber = documentNumber;
            DocumentInfo = documentInfo;
        }

        public ServiceStatusInfoArgs(SyncCoreStatus status)
            : this(status, string.Empty, -1, -1, string.Empty)
        {
            
        }

        public ServiceStatusInfoArgs(SyncCoreStatus status, string statusMessage)
            : this(status, statusMessage, -1, -1, string.Empty)
        {

        }
    }
}