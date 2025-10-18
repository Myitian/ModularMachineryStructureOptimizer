using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace ModularMachineryStructureOptimizer.Arguments;

public sealed class ArgParser
{
    private readonly string[] args;
    private readonly ArgDefinition[] defs;
    public Dictionary<string, Range> Results { get; } = new(StringComparer.OrdinalIgnoreCase);
    public ArgParser(string[] args, params ArgDefinition[] defs)
    {
        this.args = args;
        this.defs = defs;
        Dictionary<string, ArgDefinition> argMap = new(StringComparer.OrdinalIgnoreCase);
        foreach (ArgDefinition def in defs)
        {
            argMap[def.Name] = def;
            foreach (string alias in def.Aliases)
                argMap[alias] = def;
        }
        for (int i = 0; i < args.Length;)
        {
            string arg = args[i];
            if (!argMap.TryGetValue(arg, out ArgDefinition? argDef))
            {
                i++;
                continue;
            }
            int s = ++i;
            int e = i += argDef.ParamCount;
            Results[argDef.Name] = s..e;
        }
    }
    public bool TryGetSpan(string name, out ReadOnlySpan<string> value)
    {
        if (Results.TryGetValue(name, out Range range)
            && range.GetOffsetAndLength(args.Length) is (int offset, int length)
            && offset <= args.Length)
        {
            value = args.AsSpan(offset, Math.Min(args.Length - offset, length));
            return true;
        }
        value = default;
        return false;
    }
    public bool TryGetString(string name, [NotNullWhen(true)] out string? value)
    {
        if (Results.TryGetValue(name, out Range range)
            && range.GetOffsetAndLength(args.Length) is (int offset, > 0)
            && offset < args.Length)
        {
            value = args[offset];
            return true;
        }
        value = default;
        return false;
    }
    public bool TryGet<T>(string name, out T? value) where T : IParsable<T>
    {
        if (TryGetString(name, out string? s) && T.TryParse(s, null, out value))
            return true;
        value = default;
        return false;
    }
    public bool TryGetEnum<TEnum>(string name, out TEnum value) where TEnum : struct, Enum
    {
        if (TryGetString(name, out string? s) && Enum.TryParse(s, true, out value))
            return true;
        value = default;
        return false;
    }
    public bool TryGetBoolean(string name, out bool value)
    {
        if (TryGetString(name, out string? s))
        {
            if (string.IsNullOrEmpty(s))
                value = false;
            else if (long.TryParse(s, out long i))
                value = i != 0;
            else if ("yes".StartsWith(s, StringComparison.OrdinalIgnoreCase) || "true".StartsWith(s, StringComparison.OrdinalIgnoreCase))
                value = true;
            else if ("no".StartsWith(s, StringComparison.OrdinalIgnoreCase) || "false".StartsWith(s, StringComparison.OrdinalIgnoreCase))
                value = false;
            else
            {
                value = default;
                return false;
            }
            return true;
        }
        value = default;
        return false;
    }
    public void WriteHelp(TextWriter writer)
    {
        int nLen = "Name".Length, cLen = "ParamCount".Length, aLen = "Alias".Length, iLen = "Info".Length;
        foreach (ArgDefinition def in defs)
        {
            int nnLen = def.Name.Length;
            int ncLen = def.ParamCount.ToString().Length;
            int naLen = def.Aliases.Sum(a => a.Length) + Math.Max(def.Aliases.Length - 1, 0) * ", ".Length;
            int niLen = def.Info?.Length ?? 0;
            if (nnLen > nLen)
                nLen = nnLen;
            if (ncLen > cLen)
                cLen = ncLen;
            if (naLen > aLen)
                aLen = naLen;
            if (niLen > iLen)
                iLen = niLen;
        }
        writer.WriteLine("Arguments:");
        writer.WritePadLeft("Name", nLen);
        writer.Write(" | ");
        writer.WritePadLeft("ParamCount", cLen);
        writer.Write(" | ");
        writer.WritePadLeft("Alias", aLen);
        writer.Write(" | ");
        writer.WritePadLeft("Info", iLen);
        foreach (ArgDefinition aDef in defs)
        {
            writer.WriteLine();
            writer.WritePadLeft(aDef.Name, nLen);
            writer.Write(" . ");
            writer.WritePadRight(aDef.ParamCount, cLen);
            writer.Write(" . ");
            writer.WritePadLeft(string.Join(", ", aDef.Aliases), aLen);
            writer.Write(" . ");
            writer.WritePadLeft(aDef.Info ?? "", iLen);
        }
    }
}
static class Extensions
{
    public static void WritePadLeft(this TextWriter writer, string text, int width)
    {
        writer.Write(text);
        writer.Write(new string(' ', Math.Max(width - text.Length, 0)));
    }
    public static void WritePadRight(this TextWriter writer, int number, int width)
    {
        string text = number.ToString();
        writer.Write(new string(' ', Math.Max(width - text.Length, 0)));
        writer.Write(text);
    }
}