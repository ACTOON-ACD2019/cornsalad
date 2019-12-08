using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace kornsalad
{
    public static class Renamer
    {
        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string CreateANewName(string directory, string extension)
        {
            var files = Directory.GetFiles(directory);
            var rand = RandomString(20);

            foreach (var file in files)
            {
                if (file == string.Format("{0}.{1}", rand, extension))
                    rand = RandomString(20);
            }

            return string.Format("{0}.{1}", rand, extension);
        }
    }
}
