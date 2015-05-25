using System;
using System.Data;
using FirebirdSql.Data.FirebirdClient;

namespace MSSManagers.FirebirdFacade
{
    public class FbFacade : IDisposable
    {
        private const string FirstNextErrorMessage = "Not executed First method before usin Next.";
        #region Firebid Provider Objects

        private FbConnection _connection;
        private FbCommand _command;
        private FbDataAdapter _adapter;
        private FbTransaction _writeTransaction;
        private FbParameterCollection _parameters;

        #endregion

        private static string _defaultConnectionString = string.Empty;

        private readonly string _connectionString;

        private FbException _lastFbException = null;


        // TODO: Implement First-Next behavior for command.
        enum PrepareCommandType
        {
            NotUsed, // DefaultValue
            NonQuery,
            Scalar,
            Reader,
            DataSet
        }
        private PrepareCommandType _isCommandPrepareType = PrepareCommandType.NotUsed;

        #region Static Methods


        #endregion

        #region Properties

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public FbParameterCollection Parameters
        {
            get { return _parameters; }
        }

        public FbTransaction WriteTransaction
        {
            get { return _writeTransaction; }
        }

        public FbException LastFbException
        {
            get { return _lastFbException; }
        }

        #endregion

        #region Connection methods

        public bool OpenConnection()
        {
            if ((_connection.State != ConnectionState.Closed) || (_connection.ConnectionString == String.Empty))
            {
                return false;
            }
            _connection.Open();

            return true;
        }

        public bool CloseConnection()
        {
            if (_writeTransaction != null)
            {
                if ((_writeTransaction.Connection != null) &&
                    (_writeTransaction.Connection.State != ConnectionState.Closed))
                {
                    _writeTransaction.Commit();
                }
            }
            _connection.Close();
            return true;
        }

        #endregion

        #region Transaction methods

