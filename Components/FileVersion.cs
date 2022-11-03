using System;

namespace Passtable
{
    internal static class FileVersion
    {
        public static char GetChar(int ver, int type)
        {
            var totalVer = (byte)(ver * 10 + type);
            return Convert.ToChar(totalVer);
        }
    }
}