// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Connection.MySQL
{
    /// <summary>
    ///     Представляет тип сервиса для выполнения запросов к MySQL базе данных.
    /// </summary>
    public enum RequestEngineType : byte
    {
        /// <summary>
        ///     Обработчик, который запускает запросы последовательно на каждом соединении без балансировки.
        /// </summary>
        Default = 1
    }
}
