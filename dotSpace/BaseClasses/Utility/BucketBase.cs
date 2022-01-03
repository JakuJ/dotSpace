using System;
using System.Collections.Generic;
using System.Threading;
using dotSpace.Interfaces.Space;

namespace dotSpace.BaseClasses.Utility
{
    public abstract class BucketBase
    {
        private static bool IsOfType(Type tupleType, Type patternType) => tupleType == patternType;

        public abstract (List<ITuple>, ReaderWriterLockSlim) GetBucketAndLock(object[] template);

        public abstract ITuple Find(List<ITuple> bucket, ReaderWriterLockSlim bucketLock, object[] pattern,
            bool exit = true);

        protected bool Match(object[] pattern, object[] tuple)
        {
            if (tuple.Length != pattern.Length)
            {
                return false;
            }

            bool result = true;
            for (int idx = 0; idx < tuple.Length; idx++)
            {
                if (pattern[idx] is Type)
                {
                    Type tupleType = tuple[idx] is Type ? (Type) tuple[idx] : tuple[idx].GetType();
                    result &= IsOfType(tupleType, (Type) pattern[idx]);
                }
                else
                {
                    result &= tuple[idx].Equals(pattern[idx]);
                }

                if (!result) return false;
            }

            return result;
        }

        public abstract IEnumerable<ITuple> FindAll(List<ITuple> bucket, ReaderWriterLockSlim bucketLock,
            object[] pattern);
    }
}
