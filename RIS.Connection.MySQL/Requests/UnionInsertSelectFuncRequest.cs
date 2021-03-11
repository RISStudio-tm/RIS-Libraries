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
    public sealed class UnionInsertSelectFuncRequest : IQueryRequest<string>
    {
        /// <summary>
        ///     Позволяет получать экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </summary>
        public IRequestEngine Engine { get; }
        /// <summary>
        ///     Позволяет получать или устанавливать массив значений столбцов по порядку их следования в таблице <see cref="TableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </summary>
        public string[] ValuesInsert { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать таблицу, для которой будет вызван INSERT.
        /// </summary>
        public string TableInsert { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать название функции.
        /// </summary>
        public string NameFunction { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать массив значений параметров функции. Передайте пустой массив, чтобы не использовать параметры.
        /// </summary>
        public string[] ParametersValuesFunction { get; set; }
        /// <summary>
        ///     Таблица, для которой будет вызван SELECT. Передайте null или пустую строку, чтобы не использовать привязку к таблице.
        /// </summary>
        public string TableFunction { get; set; }
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

        private UnionInsertSelectFuncRequest(IRequestEngine engine, UnionInsertSelectFuncRequest request)
            : this(engine, request.ValuesInsert, request.TableInsert, request.NameFunction,
                request.ParametersValuesFunction, request.TableFunction, request.Timeout, request.IsolationLevel)
        {

        }
        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.Requests.UnionInsertSelectFuncRequest"/>.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Массив значений параметров функции. Передайте пустой массив, чтобы не использовать параметры.
        /// </param>
        /// <param name="tableFunction">
        ///     Таблица, для которой будет вызван SELECT. Передайте null или пустую строку, чтобы не использовать привязку к таблице.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        public UnionInsertSelectFuncRequest(IRequestEngine engine, string[] valuesInsert,
            string tableInsert, string nameFunction, string[] parametersValuesFunction, string tableFunction,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            : this(engine, valuesInsert, tableInsert, nameFunction, parametersValuesFunction,
                tableFunction, engine.CommandTimeout, isolationLevel)
        {

        }
        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.Requests.UnionInsertSelectFuncRequest"/>.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Массив значений параметров функции. Передайте пустой массив, чтобы не использовать параметры.
        /// </param>
        /// <param name="tableFunction">
        ///     Таблица, для которой будет вызван SELECT. Передайте null или пустую строку, чтобы не использовать привязку к таблице.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        public UnionInsertSelectFuncRequest(IRequestEngine engine, string[] valuesInsert,
            string tableInsert, string nameFunction, string[] parametersValuesFunction, string tableFunction,
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
            ValuesInsert = valuesInsert;
            TableInsert = tableInsert;
            NameFunction = nameFunction;
            ParametersValuesFunction = parametersValuesFunction;
            TableFunction = tableFunction;
            Timeout = timeout;
            IsolationLevel = isolationLevel;
            RequestCancellationToken = new CancellationTokenSource();
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с текущими параметрами.
        /// </summary>
        /// <returns>
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public string Execute()
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
        ///     Значение типа <see cref="string"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public async Task<string> ExecuteAsync()
        {
            string result = string.Empty;

            if (!Engine.CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                Engine.CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
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
                    command = new MySqlCommand(sqlBuilder.ToString())
                    {
                        CommandTimeout = (int)Timeout.TotalSeconds + 3
                    };

                    sqlBuilder.Append("INSERT INTO ").Append(TableInsert).Append(" VALUES (");

                    if (ValuesInsert.Length != 0)
                    {
                        sqlBuilder.Append("@param0");
                        command.Parameters.AddWithValue("@param0", ValuesInsert[0]);

                        RequestEngineHelper.ReplaceDBNullParameterValue(ValuesInsert[0],
                            ref command, command.Parameters[0].ParameterName);
                        RequestEngineHelper.ReplaceFunctionParameterValue(ValuesInsert[0],
                            ref command, command.Parameters[0].ParameterName, ref sqlBuilder);

                        for (int i = 1; i < ValuesInsert.Length; ++i)
                        {
                            sqlBuilder.Append(", @param").Append(i);
                            command.Parameters.AddWithValue($"@param{i}", ValuesInsert[i]);

                            RequestEngineHelper.ReplaceDBNullParameterValue(ValuesInsert[i],
                                ref command, command.Parameters[i].ParameterName);
                            RequestEngineHelper.ReplaceFunctionParameterValue(ValuesInsert[i],
                                ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                        }
                    }

                    sqlBuilder.Append("); SELECT ").Append(NameFunction).Append('(');

                    if (ParametersValuesFunction.Length != 0)
                    {
                        sqlBuilder.Append("@param").Append(ValuesInsert.Length);
                        command.Parameters.AddWithValue($"@param{ValuesInsert.Length}", ParametersValuesFunction[0]);

                        RequestEngineHelper.ReplaceDBNullParameterValue(ParametersValuesFunction[0],
                            ref command, command.Parameters[ValuesInsert.Length].ParameterName);
                        RequestEngineHelper.ReplaceFunctionParameterValue(ParametersValuesFunction[0],
                            ref command, command.Parameters[ValuesInsert.Length].ParameterName, ref sqlBuilder);

                        for (int i = ValuesInsert.Length + 1; i < ValuesInsert.Length + ParametersValuesFunction.Length; ++i)
                        {
                            int arrayIndex = i - ValuesInsert.Length;

                            sqlBuilder.Append(", @param").Append(i);
                            command.Parameters.AddWithValue($"@param{i}", ParametersValuesFunction[arrayIndex]);

                            RequestEngineHelper.ReplaceDBNullParameterValue(ParametersValuesFunction[arrayIndex],
                                ref command, command.Parameters[i].ParameterName);
                            RequestEngineHelper.ReplaceFunctionParameterValue(ParametersValuesFunction[arrayIndex],
                                ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                        }
                    }

                    sqlBuilder.Append(')');

                    if (!string.IsNullOrEmpty(TableFunction))
                        sqlBuilder.Append(" FROM ").Append(TableFunction);

                    sqlBuilder.Append(';');

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    result = (await Engine.CommandExecuteReaderAsync(command, cancellationToken.Token, IsolationLevel).ConfigureAwait(false))[0];
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
        IQueryRequest<string> IQueryRequest<string>.Copy()
        {
            return Copy(Engine);
        }
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        IQueryRequest<string> IQueryRequest<string>.Copy(IRequestEngine engine)
        {
            return Copy(engine);
        }

        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        public UnionInsertSelectFuncRequest Copy()
        {
            return Copy(Engine);
        }
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        public UnionInsertSelectFuncRequest Copy(IRequestEngine engine)
        {
            return new UnionInsertSelectFuncRequest(engine, this);
        }



        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
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
        public static string Execute(IRequestEngine engine,
            string[] valuesInsert, string tableInsert, string nameFunction,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, valuesInsert, tableInsert, nameFunction,
                Array.Empty<string>(), null, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
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
        public static string Execute(IRequestEngine engine,
            string[] valuesInsert, string tableInsert, string nameFunction,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, valuesInsert, tableInsert, nameFunction,
                Array.Empty<string>(), null, timeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
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
        public static Task<string> ExecuteAsync(IRequestEngine engine,
            string[] valuesInsert, string tableInsert, string nameFunction,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, valuesInsert, tableInsert, nameFunction,
                Array.Empty<string>(), null, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
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
        public static Task<string> ExecuteAsync(IRequestEngine engine,
            string[] valuesInsert, string tableInsert, string nameFunction,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, valuesInsert, tableInsert, nameFunction,
                Array.Empty<string>(), null, timeout, isolationLevel);
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="tableFunction">
        ///     Таблица, для которой будет вызван SELECT. Передайте null или пустую строку, чтобы не использовать привязку к таблице.
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
        public static string Execute(IRequestEngine engine, string[] valuesInsert,
            string tableInsert, string nameFunction, string tableFunction,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, valuesInsert, tableInsert, nameFunction,
                Array.Empty<string>(), tableFunction, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="tableFunction">
        ///     Таблица, для которой будет вызван SELECT. Передайте null или пустую строку, чтобы не использовать привязку к таблице.
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
        public static string Execute(IRequestEngine engine, string[] valuesInsert,
            string tableInsert, string nameFunction, string tableFunction,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, valuesInsert, tableInsert, nameFunction,
                Array.Empty<string>(), tableFunction, timeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="tableFunction">
        ///     Таблица, для которой будет вызван SELECT. Передайте null или пустую строку, чтобы не использовать привязку к таблице.
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
        public static Task<string> ExecuteAsync(IRequestEngine engine, string[] valuesInsert,
            string tableInsert, string nameFunction, string tableFunction,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, valuesInsert, tableInsert, nameFunction,
                Array.Empty<string>(), tableFunction, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="tableFunction">
        ///     Таблица, для которой будет вызван SELECT. Передайте null или пустую строку, чтобы не использовать привязку к таблице.
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
        public static Task<string> ExecuteAsync(IRequestEngine engine, string[] valuesInsert,
            string tableInsert, string nameFunction, string tableFunction,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, valuesInsert, tableInsert, nameFunction,
                Array.Empty<string>(), tableFunction, timeout, isolationLevel);
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Массив значений параметров функции. Передайте пустой массив, чтобы не использовать параметры.
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
        public static string Execute(IRequestEngine engine, string[] valuesInsert,
            string tableInsert, string nameFunction, string[] parametersValuesFunction,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, valuesInsert, tableInsert, nameFunction,
                parametersValuesFunction, null, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Массив значений параметров функции. Передайте пустой массив, чтобы не использовать параметры.
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
        public static string Execute(IRequestEngine engine, string[] valuesInsert,
            string tableInsert, string nameFunction, string[] parametersValuesFunction,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, valuesInsert, tableInsert, nameFunction,
                parametersValuesFunction, null, timeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Массив значений параметров функции. Передайте пустой массив, чтобы не использовать параметры.
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
        public static Task<string> ExecuteAsync(IRequestEngine engine, string[] valuesInsert,
            string tableInsert, string nameFunction, string[] parametersValuesFunction,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, valuesInsert, tableInsert, nameFunction,
                parametersValuesFunction, null, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Массив значений параметров функции. Передайте пустой массив, чтобы не использовать параметры.
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
        public static Task<string> ExecuteAsync(IRequestEngine engine, string[] valuesInsert,
            string tableInsert, string nameFunction, string[] parametersValuesFunction,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, valuesInsert, tableInsert, nameFunction,
                parametersValuesFunction, null, timeout, isolationLevel);
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Массив значений параметров функции. Передайте пустой массив, чтобы не использовать параметры.
        /// </param>
        /// <param name="tableFunction">
        ///     Таблица, для которой будет вызван SELECT. Передайте null или пустую строку, чтобы не использовать привязку к таблице.
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
        public static string Execute(IRequestEngine engine, string[] valuesInsert,
            string tableInsert, string nameFunction, string[] parametersValuesFunction,
            string tableFunction, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, valuesInsert, tableInsert, nameFunction,
                parametersValuesFunction, tableFunction, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Массив значений параметров функции. Передайте пустой массив, чтобы не использовать параметры.
        /// </param>
        /// <param name="tableFunction">
        ///     Таблица, для которой будет вызван SELECT. Передайте null или пустую строку, чтобы не использовать привязку к таблице.
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
        public static string Execute(IRequestEngine engine, string[] valuesInsert,
            string tableInsert, string nameFunction, string[] parametersValuesFunction,
            string tableFunction, TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new UnionInsertSelectFuncRequest(engine, valuesInsert, tableInsert, nameFunction,
                parametersValuesFunction, tableFunction, timeout, isolationLevel).Execute();
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Массив значений параметров функции. Передайте пустой массив, чтобы не использовать параметры.
        /// </param>
        /// <param name="tableFunction">
        ///     Таблица, для которой будет вызван SELECT. Передайте null или пустую строку, чтобы не использовать привязку к таблице.
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
        public static Task<string> ExecuteAsync(IRequestEngine engine, string[] valuesInsert,
            string tableInsert, string nameFunction, string[] parametersValuesFunction,
            string tableFunction, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, valuesInsert, tableInsert, nameFunction,
                parametersValuesFunction, tableFunction, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="valuesInsert">
        ///     Массив значений столбцов по порядку их следования в таблице <paramref name="tableInsert"/>. Передайте пустой массив, чтобы использовать значения по умолчанию для всех столбцов.
        /// </param>
        /// <param name="tableInsert">
        ///     Таблица, для которой будет вызван INSERT.
        /// </param>
        /// <param name="nameFunction">
        ///     Название функции.
        /// </param>
        /// <param name="parametersValuesFunction">
        ///     Массив значений параметров функции. Передайте пустой массив, чтобы не использовать параметры.
        /// </param>
        /// <param name="tableFunction">
        ///     Таблица, для которой будет вызван SELECT. Передайте null или пустую строку, чтобы не использовать привязку к таблице.
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
        public static Task<string> ExecuteAsync(IRequestEngine engine, string[] valuesInsert,
            string tableInsert, string nameFunction, string[] parametersValuesFunction,
            string tableFunction, TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new UnionInsertSelectFuncRequest(engine, valuesInsert, tableInsert, nameFunction,
                parametersValuesFunction, tableFunction, timeout, isolationLevel).ExecuteAsync();
        }
    }
}
