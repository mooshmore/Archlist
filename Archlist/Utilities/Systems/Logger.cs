using Archlist;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Utilities.Systems
{
    public static class Logger
    {
        // Place this in app.xaml.cs
        //this.Dispatcher.UnhandledException += Logger.ExceptionLogger;

        private static readonly DirectoryInfo loggerDirectory = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Archlist_ms", "logs"));

        public static void ExceptionLogger(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string currentTime = DateTime.Now.ToString("HH:mm:ss:fff", CultureInfo.InvariantCulture);
            FileInfo logFile = new(Path.Combine(loggerDirectory.FullName, $"{currentDate}.txt"));

            // Create error text
            string exceptionText = $"----------------{currentTime}----------------\n";
            exceptionText += "Error:\n\n";
            exceptionText += e.Exception.Message;
            exceptionText += "\n\nStack trace:\n";
            exceptionText += e.Exception.StackTrace;
            exceptionText += "\n\n\n";

            if (!logFile.Exists)
                File.Create(logFile.FullName).Close();

            string previousText = File.ReadAllText(logFile.FullName);
            File.WriteAllText(logFile.FullName, previousText + exceptionText);
        }

        internal static void Create(App app)
        {
            app.Dispatcher.UnhandledException += ExceptionLogger;
            loggerDirectory.Create();
        }
    }
}
