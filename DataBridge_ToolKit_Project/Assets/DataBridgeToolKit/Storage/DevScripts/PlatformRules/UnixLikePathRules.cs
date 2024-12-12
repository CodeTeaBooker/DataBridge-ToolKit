using DataBridgeToolKit.Storage.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace DataBridgeToolKit.Storage.PlatformRules
{
    public sealed class UnixLikePathRules : IPlatformPathRules
    {
        public int MaxPathLength => 4096;

        public int MaxFileNameLength => 255;

        public bool RequiresBasePathCheck => false;

        public StringComparison PathComparison => StringComparison.Ordinal;

        private static readonly HashSet<char> InvalidFileNameChars;

        static UnixLikePathRules()
        {
            InvalidFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars())
            {
                '/'
            };
        }

        public bool IsReservedName(string nameWithoutExtension)
        {
            return false;
        }

        public bool IsValidPathChar(char c)
        {
            return c != '\0';
        }

        public bool IsValidFileNameChar(char c)
        {
            return !InvalidFileNameChars.Contains(c);
        }
    }
}
