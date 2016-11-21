namespace Tx.Bond
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal static class PathUtils
    {
        public static IEnumerable<string> FlattenIfNeeded(string path)
        {
            var directory = Path.GetDirectoryName(path);
            var name = Path.GetFileName(path);

            if (string.IsNullOrEmpty(name))
            {
                name = "*.*";
            }

            if (name.Contains("*") || name.Contains("?"))
            {
                return Directory.EnumerateFiles(directory, name)
                    .ToArray();
            }

            return new[]
            {
                path,
            };
        }

        public static bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var directory = Path.GetDirectoryName(path);

            if (directory == null || !Directory.Exists(directory))
            {
                return false;
            }

            var filename = Path.GetFileName(path);

            if (string.IsNullOrEmpty(filename))
            {
                return true;
            }

            var invalidCharacters = Path.GetInvalidFileNameChars()
                .Where(i => i != '*' && i != '?')
                .ToArray();

            if (filename.Any(c => invalidCharacters.Contains(c)))
            {
                return false;
            }

            if (filename.Contains('*') || filename.Contains('?'))
            {
                return true;
            }

            var fileExists = File.Exists(path);

            return fileExists;
        }
    }
}
