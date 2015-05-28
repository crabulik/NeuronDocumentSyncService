using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuronDocumentSync.Enums;

namespace NeuronDocumentSync.Interfaces
{
    public interface INeuronLogger
    {
        LoggerType LogType { get; set; }
        void SetLogFileExtension(string logFileExtension);
        void SetLogFileName(string logFileName);

        void AddLog(string aLogMessage, Exception aException = null,
            EventLogEntryType aEntryType = EventLogEntryType.Information);

        void DeleteEventSource(string aLogSourceName);
        void DeleteEventSource();
        event Action<string, Exception> ExceptionOccured;
    }
}
