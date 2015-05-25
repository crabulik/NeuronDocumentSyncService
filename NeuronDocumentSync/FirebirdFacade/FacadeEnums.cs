namespace MSSManagers.FirebirdFacade
{
    public enum FacadeCommandOptions
    {
        NotSet,
        //Read, PS: Add if needed
        Write
    }

    public enum ConnectionStatus
    {
        Ok,
        DbFileError,
        ServerError,
        FbLoginError,
        DbCorrupt,
        DbShutdown,
        TooMuchConnect,
        HostError,
        UnableOpenDatabase
    }

    public enum FbConnectionErrorCode
    {
        IoErrorForFile = 335544344,
        IoOpenErr = 335544734,
        FileInUse = 335544791,
        Unavailable = 335544375,
        BadDbFormat = 335544323,
        NetworkError = 335544721,
        ConnectReject = 335544421,
        IbError = 335544689,
        DbUserNameNotFound = 335544753,
        Login = 335544472,
        BadChecksum = 335544649,
        DbCorrupt = 335544335,
        MetadataCorrupt = 335544346,
        Corrupt = 335544404,
        ShutInProg = 335544506,
        Shutdown = 335544528,
        MaxAttExceeded = 335544744,

        FailedToLocateHostMachine = 335544704,
        NameWasNotFoundInHostOrDNS = 335544706,

        UnableOpenDatabase = 336723983



    }
}