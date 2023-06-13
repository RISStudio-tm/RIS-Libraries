// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Unions.Types
{
    public class YesNoOrMaybe : UnionBase<Yes, No, Maybe>
    {
        protected YesNoOrMaybe(Union<Yes, No, Maybe> _)
            : base(_)
        {

        }

        public static implicit operator YesNoOrMaybe(Yes _)
        {
            return new YesNoOrMaybe(_);
        }
        public static implicit operator YesNoOrMaybe(No _)
        {
            return new YesNoOrMaybe(_);
        }
        public static implicit operator YesNoOrMaybe(Maybe _)
        {
            return new YesNoOrMaybe(_);
        }
        public static implicit operator YesNoOrMaybe(bool? value)
        {
            return new YesNoOrMaybe(
                value is null
                    ? new Maybe()
                    : value.Value
                        ? new Yes()
                        : new No()
            );
        }
    }
}