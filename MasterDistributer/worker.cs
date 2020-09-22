using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using Librarys.TCPServer;
using Newtonsoft.Json;

namespace MasterDistributer
{
    public class worker : Librarys.TCPServer.AbstractTCPServer
    {

        private Queue<string> notdone = new Queue<string>();
        private List<UserInfo> userInfos;
        private Dictionary<Guid, WorkInfo> working = new Dictionary<Guid, WorkInfo>();

        public void readfromfile()
        {
            userInfos = PasswordFileHandler.ReadPasswordFile("passwords.txt");


            using (FileStream fs = new FileStream("webster-dictionary.txt", FileMode.Open, FileAccess.Read))

            using (StreamReader dictionary = new StreamReader(fs))
            {
                while (!dictionary.EndOfStream)
                {
                    notdone.Enqueue(dictionary.ReadLine());
                }
            }
        }

        private List<string> getBatch(int wordsCount)
        {
            
            List<string> batch = new List<string>();
            for (int i = 0; i < wordsCount; i++)
            {
                if (notdone.Count == 0)
                {
                    break;
                }
                batch.Add(notdone.Dequeue());
            }
            return batch;
        }

        private WorkInfo assignWorker()
        {
            WorkInfo work = new WorkInfo();
            work.Id = Guid.NewGuid();
            work.UsersList = userInfos;
            work.WordList = getBatch(5000);

            working.Add(work.Id, work);
        }

        protected override void TcpServerWork(StreamReader sr, StreamWriter sw)
        {
            
            //Sends password list
            string s = JsonConvert.SerializeObject(userInfos);
            sw.WriteLine(s);


        }
    }
}
