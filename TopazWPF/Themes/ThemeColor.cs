using System;
using System.Windows.Media;

namespace TopazWPF.ColorUtils
{
    /// <summary>
    /// Comprehensive color manipulation and generation for theme system.
    /// Uses LAB color space for perceptually uniform lightness adjustments.
    /// Supports hue rotation for generating Secondary/Tertiary from Primary.
    /// </summary>
    public static class ThemeColor
    {
        #region Constants

        private static readonly double[][] SRGB_TO_XYZ =
        [
            [0.41233895, 0.35762064, 0.18051042],
            [0.2126, 0.7152, 0.0722],
            [0.01932141, 0.11916382, 0.95034478]
        ];

        private static readonly double[][] XYZ_TO_SRGB =
        [
            [3.2413774792388685, -1.5376652402851851, -0.49885366846268053],
            [-0.9691452513005321, 1.8758853451067872, 0.04156585616912061],
            [0.05562093689691305, -0.20395524564742123, 1.0571799111220335]
        ];

        private static readonly double[] WHITE_POINT_D65 = [95.047, 100.0, 108.883];

        #endregion

        #region Primary Generation Functions

        /// <summary>
        /// Generate a complete color palette from a base color.
        /// Includes: Base, Hover (+12% lightness), Pressed (-12% lightness), 
        /// Light (+20% lightness), and Subtle variants (10%, 20%, 25% opacity).
        /// </summary>
        public static ColorPalette GeneratePalette(Color baseColor)
        {
            return new ColorPalette
            {
                Base = baseColor,
                Hover = GenerateLighter(baseColor, 12),
                Pressed = GenerateDarker(baseColor, 12),
                Light = GenerateLighter(baseColor, 20),
                Subtle25 = ApplyOpacity(baseColor, 0.25),
                Subtle50 = ApplyOpacity(baseColor, 0.50),
                Subtle75 = ApplyOpacity(baseColor, 0.75)
            };
        }

        /// <summary>
        /// Generate a lighter variant using LAB color space.
        /// </summary>
        /// <param name="color">Base color</param>
        /// <param name="percentLighter">Amount to increase lightness (0-100)</param>
        public static Color GenerateLighter(Color color, double percentLighter)
        {
            var lab = LabFromColor(color);
            
            // Increase L* value (0-100 scale)
            double newL = Math.Min(100.0, lab[0] + percentLighter);
            
            return ColorFromLab(newL, lab[1], lab[2]);
        }

        /// <summary>
        /// Generate a darker variant using LAB color space.
        /// </summary>
        /// <param name="color">Base color</param>
        /// <param name="percentDarker">Amount to decrease lightness (0-100)</param>
        public static Color GenerateDarker(Color color, double percentDarker)
        {
            var lab = LabFromColor(color);
            
            // Decrease L* value (0-100 scale)
            double newL = Math.Max(0.0, lab[0] - percentDarker);
            
            return ColorFromLab(newL, lab[1], lab[2]);
        }

