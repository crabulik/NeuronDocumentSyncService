using System;

namespace MSSManagers.FirebirdFacade
{
    public static class CommonFbConstants
    {
        public const Int32 NrNotSet = -1;
        public const Int32 NrEmpty = 0;

        public const int TrueInt = 1;
        public const int FalseInt = 0;

        /// <summary>
        /// Equal to the value used in command MSSBackupCore.FbEventsCommands.ReconnectBackupService
        /// </summary>
        public const string ReconnectBackupServiceEventName = "RECONNECT_BACKUP_SERVICE";
    }
}