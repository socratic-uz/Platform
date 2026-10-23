#nullable enable

namespace Assets.Constants;

/// <summary>
/// Шаблоны форматирования составных CSS-классов и строковых атрибутов для *.razor файлов.
/// Позволяют полностью исключить кавычки в разметке (Zero Quotation Bloat):
/// class=@string.Format(Two, Card, Elevated) вместо class=@($"{Card} {Elevated}")
/// </summary>
public static class Formats
{
    public const string Two = "{0} {1}";
    public const string Three = "{0} {1} {2}";
    public const string Four = "{0} {1} {2} {3}";
    public const string Five = "{0} {1} {2} {3} {4}";
}
