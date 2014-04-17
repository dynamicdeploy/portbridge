using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace Countries.DataAccess
{
    public sealed class SqlHelper
    {
        // Methods
        private SqlHelper()
        {
        }


        private static void AssignParameterValues(SqlParameter[] commandParameters, DataRow dataRow)
        {
            if ((commandParameters != null) && (dataRow != null))
            {
                int num = 0;
                foreach (SqlParameter parameter in commandParameters)
                {
                    if ((parameter.ParameterName == null) || (parameter.ParameterName.Length <= 1))
                    {
                        throw new Exception(string.Format("Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.", num, parameter.ParameterName));
                    }
                    if (dataRow.Table.Columns.IndexOf(parameter.ParameterName.Substring(1)) != -1)
                    {
                        parameter.Value = dataRow[parameter.ParameterName.Substring(1)];
                    }
                    num++;
                }
            }
        }

        private static void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters != null) && (parameterValues != null))
            {
                if (commandParameters.Length != parameterValues.Length)
                {
                    throw new ArgumentException("Parameter count does not match Parameter Value count.");
                }
                int index = 0;
                int length = commandParameters.Length;
                while (index < length)
                {
                    if (parameterValues[index] is IDbDataParameter)
                    {
                        IDbDataParameter parameter = (IDbDataParameter)parameterValues[index];
                        if (parameter.Value == null)
                        {
                            commandParameters[index].Value = DBNull.Value;
                        }
                        else
                        {
                            commandParameters[index].Value = parameter.Value;
                        }
                    }
                    else if (parameterValues[index] == null)
                    {
                        commandParameters[index].Value = DBNull.Value;
                    }
                    else
                    {
                        commandParameters[index].Value = parameterValues[index];
                    }
                    index++;
                }
            }
        }

        private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (commandParameters != null)
            {
                foreach (SqlParameter parameter in commandParameters)
                {
                    if (parameter != null)
                    {
                        if (((parameter.Direction == ParameterDirection.InputOutput) || (parameter.Direction == ParameterDirection.Input)) && (parameter.Value == null))
                        {
                            parameter.Value = DBNull.Value;
                        }
                        command.Parameters.Add(parameter);
                    }
                }
            }
        }

        public static SqlCommand CreateCommand(SqlConnection connection, string spName, params string[] sourceColumns)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            SqlCommand command = new SqlCommand(spName, connection);
            command.CommandType = CommandType.StoredProcedure;
            if ((sourceColumns != null) && (sourceColumns.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                for (int i = 0; i < sourceColumns.Length; i++)
                {
                    spParameterSet[i].SourceColumn = sourceColumns[i];
                }
                AttachParameters(command, spParameterSet);
            }
            return command;
        }

        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connection, commandType, commandText, null);
        }

        public static DataSet ExecuteDataset(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
        }

        public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteDataset(transaction, commandType, commandText, null);
        }

        public static DataSet ExecuteDataset(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
        }

        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connectionString, commandType, commandText, null);
        }

        public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
        }

        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            SqlCommand command = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, connection, null, commandType, commandText, commandParameters, out mustCloseConnection);
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);
                command.Parameters.Clear();
                if (mustCloseConnection)
                {
                    connection.Close();
                }
                return dataSet;
            }
        }

        public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            SqlCommand command = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);
                command.Parameters.Clear();
                return dataSet;
            }
        }

        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                return ExecuteDataset(connection, commandType, commandText, commandParameters);
            }
        }

        public static DataSet ExecuteDatasetTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((dataRow != null) && (dataRow.ItemArray.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
        }

        public static DataSet ExecuteDatasetTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((dataRow != null) && (dataRow.ItemArray.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
        }

        public static DataSet ExecuteDatasetTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((dataRow != null) && (dataRow.ItemArray.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
        }

        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connection, commandType, commandText, null);
        }

        public static int ExecuteNonQuery(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
        }

        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(transaction, commandType, commandText, null);
        }

        public static int ExecuteNonQuery(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
        }

        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connectionString, commandType, commandText, null);
        }

        public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
        }

        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            SqlCommand command = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, connection, null, commandType, commandText, commandParameters, out mustCloseConnection);
            int num = command.ExecuteNonQuery();
            command.Parameters.Clear();
            if (mustCloseConnection)
            {
                connection.Close();
            }
            return num;
        }

        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            SqlCommand command = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            int num = command.ExecuteNonQuery();
            command.Parameters.Clear();
            return num;
        }

        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                return ExecuteNonQuery(connection, commandType, commandText, commandParameters);
            }
        }

        public static int ExecuteNonQueryTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((dataRow != null) && (dataRow.ItemArray.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
        }

        public static int ExecuteNonQueryTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((dataRow != null) && (dataRow.ItemArray.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
        }

        public static int ExecuteNonQueryTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((dataRow != null) && (dataRow.ItemArray.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
        }

        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteReader(connection, commandType, commandText, null);
        }

        public static SqlDataReader ExecuteReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteReader(connection, CommandType.StoredProcedure, spName);
        }

        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteReader(transaction, commandType, commandText, null);
        }

        public static SqlDataReader ExecuteReader(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
        }

        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteReader(connectionString, commandType, commandText, null);
        }

        public static SqlDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
        }

        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return ExecuteReader(connection, null, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
        }

        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
        }

        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            SqlDataReader reader;
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                reader = ExecuteReader(connection, null, commandType, commandText, commandParameters, SqlConnectionOwnership.Internal);
            }
            catch
            {
                if (connection != null)
                {
                    connection.Close();
                }
                throw;
            }
            return reader;
        }

        private static SqlDataReader ExecuteReader(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, SqlConnectionOwnership connectionOwnership)
        {
            SqlDataReader reader2;
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            bool mustCloseConnection = false;
            SqlCommand command = new SqlCommand();
            try
            {
                SqlDataReader reader;
                PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
                if (connectionOwnership == SqlConnectionOwnership.External)
                {
                    reader = command.ExecuteReader();
                }
                else
                {
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                }
                bool flag2 = true;
                foreach (SqlParameter parameter in command.Parameters)
                {
                    if (parameter.Direction != ParameterDirection.Input)
                    {
                        flag2 = false;
                    }
                }
                if (flag2)
                {
                    command.Parameters.Clear();
                }
                reader2 = reader;
            }
            catch
            {
                if (mustCloseConnection)
                {
                    connection.Close();
                }
                throw;
            }
            return reader2;
        }

        public static SqlDataReader ExecuteReaderTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((dataRow != null) && (dataRow.ItemArray.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteReader(connection, CommandType.StoredProcedure, spName);
        }

        public static SqlDataReader ExecuteReaderTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((dataRow != null) && (dataRow.ItemArray.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
        }

        public static SqlDataReader ExecuteReaderTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((dataRow != null) && (dataRow.ItemArray.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
        }

        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteScalar(connection, commandType, commandText, null);
        }

        public static object ExecuteScalar(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
        }

        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteScalar(transaction, commandType, commandText, null);
        }

        public static object ExecuteScalar(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
        }

        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteScalar(connectionString, commandType, commandText, null);
        }

        public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
        }

        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            SqlCommand command = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, connection, null, commandType, commandText, commandParameters, out mustCloseConnection);
            object obj2 = command.ExecuteScalar();
            command.Parameters.Clear();
            if (mustCloseConnection)
            {
                connection.Close();
            }
            return obj2;
        }

        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            SqlCommand command = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            object obj2 = command.ExecuteScalar();
            command.Parameters.Clear();
            return obj2;
        }

        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                return ExecuteScalar(connection, commandType, commandText, commandParameters);
            }
        }

        public static object ExecuteScalarTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((dataRow != null) && (dataRow.ItemArray.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
        }

        public static object ExecuteScalarTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((dataRow != null) && (dataRow.ItemArray.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
        }

        public static object ExecuteScalarTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((dataRow != null) && (dataRow.ItemArray.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
        }

        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteXmlReader(connection, commandType, commandText, null);
        }

        public static XmlReader ExecuteXmlReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
        }

        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteXmlReader(transaction, commandType, commandText, null);
        }

        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
        }

        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            XmlReader reader2;
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            bool mustCloseConnection = false;
            SqlCommand command = new SqlCommand();
            try
            {
                PrepareCommand(command, connection, null, commandType, commandText, commandParameters, out mustCloseConnection);
                XmlReader reader = command.ExecuteXmlReader();
                command.Parameters.Clear();
                reader2 = reader;
            }
            catch
            {
                if (mustCloseConnection)
                {
                    connection.Close();
                }
                throw;
            }
            return reader2;
        }

        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            SqlCommand command = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            XmlReader reader = command.ExecuteXmlReader();
            command.Parameters.Clear();
            return reader;
        }

        public static XmlReader ExecuteXmlReaderTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((dataRow != null) && (dataRow.ItemArray.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
        }

        public static XmlReader ExecuteXmlReaderTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((dataRow != null) && (dataRow.ItemArray.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(spParameterSet, dataRow);
                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
        }

        public static void FillDataset(SqlConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            FillDataset(connection, commandType, commandText, dataSet, tableNames, null);
        }

        public static void FillDataset(SqlConnection connection, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, spParameterSet);
            }
            else
            {
                FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }

        public static void FillDataset(SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            FillDataset(transaction, commandType, commandText, dataSet, tableNames, null);
        }

        public static void FillDataset(SqlTransaction transaction, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if ((transaction != null) && (transaction.Connection == null))
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                AssignParameterValues(spParameterSet, parameterValues);
                FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, spParameterSet);
            }
            else
            {
                FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }

        public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                FillDataset(connection, commandType, commandText, dataSet, tableNames);
            }
        }

        public static void FillDataset(string connectionString, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                FillDataset(connection, spName, dataSet, tableNames, parameterValues);
            }
        }

        public static void FillDataset(SqlConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            FillDataset(connection, null, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        public static void FillDataset(SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                FillDataset(connection, commandType, commandText, dataSet, tableNames, commandParameters);
            }
        }

        private static void FillDataset(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            SqlCommand command = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                if ((tableNames != null) && (tableNames.Length > 0))
                {
                    string sourceTable = "Table";
                    for (int i = 0; i < tableNames.Length; i++)
                    {
                        if ((tableNames[i] == null) || (tableNames[i].Length == 0))
                        {
                            throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                        }
                        adapter.TableMappings.Add(sourceTable, tableNames[i]);
                        sourceTable = sourceTable + ((i + 1)).ToString();
                    }
                }
                adapter.Fill(dataSet);
                command.Parameters.Clear();
            }
            if (mustCloseConnection)
            {
                connection.Close();
            }
        }

        private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if ((commandText == null) || (commandText.Length == 0))
            {
                throw new ArgumentNullException("commandText");
            }
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }
            command.Connection = connection;
            command.CommandText = commandText;
            if (transaction != null)
            {
                if (transaction.Connection == null)
                {
                    throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                }
                command.Transaction = transaction;
            }
            command.CommandType = commandType;
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
        }

        public static void UpdateDataset(SqlCommand insertCommand, SqlCommand deleteCommand, SqlCommand updateCommand, DataSet dataSet, string tableName)
        {
            if (insertCommand == null)
            {
                throw new ArgumentNullException("insertCommand");
            }
            if (deleteCommand == null)
            {
                throw new ArgumentNullException("deleteCommand");
            }
            if (updateCommand == null)
            {
                throw new ArgumentNullException("updateCommand");
            }
            if ((tableName == null) || (tableName.Length == 0))
            {
                throw new ArgumentNullException("tableName");
            }
            using (SqlDataAdapter adapter = new SqlDataAdapter())
            {
                adapter.UpdateCommand = updateCommand;
                adapter.InsertCommand = insertCommand;
                adapter.DeleteCommand = deleteCommand;
                adapter.Update(dataSet, tableName);
                dataSet.AcceptChanges();
            }
        }

        // Nested Types
        private enum SqlConnectionOwnership
        {
            Internal,
            External
        }
    }




    public sealed class SqlHelperParameterCache
    {
        // Fields
        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        // Methods
        private SqlHelperParameterCache()
        {
        }

        public static void CacheParameterSet(string connectionString, string commandText, params SqlParameter[] commandParameters)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            if ((commandText == null) || (commandText.Length == 0))
            {
                throw new ArgumentNullException("commandText");
            }
            string str = connectionString + ":" + commandText;
            paramCache[str] = commandParameters;
        }

        private static SqlParameter[] CloneParameters(SqlParameter[] originalParameters)
        {
            SqlParameter[] parameterArray = new SqlParameter[originalParameters.Length];
            int index = 0;
            int length = originalParameters.Length;
            while (index < length)
            {
                parameterArray[index] = (SqlParameter)((ICloneable)originalParameters[index]).Clone();
                index++;
            }
            return parameterArray;
        }

        private static SqlParameter[] DiscoverSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            SqlCommand command = new SqlCommand(spName, connection);
            command.CommandType = CommandType.StoredProcedure;
            connection.Open();
            SqlCommandBuilder.DeriveParameters(command);
            connection.Close();
            if (!includeReturnValueParameter)
            {
                command.Parameters.RemoveAt(0);
            }
            SqlParameter[] parameterArray = new SqlParameter[command.Parameters.Count];
            command.Parameters.CopyTo((Array)parameterArray, 0);
            foreach (SqlParameter parameter in parameterArray)
            {
                parameter.Value = DBNull.Value;
            }
            return parameterArray;
        }

        public static SqlParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            if ((commandText == null) || (commandText.Length == 0))
            {
                throw new ArgumentNullException("commandText");
            }
            string str = connectionString + ":" + commandText;
            SqlParameter[] originalParameters = paramCache[str] as SqlParameter[];
            if (originalParameters == null)
            {
                return null;
            }
            return CloneParameters(originalParameters);
        }

        internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName)
        {
            return GetSpParameterSet(connection, spName, false);
        }

        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, false);
        }

        internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            using (SqlConnection connection2 = (SqlConnection)((ICloneable)connection).Clone())
            {
                return GetSpParameterSetInternal(connection2, spName, includeReturnValueParameter);
            }
        }

        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            if ((connectionString == null) || (connectionString.Length == 0))
            {
                throw new ArgumentNullException("connectionString");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
            }
        }

        private static SqlParameter[] GetSpParameterSetInternal(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if ((spName == null) || (spName.Length == 0))
            {
                throw new ArgumentNullException("spName");
            }
            string str = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");
            SqlParameter[] originalParameters = paramCache[str] as SqlParameter[];
            if (originalParameters == null)
            {
                SqlParameter[] parameterArray2 = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                paramCache[str] = parameterArray2;
                originalParameters = parameterArray2;
            }
            return CloneParameters(originalParameters);
        }
    }


}
