using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocStack.Converters
{
    public static class FileSizeConverter
    {
        public static string ConvertBytesToString(long bytes)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (bytes == 0)
                return "0 " + suf[0];
            long bytes2 = Math.Abs(bytes);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes2, 1024)));
            double num = Math.Round(bytes2 / Math.Pow(1024, place), 1);
            return (Math.Sign(bytes) * num).ToString() + " " + suf[place];
        }
    }
}
