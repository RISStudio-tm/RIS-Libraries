// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Unions.Types
{
    public class TrueOrFalse : UnionBase<True, False>
    {
        protected TrueOrFalse(Union<True, False> _)
            : base(_)
        {

        }

        public static implicit operator TrueOrFalse(True _)
        {
            return new TrueOrFalse(_);
        }
        public static implicit operator TrueOrFalse(False _)
        {
            return new TrueOrFalse(_);
        }
        public static implicit operator TrueOrFalse(bool value)
        {
            return new TrueOrFalse(
                value
                    ? new True()
                    : new False()
            );
        }
    }
}