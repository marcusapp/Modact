using System.Data;

namespace Modact
{
    public class DbHelper : IDisposable
    {
        private bool _disposed = false;
        private bool _isTransactional = false;
        private IDbConnection? _connection;
        private IDbTransaction? _transaction;

        public string ConnectionString { get; init; }
        public DatabaseType DatabaseType { get; init; }

        public DbHelper(string connectionString, DatabaseType? engineType = DatabaseType.SQLServer, bool isTransactional = false)
        {
            this.ConnectionString = connectionString;
            this.DatabaseType = engineType ?? DatabaseType.SQLServer;
            this._isTransactional = isTransactional;
        }

        public IDbConnection Connection(bool isManagedConnection = true)
        {
            if (isManagedConnection)
            {
                if (_connection == null)
                {
                    _connection = NewConnection();
                }
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                if (_isTransactional)
                {
                    if (_transaction == null)
                    {
                        _transaction = _connection.BeginTransaction();
                    }
                }
                return _connection;
            }
            else
            {
                return NewConnection();
            }
        }

        private IDbConnection NewConnection()
        {
            if (DatabaseType != DatabaseType.SQLServer)
            {
                throw new Exception("DB engine current only support SQL server.");
            }
            IDbConnection connection;
            switch (DatabaseType)
            {
                //case DatabaseType.MySQL:
                //    connection = new MySql.Data.MySqlClient.MySqlConnection(this.ConnectionString);
                //    break;
                //case DatabaseType.PostgreSQL:
                //    connection = new Npgsql.NpgsqlConnection(strConn);
                //    break;
                //case DatabaseType.SQLite:
                //    connection = new SQLiteConnection(this.ConnectionString);
                //    break;
                //case DatabaseType.Oracle:
                //    connection = new Oracle.ManagedDataAccess.Client.OracleConnection(this.ConnectionString);
                //    break;
                //case DatabaseType.DB2:
                //    connection = new System.Data.OleDb.OleDbConnection(this.ConnectionString);
                //    break;
                default:
                    connection = new System.Data.SqlClient.SqlConnection(this.ConnectionString);
                    break;
            }
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return connection;
        }

        public IDbTransaction? Transaction()
        {
            if (!_isTransactional)
            {
                return null;
            }
            if (_transaction == null)
            {
                Connection();
            }
            return _transaction;
        }

        public void TransactionEnable()
        {
            if (!_isTransactional)
            {
                _isTransactional = true;
                Connection();
            }
        }

        public void TransactionDisable(bool isProcessCommit = false)
        {
            if (_isTransactional)
            {
                _isTransactional = false;
                if (_transaction != null)
                {
                    if (isProcessCommit){ _transaction.Commit(); }
                    else { _transaction.Rollback(); }
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
        }

        public DbHelper Commit()
        {
            if (_transaction == null) { return this; }
            if (_transaction.Connection == null) { return this; }

            if (_transaction.Connection.State != ConnectionState.Open)
            {
                _transaction.Connection.Close();
                _transaction.Connection.Open();
            }
            _transaction.Commit();

            return this;
        }

        public DbHelper Rollback()
        {
            if (_transaction == null) { return this; }
            if (_transaction.Connection == null) { return this; }

            if (_transaction.Connection.State != ConnectionState.Open)
            {
                _transaction.Connection.Close();
                _transaction.Connection.Open();
            }
            _transaction.Rollback();

            return this;
        }

        public DbHelper Close()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
            }
            if (_connection != null)
            {
                _connection.Close();
            }

            return this;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) { return; }

            if (disposing)
            {
                Close();
                if (_connection != null)
                {
                    _connection.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DbHelper()
        {
            Dispose(false);
        }
    }
}
