using System;

namespace DevToolkit.Storage.Core.Interfaces
{
    public interface IPlatformPathRules
    {
        int MaxPathLength { get; }

        int MaxFileNameLength { get; }

        StringComparison PathComparison { get; }

        bool RequiresBasePathCheck { get; }

        bool IsReservedName(string nameWithoutExtension);

        bool IsValidPathChar(char c);

        bool IsValidFileNameChar(char c);
    }
}
