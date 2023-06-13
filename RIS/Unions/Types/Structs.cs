// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Unions.Types
{
    public struct None 
    {
        public static Union<T, None> Of<T>(T type)
        {
            return new None();
        }
    }
    
    public struct Null
    {
        public static Union<T, Null> Of<T>(T type)
        {
            return new Null();
        }
    }

    public struct Unknown
    {
        public static Union<T, Unknown> Of<T>(T type)
        {
            return new Unknown();
        }
    }



    public struct Yes
    {
        
    }

    public struct No
    {
        
    }

    public struct Maybe
    {
        
    }



    public struct True
    {
        
    }

    public struct False
    {
        
    }



    public struct All
    {
        
    }

    public struct Some
    {
        
    }



    public struct Success
    {
        
    }
    public struct Success<T>
    {
        public T Value { get; }
        
        public Success(T value)
        {
            Value = value;
        }
    }

    public struct Error
    {
        
    }
    public struct Error<T>
    {
        public T Value { get; }
        
        public Error(T value)
        {
            Value = value;
        }
    }



    public struct Result<T>
    {
        public T Value { get; }
        
        public Result(T value)
        {
            Value = value;
        }
    }



    public struct NotFound
    {
        
    }
}
