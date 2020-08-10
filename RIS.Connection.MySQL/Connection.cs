// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace RIS.Connection.MySQL
{
    /// <summary>
    ///     Представляет соединение для работы с подключением к MySQL базе данных. Этот класс не может быть унаследован.
    /// </summary>
    public sealed class MySQLConnection
    {
        /// <summary>
        ///     Происходит при получении информации.
        /// </summary>
        public event EventHandler<RInformationEventArgs> Information;
        /// <summary>
        ///     Происходит при возникновении предупреждения.
        /// </summary>
        public event EventHandler<RWarningEventArgs> Warning;
        /// <summary>
        ///     Происходит при возникновении ошибки.
        /// </summary>
        public event EventHandler<RErrorEventArgs> Error;

        private MySqlConnection[] _connections;
        /// <summary>
        ///     Позволяет получать массив типа <see cref="MySql.Data.MySqlClient.MySqlConnection"/> SQL-соединений, которые используются в данном экземпляре класса <see cref="RIS.Connection.MySQL.MySQLConnection"/>.
        /// </summary>
        internal MySqlConnection[] ConnectionsArray
        {
            get
            {
                return _connections;
            }
        }
        /// <summary>
        ///     Позволяет получать <see cref="System.Collections.ObjectModel.ReadOnlyCollection{T}"/>, где T это <see cref="MySql.Data.MySqlClient.MySqlConnection"/>, SQL-соединений, которые используются в данном экземпляре класса <see cref="RIS.Connection.MySQL.MySQLConnection"/>.
        /// </summary>
        public ReadOnlyCollection<MySqlConnection> Connections
        {
            get
            {
                return new ReadOnlyCollection<MySqlConnection>(_connections);
            }
        }
        /// <summary>
        ///     Позволяет получать экзепляр класса <see cref="RIS.Connection.MySQL.Requests"/>, который используется в данном экземпляре класса <see cref="RIS.Connection.MySQL.MySQLConnection"/> для работы с запросами к SQL-соединениям из массива <see cref="Connections"/>.
        /// </summary>
        public Requests Requests { get; private set; }
        /// <summary>
        ///     Позволяет получать состояние подключения к MySQL базе данных.
        /// </summary>
        public bool ConnectionComplete { get; private set; }

        /// <summary>
        ///     Закрывает соединения и освобождает ресурсы.
        /// </summary>
        ~MySQLConnection()
        {
            CloseConnection();
        }

        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="RIS.Connection.MySQL.MySQLConnection"/> и его внутренние переменные.
        /// </summary>
        public MySQLConnection()
        {

        }

        /// <summary>
        ///     Вызывает Invoke у события Information от имени текущего экземпляра с аргументами <paramref name="e"/>
        /// </summary>
        /// <param name="e">
        ///     Аргументы для события Information.
        /// </param>
        public void OnInformation(RInformationEventArgs e)
        {
            OnInformation(this, e);
        }
        /// <summary>
        ///     Вызывает Invoke у события Information от имени объекта <paramref name="sender"/> с аргументами <paramref name="e"/>
        /// </summary>
        /// <param name="sender">
        ///     Объект-отправитель (от имени которого вызывается Invoke)
        /// </param>
        /// <param name="e">
        ///     Аргументы для события Information.
        /// </param>
        public void OnInformation(object sender, RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        /// <summary>
        ///     Вызывает Invoke у события Warning от имени текущего экземпляра с аргументами <paramref name="e"/>
        /// </summary>
        /// <param name="e">
        ///     Аргументы для события Warning.
        /// </param>
        public void OnWarning(RWarningEventArgs e)
        {
            OnWarning(this, e);
        }
        /// <summary>
        ///     Вызывает Invoke у события Warning от имени объекта <paramref name="sender"/> с аргументами <paramref name="e"/>
        /// </summary>
        /// <param name="sender">
        ///     Объект-отправитель (от имени которого вызывается Invoke)
        /// </param>
        /// <param name="e">
        ///     Аргументы для события Warning.
        /// </param>
        public void OnWarning(object sender, RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        /// <summary>
        ///     Вызывает Invoke у события Error от имени текущего экземпляра с аргументами <paramref name="e"/>
        /// </summary>
        /// <param name="e">
        ///     Аргументы для события Error.
        /// </param>
        public void OnError(RErrorEventArgs e)
        {
            OnError(this, e);
        }
        /// <summary>
        ///     Вызывает Invoke у события Error от имени объекта <paramref name="sender"/> с аргументами <paramref name="e"/>
        /// </summary>
        /// <param name="sender">
        ///     Объект-отправитель (от имени которого вызывается Invoke)
        /// </param>
        /// <param name="e">
        ///     Аргументы для события Error.
        /// </param>
        public void OnError(object sender, RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }

        /// <summary>
        ///     Открывает соединения в количестве <paramref name="numberConnections"/> c MySQL базой данных <paramref name="database"/>, расположенной по адресу хоста <paramref name="ipAddress"/>, используя кодировку <paramref name="charset"/> и аутентификацию по имени пользователя (логину) <paramref name="login"/> и паролю <paramref name="password"/>
        /// </summary>
        /// <param name="numberConnections">
        ///     Количество SQL-соединений <see cref="MySql.Data.MySqlClient.MySqlConnection"/>
        /// </param>
        /// <param name="ipAddress">
        ///     Адрес хоста, на котором расположена база данных.
        /// </param>
        /// <param name="database">
        ///     Название базы данных.
        /// </param>
        /// <param name="login">
        ///     Имя пользователя (логин).
        /// </param>
        /// <param name="password">
        ///     Пароль пользователя.
        /// </param>
        /// <param name="charset">
        ///     Кодировка для передаваемых и принимаемых данных.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="bool"/>. Если <see langword="true"/>, то соединение открыто успешно, иначе <see langword="false"/>.
        /// </returns>
        public bool OpenConnection(byte numberConnections, string ipAddress, string database, string login, string password, string charset = "utf8")
        {
            return OpenConnection(numberConnections, TimeSpan.FromMilliseconds(20000),
                ipAddress, database, login, password, charset);
        }
        /// <summary>
        ///     Открывает соединения в количестве <paramref name="numberConnections"/> c MySQL базой данных <paramref name="database"/>, расположенной по адресу хоста <paramref name="ipAddress"/>, используя кодировку <paramref name="charset"/>, стандартное время ожидания SQL-команд <paramref name="millisecondsTimeout"/> и аутентификацию по имени пользователя (логину) <paramref name="login"/> и паролю <paramref name="password"/>
        /// </summary>
        /// <param name="numberConnections">
        ///     Количество SQL-соединений <see cref="MySql.Data.MySqlClient.MySqlConnection"/>
        /// </param>
        /// <param name="millisecondsTimeout">
        ///     Стандартное время ожидания SQL-команд.
        /// </param>
        /// <param name="ipAddress">
        ///     Адрес хоста, на котором расположена база данных.
        /// </param>
        /// <param name="database">
        ///     Название базы данных.
        /// </param>
        /// <param name="login">
        ///     Имя пользователя (логин).
        /// </param>
        /// <param name="password">
        ///     Пароль пользователя.
        /// </param>
        /// <param name="charset">
        ///     Кодировка для передаваемых и принимаемых данных.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="bool"/>. Если <see langword="true"/>, то соединение открыто успешно, иначе <see langword="false"/>.
        /// </returns>
        public bool OpenConnection(byte numberConnections, uint millisecondsTimeout, string ipAddress, string database, string login, string password, string charset = "utf8")
        {
            return OpenConnection(numberConnections, TimeSpan.FromMilliseconds(millisecondsTimeout), 
                ipAddress, database, login, password, charset);
        }
        /// <summary>
        ///     Открывает соединения в количестве <paramref name="numberConnections"/> c MySQL базой данных <paramref name="database"/>, расположенной по адресу хоста <paramref name="ipAddress"/>, используя кодировку <paramref name="charset"/>, стандартное время ожидания SQL-команд <paramref name="timeout"/> и аутентификацию по имени пользователя (логину) <paramref name="login"/> и паролю <paramref name="password"/>
        /// </summary>
        /// <param name="numberConnections">
        ///     Количество SQL-соединений <see cref="MySql.Data.MySqlClient.MySqlConnection"/>
        /// </param>
        /// <param name="timeout">
        ///     Стандартное время ожидания SQL-команд.
        /// </param>
        /// <param name="ipAddress">
        ///     Адрес хоста, на котором расположена база данных.
        /// </param>
        /// <param name="database">
        ///     Название базы данных.
        /// </param>
        /// <param name="login">
        ///     Имя пользователя (логин).
        /// </param>
        /// <param name="password">
        ///     Пароль пользователя.
        /// </param>
        /// <param name="charset">
        ///     Кодировка для передаваемых и принимаемых данных.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="bool"/>. Если <see langword="true"/>, то соединение открыто успешно, иначе <see langword="false"/>.
        /// </returns>
        public bool OpenConnection(byte numberConnections, TimeSpan timeout, string ipAddress, string database, string login, string password, string charset = "utf8")
        {
            MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder(string.Empty);
            connectionStringBuilder.DefaultCommandTimeout = (uint)timeout.TotalSeconds;
            connectionStringBuilder.Server = ipAddress;
            connectionStringBuilder.Database = database;
            connectionStringBuilder.UserID = login;
            connectionStringBuilder.Password = password;
            connectionStringBuilder.CharacterSet = charset;

            return OpenConnection(numberConnections, connectionStringBuilder.ConnectionString);
        }
        /// <summary>
        ///     Открывает соединения в количестве <paramref name="numberConnections"/> c MySQL базой данных с использованием строки подключения <paramref name="connectionString"/>
        /// </summary>
        /// <param name="numberConnections">
        ///     Количество SQL-соединений <see cref="MySql.Data.MySqlClient.MySqlConnection"/>
        /// </param>
        /// <param name="connectionString">
        ///     Строка подключения для создания соединения с базой данных.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="bool"/>. Если <see langword="true"/>, то соединение открыто успешно, иначе <see langword="false"/>.
        /// </returns>
        public bool OpenConnection(byte numberConnections, string connectionString)
        {
            return OpenConnection(numberConnections, TimeSpan.FromMilliseconds(20000),
                connectionString);
        }
        /// <summary>
        ///     Открывает соединения в количестве <paramref name="numberConnections"/> c MySQL базой данных с использованием строки подключения <paramref name="connectionString"/> и стандартного времени ожидания SQL-команд <paramref name="millisecondsTimeout"/>
        /// </summary>
        /// <param name="numberConnections">
        ///     Количество SQL-соединений <see cref="MySql.Data.MySqlClient.MySqlConnection"/>
        /// </param>
        /// <param name="millisecondsTimeout">
        ///     Стандартное время ожидания SQL-команд.
        /// </param>
        /// <param name="connectionString">
        ///     Строка подключения для создания соединения с базой данных.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="bool"/>. Если <see langword="true"/>, то соединение открыто успешно, иначе <see langword="false"/>.
        /// </returns>
        public bool OpenConnection(byte numberConnections, uint millisecondsTimeout, string connectionString)
        {
            return OpenConnection(numberConnections, TimeSpan.FromMilliseconds(millisecondsTimeout),
                connectionString);
        }
        /// <summary>
        ///     Открывает соединения в количестве <paramref name="numberConnections"/> c MySQL базой данных с использованием строки подключения <paramref name="connectionString"/> и стандартного времени ожидания SQL-команд <paramref name="timeout"/>
        /// </summary>
        /// <param name="numberConnections">
        ///     Количество SQL-соединений <see cref="MySql.Data.MySqlClient.MySqlConnection"/>
        /// </param>
        /// <param name="timeout">
        ///     Стандартное время ожидания SQL-команд.
        /// </param>
        /// <param name="connectionString">
        ///     Строка подключения для создания соединения с базой данных.
        /// </param>
        /// <returns>
        ///     Значение типа <see cref="bool"/>. Если <see langword="true"/>, то соединение открыто успешно, иначе <see langword="false"/>.
        /// </returns>
        public bool OpenConnection(byte numberConnections, TimeSpan timeout, string connectionString)
        {
            try
            {
                if (numberConnections < 1)
                    numberConnections = 1;

                Task.Factory.StartNew(() => CreateConnections(numberConnections, connectionString)).Wait();

                if (ConnectionComplete)
                {
                    Requests = new Requests(this, timeout);
                }

                return ConnectionComplete;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }

        /// <summary>
        ///     Закрывает соединения и освобождает ресурсы.
        /// </summary>
        public void CloseConnection()
        {
            if (!ConnectionComplete)
                return;

            ConnectionComplete = false;
            Requests.CancelAllRequests();
            Requests = null;
            foreach (var connection in ConnectionsArray)
            {
                if (connection?.State != ConnectionState.Closed)
                {
                    connection?.Close();
                }
                connection?.Dispose();
            }
            _connections = null;
        }

        private void CreateConnections(byte numberConnections, string connectionString)
        {
            _connections = new MySqlConnection[numberConnections];

            for (int i = 0; i < numberConnections; ++i)
            {
                try
                {
                    ConnectionsArray[i] = new MySqlConnection(connectionString);
                    ConnectionsArray[i].Open();

                    while (ConnectionsArray[i] != null)
                    {
                        if (ConnectionsArray[i].State == ConnectionState.Connecting)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        if (ConnectionsArray[i].State == ConnectionState.Broken)
                        {
                            ConnectionComplete = false;
                            break;
                        }

                        if (ConnectionsArray[i].State == ConnectionState.Closed)
                        {
                            ConnectionComplete = false;
                            break;
                        }

                        if (ConnectionsArray[i].State == ConnectionState.Open)
                        {
                            ConnectionComplete = true;
                            ConnectionsArray[i].StateChange += Connection_StateChange;
                            break;
                        }
                    }

                    if (!ConnectionComplete)
                        break;
                }
                catch (MySqlException ex)
                {
                    ConnectionComplete = false;

                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                    throw;
                }
                catch (InvalidOperationException ex)
                {
                    ConnectionComplete = false;

                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                    throw;
                }
            }
        }

        private void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Closed)
            {
                CloseConnection();

                return;
            }
            else if (e.CurrentState == ConnectionState.Broken)
            {
                CloseConnection();
            }
        }
    }
}
