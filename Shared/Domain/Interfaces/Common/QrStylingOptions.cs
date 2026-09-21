using System;

namespace Domain.Interfaces.Common;

public class QrStylingOptions : IEquatable<QrStylingOptions>
{
    public int Size { get; set; } = 320;
    public string PrimaryColor { get; set; } = "#3f51b5";
    public string BackgroundColor { get; set; } = "#ffffff";
    public string CornerColor { get; set; } = "#1a237e";
    public string CornerDotColor { get; set; } = "#3f51b5";
    public string DotType { get; set; } = "rounded";
    public string CornerType { get; set; } = "extra-rounded";
    public string CornerDotType { get; set; } = "dot";
    public double ImageSizeFactor { get; set; } = 0.4;
    public int Margin { get; set; } = 6;

    public bool UseBackgroundGradient { get; set; } = false;
    public string BackgroundGradientType { get; set; } = "linear";
    public int BackgroundGradientRotation { get; set; } = 0;
    public string BackgroundGradientStartColor { get; set; } = "#ffffff";
    public string BackgroundGradientEndColor { get; set; } = "#eceff1";

    public bool Equals(QrStylingOptions? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Size == other.Size &&
               PrimaryColor == other.PrimaryColor &&
               BackgroundColor == other.BackgroundColor &&
               CornerColor == other.CornerColor &&
               CornerDotColor == other.CornerDotColor &&
               DotType == other.DotType &&
               CornerType == other.CornerType &&
               CornerDotType == other.CornerDotType &&
               Math.Abs(ImageSizeFactor - other.ImageSizeFactor) < 0.001 &&
               Margin == other.Margin &&
               UseBackgroundGradient == other.UseBackgroundGradient &&
               BackgroundGradientType == other.BackgroundGradientType &&
               BackgroundGradientRotation == other.BackgroundGradientRotation &&
               BackgroundGradientStartColor == other.BackgroundGradientStartColor &&
               BackgroundGradientEndColor == other.BackgroundGradientEndColor;
    }

    public override bool Equals(object? obj) => Equals(obj as QrStylingOptions);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Size);
        hash.Add(PrimaryColor);
        hash.Add(BackgroundColor);
        hash.Add(CornerColor);
        hash.Add(CornerDotColor);
        hash.Add(DotType);
        hash.Add(CornerType);
        hash.Add(CornerDotType);
        hash.Add(ImageSizeFactor);
        hash.Add(Margin);
        hash.Add(UseBackgroundGradient);
        hash.Add(BackgroundGradientType);
        hash.Add(BackgroundGradientRotation);
        hash.Add(BackgroundGradientStartColor);
        hash.Add(BackgroundGradientEndColor);
        return hash.ToHashCode();
    }

    public QrStylingOptions Clone()
    {
        return new QrStylingOptions
        {
            Size = this.Size,
            PrimaryColor = this.PrimaryColor,
            BackgroundColor = this.BackgroundColor,
            CornerColor = this.CornerColor,
            CornerDotColor = this.CornerDotColor,
            DotType = this.DotType,
            CornerType = this.CornerType,
            CornerDotType = this.CornerDotType,
            ImageSizeFactor = this.ImageSizeFactor,
            Margin = this.Margin,
            UseBackgroundGradient = this.UseBackgroundGradient,
            BackgroundGradientType = this.BackgroundGradientType,
            BackgroundGradientRotation = this.BackgroundGradientRotation,
            BackgroundGradientStartColor = this.BackgroundGradientStartColor,
            BackgroundGradientEndColor = this.BackgroundGradientEndColor
        };
    }
}
