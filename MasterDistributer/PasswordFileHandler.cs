using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MasterDistributer
{
    public class PasswordFileHandler
    {
        public static List<UserInfo> ReadPasswordFile(String filename)
        {
            List<UserInfo> result = new List<UserInfo>();

            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using (StreamReader sr = new StreamReader(fs))
            {

                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();
                    String[] parts = line.Split(":".ToCharArray());
                    UserInfo userInfo = new UserInfo(parts[0], parts[1]);
                    result.Add(userInfo);
                }
                return result;
            }
        }
    }
}
