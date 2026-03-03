namespace ScreenSearchOnly;

internal static class KeyLabelGenerator
{
    private const string Alphabet = "ASDFGHJKLQWERTYUIOPZXCVBNM";

    public static IReadOnlyList<string> Generate(int count)
    {
        var labels = new List<string>(count);
        var length = 1;

        while (labels.Count < count)
        {
            AppendLabels(labels, string.Empty, length, count);
            length++;
        }

        return labels;
    }

    private static void AppendLabels(List<string> labels, string prefix, int remainingLength, int max)
    {
        if (labels.Count >= max)
        {
            return;
        }

        if (remainingLength == 0)
        {
            labels.Add(prefix);
            return;
        }

        foreach (var ch in Alphabet)
        {
            AppendLabels(labels, prefix + ch, remainingLength - 1, max);
            if (labels.Count >= max)
            {
                return;
            }
        }
    }
}