        /// <summary>
        /// Apply opacity/alpha to a color.
        /// </summary>
        public static Color ApplyOpacity(Color color, double opacity)
        {
            byte alpha = (byte)(255 * Math.Clamp(opacity, 0.0, 1.0));
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        #endregion

        #region Hue Rotation

        /// <summary>
        /// Rotate the hue of a color by a specified number of degrees.
        /// Used for generating Secondary (+120°) and Tertiary (+240°) from Primary.
        /// </summary>
        /// <param name="color">Base color</param>
        /// <param name="degrees">Degrees to rotate (0-360)</param>
        public static Color RotateHue(Color color, double degrees)
        {
            var hsl = HslFromColor(color);
            
            // Rotate hue and wrap around
            double newH = (hsl.H + degrees / 360.0) % 1.0;
            if (newH < 0) newH += 1.0;
            
            return ColorFromHsl(newH, hsl.S, hsl.L);
        }

        /// <summary>
        /// Generate Secondary color from Primary using +120° hue rotation.
        /// </summary>
        public static Color GenerateSecondary(Color primary) => RotateHue(primary, 120);

        /// <summary>
        /// Generate Tertiary color from Primary using +240° hue rotation.
        /// </summary>
        public static Color GenerateTertiary(Color primary) => RotateHue(primary, 240);

        #endregion

        #region Color Space Conversions - LAB

        /// <summary>
        /// Convert WPF Color to LAB color space.
        /// Returns [L, a, b] where L is 0-100, a and b are typically -128 to 127.
        /// </summary>
        public static double[] LabFromColor(Color color)
        {
            // RGB to linear RGB
            double linearR = Linearized(color.R);
            double linearG = Linearized(color.G);
            double linearB = Linearized(color.B);

            // Linear RGB to XYZ
            double[][] m = SRGB_TO_XYZ;
            double x = m[0][0] * linearR + m[0][1] * linearG + m[0][2] * linearB;
            double y = m[1][0] * linearR + m[1][1] * linearG + m[1][2] * linearB;
            double z = m[2][0] * linearR + m[2][1] * linearG + m[2][2] * linearB;

            // XYZ to LAB
            double xNorm = x / WHITE_POINT_D65[0];
            double yNorm = y / WHITE_POINT_D65[1];
            double zNorm = z / WHITE_POINT_D65[2];

            double fx = LabF(xNorm);
            double fy = LabF(yNorm);
            double fz = LabF(zNorm);

            double l = 116.0 * fy - 16.0;
            double a = 500.0 * (fx - fy);
            double b = 200.0 * (fy - fz);

            return [l, a, b];
        }

        /// <summary>
        /// Convert LAB color space to WPF Color.
        /// </summary>
        public static Color ColorFromLab(double l, double a, double b)
        {
            // LAB to XYZ
            double fy = (l + 16.0) / 116.0;
            double fx = a / 500.0 + fy;
            double fz = fy - b / 200.0;

            double xNormalized = LabInvf(fx);
            double yNormalized = LabInvf(fy);
            double zNormalized = LabInvf(fz);

            double x = xNormalized * WHITE_POINT_D65[0];
            double y = yNormalized * WHITE_POINT_D65[1];
            double z = zNormalized * WHITE_POINT_D65[2];

            // XYZ to linear RGB
            double[][] m = XYZ_TO_SRGB;
            double linearR = m[0][0] * x + m[0][1] * y + m[0][2] * z;
            double linearG = m[1][0] * x + m[1][1] * y + m[1][2] * z;
            double linearB = m[2][0] * x + m[2][1] * y + m[2][2] * z;

            // Linear RGB to sRGB
            int red = Delinearized(linearR);
            int grn = Delinearized(linearG);
            int blu = Delinearized(linearB);

            return Color.FromRgb((byte)red, (byte)grn, (byte)blu);
        }

        private static double LabF(double t)
        {
            const double e = 216.0 / 24389.0;
            const double kappa = 24389.0 / 27.0;
            
            if (t > e)
                return Math.Pow(t, 1.0 / 3.0);
            
            return (kappa * t + 16.0) / 116.0;
        }

        private static double LabInvf(double ft)
        {
            const double e = 216.0 / 24389.0;
            const double kappa = 24389.0 / 27.0;
            
            double ft3 = ft * ft * ft;
            
            if (ft3 > e)
                return ft3;
            
            return (116.0 * ft - 16.0) / kappa;
        }

        #endregion

        #region Color Space Conversions - HSL

        /// <summary>
        /// Convert WPF Color to HSL color space.
        /// Returns HslColor with H, S, L in range [0-1].
        /// </summary>
        public static HslColor HslFromColor(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double h = 0;
            double s = 0;
            double l = (max + min) / 2.0;

            if (delta != 0)
            {
                s = l < 0.5 ? delta / (max + min) : delta / (2.0 - max - min);

                if (max == r)
                    h = ((g - b) / delta) + (g < b ? 6 : 0);
                else if (max == g)
                    h = ((b - r) / delta) + 2;
                else
                    h = ((r - g) / delta) + 4;

                h /= 6.0;
            }

            return new HslColor { H = h, S = s, L = l };
        }

        /// <summary>
        /// Convert HSL color space to WPF Color.
        /// </summary>
        public static Color ColorFromHsl(double h, double s, double l)
        {
            double r, g, b;

            if (s == 0)
            {
                r = g = b = l; // Achromatic
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;

                r = HueToRgb(p, q, h + 1.0 / 3.0);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 1.0 / 3.0);
            }

            return Color.FromRgb(
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255)
            );
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
        }

        #endregion

        #region Color Space Conversions - HSV

