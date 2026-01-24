using System;

namespace ByteScore
{
    /// <summary>
    /// Utility class for formatting byte sizes and calculating flavor text.
    /// </summary>
    public static class SizeFormatter
    {
        /// <summary>
        /// Gets flavor text based on byte size.
        /// </summary>
        public static string GetFlavorText(long bytes)
        {
            if (bytes < AppConfig.SizeTiny) return "Tiny";
            if (bytes < AppConfig.SizeSmall) return "Small";
            if (bytes < AppConfig.SizeMedium) return "Medium";
            if (bytes < AppConfig.SizeLarge) return "Large";
            if (bytes < AppConfig.SizeLarger) return "Larger";
            if (bytes < AppConfig.SizeHuge) return "Huge";
            if (bytes < AppConfig.SizeMassive) return "Massive";
            return "Colossal";
        }

        /// <summary>
        /// Formats bytes to human-readable string (e.g., "1.2 GB").
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Gets the number of digits needed to display a value.
        /// </summary>
        public static int GetDigitCount(long value)
        {
            if (value == 0) return 1;
            return (int)Math.Floor(Math.Log10(Math.Abs(value))) + 1;
        }
    }
}
