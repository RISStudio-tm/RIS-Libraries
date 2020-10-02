// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;
using RIS.Extensions;

namespace RIS.Connection.MySQL.Requests
{
    /// <summary>
    ///     Представляет запрос к MySQL базе данных. Этот класс не может быть унаследован.
    /// </summary>
    public sealed class SelectColumnsOneTableRequest : IQueryRequest<string[][]>
    {
        /// <summary>
        ///     Позволяет получать экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </summary>
        public IRequestEngine Engine { get; }
        /// <summary>
        ///     Позволяет получать или устанавливать массив названий столбцов, значения которых надо получить.
        /// </summary>
        public string[] ColumnsNames { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать таблицу, для которой будет вызван SELECT.
        /// </summary>
        public string Table { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать номер строки, с которой начинается считывание (все строки до этой будут пропущены). Передайте 0 или 1, чтобы начать с первой строки.
        /// </summary>
        public ulong NumberStartRow { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать количество строк, которые будут считаны. Передайте 0, чтобы считать все строки, начиная с начальной строки <see cref="NumberStartRow"/>.
        /// </summary>
        public ulong CountRows { get; set; }
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

        private SelectColumnsOneTableRequest(IRequestEngine engine, SelectColumnsOneTableRequest request)
            : this(engine, request.ColumnsNames, request.Table, request.NumberStartRow,
                request.CountRows, request.Timeout, request.IsolationLevel)
        {

        }
        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.Requests.SelectColumnsOneTableRequest"/>.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnsNames">
        ///     Массив названий столбцов, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Передайте 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Передайте 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        public SelectColumnsOneTableRequest(IRequestEngine engine, string[] columnsNames,
            string table, ulong numberStartRow, ulong countRows,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            : this(engine, columnsNames, table, numberStartRow,
                countRows, engine.CommandTimeout, isolationLevel)
        {

        }
        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.Requests.SelectColumnsOneTableRequest"/>.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnsNames">
        ///     Массив названий столбцов, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Передайте 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Передайте 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        public SelectColumnsOneTableRequest(IRequestEngine engine, string[] columnsNames,
            string table, ulong numberStartRow, ulong countRows, TimeSpan timeout,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (engine == null)
            {
                var exception = new ArgumentNullException(nameof(engine), $"{nameof(engine)} cannot be null");
                Events.OnError(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            Engine = engine;
            ColumnsNames = columnsNames;
            Table = table;
            NumberStartRow = numberStartRow;
            CountRows = countRows;
            Timeout = timeout;
            IsolationLevel = isolationLevel;
            RequestCancellationToken = new CancellationTokenSource();
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с текущими параметрами.
        /// </summary>
        /// <returns>
        ///     Массив массивов типа <see cref="string"/>, которые содержат ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public string[][] Execute()
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
        ///     Массив массивов типа <see cref="string"/>, которые содержат ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public async Task<string[][]> ExecuteAsync()
        {
            string[][] result = Array.Empty<string[]>();

            if (!Engine.CurrentMySQLConnection.ConnectionComplete)
            {
                var exception = new Exception("MySQLConnection is not open");
                Events.OnError(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                Engine.CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (ColumnsNames.Length == 0)
            {
                var exception = new ArgumentException("Count of columns names is 0");
                Events.OnError(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                Engine.CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(RequestCancellationToken.Token, Engine.GlobalCancellationToken.Token);
            Task<Task> request = Task.Run(async () =>
            {
                string sql = string.Empty;
                MySqlDataAdapter adapter = null;

                try
                {
                    DataSet data = new DataSet();

                    sql = $"SELECT {string.Join(", ", ColumnsNames)} FROM {Table}";

                    if (NumberStartRow > 0)
                        --NumberStartRow;
                    if (CountRows == 0)
                        CountRows = ulong.MaxValue;

                    sql += $" LIMIT {NumberStartRow},{CountRows}";

                    adapter = new MySqlDataAdapter(new MySqlCommand(sql))
                    {
                        SelectCommand = { CommandTimeout = (int)Timeout.TotalSeconds + 3 }
                    };

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    data = await Engine.CommandExecuteAdapterAsync(adapter, cancellationToken.Token, IsolationLevel).ConfigureAwait(false);

                    result = new string[data.Tables[0].Columns.Count][];
                    for (int i = 0; i < data.Tables[0].Columns.Count; ++i)
                    {
                        result[i] = new string[data.Tables[0].Rows.Count];
                        for (int j = 0; j < data.Tables[0].Rows.Count; ++j)
                        {
                            result[i][j] = data.Tables[0].Rows[j][ColumnsNames[i]].ToString();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sql}] waiting timeout[{Timeout}] or canceled");
                    Events.OnError(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));
                    Engine.CurrentMySQLConnection.OnError(this,
                        new RErrorEventArgs(exception.Message, exception.StackTrace));

                    if (adapter?.SelectCommand?.Transaction != null)
                        await adapter.SelectCommand.Transaction.RollbackAsync().ConfigureAwait(false);

                    throw exception;
                }
                catch (Exception ex)
                {
                    Events.OnError(this,
                        new RErrorEventArgs($"MySQLRequest[{sql}] execute error - " + ex.Message, ex.StackTrace));
                    Engine.CurrentMySQLConnection.OnError(this,
                        new RErrorEventArgs(ex.Message, ex.StackTrace));

                    if (adapter?.SelectCommand?.Transaction != null)
                        await adapter.SelectCommand.Transaction.RollbackAsync().ConfigureAwait(false);

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
        IQueryRequest<string[][]> IQueryRequest<string[][]>.Copy()
        {
            return Copy(Engine);
        }
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        IQueryRequest<string[][]> IQueryRequest<string[][]>.Copy(IRequestEngine engine)
        {
            return Copy(engine);
        }

        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        public SelectColumnsOneTableRequest Copy()
        {
            return Copy(Engine);
        }
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        public SelectColumnsOneTableRequest Copy(IRequestEngine engine)
        {
            return new SelectColumnsOneTableRequest(engine, this);
        }



        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnsNames">
        ///     Массив названий столбцов, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Передайте 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Передайте 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив массивов типа <see cref="string"/>, которые содержат ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static string[][] Execute(IRequestEngine engine,
            string[] columnsNames, string table, ulong numberStartRow, ulong countRows,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, columnsNames, table, numberStartRow,
                countRows, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnsNames">
        ///     Массив названий столбцов, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Передайте 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Передайте 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static string[][] Execute(IRequestEngine engine,
            string[] columnsNames, string table, ulong numberStartRow, ulong countRows,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new SelectColumnsOneTableRequest(engine, columnsNames, table, numberStartRow,
                countRows, timeout, isolationLevel).Execute();
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnsNames">
        ///     Массив названий столбцов, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Передайте 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Передайте 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив массивов типа <see cref="string"/>, которые содержат ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<string[][]> ExecuteAsync(IRequestEngine engine,
            string[] columnsNames, string table, ulong numberStartRow, ulong countRows,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, columnsNames, table, numberStartRow,
                countRows, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columnsNames">
        ///     Массив названий столбцов, значения которых надо получить.
        /// </param>
        /// <param name="table">
        ///     Таблица, для которой будет вызван SELECT.
        /// </param>
        /// <param name="numberStartRow">
        ///     Номер строки, с которой начинается считывание (все строки до этой будут пропущены). Передайте 0 или 1, чтобы начать с первой строки.
        /// </param>
        /// <param name="countRows">
        ///     Количество строк, которые будут считаны. Передайте 0, чтобы считать все строки, начиная с начальной строки <paramref name="numberStartRow"/>.
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
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<string[][]> ExecuteAsync(IRequestEngine engine,
            string[] columnsNames, string table, ulong numberStartRow, ulong countRows,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new SelectColumnsOneTableRequest(engine, columnsNames, table, numberStartRow,
                countRows, timeout, isolationLevel).ExecuteAsync();
        }
    }
}
