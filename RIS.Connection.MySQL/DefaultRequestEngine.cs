// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;
using RIS.Connection.MySQL.Requests;
using RIS.Extensions;
using RIS.Synchronization;

namespace RIS.Connection.MySQL
{
    /// <summary>
    ///     Представляет сервис для выполнения запросов к MySQL базе данных. Этот класс не может быть унаследован.
    /// </summary>
    public sealed class DefaultRequestEngine : IRequestEngine
    {
        private AsyncLock[] LockObjExecReaders { get; }
        private object LockObjNextConnection { get; }
        private uint _nextConnectionIndex;
        private uint NextConnectionIndex
        {
            get
            {
                lock (LockObjNextConnection)
                {
                    if (_nextConnectionIndex > CurrentMySQLConnection.ConnectionsArray.Length - 1)
                        _nextConnectionIndex = 0;

                    return _nextConnectionIndex++;
                }
            }
        }

        /// <summary>
        ///     Позволяет получать соединение <see cref="RIS.Connection.MySQL.MySQLConnection"/>, которое используется для данного экземпляра сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>.
        /// </summary>
        public MySQLConnection CurrentMySQLConnection { get; }
        private TimeSpan _commandTimeout;
        /// <summary>
        ///     Позволяет получать или устанавливать стандартное время ожидания запросов.
        /// </summary>
        public TimeSpan CommandTimeout
        {
            get
            {
                return _commandTimeout;
            }
            set
            {
                if (value.TotalSeconds < TimeSpan.FromSeconds(3).TotalSeconds)
                    value = TimeSpan.FromSeconds(3);

                _commandTimeout = value;
            }
        }
        /// <summary>
        ///     Позволяет получать глобальный токен отмены запросов.
        /// </summary>
        public CancellationTokenSource GlobalCancellationToken { get; }

        internal DefaultRequestEngine(MySQLConnection connection, TimeSpan timeout)
        {
            if (connection == null)
            {
                var exception = new ArgumentNullException(nameof(connection), $"{nameof(connection)} cannot be null");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            CurrentMySQLConnection = connection;
            CommandTimeout = timeout;

            LockObjExecReaders = new AsyncLock[connection.ConnectionsArray.Length];
            for (byte i = 0; i < LockObjExecReaders.Length; ++i)
            {
                LockObjExecReaders[i] = new AsyncLock();
            }
            LockObjNextConnection = new object();

            GlobalCancellationToken = new CancellationTokenSource();
        }

        private (MySqlConnection connection, uint connectionIndex) GetNextConnection()
        {
            uint connectionIndex = NextConnectionIndex;
            return (CurrentMySQLConnection.ConnectionsArray[connectionIndex], connectionIndex);
        }

        /// <summary>
        ///     Вызывает отмену всех запросов.
        /// </summary>
        /// <exception cref="AggregateException"></exception>
        public void CancelAll()
        {
            try
            {
                GlobalCancellationToken.Cancel();
            }
            catch (Exception)
            {
                var exception = new Exception("Failed to cancel all requests");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                CurrentMySQLConnection.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
        }



        /// <summary>
        ///     Выполняет команду без получения результата.
        /// </summary>
        /// <param name="command">
        ///     Команда, которая будет выполнена.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="DbException"></exception>
        public Task CommandExecuteNonQueryAsync(MySqlCommand command,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return CommandExecuteNonQueryAsync(command, CancellationToken.None, isolationLevel);
        }
        /// <summary>
        ///     Выполняет команду без получения результата.
        /// </summary>
        /// <param name="command">
        ///     Команда, которая будет выполнена.
        /// </param>
        /// <param name="cancellationToken">
        ///     Токен отмены выполнения команды.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Имеет возвращаемый тип <see langword="void"/>.
        /// </returns>
        /// <exception cref="DbException"></exception>
        public async Task CommandExecuteNonQueryAsync(MySqlCommand command,
            CancellationToken cancellationToken, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            (MySqlConnection connection, uint connectionIndex) = GetNextConnection();
            command.Connection = connection;

            using (await LockObjExecReaders[connectionIndex].LockAsync(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                command.Transaction = await connection.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);

                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                await command.Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Выполняет команду.
        /// </summary>
        /// <param name="command">
        ///     Команда, которая будет выполнена.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        public Task<string[]> CommandExecuteReaderAsync(MySqlCommand command,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return CommandExecuteReaderAsync(command, CancellationToken.None, isolationLevel);
        }
        /// <summary>
        ///     Выполняет команду.
        /// </summary>
        /// <param name="command">
        ///     Команда, которая будет выполнена.
        /// </param>
        /// <param name="cancellationToken">
        ///     Токен отмены выполнения команды.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Массив типа <see cref="string"/>, который содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        public async Task<string[]> CommandExecuteReaderAsync(MySqlCommand command,
            CancellationToken cancellationToken, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            string[] result = Array.Empty<string>();

            (MySqlConnection connection, uint connectionIndex) = GetNextConnection();
            command.Connection = connection;

            using (await LockObjExecReaders[connectionIndex].LockAsync(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                command.Transaction = await connection.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);

                MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

                await reader.ReadAsync(cancellationToken).ConfigureAwait(false);

                result = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; ++i)
                {
                    result[i] = reader.GetValue(i).ToString();
                }

                await reader.DisposeAsync().ConfigureAwait(false);

                await command.Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        ///     Выполняет команду SelectCommand у адаптера.
        /// </summary>
        /// <param name="adapter">
        ///     Адаптер, у которого будет выполнена команда SelectCommand.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="DataSet"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        public Task<DataSet> CommandExecuteAdapterAsync(MySqlDataAdapter adapter,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return CommandExecuteAdapterAsync(adapter, CancellationToken.None, isolationLevel);
        }
        /// <summary>
        ///     Выполняет SelectCommand у адаптера.
        /// </summary>
        /// <param name="adapter">
        ///     Адаптер, у которого будет выполнена команда SelectCommand.
        /// </param>
        /// <param name="cancellationToken">
        ///     Токен отмены выполнения команды.
        /// </param>
        /// <param name="isolationLevel">
        ///     Уровень изоляции транзакции.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="DataSet"/>, которое содержит ответ сервера.
        /// </returns>
        /// <exception cref="DbException"></exception>
        public async Task<DataSet> CommandExecuteAdapterAsync(MySqlDataAdapter adapter,
            CancellationToken cancellationToken, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            DataSet result = new DataSet();

            (MySqlConnection connection, uint connectionIndex) = GetNextConnection();
            adapter.SelectCommand.Connection = connection;

            using (await LockObjExecReaders[connectionIndex].LockAsync(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                adapter.SelectCommand.Transaction = await connection.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);

                adapter.Fill(result);

                await adapter.SelectCommand.Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }

            return result;
        }
    }
}
