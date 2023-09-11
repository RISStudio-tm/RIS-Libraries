// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Unions.Types
{
    public class YesOrNo : UnionBase<Yes, No>
    {
        protected YesOrNo(Union<Yes, No> _)
            : base(_)
        {

        }

        public static implicit operator YesOrNo(Yes _)
        {
            return new YesOrNo(_);
        }
        public static implicit operator YesOrNo(No _)
        {
            return new YesOrNo(_);
        }
        public static implicit operator YesOrNo(bool value)
        {
            return new YesOrNo(
                value
                    ? new Yes()
                    : new No());
        }
    }
}