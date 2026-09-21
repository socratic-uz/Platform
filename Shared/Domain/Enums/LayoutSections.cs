using System;

namespace Domain.Enums
{
    [Flags]
    public enum LayoutSections
    {
        None = 0,
        Header = 1 << 0,
        Footer = 1 << 1,
        Nav = 1 << 2,
        All = Header | Footer | Nav
    }
}
