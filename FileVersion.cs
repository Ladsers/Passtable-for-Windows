using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Passtable
{
    class FileVersion
    {
        public static char GetChar(int ver, int type)
        {
            byte totalVer = (byte)(ver * 10 + type);
            return Convert.ToChar(totalVer);
        }
    }
}
