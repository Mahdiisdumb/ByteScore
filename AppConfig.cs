using System;
using System.Drawing;

namespace ByteScore
{
    /// <summary>
    /// Centralized configuration for the ByteScore application.
    /// </summary>
    public static class AppConfig
    {
        // UI Constants
        public const int MaxDigitsDisplay = 20;
        public const int DigitFontSize = 48;
        public const int DigitWidth = 50;
        public const int DigitHeight = 100;
        public const int PanelPadding = 50;
        public const int FlavorTextFontSize = 24;
        public const int FlavorTextHeight = 80;

        // Animation Constants
        public const int FileProcessingDelayMs = 5;
        public const int DigitRollingDelayMs = 15;
        public const double RandomFlashProbability = 0.1;
        public const double DigitRollingFlashProbability = 0.3;

        // Performance Constants
        public const string SearchPatternAllFiles = "*";
        public const int MaxRandomColorValue = 256;
        public const int PulseColorMin = 20;
        public const int PulseColorMax = 60;

        // Size Categories (bytes)
        public const long SizeTiny = 1_000;
        public const long SizeSmall = 1_000_000;
        public const long SizeMedium = 10_000_000;
        public const long SizeLarge = 100_000_000;
        public const long SizeLarger = 1_000_000_000;
        public const long SizeHuge = 10_000_000_000;
        public const long SizeMassive = 100_000_000_000;

        // Colors
        public static readonly Color BackgroundColor = Color.Black;
        public static readonly Color TextColor = Color.White;
        public static readonly Color PulseColor = Color.FromArgb(40, 0, 0);

        // Audio Files
        public const string PreNoiseAudioFile = "preNoise.wav";
        public const string MainMusicAudioFile = "highscore.wav";
        public const string EndNoiseAudioFile = "endNoise.wav";

        // Messages
        public const string InitialPrompt = "Click anywhere to select a folder…";
        public const string ScanningMessage = "Scanning folder…";
        public const string ErrorAccessingFolder = "Error accessing folder. Please check permissions.";
        public const string ErrorNoFilesFound = "No files found in selected folder.";
        public const string NoFolderSelected = "No folder selected.";
    }
}
