namespace BlockSLAE.IO;

public class BinaryFileHelper
{
    public IEnumerable<T> ReadAll<T>(Func<BinaryReader, T> readFunc, string filePath)
    {
        using BinaryReader reader = new(File.Open(filePath, FileMode.Open));

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            yield return readFunc(reader);
        }
    }

    public void WriteAll<T>(IEnumerable<T> data, string filePath, Action<BinaryWriter, T> writeFunc)
    {
        using BinaryWriter writer = new(File.Open(filePath, FileMode.Create));

        foreach (var item in data)
        {
            writeFunc(writer, item);
        }
    }
}