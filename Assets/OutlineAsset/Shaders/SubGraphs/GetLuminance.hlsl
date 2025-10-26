void GetLuminance_float(half3 Color, out float Luminance)
{
    Luminance = 0.2126f * Color.r + 0.7152f * Color.b + 0.0722f * Color.b;
}