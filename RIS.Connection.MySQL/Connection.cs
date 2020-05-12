using System;
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
        ///     Позволяет получать массив типа <see cref="MySql.Data.MySqlClient.MySqlConnection"/> SQL-соединений, которые используются в данном экземпляре класса <see cref="RIS.Connection.MySQL.MySQLConnection"/>.
        /// </summary>
        public MySqlConnection[] Connections { get; private set; }
        /// <summary>
        ///     Позволяет получать экзепляр класса <see cref="RIS.Connection.MySQL.Requests"/>, который используется в данном экземпляре класса <see cref="RIS.Connection.MySQL.MySQLConnection"/> для работы с запросами к SQL-соединениям из массива <see cref="Connections"/>.
        /// </summary>
        public Requests Requests { get; private set; }
        /// <summary>
        ///     Позволяет получать состояние подключения к MySQL базе данных.
        /// </summary>
        public bool ConnectionComplete { get; private set; }

        private object LockObjMessageEvent { get; }
        private event EventHandler<RMessageEventArgs> PShowMessage;
        /// <summary>
        ///     Используется для вывода информации или предупреждений.
        /// </summary>
        public event EventHandler<RMessageEventArgs> ShowMessage
        {
            add
            {
                lock (LockObjMessageEvent)
                {
                    PShowMessage += value;
                    DShowMessage += value;
                }
            }
            remove
            {
                lock (LockObjMessageEvent)
                {
                    if (PShowMessage != null)
                        PShowMessage -= value;
                    if (DShowMessage != null)
                        DShowMessage -= value;
                }
            }
        }
        internal EventHandler<RMessageEventArgs> DShowMessage { get; private set; }

        private object LockObjErrorEvent { get; }
        private event EventHandler<RErrorEventArgs> PShowError;
        /// <summary>
        ///     Используется для вывода ошибок.
        /// </summary>
        public event EventHandler<RErrorEventArgs> ShowError
        {
            add
            {
                lock (LockObjErrorEvent)
                {
                    PShowError += value;
                    DShowError += value;
                }
            }
            remove
            {
                lock (LockObjErrorEvent)
                {
                    if (PShowError != null)
                        PShowError -= value;
                    if (DShowError != null)
                        DShowError -= value;
                }
            }
        }
        internal EventHandler<RErrorEventArgs> DShowError { get; private set; }

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
            LockObjMessageEvent = new object();
            LockObjErrorEvent = new object();
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
                {
                    numberConnections = 1;
                }

                Task.Factory.StartNew(() => CreateConnections(numberConnections, connectionString)).Wait();

                if (ConnectionComplete)
                {
                    Requests = new Requests(this, Connections, timeout);
                }

                return ConnectionComplete;
            }
            catch (Exception ex)
            {
                Events.DShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                PShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }

        /// <summary>
        ///     Закрывает соединения и освобождает ресурсы.
        /// </summary>
        public void CloseConnection()
        {
            if (ConnectionComplete)
            {
                ConnectionComplete = false;
                Requests.CancelAllRequests();
                Requests = null;
                foreach (var connection in Connections)
                {
                    if (connection?.State != ConnectionState.Closed)
                    {
                        connection?.Close();
                    }
                    connection?.Dispose();
                }
                Connections = null;
            }
        }

        private void CreateConnections(byte numberConnections, string connectionString)
        {
            Connections = new MySqlConnection[numberConnections];

            for (byte i = 0; i < numberConnections; i++)
            {
                try
                {
                    Connections[i] = new MySqlConnection(connectionString);
                    Connections[i].Open();

                    while (Connections[i] != null)
                    {
                        if (Connections[i].State == ConnectionState.Connecting)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        if (Connections[i].State == ConnectionState.Broken)
                        {
                            ConnectionComplete = false;
                            break;
                        }

                        if (Connections[i].State == ConnectionState.Closed)
                        {
                            ConnectionComplete = false;
                            break;
                        }

                        if (Connections[i].State == ConnectionState.Open)
                        {
                            ConnectionComplete = true;
                            Connections[i].StateChange += Connection_StateChange;
                            break;
                        }
                    }

                    if (!ConnectionComplete)
                    {
                        break;
                    }
                }
                catch (MySqlException ex)
                {
                    ConnectionComplete = false;

                    Events.DShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    PShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    throw;
                }
                catch (InvalidOperationException ex)
                {
                    ConnectionComplete = false;

                    Events.DShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    PShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
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

            if (e.CurrentState == ConnectionState.Broken)
            {
                CloseConnection();
            }
        }
    }
}
