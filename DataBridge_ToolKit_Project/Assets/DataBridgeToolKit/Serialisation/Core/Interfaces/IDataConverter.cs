using System.Threading;
using System.Threading.Tasks;

namespace DevToolkit.Serialization.Core.Interfaces
{
    public interface IDataConverter<TData>
    {
        string ContentType { get; }
        string FileExtension { get; }

        byte[] ToBytes(TData data);
        TData FromBytes(byte[] data);

        Task<byte[]> ToBytesAsync(TData data, CancellationToken token);
        Task<TData> FromBytesAsync(byte[] data, CancellationToken token);

    }
}


