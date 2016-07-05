using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BondEtwDriver
{
    public static class Utilities
    {
        public static void EmptyDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // sure shot way to delete.
                        ForceDeleteFile(file);
                    }
                }
            }
        }

        public static void ForceDeleteFile(string path)
        {
            if (File.Exists(path))
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    stream.Dispose();
                }

                File.Delete(path);
            }
        }

        public static void ForceDeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                EmptyDirectory(path);
                Directory.Delete(path);
            }
        }
    }
}
