using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using dotSpace.BaseClasses.Utility;
using dotSpace.Interfaces.Space;

namespace dotSpace.Objects.Utility
{
    public class PerTypeBucket : BucketBase
    {
        private readonly Dictionary<ulong, List<ITuple>> buckets;
        private readonly Dictionary<ulong, ReaderWriterLockSlim> bucketLocks;

        public override (List<ITuple>, ReaderWriterLockSlim) GetBucketAndLock(object[] template)
        {
            var hash = ComputeHash(template);
            return (GetBucket(hash), GetBucketLock(hash));
        }

        public override ITuple Find(List<ITuple> bucket, ReaderWriterLockSlim bucketLock, object[] pattern,
            bool exit = true)
        {
            bucketLock.EnterReadLock();
            ITuple t = bucket.FirstOrDefault(x => this.Match(pattern, x.Fields));
            if (exit) bucketLock.ExitReadLock();
            return t;
        }

        public override IEnumerable<ITuple> FindAll(List<ITuple> bucket, ReaderWriterLockSlim bucketLock,
            object[] pattern)
        {
            bucketLock.EnterReadLock();
            IEnumerable<ITuple> t = bucket.Where(x => this.Match(pattern, x.Fields)).ToList();
            bucketLock.ExitReadLock();
            return t;
        }

        private List<ITuple> GetBucket(ulong hash)
        {
            if (!buckets.ContainsKey(hash))
            {
                buckets.Add(hash, new List<ITuple>());
            }

            return buckets[hash];
        }

        private ReaderWriterLockSlim GetBucketLock(ulong hash)
        {
            if (!bucketLocks.ContainsKey(hash))
            {
                bucketLocks.Add(hash, new ReaderWriterLockSlim());
            }

            return bucketLocks[hash];
        }

        private ulong ComputeHash(object[] values)
        {
            ulong result = 31;
            foreach (object value in values)
            {
                Type t = value is Type ? (Type) value : value.GetType();
                result = result * 31 + (ulong) t.GetHashCode();
            }

            return result;
        }
    }
}
