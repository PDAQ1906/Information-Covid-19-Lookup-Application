using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using Leaf.xNet;


namespace Server
{

    public class Data
    {
        public string? country { get; set; }
        public string? cases { get; set; }
        public string? todayCases { get; set; }
        public string? deaths { get; set; }
        public string? todayDeaths { get; set; }
        public string? recovered { get; set; }
        public string? active { get; set; }
        public string? critical { get; set; }
        public string? casesPerOneMillion { get; set; }
        public string? deathsPerOneMillion { get; set; }
        public string? totalTests { get; set; }
        public string? testsPerOneMillion { get; set; }
    }

    public class User
    {
        public string ID { get; set; }
        public string PASSWD { get; set; }

    }
    public class ToDo
    {
        public int SignUp(User user, string filePath, List<User> users)
        {
            // check for duplicate IDs
            foreach (var item in users)
            {
                if (item.ID == user.ID)
                    return 0;
            }
            // add new user to the list and write in file .json
            users.Add(user);
            string str = JsonConvert.SerializeObject(users);
            File.WriteAllText(filePath, str);
            return 1;
        }

        public int SignIn(User user, List<User> users)
        {
            // check ID and password
            foreach (var item in users)
            {
                // Sign in successfully
                if (item.ID == user.ID && item.PASSWD == user.PASSWD)
                    return 1;
            }
            // the user is not existence or the password is wrong
            return 0;
        }

        public Data Search(string country, List<Data> data)
        {
            foreach (var item in data)
            {
                if (item.country == country)
                    return item;
            }
            // The country is not existence
            return null;
        }
    }
    public class ServerTransfer
    {

        private const int BUFFER_SIZE = 1024;
        public const int PORT_NUMBER = 9999;

        static bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        public static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        public static UInt16 ReverseBytes(UInt16 value)
        {
            return (UInt16)((value >> 8) | (value << 8));
        }
        public byte[] SignIn(byte[] receivingBytes, ToDo toDo, List<User> users)
        {
            byte lengthID = receivingBytes[1];
            byte lengthPwd = receivingBytes[2];
            string id = "";
            string passwd = "";
            // read string ID
            for (int i = 0; i < lengthID; i++)
            {
                id += (char)receivingBytes[3 + i];
            }
            // read string Password
            for (int j = 0; j < lengthPwd; j++)
            {
                passwd += (char)receivingBytes[3 + lengthID + j];
            }

            User user = new User { ID = id, PASSWD = passwd };

            byte[] res = new byte[2];
            res[0] = 0;
            res[1] = (byte)toDo.SignIn(user, users);
            return res;
        }

        public byte[] SignUp(byte[] receivingBytes, ToDo toDo, List<User> users, string filePath)
        {
            byte lengthID = receivingBytes[1];
            byte lengthPwd = receivingBytes[2];
            string id = "";
            string passwd = "";
            // read string ID
            for (int i = 0; i < lengthID; i++)
            {
                id += (char)receivingBytes[3 + i];
            }
            // read string Password
            for (int j = 0; j < lengthPwd; j++)
            {
                passwd += (char)receivingBytes[3 + lengthID + j];
            }

            User user = new User { ID = id, PASSWD = passwd };

            byte[] res = new byte[2];
            res[0] = 1;
            res[1] = (byte)toDo.SignUp(user, filePath, users);

            return res;
        }

