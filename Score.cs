using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace ByteScore
{
    /// <summary>
    /// Main form for ByteScore - displays folder size with animated digit counter.
    /// </summary>
    public class Score : Form, IDisposable
    {
        private FlowLayoutPanel digitsPanel;
        private Label flavorText;
        private Label sizeTextLabel;
        private FolderBrowserDialog folderDialog;

        private SoundPlayer preNoisePlayer;
        private SoundPlayer mainMusicPlayer;
        private SoundPlayer endNoisePlayer;

        private readonly Random rand = new Random();
        private CancellationTokenSource cancellationTokenSource;
        private Label[] digitLabels;
        private int fileCount = 0;

        public Score()
        {
            InitializeForm();
            InitializeUI();
            InitializeAudio();
            AttachEventHandlers();
        }

        /// <summary>
        /// Initializes form properties (fullscreen, styling, etc).
        /// </summary>
        private void InitializeForm()
        {
            Text = "ByteScore";
            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppConfig.BackgroundColor;
            StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// Initializes UI controls (panels, labels, etc).
        /// </summary>
        private void InitializeUI()
        {
            folderDialog = new FolderBrowserDialog
            {
                Description = "Select a folder to scan",
                ShowNewFolderButton = false
            };

            // Flavor text label (top description)
            flavorText = new Label
            {
                Text = AppConfig.InitialPrompt,
                ForeColor = AppConfig.TextColor,
                Font = new Font("Consolas", AppConfig.FlavorTextFontSize, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = AppConfig.FlavorTextHeight,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = AppConfig.BackgroundColor
            };
            Controls.Add(flavorText);

            // Size text label (human-readable format)
            sizeTextLabel = new Label
            {
                Text = "",
                ForeColor = Color.Gray,
                Font = new Font("Consolas", 14, FontStyle.Regular),
                Dock = DockStyle.Bottom,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = AppConfig.BackgroundColor
            };
            Controls.Add(sizeTextLabel);

            // Digits panel
            digitsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = AppConfig.BackgroundColor,
                AutoScroll = false,
                Padding = new Padding(AppConfig.PanelPadding)
            };
            Controls.Add(digitsPanel);
        }

        /// <summary>
        /// Initializes audio players with error handling.
        /// </summary>
        private void InitializeAudio()
        {
            try
            {
                preNoisePlayer = new SoundPlayer(AppConfig.PreNoiseAudioFile);
                mainMusicPlayer = new SoundPlayer(AppConfig.MainMusicAudioFile);
                endNoisePlayer = new SoundPlayer(AppConfig.EndNoiseAudioFile);

                // Pre-load audio to catch missing files early
                preNoisePlayer.Load();
                mainMusicPlayer.Load();
                endNoisePlayer.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Warning: Audio files not found. ({ex.Message})", "Audio Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Attaches event handlers to the form.
        /// </summary>
        private void AttachEventHandlers()
        {
            // Click handler for folder selection
            AttachClickHandler(this, async (s, e) => await PickFolderAsync());

            // Escape key to exit
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    cancellationTokenSource?.Cancel();
                    e.Handled = true;
                    Close();
                }
            };
        }

        /// <summary>
        /// Recursively attaches click handler to control and all children.
        /// </summary>
        private void AttachClickHandler(Control control, EventHandler handler)
        {
            control.Click += handler;
            foreach (Control child in control.Controls)
            {
                AttachClickHandler(child, handler);
            }
        }

        /// <summary>
        /// Disposes of resources properly.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cancellationTokenSource?.Dispose();
                preNoisePlayer?.Dispose();
                mainMusicPlayer?.Dispose();
                endNoisePlayer?.Dispose();
                folderDialog?.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Prompts user to select a folder and starts the scanning process.
        /// </summary>
        private async Task PickFolderAsync()
        {
            if (folderDialog.ShowDialog() != DialogResult.OK)
            {
                flavorText.Text = AppConfig.NoFolderSelected;
                return;
            }

            string folder = folderDialog.SelectedPath;
            if (!Directory.Exists(folder))
            {
                MessageBox.Show(AppConfig.ErrorAccessingFolder, "Folder Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();
            await ScanFolderAsync(folder, cancellationTokenSource.Token);
        }

        /// <summary>
        /// Scans folder recursively and animates the result.
        /// </summary>
        private async Task ScanFolderAsync(string folder, CancellationToken cancellationToken)
        {
            try
            {
                PrepareUI();
                PlayStartAudio();

                var files = GetFilesWithErrorHandling(folder);

                if (files.Count == 0)
                {
                    flavorText.Text = AppConfig.ErrorNoFilesFound;
                    mainMusicPlayer?.Stop();
                    return;
                }

                fileCount = files.Count;
                await AnimateFileProcessing(files, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                // Calculate final total
                long totalBytes = CalculateTotalBytes(files);

                // Animate final digit rolling
                await AnimateFinalRolling(totalBytes, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    PlayEndAudio();
                    DisplayFinalResult(totalBytes);
                }
            }
            catch (OperationCanceledException)
            {
                // User cancelled the operation
                mainMusicPlayer?.Stop();
                flavorText.Text = "Cancelled";
                BackColor = AppConfig.BackgroundColor;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error scanning folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                mainMusicPlayer?.Stop();
                flavorText.Text = AppConfig.InitialPrompt;
                BackColor = AppConfig.BackgroundColor;
            }
        }

        /// <summary>
        /// Prepares UI for scanning (clears panel, creates digit labels).
        /// </summary>
        private void PrepareUI()
        {
            digitsPanel.Controls.Clear();
            flavorText.Text = AppConfig.ScanningMessage;
            sizeTextLabel.Text = "";
            fileCount = 0;
            BackColor = AppConfig.BackgroundColor;

            digitLabels = new Label[AppConfig.MaxDigitsDisplay];
            for (int i = 0; i < AppConfig.MaxDigitsDisplay; i++)
            {
                var lbl = new Label
                {
                    Text = "0",
                    ForeColor = AppConfig.TextColor,
                    Font = new Font("Consolas", AppConfig.DigitFontSize, FontStyle.Bold),
                    Width = AppConfig.DigitWidth,
                    Height = AppConfig.DigitHeight,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = AppConfig.BackgroundColor
                };
                digitLabels[i] = lbl;
                digitsPanel.Controls.Add(lbl);
            }
        }

        /// <summary>
        /// Gets all files in folder with error handling for permission issues.
        /// </summary>
        private List<string> GetFilesWithErrorHandling(string folder)
        {
            var files = new List<string>();

            try
            {
                files.AddRange(Directory.GetFiles(folder, AppConfig.SearchPatternAllFiles, SearchOption.AllDirectories));
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access denied to some folders. Partial results shown.", "Permission Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show(AppConfig.ErrorAccessingFolder, "Folder Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return files;
        }

        /// <summary>
        /// Animates the digit counter while processing files.
        /// </summary>
        private async Task AnimateFileProcessing(List<string> files, CancellationToken cancellationToken)
        {
            char[] displayDigits = new string('0', AppConfig.MaxDigitsDisplay).ToCharArray();
            long displayedBytes = 0;

            int processedCount = 0;
            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                long fileSize = GetFileSizeWithErrorHandling(file);
                displayedBytes += fileSize;

                // Update display digits
                UpdateDisplayDigits(displayDigits, displayedBytes);

                // Update UI
                UpdateDigitLabels(displayDigits);
                flavorText.Text = SizeFormatter.GetFlavorText(displayedBytes);
                sizeTextLabel.Text = SizeFormatter.FormatBytes(displayedBytes);
                AnimateBackgroundPulse();

                processedCount++;
                if (processedCount % 10 == 0) // Update less frequently for performance
                {
                    Application.DoEvents();
                }

                await Task.Delay(AppConfig.FileProcessingDelayMs, cancellationToken);
            }
        }

        /// <summary>
        /// Calculates the total bytes for all files.
        /// </summary>
        private long CalculateTotalBytes(List<string> files)
        {
            long total = 0;
            foreach (var file in files)
            {
                total += GetFileSizeWithErrorHandling(file);
            }
            return total;
        }

        /// <summary>
        /// Gets file size with error handling.
        /// </summary>
        private long GetFileSizeWithErrorHandling(string filePath)
        {
            try
            {
                return new FileInfo(filePath).Length;
            }
            catch
            {
                return 0; // Skip files that can't be read
            }
        }

        /// <summary>
        /// Updates the display digit array based on current total.
        /// </summary>
        private void UpdateDisplayDigits(char[] displayDigits, long currentBytes)
        {
            string targetStr = currentBytes.ToString().PadLeft(AppConfig.MaxDigitsDisplay, '0');

            for (int i = 0; i < AppConfig.MaxDigitsDisplay; i++)
            {
                char targetChar = targetStr[i];
                char currentChar = displayDigits[i];

                if (currentChar < targetChar)
                {
                    currentChar++;
                    if (currentChar > targetChar) currentChar = targetChar;
                    displayDigits[i] = currentChar;
                }
                else if (currentChar > targetChar)
                {
                    displayDigits[i] = targetChar;
                }
            }
        }

        /// <summary>
        /// Updates label text and applies random flash effect.
        /// </summary>
        private void UpdateDigitLabels(char[] displayDigits)
        {
            for (int i = 0; i < AppConfig.MaxDigitsDisplay; i++)
            {
                digitLabels[i].Text = displayDigits[i].ToString();

                if (rand.NextDouble() < AppConfig.RandomFlashProbability)
                {
                    digitLabels[i].ForeColor = GetRandomColor();
                }
                else
                {
                    digitLabels[i].ForeColor = AppConfig.TextColor;
                }
            }
        }

        /// <summary>
        /// Animates the final rolling of digits to the exact total.
        /// </summary>
        private async Task AnimateFinalRolling(long totalBytes, CancellationToken cancellationToken)
        {
            string finalStr = totalBytes.ToString().PadLeft(AppConfig.MaxDigitsDisplay, '0');
            bool finished = false;

            while (!finished && !cancellationToken.IsCancellationRequested)
            {
                finished = true;
                for (int i = 0; i < AppConfig.MaxDigitsDisplay; i++)
                {
                    char currentChar = digitLabels[i].Text[0];
                    char targetChar = finalStr[i];

                    if (currentChar != targetChar)
                    {
                        finished = false;
                        if (currentChar < targetChar)
                        {
                            currentChar++;
                            if (currentChar > targetChar) currentChar = targetChar;
                        }
                        else
                        {
                            currentChar = targetChar;
                        }
                        digitLabels[i].Text = currentChar.ToString();

                        if (rand.NextDouble() < AppConfig.DigitRollingFlashProbability)
                        {
                            digitLabels[i].ForeColor = GetRandomColor();
                        }
                        else
                        {
                            digitLabels[i].ForeColor = AppConfig.TextColor;
                        }
                    }
                }

                flavorText.Text = SizeFormatter.GetFlavorText(totalBytes);
                sizeTextLabel.Text = SizeFormatter.FormatBytes(totalBytes);

                await Task.Delay(AppConfig.DigitRollingDelayMs, cancellationToken);
            }
        }

        /// <summary>
        /// Animates background color pulse.
        /// </summary>
        private void AnimateBackgroundPulse()
        {
            int pulse = rand.Next(AppConfig.PulseColorMin, AppConfig.PulseColorMax);
            BackColor = Color.FromArgb(pulse, 0, 0);
        }

        /// <summary>
        /// Gets a random color for flash effects.
        /// </summary>
        private Color GetRandomColor()
        {
            return Color.FromArgb(
                rand.Next(AppConfig.MaxRandomColorValue),
                rand.Next(AppConfig.MaxRandomColorValue),
                rand.Next(AppConfig.MaxRandomColorValue)
            );
        }

        /// <summary>
        /// Displays the final result with celebration message.
        /// </summary>
        private void DisplayFinalResult(long totalBytes)
        {
            BackColor = AppConfig.BackgroundColor;
            string humanReadable = SizeFormatter.FormatBytes(totalBytes);
            flavorText.Text = $"🎉 Complete! 🎉";
            sizeTextLabel.Text = $"Total: {totalBytes:N0} bytes ({humanReadable})";
        }

        /// <summary>
        /// Plays audio at start of scan.
        /// </summary>
        private void PlayStartAudio()
        {
            try
            {
                preNoisePlayer?.PlaySync();
                mainMusicPlayer?.PlayLooping();
            }
            catch (Exception ex)
            {
                // Audio playback is optional; log but don't fail
                System.Diagnostics.Debug.WriteLine($"Audio playback error: {ex.Message}");
            }
        }

        /// <summary>
        /// Plays audio at end of scan.
        /// </summary>
        private void PlayEndAudio()
        {
            try
            {
                mainMusicPlayer?.Stop();
                endNoisePlayer?.Play();
            }
            catch (Exception ex)
            {
                // Audio playback is optional; log but don't fail
                System.Diagnostics.Debug.WriteLine($"Audio playback error: {ex.Message}");
            }
        }
    }
}