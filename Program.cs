using System;
using System.Windows.Forms; 

namespace ByteScore
{
    /// <summary>
    /// Entry point for the ByteScore application.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable high-DPI support for modern displays
            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            
            // Initialize application configuration
            ApplicationConfiguration.Initialize();
            
            // Run the main form
            Application.Run(new Score());
        }
    }
}