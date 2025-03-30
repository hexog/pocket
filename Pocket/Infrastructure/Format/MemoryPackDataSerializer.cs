using MemoryPack;
using Microsoft.AspNetCore.Authentication;

namespace Pocket.Infrastructure.Format;

public class MemoryPackDataSerializer<T> : IDataSerializer<T>
{
    public byte[] Serialize(T model)
    {
        return MemoryPackSerializer.Serialize(model);
    }

    public T? Deserialize(byte[] data)
    {
        return MemoryPackSerializer.Deserialize<T>(data);
    }
}