        /// <summary>
        /// Convert WPF Color to HSV color space.
        /// Returns HsvColor with H, S, V in range [0-1].
        /// </summary>
        public static HsvColor HsvFromColor(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double h = 0;
            double s = max == 0 ? 0 : delta / max;
            double v = max;

            if (delta != 0)
            {
                if (max == r)
                    h = ((g - b) / delta) + (g < b ? 6 : 0);
                else if (max == g)
                    h = ((b - r) / delta) + 2;
                else
                    h = ((r - g) / delta) + 4;

                h /= 6.0;
            }

            return new HsvColor { H = h, S = s, V = v };
        }

        /// <summary>
        /// Convert HSV color space to WPF Color.
        /// </summary>
        public static Color ColorFromHsv(double h, double s, double v)
        {
            double c = v * s;
            double x = c * (1 - Math.Abs((h * 6) % 2 - 1));
            double m = v - c;

            double r = 0, g = 0, b = 0;
            double hSector = h * 6;

            if (hSector < 1) { r = c; g = x; b = 0; }
            else if (hSector < 2) { r = x; g = c; b = 0; }
            else if (hSector < 3) { r = 0; g = c; b = x; }
            else if (hSector < 4) { r = 0; g = x; b = c; }
            else if (hSector < 5) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return Color.FromRgb(
                (byte)((r + m) * 255),
                (byte)((g + m) * 255),
                (byte)((b + m) * 255)
            );
        }

        #endregion

        #region Gamma Correction (sRGB)

        /// <summary>
        /// Linearize an sRGB component (gamma expansion).
        /// Input: 0-255, Output: 0-100 (scaled for XYZ conversion).
        /// </summary>
        private static double Linearized(int rgbComponent)
        {
            double normalized = rgbComponent / 255.0;
            
            if (normalized <= 0.04045)
                return normalized / 12.92 * 100.0;
            
            return Math.Pow((normalized + 0.055) / 1.055, 2.4) * 100.0;
        }

        /// <summary>
        /// Delinearize a linear RGB component (gamma compression).
        /// Input: 0-100, Output: 0-255.
        /// </summary>
        private static int Delinearized(double rgbComponent)
        {
            double normalized = rgbComponent / 100.0;
            double delinearized;
            
            if (normalized <= 0.0031308)
                delinearized = normalized * 12.92;
            else
                delinearized = 1.055 * Math.Pow(normalized, 1.0 / 2.4) - 0.055;
            
            int value = (int)Math.Round(delinearized * 255.0);
            return Math.Clamp(value, 0, 255);
        }

        #endregion

        #region Hex Conversion

        /// <summary>
        /// Parse a hex color string to WPF Color.
        /// Supports formats: "#RRGGBB", "RRGGBB", "#RGB", "RGB".
        /// </summary>
        public static Color ColorFromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Hex string cannot be null or empty.", nameof(hex));

            hex = hex.Trim().TrimStart('#');

            // Handle 3-character shorthand (e.g., "F0A" -> "FF00AA")
            if (hex.Length == 3)
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";

            if (hex.Length != 6)
                throw new ArgumentException("Hex string must be 3 or 6 characters.", nameof(hex));

            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);

            return Color.FromRgb(r, g, b);
        }

        /// <summary>
        /// Convert WPF Color to hex string (e.g., "#2A7AE2").
        /// </summary>
        public static string HexFromColor(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        #endregion
    }

    #region Helper Structs

    /// <summary>
    /// Complete color palette with all variants.
    /// </summary>
    public class ColorPalette
    {
        public Color Base { get; set; }
        public Color Hover { get; set; }
        public Color Pressed { get; set; }
        public Color Lighter { get; set; }
        public Color Light { get; set; }
        public Color Dark { get; set; }
        public Color Darker { get; set; }
        public Color Subtle25 { get; set; }
        public Color Subtle50 { get; set; }
        public Color Subtle75 { get; set; }
    }

    /// <summary>
    /// HSL color representation (Hue, Saturation, Lightness).
    /// All values are in range [0-1].
    /// </summary>
    public struct HslColor
    {
        public double H; // Hue [0-1]
        public double S; // Saturation [0-1]
        public double L; // Lightness [0-1]
    }

    /// <summary>
    /// HSV color representation (Hue, Saturation, Value).
    /// All values are in range [0-1].
    /// </summary>
    public struct HsvColor
    {
        public double H; // Hue [0-1]
        public double S; // Saturation [0-1]
        public double V; // Value [0-1]
    }

    #endregion
}
