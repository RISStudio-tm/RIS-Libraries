// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Pools
{
    public sealed class ReusableResource<TResource> : IDisposable
    {
        private readonly Action _disposeAction;
        public TResource Resource { get; }

        public ReusableResource(TResource resource, Action disposeAction)
        {
            Resource = resource;
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction();
        }
    }
}
