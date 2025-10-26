using System.Globalization;
using BlockSLAE.Storages;
using BlockSLAE.Storages.Structures;

namespace BlockSLAE.IO;

public abstract class SLAEReader
{
    private static readonly Dictionary<BlockMatrixFileNames, string> FilesDictionary = new()
    {
        [BlockMatrixFileNames.Di] = "di",
        [BlockMatrixFileNames.Idi] = "idi",
        [BlockMatrixFileNames.Gg] = "gg",
        [BlockMatrixFileNames.Ijg] = "ijg",
        [BlockMatrixFileNames.Ig] = "ig",
        [BlockMatrixFileNames.Jg] = "jg",
        [BlockMatrixFileNames.RightSide] = "pr",
        [BlockMatrixFileNames.SLAEConfig] = "kuslau",
    };

    public static ComplexEquation BuildEquation(string path, BinaryFileHelper helper)
    {
        if (!Directory.Exists(path))
        {
            return ComplexEquation.None;
        }

        var di = helper
            .ReadAll(func => func.ReadDouble(), Path.Combine(path, FilesDictionary[BlockMatrixFileNames.Di]));
        var idi = helper
            .ReadAll(func => func.ReadInt32(), Path.Combine(path, FilesDictionary[BlockMatrixFileNames.Idi]))
            .Select(el => --el);
        var gg = helper
            .ReadAll(func => func.ReadDouble(), Path.Combine(path, FilesDictionary[BlockMatrixFileNames.Gg]));
        var ijg = helper
            .ReadAll(func => func.ReadInt32(), Path.Combine(path, FilesDictionary[BlockMatrixFileNames.Ijg]))
            .Select(el => --el);
        var ig = helper
            .ReadAll(func => func.ReadInt32(), Path.Combine(path, FilesDictionary[BlockMatrixFileNames.Ig]))
            .Select(el => --el);
        var jg = helper
            .ReadAll(func => func.ReadInt32(), Path.Combine(path, FilesDictionary[BlockMatrixFileNames.Jg]))
            .Select(el => --el);

        var b = helper
            .ReadAll(func => func.ReadDouble(), Path.Combine(path, FilesDictionary[BlockMatrixFileNames.RightSide]));

        var matrix = new BlockMatrix(di, gg, idi, ijg, ig, jg);
        var rightSide = ComplexVector.Create(b);
        var solution = ComplexVector.Create(rightSide.Length);

        return new ComplexEquation(matrix, solution, rightSide);
    }

    public static SLAEConfig ReadSLAEConfig(string path)
    {
        using var reader = new StreamReader(Path.Combine(path, FilesDictionary[BlockMatrixFileNames.SLAEConfig]));

        var values = reader
            .ReadToEnd()
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => double.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture))
            .ToArray();

        return new SLAEConfig((int)values[0],(int)values[2], values[1]);
    }
}