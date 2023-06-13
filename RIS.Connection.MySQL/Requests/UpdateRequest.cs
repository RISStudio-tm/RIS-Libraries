// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;
using RIS.Connection.MySQL.Builders;
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
        private MySQLConditionBuilder _conditions;
        /// <summary>
        ///     Позволяет получать или устанавливать экземпляр класса <see cref="RIS.Connection.MySQL.Builders.MySQLConditionBuilder"/>, который используется для формирования условий для запроса. Передайте пустой или null, чтобы не использовать условия.
        /// </summary>
        public MySQLConditionBuilder Conditions
        {
            get
            {
                if (_conditions == null
                    || _conditions == MySQLConditionBuilder.Empty)
                {
                    _conditions = MySQLConditionBuilder.Empty
                        .Copy();
                }

                return _conditions;
            }
            set
            {
                _conditions = value;
            }
        }
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
        ///     Экземпляр класса <see cref="RIS.Connection.MySQL.Builders.MySQLConditionBuilder"/>, который используется для формирования условий для запроса. Передайте пустой или null, чтобы не использовать условия.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        public UpdateRequest(IRequestEngine engine, string columnName,
            string columnValue, string table, MySQLConditionBuilder conditions,
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
        ///     Экземпляр класса <see cref="RIS.Connection.MySQL.Builders.MySQLConditionBuilder"/>, который используется для формирования условий для запроса. Передайте пустой или null, чтобы не использовать условия.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        public UpdateRequest(IRequestEngine engine, string columnName,
            string columnValue, string table, MySQLConditionBuilder conditions,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (engine == null)
            {
                var exception = new ArgumentNullException(nameof(engine), $"{nameof(engine)} cannot be null");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
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
                    new RErrorEventArgs(exception, exception.Message));
                Engine.CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            var constructedConditions = Conditions.Build();

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

                    if (!Conditions.IsEmpty())
                    {
                        sqlBuilder.Append(constructedConditions.Sql);

                        foreach (var parameter in constructedConditions.Parameters)
                        {
                            command.Parameters.AddWithValue(parameter.Name, parameter.Value);
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
                        new RErrorEventArgs(exception, exception.Message));
                    Engine.CurrentMySQLConnection.OnError(this,
                        new RErrorEventArgs(exception, exception.Message));

                    if (command?.Transaction != null)
                        await command.Transaction.RollbackAsync().ConfigureAwait(false);

                    throw exception;
                }
                catch (Exception ex)
                {
                    Events.OnError(this,
                        new RErrorEventArgs(ex, $"MySQLRequest[{sqlBuilder}] execute error - " + ex.Message));
                    Engine.CurrentMySQLConnection.OnError(this,
                        new RErrorEventArgs(ex, $"MySQLRequest[{sqlBuilder}] execute error - " + ex.Message));

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
                    new RErrorEventArgs(exception, exception.Message));
                Engine.CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));

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
                    new RErrorEventArgs(exception, exception.Message));
                Engine.CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
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
                MySQLConditionBuilder.Empty,
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
                MySQLConditionBuilder.Empty,
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
                MySQLConditionBuilder.Empty,
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
                MySQLConditionBuilder.Empty,
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
        ///     Экземпляр класса <see cref="RIS.Connection.MySQL.Builders.MySQLConditionBuilder"/>, который используется для формирования условий для запроса. Передайте пустой или null, чтобы не использовать условия.
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
            string columnValue, string table, MySQLConditionBuilder conditions,
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
        ///     Экземпляр класса <see cref="RIS.Connection.MySQL.Builders.MySQLConditionBuilder"/>, который используется для формирования условий для запроса. Передайте пустой или null, чтобы не использовать условия.
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
            string columnValue, string table, MySQLConditionBuilder conditions,
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
        ///    Экземпляр класса <see cref="RIS.Connection.MySQL.Builders.MySQLConditionBuilder"/>, который используется для формирования условий для запроса. Передайте пустой или null, чтобы не использовать условия.
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
            string columnValue, string table, MySQLConditionBuilder conditions,
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
        ///     Экземпляр класса <see cref="RIS.Connection.MySQL.Builders.MySQLConditionBuilder"/>, который используется для формирования условий для запроса. Передайте пустой или null, чтобы не использовать условия.
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
            string columnValue, string table, MySQLConditionBuilder conditions,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new UpdateRequest(engine, columnName, columnValue, table,
                conditions, timeout, isolationLevel).ExecuteAsync();
        }
    }
}
