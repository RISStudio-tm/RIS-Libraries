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
    public sealed class SelectColumnsRequest : IQueryRequest<string[][]>
    {
        /// <summary>
        ///     Позволяет получать экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </summary>
        public IRequestEngine Engine { get; }
        /// <summary>
        ///     Позволяет получать или устанавливать массив столбцов, значения которых надо получить.
        ///     Name - название столбца.
        ///     Table - таблица, для которой будет вызван SELECT.
        ///     NumberStartRow - номер строки, с которой начнётся считывание (все строки до этой будут пропущены). Передайте 0 или 1, чтобы начать с первой строки.
        ///     CountRows - Количество строк, которые будут считаны. Передайте 0, чтобы считать все строки, начиная с начальной строки <see cref="Columns"/>.NumberStartRow.
        /// </summary>
        public (string Name, string Table, ulong NumberStartRow, ulong CountRows)[] Columns { get; set; }
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

        private SelectColumnsRequest(IRequestEngine engine, SelectColumnsRequest request)
            : this(engine, request.Columns, request.Timeout, request.IsolationLevel)
        {

        }
        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.Requests.SelectColumnsRequest"/>.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columns">
        ///     Массив столбцов, значения которых надо получить.
        ///     Name - название столбца.
        ///     Table - таблица, для которой будет вызван SELECT.
        ///     NumberStartRow - номер строки, с которой начнётся считывание (все строки до этой будут пропущены). Передайте 0 или 1, чтобы начать с первой строки.
        ///     CountRows - Количество строк, которые будут считаны. Передайте 0, чтобы считать все строки, начиная с начальной строки <paramref name="columns"/>.NumberStartRow.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        public SelectColumnsRequest(IRequestEngine engine,
            (string Name, string Table, ulong NumberStartRow, ulong CountRows)[] columns,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            : this(engine, columns, engine.CommandTimeout, isolationLevel)
        {

        }
        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.Requests.SelectColumnsRequest"/>.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columns">
        ///     Массив столбцов, значения которых надо получить.
        ///     Name - название столбца.
        ///     Table - таблица, для которой будет вызван SELECT.
        ///     NumberStartRow - номер строки, с которой начнётся считывание (все строки до этой будут пропущены). Передайте 0 или 1, чтобы начать с первой строки.
        ///     CountRows - Количество строк, которые будут считаны. Передайте 0, чтобы считать все строки, начиная с начальной строки <paramref name="columns"/>.NumberStartRow.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        public SelectColumnsRequest(IRequestEngine engine,
            (string Name, string Table, ulong NumberStartRow, ulong CountRows)[] columns,
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
            Columns = columns;
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
                    new RErrorEventArgs(exception, exception.Message));
                Engine.CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (Columns.Length == 0)
            {
                var exception = new ArgumentException("Count of columns is 0");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                Engine.CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            CancellationTokenSource cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(RequestCancellationToken.Token, Engine.GlobalCancellationToken.Token);
            Task<Task> request = Task.Run(async () =>
            {
                MySqlDataAdapter adapter = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    DataSet data = new DataSet();

                    adapter = new MySqlDataAdapter(new MySqlCommand(sqlBuilder.ToString()))
                    {
                        SelectCommand = { CommandTimeout = (int)Timeout.TotalSeconds + 3 }
                    };

                    for (int i = 0; i < Columns.Length; ++i)
                    {
                        sqlBuilder.Append("SELECT ").Append(Columns[i].Name).Append(" FROM ").Append(Columns[i].Table);

                        if (Columns[i].NumberStartRow > 0)
                            --Columns[i].NumberStartRow;
                        if (Columns[i].CountRows == 0)
                            Columns[i].CountRows = ulong.MaxValue;

                        sqlBuilder.Append(" LIMIT ").Append(Columns[i].NumberStartRow).Append(',').Append(Columns[i].CountRows).Append("; ");
                    }

                    adapter.SelectCommand.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    data = await Engine.CommandExecuteAdapterAsync(adapter, cancellationToken.Token, IsolationLevel).ConfigureAwait(false);

                    result = new string[data.Tables.Count][];
                    for (int i = 0; i < data.Tables.Count; ++i)
                    {
                        result[i] = new string[data.Tables[i].Rows.Count];
                        for (int j = 0; j < data.Tables[i].Rows.Count; ++j)
                        {
                            result[i][j] = data.Tables[i].Rows[j][Columns[i].Name].ToString();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder}] waiting timeout[{Timeout}] or canceled");
                    Events.OnError(this,
                        new RErrorEventArgs(exception, exception.Message));
                    Engine.CurrentMySQLConnection.OnError(this,
                        new RErrorEventArgs(exception, exception.Message));

                    if (adapter?.SelectCommand?.Transaction != null)
                        await adapter.SelectCommand.Transaction.RollbackAsync().ConfigureAwait(false);

                    throw exception;
                }
                catch (Exception ex)
                {
                    Events.OnError(this,
                        new RErrorEventArgs(ex, $"MySQLRequest[{sqlBuilder}] execute error - " + ex.Message));
                    Engine.CurrentMySQLConnection.OnError(this,
                        new RErrorEventArgs(ex, $"MySQLRequest[{sqlBuilder}] execute error - " + ex.Message));

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
        public SelectColumnsRequest Copy()
        {
            return Copy(Engine);
        }
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        public SelectColumnsRequest Copy(IRequestEngine engine)
        {
            return new SelectColumnsRequest(engine, this);
        }



        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columns">
        ///     Массив столбцов, значения которых надо получить.
        ///     Name - название столбца.
        ///     Table - таблица, для которой будет вызван SELECT.
        ///     NumberStartRow - номер строки, с которой начнётся считывание (все строки до этой будут пропущены). Передайте 0 или 1, чтобы начать с первой строки.
        ///     CountRows - Количество строк, которые будут считаны. Передайте 0, чтобы считать все строки, начиная с начальной строки <paramref name="columns"/>.NumberStartRow.
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
            (string Name, string Table, ulong NumberStartRow, ulong CountRows)[] columns,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, columns, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columns">
        ///     Массив столбцов, значения которых надо получить.
        ///     Name - название столбца.
        ///     Table - таблица, для которой будет вызван SELECT.
        ///     NumberStartRow - номер строки, с которой начнётся считывание (все строки до этой будут пропущены). Передайте 0 или 1, чтобы начать с первой строки.
        ///     CountRows - Количество строк, которые будут считаны. Передайте 0, чтобы считать все строки, начиная с начальной строки <paramref name="columns"/>.NumberStartRow.
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
            (string Name, string Table, ulong NumberStartRow, ulong CountRows)[] columns,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new SelectColumnsRequest(engine, columns, timeout, isolationLevel).Execute();
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columns">
        ///     Массив столбцов, значения которых надо получить.
        ///     Name - название столбца.
        ///     Table - таблица, для которой будет вызван SELECT.
        ///     NumberStartRow - номер строки, с которой начнётся считывание (все строки до этой будут пропущены). Передайте 0 или 1, чтобы начать с первой строки.
        ///     CountRows - Количество строк, которые будут считаны. Передайте 0, чтобы считать все строки, начиная с начальной строки <paramref name="columns"/>.NumberStartRow.
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
            (string Name, string Table, ulong NumberStartRow, ulong CountRows)[] columns,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, columns, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="columns">
        ///     Массив столбцов, значения которых надо получить.
        ///     Name - название столбца.
        ///     Table - таблица, для которой будет вызван SELECT.
        ///     NumberStartRow - номер строки, с которой начнётся считывание (все строки до этой будут пропущены). Передайте 0 или 1, чтобы начать с первой строки.
        ///     CountRows - Количество строк, которые будут считаны. Передайте 0, чтобы считать все строки, начиная с начальной строки <paramref name="columns"/>.NumberStartRow.
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
            (string Name, string Table, ulong NumberStartRow, ulong CountRows)[] columns,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new SelectColumnsRequest(engine, columns, timeout, isolationLevel).ExecuteAsync();
        }
    }
}
