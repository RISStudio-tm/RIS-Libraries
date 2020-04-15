using System;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace RIS.Connection.MySQL
{
    /// <summary>
    ///     Представляет запросы для работы с MySQL базой данных. Этот класс не может быть унаследован.
    /// </summary>
    public sealed class Requests
    {
        private object[] LockObjExecReaders { get; }
        private MySqlConnection[] Connections { get; }
        private object LockObjNextConnection { get; }
        private ushort _nextConnectionIndex;
        private ushort NextConnectionIndex
        {
            get
            {
                lock (LockObjNextConnection)
                {
                    if (_nextConnectionIndex > Connections.Length - 1)
                        _nextConnectionIndex = 0;
                    return _nextConnectionIndex++;
                }
            }
        }
        private TimeSpan _commandTimeout;
        /// <summary>
        ///     Позволяет получать и устанавливать стандартное время ожидания SQL-команд.
        /// </summary>
        public TimeSpan CommandTimeout
        {
            get
            {
                return _commandTimeout;
            }
            set
            {
                if (value < TimeSpan.FromMilliseconds(3000))
                {
                    _commandTimeout = TimeSpan.FromMilliseconds(3000);
                }
                else
                {
                    _commandTimeout = value;
                }
            }
        }
        /// <summary>
        ///     Позволяет получать глобальный токен отмены SQL-команд.
        /// </summary>
        public CancellationTokenSource GlobalCancellationToken { get; }

        private MySQLConnection CurrentMySQLConnection { get; }

        internal Requests(MySQLConnection sqlConnection, MySqlConnection[] connections, ushort millisecondsCommandTimeout = 20000)
        {
            CurrentMySQLConnection = sqlConnection;
            Connections = connections;
            CommandTimeout = TimeSpan.FromMilliseconds(millisecondsCommandTimeout);

            LockObjExecReaders = new object[connections.Length];
            for (byte i = 0; i < LockObjExecReaders.Length; ++i)
            {
                LockObjExecReaders[i] = new object();
            }
            LockObjNextConnection = new object();

            GlobalCancellationToken = new CancellationTokenSource();
        }
        internal Requests(MySQLConnection sqlConnection, MySqlConnection[] connections, TimeSpan timeSpanTimeout)
        {
            CurrentMySQLConnection = sqlConnection;
            Connections = connections;
            CommandTimeout = timeSpanTimeout;

            LockObjExecReaders = new object[connections.Length];
            for (byte i = 0; i < LockObjExecReaders.Length; ++i)
            {
                LockObjExecReaders[i] = new object();
            }
            LockObjNextConnection = new object();

            GlobalCancellationToken = new CancellationTokenSource();
        }

        /// <summary>
        ///     Вызывает отмену всех SQL-команд.
        /// </summary>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="Exception"></exception>
        public void CancelAllRequests()
        {
            try
            {
                GlobalCancellationToken.Cancel();
            }
            catch (Exception)
            {
                var exception = new Exception("Failed to cancel all requests");
                Events.DShowError?.Invoke(this, 
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
        }
        
        private MySqlConnection GetNextMySqlConnection(out ushort connectionIndex)
        {
            connectionIndex = NextConnectionIndex;
            return Connections[connectionIndex];
        }



        private void CommandExecuteNonQuery(ushort connectionIndex, IsolationLevel isolationLevel,
            ref MySqlCommand command, ref MySqlTransaction transaction, CancellationToken cancellationToken)
        {
            lock (LockObjExecReaders[connectionIndex])
            {
                cancellationToken.ThrowIfCancellationRequested();

                transaction = Connections[connectionIndex].BeginTransaction(isolationLevel);
                command.Transaction = transaction;

                command.ExecuteNonQuery();

                transaction.Commit();
            }
        }

        private string[] CommandExecuteReader(ushort connectionIndex, IsolationLevel isolationLevel,
            ref MySqlCommand command, ref MySqlTransaction transaction, CancellationToken cancellationToken)
        {
            string[] result = Array.Empty<string>();

            lock (LockObjExecReaders[connectionIndex])
            {
                cancellationToken.ThrowIfCancellationRequested();

                transaction = Connections[connectionIndex].BeginTransaction(isolationLevel);
                command.Transaction = transaction;

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();

                    result = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; ++i)
                    {
                        result[i] = reader.GetValue(i).ToString();
                    }
                }

                transaction.Commit();
            }

            return result;
        }

        private DataSet CommandExecuteAdapter(ushort connectionIndex, IsolationLevel isolationLevel, 
            ref MySqlDataAdapter adapter, ref MySqlTransaction transaction, CancellationToken cancellationToken)
        {
            DataSet result = new DataSet();

            lock (LockObjExecReaders[connectionIndex])
            {
                cancellationToken.ThrowIfCancellationRequested();

                transaction = Connections[connectionIndex].BeginTransaction(isolationLevel);
                adapter.SelectCommand.Transaction = transaction;

                adapter.Fill(result);

                transaction.Commit();
            }

            return result;
        }



        private void ReplaceDBNullParameterValue(string value, ref MySqlCommand command, 
            string parameterName)
        {
            if (value == "NULL" || value == null)
                command.Parameters[parameterName].Value = DBNull.Value;
            if (value == "'NULL'")
                command.Parameters[parameterName].Value = "NULL";
        }
        private void ReplaceDBNullParameterValue(string value, ref MySqlDataAdapter adapter,
            string parameterName)
        {
            if (value == "NULL" || value == null)
                adapter.SelectCommand.Parameters[parameterName].Value = DBNull.Value;
            if (value == "'NULL'")
                adapter.SelectCommand.Parameters[parameterName].Value = "NULL";
        }

        private void ReplaceFunctionParameterValue(string value, ref MySqlCommand command, 
            string parameterName, ref string sql)
        {
            if (value == "CURRENT_TIMESTAMP")
                sql = sql.Replace(parameterName, "CURRENT_TIMESTAMP");
            if (value == "'CURRENT_TIMESTAMP'")
                command.Parameters[parameterName].Value = "CURRENT_TIMESTAMP";
        }
        private void ReplaceFunctionParameterValue(string value, ref MySqlCommand command,
            string parameterName, ref StringBuilder sqlBuilder)
        {
            if (value == "CURRENT_TIMESTAMP")
                sqlBuilder = sqlBuilder.Replace(parameterName, "CURRENT_TIMESTAMP");
            if (value == "'CURRENT_TIMESTAMP'")
                command.Parameters[parameterName].Value = "CURRENT_TIMESTAMP";
        }
        private void ReplaceFunctionParameterValue(string value, ref MySqlDataAdapter adapter,
            string parameterName, ref string sql)
        {
            if (value == "CURRENT_TIMESTAMP")
                sql = sql.Replace(parameterName, "CURRENT_TIMESTAMP");
            if (value == "'CURRENT_TIMESTAMP'")
                adapter.SelectCommand.Parameters[parameterName].Value = "CURRENT_TIMESTAMP";
        }
        private void ReplaceFunctionParameterValue(string value, ref MySqlDataAdapter adapter,
            string parameterName, ref StringBuilder sqlBuilder)
        {
            if (value == "CURRENT_TIMESTAMP")
                sqlBuilder = sqlBuilder.Replace(parameterName, "CURRENT_TIMESTAMP");
            if (value == "'CURRENT_TIMESTAMP'")
                adapter.SelectCommand.Parameters[parameterName].Value = "CURRENT_TIMESTAMP";
        }



        /// <summary>
        ///     Вызывает выполнение хранимой процедуры с названием <paramref name="nameProcedure"/>, входными параметрами <paramref name="inParametersProcedure"/> и выходными параметрами <paramref name="outParametersNamesProcedure"/>.
        /// </summary>
        /// <param name="nameProcedure">
        ///     Название процедуры.
        /// </param>
        /// <param name="inParametersProcedure">
        ///     Входные параметры процедуры.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="outParametersNamesProcedure">
        ///     Массив названий выходных параметров процедуры.
        /// </param>
        /// <param name="outputValues">
        ///     Массив значений выходных параметров процедуры, доступных после её вызова.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public long ExecStoredProcedure(string nameProcedure, (string Name, string Value)[] inParametersProcedure, 
            string[] outParametersNamesProcedure, out string[] outputValues,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            long result = 0;
            string[] outputValuesTemp = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();
                
                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"{nameProcedure}");

                    for (int i = 0; i < inParametersProcedure.Length; ++i)
                    {
                        command.Parameters.AddWithValue(inParametersProcedure[i].Name, inParametersProcedure[i].Value);
                        command.Parameters[i].Direction = ParameterDirection.Input;

                        ReplaceDBNullParameterValue(inParametersProcedure[i].Value,
                            ref command, command.Parameters[i].ParameterName);
                        ReplaceFunctionParameterValue(inParametersProcedure[i].Value,
                            ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                    }

                    for (int i = inParametersProcedure.Length; i < inParametersProcedure.Length + outParametersNamesProcedure.Length; ++i)
                    {
                        int arrayIndex = i - inParametersProcedure.Length;

                        command.Parameters.Add(outParametersNamesProcedure[arrayIndex]);
                        command.Parameters[i].Direction = ParameterDirection.Output;
                    }

                    command.Parameters.Add("@r_cmd_return_value", MySqlDbType.Int64);
                    command.Parameters["@r_cmd_return_value"].Direction = ParameterDirection.ReturnValue;

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction, cancellationToken.Token);

                    outputValuesTemp = new string[outParametersNamesProcedure.Length];
                    for (int i = inParametersProcedure.Length; i < inParametersProcedure.Length + outParametersNamesProcedure.Length; ++i)
                    {
                        int arrayIndex = i - inParametersProcedure.Length;

                        outputValuesTemp[arrayIndex] = command.Parameters[i].Value.ToString();
                    }

                    result = (long)command.Parameters["@r_cmd_return_value"].Value;
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{CommandTimeout}]");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            outputValues = outputValuesTemp;
            return result;
        }
        /// <summary>
        ///     Вызывает выполнение хранимой процедуры с названием <paramref name="nameProcedure"/>, входными параметрами <paramref name="inParametersProcedure"/>, выходными параметрами <paramref name="outParametersNamesProcedure"/> и временем ожидания <paramref name="timeout"/>.
        /// </summary>
        /// <param name="nameProcedure">
        ///     Название процедуры.
        /// </param>
        /// <param name="inParametersProcedure">
        ///     Входные параметры процедуры.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="outParametersNamesProcedure">
        ///     Массив названий выходных параметров процедуры.
        /// </param>
        /// <param name="outputValues">
        ///     Массив значений выходных параметров процедуры, доступных после её вызова.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public long ExecStoredProcedure(string nameProcedure, (string Name, string Value)[] inParametersProcedure, 
            string[] outParametersNamesProcedure, out string[] outputValues,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            long result = 0;
            string[] outputValuesTemp = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"{nameProcedure}");

                    for (int i = 0; i < inParametersProcedure.Length; ++i)
                    {
                        command.Parameters.AddWithValue(inParametersProcedure[i].Name, inParametersProcedure[i].Value);
                        command.Parameters[i].Direction = ParameterDirection.Input;

                        ReplaceDBNullParameterValue(inParametersProcedure[i].Value,
                            ref command, command.Parameters[i].ParameterName);
                        ReplaceFunctionParameterValue(inParametersProcedure[i].Value,
                            ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                    }

                    for (int i = inParametersProcedure.Length; i < inParametersProcedure.Length + outParametersNamesProcedure.Length; ++i)
                    {
                        int arrayIndex = i - inParametersProcedure.Length;

                        command.Parameters.Add(outParametersNamesProcedure[arrayIndex]);
                        command.Parameters[i].Direction = ParameterDirection.Output;
                    }

                    command.Parameters.Add("@r_cmd_return_value", MySqlDbType.Int64);
                    command.Parameters["@r_cmd_return_value"].Direction = ParameterDirection.ReturnValue;

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction, cancellationToken.Token);

                    outputValuesTemp = new string[outParametersNamesProcedure.Length];
                    for (int i = inParametersProcedure.Length; i < inParametersProcedure.Length + outParametersNamesProcedure.Length; ++i)
                    {
                        int arrayIndex = i - inParametersProcedure.Length;

                        outputValuesTemp[arrayIndex] = command.Parameters[i].Value.ToString();
                    }

                    result = (long)command.Parameters["@r_cmd_return_value"].Value;
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{timeout}]");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            outputValues = outputValuesTemp;
            return result;
        }


        /// <summary>
        ///     Вызывает выполнение хранимой процедуры с названием <paramref name="nameProcedure"/> без получения результата, входными параметрами <paramref name="inParametersProcedure"/> и выходными параметрами <paramref name="outParametersNamesProcedure"/>.
        /// </summary>
        /// <param name="nameProcedure">
        ///     Название процедуры.
        /// </param>
        /// <param name="inParametersProcedure">
        ///     Входные параметры процедуры.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="outParametersNamesProcedure">
        ///     Массив названий выходных параметров процедуры.
        /// </param>
        /// <param name="outputValues">
        ///     Массив значений выходных параметров процедуры, доступных после её вызова.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void ExecStoredProcedureNotReturn(string nameProcedure, (string Name, string Value)[] inParametersProcedure,
            string[] outParametersNamesProcedure, out string[] outputValues,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] outputValuesTemp = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"{nameProcedure}");

                    for (int i = 0; i < inParametersProcedure.Length; ++i)
                    {
                        command.Parameters.AddWithValue(inParametersProcedure[i].Name, inParametersProcedure[i].Value);
                        command.Parameters[i].Direction = ParameterDirection.Input;

                        ReplaceDBNullParameterValue(inParametersProcedure[i].Value,
                            ref command, command.Parameters[i].ParameterName);
                        ReplaceFunctionParameterValue(inParametersProcedure[i].Value,
                            ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                    }

                    for (int i = inParametersProcedure.Length; i < inParametersProcedure.Length + outParametersNamesProcedure.Length; ++i)
                    {
                        int arrayIndex = i - inParametersProcedure.Length;

                        command.Parameters.Add(outParametersNamesProcedure[arrayIndex]);
                        command.Parameters[i].Direction = ParameterDirection.Output;
                    }

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction, cancellationToken.Token);

                    outputValuesTemp = new string[outParametersNamesProcedure.Length];
                    for (int i = inParametersProcedure.Length; i < inParametersProcedure.Length + outParametersNamesProcedure.Length; ++i)
                    {
                        int arrayIndex = i - inParametersProcedure.Length;

                        outputValuesTemp[arrayIndex] = command.Parameters[i].Value.ToString();
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{CommandTimeout}]");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            outputValues = outputValuesTemp;
        }
        /// <summary>
        ///     Вызывает выполнение хранимой процедуры с названием <paramref name="nameProcedure"/> без получения результата, входными параметрами <paramref name="inParametersProcedure"/>, выходными параметрами <paramref name="outParametersNamesProcedure"/> и временем ожидания <paramref name="timeout"/>.
        /// </summary>
        /// <param name="nameProcedure">
        ///     Название процедуры.
        /// </param>
        /// <param name="inParametersProcedure">
        ///     Входные параметры процедуры.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="outParametersNamesProcedure">
        ///     Массив названий выходных параметров процедуры.
        /// </param>
        /// <param name="outputValues">
        ///     Массив значений выходных параметров процедуры, доступных после её вызова.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void ExecStoredProcedureNotReturn(string nameProcedure, (string Name, string Value)[] inParametersProcedure,
            string[] outParametersNamesProcedure, out string[] outputValues,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] outputValuesTemp = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"{nameProcedure}");

                    for (int i = 0; i < inParametersProcedure.Length; ++i)
                    {
                        command.Parameters.AddWithValue(inParametersProcedure[i].Name, inParametersProcedure[i].Value);
                        command.Parameters[i].Direction = ParameterDirection.Input;

                        ReplaceDBNullParameterValue(inParametersProcedure[i].Value,
                            ref command, command.Parameters[i].ParameterName);
                        ReplaceFunctionParameterValue(inParametersProcedure[i].Value,
                            ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                    }

                    for (int i = inParametersProcedure.Length; i < inParametersProcedure.Length + outParametersNamesProcedure.Length; ++i)
                    {
                        int arrayIndex = i - inParametersProcedure.Length;

                        command.Parameters.Add(outParametersNamesProcedure[arrayIndex]);
                        command.Parameters[i].Direction = ParameterDirection.Output;
                    }

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction, cancellationToken.Token);

                    outputValuesTemp = new string[outParametersNamesProcedure.Length];
                    for (int i = inParametersProcedure.Length; i < inParametersProcedure.Length + outParametersNamesProcedure.Length; ++i)
                    {
                        int arrayIndex = i - inParametersProcedure.Length;

                        outputValuesTemp[arrayIndex] = command.Parameters[i].Value.ToString();
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{timeout}]");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            outputValues = outputValuesTemp;
        }



        /// <summary>
        ///     Вызывает выполнение команды с текстом <paramref name="commandText"/>.
        /// </summary>
        /// <param name="commandText">
        ///     Текст команды.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="DataSet"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public DataSet CustomCommand(string commandText,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            DataSet result = new DataSet();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = commandText;
                    adapter = new MySqlDataAdapter(sql, connection);
                    adapter.SelectCommand.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает выполнение команды с текстом <paramref name="commandText"/> и временем ожидания <paramref name="timeout"/>.
        /// </summary>
        /// <param name="commandText">
        ///     Текст команды.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="DataSet"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public DataSet CustomCommand(string commandText, TimeSpan timeout, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            DataSet result = new DataSet();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = commandText;
                    adapter = new MySqlDataAdapter(sql, connection);
                    adapter.SelectCommand.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }

        /// <summary>
        ///     Вызывает выполнение команды с текстом <paramref name="commandText"/> и параметрами <paramref name="parameters"/>.
        /// </summary>
        /// <param name="commandText">
        ///     Текст команды.
        /// </param>
        /// <param name="parameters">
        ///     Параметры команды.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="DataSet"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public DataSet CustomCommand(string commandText, (string Name, string Value)[] parameters,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            DataSet result = new DataSet();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = commandText;
                    adapter = new MySqlDataAdapter(sql, connection);
                    adapter.SelectCommand.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        adapter.SelectCommand.Parameters.AddWithValue(parameters[i].Name, parameters[i].Value);

                        ReplaceDBNullParameterValue(parameters[i].Value,
                            ref adapter, adapter.SelectCommand.Parameters[i].ParameterName);
                        ReplaceFunctionParameterValue(parameters[i].Value,
                            ref adapter, adapter.SelectCommand.Parameters[i].ParameterName, ref sql);
                    }

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает выполнение команды с текстом <paramref name="commandText"/>, параметрами <paramref name="parameters"/> и временем ожидания <paramref name="timeout"/>.
        /// </summary>
        /// <param name="commandText">
        ///     Текст команды.
        /// </param>
        /// <param name="parameters">
        ///     Параметры команды.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="DataSet"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public DataSet CustomCommand(string commandText, (string Name, string Value)[] parameters, 
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            DataSet result = new DataSet();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = commandText;
                    adapter = new MySqlDataAdapter(sql, connection);
                    adapter.SelectCommand.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        adapter.SelectCommand.Parameters.AddWithValue(parameters[i].Name, parameters[i].Value);

                        ReplaceDBNullParameterValue(parameters[i].Value,
                            ref adapter, adapter.SelectCommand.Parameters[i].ParameterName);
                        ReplaceFunctionParameterValue(parameters[i].Value,
                            ref adapter, adapter.SelectCommand.Parameters[i].ParameterName, ref sql);
                    }

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }


        /// <summary>
        ///     Вызывает выполнение без получения результата команды с текстом <paramref name="commandText"/>.
        /// </summary>
        /// <param name="commandText">
        ///     Текст команды.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void CustomCommandNonQuery(string commandText,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = commandText;
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }

            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);
        }
        /// <summary>
        ///     Вызывает выполнение без получения результата команды с текстом <paramref name="commandText"/> и временем ожидания <paramref name="timeout"/>.
        /// </summary>
        /// <param name="commandText">
        ///     Текст команды.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void CustomCommandNonQuery(string commandText, TimeSpan timeout,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = commandText;
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }

            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);
        }

        /// <summary>
        ///     Вызывает выполнение без получения результата команды с текстом <paramref name="commandText"/> и параметрами <paramref name="parameters"/>.
        /// </summary>
        /// <param name="commandText">
        ///     Текст команды.
        /// </param>
        /// <param name="parameters">
        ///     Параметры команды.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void CustomCommandNonQuery(string commandText, (string Name, string Value)[] parameters,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = commandText;
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        command.Parameters.AddWithValue(parameters[i].Name, parameters[i].Value);

                        ReplaceDBNullParameterValue(parameters[i].Value,
                            ref command, command.Parameters[i].ParameterName);
                        ReplaceFunctionParameterValue(parameters[i].Value,
                            ref command, command.Parameters[i].ParameterName, ref sql);
                    }

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);
        }
        /// <summary>
        ///     Вызывает выполнение без получения результата команды с текстом <paramref name="commandText"/>, параметрами <paramref name="parameters"/> и временем ожидания <paramref name="timeout"/>.
        /// </summary>
        /// <param name="commandText">
        ///     Текст команды.
        /// </param>
        /// <param name="parameters">
        ///     Параметры команды.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void CustomCommandNonQuery(string commandText, (string Name, string Value)[] parameters,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = commandText;
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        command.Parameters.AddWithValue(parameters[i].Name, parameters[i].Value);

                        ReplaceDBNullParameterValue(parameters[i].Value,
                            ref command, command.Parameters[i].ParameterName);
                        ReplaceFunctionParameterValue(parameters[i].Value,
                            ref command, command.Parameters[i].ParameterName, ref sql);
                    }

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);
        }



        /// <summary>
        ///     Вызывает SELECT для получения результата функции.
        /// </summary>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Параметры функции. Передайте пустой массив для вызова функции без параметров.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string SelectFunction(string nameFunction, string[] parametersValuesFunction, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string result = string.Empty;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"SELECT {nameFunction}(");

                    if (parametersValuesFunction.Length != 0)
                    {
                        sqlBuilder.Append("@param0");
                        command.Parameters.AddWithValue("@param0", parametersValuesFunction[0]);

                        ReplaceDBNullParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[0].ParameterName);
                        ReplaceFunctionParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[0].ParameterName, ref sqlBuilder);

                        for (int i = 1; i < parametersValuesFunction.Length; ++i)
                        {
                            sqlBuilder.Append($", @param{i.ToString()}");
                            command.Parameters.AddWithValue($"@param{i.ToString()}", parametersValuesFunction[i]);

                            ReplaceDBNullParameterValue(parametersValuesFunction[i],
                                ref command, command.Parameters[i].ParameterName);
                            ReplaceFunctionParameterValue(parametersValuesFunction[i],
                                ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                        }
                    }

                    sqlBuilder.Append(")");

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token)[0];
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{CommandTimeout}]");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает SELECT для получения результата функции с временем ожидания <paramref name="timeout"/>.
        /// </summary>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Параметры функции. Передайте пустой массив для вызова функции без параметров.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string SelectFunction(string nameFunction, string[] parametersValuesFunction, 
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string result = string.Empty;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"SELECT {nameFunction}(");

                    if (parametersValuesFunction.Length != 0)
                    {
                        sqlBuilder.Append("@param0");
                        command.Parameters.AddWithValue("@param0", parametersValuesFunction[0]);

                        ReplaceDBNullParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[0].ParameterName);
                        ReplaceFunctionParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[0].ParameterName, ref sqlBuilder);

                        for (int i = 1; i < parametersValuesFunction.Length; ++i)
                        {
                            sqlBuilder.Append($", @param{i.ToString()}");
                            command.Parameters.AddWithValue($"@param{i.ToString()}", parametersValuesFunction[i]);

                            ReplaceDBNullParameterValue(parametersValuesFunction[i],
                                ref command, command.Parameters[i].ParameterName);
                            ReplaceFunctionParameterValue(parametersValuesFunction[i],
                                ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                        }
                    }

                    sqlBuilder.Append(")");

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token)[0];
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{timeout}]");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }

        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="tableFunction"/> для получения результата функции.
        /// </summary>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Параметры функции. Передайте пустой массив для вызова функции без параметров.
        /// </param>
        /// <param name="tableFunction">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string SelectFunction(string nameFunction, string[] parametersValuesFunction, string tableFunction, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string result = string.Empty;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"SELECT {nameFunction}(");

                    if (parametersValuesFunction.Length != 0)
                    {
                        sqlBuilder.Append("@param0");
                        command.Parameters.AddWithValue("@param0", parametersValuesFunction[0]);

                        ReplaceDBNullParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[0].ParameterName);
                        ReplaceFunctionParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[0].ParameterName, ref sqlBuilder);

                        for (int i = 1; i < parametersValuesFunction.Length; ++i)
                        {
                            sqlBuilder.Append($", @param{i.ToString()}");
                            command.Parameters.AddWithValue($"@param{i.ToString()}", parametersValuesFunction[i]);

                            ReplaceDBNullParameterValue(parametersValuesFunction[i],
                                ref command, command.Parameters[i].ParameterName);
                            ReplaceFunctionParameterValue(parametersValuesFunction[i],
                                ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                        }
                    }

                    sqlBuilder.Append($") FROM {tableFunction}");

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token)[0];
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{CommandTimeout}]");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="tableFunction"/> для получения результата функции с временем ожидания <paramref name="timeout"/>.
        /// </summary>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Параметры функции. Передайте пустой массив для вызова функции без параметров.
        /// </param>
        /// <param name="tableFunction">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string SelectFunction(string nameFunction, string[] parametersValuesFunction, string tableFunction, 
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string result = string.Empty;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"SELECT {nameFunction}(");

                    if (parametersValuesFunction.Length != 0)
                    {
                        sqlBuilder.Append("@param0");
                        command.Parameters.AddWithValue("@param0", parametersValuesFunction[0]);

                        ReplaceDBNullParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[0].ParameterName);
                        ReplaceFunctionParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[0].ParameterName, ref sqlBuilder);

                        for (int i = 1; i < parametersValuesFunction.Length; ++i)
                        {
                            sqlBuilder.Append($", @param{i.ToString()}");
                            command.Parameters.AddWithValue($"@param{i.ToString()}", parametersValuesFunction[i]);

                            ReplaceDBNullParameterValue(parametersValuesFunction[i],
                                ref command, command.Parameters[i].ParameterName);
                            ReplaceFunctionParameterValue(parametersValuesFunction[i],
                                ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                        }
                    }

                    sqlBuilder.Append($") FROM {tableFunction}");

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token)[0];
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{timeout}]");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }



        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> для получения значений столбца <paramref name="column"/> во всех строках.
        /// </summary>
        /// <param name="column">
        ///     Название столбца, значения которого надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Укажите 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Укажите 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[] SelectColumn(string column, string table, ulong numberStartRow, ulong countRows,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    DataSet data = new DataSet();

                    sql = $"SELECT {column} FROM {table}";

                    if (numberStartRow > 0)
                        numberStartRow -= 1;
                    if (countRows == 0)
                        countRows = ulong.MaxValue;

                    sql += $" LIMIT {numberStartRow.ToString()},{countRows.ToString()}";

                    adapter = new MySqlDataAdapter(sql, connection);
                    adapter.SelectCommand.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    data = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);

                    result = new string[data.Tables[0].Rows.Count];
                    for (int i = 0; i < data.Tables[0].Rows.Count; ++i)
                    {
                        result[i] = data.Tables[0].Rows[i][column].ToString();
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> для получения значений столбца <paramref name="column"/> во всех строках с временем ожидания <paramref name="timeout"/>.
        /// </summary>
        /// <param name="column">
        ///     Название столбца, значения которого надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Укажите 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Укажите 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[] SelectColumn(string column, string table, ulong numberStartRow, ulong countRows,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    DataSet data = new DataSet();

                    sql = $"SELECT {column} FROM {table}";

                    if (numberStartRow > 0)
                        numberStartRow -= 1;
                    if (countRows == 0)
                        countRows = ulong.MaxValue;

                    sql += $" LIMIT {numberStartRow.ToString()},{countRows.ToString()}";

                    adapter = new MySqlDataAdapter(sql, connection);
                    adapter.SelectCommand.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    data = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);

                    result = new string[data.Tables[0].Rows.Count];
                    for (int i = 0; i < data.Tables[0].Rows.Count; ++i)
                    {
                        result[i] = data.Tables[0].Rows[i][column].ToString();
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }

        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> для получения значений столбца <paramref name="column"/> во всех строках с условием выборки: столбец <paramref name="conditionName"/> = <paramref name="conditionValue"/>.
        /// </summary>
        /// <param name="column">
        ///     Название столбца, значения которого надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditionName">
        ///     Название столбца для условия.
        /// </param>
        /// <param name="conditionValue">
        ///     Значение столбца для условия.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Укажите 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Укажите 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[] SelectColumn(string column, string table, string conditionName, string conditionValue,
            ulong numberStartRow, ulong countRows, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    DataSet data = new DataSet();

                    sql = $"SELECT {column} FROM {table} WHERE {conditionName} = @param0";

                    if (numberStartRow > 0)
                        numberStartRow -= 1;
                    if (countRows == 0)
                        countRows = ulong.MaxValue;

                    sql += $" LIMIT {numberStartRow.ToString()},{countRows.ToString()}";

                    adapter = new MySqlDataAdapter(sql, connection);
                    adapter.SelectCommand.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    adapter.SelectCommand.Parameters.AddWithValue("@param0", conditionValue);

                    ReplaceDBNullParameterValue(conditionValue, ref adapter, 
                        adapter.SelectCommand.Parameters[0].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    data = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);

                    result = new string[data.Tables[0].Rows.Count];
                    for (int i = 0; i < data.Tables[0].Rows.Count; ++i)
                    {
                        result[i] = data.Tables[0].Rows[i][column].ToString();
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> для получения значений столбца <paramref name="column"/> во всех строках с временем ожидания <paramref name="timeout"/> и условием выборки: столбец <paramref name="conditionName"/> = <paramref name="conditionValue"/>.
        /// </summary>
        /// <param name="column">
        ///     Название столбца, значения которого надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditionName">
        ///     Название столбца для условия.
        /// </param>
        /// <param name="conditionValue">
        ///     Значение столбца для условия.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Укажите 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Укажите 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[] SelectColumn(string column, string table, string conditionName, string conditionValue,
            ulong numberStartRow, ulong countRows, TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    DataSet data = new DataSet();

                    sql = $"SELECT {column} FROM {table} WHERE {conditionName} = @param0";

                    if (numberStartRow > 0)
                        numberStartRow -= 1;
                    if (countRows == 0)
                        countRows = ulong.MaxValue;

                    sql += $" LIMIT {numberStartRow.ToString()},{countRows.ToString()}";

                    adapter = new MySqlDataAdapter(sql, connection);
                    adapter.SelectCommand.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    adapter.SelectCommand.Parameters.AddWithValue("@param0", conditionValue);

                    ReplaceDBNullParameterValue(conditionValue, ref adapter,
                        adapter.SelectCommand.Parameters[0].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    data = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);

                    result = new string[data.Tables[0].Rows.Count];
                    for (int i = 0; i < data.Tables[0].Rows.Count; ++i)
                    {
                        result[i] = data.Tables[0].Rows[i][column].ToString();
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }

        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> для получения значений столбца <paramref name="column"/> во всех строках с условием выборки: столбец <paramref name="conditionName1"/> = <paramref name="conditionValue1"/> AND столбец <paramref name="conditionName2"/> = <paramref name="conditionValue2"/>.
        /// </summary>
        /// <param name="column">
        ///     Название столбца, значения которого надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditionName1">
        ///     Название столбца для условия #1.
        /// </param>
        /// <param name="conditionValue1">
        ///     Значение столбца для условия #1.
        /// </param>
        /// <param name="conditionName2">
        ///     Название столбца для условия #2.
        /// </param>
        /// <param name="conditionValue2">
        ///     Значение столбца для условия #2.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Укажите 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Укажите 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[] SelectColumn(string column, string table, string conditionName1,
            string conditionValue1, string conditionName2, string conditionValue2, ulong numberStartRow, 
            ulong countRows, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    DataSet data = new DataSet();

                    sql = $"SELECT {column} FROM {table} WHERE {conditionName1} = @param0 " +
                      $"AND {conditionName2} = @param1";

                    if (numberStartRow > 0)
                        numberStartRow -= 1;
                    if (countRows == 0)
                        countRows = ulong.MaxValue;

                    sql += $" LIMIT {numberStartRow.ToString()},{countRows.ToString()}";

                    adapter = new MySqlDataAdapter(sql, connection);
                    adapter.SelectCommand.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    adapter.SelectCommand.Parameters.AddWithValue("@param0", conditionValue1);
                    adapter.SelectCommand.Parameters.AddWithValue("@param1", conditionValue2);

                    ReplaceDBNullParameterValue(conditionValue1, ref adapter,
                        adapter.SelectCommand.Parameters[0].ParameterName);

                    ReplaceDBNullParameterValue(conditionValue2, ref adapter,
                        adapter.SelectCommand.Parameters[1].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    data = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);

                    result = new string[data.Tables[0].Rows.Count];
                    for (int i = 0; i < data.Tables[0].Rows.Count; ++i)
                    {
                        result[i] = data.Tables[0].Rows[i][column].ToString();
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> для получения значений столбца <paramref name="column"/> во всех строках с временем ожидания <paramref name="timeout"/> и условием выборки: столбец <paramref name="conditionName1"/> = <paramref name="conditionValue1"/> AND столбец <paramref name="conditionName2"/> = <paramref name="conditionValue2"/>.
        /// </summary>
        /// <param name="column">
        ///     Название столбца, значения которого надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditionName1">
        ///     Название столбца для условия #1.
        /// </param>
        /// <param name="conditionValue1">
        ///     Значение столбца для условия #1.
        /// </param>
        /// <param name="conditionName2">
        ///     Название столбца для условия #2.
        /// </param>
        /// <param name="conditionValue2">
        ///     Значение столбца для условия #2.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Укажите 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Укажите 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[] SelectColumn(string column, string table, string conditionName1,
            string conditionValue1, string conditionName2, string conditionValue2, ulong numberStartRow, 
            ulong countRows, TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    DataSet data = new DataSet();

                    sql = $"SELECT {column} FROM {table} WHERE {conditionName1} = @param0 " +
                      $"AND {conditionName2} = @param1";

                    if (numberStartRow > 0)
                        numberStartRow -= 1;
                    if (countRows == 0)
                        countRows = ulong.MaxValue;
                    
                    sql += $" LIMIT {numberStartRow.ToString()},{countRows.ToString()}";

                    adapter = new MySqlDataAdapter(sql, connection);
                    adapter.SelectCommand.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    adapter.SelectCommand.Parameters.AddWithValue("@param0", conditionValue1);
                    adapter.SelectCommand.Parameters.AddWithValue("@param1", conditionValue2);

                    ReplaceDBNullParameterValue(conditionValue1, ref adapter,
                        adapter.SelectCommand.Parameters[0].ParameterName);

                    ReplaceDBNullParameterValue(conditionValue2, ref adapter,
                        adapter.SelectCommand.Parameters[1].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    data = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);

                    result = new string[data.Tables[0].Rows.Count];
                    for (int i = 0; i < data.Tables[0].Rows.Count; ++i)
                    {
                        result[i] = data.Tables[0].Rows[i][column].ToString();
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }

        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> для получения значений столбца <paramref name="column"/> во всех строках, соответствующих условиям из массива <paramref name="conditions"/>: каждый столбец с именем <paramref name="conditions"/>.Name равен соответствующему значению <paramref name="conditions"/>.Value.
        /// </summary>
        /// <param name="column">
        ///     Название столбца, значения которого надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса.
        ///     Name - название столбца.
        ///     Value - значение столбца.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Укажите 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Укажите 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[] SelectColumn(string column, string table, (string Name, string Value)[] conditions,
            ulong numberStartRow, ulong countRows, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (conditions.Length == 0)
            {
                var exception = new ArgumentException("Count of conditions is 0");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    DataSet data = new DataSet();

                    adapter = new MySqlDataAdapter(sqlBuilder.ToString(), connection);
                    adapter.SelectCommand.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"SELECT {column} FROM {table} WHERE ");

                    sqlBuilder.Append($"{conditions[0].Name} = @param0");
                    adapter.SelectCommand.Parameters.AddWithValue("@param0", conditions[0].Value);

                    ReplaceDBNullParameterValue(conditions[0].Value, ref adapter,
                        adapter.SelectCommand.Parameters[0].ParameterName);

                    for (int i = 1; i < conditions.Length; ++i)
                    {
                        sqlBuilder.Append($" AND {conditions[i].Name} = @param{i.ToString()}");
                        adapter.SelectCommand.Parameters.AddWithValue($"@param{i.ToString()}", conditions[i].Value);

                        ReplaceDBNullParameterValue(conditions[i].Value, ref adapter,
                            adapter.SelectCommand.Parameters[i].ParameterName);
                    }

                    if (numberStartRow > 0)
                        numberStartRow -= 1;
                    if (countRows == 0)
                        countRows = ulong.MaxValue;
                    
                    sqlBuilder.Append($" LIMIT {numberStartRow.ToString()},{countRows.ToString()}");

                    adapter.SelectCommand.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    data = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);

                    result = new string[data.Tables[0].Rows.Count];
                    for (int i = 0; i < data.Tables[0].Rows.Count; ++i)
                    {
                        result[i] = data.Tables[0].Rows[i][column].ToString();
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{CommandTimeout}]");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> для получения значений столбца <paramref name="column"/> во всех строках с временем ожидания <paramref name="timeout"/>, соответствующих условиям из массива <paramref name="conditions"/>: каждый столбец с именем <paramref name="conditions"/>.Name равен соответствующему значению <paramref name="conditions"/>.Value.
        /// </summary>
        /// <param name="column">
        ///     Название столбца, значения которого надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса.
        ///     Name - название столбца.
        ///     Value - значение столбца.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Укажите 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Укажите 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[] SelectColumn(string column, string table, (string Name, string Value)[] conditions,
            ulong numberStartRow, ulong countRows, TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (conditions.Length == 0)
            {
                var exception = new ArgumentException("Count of conditions is 0");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    DataSet data = new DataSet();

                    adapter = new MySqlDataAdapter(sqlBuilder.ToString(), connection);
                    adapter.SelectCommand.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"SELECT {column} FROM {table} WHERE ");

                    sqlBuilder.Append($"{conditions[0].Name} = @param0");
                    adapter.SelectCommand.Parameters.AddWithValue("@param0", conditions[0].Value);

                    ReplaceDBNullParameterValue(conditions[0].Value, ref adapter,
                        adapter.SelectCommand.Parameters[0].ParameterName);

                    for (int i = 1; i < conditions.Length; ++i)
                    {
                        sqlBuilder.Append($" AND {conditions[i].Name} = @param{i.ToString()}");
                        adapter.SelectCommand.Parameters.AddWithValue($"@param{i.ToString()}", conditions[i].Value);

                        ReplaceDBNullParameterValue(conditions[i].Value, ref adapter,
                            adapter.SelectCommand.Parameters[i].ParameterName);
                    }

                    if (numberStartRow > 0)
                        numberStartRow -= 1;
                    if (countRows == 0)
                        countRows = ulong.MaxValue;

                    sqlBuilder.Append($" LIMIT {numberStartRow.ToString()},{countRows.ToString()}");

                    adapter.SelectCommand.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    data = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);

                    result = new string[data.Tables[0].Rows.Count];
                    for (int i = 0; i < data.Tables[0].Rows.Count; ++i)
                    {
                        result[i] = data.Tables[0].Rows[i][column].ToString();
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{timeout}]");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }



        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="tableColumns"/> для получения значений столбцов <paramref name="columns"/> во всех строках.
        /// </summary>
        /// <param name="columns">
        ///     Массив названий столбцов, значения которых надо получить.
        /// </param>
        /// <param name="tableColumns">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Укажите 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Укажите 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив массивов типа <see cref="string"/>, которые содержат ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[][] SelectColumns(string[] columns, string tableColumns, ulong numberStartRow, 
            ulong countRows, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[][] result = Array.Empty<string[]>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    DataSet data = new DataSet();

                    sql = $"SELECT {string.Join(", ", columns)} FROM {tableColumns}";

                    if (numberStartRow > 0)
                        numberStartRow -= 1;
                    if (countRows == 0)
                        countRows = ulong.MaxValue;

                    sql += $" LIMIT {numberStartRow.ToString()},{countRows.ToString()}";

                    adapter = new MySqlDataAdapter(sql, connection);
                    adapter.SelectCommand.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    data = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);

                    result = new string[data.Tables[0].Columns.Count][];
                    for (int i = 0; i < data.Tables[0].Columns.Count; ++i)
                    {
                        result[i] = new string[data.Tables[0].Rows.Count];
                        for (int j = 0; j < data.Tables[0].Rows.Count; ++j)
                        {
                            result[i][j] = data.Tables[0].Rows[j][columns[i]].ToString();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="tableColumns"/> для получения значений столбцов <paramref name="columns"/> во всех строках с временем ожидания <paramref name="timeout"/>.
        /// </summary>
        /// <param name="columns">
        ///     Массив названий столбцов, значения которых надо получить.
        /// </param>
        /// <param name="tableColumns">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Укажите 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Укажите 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив массивов типа <see cref="string"/>, которые содержат ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[][] SelectColumns(string[] columns, string tableColumns, ulong numberStartRow, 
            ulong countRows, TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[][] result = Array.Empty<string[]>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    DataSet data = new DataSet();

                    sql = $"SELECT {string.Join(", ", columns)} FROM {tableColumns}";

                    if (numberStartRow > 0)
                        numberStartRow -= 1;
                    if (countRows == 0)
                        countRows = ulong.MaxValue;

                    sql += $" LIMIT {numberStartRow.ToString()},{countRows.ToString()}";

                    adapter = new MySqlDataAdapter(sql, connection);
                    adapter.SelectCommand.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    data = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);

                    result = new string[data.Tables[0].Columns.Count][];
                    for (int i = 0; i < data.Tables[0].Columns.Count; ++i)
                    {
                        result[i] = new string[data.Tables[0].Rows.Count];
                        for (int j = 0; j < data.Tables[0].Rows.Count; ++j)
                        {
                            result[i][j] = data.Tables[0].Rows[j][columns[i]].ToString();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }

        /// <summary>
        ///     Вызывает SELECT для получения значений столбцов <paramref name="columns"/> во всех строках.
        /// </summary>
        /// <param name="columns">
        ///     Массив столбцов, значения которых надо получить.
        ///     Name - название столбца.
        ///     Table - таблица, для которой будет вызван SELECT.
        ///     NumberStartRow - номер строки, с которой начнётся считывание (все строки до этой будут пропущены). Укажите 0 или 1, чтобы начать с первой строки.
        ///     CountRows - Количество строк, которые будут считаны. Укажите 0, чтобы считать все строки, начиная с начальной строки <paramref name="columns"/>.NumberStartRow.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив массивов типа <see cref="string"/>, которые содержат ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[][] SelectColumns((string Name, string Table, ulong NumberStartRow, ulong CountRows)[] columns, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[][] result = Array.Empty<string[]>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (columns.Length == 0)
            {
                var exception = new ArgumentException("Count of columns is 0");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    DataSet data = new DataSet();

                    adapter = new MySqlDataAdapter(sqlBuilder.ToString(), connection);
                    adapter.SelectCommand.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    for (int i = 0; i < columns.Length; ++i)
                    {
                        sqlBuilder.Append($"SELECT {columns[i].Name} FROM {columns[i].Table}");

                        if (columns[i].NumberStartRow > 0)
                            columns[i].NumberStartRow -= 1;
                        if (columns[i].CountRows == 0)
                            columns[i].CountRows = ulong.MaxValue;

                        sqlBuilder.Append($" LIMIT {columns[i].NumberStartRow.ToString()},{columns[i].CountRows.ToString()}; ");
                    }

                    adapter.SelectCommand.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    data = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);

                    result = new string[data.Tables.Count][];
                    for (int i = 0; i < data.Tables.Count; ++i)
                    {
                        result[i] = new string[data.Tables[i].Rows.Count];
                        for (int j = 0; j < data.Tables[i].Rows.Count; ++j)
                        {
                            result[i][j] = data.Tables[i].Rows[j][columns[i].Name].ToString();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{CommandTimeout}]");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает SELECT с временем ожидания <paramref name="timeout"/> для получения значений столбцов <paramref name="columns"/> во всех строках.
        /// </summary>
        /// <param name="columns">
        ///     Массив столбцов, значения которых надо получить.
        ///     Name - название столбца.
        ///     Table - таблица, для которой будет вызван SELECT.
        ///     NumberStartRow - номер строки, с которой начнётся считывание (все строки до этой будут пропущены). Укажите 0 или 1, чтобы начать с первой строки.
        ///     CountRows - Количество строк, которые будут считаны. Укажите 0, чтобы считать все строки, начиная с начальной строки <paramref name="columns"/>.NumberStartRow.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив массивов типа <see cref="string"/>, которые содержат ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[][] SelectColumns((string Name, string Table, ulong NumberStartRow, ulong CountRows)[] columns, 
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[][] result = Array.Empty<string[]>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (columns.Length == 0)
            {
                var exception = new ArgumentException("Count of columns is 0");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlDataAdapter adapter;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    DataSet data = new DataSet();

                    adapter = new MySqlDataAdapter(sqlBuilder.ToString(), connection);
                    adapter.SelectCommand.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    for (int i = 0; i < columns.Length; ++i)
                    {
                        sqlBuilder.Append($"SELECT {columns[i].Name} FROM {columns[i].Table}");

                        if (columns[i].NumberStartRow > 0)
                            columns[i].NumberStartRow -= 1;
                        if (columns[i].CountRows == 0)
                            columns[i].CountRows = ulong.MaxValue;

                        sqlBuilder.Append($" LIMIT {columns[i].NumberStartRow.ToString()},{columns[i].CountRows.ToString()}; ");
                    }

                    adapter.SelectCommand.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    data = CommandExecuteAdapter(connectionIndex, isolationLevel, ref adapter, ref transaction,
                        cancellationToken.Token);

                    result = new string[data.Tables.Count][];
                    for (int i = 0; i < data.Tables.Count; ++i)
                    {
                        result[i] = new string[data.Tables[i].Rows.Count];
                        for (int j = 0; j < data.Tables[i].Rows.Count; ++j)
                        {
                            result[i][j] = data.Tables[i].Rows[j][columns[i].Name].ToString();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{timeout}]");
                    Events.DShowError?.Invoke(this, 
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this, 
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }



        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> для получения значения столбца <paramref name="searchField"/> в первой найденной строке, соответствующей условию: столбец <paramref name="conditionName"/> = <paramref name="conditionValue"/>.
        /// </summary>
        /// <param name="searchField">
        ///     Названия столбца, значение которого надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditionName">
        ///     Название столбца для условия.
        /// </param>
        /// <param name="conditionValue">
        ///     Значение столбца для условия.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string Select(string searchField, string table, string conditionName,
            string conditionValue, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string result = string.Empty;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"SELECT {searchField} FROM {table} WHERE " + 
                          $"{conditionName} = @param0";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", conditionValue);

                    ReplaceDBNullParameterValue(conditionValue, ref command,
                        command.Parameters[0].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token)[0];
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> с временем ожидания <paramref name="timeout"/> для получения значения столбца <paramref name="searchField"/> в первой найденной строке, соответствующей условию: столбец <paramref name="conditionName"/> = <paramref name="conditionValue"/>.
        /// </summary>
        /// <param name="searchField">
        ///     Названия столбца, значение которого надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditionName">
        ///     Название столбца для условия.
        /// </param>
        /// <param name="conditionValue">
        ///     Значение столбца для условия.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string Select(string searchField, string table, string conditionName,
            string conditionValue, TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string result = string.Empty;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"SELECT {searchField} FROM {table} WHERE " +
                          $"{conditionName} = @param0";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", conditionValue);

                    ReplaceDBNullParameterValue(conditionValue, ref command,
                        command.Parameters[0].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token)[0];
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }

        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> для получения значения столбца <paramref name="searchField"/> в первой найденной строке, соответствующей условиям: столбец <paramref name="conditionName1"/> = <paramref name="conditionValue1"/> AND столбец <paramref name="conditionName2"/> = <paramref name="conditionValue2"/>.
        /// </summary>
        /// <param name="searchField">
        ///     Названия столбца, значение которого надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditionName1">
        ///     Название столбца для условия #1.
        /// </param>
        /// <param name="conditionValue1">
        ///     Значение столбца для условия #1.
        /// </param>
        /// <param name="conditionName2">
        ///     Название столбца для условия #2.
        /// </param>
        /// <param name="conditionValue2">
        ///     Значение столбца для условия #2.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string Select(string searchField, string table, string conditionName1,
            string conditionValue1, string conditionName2, string conditionValue2, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string result = string.Empty;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"SELECT {searchField} FROM {table} WHERE " + 
                          $"{conditionName1} = @param0 " + 
                          $"AND {conditionName2} = @param1";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", conditionValue1);
                    command.Parameters.AddWithValue("@param1", conditionValue2);

                    ReplaceDBNullParameterValue(conditionValue1, ref command,
                        command.Parameters[0].ParameterName);

                    ReplaceDBNullParameterValue(conditionValue2, ref command,
                        command.Parameters[1].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token)[0];
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> с временем ожидания <paramref name="timeout"/> для получения значения столбца <paramref name="searchField"/> в первой найденной строке, соответствующей условиям: столбец <paramref name="conditionName1"/> = <paramref name="conditionValue1"/> AND столбец <paramref name="conditionName2"/> = <paramref name="conditionValue2"/>.
        /// </summary>
        /// <param name="searchField">
        ///     Названия столбца, значение которого надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditionName1">
        ///     Название столбца для условия #1.
        /// </param>
        /// <param name="conditionValue1">
        ///     Значение столбца для условия #1.
        /// </param>
        /// <param name="conditionName2">
        ///     Название столбца для условия #2.
        /// </param>
        /// <param name="conditionValue2">
        ///     Значение столбца для условия #2.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string Select(string searchField, string table, string conditionName1,
            string conditionValue1, string conditionName2, string conditionValue2, 
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string result = string.Empty;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"SELECT {searchField} FROM {table} WHERE " +
                          $"{conditionName1} = @param0 " +
                          $"AND {conditionName2} = @param1";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", conditionValue1);
                    command.Parameters.AddWithValue("@param1", conditionValue2);

                    ReplaceDBNullParameterValue(conditionValue1, ref command,
                        command.Parameters[0].ParameterName);

                    ReplaceDBNullParameterValue(conditionValue2, ref command,
                        command.Parameters[1].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token)[0];
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }

        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> для получения значения столбца <paramref name="searchField"/> в первой найденной строке, соответствующей условиям из массива <paramref name="conditions"/>: каждый столбец с именем <paramref name="conditions"/>.Name равен соответствующему значению <paramref name="conditions"/>.Value.
        /// </summary>
        /// <param name="searchField">
        ///     Название столбца, значение которого надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса.
        ///     Name - название столбца.
        ///     Value - значение столбца.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string Select(string searchField, string table, (string Name, string Value)[] conditions, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string result = string.Empty;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (conditions.Length == 0)
            {
                var exception = new ArgumentException("Count of conditions is 0");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"SELECT {searchField} FROM {table} WHERE ");

                    sqlBuilder.Append($"{conditions[0].Name} = @param0");
                    command.Parameters.AddWithValue("@param0", conditions[0].Value);

                    ReplaceDBNullParameterValue(conditions[0].Value, ref command,
                        command.Parameters[0].ParameterName);

                    for (int i = 1; i < conditions.Length; ++i)
                    {
                        sqlBuilder.Append($" AND {conditions[i].Name} = @param{i.ToString()}");
                        command.Parameters.AddWithValue($"@param{i.ToString()}", conditions[i].Value);

                        ReplaceDBNullParameterValue(conditions[i].Value, ref command,
                            command.Parameters[i].ParameterName);
                    }

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token)[0];
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }

            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> с временем ожидания <paramref name="timeout"/> для получения значения столбца <paramref name="searchField"/> в первой найденной строке, соответствующей условиям из массива <paramref name="conditions"/>: каждый столбец с именем <paramref name="conditions"/>.Name равен соответствующему значению <paramref name="conditions"/>.Value.
        /// </summary>
        /// <param name="searchField">
        ///     Название столбца, значение которого надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса.
        ///     Name - название столбца.
        ///     Value - значение столбца.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string Select(string searchField, string table, (string Name, string Value)[] conditions, 
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string result = string.Empty;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (conditions.Length == 0)
            {
                var exception = new ArgumentException("Count of conditions is 0");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"SELECT {searchField} FROM {table} WHERE ");

                    sqlBuilder.Append($"{conditions[0].Name} = @param0");
                    command.Parameters.AddWithValue("@param0", conditions[0].Value);

                    ReplaceDBNullParameterValue(conditions[0].Value, ref command,
                        command.Parameters[0].ParameterName);

                    for (int i = 1; i < conditions.Length; ++i)
                    {
                        sqlBuilder.Append($" AND {conditions[i].Name} = @param{i.ToString()}");
                        command.Parameters.AddWithValue($"@param{i.ToString()}", conditions[i].Value);

                        ReplaceDBNullParameterValue(conditions[i].Value, ref command,
                            command.Parameters[i].ParameterName);
                    }

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token)[0];
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }

            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }


        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> для получения значений столбцов <paramref name="searchFields"/> в первой найденной строке, соответствующей условию: столбец <paramref name="conditionName"/> = <paramref name="conditionValue"/>.
        /// </summary>
        /// <param name="searchFields">
        ///     Названия столбцов, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditionName">
        ///     Название столбца для условия.
        /// </param>
        /// <param name="conditionValue">
        ///     Значение столбца для условия.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[] Select(string[] searchFields, string table, string conditionName,
            string conditionValue, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"SELECT {string.Join(", ", searchFields)} FROM {table} WHERE " +
                          $"{conditionName} = @param0";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", conditionValue);

                    ReplaceDBNullParameterValue(conditionValue, ref command,
                        command.Parameters[0].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> с временем ожидания <paramref name="timeout"/> для получения значений столбцов <paramref name="searchFields"/> в первой найденной строке, соответствующей условию: столбец <paramref name="conditionName"/> = <paramref name="conditionValue"/>.
        /// </summary>
        /// <param name="searchFields">
        ///     Названия столбцов, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditionName">
        ///     Название столбца для условия.
        /// </param>
        /// <param name="conditionValue">
        ///     Значение столбца для условия.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[] Select(string[] searchFields, string table, string conditionName,
            string conditionValue, TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"SELECT {string.Join(", ", searchFields)} FROM {table} WHERE " +
                          $"{conditionName} = @param0";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", conditionValue);

                    ReplaceDBNullParameterValue(conditionValue, ref command,
                        command.Parameters[0].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }

        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> для получения значений столбцов <paramref name="searchFields"/> в первой найденной строке, соответствующей условиям: столбец <paramref name="conditionName1"/> = <paramref name="conditionValue1"/> AND столбец <paramref name="conditionName2"/> = <paramref name="conditionValue2"/>.
        /// </summary>
        /// <param name="searchFields">
        ///     Названия столбцов, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditionName1">
        ///     Название столбца для условия #1.
        /// </param>
        /// <param name="conditionValue1">
        ///     Значение столбца для условия #1.
        /// </param>
        /// <param name="conditionName2">
        ///     Название столбца для условия #2.
        /// </param>
        /// <param name="conditionValue2">
        ///     Значение столбца для условия #2.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[] Select(string[] searchFields, string table, string conditionName1,
            string conditionValue1, string conditionName2, string conditionValue2,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"SELECT {string.Join(", ", searchFields)} FROM {table} WHERE " +
                          $"{conditionName1} = @param0 " +
                          $"AND {conditionName2} = @param1";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int) CommandTimeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", conditionValue1);
                    command.Parameters.AddWithValue("@param1", conditionValue2);

                    ReplaceDBNullParameterValue(conditionValue1, ref command,
                        command.Parameters[0].ParameterName);

                    ReplaceDBNullParameterValue(conditionValue2, ref command,
                        command.Parameters[1].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> с временем ожидания <paramref name="timeout"/> для получения значений столбцов <paramref name="searchFields"/> в первой найденной строке, соответствующей условиям: столбец <paramref name="conditionName1"/> = <paramref name="conditionValue1"/> AND столбец <paramref name="conditionName2"/> = <paramref name="conditionValue2"/>.
        /// </summary>
        /// <param name="searchFields">
        ///     Названия столбцов, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditionName1">
        ///     Название столбца для условия #1.
        /// </param>
        /// <param name="conditionValue1">
        ///     Значение столбца для условия #1.
        /// </param>
        /// <param name="conditionName2">
        ///     Название столбца для условия #2.
        /// </param>
        /// <param name="conditionValue2">
        ///     Значение столбца для условия #2.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[] Select(string[] searchFields, string table, string conditionName1,
            string conditionValue1, string conditionName2, string conditionValue2,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"SELECT {string.Join(", ", searchFields)} FROM {table} WHERE " +
                          $"{conditionName1} = @param0 " +
                          $"AND {conditionName2} = @param1";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", conditionValue1);
                    command.Parameters.AddWithValue("@param1", conditionValue2);

                    ReplaceDBNullParameterValue(conditionValue1, ref command,
                        command.Parameters[0].ParameterName);

                    ReplaceDBNullParameterValue(conditionValue2, ref command,
                        command.Parameters[1].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }

        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> для получения значений столбцов <paramref name="searchFields"/> в первой найденной строке, соответствующей условиям из массива <paramref name="conditions"/>: каждый столбец с именем <paramref name="conditions"/>.Name равен соответствующему значению <paramref name="conditions"/>.Value.
        /// </summary>
        /// <param name="searchFields">
        ///     Названия столбцов, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса.
        ///     Name - название столбца.
        ///     Value - значение столбца.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[] Select(string[] searchFields, string table, (string Name, string Value)[] conditions,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (conditions.Length == 0)
            {
                var exception = new ArgumentException("Count of conditions is 0");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int) CommandTimeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"SELECT {string.Join(", ", searchFields)} FROM {table} WHERE ");

                    sqlBuilder.Append($"{conditions[0].Name} = @param0");
                    command.Parameters.AddWithValue("@param0", conditions[0].Value);

                    ReplaceDBNullParameterValue(conditions[0].Value, ref command,
                        command.Parameters[0].ParameterName);

                    for (int i = 1; i < conditions.Length; ++i)
                    {
                        sqlBuilder.Append($" AND {conditions[i].Name} = @param{i.ToString()}");
                        command.Parameters.AddWithValue($"@param{i.ToString()}", conditions[i].Value);

                        ReplaceDBNullParameterValue(conditions[i].Value, ref command,
                            command.Parameters[i].ParameterName);
                    }

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает SELECT в таблице <paramref name="table"/> с временем ожидания <paramref name="timeout"/> для получения значений столбцов <paramref name="searchFields"/> в первой найденной строке, соответствующей условиям из массива <paramref name="conditions"/>: каждый столбец с именем <paramref name="conditions"/>.Name равен соответствующему значению <paramref name="conditions"/>.Value.
        /// </summary>
        /// <param name="searchFields">
        ///     Название столбцов, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса.
        ///     Name - название столбца.
        ///     Value - значение столбца.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string[] Select(string[] searchFields, string table, (string Name, string Value)[] conditions, 
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (conditions.Length == 0)
            {
                var exception = new ArgumentException("Count of conditions is 0");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"SELECT {string.Join(", ", searchFields)} FROM {table} WHERE ");

                    sqlBuilder.Append($"{conditions[0].Name} = @param0");
                    command.Parameters.AddWithValue("@param0", conditions[0].Value);

                    ReplaceDBNullParameterValue(conditions[0].Value, ref command,
                        command.Parameters[0].ParameterName);

                    for (int i = 1; i < conditions.Length; ++i)
                    {
                        sqlBuilder.Append($" AND {conditions[i].Name} = @param{i.ToString()}");
                        command.Parameters.AddWithValue($"@param{i.ToString()}", conditions[i].Value);

                        ReplaceDBNullParameterValue(conditions[i].Value, ref command,
                            command.Parameters[i].ParameterName);
                    }

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }



        /// <summary>
        ///     Вызывает DELETE в таблице <paramref name="table"/> для удаления всех строк.
        /// </summary>
        /// <param name="table">
        ///     Таблица, для которой будет вызван DELETE.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Delete(string table, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"DELETE FROM {table}";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);
        }
        /// <summary>
        ///     Вызывает DELETE в таблице <paramref name="table"/> с временем ожидания <paramref name="timeout"/> для удаления всех строк.
        /// </summary>
        /// <param name="table">
        ///     Таблица, для которой будет вызван DELETE.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Delete(string table, TimeSpan timeout, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"DELETE FROM {table}";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);
        }

        /// <summary>
        ///     Вызывает DELETE в таблице <paramref name="table"/> для удаления всех строк, соответствующих условию: столбец <paramref name="conditionName"/> = <paramref name="conditionValue"/>.
        /// </summary>
        /// <param name="table">
        ///     Таблица, для которой будет вызван DELETE.
        /// </param>
        /// <param name="conditionName">
        ///     Название столбца для условия.
        /// </param>
        /// <param name="conditionValue">
        ///     Значение столбца для условия.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Delete(string table, string conditionName, string conditionValue, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"DELETE FROM {table} WHERE {conditionName} = @param0";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", conditionValue);

                    ReplaceDBNullParameterValue(conditionValue, ref command,
                        command.Parameters[0].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);
        }
        /// <summary>
        ///     Вызывает DELETE в таблице <paramref name="table"/> с временем ожидания <paramref name="timeout"/> для удаления всех строк, соответствующих условию: столбец <paramref name="conditionName"/> = <paramref name="conditionValue"/>.
        /// </summary>
        /// <param name="table">
        ///     Таблица, для которой будет вызван DELETE.
        /// </param>
        /// <param name="conditionName">
        ///     Название столбца для условия.
        /// </param>
        /// <param name="conditionValue">
        ///     Значение столбца для условия.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Delete(string table, string conditionName, string conditionValue, 
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"DELETE FROM {table} WHERE {conditionName} = @param0";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", conditionValue);

                    ReplaceDBNullParameterValue(conditionValue, ref command,
                        command.Parameters[0].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);
        }

        /// <summary>
        ///     Вызывает DELETE в таблице <paramref name="table"/> для удаления всех строк, соответствующих условиям: столбец <paramref name="conditionName1"/> = <paramref name="conditionValue1"/> AND столбец <paramref name="conditionName2"/> = <paramref name="conditionValue2"/>.
        /// </summary>
        /// <param name="table">
        ///     Таблица, для которой будет вызван DELETE.
        /// </param>
        /// <param name="conditionName1">
        ///     Название столбца для условия #1.
        /// </param>
        /// <param name="conditionValue1">
        ///     Значение столбца для условия #1.
        /// </param>
        /// <param name="conditionName2">
        ///     Название столбца для условия #2.
        /// </param>
        /// <param name="conditionValue2">
        ///     Значение столбца для условия #2.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Delete(string table, string conditionName1, string conditionValue1,
            string conditionName2, string conditionValue2, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"DELETE FROM {table} WHERE {conditionName1} = @param0 " +
                          $"AND {conditionName2} = @param1";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", conditionValue1);
                    command.Parameters.AddWithValue("@param1", conditionValue2);

                    ReplaceDBNullParameterValue(conditionValue1, ref command,
                        command.Parameters[0].ParameterName);

                    ReplaceDBNullParameterValue(conditionValue2, ref command,
                        command.Parameters[1].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);
        }
        /// <summary>
        ///     Вызывает DELETE в таблице <paramref name="table"/> с временем ожидания <paramref name="timeout"/> для удаления всех строк, соответствующих условиям: столбец <paramref name="conditionName1"/> = <paramref name="conditionValue1"/> AND столбец <paramref name="conditionName2"/> = <paramref name="conditionValue2"/>.
        /// </summary>
        /// <param name="table">
        ///     Таблица, для которой будет вызван DELETE.
        /// </param>
        /// <param name="conditionName1">
        ///     Название столбца для условия #1.
        /// </param>
        /// <param name="conditionValue1">
        ///     Значение столбца для условия #1.
        /// </param>
        /// <param name="conditionName2">
        ///     Название столбца для условия #2.
        /// </param>
        /// <param name="conditionValue2">
        ///     Значение столбца для условия #2.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Delete(string table, string conditionName1, string conditionValue1,
            string conditionName2, string conditionValue2, TimeSpan timeout, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"DELETE FROM {table} WHERE {conditionName1} = @param0 " +
                          $"AND {conditionName2} = @param1";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", conditionValue1);
                    command.Parameters.AddWithValue("@param1", conditionValue2);

                    ReplaceDBNullParameterValue(conditionValue1, ref command,
                        command.Parameters[0].ParameterName);

                    ReplaceDBNullParameterValue(conditionValue2, ref command,
                        command.Parameters[1].ParameterName);

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);
        }

        /// <summary>
        ///     Вызывает DELETE в таблице <paramref name="table"/> для удаления всех строк, соответствующих условиям из массива <paramref name="conditions"/>: каждый столбец с именем <paramref name="conditions"/>.Name равен соответствующему значению <paramref name="conditions"/>.Value.
        /// </summary>
        /// <param name="table">
        ///     Таблица, для которой будет вызван DELETE.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса.
        ///     Name - название столбца.
        ///     Value - значение столбца.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Delete(string table, (string Name, string Value)[] conditions, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (conditions.Length == 0)
            {
                var exception = new ArgumentException("Count of conditions is 0");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"DELETE FROM {table} WHERE ");

                    sqlBuilder.Append($"{conditions[0].Name} = @param0");
                    command.Parameters.AddWithValue("@param0", conditions[0].Value);

                    ReplaceDBNullParameterValue(conditions[0].Value, ref command,
                        command.Parameters[0].ParameterName);

                    for (int i = 1; i < conditions.Length; ++i)
                    {
                        sqlBuilder.Append($" AND {conditions[i].Name} = @param{i.ToString()}");
                        command.Parameters.AddWithValue($"@param{i.ToString()}", conditions[i].Value);

                        ReplaceDBNullParameterValue(conditions[i].Value, ref command,
                            command.Parameters[i].ParameterName);
                    }

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{CommandTimeout}]");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);
        }
        /// <summary>
        ///     Вызывает DELETE в таблице <paramref name="table"/> с временем ожидания <paramref name="timeout"/> для удаления всех строк, соответствующих условиям из массива <paramref name="conditions"/>: каждый столбец с именем <paramref name="conditions"/>.Name равен соответствующему значению <paramref name="conditions"/>.Value.
        /// </summary>
        /// <param name="table">
        ///     Таблица, для которой будет вызван DELETE.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса.
        ///     Name - название столбца.
        ///     Value - значение столбца.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Delete(string table, (string Name, string Value)[] conditions, 
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (conditions.Length == 0)
            {
                var exception = new ArgumentException("Count of conditions is 0");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"DELETE FROM {table} WHERE ");

                    sqlBuilder.Append($"{conditions[0].Name} = @param0");
                    command.Parameters.AddWithValue("@param0", conditions[0].Value);

                    ReplaceDBNullParameterValue(conditions[0].Value, ref command,
                        command.Parameters[0].ParameterName);

                    for (int i = 1; i < conditions.Length; ++i)
                    {
                        sqlBuilder.Append($" AND {conditions[i].Name} = @param{i.ToString()}");
                        command.Parameters.AddWithValue($"@param{i.ToString()}", conditions[i].Value);

                        ReplaceDBNullParameterValue(conditions[i].Value, ref command,
                            command.Parameters[i].ParameterName);
                    }

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{timeout}]");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);
        }



        /// <summary>
        ///     Вызывает UPDATE в таблице <paramref name="table"/> для изменения значения столбца <paramref name="changingField"/> на <paramref name="newFieldValue"/> во всех строках.
        /// </summary>
        /// <param name="changingField">
        ///     Названия столбца, значение которого надо изменить.
        /// </param>
        /// <param name="newFieldValue">
        ///     Новое значение для столбца <paramref name="changingField"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван UPDATE.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Update(string changingField, string newFieldValue, string table, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"UPDATE {table} SET {changingField} = @param0";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", newFieldValue);

                    ReplaceDBNullParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName);
                    ReplaceFunctionParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName, ref sql);

                    command.CommandText = sql;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);
        }
        /// <summary>
        ///     Вызывает UPDATE в таблице <paramref name="table"/> с временем ожидания <paramref name="timeout"/> для изменения значения столбца <paramref name="changingField"/> на <paramref name="newFieldValue"/> во всех строках.
        /// </summary>
        /// <param name="changingField">
        ///     Названия столбца, значение которого надо изменить.
        /// </param>
        ///  <param name="newFieldValue">
        ///     Новое значение для столбца <paramref name="changingField"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван UPDATE.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Update(string changingField, string newFieldValue, string table, 
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"UPDATE {table} SET {changingField} = @param0";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", newFieldValue);

                    ReplaceDBNullParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName);
                    ReplaceFunctionParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName, ref sql);

                    command.CommandText = sql;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);
        }

        /// <summary>
        ///     Вызывает UPDATE в таблице <paramref name="table"/> для изменения значения столбца <paramref name="changingField"/> на <paramref name="newFieldValue"/> во всех строках, соответствующих условию: столбец <paramref name="conditionName"/> = <paramref name="conditionValue"/>.
        /// </summary>
        /// <param name="changingField">
        ///     Названия столбца, значение которого надо изменить.
        /// </param>
        /// <param name="newFieldValue">
        ///     Новое значение для столбца <paramref name="changingField"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван UPDATE.
        /// </param>
        /// <param name="conditionName">
        ///     Название столбца для условия.
        /// </param>
        /// <param name="conditionValue">
        ///     Значение столбца для условия.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Update(string changingField, string newFieldValue, string table,
            string conditionName, string conditionValue, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"UPDATE {table} SET {changingField} = @param0 WHERE " +
                          $"{conditionName} = @param1";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", newFieldValue);

                    ReplaceDBNullParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName);
                    ReplaceFunctionParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName, ref sql);

                    command.Parameters.AddWithValue("@param1", newFieldValue);

                    ReplaceDBNullParameterValue(conditionValue, ref command,
                        command.Parameters[1].ParameterName);

                    command.CommandText = sql;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);
        }
        /// <summary>
        ///     Вызывает UPDATE в таблице <paramref name="table"/> с временем ожидания <paramref name="timeout"/> для изменения значения столбца <paramref name="changingField"/> на <paramref name="newFieldValue"/> во всех строках, соответствующих условию: столбец <paramref name="conditionName"/> = <paramref name="conditionValue"/>.
        /// </summary>
        /// <param name="changingField">
        ///     Названия столбца, значение которого надо изменить.
        /// </param>
        ///  <param name="newFieldValue">
        ///     Новое значение для столбца <paramref name="changingField"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван UPDATE.
        /// </param>
        /// <param name="conditionName">
        ///     Название столбца для условия.
        /// </param>
        /// <param name="conditionValue">
        ///     Значение столбца для условия.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Update(string changingField, string newFieldValue, string table,
            string conditionName, string conditionValue, TimeSpan timeout, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"UPDATE {table} SET {changingField} = @param0 WHERE " +
                          $"{conditionName} = @param1";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", newFieldValue);

                    ReplaceDBNullParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName);
                    ReplaceFunctionParameterValue(newFieldValue, ref command, 
                        command.Parameters[0].ParameterName, ref sql);

                    command.Parameters.AddWithValue("@param1", newFieldValue);

                    ReplaceDBNullParameterValue(conditionValue, ref command,
                        command.Parameters[1].ParameterName);

                    command.CommandText = sql;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);
        }

        /// <summary>
        ///     Вызывает UPDATE в таблице <paramref name="table"/> для изменения значения столбца <paramref name="changingField"/> на <paramref name="newFieldValue"/> во всех строках, соответствующих условиям: столбец <paramref name="conditionName1"/> = <paramref name="conditionValue1"/> AND столбец <paramref name="conditionName2"/> = <paramref name="conditionValue2"/>.
        /// </summary>
        /// <param name="changingField">
        ///     Названия столбца, значение которого надо изменить.
        /// </param>
        /// <param name="newFieldValue">
        ///     Новое значение для столбца <paramref name="changingField"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван UPDATE.
        /// </param>
        /// <param name="conditionName1">
        ///     Название столбца для условия #1.
        /// </param>
        /// <param name="conditionValue1">
        ///     Значение столбца для условия #1.
        /// </param>
        /// <param name="conditionName2">
        ///     Название столбца для условия #2.
        /// </param>
        /// <param name="conditionValue2">
        ///     Значение столбца для условия #2.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Update(string changingField, string newFieldValue, string table,
            string conditionName1, string conditionValue1, string conditionName2, string conditionValue2, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"UPDATE {table} SET {changingField} = @param0 WHERE " +
                          $"{conditionName1} = @param1 " +
                          $"AND {conditionName2} = @param2";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", newFieldValue);

                    ReplaceDBNullParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName);
                    ReplaceFunctionParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName, ref sql);

                    command.Parameters.AddWithValue("@param1", conditionValue1);
                    command.Parameters.AddWithValue("@param2", conditionValue2);

                    ReplaceDBNullParameterValue(conditionValue1, ref command,
                        command.Parameters[1].ParameterName);

                    ReplaceDBNullParameterValue(conditionValue2, ref command,
                        command.Parameters[2].ParameterName);

                    command.CommandText = sql;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);
        }
        /// <summary>
        ///     Вызывает UPDATE в таблице <paramref name="table"/> для изменения значения столбца <paramref name="changingField"/> на <paramref name="newFieldValue"/> во всех строках, соответствующих условиям: столбец <paramref name="conditionName1"/> = <paramref name="conditionValue1"/> AND столбец <paramref name="conditionName2"/> = <paramref name="conditionValue2"/>.
        /// </summary>
        /// <param name="changingField">
        ///     Названия столбца, значение которого надо изменить.
        /// </param>
        /// <param name="newFieldValue">
        ///     Новое значение для столбца <paramref name="changingField"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван UPDATE.
        /// </param>
        /// <param name="conditionName1">
        ///     Название столбца для условия #1.
        /// </param>
        /// <param name="conditionValue1">
        ///     Значение столбца для условия #1.
        /// </param>
        /// <param name="conditionName2">
        ///     Название столбца для условия #2.
        /// </param>
        /// <param name="conditionValue2">
        ///     Значение столбца для условия #2.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Update(string changingField, string newFieldValue, string table,
            string conditionName1, string conditionValue1, string conditionName2, string conditionValue2, 
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"UPDATE {table} SET {changingField} = @param0 WHERE " +
                          $"{conditionName1} = @param1 " +
                          $"AND {conditionName2} = @param2";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    command.Parameters.AddWithValue("@param0", newFieldValue);

                    ReplaceDBNullParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName);
                    ReplaceFunctionParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName, ref sql);

                    command.Parameters.AddWithValue("@param1", conditionValue1);
                    command.Parameters.AddWithValue("@param2", conditionValue2);

                    ReplaceDBNullParameterValue(conditionValue1, ref command,
                        command.Parameters[1].ParameterName);

                    ReplaceDBNullParameterValue(conditionValue2, ref command,
                        command.Parameters[2].ParameterName);

                    command.CommandText = sql;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);
        }

        /// <summary>
        ///     Вызывает UPDATE в таблице <paramref name="table"/> для изменения значения столбца <paramref name="changingField"/> на <paramref name="newFieldValue"/> во всех строках, соответствующих условиям из массива <paramref name="conditions"/>: каждый столбец с именем <paramref name="conditions"/>.Name равен соответствующему значению <paramref name="conditions"/>.Value.
        /// </summary>
        /// <param name="changingField">
        ///     Названия столбца, значение которого надо изменить.
        /// </param>
        /// <param name="newFieldValue">
        ///     Новое значение для столбца <paramref name="changingField"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван UPDATE.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса.
        ///     Name - название столбца.
        ///     Value - значение столбца.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Update(string changingField, string newFieldValue, string table, (string Name, string Value)[] conditions, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (conditions.Length == 0)
            {
                var exception = new ArgumentException("Count of conditions is 0");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"UPDATE {table} SET {changingField} = @param0 WHERE ");

                    command.Parameters.AddWithValue("@param0", newFieldValue);

                    ReplaceDBNullParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName);
                    ReplaceFunctionParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName, ref sqlBuilder);

                    sqlBuilder.Append($"{conditions[0].Name} = @param1");
                    command.Parameters.AddWithValue("@param1", conditions[0].Value);

                    ReplaceDBNullParameterValue(conditions[0].Value, ref command,
                        command.Parameters[1].ParameterName);

                    for (int i = 2; i < conditions.Length; ++i)
                    {
                        int arrayIndex = i - 1;

                        sqlBuilder.Append($" AND {conditions[arrayIndex].Name} = @param{i.ToString()}");
                        command.Parameters.AddWithValue($"@param{i.ToString()}", conditions[arrayIndex].Value);

                        ReplaceDBNullParameterValue(conditions[arrayIndex].Value, ref command,
                            command.Parameters[i].ParameterName);
                    }

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{CommandTimeout}]");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);
        }
        /// <summary>
        ///     Вызывает UPDATE в таблице <paramref name="table"/> для изменения значения столбца <paramref name="changingField"/> на <paramref name="newFieldValue"/> во всех строках, соответствующих условиям из массива <paramref name="conditions"/>: каждый столбец с именем <paramref name="conditions"/>.Name равен соответствующему значению <paramref name="conditions"/>.Value.
        /// </summary>
        /// <param name="changingField">
        ///     Названия столбца, значение которого надо изменить.
        /// </param>
        /// <param name="newFieldValue">
        ///     Новое значение для столбца <paramref name="changingField"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван UPDATE.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса.
        ///     Name - название столбца.
        ///     Value - значение столбца.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Update(string changingField, string newFieldValue, string table, (string Name, string Value)[] conditions, 
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (conditions.Length == 0)
            {
                var exception = new ArgumentException("Count of conditions is 0");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"UPDATE {table} SET {changingField} = @param0 WHERE ");

                    command.Parameters.AddWithValue("@param0", newFieldValue);

                    ReplaceDBNullParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName);
                    ReplaceFunctionParameterValue(newFieldValue, ref command,
                        command.Parameters[0].ParameterName, ref sqlBuilder);

                    sqlBuilder.Append($"{conditions[0].Name} = @param1");
                    command.Parameters.AddWithValue("@param1", conditions[0].Value);

                    ReplaceDBNullParameterValue(conditions[0].Value, ref command,
                        command.Parameters[1].ParameterName);

                    for (int i = 2; i < conditions.Length; ++i)
                    {
                        int arrayIndex = i - 1;

                        sqlBuilder.Append($" AND {conditions[arrayIndex].Name} = @param{i.ToString()}");
                        command.Parameters.AddWithValue($"@param{i.ToString()}", conditions[arrayIndex].Value);

                        ReplaceDBNullParameterValue(conditions[arrayIndex].Value, ref command,
                            command.Parameters[i].ParameterName);
                    }

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{timeout}]");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);
        }



        /// <summary>
        ///     Вызывает INSERT для вставки новой строки в таблицу <paramref name="table"/>.
        /// </summary>
        /// <param name="values">
        ///     Значения столбцов по порядку их следования в таблице <paramref name="table"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public long Insert(string[] values, string table, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            long result = 0L;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"INSERT INTO {table} VALUES (");

                    sqlBuilder.Append("@param0");
                    command.Parameters.AddWithValue("@param0", values[0]);

                    ReplaceDBNullParameterValue(values[0],
                        ref command, command.Parameters[0].ParameterName);
                    ReplaceFunctionParameterValue(values[0],
                        ref command, command.Parameters[0].ParameterName, ref sqlBuilder);

                    for (int i = 1; i < values.Length; ++i)
                    {
                        sqlBuilder.Append($", @param{i.ToString()}");
                        command.Parameters.AddWithValue($"@param{i.ToString()}", values[i]);

                        ReplaceDBNullParameterValue(values[i],
                            ref command, command.Parameters[i].ParameterName);
                        ReplaceFunctionParameterValue(values[i],
                            ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                    }

                    sqlBuilder.Append(")");

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);

                    result = command.LastInsertedId;
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{CommandTimeout}]");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Вызывает INSERT с временем ожидания <paramref name="timeout"/> для вставки новой строки в таблицу <paramref name="table"/>.
        /// </summary>
        /// <param name="values">
        ///     Значения столбцов по порядку их следования в таблице <paramref name="table"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public long Insert(string[] values, string table, TimeSpan timeout, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            long result = 0L;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"INSERT INTO {table} VALUES (");

                    sqlBuilder.Append("@param0");
                    command.Parameters.AddWithValue("@param0", values[0]);

                    ReplaceDBNullParameterValue(values[0],
                        ref command, command.Parameters[0].ParameterName);
                    ReplaceFunctionParameterValue(values[0],
                        ref command, command.Parameters[0].ParameterName, ref sqlBuilder);

                    for (int i = 1; i < values.Length; ++i)
                    {
                        sqlBuilder.Append($", @param{i.ToString()}");
                        command.Parameters.AddWithValue($"@param{i.ToString()}", values[i]);

                        ReplaceDBNullParameterValue(values[i],
                            ref command, command.Parameters[i].ParameterName);
                        ReplaceFunctionParameterValue(values[i],
                            ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                    }

                    sqlBuilder.Append(")");

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);

                    result = command.LastInsertedId;
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{timeout}]");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }



        /// <summary>
        ///     Вызывает TRUNCATE для очистки таблицы <paramref name="table"/>.
        /// </summary>
        /// <param name="table">
        ///     Таблица, для которой будет вызван TRUNCATE.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void TruncateTable(string table, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"TRUNCATE TABLE {table}";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{CommandTimeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);
        }
        /// <summary>
        ///     Вызывает TRUNCATE для очистки таблицы <paramref name="table"/>.
        /// </summary>
        /// <param name="table">
        ///     Таблица, для которой будет вызван TRUNCATE.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void TruncateTable(string table, TimeSpan timeout, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                try
                {
                    sql = $"TRUNCATE TABLE {table}";
                    command = new MySqlCommand(sql, connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    CommandExecuteNonQuery(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{timeout}] or canceled");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);
        }



        /// <summary>
        ///     Объединяет вызов INSERT для вставки новой строки в таблицу <paramref name="tableInsert"/> с последующим вызовом SELECT для получения результата функции. 
        /// </summary>
        /// <param name="valuesInsert">
        ///     Значения столбцов по порядку их следования в таблице <paramref name="tableInsert"/>.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Параметры функции. Передайте пустой массив для вызова функции без параметров.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string Union_Insert_SelectFunction(string[] valuesInsert, string tableInsert,
            string nameFunction, string[] parametersValuesFunction,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string result = string.Empty;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"INSERT INTO {tableInsert} VALUES (");

                    sqlBuilder.Append("@param0");
                    command.Parameters.AddWithValue("@param0", valuesInsert[0]);

                    ReplaceDBNullParameterValue(valuesInsert[0],
                        ref command, command.Parameters[0].ParameterName);
                    ReplaceFunctionParameterValue(valuesInsert[0],
                        ref command, command.Parameters[0].ParameterName, ref sqlBuilder);

                    for (int i = 1; i < valuesInsert.Length; ++i)
                    {
                        sqlBuilder.Append($", @param{i.ToString()}");
                        command.Parameters.AddWithValue($"@param{i.ToString()}", valuesInsert[i]);

                        ReplaceDBNullParameterValue(valuesInsert[i],
                            ref command, command.Parameters[i].ParameterName);
                        ReplaceFunctionParameterValue(valuesInsert[i],
                            ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                    }

                    sqlBuilder.Append($"); SELECT SELECT {nameFunction}(");

                    if (parametersValuesFunction.Length != 0)
                    {
                        sqlBuilder.Append($"@param{valuesInsert.Length}");
                        command.Parameters.AddWithValue($"@param{valuesInsert.Length}", parametersValuesFunction[0]);

                        ReplaceDBNullParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[valuesInsert.Length].ParameterName);
                        ReplaceFunctionParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[valuesInsert.Length].ParameterName, ref sqlBuilder);

                        for (int i = valuesInsert.Length + 1; i < valuesInsert.Length + parametersValuesFunction.Length; ++i)
                        {
                            int arrayIndex = i - valuesInsert.Length;

                            sqlBuilder.Append($", @param{i.ToString()}");
                            command.Parameters.AddWithValue($"@param{i.ToString()}", parametersValuesFunction[arrayIndex]);

                            ReplaceDBNullParameterValue(parametersValuesFunction[arrayIndex],
                                ref command, command.Parameters[i].ParameterName);
                            ReplaceFunctionParameterValue(parametersValuesFunction[arrayIndex],
                                ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                        }
                    }

                    sqlBuilder.Append($");");

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token)[0];
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{CommandTimeout}]");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Объединяет вызов INSERT для вставки новой строки в таблицу <paramref name="tableInsert"/> с последующим вызовом SELECT для получения результата функции с общим временем ожидания <paramref name="timeout"/>.
        /// </summary>
        /// <param name="valuesInsert">
        ///     Значения столбцов по порядку их следования в таблице <paramref name="tableInsert"/>.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Параметры функции. Передайте пустой массив для вызова функции без параметров.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string Union_Insert_SelectFunction(string[] valuesInsert, string tableInsert,
            string nameFunction, string[] parametersValuesFunction, TimeSpan timeout,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string result = string.Empty;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"INSERT INTO {tableInsert} VALUES (");

                    sqlBuilder.Append("@param0");
                    command.Parameters.AddWithValue("@param0", valuesInsert[0]);

                    ReplaceDBNullParameterValue(valuesInsert[0],
                        ref command, command.Parameters[0].ParameterName);
                    ReplaceFunctionParameterValue(valuesInsert[0],
                        ref command, command.Parameters[0].ParameterName, ref sqlBuilder);

                    for (int i = 1; i < valuesInsert.Length; ++i)
                    {
                        sqlBuilder.Append($", @param{i.ToString()}");
                        command.Parameters.AddWithValue($"@param{i.ToString()}", valuesInsert[i]);

                        ReplaceDBNullParameterValue(valuesInsert[i],
                            ref command, command.Parameters[i].ParameterName);
                        ReplaceFunctionParameterValue(valuesInsert[i],
                            ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                    }

                    sqlBuilder.Append($"); SELECT SELECT {nameFunction}(");

                    if (parametersValuesFunction.Length != 0)
                    {
                        sqlBuilder.Append($"@param{valuesInsert.Length}");
                        command.Parameters.AddWithValue($"@param{valuesInsert.Length}", parametersValuesFunction[0]);

                        ReplaceDBNullParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[valuesInsert.Length].ParameterName);
                        ReplaceFunctionParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[valuesInsert.Length].ParameterName, ref sqlBuilder);

                        for (int i = valuesInsert.Length + 1; i < valuesInsert.Length + parametersValuesFunction.Length; ++i)
                        {
                            int arrayIndex = i - valuesInsert.Length;

                            sqlBuilder.Append($", @param{i.ToString()}");
                            command.Parameters.AddWithValue($"@param{i.ToString()}", parametersValuesFunction[arrayIndex]);

                            ReplaceDBNullParameterValue(parametersValuesFunction[arrayIndex],
                                ref command, command.Parameters[i].ParameterName);
                            ReplaceFunctionParameterValue(parametersValuesFunction[arrayIndex],
                                ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                        }
                    }

                    sqlBuilder.Append($");");

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token)[0];
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{timeout}]");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }

        /// <summary>
        ///     Объединяет вызов INSERT для вставки новой строки в таблицу <paramref name="tableInsert"/> с последующим вызовом SELECT в таблице <paramref name="tableFunction"/> для получения результата функции. 
        /// </summary>
        /// <param name="valuesInsert">
        ///     Значения столбцов по порядку их следования в таблице <paramref name="tableInsert"/>.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Параметры функции. Передайте пустой массив для вызова функции без параметров.
        /// </param>
        /// <param name="tableFunction">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string Union_Insert_SelectFunction(string[] valuesInsert, string tableInsert,
            string nameFunction, string[] parametersValuesFunction, string tableFunction, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string result = string.Empty;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandTimeout = (int)CommandTimeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"INSERT INTO {tableInsert} VALUES (");

                    sqlBuilder.Append("@param0");
                    command.Parameters.AddWithValue("@param0", valuesInsert[0]);

                    ReplaceDBNullParameterValue(valuesInsert[0],
                        ref command, command.Parameters[0].ParameterName);
                    ReplaceFunctionParameterValue(valuesInsert[0],
                        ref command, command.Parameters[0].ParameterName, ref sqlBuilder);

                    for (int i = 1; i < valuesInsert.Length; ++i)
                    {
                        sqlBuilder.Append($", @param{i.ToString()}");
                        command.Parameters.AddWithValue($"@param{i.ToString()}", valuesInsert[i]);

                        ReplaceDBNullParameterValue(valuesInsert[i],
                            ref command, command.Parameters[i].ParameterName);
                        ReplaceFunctionParameterValue(valuesInsert[i],
                            ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                    }

                    sqlBuilder.Append($"); SELECT SELECT {nameFunction}(");

                    if (parametersValuesFunction.Length != 0)
                    {
                        sqlBuilder.Append($"@param{valuesInsert.Length}");
                        command.Parameters.AddWithValue($"@param{valuesInsert.Length}", parametersValuesFunction[0]);

                        ReplaceDBNullParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[valuesInsert.Length].ParameterName);
                        ReplaceFunctionParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[valuesInsert.Length].ParameterName, ref sqlBuilder);

                        for (int i = valuesInsert.Length + 1; i < valuesInsert.Length + parametersValuesFunction.Length; ++i)
                        {
                            int arrayIndex = i - valuesInsert.Length;

                            sqlBuilder.Append($", @param{i.ToString()}");
                            command.Parameters.AddWithValue($"@param{i.ToString()}", parametersValuesFunction[arrayIndex]);

                            ReplaceDBNullParameterValue(parametersValuesFunction[arrayIndex],
                                ref command, command.Parameters[i].ParameterName);
                            ReplaceFunctionParameterValue(parametersValuesFunction[arrayIndex],
                                ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                        }
                    }

                    sqlBuilder.Append($") FROM {tableFunction};");

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token)[0];
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{CommandTimeout}]");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(CommandTimeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
        /// <summary>
        ///     Объединяет вызов INSERT для вставки новой строки в таблицу <paramref name="tableInsert"/> с последующим вызовом SELECT в таблице <paramref name="tableFunction"/> для получения результата функции с общим временем ожидания <paramref name="timeout"/>.
        /// </summary>
        /// <param name="valuesInsert">
        ///     Значения столбцов по порядку их следования в таблице <paramref name="tableInsert"/>.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Параметры функции. Передайте пустой массив для вызова функции без параметров.
        /// </param>
        /// <param name="tableFunction">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="MySql.Data.MySqlClient.MySqlException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string Union_Insert_SelectFunction(string[] valuesInsert, string tableInsert,
            string nameFunction, string[] parametersValuesFunction, string tableFunction, TimeSpan timeout, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string result = string.Empty;

            if (!CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                CurrentMySQLConnection.DShowError?.Invoke(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource requestCancellationToken = new CancellationTokenSource();
            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(requestCancellationToken.Token, GlobalCancellationToken.Token);
            Task read = Task.Factory.StartNew(() =>
            {
                string sql = string.Empty;
                MySqlCommand command;
                MySqlConnection connection = GetNextMySqlConnection(out ushort connectionIndex);
                MySqlTransaction transaction = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString(), connection);
                    command.CommandTimeout = (int)timeout.TotalMilliseconds + 3000;

                    sqlBuilder.Append($"INSERT INTO {tableInsert} VALUES (");

                    sqlBuilder.Append("@param0");
                    command.Parameters.AddWithValue("@param0", valuesInsert[0]);

                    ReplaceDBNullParameterValue(valuesInsert[0],
                        ref command, command.Parameters[0].ParameterName);
                    ReplaceFunctionParameterValue(valuesInsert[0],
                        ref command, command.Parameters[0].ParameterName, ref sqlBuilder);

                    for (int i = 1; i < valuesInsert.Length; ++i)
                    {
                        sqlBuilder.Append($", @param{i.ToString()}");
                        command.Parameters.AddWithValue($"@param{i.ToString()}", valuesInsert[i]);

                        ReplaceDBNullParameterValue(valuesInsert[i],
                            ref command, command.Parameters[i].ParameterName);
                        ReplaceFunctionParameterValue(valuesInsert[i],
                            ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                    }

                    sqlBuilder.Append($"); SELECT SELECT {nameFunction}(");

                    if (parametersValuesFunction.Length != 0)
                    {
                        sqlBuilder.Append($"@param{valuesInsert.Length}");
                        command.Parameters.AddWithValue($"@param{valuesInsert.Length}", parametersValuesFunction[0]);

                        ReplaceDBNullParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[valuesInsert.Length].ParameterName);
                        ReplaceFunctionParameterValue(parametersValuesFunction[0],
                            ref command, command.Parameters[valuesInsert.Length].ParameterName, ref sqlBuilder);

                        for (int i = valuesInsert.Length + 1; i < valuesInsert.Length + parametersValuesFunction.Length; ++i)
                        {
                            int arrayIndex = i - valuesInsert.Length;

                            sqlBuilder.Append($", @param{i.ToString()}");
                            command.Parameters.AddWithValue($"@param{i.ToString()}", parametersValuesFunction[arrayIndex]);

                            ReplaceDBNullParameterValue(parametersValuesFunction[arrayIndex],
                                ref command, command.Parameters[i].ParameterName);
                            ReplaceFunctionParameterValue(parametersValuesFunction[arrayIndex],
                                ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                        }
                    }

                    sqlBuilder.Append($") FROM {tableFunction};");

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = CommandExecuteReader(connectionIndex, isolationLevel, ref command, ref transaction,
                        cancellationToken.Token)[0];
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder.ToString()}] waiting timeout[{timeout}]");
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    transaction?.Rollback();
                    throw exception;
                }
                catch (MySqlException ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    Events.DShowError?.Invoke(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder.ToString()}] execute error - " + ex.Message, ex.StackTrace));
                    CurrentMySQLConnection.DShowError?.Invoke(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));
                    transaction?.Rollback();
                    throw;
                }
                finally
                {
                    requestCancellationToken.Dispose();
                    cancellationToken.Dispose();
                }
            }, cancellationToken.Token);

            cancellationToken.CancelAfter(timeout);
            read.Wait(cancellationToken.Token);

            return result;
        }
    }
}
