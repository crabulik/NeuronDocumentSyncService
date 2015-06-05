using System;
using System.Diagnostics;
using System.IO;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Interfaces;

namespace NeuronDocumentSync.Infrastructure
{

    public class Logger : INeuronLogger
    {
        

        public Logger()
        {
            _appDirectoryPath = Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData) + @"\NeuronDocumentSync\";
        }

        private const string _logName = "Application";
        private readonly string _appDirectoryPath;
        private string _logSourceName = "NeuronDocumentSyncService";
        private string _logFileExtension = ".log";
        private LoggerType _logType = LoggerType.File;

        private string logFileName
        {
            get { return _logSourceName + _logFileExtension; }
        }
        private string logFile
        {
            get { return Path.Combine(_appDirectoryPath, logFileName); }
        }

        public LoggerType LogType
        {
            get { return _logType; }
            set { _logType = value; }
        }

        public void SetLogFileExtension(string logFileExtension)
        {
            _logFileExtension = logFileExtension;
        }

        public void SetLogFileName(string logFileName)
        {
            _logSourceName = logFileName;
        }

        public void AddLog(string aLogMessage, Exception aException = null,
            EventLogEntryType aEntryType = EventLogEntryType.Information)
        {
            var exStr = aException == null ? string.Empty : Environment.NewLine + aException;
            var msg = string.Format("{0} {1} {2}: {3}{4}{5}",
                DateTime.Now.ToShortDateString(),
                DateTime.Now.ToShortTimeString(),
                aEntryType, aLogMessage, exStr,
                Environment.NewLine);

            if (LogType.HasFlag(LoggerType.Console))
            {
                Console.WriteLine(msg);
            }
            if (LogType.HasFlag(LoggerType.EventLog))
            {
                try
                {
                    if (!EventLog.SourceExists(_logSourceName))
                    {
                        EventLog.CreateEventSource(_logSourceName, _logName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return;
                }
                try
                {
                    var rawMsg = aLogMessage + (aException == null ? string.Empty : Environment.NewLine + aException);
                    var evntMsg = string.IsNullOrEmpty(rawMsg)
                        ? string.Empty
                        : rawMsg.Length > 15000 ? rawMsg.Substring(0, 15000) : rawMsg;

                    EventLog.WriteEntry(_logSourceName, evntMsg, aEntryType);
                }
                catch (Exception ex)
                {
                    try
                    {
                        EventLog.WriteEntry(_logSourceName,
                            "Can not add log message!" +
                            Environment.NewLine + aException,
                            EventLogEntryType.Error);
                        Console.WriteLine(ex);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return;
                    }
                }
            }
            if (LogType.HasFlag(LoggerType.File))
            {
                try
                {
                    // Todo: Handle max file size for log.
                    File.AppendAllText(logFile, msg);
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (!EventLog.SourceExists(_logSourceName))
                        {
                            EventLog.CreateEventSource(_logSourceName, _logName);
                        }
                        EventLog.WriteEntry(_logSourceName,
                            "Can not add log message!" +
                            Environment.NewLine + aException,
                            EventLogEntryType.Error);
                        Console.WriteLine(ex);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return;
                    }
                }
            }

            if (aException != null)
            {
                HandleException(aLogMessage, aException);
            }
        }

        public void DeleteEventSource(string aLogSourceName)
        {
            if (EventLog.SourceExists(aLogSourceName))
            {
                EventLog.DeleteEventSource(aLogSourceName);
            }
        }

        public void DeleteEventSource()
        {
            DeleteEventSource(_logSourceName);
        }

        public event Action<string, Exception> ExceptionOccured;

        private void HandleException(string aLogMessage, Exception aException)
        {
            if (ExceptionOccured != null)
            {
                ExceptionOccured(aLogMessage, aException);
            }
        }
    } 
}