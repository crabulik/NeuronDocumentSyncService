using System;

namespace NeuronDocumentSync.Enums
{
    [Flags]
    public enum LoggerType
    {
        None = 0,
        EventLog = 1,
        File = 2,
        Console = 4
    }
}