        public bool BeginWriteTransaction()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                var options = new FbTransactionOptions();
                options.TransactionBehavior = FbTransactionBehavior.Write |
                                              FbTransactionBehavior.Concurrency |
                                              FbTransactionBehavior.NoWait;
                _writeTransaction = _connection.BeginTransaction(options);
                _command.Transaction = _writeTransaction;
                return true;
            }
            return false;
        }

        public bool CommitWriteTransaction()
        {
            if (_writeTransaction != null)
            {
                _writeTransaction.Commit();
                _writeTransaction.Dispose();
                _writeTransaction = null;
                _command.Transaction = null;
                return true;
            }
            return false;
        }

        public void RollbackWriteTransaction()
        {
            if (_writeTransaction != null)
            {
                _writeTransaction.Rollback();
                _writeTransaction.Dispose();
                _writeTransaction = null;
                _command.Transaction = null;
            }
        }

        #endregion

        #region Execute methods

        /// <param name="commandText">SQL query text.</param>
        /// <param name="commandOptions">Value "FacadeCommandOptions.Write" uses for
        /// automatic begin and end of the transaction for quick write queries.</param>
        public int ExecuteNonQuery(string commandText, FacadeCommandOptions commandOptions = FacadeCommandOptions.NotSet)
        {
            bool commitWrite = false;
            if ((commandOptions == FacadeCommandOptions.Write) && (_writeTransaction == null))
            {
                if (!BeginWriteTransaction())
                    return -1;
                commitWrite = true;
            }
            _command.CommandText = commandText;
            var result = _command.ExecuteNonQuery();
            _isCommandPrepareType = PrepareCommandType.NotUsed;
            if (commitWrite)
            {
                CommitWriteTransaction();
            }
            return result;
        }

        /// <param name="commandText">SQL query text.</param>
        /// <param name="commandOptions">Value "FacadeCommandOptions.Write" uses for
        /// automatic begin and end of the transaction for quick write queries.</param>
        public object ExecuteScalar(string commandText,
            FacadeCommandOptions commandOptions = FacadeCommandOptions.NotSet)
        {
            bool commitWrite = false;
            if ((commandOptions == FacadeCommandOptions.Write) && (_writeTransaction == null))
            {
                if (!BeginWriteTransaction())
                    return null;
                commitWrite = true;
            }
            _command.CommandText = commandText;
            var result = _command.ExecuteScalar();
            _isCommandPrepareType = PrepareCommandType.NotUsed;
            if (commitWrite)
            {
                CommitWriteTransaction();
            }
            return result;
        }

        public DataTable ExecuteForDataTable(string commandText)
        {
            try
            {
                _command.CommandText = commandText;
                _adapter = new FbDataAdapter(_command);
                var table = new DataTable();
                _adapter.Fill(table);
                _isCommandPrepareType = PrepareCommandType.NotUsed;
                return table;
            }
            finally
            {
                _adapter.Dispose();
                _adapter = null;
            }
        }

        /// <param name="commandText">SQL query text.</param>
        public FbDataReader ExecuteReader(string commandText)
        {
            _command.CommandText = commandText;
            _isCommandPrepareType = PrepareCommandType.NotUsed;
            return _command.ExecuteReader();
        }

        /// <param name="commandText">SQL query text.</param>
        /// <param name="behavior">Set behavior for execution comand with Reader</param>
        public FbDataReader ExecuteReader(string commandText, CommandBehavior behavior)
        {
            _command.CommandText = commandText;
            _isCommandPrepareType = _isCommandPrepareType = PrepareCommandType.NotUsed;
            return _command.ExecuteReader(behavior);
        }

        #region First-Next executions
        //PS: AutoCommit write operations in First-Next executions are not used!

        public object ExecuteScalarFirst(string commandText)
        {
            _command.CommandText = commandText;
            _command.Prepare();
            var result = _command.ExecuteScalar();
            _isCommandPrepareType = PrepareCommandType.Scalar;
            return result;
        }

        public object ExecuteScalarNext()
        {
            if (_isCommandPrepareType != PrepareCommandType.Scalar)
            {
                throw new InvalidOperationException(FirstNextErrorMessage);
            }
            var result = _command.ExecuteScalar();
            return result;
        }
        #endregion

        public int ExecuteNonQueryFirst(string commandText)
        {
            _command.CommandText = commandText;
            _command.Prepare();
            var result = _command.ExecuteNonQuery();
            _isCommandPrepareType = PrepareCommandType.NonQuery;
            return result;
        }

        public int ExecuteNonQueryNext()
        {
            if (_isCommandPrepareType != PrepareCommandType.NonQuery)
            {
                throw new InvalidOperationException(FirstNextErrorMessage);
            }
            var result = _command.ExecuteNonQuery();
            return result;
        }
        #endregion

        public int GenNrForOldDb(string generatorName)
        {
            const string qryGenerator = @"
                EXECUTE block (GeneratorName VARCHAR(64) = @GeneratorName)
                  RETURNS (ResultNr INT)
                AS
                BEGIN
                    SELECT ""Nr"" FROM ""_Nr_Generator""
                        WHERE ""Table"" = :GeneratorName
                        For Update With Lock
                        INTO ResultNr;
                    if (ResultNr is null) then
                    begin
                      ResultNr = 100001;
                      INSERT INTO ""_Nr_Generator"" (""Nr"", ""Table"")
                        VALUES (:ResultNr, :GeneratorName);
                    end
                    else
                    begin
                        ResultNr = ResultNr + 1;
                        UPDATE ""_Nr_Generator""
                            SET ""Nr"" = :ResultNr
                            where ""Table"" = :GeneratorName;
                    end
                    SUSPEND;
                end;                
            ";
            FbConnection additionalConnection = new FbConnection(_connectionString);
            try
            {
                var getnrCommand = additionalConnection.CreateCommand();
                getnrCommand.CommandText = qryGenerator;
                getnrCommand.Parameters.AddWithValue("@GeneratorName", generatorName);

                var options = new FbTransactionOptions();
                options.TransactionBehavior = FbTransactionBehavior.Write |
                                              FbTransactionBehavior.Concurrency |
                                              FbTransactionBehavior.Wait;
                additionalConnection.Open();
                getnrCommand.Transaction = additionalConnection.BeginTransaction(options, "OldNrGenerator");

                try
                {
                    var result = getnrCommand.ExecuteScalar();
                    getnrCommand.Transaction.Commit();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        return -1;
                    }
                }
                catch (Exception)
                {
                    getnrCommand.Transaction.Rollback();
                    throw;
                }
            }
            finally
            {
                additionalConnection.Close();
            }

        }

        public int GenNr(string generatorName)
        {
            const string qryGeneratorStart = @" SELECT NEXT VALUE FOR ";
            const string qryGeneratorEnd = @" FROM RDB$DATABASE ";
            FbConnection additionalConnection = new FbConnection(_connectionString);
            try
            {
                var getnrCommand = additionalConnection.CreateCommand();
                getnrCommand.CommandText = qryGeneratorStart + generatorName + qryGeneratorEnd;

                additionalConnection.Open();

                try
                {
                    var result = getnrCommand.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        return -1;
                    }
                }
                catch (Exception)
                {
                    getnrCommand.Transaction.Rollback();
                    throw;
                }
            }
            finally
            {
                additionalConnection.Close();
            }
        }

        public ConnectionStatus CheckConnection()
        {
            try
            {
                _connection.Open();
                _connection.Close();
            }
            catch (Exception e)
            {
                if (e is FbException)
                {
                    _lastFbException = (FbException)e;
                    switch ((FbConnectionErrorCode)_lastFbException.ErrorCode)
                    {
                        case FbConnectionErrorCode.IoErrorForFile:
                        case FbConnectionErrorCode.IoOpenErr:
                        case FbConnectionErrorCode.FileInUse:
                        case FbConnectionErrorCode.Unavailable:
                        case FbConnectionErrorCode.BadDbFormat:
                            return ConnectionStatus.DbFileError;
                        case FbConnectionErrorCode.NetworkError:
                        case FbConnectionErrorCode.ConnectReject:
                        case FbConnectionErrorCode.IbError:
                            return ConnectionStatus.ServerError;
                        case FbConnectionErrorCode.DbUserNameNotFound:
                        case FbConnectionErrorCode.Login:
                            return ConnectionStatus.FbLoginError;
                        case FbConnectionErrorCode.BadChecksum:
                        case FbConnectionErrorCode.DbCorrupt:
                        case FbConnectionErrorCode.MetadataCorrupt:
                        case FbConnectionErrorCode.Corrupt:
                            return ConnectionStatus.DbCorrupt;
                        case FbConnectionErrorCode.ShutInProg:
                        case FbConnectionErrorCode.Shutdown:
                            return ConnectionStatus.DbShutdown;
                        case FbConnectionErrorCode.MaxAttExceeded:
                            return ConnectionStatus.TooMuchConnect;
                        case FbConnectionErrorCode.FailedToLocateHostMachine:
                        case FbConnectionErrorCode.NameWasNotFoundInHostOrDNS:
                            return ConnectionStatus.HostError;
                        case FbConnectionErrorCode.UnableOpenDatabase:
                            return ConnectionStatus.UnableOpenDatabase;
                    }
                }
                throw;
            }
            return ConnectionStatus.Ok;
        }

        public void SetCurrentConnectionToDefault()
        {
            _defaultConnectionString = _connectionString;
        }

        public FbFacade(bool forceOpen, string connectionString)
        {
            _connectionString = connectionString;
            _connection = new FbConnection { ConnectionString = _connectionString };
            _command = _connection.CreateCommand();
            _parameters = _command.Parameters;
            if (forceOpen)
            {
                OpenConnection();
            }
        }

        public void Dispose()
        {
            if (_writeTransaction != null)
            {
                if ((_writeTransaction.Connection != null) &&
                    (_writeTransaction.Connection.State != ConnectionState.Closed))
                {
                    _writeTransaction.Rollback();
                }
                _command.Dispose();
            }
            if (_command != null)
            {
                _command.Dispose();
            }
            if (_adapter != null)
            {
                _adapter.Dispose();
            }
            if (_connection != null)
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    _connection.Close();
                }
                _connection.Dispose();
            }
            _parameters = null;
            _connection = null;
            _command = null;
            _adapter = null;
            _writeTransaction = null;
        }
    }
}