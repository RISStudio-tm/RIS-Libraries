// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Text;
using MySqlConnector;

namespace RIS.Connection.MySQL
{
    internal static class RequestEngineHelper
    {
        internal static void ReplaceDBNullParameterValue(string value, ref MySqlCommand command,
            string parameterName)
        {
            if (command != null && (value == "NULL" || value == null))
                command.Parameters[parameterName].Value = DBNull.Value;
            if (command != null && (value == "'NULL'"))
                command.Parameters[parameterName].Value = "NULL";
        }
        internal static void ReplaceDBNullParameterValue(string value, ref MySqlDataAdapter adapter,
            string parameterName)
        {
            if (adapter?.SelectCommand != null && (value == "NULL" || value == null))
                adapter.SelectCommand.Parameters[parameterName].Value = DBNull.Value;
            if (adapter?.SelectCommand != null && (value == "'NULL'"))
                adapter.SelectCommand.Parameters[parameterName].Value = "NULL";
        }

        internal static void ReplaceFunctionParameterValue(string value, ref MySqlCommand command,
            string parameterName, ref string sql)
        {
            if (value == "CURRENT_TIMESTAMP")
                sql = sql.Replace(parameterName, "CURRENT_TIMESTAMP");
            if (command != null && (value == "'CURRENT_TIMESTAMP'"))
                command.Parameters[parameterName].Value = "CURRENT_TIMESTAMP";
        }
        internal static void ReplaceFunctionParameterValue(string value, ref MySqlCommand command,
            string parameterName, ref StringBuilder sqlBuilder)
        {
            if (value == "CURRENT_TIMESTAMP")
                sqlBuilder = sqlBuilder.Replace(parameterName, "CURRENT_TIMESTAMP");
            if (command != null && (value == "'CURRENT_TIMESTAMP'"))
                command.Parameters[parameterName].Value = "CURRENT_TIMESTAMP";
        }
        internal static void ReplaceFunctionParameterValue(string value, ref MySqlDataAdapter adapter,
            string parameterName, ref string sql)
        {
            if (value == "CURRENT_TIMESTAMP")
                sql = sql.Replace(parameterName, "CURRENT_TIMESTAMP");
            if (adapter?.SelectCommand != null && (value == "'CURRENT_TIMESTAMP'"))
                adapter.SelectCommand.Parameters[parameterName].Value = "CURRENT_TIMESTAMP";
        }
        internal static void ReplaceFunctionParameterValue(string value, ref MySqlDataAdapter adapter,
            string parameterName, ref StringBuilder sqlBuilder)
        {
            if (value == "CURRENT_TIMESTAMP")
                sqlBuilder = sqlBuilder.Replace(parameterName, "CURRENT_TIMESTAMP");
            if (adapter?.SelectCommand != null && (value == "'CURRENT_TIMESTAMP'"))
                adapter.SelectCommand.Parameters[parameterName].Value = "CURRENT_TIMESTAMP";
        }
    }
}
