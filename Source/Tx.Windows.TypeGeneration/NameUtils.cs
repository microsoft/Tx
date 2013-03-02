using Microsoft.CSharp;

namespace Tx.Windows
{
    internal class NameUtils
    {
        private static readonly CSharpCodeProvider _provider = new CSharpCodeProvider();

        public static string CreateIdentifier(string s)
        {
            char[] chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                {
                    chars[i] = '_';
                }
            }

            return _provider.CreateValidIdentifier(new string(chars));
        }
    }
}