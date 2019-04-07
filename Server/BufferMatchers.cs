using System.Linq;

public class BufferMatchers
{
    public static bool FindPatternEndsWith(byte[] target, byte[] pattern)
    {
        if (pattern.Length > target.Length)
        {
            return false;
        }

        var chunk = target.Skip(target.Length - pattern.Length)
            .ToArray();

        return chunk.SequenceEqual(pattern);
    }
}