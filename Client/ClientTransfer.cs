using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Client
{
    /// <summary>
    /// CountryData: Class containing country's Covid-19 data returned by server.
    /// </summary>
    internal class CountryData
    {
        public CountryData()
        {
            Name = "Unknown";
            TotalCases = 0;
            TotalDeaths = 0;
            CasesToday = 0;
            DeathsToday = 0;
            Recovered = 0;
        }

        public CountryData(string inputName,
            UInt32 inputTotalCases, UInt32 inputTotalDeaths,
            UInt32 inputCasesToday, UInt16 inputDeathsToday,
            UInt32 inputRecovered)
        {
            Name = inputName;
            TotalCases = inputTotalCases;
            TotalDeaths = inputTotalDeaths;
            CasesToday = inputCasesToday;
            DeathsToday = inputDeathsToday;
            Recovered = inputRecovered;
        }

        public string Name { get; set; }
        public UInt32 TotalCases { get; set; }
        public UInt32 TotalDeaths { get; set; }
        public UInt32 CasesToday { get; set; }
        public UInt16 DeathsToday { get; set; }
        public UInt32 Recovered { get; set; }

    }

    internal class ClientTransfer
    {
        private int PORT_NUM = 9999;
        private static TcpClient client = new TcpClient();
        public string errorString = string.Empty;

        public ClientTransfer()
        {
            if (client == null)
            {
                client.Connect("127.0.0.1", PORT_NUM);
            }
        }

        // Header 3.
        ~ClientTransfer()
        {
            //byte[] buffer = new byte[1];
            //buffer[0] = 3;
            //client.Client.Send(buffer);
            //client.Close();
        }

        public void ClosingApp()
        {
            byte[] buffer = new byte[1];
            buffer[0] = 3;
            try
            {
                client.Client.Send(buffer);
            }
            catch
            {

            }
            client.Close();
        }

        // Constructor.
        public ClientTransfer(string IpAdress)
        {
            if (!client.Connected)
            {
                client.Connect(IpAdress, PORT_NUM);
            }
        }

        // Header 0: Sign in.
        public bool SignIn(string UserName, string Password)
        {
            if (client == null)
            {
                return false;
            }

            try
            {
                byte[] sendingBytes = new byte[1 + 2 + UserName.Length + Password.Length];

                sendingBytes[0] = (byte)0;
                sendingBytes[1] = (byte)(UserName.Length);
                sendingBytes[2] = (byte)(Password.Length);

                for (int i = 0; i < UserName.Length; i++)
                {
                    sendingBytes[3 + i] = (byte)(UserName[i]);
                }

                for (int i = 0; i < Password.Length; i++)
                {
                    sendingBytes[3 + UserName.Length + i] = (byte)(Password[i]);
                }

                client.Client.Send(sendingBytes);
                byte[] checkMatchedInfo = new byte[2]; // Get returned value by server

                if (client.Client.Receive(checkMatchedInfo) != 2)
                    return false;

                if (checkMatchedInfo[0] == 0)
                    return (checkMatchedInfo[1] == 1);
                return false;
            }
            catch (Exception ex)
            {
                errorString = ex.Message;
                return false;
            }
        }

        // Header 1: Sign up.
        public bool SignUp(string UserName, string Password)
        {
            if (client == null)
            {
                return false;
            }

            try
            {
                byte[] sendingBytes = new byte[1 + 2 + UserName.Length + Password.Length];

                sendingBytes[0] = (byte)1;
                sendingBytes[1] = (byte)(UserName.Length);
                sendingBytes[2] = (byte)(Password.Length);

                for (int i = 0; i < UserName.Length; i++)
                {
                    sendingBytes[3 + i] = (byte)(UserName[i]);
                }

                for (int i = 0; i < Password.Length; i++)
                {
                    sendingBytes[3 + UserName.Length + i] = (byte)(Password[i]);
                }

                client.Client.Send(sendingBytes);

                byte[] checkExistInfo = new byte[2];

                if (client.Client.Receive(checkExistInfo) != 2)
                    return false;

                if (checkExistInfo[0] == 1)
                    return (checkExistInfo[1] == 1);
                return false;
            }
            catch (Exception ex)
            {
                errorString = ex.Message;
                return false;
            }
        }

        // Since BitConverter.ToInt() reads bytes by Little Edian order,
        // integers' bytes need to be reversed.
        public static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        public static UInt16 ReverseBytes(UInt16 value)
        {
            return (UInt16)((value >> 8) | (value << 8));
        }

        private string country_name = "";
        // Header 2: User ask for country's data.
        public void RequestCountryData(string CountryName, Int16 Day, Int16 Month, Int16 Year)
        {
            if (client == null)
            {
                return;
            }

            if (CountryName == "" || CountryName == "world")
            {
                CountryName = "World";
            }
            try
            {
                country_name = CountryName;
                byte[] sendingBytes = new byte[1 + 1 + CountryName.Length + 4];

                sendingBytes[0] = (byte)2;
                sendingBytes[1] = (byte)(CountryName.Length);

                for (int i = 0; i < CountryName.Length; i++)
                {
                    sendingBytes[2 + i] = (byte)(CountryName[i]);
                }
                // Add requested date to sending byte array.
                sendingBytes[sendingBytes.Length - 4] = (byte)(Day);
                sendingBytes[sendingBytes.Length - 3] = (byte)(Month);
                sendingBytes[sendingBytes.Length - 2] = (byte)(Year >> 8);
                sendingBytes[sendingBytes.Length - 1] = (byte)(Year);

                client.Client.Send(sendingBytes);
                //stream.Write(sendingBytes, 0, sendingBytes.Length);

                //byte[] CasesData = new byte[20];
                //if (client.Client.Receive(CasesData) != 20)
                //    return Data;

                //if (CasesData[0] != 2 || CasesData[1] != 1)
                //    return Data;
                //// Fill gotten data to a CountryData class and return its value.
                //Data.Name = CountryName;

                //if (!BitConverter.IsLittleEndian)
                //{
                //    Data.TotalCases = ReverseBytes(BitConverter.ToUInt32(CasesData, 2));
                //    Data.TotalDeaths = ReverseBytes(BitConverter.ToUInt32(CasesData, 6));
                //    Data.CasesToday = ReverseBytes(BitConverter.ToUInt32(CasesData, 10));
                //    Data.DeathsToday = ReverseBytes(BitConverter.ToUInt16(CasesData, 14));
                //    Data.Recovered = ReverseBytes(BitConverter.ToUInt32(CasesData, 16));
                //    return Data;
                //}

                //Data.TotalCases = BitConverter.ToUInt32(CasesData, 2);
                //Data.TotalDeaths = BitConverter.ToUInt32(CasesData, 6);
                //Data.CasesToday = BitConverter.ToUInt32(CasesData, 10);
                //Data.DeathsToday = BitConverter.ToUInt16(CasesData, 14);
                //Data.Recovered = BitConverter.ToUInt32(CasesData, 16);

                //return Data;
            }
            catch (Exception ex)
            {
                errorString = ex.Message;
                //return Data;
            }
        }

        private byte[] receiveByte = new byte[1024];

        public int transferData(ref CountryData country)
        {
            if (client == null)
            {
                return -2;
            }
            int size = 0;
            try
            {
                size = client.Client.Receive(receiveByte);
            }
            catch
            {
                return -1;
            }

            int header = receiveByte[0];

            if (header == 3)
            {
                closeConnection();
                return 3;
            }

            if (header == 2)
            {
                if (size != 20)
                    return 2;

                if (receiveByte[1] != 1)
                {
                    country.Name = "Unknown";
                    return 2;
                }
                // Fill gotten data to a CountryData class and return its value.
                country.Name = country_name;

                if (!BitConverter.IsLittleEndian)
                {
                    country.TotalCases = ReverseBytes(BitConverter.ToUInt32(receiveByte, 2));
                    country.TotalDeaths = ReverseBytes(BitConverter.ToUInt32(receiveByte, 6));
                    country.CasesToday = ReverseBytes(BitConverter.ToUInt32(receiveByte, 10));
                    country.DeathsToday = ReverseBytes(BitConverter.ToUInt16(receiveByte, 14));
                    country.Recovered = ReverseBytes(BitConverter.ToUInt32(receiveByte, 16));
                    return 2;
                }

                country.TotalCases = BitConverter.ToUInt32(receiveByte, 2);
                country.TotalDeaths = BitConverter.ToUInt32(receiveByte, 6);
                country.CasesToday = BitConverter.ToUInt32(receiveByte, 10);
                country.DeathsToday = BitConverter.ToUInt16(receiveByte, 14);
                country.Recovered = BitConverter.ToUInt32(receiveByte, 16);

                return 2;
            }
            return 0;
        }

        public void closeConnection()
        {
            byte[] closing = new byte[1] { (byte)3 };
            try
            {
                client.Client.Send(closing);
            }
            catch
            {

            }
        }

        // Header 4: Check Connection.
        //public bool IsStillConnected()
        //{
        //    if (client == null)
        //        return false;
        //    byte[] activeHeader = new byte[1];
        //    activeHeader[0] = (byte)(4);
        //    client.Client.Send(activeHeader);

        //    byte[] checkStillConnect = new byte[1];
        //    try
        //    {
        //        if (client.Client.Send(checkStillConnect) != 1)
        //            return false;
        //        if (checkStillConnect[0] != 4)
        //            return false;
        //    }
        //    catch
        //    {

        //    }
        //    return true;
        //}
    }
}
