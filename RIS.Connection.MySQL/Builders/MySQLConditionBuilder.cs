// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using RIS.Extensions;

namespace RIS.Connection.MySQL.Builders
{
    public sealed class MySQLConditionBuilder : ICloneable
    {
        private static readonly Dictionary<string, string> ParenthesesMap;

        public static readonly MySQLConditionBuilder Empty;

        private readonly StringBuilder _sql;
        private readonly List<MySQLConditionParameter> _parameters;
        private readonly Stack<(string Parenthesis, int Index)> _parentheses;

        private bool _locked;

        public bool ParenthesesAutoComplete { get; set; }

        static MySQLConditionBuilder()
        {
            ParenthesesMap = new Dictionary<string, string>
            {
                { ")", "(" },
                { "]", "[" },
                { "}", "{" }
            };

            Empty = Where()
                .Lock();
        }

        private MySQLConditionBuilder(
            bool parenthesesAutoComplete = true)
        {
            _parentheses = new Stack<(string Parenthesis, int Index)>();
            _sql = new StringBuilder();
            _parameters = new List<MySQLConditionParameter>();

            ParenthesesAutoComplete = parenthesesAutoComplete;
        }
        private MySQLConditionBuilder(
            MySQLConditionBuilder builder,
            bool locked = false)
        {
            _sql = new StringBuilder(
                builder._sql
                    .ToString()
                    .TrimEnd());
            _parameters = new List<MySQLConditionParameter>(
                builder._parameters);
            _parentheses = new Stack<(string Parenthesis, int Index)>(
                builder._parentheses);

            ParenthesesAutoComplete = builder.ParenthesesAutoComplete;

            if (locked)
                Lock();
        }



        private void ThrowIfLocked()
        {
            if (!_locked)
                return;

            var exception =
                new InvalidOperationException($"The current MySQLConditionBuilder[{_sql}] instance is locked");
            Events.OnError(this,
                new RErrorEventArgs(exception, exception.Message));

            throw exception;
        }

        private void CompleteParentheses()
        {
            while (_parentheses.Count > 0)
            {
                var parenthesis = _parentheses.Pop().Parenthesis;

                var parenthesisIndex = ParenthesesMap.Values
                    .IndexesWhere(value => value == parenthesis)
                    .DefaultIfEmpty(-1).First();

                if (parenthesisIndex == -1)
                    continue;

                var parenthesisMapped = ParenthesesMap.Keys
                    .ElementAt(parenthesisIndex);

                if (_sql[_sql.Length - 1] == ' ')
                {
                    _sql.Remove(_sql.Length - 1, 1);
                }

                _sql.Append(parenthesisMapped);
            }
        }

        private void ValidateParameterValue(MySQLConditionParameter parameter)
        {
            if (parameter == null)
                return;

            object value = parameter.Value;

            if (value is string valueString)
                value = valueString.ToUpperInvariant();

            switch (value)
            {
                case null:
                case "NULL"
                    when parameter.Value is string:

                    parameter.Value = DBNull.Value;
                    break;
                case "'NULL'"
                    when parameter.Value is string parameterValue:

                    parameter.Value = parameterValue
                        .Substring(1, parameterValue.Length - 2);
                    break;
                default:
                    break;
            }
        }

        private bool IsValidParentheses()
        {
            if (ParenthesesAutoComplete)
            {
                CompleteParentheses();

                return true;
            }

            if (_parentheses.Count != 0)
            {
                var sql = _sql
                    .ToString()
                    .TrimEnd();

                var exception =
                    new FormatException($"Parentheses are not closed in MySQL condition string [{sql}] at start indexes [{string.Join(", ", _parentheses.Select((value => value.Index)).ToArray())}]");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));

                throw exception;
            }

