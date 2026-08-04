namespace Pgn.Dms.Web.Services;

/// <summary>Maps file extensions to a Lucide icon name and an accent colour class.</summary>
public static class FileTypes
{
    private static readonly Dictionary<string, (string Icon, string Color)> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["pdf"] = ("file-text", "text-red-500"),
        ["doc"] = ("file-text", "text-blue-500"),
        ["docx"] = ("file-text", "text-blue-500"),
        ["xls"] = ("file-spreadsheet", "text-emerald-500"),
        ["xlsx"] = ("file-spreadsheet", "text-emerald-500"),
        ["csv"] = ("file-spreadsheet", "text-emerald-600"),
        ["ppt"] = ("presentation", "text-orange-500"),
        ["pptx"] = ("presentation", "text-orange-500"),
        ["png"] = ("image", "text-violet-500"),
        ["jpg"] = ("image", "text-violet-500"),
        ["jpeg"] = ("image", "text-violet-500"),
        ["gif"] = ("image", "text-violet-500"),
        ["svg"] = ("image", "text-violet-400"),
        ["zip"] = ("file-archive", "text-amber-500"),
        ["md"] = ("file-code", "text-slate-500"),
        ["txt"] = ("file", "text-slate-500")
    };

    public static string Icon(string extension) =>
        Map.TryGetValue(extension, out var entry) ? entry.Icon : "file";

    public static string Color(string extension) =>
        Map.TryGetValue(extension, out var entry) ? entry.Color : "text-muted-foreground";

    /// <summary>Extensions the upload dialog accepts, as an HTML <c>accept</c> attribute value.</summary>
    public const string AcceptedUploads =
        ".pdf,.doc,.docx,.xls,.xlsx,.csv,.ppt,.pptx,.png,.jpg,.jpeg,.gif,.svg,.zip,.md,.txt";
}
