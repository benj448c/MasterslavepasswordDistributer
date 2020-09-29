
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using Librarys.TCPServer;
using MasterDistributer.models;
using Newtonsoft.Json;

namespace MasterDistributer
{
    public class worker : Librarys.TCPServer.AbstractTCPServer
    {
        private Queue<string> notdone = new Queue<string>();
        private List<UserInfo> userInfos;
        private Dictionary<Guid, WorkInfo> working = new Dictionary<Guid, WorkInfo>();
        private List<UserInfoClearText> passwordmatches = new List<UserInfoClearText>();

        private int batch_size = 5000;
        public void readfromfile()
        {
            userInfos = PasswordFileHandler.ReadPasswordFile("passwords.txt");


            using (FileStream fs = new FileStream("webster-dictionary_test.txt", FileMode.Open, FileAccess.Read))

            using (StreamReader dictionary = new StreamReader(fs))
            {
                while (!dictionary.EndOfStream)
                {
                    notdone.Enqueue(dictionary.ReadLine());
                }
            }

            System.Console.WriteLine("Ready to hack!!");
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

            Console.WriteLine("words left : " + notdone.Count);
            Console.WriteLine("passwords found : " + passwordmatches.Count);

            return batch;
        }

        private WorkInfo assignWorker()
        {

            WorkInfo work = new WorkInfo();
            work.WordList = getBatch(batch_size);
            if (work.WordList.Count == 0)
            {
                return null;
            }

            work.Id = Guid.NewGuid();
            work.UsersList = userInfos;
            working.Add(work.Id, work);
            return work;
        }

        protected override void TcpServerWork(StreamReader sr, StreamWriter sw)
        {
            bool hackInProgress = true;

            while (hackInProgress)
            {
                //TODO protocol: 
                //indgående: 
                //Forespørgelse på en workinfo: hack
                //Svar på hack: completed
                //Svar på password fundet : passwordfound
                //udgående:
                //



                
                //Console.WriteLine("listening for message");
                string message = sr.ReadLine();
                Console.WriteLine(message);

                switch (message)
                {
                    


                    case "hack": 
                        WorkInfo w = assignWorker();
                        if (w != null)
                        {
                            string workinfo_string = JsonConvert.SerializeObject(w);
                            Console.WriteLine(w.WordList.Count + w.WordList[0]);
                            Console.WriteLine("Sending hack info!!");
                            sw.WriteLine(workinfo_string);
                            sw.Flush();
                            Console.WriteLine("hack send");
                        }
                        else
                        {
                            sw.WriteLine("nothing to hack");
                        }

                        break;

                    case "passwordfound":
                        string passwordsfound = sr.ReadLine();
                        List<UserInfoClearText> userpassfound =
                            JsonConvert.DeserializeObject<List<UserInfoClearText>>(passwordsfound);

                        foreach (UserInfoClearText passwordmatch in userpassfound)
                        {
                            passwordmatches.Add(passwordmatch);



                            List<UserInfo> usersToRemove = new List<UserInfo>();

                            foreach (UserInfo user in userInfos)
                            {
                                if (user.Username == passwordmatch.UserName)
                                {
                                    usersToRemove.Add(user);
                                }
                            }

                            foreach (UserInfo user in usersToRemove)
                            {
                                userInfos.Remove(user);
                            }
                            

                        }

                        break;

                    case "completed":
                        string completeInfo = sr.ReadLine();
                        Guid id = Guid.Parse(completeInfo);
                        working.Remove(id);
                        break;
                }

                if(working.Count == 0 && notdone.Count == 0 || userInfos.Count == 0)
                {
                    Console.WriteLine("----- Cracking Completed");
                    foreach (UserInfoClearText userinfo in passwordmatches)
                    {
                        Console.WriteLine(userinfo.ToString());
                    }
                    hackInProgress = false;
                     

                    

                }

           
            }


        }
    }
}