            return true;
        }

        private bool IsValid()
        {
            if (!IsValidParentheses())
                return false;

            return true;
        }



        private MySQLConditionBuilder Not()
        {
            ThrowIfLocked();

            if (_sql[_sql.Length - 1] != '('
                && _sql[_sql.Length - 1] != ' ')
            {
                _sql.Append(' ');
            }

            _sql.Append("NOT");

            return this;
        }



        public MySQLConditionBuilder Lock()
        {
            _locked = true;

            return this;
        }

        public bool IsEmpty()
        {
            var sql = _sql
                .ToString()
                .TrimEnd();

            if (sql.StartsWith(" WHERE"))
                return sql.Length <= 6;

            return sql.Length == 0;
        }

        public ReadOnlyCollection<MySQLConditionParameter> GetParameters()
        {
            return new ReadOnlyCollection<MySQLConditionParameter>(_parameters);
        }

        public (string Sql, ReadOnlyCollection<MySQLConditionParameter> Parameters) Build()
        {
            string sql;

            if (!IsValid())
            {
                sql = _sql
                    .ToString()
                    .TrimEnd();

                var exception =
                    new FormatException($"An error occurred during the MySQL condition string build because the result string [{sql}] had an incorrect format or was invalid");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));

                throw exception;
            }

            sql = _sql
                .ToString()
                .TrimEnd();

            return (sql, GetParameters());
        }

        public MySQLConditionBuilder Copy()
        {
            return new MySQLConditionBuilder(this);
        }

        public override string ToString()
        {
            return Build().Sql;
        }

        public object Clone()
        {
            return Copy();
        }



        public MySQLConditionBuilder OpenBracket()
        {
            ThrowIfLocked();

            if (_sql[_sql.Length - 1] != '('
                && _sql[_sql.Length - 1] != ' ')
            {
                _sql.Append(' ');
            }

            _sql.Append('(');

            if (ParenthesesMap.ContainsValue("("))
                _parentheses.Push(("(", _sql.Length - 1));

            return this;
        }

        public MySQLConditionBuilder CloseBracket()
        {
            ThrowIfLocked();

            if (_sql[_sql.Length - 1] == ' ')
            {
                _sql.Remove(_sql.Length - 1, 1);
            }

            _sql.Append(')');

            if (ParenthesesMap.TryGetValue(")", out var parenthesisMapped))
            {
                if (_parentheses.Count == 0
                    || _parentheses.Pop().Parenthesis != parenthesisMapped)
                {
                    var sql = _sql
                        .ToString()
                        .TrimEnd();

                    var exception =
                        new InvalidOperationException($"Free open parenthesis of this type for its closing was not found in MySQL condition string [{sql}] at start index [{sql.Length - 1}]");
                    Events.OnError(this,
                        new RErrorEventArgs(exception, exception.Message));

                    throw exception;
                }
            }

            return this;
        }

        public MySQLConditionBuilder And()
        {
            ThrowIfLocked();

            if (_sql[_sql.Length - 1] != '('
                && _sql[_sql.Length - 1] != ' ')
            {
                _sql.Append(' ');
            }

            _sql.Append("AND");

            return this;
        }

        public MySQLConditionBuilder Or()
        {
            ThrowIfLocked();

            if (_sql[_sql.Length - 1] != '('
                && _sql[_sql.Length - 1] != ' ')
            {
                _sql.Append(' ');
            }

            _sql.Append("OR");

            return this;
        }

        public MySQLConditionBuilder IsTrue(string name, string value,
            MySQLComparisonModeType comparisonMode = MySQLComparisonModeType.Equal)
        {
            ThrowIfLocked();

            if (string.IsNullOrEmpty(name))
            {
                var sql = _sql
                    .ToString()
                    .TrimEnd();

                var exception =
                    new ArgumentNullException(nameof(name), $"{nameof(name)} cannot be null for inserting a condition {nameof(IsTrue)} in MySQL condition string [{sql}]");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));

                throw exception;
            }

            if (_sql[_sql.Length - 1] != '('
                && _sql[_sql.Length - 1] != ' ')
            {
                _sql.Append(' ');
            }

            if (comparisonMode == MySQLComparisonModeType.NotEqualNullSafe
                && !(string.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase) || value == null))
            {
                Not()
                    .OpenBracket();
            }

            _sql.Append(name);

            if (string.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase) || value == null)
            {
                switch (comparisonMode)
                {
                    case MySQLComparisonModeType.Equal:
                    case MySQLComparisonModeType.EqualNullSafe:
                    case MySQLComparisonModeType.GreaterThanOrEqual:
                    case MySQLComparisonModeType.LessThanOrEqual:
                        _sql.Append(" IS NULL");
                        break;
                    case MySQLComparisonModeType.NotEqual:
                    case MySQLComparisonModeType.NotEqualNullSafe:
                    case MySQLComparisonModeType.GreaterThan:
                    case MySQLComparisonModeType.LessThan:
                        _sql.Append(" IS NOT NULL");
                        break;
                    default:
                        _sql.Append(" IS NULL");
                        break;
                }

                return this;
            }

            var parameter = new MySQLConditionParameter(
                $"param_condition{_parameters.Count}",
                value);

            ValidateParameterValue(parameter);

            switch (comparisonMode)
            {
                case MySQLComparisonModeType.Equal:
                    _sql.Append(" =");
                    break;
                case MySQLComparisonModeType.EqualNullSafe:
                    _sql.Append(" <=>");
                    break;
                case MySQLComparisonModeType.NotEqual:
                    _sql.Append(" <>");
                    break;
                case MySQLComparisonModeType.NotEqualNullSafe:
                    _sql.Append(" <=>");
                    break;
                case MySQLComparisonModeType.GreaterThan:
                    _sql.Append(" >");
                    break;
                case MySQLComparisonModeType.GreaterThanOrEqual:
                    _sql.Append(" >=");
                    break;
                case MySQLComparisonModeType.LessThan:
                    _sql.Append(" <");
                    break;
                case MySQLComparisonModeType.LessThanOrEqual:
                    _sql.Append(" <=");
                    break;
                default:
                    break;
            }

            _sql.Append(' ')
                .Append(parameter.Name);

            if (comparisonMode == MySQLComparisonModeType.NotEqualNullSafe)
            {
                CloseBracket();
            }

            _parameters.Add(parameter);

            return this;
        }

        public MySQLConditionBuilder IsFalse(string name, string value,
            MySQLComparisonModeType comparisonMode = MySQLComparisonModeType.Equal)
        {
            return Not()
                .OpenBracket()
                .IsTrue(name, value, comparisonMode)
                .CloseBracket();
        }

        public MySQLConditionBuilder IsNull(string name)
        {
            return IsTrue(name, null, MySQLComparisonModeType.Equal);
        }

        public MySQLConditionBuilder IsNotNull(string name)
        {
            return IsTrue(name, null, MySQLComparisonModeType.NotEqual);
        }

        public MySQLConditionBuilder Like(string name, string value,
            char escapeCharacter = '\\')
        {
            ThrowIfLocked();

            if (string.IsNullOrEmpty(name))
            {
                var sql = _sql
                    .ToString()
                    .TrimEnd();

                var exception =
                    new ArgumentNullException(nameof(name), $"{nameof(name)} cannot be null for inserting a condition {nameof(Like)} in MySQL condition string [{sql}]");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));

                throw exception;
            }

            if (_sql[_sql.Length - 1] != '('
                && _sql[_sql.Length - 1] != ' ')
            {
                _sql.Append(' ');
            }

            _sql.Append(name);

            if (string.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase) || value == null)
            {
                _sql.Append(" IS NULL");

                return this;
            }

            var parameter = new MySQLConditionParameter(
                $"param_condition{_parameters.Count}",
                value);

            ValidateParameterValue(parameter);

            _sql.Append(" LIKE ")
                .Append(parameter.Name)
                .Append(" ESCAPE '")
                .Append(escapeCharacter);

            if (escapeCharacter == '\\')
                _sql.Append(escapeCharacter);

            _sql.Append('\'');

            _parameters.Add(parameter);

            return this;
        }

        public MySQLConditionBuilder NotLike(string name, string value,
            char escapeCharacter = '\\')
        {
            return Not()
                .OpenBracket()
                .Like(name, value, escapeCharacter)
                .CloseBracket();
        }



        public static MySQLConditionBuilder Where(
            bool parenthesesAutoComplete = true)
        {
            var builder = new MySQLConditionBuilder(
                parenthesesAutoComplete);

            builder._sql.Append(" WHERE");

            return builder;
        }
    }
}
