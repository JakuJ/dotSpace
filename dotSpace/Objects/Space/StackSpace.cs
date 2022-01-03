using dotSpace.BaseClasses.Space;
using dotSpace.Interfaces.Space;
using dotSpace.Objects.Utility;

namespace dotSpace.Objects.Space
{
    /// <summary>
    /// Concrete implementation of a tuplespace datastructure.
    /// Represents a strongly typed set of tuples that can be access through pattern matching. Provides methods to query and manipulate the set.
    /// This class imposes lifo ordering on the underlying tuples.
    /// </summary>
    public sealed class StackSpace : SpaceBase
    {
        /// <summary>
        /// Initializes a new instance of the StackSpace class. All tuples will be created using the provided tuple factory;
        /// if none is provided the default TupleFactory will be used.
        /// </summary>
        public StackSpace(ITupleFactory tuplefactory = null) : base(tuplefactory ?? new TupleFactory(),
            new SingleBucket())
        {
        }

        /// <inheritdoc />
        protected override int GetIndex(int size) => 0;
    }
}
