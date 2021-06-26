using System;

public class BigArray<T>
{
    private const int ChunkSize = 1024 * 1024; // Must be 2^n in order for the fastmodule to work and under 0X7FEFFFFF
    private const int FastModulo = ChunkSize - 1;

    public readonly T[][] Data;

    public readonly long Length;

    public BigArray(long length)
    {
        Length = length;
        var chunk = (int) (length / ChunkSize);
        var index = (int) (length % ChunkSize);

        Data = new T[chunk + 1][];
        var i = 0;
        for (; i < chunk; i++)
        {
            Data[i] = new T[ChunkSize];
        }

        Data[i] = new T[index];
    }

    public T this[long i]
    {
        get
        {
            var chunk = (int) (i / ChunkSize);
            var index = (int) (i & FastModulo);
            return Data[chunk][index];
        }
        set
        {
            var chunk = (int) (i / ChunkSize);
            var index = (int) (i & FastModulo);
            Data[chunk][index] = value;
        }
    }
    
    public T[] ToArray()
    {
        if (Length > 2146435071)
        {
            throw new ArgumentException("BigArray is too big to fit into regular array");
        }
            
        var output = new T[Length];
        var offset = 0;
        for (var i = 0; i < Data.Length; i++)
        {
            var chunkLength = Data[i].Length;
            Array.Copy(Data[i], 0, output, offset, chunkLength);
            offset += chunkLength;
        }

        return output;
    }
}