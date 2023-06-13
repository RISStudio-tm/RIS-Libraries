// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;

namespace RIS.Connection.MySQL
{
    /// <summary>
    ///     Представляет интерфейс сервиса обработки запросов к MySQL базе данных.
    /// </summary>
    public interface IRequestEngine
    {
        /// <summary>
        ///     Позволяет получать соединение <see cref="RIS.Connection.MySQL.MySQLConnection"/>, которое используется для данного экземпляра сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>.
        /// </summary>
        MySQLConnection CurrentMySQLConnection { get; }
        /// <summary>
        ///     Позволяет получать или устанавливать стандартное время ожидания запросов.
        /// </summary>
        TimeSpan CommandTimeout { get; set; }
        /// <summary>
        ///     Позволяет получать глобальный токен отмены запросов.
        /// </summary>
        CancellationTokenSource GlobalCancellationToken { get; }

        /// <summary>
        ///     Вызывает отмену всех запросов.
        /// </summary>
        /// <exception cref="AggregateException"></exception>
        void CancelAll();



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
        Task CommandExecuteNonQueryAsync(MySqlCommand command,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
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
        Task CommandExecuteNonQueryAsync(MySqlCommand command,
            CancellationToken cancellationToken, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

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
        Task<string[]> CommandExecuteReaderAsync(MySqlCommand command,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
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
        Task<string[]> CommandExecuteReaderAsync(MySqlCommand command,
            CancellationToken cancellationToken, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

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
        Task<DataSet> CommandExecuteAdapterAsync(MySqlDataAdapter adapter,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
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
        Task<DataSet> CommandExecuteAdapterAsync(MySqlDataAdapter adapter,
            CancellationToken cancellationToken, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }
}
