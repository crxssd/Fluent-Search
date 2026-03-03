namespace ScreenSearchOnly;

internal static class KeyLabelGenerator
{
    private const string Alphabet = "ASDFGHJKLQWERTYUIOPZXCVBNM";

    public static IReadOnlyList<string> Generate(int count)
    {
        if (count <= 0)
        {
            return Array.Empty<string>();
        }

        var labels = new List<string>(count);

        if (count <= Alphabet.Length)
        {
            for (var i = 0; i < count; i++)
            {
                labels.Add(Alphabet[i].ToString());
            }

            return labels;
        }

        foreach (var first in Alphabet)
        {
            foreach (var second in Alphabet)
            {
                labels.Add($"{first}{second}");
                if (labels.Count >= count)
                {
                    return labels;
                }
            }
        }

        return labels;
    }
}
