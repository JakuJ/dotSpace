﻿using dotSpace.BaseClasses.Space;
using dotSpace.Interfaces.Space;
using dotSpace.Objects.Utility;

namespace dotSpace.Objects.Space
{
    /// <summary>
    /// Concrete implementation of a tuplespace datastructure.
    /// Represents a strongly typed set of tuples that can be access through pattern matching. Provides methods to query and manipulate the set.
    /// This class imposes fifo ordering on the underlying tuples.
    /// </summary>
    public sealed class SequentialSpace : SpaceBase
    {
        /////////////////////////////////////////////////////////////////////////////////////////////

        #region // Constructors

        /// <summary>
        /// Initializes a new instance of the SequentialSpace class. All tuples will be created using the provided tuple factory;
        /// if none is provided the default TupleFactory will be used.
        /// </summary>
        public SequentialSpace(ITupleFactory tuplefactory = null) : base(tuplefactory ?? new TupleFactory(), new PerTypeBucket())
        {
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////

        #region // Protected Methods

        /// <summary>
        /// Returns the last index contained within the space to force fifo ordering.
        /// </summary>
        protected override int GetIndex(int size)
        {
            return size;
        }

        #endregion
    }
}
