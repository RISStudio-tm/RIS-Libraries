// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Collections.Pools
{
    public readonly struct AsyncResourcePoolOptions
    {
        public static readonly TimeSpan DefaultResourceCreationRetryInterval = TimeSpan.FromSeconds(1);
        public const int DefaultNumResourceCreationRetries = 3;

        public int MinNumResources { get; }
        public int MaxNumResources { get; }
        public TimeSpan? ResourcesExpireAfter { get; }
        public int MaxNumResourceCreationAttempts { get; }
        public TimeSpan ResourceCreationRetryInterval { get; }

        public AsyncResourcePoolOptions(
            int minNumResources,
            int maxNumResources = int.MaxValue,
            TimeSpan? resourcesExpireAfter = null,
            int maxNumResourceCreationAttempts = DefaultNumResourceCreationRetries,
            TimeSpan? resourceCreationRetryInterval = null)
        {
            if (minNumResources < 0)
            {
                throw new ArgumentException($"{nameof(minNumResources)} must be >= 0", nameof(minNumResources));
            }

            if (maxNumResources < 1)
            {
                throw new ArgumentException($"{nameof(maxNumResources)} must be > 0", nameof(maxNumResources));
            }

            if (minNumResources > maxNumResources)
            {
                throw new ArgumentException($"{nameof(minNumResources)} must be <= {nameof(maxNumResources)}");
            }

            MinNumResources = minNumResources;
            MaxNumResources = maxNumResources;
            ResourcesExpireAfter = resourcesExpireAfter;
            MaxNumResourceCreationAttempts = maxNumResourceCreationAttempts;
            ResourceCreationRetryInterval = resourceCreationRetryInterval ?? DefaultResourceCreationRetryInterval;
        }
    }
}
