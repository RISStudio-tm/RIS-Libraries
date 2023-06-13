// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RIS.Pools
{
    public sealed class AsyncResourcePool<TResource>
    {
        private interface IResourceMessage
        {

        }

        private sealed class ResourceRequestMessage : IResourceMessage
        {
            public TaskCompletionSource<ReusableResource<TResource>> TaskCompletionSource { get; }
            public CancellationToken CancellationToken { get; }

            public ResourceRequestMessage(TaskCompletionSource<ReusableResource<TResource>> taskCompletionSource, CancellationToken cancellationToken)
            {
                TaskCompletionSource = taskCompletionSource;
                CancellationToken = cancellationToken;
            }
        }

        private sealed class ResourceAvailableMessage : IResourceMessage
        {
            public TResource Resource { get; }

            public ResourceAvailableMessage(TResource resource)
            {
                Resource = resource;
            }
        }

        private sealed class PurgeExpiredResourcesMessage : IResourceMessage
        {

        }

        private sealed class EnsureAvailableResourcesMessage : IResourceMessage
        {
            public int AttemptNumber { get; }

            public EnsureAvailableResourcesMessage(int attemptNumber = 1)
            {
                AttemptNumber = attemptNumber;
            }
        }

        private sealed class CreateResourceFailedMessage : IResourceMessage
        {
            public Exception Exception { get; }
            public int AttemptNumber { get; }

            public CreateResourceFailedMessage(Exception exception, int attemptNumber)
            {
                Exception = exception;
                AttemptNumber = attemptNumber;
            }
        }

        private sealed class TimestampedResource
        {
            public TResource Resource { get; }
            public DateTime Created { get; }

            private TimestampedResource(TResource resource, DateTime created)
            {
                Resource = resource;
                Created = created;
            }

            public static TimestampedResource Create(TResource resource)
            {
                return new TimestampedResource(resource, DateTime.UtcNow);
            }
        }

        public static event EventHandler<RInformationEventArgs> Information;
        public static event EventHandler<RWarningEventArgs> Warning;
        public static event EventHandler<RErrorEventArgs> Error;

        private readonly int _minNumResources;
        private readonly int _maxNumResources;
        private readonly TimeSpan? _resourcesExpireAfter;
        private readonly int _maxNumResourceCreationAttempts;
        private readonly TimeSpan _resourceCreationRetryInterval;
        private readonly Func<Task<TResource>> _resourceTaskFactory;
        private readonly Queue<TimestampedResource> _availableResources;
        private readonly Queue<ResourceRequestMessage> _pendingResourceRequests;
        private readonly ActionBlock<IResourceMessage> _messageHandler;

        private int _numResources = 0;
        private int _numResourcesInUse = 0;

        public AsyncResourcePool(Func<Task<TResource>> resourceTaskFactory, AsyncResourcePoolOptions options)
        {
            _minNumResources = options.MinNumResources;
            _maxNumResources = options.MaxNumResources;
            _resourcesExpireAfter = options.ResourcesExpireAfter;
            _maxNumResourceCreationAttempts = options.MaxNumResourceCreationAttempts;
            _resourceCreationRetryInterval = options.ResourceCreationRetryInterval;
            _availableResources = new Queue<TimestampedResource>();
            _pendingResourceRequests = new Queue<ResourceRequestMessage>();
            _resourceTaskFactory = resourceTaskFactory;

            // Important: These functions must be called after all instance members have been initialised!
            _messageHandler = GetMessageHandler();
            _ = SetupErrorHandling();
            _ = SetupPeriodicPurge();

            _messageHandler.Post(new EnsureAvailableResourcesMessage());
        }
        public AsyncResourcePool(Func<TResource> resourceFactory, AsyncResourcePoolOptions options)
            : this(() => Task.FromResult(resourceFactory()), options)
        {

        }

        private ActionBlock<IResourceMessage> GetMessageHandler()
        {
            return new ActionBlock<IResourceMessage>(message =>
            {
                switch (message)
                {
                    case ResourceRequestMessage resourceRequest:
                        _pendingResourceRequests.Enqueue(resourceRequest);
                        HandlePendingResourceRequests();
                        break;
                    case ResourceAvailableMessage resourceAvailableMessage:
                        HandleResourceAvailable(resourceAvailableMessage);
                        HandlePendingResourceRequests();
                        break;
                    case PurgeExpiredResourcesMessage purgeExpiredResources:
                        HandlePurgeExpiredResource(purgeExpiredResources);
                        break;
                    case EnsureAvailableResourcesMessage ensureAvailableResourcesMessage:
                        _ = HandleEnsureAvailableResourcesMessage(ensureAvailableResourcesMessage);
                        break;
                    case CreateResourceFailedMessage createResourceFailedMessage:
                        // TODO: Log the exception!
                        if (createResourceFailedMessage.AttemptNumber >= _maxNumResourceCreationAttempts)
                        {
                            ClearAllPendingRequests(createResourceFailedMessage.Exception);
                        }
                        else
                        {
                            Task.Run(async () =>
                            {
                                await Task.Delay(_resourceCreationRetryInterval).ConfigureAwait(false);
                                var nextAttemptNumber = createResourceFailedMessage.AttemptNumber + 1;
                                _messageHandler.Post(new EnsureAvailableResourcesMessage(nextAttemptNumber));
                            });
                        }

                        break;
                    default:
                        throw new InvalidOperationException($"Unhandled {nameof(message)} type: {message.GetType()}");
                }
            });
        }

        private async Task SetupErrorHandling()
        {
            try
            {
                await _messageHandler.Completion.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ClearAllPendingRequests(ex);
            }
        }

        private void ClearAllPendingRequests(Exception ex)
        {
            while (_pendingResourceRequests.Count > 0)
            {
                var request = _pendingResourceRequests.Dequeue();
                request.TaskCompletionSource.SetException(ex);
            }
        }

        private async Task SetupPeriodicPurge()
        {
            if (_resourcesExpireAfter != null)
            {
                using (var purgeCancellationTokenSource = new CancellationTokenSource())
                {
                    var purgeCancellationToken = purgeCancellationTokenSource.Token;

                    _ = Task.Run(async () =>
                    {
                        while (!purgeCancellationToken.IsCancellationRequested)
                        {
                            const int frequency = 10;

                            await Task.Delay(new TimeSpan(_resourcesExpireAfter.Value.Ticks / frequency),
                                purgeCancellationToken).ConfigureAwait(false);
                            var purgeMessage = new PurgeExpiredResourcesMessage();

                            _messageHandler.Post(purgeMessage);
                        }
                    }, purgeCancellationToken);

                    try
                    {
                        await _messageHandler.Completion.ConfigureAwait(false);
                    }
                    finally
                    {
                        purgeCancellationTokenSource.Cancel();
                    }
                }
            }
        }

        private ReusableResource<TResource> TryGetReusableResource()
        {
            ReusableResource<TResource> reusableResource = null;

            while (_availableResources.Count > 0)
            {
                var timestampedResource = _availableResources.Dequeue();
                var resource = timestampedResource.Resource;

                if (IsResourceExpired(timestampedResource))
                    _ = DisposeResource(resource);
                else
                    reusableResource = GetReusableResource(resource);
            }

            _messageHandler.Post(new EnsureAvailableResourcesMessage());

            return reusableResource;
        }

        private void HandlePendingResourceRequests()
        {
            while (_pendingResourceRequests.Count > 0)
            {
                if (_pendingResourceRequests.Peek().CancellationToken.IsCancellationRequested)
                {
                    _pendingResourceRequests.Dequeue();
                    continue;
                }

                var result = TryGetReusableResource();
                if (result != null)
                {
                    var request = _pendingResourceRequests.Dequeue();
                    request.TaskCompletionSource.SetResult(result);
                }
                else
                {
                    break;
                }
            }
        }

        private void HandleResourceAvailable(ResourceAvailableMessage resourceAvailableMessage)
        {
            var resource = resourceAvailableMessage.Resource;
            var timestampedResource = TimestampedResource.Create(resource);

            _availableResources.Enqueue(timestampedResource);
        }

        private void HandlePurgeExpiredResource(PurgeExpiredResourcesMessage purgeExpiredResourcesMessage)
        {
            var nonExpiredResources = new List<TimestampedResource>();

            while (_availableResources.Count > 0)
            {
                var timestampedResource = _availableResources.Dequeue();
                if (IsResourceExpired(timestampedResource))
                    _ = DisposeResource(timestampedResource.Resource);
                else
                    nonExpiredResources.Add(timestampedResource);
            }

            foreach (var timestampedResource in nonExpiredResources)
            {
                _availableResources.Enqueue(timestampedResource);
            }

            _messageHandler.Post(new EnsureAvailableResourcesMessage());
        }

        private async Task HandleEnsureAvailableResourcesMessage(EnsureAvailableResourcesMessage ensureAvailableResourcesMessage)
        {
            var effectiveNumResourcesAvailable = _numResources - _numResourcesInUse;
            var availableResourcesGap = _minNumResources - effectiveNumResourcesAvailable;
            var remainingCapacity = _maxNumResources - _numResources;
            var numResourcesToCreate = System.Math.Max(0, System.Math.Min(availableResourcesGap, remainingCapacity));

            var createResourceTasks = Enumerable.Range(0, numResourcesToCreate)
                .Select(_ => TryCreateResource());

            try
            {
                await Task.WhenAll(createResourceTasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var errorMessage = new CreateResourceFailedMessage(ex, ensureAvailableResourcesMessage.AttemptNumber);
                _messageHandler.Post(errorMessage);
            }
        }

        private async Task TryCreateResource()
        {
            var resourceTask = _resourceTaskFactory();
            try
            {
                Interlocked.Increment(ref _numResources);

                var resource = await resourceTask.ConfigureAwait(false);

                MakeResourceAvailable(resource);
            }
            catch (Exception)
            {
                Interlocked.Decrement(ref _numResources);

                throw;
            }
        }

        private void MakeResourceAvailable(TResource resource)
        {
            var resourceAvailableMessage = new ResourceAvailableMessage(resource);
            if (!_messageHandler.Post(resourceAvailableMessage))
            {
                _ = DisposeResource(resource);
            }
        }

        private bool IsResourceExpired(TimestampedResource timestampedResource)
        {
            return _resourcesExpireAfter != null
                && timestampedResource.Created.Add(_resourcesExpireAfter.Value).ToUniversalTime() < DateTime.UtcNow;
        }

        private ReusableResource<TResource> GetReusableResource(TResource resource)
        {
            Interlocked.Increment(ref _numResourcesInUse);

            return new ReusableResource<TResource>(resource, () =>
            {
                Interlocked.Decrement(ref _numResourcesInUse);
                MakeResourceAvailable(resource);
            });
        }

        public Task<ReusableResource<TResource>> Get(CancellationToken cancellationToken = default)
        {
            var taskCompletionSource = new TaskCompletionSource<ReusableResource<TResource>>();
            var request = new ResourceRequestMessage(taskCompletionSource, cancellationToken);

            if (!_messageHandler.Post(request))
            {
                var exception = new ObjectDisposedException($"Requested a resource on a disposed {nameof(AsyncResourcePool<TResource>)}");
                taskCompletionSource.SetException(exception);
            }

            return taskCompletionSource.Task;
        }

        private async Task DisposeResource(TResource resource)
        {
            Interlocked.Decrement(ref _numResources);

            if (resource is IDisposable disposableResource)
            {
                await Task.Run(() => disposableResource.Dispose()).ConfigureAwait(false);
            }
        }

        public async Task Dispose()
        {
            _messageHandler.Complete();
            await _messageHandler.Completion.ConfigureAwait(false);

            while (_pendingResourceRequests.Count > 0)
            {
                var request = _pendingResourceRequests.Dequeue();
                var exception = new ObjectDisposedException($"Requested a resource on a disposed {nameof(AsyncResourcePool<TResource>)}");
                request.TaskCompletionSource.SetException(exception);
            }

            while (_availableResources.Count > 0)
            {
                var timestampedResource = _availableResources.Dequeue();
                await DisposeResource(timestampedResource.Resource).ConfigureAwait(false);
            }
        }
    }
}