// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;

namespace RIS.Connection.MySQL.Requests
{
    /// <summary>
    ///     Представляет интерфейс запроса к MySQL базе данных.
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        ///     Позволяет получать экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </summary>
        IRequestEngine Engine { get; }
        /// <summary>
        ///     Позволяет получать или устанавливать время ожидания ответа от сервера.
        /// </summary>
        TimeSpan Timeout { get; set; }
        /// <summary>
        ///     Позволяет получать или устанавливать уровень изоляции транзакции.
        /// </summary>
        IsolationLevel IsolationLevel { get; set; }
        /// <summary>
        ///     Позволяет получать токен отмены запроса.
        /// </summary>
        CancellationTokenSource RequestCancellationToken { get; }

        /// <summary>
        ///     Вызывает отмену запроса.
        /// </summary>
        /// <exception cref="AggregateException"></exception>
        void Cancel();

        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        IRequest Copy();
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        IRequest Copy(IRequestEngine engine);
    }

    /// <summary>
    ///     Представляет интерфейс запроса к MySQL базе данных.
    /// </summary>
    public interface INonQueryRequest : IRequest
    {
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
        void Execute();
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
        Task ExecuteAsync();

        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        new INonQueryRequest Copy();
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        new INonQueryRequest Copy(IRequestEngine engine);
    }

    /// <summary>
    ///     Представляет интерфейс запроса к MySQL базе данных.
    /// </summary>
    /// <typeparam name="TResult">
    ///     Тип результата, который возвращает запрос.
    /// </typeparam>
    public interface IQueryRequest<TResult> : IRequest
    {
        /// <summary>
        ///     (Синхронно) Вызывает выполнение запроса с текущими параметрами.
        /// </summary>
        /// <returns>
        ///     Значение типа <typeparamref name="TResult"/>, представляющее результат вызова запроса.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        TResult Execute();
        /// <summary>
        ///     (Асинхронно) Вызывает выполнение запроса с текущими параметрами.
        /// </summary>
        /// <returns>
        ///     Значение типа <typeparamref name="TResult"/>, представляющее результат вызова запроса.
        /// </returns>
        /// <exception cref="DbException"></exception>
        /// <exception cref="MySqlConnector.MySqlException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AggregateException"></exception>
        Task<TResult> ExecuteAsync();

        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        new IQueryRequest<TResult> Copy();
        /// <summary>
        ///     Создаёт копию запроса.
        /// </summary>
        /// <param name="engine">
        ///     Экземпляр сервиса <see cref="RIS.Connection.MySQL.IRequestEngine"/>, с помощью которого будет выполняться запрос.
        /// </param>
        new IQueryRequest<TResult> Copy(IRequestEngine engine);
    }
}
