using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGen
{
    class StringUtils {


        public static string LowFirst(string s)
        {
            return s.Substring(0, 1).ToLower() + s.Substring(1);
        }

        public static string UpFirst(string s)
        {
            if (s.Length == 0) return "";
            return s.Substring(0, 1).ToUpper() + s.Substring(1);
        }

        public static string ToLower(string s)
        {
            return s.ToLower();
        }

        public static string ToUpper(string s)
        {
            return s.ToUpper();
        }

        public static string ToEntityName(string s)
        {
            string[] arr = s.Split('_');

            if (arr.Length > 0)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = UpFirst(arr[i]);
                }
                return string.Join("", arr);
            }
            return s;
        }

        public static string ToentityName(string s)
        {
            return LowFirst(ToEntityName(s));
        }

    }




}
