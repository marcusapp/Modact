using System;

namespace Modact
{
    public class DbHelperList
    {
        private readonly Dictionary<string, Lazy<DbHelper>> _dbList = new();
        private readonly bool _isTransactional; //This list transaction only affect to one DB itself. Across DB such as, if _dbList["A"] has error rollback, will not affect to _dbList["B"].
        private readonly string _keyAppDatabase;
        private readonly string _keyLogDatabase;

        public DbHelper? AppDatabase
        {
            get
            {
                if (string.IsNullOrEmpty(_keyAppDatabase))
                {
                    return null;
                }
                if (_dbList.ContainsKey(_keyAppDatabase))
                {
                    return _dbList[_keyAppDatabase].Value;
                }
                else
                {
                    throw new NullReferenceException("Database: [" + _keyAppDatabase + "]");
                }
            }
        }
        public DbHelper? LogDatabase
        {
            get
            {
                if (string.IsNullOrEmpty(_keyLogDatabase))
                {
                    return null;
                }
                if (_dbList.ContainsKey(_keyLogDatabase))
                {
                    return _dbList[_keyLogDatabase].Value;
                }
                else
                {
                    throw new NullReferenceException("Database: [" + _keyLogDatabase + "]");
                }
            }
        }

        public DbHelperList(Dictionary<string, DatabaseConnectionConfig> databaseList, bool _isTransactional = false, string? keyAppDatabase = null, string? keyLogDatabase = null)
        {
            keyAppDatabase = keyAppDatabase ?? string.Empty;
            keyLogDatabase = keyLogDatabase ?? string.Empty;
            this._keyAppDatabase = keyAppDatabase;
            this._keyLogDatabase = keyLogDatabase;
            this._isTransactional = _isTransactional;
            DbInit(databaseList);
        }

        public DbHelper this[string databaseKey]
        {
            get
            {
                if (_dbList == null)
                {
                    return null;
                }
                return _dbList[databaseKey].Value;
            }
        }

        private void DbInit(Dictionary<string, DatabaseConnectionConfig> DbConnList)
        {
            foreach (KeyValuePair<string, DatabaseConnectionConfig> kv in DbConnList)
            {
                Lazy<DbHelper> dbHelper;
                if (kv.Key != this._keyLogDatabase)
                {
                    dbHelper = new Lazy<DbHelper>(() => new DbHelper(kv.Value.ConnectionString, kv.Value.Type, this._isTransactional));
                }
                else
                {
                    dbHelper = new Lazy<DbHelper>(() => new DbHelper(kv.Value.ConnectionString, kv.Value.Type, false));
                }                
                this._dbList.Add(kv.Key, dbHelper);
            }
        }
        public DbHelperList CommitAll()
        {
            foreach (var pair in _dbList)
            {
                pair.Value.Value.Commit();
            }

            return this;
        }

        public DbHelperList RollbackAll()
        {
            foreach (var pair in _dbList)
            {
                pair.Value.Value.Rollback();
            }

            return this;
        }

        public void DisposeAll()
        {
            foreach (var pair in _dbList)
            {
                pair.Value.Value.Dispose();
            }
        }
    }
}
