using System.Collections.Generic;
using System.Linq;
using System.Threading;
using dotSpace.Interfaces.Space;
using dotSpace.BaseClasses.Utility;

namespace dotSpace.Objects.Utility
{
    public class SingleBucket : BucketBase
    {
        private readonly List<ITuple> bucket = new List<ITuple>();
        private readonly ReaderWriterLockSlim bucketLock = new ReaderWriterLockSlim();

        public override (List<ITuple>, ReaderWriterLockSlim) GetBucketAndLock(object[] template) =>
            (bucket, bucketLock);

        public override ITuple Find(List<ITuple> bucket, ReaderWriterLockSlim bucketLock, object[] pattern,
            bool exit = true)
        {
            bucketLock.EnterReadLock();
            var t = bucket.FirstOrDefault();
            if (t != null && !Match(pattern, t.Fields))
            {
                t = null;
            }

            if (exit) bucketLock.ExitReadLock();
            return t;
        }

        public override IEnumerable<ITuple> FindAll(List<ITuple> bucket, ReaderWriterLockSlim bucketLock,
            object[] pattern)
        {
            bucketLock.EnterReadLock();
            IEnumerable<ITuple> t = bucket.TakeWhile(x => Match(pattern, x.Fields)).ToList();
            bucketLock.ExitReadLock();
            return t;
        }
    }
}
