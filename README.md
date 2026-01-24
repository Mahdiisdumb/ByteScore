# ByteScore

A visually stunning Windows application that scans folders and displays their total size with an animated, pulsing digit counter. Great for understanding the disk footprint of your projects!

## Features

✨ **Animated Digit Counter** - Watch numbers roll up in real-time as the app scans your folder
🎨 **Visual Effects** - Random color flashes and pulsing background for an eye-catching display
🔊 **Audio Feedback** - Sound effects play during scanning and completion
📊 **Human-Readable Sizes** - Shows both raw bytes and formatted sizes (KB, MB, GB, etc.)
⚡ **Error Resilient** - Gracefully handles permission errors and inaccessible files
🎯 **Size Categories** - Displays flavor text based on total size (Tiny → Colossal)
🛑 **Cancellable** - Press Escape to cancel scanning at any time

## Requirements

- Windows 10 or later
- .NET 10 Runtime (or .NET 10 SDK if building from source)

## Usage

1. Launch the application
2. Click anywhere on the screen to open the folder browser
3. Select a folder to scan
4. Watch as the app calculates the total size with cool animations!
5. Press **Escape** at any time to exit or cancel

## Building from Source

### Prerequisites
- Visual Studio 2022 or Visual Studio Code
- .NET 10 SDK
- Git (optional)

### Build Steps

```bash
# Clone the repository (if needed)
git clone https://github.com/Mahdiisdumb/ByteScore.git
cd ByteScore

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run

# Build for release (single executable)
dotnet publish -c Release
```

## Project Structure

```
ByteScore/
├── Program.cs              # Application entry point
├── Score.cs                # Main UI form and logic
├── AppConfig.cs            # Centralized configuration and constants
├── SizeFormatter.cs        # Utility for size formatting and flavor text
├── ByteScore.csproj        # Project configuration
├── preNoise.wav            # Sound effect (pre-scan)
├── highscore.wav           # Background music (during scan)
├── endNoise.wav            # Sound effect (completion)
└── README.md               # This file
```

## Configuration

All application constants (colors, sizes, timings) are defined in `AppConfig.cs`:

- **UI Dimensions**: Customize digit size, padding, label heights
- **Animation Timing**: Adjust delay between digit updates
- **Size Categories**: Define threshold values for flavor text
- **Audio Files**: Specify sound effect file paths
- **Colors**: Customize background, text, and pulse colors

## Architecture Improvements

This refactored version includes:

### Code Quality
- **Comprehensive Documentation** - XML comments on all public methods
- **Separation of Concerns** - Logic split into focused methods
- **Constants Management** - All magic numbers moved to `AppConfig`
- **Error Handling** - Try-catch blocks for file access and audio loading
- **Async Operations** - Proper async/await with cancellation support

### Performance
- **Cancellation Token** - Non-blocking cancel with Escape key
- **Reduced Allocations** - Uses char arrays instead of string manipulation
- **UI Updates** - Only updates every 10 files to prevent excessive refreshes
- **Exception Handling** - Per-file error handling doesn't break the scan

### User Experience
- **Human-Readable Output** - Shows sizes in KB, MB, GB format
- **Better Feedback** - Progress indicator and formatted byte display
- **Error Messages** - Clear dialog boxes for permission issues
- **Loading State** - User always knows what's happening

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **Click** | Select folder to scan |
| **Escape** | Cancel scan or exit application |

## Troubleshooting

### Sound effects not playing?
- Ensure `preNoise.wav`, `highscore.wav`, and `endNoise.wav` are in the application directory
- Check your system volume settings
- The application will still work without audio files

### Folder scanning hangs?
- Press Escape to cancel
- Try scanning a smaller folder first
- Check if you have permission to access the folder

### High memory usage?
- This is normal for very large folders with millions of files
- Close other applications to free up memory
- Consider scanning a subfolder instead

## Future Enhancements

- [ ] Clipboard copy of results
- [ ] Folder size breakdown chart
- [ ] Exclude patterns (e.g., .git, node_modules)
- [ ] Themes/dark mode toggle
- [ ] Save scan history
- [ ] Comparison between scans

## License

See LICENSE file for details.

## Contributing

Contributions are welcome! Feel free to submit issues and pull requests.

---

**Made with ❤️ by Mahdi Studios**
Check Your Folders Byte count with ucn music
