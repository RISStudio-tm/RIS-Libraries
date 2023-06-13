// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Unions.Types
{
    public class TrueFalseOrNull : UnionBase<True, False, Null>
    {
        protected TrueFalseOrNull(Union<True, False, Null> _)
            : base(_)
        {

        }

        public static implicit operator TrueFalseOrNull(True _)
        {
            return new TrueFalseOrNull(_);
        }
        public static implicit operator TrueFalseOrNull(False _)
        {
            return new TrueFalseOrNull(_);
        }
        public static implicit operator TrueFalseOrNull(Null _)
        {
            return new TrueFalseOrNull(_);
        }
        public static implicit operator TrueFalseOrNull(bool? value)
        {
            return new TrueFalseOrNull(
                value is null
                    ? new Null()
                    : value.Value
                        ? new True()
                        : new False()
            );
        }
    }
}
