// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Synchronization
{
    public class LeftRight<TInner>
    {
        private enum LeftRightChoice : byte
        {
            None = 0,
            Left = 1,
            Right = 2
        }

        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        private readonly object _writerLock;
        private readonly TInner _leftInner;
        private readonly TInner _rightInner;
        private readonly LeftRightVersion _leftVersion;
        private readonly LeftRightVersion _rightVersion;
        private LeftRightChoice _innerChoice;
        private LeftRightChoice _versionChoice;

        private LeftRight()
        {
            _writerLock = new object();

            _leftVersion = new LeftRightVersion();
            _rightVersion = new LeftRightVersion();

            _innerChoice = LeftRightChoice.Left;
            _versionChoice = LeftRightChoice.Left;
        }
        public LeftRight(TInner leftInner, TInner rightInner) : this()
        {
            if (leftInner == null)
            {
                var exception = new ArgumentNullException(nameof(leftInner), $"Параметр {nameof(leftInner)} равен null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (rightInner == null)
            {
                var exception = new ArgumentNullException(nameof(rightInner),$"Параметр {nameof(rightInner)} равен null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            _leftInner = leftInner;
            _rightInner = rightInner;
        }
        public LeftRight(Func<TInner> innerFactory) : this()
        {
            _leftInner = innerFactory();
            _rightInner = innerFactory();
        }

        public void OnInformation(RInformationEventArgs e)
        {
            OnInformation(this, e);
        }
        public void OnInformation(object sender, RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public void OnWarning(RWarningEventArgs e)
        {
            OnWarning(this, e);
        }
        public void OnWarning(object sender, RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public void OnError(RErrorEventArgs e)
        {
            OnError(this, e);
        }
        public void OnError(object sender, RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }

        public virtual TResult Read<TResult>(Func<TInner, TResult> reader)
        {
            if (reader == null)
            {
                var exception = new ArgumentNullException(nameof(reader), $"Параметр {nameof(reader)} равен null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            var versionChoice = _versionChoice;
            var version = GetLeftRightVersion(versionChoice);

            version.Arrive();

            var innerInstance = GetInnerObject();
            var result = reader(innerInstance);

            version.Depart();

            return result;
        }

        public virtual TResult Write<TResult>(Func<TInner, TResult> writer)
        {
            if (writer == null)
            {
                var exception = new ArgumentNullException(nameof(writer), $"Параметр {nameof(writer)} равен null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            lock (_writerLock)
            {
                WriteToInnerObject(writer);

                SwapInstanceVersionChoice();

                WaitForEmptyVersion();

                SwapLeftRightVersionChoice();

                WaitForEmptyVersion();

                return WriteToInnerObject(writer);
            }
        }

        private TResult WriteToInnerObject<TResult>(Func<TInner, TResult> writer)
        {
            var innerInstance = GetInnerObjectForWriter();

            return writer(innerInstance);
        }

        private void WaitForEmptyVersion()
        {
            var currentVersionChoice = _versionChoice;
            var toWaitVersion = GetWaitVersion(currentVersionChoice);

            toWaitVersion.WaitForEmptyVersion();
        }

        private LeftRightVersion GetWaitVersion(LeftRightChoice currentVersionChoice)
        {
            return GetLeftRightVersion((currentVersionChoice == LeftRightChoice.Left)
                ? LeftRightChoice.Right
                : LeftRightChoice.Left);
        }

        private TInner GetInnerObjectForWriter()
        {
            return (_innerChoice == LeftRightChoice.Left)
                ? _rightInner
                : _leftInner;
        }

        private TInner GetInnerObject()
        {
            return (_innerChoice == LeftRightChoice.Left)
                ? _leftInner
                : _rightInner;
        }

        private LeftRightVersion GetLeftRightVersion(LeftRightChoice version)
        {
            return (version == LeftRightChoice.Left)
                ? _leftVersion
                : _rightVersion;
        }

        private void SwapLeftRightVersionChoice()
        {
            _versionChoice = (_versionChoice == LeftRightChoice.Left)
                ? LeftRightChoice.Right
                : LeftRightChoice.Left;
        }

        private void SwapInstanceVersionChoice()
        {
            _innerChoice = (_innerChoice == LeftRightChoice.Left)
                ? LeftRightChoice.Right
                : LeftRightChoice.Left;
        }
    }
}