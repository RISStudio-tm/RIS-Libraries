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
    public sealed class StoredProcedureRequest : IQueryRequest<(long Result, string[] OutputValues)>
    {
        /// <summary>
        ///     Позволяет получать экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </summary>
        public IRequestEngine Engine { get; }
        /// <summary>
        ///     Позволяет получать или устанавливать название процедуры.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать массив входных параметров процедуры. Передайте пустой массив, чтобы не использовать входные параметры.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </summary>
        public (string Name, string Value)[] InParameters { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать массив названий выходных параметров процедуры. Передайте пустой массив, чтобы не использовать выходные параметры.
        /// </summary>
        public string[] OutParametersNames { get; set; }
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

        private StoredProcedureRequest(IRequestEngine engine, StoredProcedureRequest request)
            : this(engine, request.Name, request.InParameters, request.OutParametersNames,
                request.Timeout, request.IsolationLevel)
        {

        }
        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.Requests.StoredProcedureRequest"/>.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="inParameters">
        ///     Массив входных параметров процедуры. Передайте пустой массив, чтобы не использовать входные параметры.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="outParametersNames">
        ///     Массив названий выходных параметров процедуры. Передайте пустой массив, чтобы не использовать выходные параметры.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        public StoredProcedureRequest(IRequestEngine engine, string name,
            (string Name, string Value)[] inParameters, string[] outParametersNames,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            : this(engine, name, inParameters, outParametersNames,
                engine.CommandTimeout, isolationLevel)
        {

        }
        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.Requests.StoredProcedureRequest"/>.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="inParameters">
        ///     Массив входных параметров процедуры. Передайте пустой массив, чтобы не использовать входные параметры.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="outParametersNames">
        ///     Массив названий выходных параметров процедуры. Передайте пустой массив, чтобы не использовать выходные параметры.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        public StoredProcedureRequest(IRequestEngine engine, string name,
            (string Name, string Value)[] inParameters, string[] outParametersNames,
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
            Name = name;
            InParameters = inParameters;
            OutParametersNames = outParametersNames;
            Timeout = timeout;
            IsolationLevel = isolationLevel;
            RequestCancellationToken = new CancellationTokenSource();
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с текущими параметрами.
        /// </summary>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public (long Result, string[] OutputValues) Execute()
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
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public async Task<(long Result, string[] OutputValues)> ExecuteAsync()
        {
            long result = 0;
            string[] outputValuesTemp = Array.Empty<string>();

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
                MySqlCommand command = null;

                StringBuilder sqlBuilder = new StringBuilder();

                try
                {
                    command = new MySqlCommand(sqlBuilder.ToString())
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandTimeout = (int)Timeout.TotalSeconds + 3
                    };

                    sqlBuilder.Append(Name);

                    for (int i = 0; i < InParameters.Length; ++i)
                    {
                        command.Parameters.AddWithValue(InParameters[i].Name, InParameters[i].Value);
                        command.Parameters[i].Direction = ParameterDirection.Input;

                        RequestEngineHelper.ReplaceDBNullParameterValue(InParameters[i].Value,
                            ref command, command.Parameters[i].ParameterName);
                        RequestEngineHelper.ReplaceFunctionParameterValue(InParameters[i].Value,
                            ref command, command.Parameters[i].ParameterName, ref sqlBuilder);
                    }

                    for (int i = InParameters.Length; i < InParameters.Length + OutParametersNames.Length; ++i)
                    {
                        int arrayIndex = i - InParameters.Length;

                        command.Parameters.Add(OutParametersNames[arrayIndex]);
                        command.Parameters[i].Direction = ParameterDirection.Output;
                    }

                    command.Parameters.Add("@r_cmd_return_value", MySqlDbType.Int64);
                    command.Parameters["@r_cmd_return_value"].Direction = ParameterDirection.ReturnValue;

                    command.CommandText = sqlBuilder.ToString();

                    cancellationToken.Token.ThrowIfCancellationRequested();

                    await Engine.CommandExecuteNonQueryAsync(command, cancellationToken.Token, IsolationLevel).ConfigureAwait(false);

                    outputValuesTemp = new string[OutParametersNames.Length];
                    for (int i = InParameters.Length; i < InParameters.Length + OutParametersNames.Length; ++i)
                    {
                        int arrayIndex = i - InParameters.Length;

                        outputValuesTemp[arrayIndex] = command.Parameters[i].Value?.ToString() ?? string.Empty;
                    }

                    result = (long)(command.Parameters["@r_cmd_return_value"].Value ?? 0L);
                }
                catch (OperationCanceledException)
                {
                    var exception = new TimeoutException($"MySQLRequest[{sqlBuilder}] waiting Timeout[{Timeout}] or canceled");
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

            return (result, outputValuesTemp);
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
        IQueryRequest<(long Result, string[] OutputValues)> IQueryRequest<(long Result, string[] OutputValues)>.Copy()
        {
            return Copy(Engine);
        }
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        IQueryRequest<(long Result, string[] OutputValues)> IQueryRequest<(long Result, string[] OutputValues)>.Copy(IRequestEngine engine)
        {
            return Copy(engine);
        }

        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        public StoredProcedureRequest Copy()
        {
            return Copy(Engine);
        }
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        public StoredProcedureRequest Copy(IRequestEngine engine)
        {
            return new StoredProcedureRequest(engine, this);
        }



        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static (long Result, string[] OutputValues) Execute(IRequestEngine engine,
            string name, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, name, Array.Empty<(string Name, string Value)>(),
                Array.Empty<string>(), engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static (long Result, string[] OutputValues) Execute(IRequestEngine engine,
            string name, TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, name, Array.Empty<(string Name, string Value)>(),
                Array.Empty<string>(), timeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<(long Result, string[] OutputValues)> ExecuteAsync(IRequestEngine engine,
            string name, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, name, Array.Empty<(string Name, string Value)>(),
                Array.Empty<string>(), engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<(long Result, string[] OutputValues)> ExecuteAsync(IRequestEngine engine,
            string name, TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, name, Array.Empty<(string Name, string Value)>(),
                Array.Empty<string>(), timeout, isolationLevel);
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="inParameters">
        ///     Массив входных параметров процедуры. Передайте пустой массив, чтобы не использовать входные параметры.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static (long Result, string[] OutputValues) Execute(IRequestEngine engine,
            string name, (string Name, string Value)[] inParameters,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, name, inParameters, Array.Empty<string>(),
                engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="inParameters">
        ///     Массив входных параметров процедуры. Передайте пустой массив, чтобы не использовать входные параметры.
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
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static (long Result, string[] OutputValues) Execute(IRequestEngine engine,
            string name, (string Name, string Value)[] inParameters, TimeSpan timeout,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, name, inParameters, Array.Empty<string>(),
                timeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="inParameters">
        ///     Массив входных параметров процедуры. Передайте пустой массив, чтобы не использовать входные параметры.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<(long Result, string[] OutputValues)> ExecuteAsync(IRequestEngine engine,
            string name, (string Name, string Value)[] inParameters,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, name, inParameters, Array.Empty<string>(),
                engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="inParameters">
        ///     Массив входных параметров процедуры. Передайте пустой массив, чтобы не использовать входные параметры.
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
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<(long Result, string[] OutputValues)> ExecuteAsync(IRequestEngine engine,
            string name, (string Name, string Value)[] inParameters, TimeSpan timeout,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, name, inParameters, Array.Empty<string>(),
                timeout, isolationLevel);
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="outParametersNames">
        ///     Массив названий выходных параметров процедуры. Передайте пустой массив, чтобы не использовать выходные параметры.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static (long Result, string[] OutputValues) Execute(IRequestEngine engine, string name,
            string[] outParametersNames, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, name, Array.Empty<(string Name, string Value)>(),
                outParametersNames, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="outParametersNames">
        ///     Массив названий выходных параметров процедуры. Передайте пустой массив, чтобы не использовать выходные параметры.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static (long Result, string[] OutputValues) Execute(IRequestEngine engine, string name,
            string[] outParametersNames, TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, name, Array.Empty<(string Name, string Value)>(),
                outParametersNames, timeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="outParametersNames">
        ///     Массив названий выходных параметров процедуры. Передайте пустой массив, чтобы не использовать выходные параметры.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<(long Result, string[] OutputValues)> ExecuteAsync(IRequestEngine engine, string name,
            string[] outParametersNames, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, name, Array.Empty<(string Name, string Value)>(),
                outParametersNames, engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="outParametersNames">
        ///     Массив названий выходных параметров процедуры. Передайте пустой массив, чтобы не использовать выходные параметры.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<(long Result, string[] OutputValues)> ExecuteAsync(IRequestEngine engine, string name,
            string[] outParametersNames, TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, name, Array.Empty<(string Name, string Value)>(),
                outParametersNames, timeout, isolationLevel);
        }

        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="inParameters">
        ///     Массив входных параметров процедуры. Передайте пустой массив, чтобы не использовать входные параметры.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="outParametersNames">
        ///     Массив названий выходных параметров процедуры. Передайте пустой массив, чтобы не использовать выходные параметры.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static (long Result, string[] OutputValues) Execute(IRequestEngine engine, string name,
            (string Name, string Value)[] inParameters, string[] outParametersNames,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return Execute(engine, name, inParameters, outParametersNames,
                engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="inParameters">
        ///     Массив входных параметров процедуры. Передайте пустой массив, чтобы не использовать входные параметры.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="outParametersNames">
        ///     Массив названий выходных параметров процедуры. Передайте пустой массив, чтобы не использовать выходные параметры.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static (long Result, string[] OutputValues) Execute(IRequestEngine engine, string name,
            (string Name, string Value)[] inParameters, string[] outParametersNames,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new StoredProcedureRequest(engine, name, inParameters, outParametersNames,
                timeout, isolationLevel).Execute();
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="inParameters">
        ///     Массив входных параметров процедуры. Передайте пустой массив, чтобы не использовать входные параметры.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="outParametersNames">
        ///     Массив названий выходных параметров процедуры. Передайте пустой массив, чтобы не использовать выходные параметры.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<(long Result, string[] OutputValues)> ExecuteAsync(IRequestEngine engine, string name,
            (string Name, string Value)[] inParameters, string[] outParametersNames,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return ExecuteAsync(engine, name, inParameters, outParametersNames,
                engine.CommandTimeout, isolationLevel);
        }
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с заданными параметрами.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        /// <param name="name">
        ///     Название процедуры.
        /// </param>
        /// <param name="inParameters">
        ///     Массив входных параметров процедуры. Передайте пустой массив, чтобы не использовать входные параметры.
        ///     Name - название параметра.
        ///     Value - значение параметра.
        /// </param>
        /// <param name="outParametersNames">
        ///     Массив названий выходных параметров процедуры. Передайте пустой массив, чтобы не использовать выходные параметры.
        /// </param>
        /// <param name="timeout">
        ///     Время ожидания ответа от сервера.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="long"/>, возвращённое оператором RETURN и массив значений типа <see cref="string"/> выходных параметров процедуры, доступных после её вызова.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        public static Task<(long Result, string[] OutputValues)> ExecuteAsync(IRequestEngine engine, string name,
            (string Name, string Value)[] inParameters, string[] outParametersNames,
            TimeSpan timeout, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new StoredProcedureRequest(engine, name, inParameters, outParametersNames,
                timeout, isolationLevel).ExecuteAsync();
        }
    }
}
