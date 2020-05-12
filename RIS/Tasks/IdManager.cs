using System;
using System.Threading;

namespace RIS.Tasks
{
    internal static class IdManager<TTag>
    {
        private static int _lastId;

        public static int GetId(ref int id)
        {
            if (id != 0)
                return id;

            int newId;

            do
            {
                newId = Interlocked.Increment(ref _lastId);
            }
            while (newId == 0);

            Interlocked.CompareExchange(ref id, newId, 0);

            return id;
        }
    }
}
