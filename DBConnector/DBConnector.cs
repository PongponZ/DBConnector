using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DBConnector
{
    public class Database
    {
        private SqlConnectionStringBuilder _builder = new SqlConnectionStringBuilder();
        private SqlConnection _connection;

        public Database()
        {
            _builder.DataSource = ConfigurationManager.AppSettings["Host"];
            _builder.InitialCatalog = ConfigurationManager.AppSettings["DatabaseName"];
            _builder.UserID = ConfigurationManager.AppSettings["Username"];
            _builder.Password = ConfigurationManager.AppSettings["Password"];

            _connection = new SqlConnection(_builder.ConnectionString);
            _connection.Open();

            if (_connection.State == ConnectionState.Open)
                Console.WriteLine($"Database Connected !!.");

            _connection.Close();
        }

        public Database(string host, string databaseName, string username, string password)
        {
            _builder.DataSource = host;
            _builder.InitialCatalog = databaseName;
            _builder.UserID = username;
            _builder.Password = password;

            _connection = new SqlConnection(_builder.ConnectionString);
            _connection.Open();

            if (_connection.State == ConnectionState.Open)
                Console.WriteLine($"Database Connected.");

            _connection.Close();
        }

        public bool Exec(string statement)
        {
            return Exec(statement, null);
        }

        public bool Exec(string statement, Parameters[] parameters)
        {
            try
            {
                _connection.Open();

                SqlCommand cmd = new SqlCommand(statement, _connection);

                if (parameters != null)
                    cmd.Parameters.AddRange(ConvertParam(parameters));

                cmd.ExecuteNonQuery();
                cmd.Dispose();

                _connection.Close();

                return true;
            }
            catch (SqlException err)
            {
                Console.WriteLine(err);
                return false;
            }

        }

        public DataTable Query(string statement)
        {
            return Query(statement, null);
        }


        public DataTable Query(string statement, Parameters[] parameters)
        {
            try
            {
                _connection.Open();

                SqlCommand cmd = new SqlCommand(statement, _connection);


                if (parameters != null)
                    cmd.Parameters.AddRange(ConvertParam(parameters));


                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable table = new DataTable();

                adapter.Fill(table);
                adapter.Dispose();
                cmd.Dispose();

                _connection.Close();


                return table;
            }
            catch (SqlException err)
            {
                Console.WriteLine(err);
                return null;
            }

        }

        public DataTable StoreQuery(string statement)
        {
            return StoreQuery(statement, null);
        }

        public DataTable StoreQuery(string storeName, Parameters[] parameters)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = _connection.CreateCommand();

            cmd.CommandText = storeName;
            cmd.CommandType = CommandType.StoredProcedure;

            if (parameters != null)
                cmd.Parameters.AddRange(ConvertParam(parameters));

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);

            adapter.Fill(table);
            adapter.Dispose();
            cmd.Dispose();

            return table;
        }

        public bool ExecStore(string storeName)
        {
            return ExecStore(storeName, null);
        }

        public bool ExecStore(string storeName, Parameters[] parameters)
        {
            try
            {
                _connection.Open();

                SqlCommand cmd = _connection.CreateCommand();

                cmd.CommandText = storeName;
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                    cmd.Parameters.AddRange(ConvertParam(parameters));
              
                cmd.ExecuteNonQuery();
                cmd.Dispose();

                _connection.Close();

                return true;
            }
            catch (SqlException err)
            {
                Console.WriteLine(err);
                return false;
            }
        }

        private SqlParameter[] ConvertParam(Parameters[] param)
        {
            
            SqlParameter[] pm = new SqlParameter[param.Length];

            for (int i = 0; i < param.Length; i++)
            {
                pm[i] = new SqlParameter(param[i].paramName, param[i].type) { Value=param[i].value };
            }
            
            return pm;
        }

    }

    public class Parameters
    {
        public string paramName { get; set; }
        public SqlDbType type { get; set; }
        public object value { get; set; }
    }

}
