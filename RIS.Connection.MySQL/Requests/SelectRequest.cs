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
    public sealed class SelectRequest : IQueryRequest<string[]>
    {
        /// <summary>
        ///     Позволяет получать экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </summary>
        public IRequestEngine Engine { get; }
        /// <summary>
        ///     Позволяет получать или устанавливать массив названий полей, значения которых надо получить.
        /// </summary>
        public string[] FieldsNames { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать таблицу, для которой будет вызван SELECT.
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

        private SelectRequest(IRequestEngine engine, SelectRequest request)
            : this(engine, request.FieldsNames, request.Table, request.Conditions,
                request.Timeout, request.IsolationLevel)
        {

        }
        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.Requests.SelectRequest"/>.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="fieldsNames">
        ///     Массив названий полей, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="conditions">
        ///     Массив условий для запроса. Передайте пустой массив, чтобы не использовать условия.
        ///     Name - название столбца.
        ///     Value - значение столбца.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        public SelectRequest(IRequestEngine engine, string[] fieldsNames,
            string table, (string Name, string Value)[] conditions,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            : this(engine, fieldsNames, table, conditions, engine.CommandTimeout, isolationLevel)
        {

        }
        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.Requests.SelectRequest"/>.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="fieldsNames">
        ///     Массив названий полей, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
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
        public SelectRequest(IRequestEngine engine, string[] fieldsNames,
            string table, (string Name, string Value)[] conditions, TimeSpan timeout,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (engine == null)
            {
                var exception = new ArgumentNullException(nameof(engine), $"{nameof(engine)} cannot be null");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            Engine = engine;
            FieldsNames = fieldsNames;
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
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public string[] Execute()
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
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public async Task<string[]> ExecuteAsync()
        {
            string[] result = Array.Empty<string>();

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
                string sql = string.Empty;
                MySqlCommand command = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sql)
                    {
                        CommandTimeout = (int)Timeout.TotalSeconds + 3
                    };

                    sqlBuilder.Append("SELECT ").Append(string.Join(", ", FieldsNames)).Append(" FROM ").Append(Table);

                    if (Conditions.Length != 0)
                    {
                        sqlBuilder.Append(" WHERE ");

                        sqlBuilder.Append(Conditions[0].Name).Append(" = @param0");
                        command.Parameters.AddWithValue("@param0", Conditions[0].Value);

                        RequestEngineHelper.ReplaceDBNullParameterValue(Conditions[0].Value, ref command,
                            command.Parameters[0].ParameterName);

                        for (int i = 1; i < Conditions.Length; ++i)
                        {
                            sqlBuilder.Append(" AND ").Append(Conditions[i].Name).Append(" = @param").Append(i);
                            command.Parameters.AddWithValue($"@param{i}", Conditions[i].Value);

                            RequestEngineHelper.ReplaceDBNullParameterValue(Conditions[i].Value, ref command,
                                command.Parameters[i].ParameterName);
                        }
                    }

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = await Engine.CommandExecuteReaderAsync(command, cancellationToken.Token, IsolationLevel).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{Timeout}] or canceled");
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
                        new RErrorEventArgs(ex, $"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
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
        IQueryRequest<string[]> IQueryRequest<string[]>.Copy()
        {
            return Copy(Engine);
        }
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        IQueryRequest<string[]> IQueryRequest<string[]>.Copy(IRequestEngine engine)
        {
            return Copy(engine);
        }

        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        public SelectRequest Copy()
        {
            return Copy(Engine);
        }
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        public SelectRequest Copy(IRequestEngine engine)
        {
            return new SelectRequest(engine, this);
        }



        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="fieldsNames">
        ///     Массив названий полей, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static string[] Execute(IRequestEngine engine, string[] fieldsNames,
            string table, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, fieldsNames, table,
                Array.Empty<(string Name, string Value)>(),
                engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="fieldsNames">
        ///     Массив названий полей, значения которых надо получить.
        /// </param>
        /// <param name="table">
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static string[] Execute(IRequestEngine engine, string[] fieldsNames,
            string table, TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, fieldsNames, table,
                Array.Empty<(string Name, string Value)>(),
                timeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="fieldsNames">
        ///     Массив названий полей, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<string[]> ExecuteAsync(IRequestEngine engine, string[] fieldsNames,
            string table, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, fieldsNames, table,
                Array.Empty<(string Name, string Value)>(),
                engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="fieldsNames">
        ///     Массив названий полей, значения которых надо получить.
        /// </param>
        /// <param name="table">
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<string[]> ExecuteAsync(IRequestEngine engine, string[] fieldsNames,
            string table, TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, fieldsNames, table,
                Array.Empty<(string Name, string Value)>(),
                timeout, isolationLevel);
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="fieldsNames">
        ///     Массив названий полей, значения которых надо получить.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static string[] Execute(IRequestEngine engine, string[] fieldsNames,
            string table, string conditionName, string conditionValue,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, fieldsNames, table,
                new (string Name, string Value)[] { (conditionName, conditionValue) },
                engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="fieldsNames">
        ///     Массив названий полей, значения которых надо получить.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static string[] Execute(IRequestEngine engine, string[] fieldsNames,
            string table, string conditionName, string conditionValue, TimeSpan timeout,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, fieldsNames, table,
                new (string Name, string Value)[] { (conditionName, conditionValue) },
                timeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="fieldsNames">
        ///     Массив названий полей, значения которых надо получить.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<string[]> ExecuteAsync(IRequestEngine engine, string[] fieldsNames,
            string table, string conditionName, string conditionValue,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, fieldsNames, table,
                new (string Name, string Value)[] { (conditionName, conditionValue) },
                engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="fieldsNames">
        ///     Массив названий полей, значения которых надо получить.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<string[]> ExecuteAsync(IRequestEngine engine, string[] fieldsNames,
            string table, string conditionName, string conditionValue, TimeSpan timeout,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, fieldsNames, table,
                new (string Name, string Value)[] { (conditionName, conditionValue) },
                timeout, isolationLevel);
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="fieldsNames">
        ///     Массив названий полей, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
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
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static string[] Execute(IRequestEngine engine, string[] fieldsNames,
            string table, (string Name, string Value)[] conditions,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, fieldsNames, table, conditions,
                engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="fieldsNames">
        ///     Массив названий полей, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
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
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static string[] Execute(IRequestEngine engine, string[] fieldsNames,
            string table, (string Name, string Value)[] conditions, TimeSpan timeout,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new SelectRequest(engine, fieldsNames, table, conditions,
                timeout, isolationLevel).Execute();
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="fieldsNames">
        ///     Массив названий полей, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
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
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<string[]> ExecuteAsync(IRequestEngine engine, string[] fieldsNames,
            string table, (string Name, string Value)[] conditions,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, fieldsNames, table, conditions,
                engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="fieldsNames">
        ///     Массив названий полей, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
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
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<string[]> ExecuteAsync(IRequestEngine engine, string[] fieldsNames,
            string table, (string Name, string Value)[] conditions, TimeSpan timeout,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new SelectRequest(engine, fieldsNames, table, conditions,
                timeout, isolationLevel).ExecuteAsync();
        }
    }
}