        public byte[] Search(byte[] receivingBytes, int lengthReceivingBytes, ToDo toDo, ref string name_string)
        {
            // read string country name
            byte lengthCountryName = receivingBytes[1];
            string countryName = "";
            for (int i = 0; i < lengthCountryName; i++)
            {
                countryName += (char)receivingBytes[2 + i];
            }
            name_string = countryName;
            // read date need search
            string date = "Data/";

            int day = (int)receivingBytes[lengthReceivingBytes - 4];
            int month = (int)receivingBytes[lengthReceivingBytes - 3];
            byte[] year = { receivingBytes[lengthReceivingBytes - 2], receivingBytes[lengthReceivingBytes - 1] };
            int Year = (year[0] << 8) | year[1];

            date += Year.ToString();
            date += month.ToString();
            date += day.ToString();
            date += ".json";
            byte[] res = new byte[20];
            // header = 2
            res[0] = (byte)2;
            for (int i = 1; i < 20; i++)
            {
                res[i] = (byte)0;
            }

            // read file Data.json to string and convert to obj (List<Data>)
            if (!File.Exists(date))
                return res;
            string dateJson = File.ReadAllText(date);
            //Send result

            // check data 
            List<Data> json = JsonConvert.DeserializeObject<List<Data>>(dateJson);
            //Search
            Data countryInfor = toDo.Search(countryName, json);
            // country is existence
            if (countryInfor != null)
            {
                res[1] = 1;
                // send infor
                UInt32 totalCases = UInt32.Parse(countryInfor.cases);
                UInt32 totalDeaths = UInt32.Parse(countryInfor.deaths);
                UInt32 casesToday = UInt32.Parse(countryInfor.todayCases);
                UInt16 deathsToday = UInt16.Parse(countryInfor.todayDeaths);
                UInt32 recoveredCases = UInt32.Parse(countryInfor.recovered);
                if (!BitConverter.IsLittleEndian) // computer's architecture affects how bytes are send/read
                {
                    ReverseBytes(totalCases);
                    ReverseBytes(totalDeaths);
                    ReverseBytes(casesToday);
                    ReverseBytes(deathsToday);
                    ReverseBytes(recoveredCases);
                }
                byte[] tempTotalCases = BitConverter.GetBytes(totalCases);
                byte[] tempTotalDeaths = BitConverter.GetBytes(totalDeaths);
                byte[] tempcasesToday = BitConverter.GetBytes(casesToday);
                byte[] tempDeathsToday = BitConverter.GetBytes(deathsToday);
                byte[] tempRecoveredCases = BitConverter.GetBytes(recoveredCases);

                for (int i = 0; i < tempTotalCases.Length; i++)
                {
                    res[2 + i] = tempTotalCases[i];
                    res[6 + i] = tempTotalDeaths[i];
                    res[10 + i] = tempcasesToday[i];
                    res[16 + i] = tempRecoveredCases[i];
                }
                for (int j = 0; j < tempDeathsToday.Length; j++)
                {
                    res[14 + j] = tempDeathsToday[j];
                }

            }
            return res;
        }

        public int Receive(Socket socket, byte[] receivingBytes)
        {
            int lengthReceivingBytes = socket.Receive(receivingBytes);
            return lengthReceivingBytes;
        }

        public List<User> Users(string filePath)
        {
            String tempUsers = File.ReadAllText(filePath);
            List<User> users = JsonConvert.DeserializeObject<List<User>>(tempUsers);
            return users;
        }
        public static int RespondClient(Socket socket, ref Server.ActionTracking actionTracking)
        {
            ServerTransfer server = new ServerTransfer();
            //Receive
            byte[] receivingBytes = new byte[1024];
            int lengthReceivingBytes = 0;
            try
            {
                lengthReceivingBytes = server.Receive(socket, receivingBytes);
            }
            catch
            {
                actionTracking.Action = "Client disconnected.";
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                return 1;
            }
            int header = receivingBytes[0];
            string name_string = "";
            //To Do
            ToDo toDo = new ToDo();

            // Action Tracking is used to report actions to server
            actionTracking.Date = DateTime.Now.ToString("HH:mm:ss, dd/MM/yyyy");
            actionTracking.IP = (((IPEndPoint)socket.RemoteEndPoint).Address).ToString();
            actionTracking.Port = (((IPEndPoint)socket.RemoteEndPoint).Port).ToString();
            try
            {
                // read file Users.json to string and convert to obj (List<User>)
                string filePath = "Data/Users.json";
                List<User> users = server.Users(filePath);
                // Sign in
                if (header == 0)
                {
                    byte[] res = server.SignIn(receivingBytes, toDo, users);
                    socket.Send(res);
                    actionTracking.Action = "Client signed in.";
                }
                // Sign up
                else if (header == 1)
                {
                    byte[] res = server.SignUp(receivingBytes, toDo, users, filePath);
                    socket.Send(res);
                    actionTracking.Action = "Client signed up.";
                }
                // Search
                else if (header == 2)
                {
                    byte[] res = server.Search(receivingBytes, lengthReceivingBytes, toDo, ref name_string);
                    socket.Send(res);
                    actionTracking.Action = "Client searched for \"" + name_string + "\".";
                }
                else if (header == 3)
                {
                    actionTracking.Action = "Client closed connection.";
                    socket.Disconnect(true);
                    socket.Close();
                    return 1;
                }
            }
            catch
            {
                actionTracking.Action = "Client disconnected.";
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                return 1;
            }
            return 0;
        }
        public static void UpdateData()
        {
            try
            {
                HttpRequest http = new HttpRequest();
                DateTime date = DateTime.Now;
                string fileName = "Data/";
                fileName += date.Year.ToString() + date.Month.ToString() + date.Day.ToString() + ".json";
                string json = http.Get("https://coronavirus-19-api.herokuapp.com/countries").ToString();
                if (json != null)
                    File.WriteAllText(fileName, json);
            }
            catch
            {

            }
            Thread.Sleep(TimeSpan.FromMinutes(60));
        }
    }
}
