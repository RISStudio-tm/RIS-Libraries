using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    public sealed class FileLock
    {
        public static event EventHandler<RMessageEventArgs> ShowMessage;
        public static event EventHandler<RErrorEventArgs> ShowError;

        private const string LockFileExtension = "rlock";

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly FileLockNode _node;
        private readonly string _path;

        public FileLock(string path)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _path = GetLockFileName(path);
            _node = new FileLockNode(_path);
        }
        public FileLock(FileInfo fileInfo)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _path = GetLockFileName(fileInfo);
            _node = new FileLockNode(_path);
        }

        public async Task AddTime(TimeSpan lockTime)
        {
            await _node.TrySetReleaseDate(
                await _node.GetReleaseDate().ConfigureAwait(false) + lockTime
                ).ConfigureAwait(false);
        }

        public async Task<DateTime> GetReleaseDate()
        {
            return await _node.GetReleaseDate().ConfigureAwait(false);
        }

        public async Task<bool> TryAcquire(DateTime releaseDate)
        {
            if (File.Exists(_path) && (await _node.GetReleaseDate().ConfigureAwait(false)).ToUniversalTime() > DateTime.UtcNow)
                return false;

            return await _node.TrySetReleaseDate(releaseDate).ConfigureAwait(false);
        }
        public async Task<bool> TryAcquire(TimeSpan lockTime, bool refreshContinuously = false)
        {
            if (!File.Exists(_path))
                return await _node.TrySetReleaseDate(DateTime.UtcNow + lockTime).ConfigureAwait(false);

            if (File.Exists(_path) && (await _node.GetReleaseDate().ConfigureAwait(false)).ToUniversalTime() > DateTime.UtcNow)
                return false;

            if (!await _node.TrySetReleaseDate(DateTime.UtcNow + lockTime).ConfigureAwait(false))
                return false;

            if (refreshContinuously)
            {
                var refreshTime = (int)(lockTime.TotalMilliseconds * 0.9);
                await Task.Run(async () =>
                {
                    while(!_cancellationTokenSource.IsCancellationRequested)
                    {
                        await AddTime(TimeSpan.FromMilliseconds(refreshTime)).ConfigureAwait(false);
                        await Task.Delay(refreshTime).ConfigureAwait(false);
                    }
                }, _cancellationTokenSource.Token).ConfigureAwait(false);
            }

            return true;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();

            if (File.Exists(_path))
                File.Delete(_path);
        }

        private static string GetLockFileName(string path)
        {
            return $"{Path.GetFileName(path)}.{LockFileExtension}";
        }
        private static string GetLockFileName(FileInfo fileInfo)
        {
            string path = fileInfo.FullName;
            return $"{Path.GetFileName(path)}.{LockFileExtension}";
        }
    }
}
