// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;
using RIS.Extensions;

namespace RIS.Connection.MySQL.Requests
{
    /// <summary>
    ///     Представляет запрос к MySQL базе данных. Этот класс не может быть унаследован.
    /// </summary>
    public sealed class InsertRequest : IQueryRequest<long>
    {
        /// <summary>
        ///     Позволяет получать экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </summary>
        public IRequestEngine Engine { get; }
        /// <summary>
        ///     Позволяет получать или устанавливать массив значений столбцов по порядку их следования в таблице <see cref="Table"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </summary>
        public string[] Values { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать таблицу, для которой будет вызван INSERT.
        /// </summary>
        public string Table { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать время ожидания ответа от сервера.
        /// </summary>
        public TimeSpan Timeout { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать уровень изоляции транзакции.
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; }
        /// <summary>
        ///     Позволяет получать токен отмены запроса.
        /// </summary>
        public CancellationTokenSource RequestCancellationToken { get; }

        private InsertRequest(IRequestEngine engine, InsertRequest request)
            : this(engine, request.Values, request.Table, request.Timeout, request.IsolationLevel)
        {

        }
        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.Requests.InsertRequest"/>.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="values">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="table"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        public InsertRequest(IRequestEngine engine, string[] values, string table,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            : this(engine, values, table, engine.CommandTimeout, isolationLevel)
        {

        }
        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.Requests.InsertRequest"/>.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="values">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="table"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
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
        public InsertRequest(IRequestEngine engine, string[] values, string table,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (engine == null)
            {
                var exception = new ArgumentNullException(nameof(engine), $"{nameof(engine)} cannot be null");
                Events.OnError(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            Engine = engine;
            Values = values;
            Table = table;
            Timeout = timeout;
            IsolationLevel = isolationLevel;
            RequestCancellationToken = new CancellationTokenSource();
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с текущими параметрами.
        /// </summary>
        /// <returns>
        ///     Значение типа <see cref="DataSet"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public long Execute()
        {
            try
            {
                var task = ExecuteAsync();
                task.Wait();
                return task.Result;
            }
            catch (AggregateException ex)
                when (ex.InnerExceptions.Count == 1)
            {
                throw ex.InnerExceptions[0];
            }
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с текущими параметрами.
        /// </summary>
        /// <returns>
        ///     Значение типа <see cref="DataSet"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public async Task<long> ExecuteAsync()
        {
            long result = 0L;

            if (!Engine.CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.OnError(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                Engine.CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(RequestCancellationToken.Token, Engine.GlobalCancellationToken.Token);
            Task<Task> request = Task.Run(async () =>
            {
                MySqlCommand command = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString())
                    {
                        CommandTimeout = (int)Timeout.TotalSeconds + 3
                    };

                    sqlBuilder.Append("INSERT INTO ").Append(Table).Append(" VALUES (");

                    if (Values.Length != 0)
                    {
                        sqlBuilder.Append("@param0");
                        command.Parameters.AddWithValue("@param0", Values[0]);

                        RequestEngineHelper.ReplaceDBNullParameterValue(Values[0],
                            ref command, command.Parameters[0].ParameterName);
                        RequestEngineHelper.ReplaceFunctionParameterValue(Values[0],
                            ref command, command.Parameters[0].ParameterName, ref sqlBuilder);

                        for (int i = 1; i < Values.Length; ++i)
                        {
                            sqlBuilder.Append(", @param").Append(i);
                            command.Parameters.AddWithValue($"@param{i}", Values[i]);

                            RequestEngineHelper.ReplaceDBNullParameterValue(Values[i],
                                ref command, command.Parameters[i].ParameterName);
                            RequestEngineHelper.ReplaceFunctionParameterValue(Values[i],
                                ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                        }
                    }

                    sqlBuilder.Append(')');

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    await Engine.CommandExecuteNonQueryAsync(command, cancellationToken.Token, IsolationLevel).ConfigureAwait(false);

                    result = command.LastInsertedId;
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder}] waiting timeout[{Timeout}] or canceled");
                    Events.OnError(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    Engine.CurrentMySQLConnection.OnError(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));

                    if (command?.Transaction != null)
                        await command.Transaction.RollbackAsync().ConfigureAwait(false);

                    throw exception;
                }
                catch (Exception ex)
                {
                    Events.OnError(this,
                        new RErrorEventArgs($"MySQLRequest[{sqlBuilder}] execute error - " + ex.Message, ex.StackTrace));
                    Engine.CurrentMySQLConnection.OnError(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));

                    if (command?.Transaction != null)
                        await command.Transaction.RollbackAsync().ConfigureAwait(false);

                    throw;
                }
                finally
                {
                    cancellationToken.Dispose();
                }

                return Task.CompletedTask;
            }, cancellationToken.Token);

            try
            {
                RequestCancellationToken.CancelAfter(Timeout);
                await request.WaitAsync(Timeout).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                var exception = new TimeoutException($"MySQLRequest[unknown] waiting Timeout[{Timeout}] or canceled");
                Events.OnError(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                Engine.CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));

                throw exception;
            }
            catch (AggregateException ex)
                when (ex.InnerExceptions.Count == 1)
            {
                throw ex.InnerExceptions[0];
            }

            return result;
        }

        /// <summary>
        ///     Вызывает отмену запроса.
        /// </summary>
        /// <exception cref="AggregateException"></exception>
        public void Cancel()
        {
            try
            {
                RequestCancellationToken.Cancel();
            }
            catch (Exception)
            {
                var exception = new Exception("Failed to cancel request");
                Events.OnError(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                Engine.CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
        }

        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        IRequest IRequest.Copy()
        {
            return Copy(Engine);
        }
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        IRequest IRequest.Copy(IRequestEngine engine)
        {
            return Copy(engine);
        }

        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        IQueryRequest<long> IQueryRequest<long>.Copy()
        {
            return Copy(Engine);
        }
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        IQueryRequest<long> IQueryRequest<long>.Copy(IRequestEngine engine)
        {
            return Copy(engine);
        }

        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        public InsertRequest Copy()
        {
            return Copy(Engine);
        }
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        public InsertRequest Copy(IRequestEngine engine)
        {
            return new InsertRequest(engine, this);
        }



        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="values">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="table"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static long Insert(IRequestEngine engine, string[] values, string table,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Insert(engine, values, table, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="values">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="table"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static long Insert(IRequestEngine engine, string[] values, string table,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new InsertRequest(engine, values, table,
                timeout, isolationLevel).Execute();
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="values">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="table"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<long> InsertAsync(IRequestEngine engine, string[] values, string table,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return InsertAsync(engine, values, table, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="values">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="table"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<long> InsertAsync(IRequestEngine engine, string[] values, string table,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new InsertRequest(engine, values, table,
                timeout, isolationLevel).ExecuteAsync();
        }
    }
}
