using DevToolkit.Storage.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace DevToolkit.Storage.PlatformRules
{
    public sealed class WindowsPathRules : IPlatformPathRules
    {
      
        public int MaxPathLength => 260;

        public int MaxFileNameLength => 255;
      
        public StringComparison PathComparison => StringComparison.OrdinalIgnoreCase;

        public bool RequiresBasePathCheck => true;
      
        private static readonly HashSet<char> InvalidFileNameChars;

        private static readonly HashSet<char> InvalidPathChars;

        private static readonly HashSet<string> ReservedNames;

        static WindowsPathRules()
        {
            InvalidFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars());
            InvalidPathChars = new HashSet<char>(Path.GetInvalidPathChars());

            ReservedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CON", "PRN", "AUX", "NUL",
                "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            };
        }

        public bool IsReservedName(string nameWithoutExtension)
        {
            if (string.IsNullOrWhiteSpace(nameWithoutExtension))
                return false;

            return ReservedNames.Contains(nameWithoutExtension);
        }
    
        public bool IsValidPathChar(char c)
        {
            return !InvalidPathChars.Contains(c);
        }

        public bool IsValidFileNameChar(char c)
        {
            return !InvalidFileNameChars.Contains(c);
        }
    }
}
