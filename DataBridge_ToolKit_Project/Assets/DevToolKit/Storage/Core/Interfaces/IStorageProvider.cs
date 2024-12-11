using System;

namespace DevToolkit.Storage.Core.Interfaces
{
    /// <summary>
    /// Provides factory methods to create storage readers and writers.
    /// </summary>
    public interface IStorageProvider : IDisposable
    {
        /// <summary>
        /// Creates a new instance of a storage reader.
        /// </summary>
        /// <returns>A storage reader instance.</returns>
        IStorageReader CreateReader();

        /// <summary>
        /// Creates a new instance of a storage writer.
        /// </summary>
        /// <returns>A storage writer instance.</returns>
        IStorageWriter CreateWriter();
    }
}
