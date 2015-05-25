using System;
using System.Data;
using FirebirdSql.Data.FirebirdClient;

namespace MSSManagers.FirebirdFacade
{
    public class FbEventer : IDisposable
    {
        private FbRemoteEvent _fbRemoteEvent;

        private FbConnection _fbEventConnection;

        public FbRemoteEvent FbRemoteEvent
        {
            get { return _fbRemoteEvent; }
        }

        public FbConnection FbEventConnection
        {
            get { return _fbEventConnection; }
        }

        #region Connection methods

        public bool OpenConnection()
        {
            if ((_fbEventConnection.State != ConnectionState.Closed) || (_fbEventConnection.ConnectionString == String.Empty))
            {
                return false;
            }
            _fbEventConnection.Open();
            _fbRemoteEvent = new FbRemoteEvent(_fbEventConnection);
            return true;
        }

        public bool CloseConnection()
        {
            if (_fbRemoteEvent != null)
            {
                _fbRemoteEvent = null;
            }
            try
            {
                _fbEventConnection.Close();
                _fbEventConnection.Dispose();
            }
            catch (Exception)
            {
                _fbEventConnection.Dispose();
            }

            return true;
        }

        #endregion


        public void Dispose()
        {
            if (_fbRemoteEvent != null)
            {
                _fbRemoteEvent.CancelEvents();
                _fbRemoteEvent = null;
            }

            if (_fbEventConnection != null)
            {
                if (_fbEventConnection.State != ConnectionState.Closed)
                {
                    try
                    {
                        _fbEventConnection.Close();
                    }
                    catch
                    {
                    }

                }
                _fbEventConnection.Dispose();
            }
        }
    }
}