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
    public sealed class UpdateRequest : INonQueryRequest
    {
        /// <summary>
        ///     Позволяет получать экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </summary>
        public IRequestEngine Engine { get; }
        /// <summary>
        ///     Позволяет получать или устанавливать название столбца, значение которого надо изменить.
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать новое значение для столбца <see cref="ColumnName"/>.
        /// </summary>
        public string ColumnValue { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать таблицу, для которой будет вызван UPDATE.
        /// </summary>
        public string Table { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать массив условий для запроса. Передайте пустой массив, чтобы не использовать условия.
        ///     Name - название столбца.
        ///     Value - значение столбца.
        /// </summary>
        public (string Name, string Value)[] Conditions { get; set; }
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

        private UpdateRequest(IRequestEngine engine, UpdateRequest request)
            : this(engine, request.ColumnName, request.ColumnValue, request.Table,
                request.Conditions, request.Timeout, request.IsolationLevel)
        {

        }
        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.Requests.UpdateRequest"/>.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnName">
        ///     Название столбца, значение которого надо изменить.
        /// </param>
        /// <param name="columnValue">
        ///     Новое значение для столбца <paramref name="columnName"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван UPDATE.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса. Передайте пустой массив, чтобы не использовать условия.
        ///     Name - название столбца.
        ///     Value - значение столбца.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        public UpdateRequest(IRequestEngine engine, string columnName,
            string columnValue, string table, (string Name, string Value)[] conditions,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            : this(engine, columnName, columnValue, table,
                conditions, engine.CommandTimeout, isolationLevel)
        {

        }
        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.Requests.UpdateRequest"/>.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnName">
        ///     Название столбца, значение которого надо изменить.
        /// </param>
        /// <param name="columnValue">
        ///     Новое значение для столбца <paramref name="columnName"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван UPDATE.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса. Передайте пустой массив, чтобы не использовать условия.
        ///     Name - название столбца.
        ///     Value - значение столбца.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        public UpdateRequest(IRequestEngine engine, string columnName,
            string columnValue, string table, (string Name, string Value)[] conditions,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (engine == null)
            {
                var exception = new ArgumentNullException(nameof(engine), $"{nameof(engine)} cannot be null");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            Engine = engine;
            ColumnName = columnName;
            ColumnValue = columnValue;
            Table = table;
            Conditions = conditions;
            Timeout = timeout;
            IsolationLevel = isolationLevel;
            RequestCancellationToken = new CancellationTokenSource();
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с текущими параметрами.
        /// </summary>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public void Execute()
        {
            try
            {
                var task = ExecuteAsync();
                task.Wait();
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
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public async Task ExecuteAsync()
        {
            if (!Engine.CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                Engine.CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
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

                    sqlBuilder.Append("UPDATE ").Append(Table).Append(" SET ").Append(ColumnName).Append(" = @param0");

                    command.Parameters.AddWithValue("@param0", ColumnValue);

                    RequestEngineHelper.ReplaceDBNullParameterValue(ColumnValue, ref command,
                        command.Parameters[0].ParameterName);
                    RequestEngineHelper.ReplaceFunctionParameterValue(ColumnValue, ref command,
                        command.Parameters[0].ParameterName, ref sqlBuilder);

                    if (Conditions.Length != 0)
                    {
                        sqlBuilder.Append(" WHERE ");

                        sqlBuilder.Append(Conditions[0].Name).Append(" = @param1");
                        command.Parameters.AddWithValue("@param1", Conditions[0].Value);

                        RequestEngineHelper.ReplaceDBNullParameterValue(Conditions[0].Value, ref command,
                            command.Parameters[1].ParameterName);

                        for (int i = 2; i < Conditions.Length + 1; ++i)
                        {
                            int arrayIndex = i - 1;

                            sqlBuilder.Append(" AND ").Append(Conditions[arrayIndex].Name).Append(" = @param")
                                .Append(i);
                            command.Parameters.AddWithValue($"@param{i}", Conditions[arrayIndex].Value);

                            RequestEngineHelper.ReplaceDBNullParameterValue(Conditions[arrayIndex].Value, ref command,
                                command.Parameters[i].ParameterName);
                        }
                    }

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    await Engine.CommandExecuteNonQueryAsync(command, cancellationToken.Token, IsolationLevel).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder}] waiting timeout[{Timeout}] or canceled");
                    Events.OnError(this,
                        new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    Engine.CurrentMySQLConnection.OnError(this,
                        new RErrorEventArgs(exception, exception.Message, exception.StackTrace));

                    if (command?.Transaction != null)
                        await command.Transaction.RollbackAsync().ConfigureAwait(false);

                    throw exception;
                }
                catch (Exception ex)
                {
                    Events.OnError(this,
                        new RErrorEventArgs(ex, $"MySQLRequest[{sqlBuilder}] execute error - " + ex.Message, ex.StackTrace));
                    Engine.CurrentMySQLConnection.OnError(this,
                        new RErrorEventArgs(ex, $"MySQLRequest[{sqlBuilder}] execute error - " + ex.Message, ex.StackTrace));

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
                    new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                Engine.CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception, exception.Message, exception.StackTrace));

                throw exception;
            }
            catch (AggregateException ex)
                when (ex.InnerExceptions.Count == 1)
            {
                throw ex.InnerExceptions[0];
            }
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
                    new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                Engine.CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
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
        INonQueryRequest INonQueryRequest.Copy()
        {
            return Copy(Engine);
        }
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        INonQueryRequest INonQueryRequest.Copy(IRequestEngine engine)
        {
            return Copy(engine);
        }

        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        public UpdateRequest Copy()
        {
            return Copy(Engine);
        }
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        public UpdateRequest Copy(IRequestEngine engine)
        {
            return new UpdateRequest(engine, this);
        }



        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnName">
        ///     Название столбца, значение которого надо изменить.
        /// </param>
        /// <param name="columnValue">
        ///     Новое значение для столбца <paramref name="columnName"/>.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static void Execute(IRequestEngine engine,
            string columnName, string columnValue, string table,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            Execute(engine, columnName, columnValue, table,
                Array.Empty<(string name, string Value)>(),
                engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnName">
        ///     Название столбца, значение которого надо изменить.
        /// </param>
        /// <param name="columnValue">
        ///     Новое значение для столбца <paramref name="columnName"/>.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static void Execute(IRequestEngine engine,
            string columnName, string columnValue, string table, TimeSpan timeout,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            Execute(engine, columnName, columnValue, table,
                Array.Empty<(string name, string Value)>(),
                timeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnName">
        ///     Название столбца, значение которого надо изменить.
        /// </param>
        /// <param name="columnValue">
        ///     Новое значение для столбца <paramref name="columnName"/>.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task ExecuteAsync(IRequestEngine engine,
            string columnName, string columnValue, string table,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, columnName, columnValue, table,
                Array.Empty<(string name, string Value)>(),
                engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnName">
        ///     Название столбца, значение которого надо изменить.
        /// </param>
        /// <param name="columnValue">
        ///     Новое значение для столбца <paramref name="columnName"/>.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task ExecuteAsync(IRequestEngine engine,
            string columnName, string columnValue, string table, TimeSpan timeout,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, columnName, columnValue, table,
                Array.Empty<(string name, string Value)>(),
                timeout, isolationLevel);
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnName">
        ///     Название столбца, значение которого надо изменить.
        /// </param>
        /// <param name="columnValue">
        ///     Новое значение для столбца <paramref name="columnName"/>.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static void Execute(IRequestEngine engine, string columnName,
            string columnValue, string table, string conditionName, string conditionValue,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            Execute(engine, columnName, columnValue, table,
                new (string Name, string Value)[] { (conditionName, conditionValue) },
                engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnName">
        ///     Название столбца, значение которого надо изменить.
        /// </param>
        /// <param name="columnValue">
        ///     Новое значение для столбца <paramref name="columnName"/>.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static void Execute(IRequestEngine engine, string columnName,
            string columnValue, string table, string conditionName, string conditionValue,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            Execute(engine, columnName, columnValue, table,
                new (string Name, string Value)[] { (conditionName, conditionValue) },
                timeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnName">
        ///     Название столбца, значение которого надо изменить.
        /// </param>
        /// <param name="columnValue">
        ///     Новое значение для столбца <paramref name="columnName"/>.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task ExecuteAsync(IRequestEngine engine, string columnName,
            string columnValue, string table, string conditionName, string conditionValue,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, columnName, columnValue, table,
                new (string Name, string Value)[] { (conditionName, conditionValue) },
                engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnName">
        ///     Название столбца, значение которого надо изменить.
        /// </param>
        /// <param name="columnValue">
        ///     Новое значение для столбца <paramref name="columnName"/>.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task ExecuteAsync(IRequestEngine engine, string columnName,
            string columnValue, string table, string conditionName, string conditionValue,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, columnName, columnValue, table,
                new (string Name, string Value)[] { (conditionName, conditionValue) },
                timeout, isolationLevel);
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnName">
        ///     Название столбца, значение которого надо изменить.
        /// </param>
        /// <param name="columnValue">
        ///     Новое значение для столбца <paramref name="columnName"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван UPDATE.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса. Передайте пустой массив, чтобы не использовать условия.
        ///     Name - название столбца.
        ///     Value - значение столбца.
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
        public static void Execute(IRequestEngine engine, string columnName,
            string columnValue, string table, (string Name, string Value)[] conditions,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            Execute(engine, columnName, columnValue, table,
                conditions, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnName">
        ///     Название столбца, значение которого надо изменить.
        /// </param>
        /// <param name="columnValue">
        ///     Новое значение для столбца <paramref name="columnName"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван UPDATE.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса. Передайте пустой массив, чтобы не использовать условия.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static void Execute(IRequestEngine engine, string columnName,
            string columnValue, string table, (string Name, string Value)[] conditions,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            new UpdateRequest(engine, columnName, columnValue, table,
                conditions, timeout, isolationLevel).Execute();
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnName">
        ///     Название столбца, значение которого надо изменить.
        /// </param>
        /// <param name="columnValue">
        ///     Новое значение для столбца <paramref name="columnName"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван UPDATE.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса. Передайте пустой массив, чтобы не использовать условия.
        ///     Name - название столбца.
        ///     Value - значение столбца.
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
        public static Task ExecuteAsync(IRequestEngine engine, string columnName,
            string columnValue, string table, (string Name, string Value)[] conditions,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, columnName, columnValue, table,
                conditions, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnName">
        ///     Название столбца, значение которого надо изменить.
        /// </param>
        /// <param name="columnValue">
        ///     Новое значение для столбца <paramref name="columnName"/>.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван UPDATE.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса. Передайте пустой массив, чтобы не использовать условия.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task ExecuteAsync(IRequestEngine engine, string columnName,
            string columnValue, string table, (string Name, string Value)[] conditions,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new UpdateRequest(engine, columnName, columnValue, table,
                conditions, timeout, isolationLevel).ExecuteAsync();
        }
    }
}
