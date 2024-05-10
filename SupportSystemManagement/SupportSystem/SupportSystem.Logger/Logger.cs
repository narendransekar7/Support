using System;
using System.IO;

namespace SupportSystem.Logger
{
    public class Logger
    {
        private readonly string logFilePath;

        public Logger(string logFilePath)
        {
            this.logFilePath = logFilePath;

            try
            {
                Console.WriteLine($"create code folder starting" + logFilePath);
                // Ensure the log directory exists; create it if it doesn't
                if (!Directory.Exists(Path.GetDirectoryName(logFilePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
                    Console.WriteLine($"create code folder reached" + logFilePath);
                }


            }
            catch(Exception ex)
            {

                Console.WriteLine($"Create code error: {ex.Message}");
            }




        }

        public void Log(string message)
        {
            try
            {
                // Create or append to the log file
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    // Write the current date/time and the message to the file
                    writer.WriteLine($"[{DateTime.Now}] {message}");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during logging
                Console.WriteLine($"Error while logging: {ex.Message}");
            }
        }
    }


}
