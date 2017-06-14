﻿using dotSpace.BaseClasses;
using System;

namespace dotSpace.Interfaces
{
    /// <summary>
    /// Defines the operations required to support encoding and decoding messages from objects to a string representation.
    /// </summary>
    public interface IEncoder
    {
        /// <summary>
        /// Composes and returns a new object based on the provided string.
        /// </summary>
        T Deserialize<T>(string json, params Type[] types);
        /// <summary>
        /// Decomposes the passed object into a string.
        /// </summary>
        string Serialize(MessageBase message, params Type[] types);
        /// <summary>
        /// Template method for deserializing and unboxing the interoperable types specified of the string representation into valid .NET primitive types.
        /// </summary>
        MessageBase Decode(string msg);
        /// <summary>
        /// Template method for serializing and boxing the passed message into interoperable types specified as a string.
        /// </summary>
        string Encode(MessageBase message);
    }
}