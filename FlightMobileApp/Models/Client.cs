using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FlightMobileApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlightMobileApp.Model
{
    interface IClient
    {
        bool Connect(string ip, int port);
        void Write(string command);
        string Read();
        void Disconnect();
    }

    class FlightGearClient : IClient
    {
        private static FlightGearClient instance;
        private readonly BlockingCollection<AsyncCommand> queue;
        // Tcp client.
        private TcpClient tcp_client;
        // Data stream.
        private NetworkStream stream;

        private bool isConected = false;

        // Lock synchronization object
        private static readonly object syncLock = new object();

        // Constructor (protected)
        protected FlightGearClient()
        {
            this.queue = new BlockingCollection<AsyncCommand>();
            //start a new task 
            this.Start();
        }

        public static FlightGearClient GetFlightGearClient()
        {
            if (instance == null)
            {
                lock (syncLock)
                {
                    if (instance == null)
                    {
                        instance = new FlightGearClient();
                    }
                }
            }
            return instance;
        }

        public Task<Result> Execute(Command cmd)
        {
            var asyncCommand = new AsyncCommand(cmd);
            this.queue.Add(asyncCommand);
            return asyncCommand.Task;
        }

        private void Start()
        {
            Console.WriteLine("creating thread of simlator");
            Task.Factory.StartNew(ProcessCommands);
        }

        public void ProcessCommands()
        {
            foreach (AsyncCommand aCommand in this.queue.GetConsumingEnumerable())
            {
                Result res = Result.Ok;
                if (!Connect("127.0.0.1", 5404))
                {
                    aCommand.Completion.SetException(new Exception("failed Conecting to FlightGear"));
                    continue;
                }
                try
                {
                    if (!SetValueAileron(aCommand))
                    {
                        aCommand.Completion.SetException(new Exception("Error - can not set ailron"));
                        continue;
                    }
                    if (!SetValueElevator(aCommand))
                    {
                        aCommand.Completion.SetException(new Exception("Error - can not set elevator"));
                        continue;
                    }
                    if (!SetValueThrottle(aCommand))
                    {
                        aCommand.Completion.SetException(new Exception("Error - can not set throttle"));
                        continue;
                    }
                    if (!SetValueRudder(aCommand))
                    {
                        aCommand.Completion.SetException(new Exception("Error - can not set rudder"));
                        continue;
                    }

                } catch (Exception e)
                {
                    aCommand.Completion.SetException(e);
                    continue;
                }
                aCommand.Completion.SetResult(res);
            }
        }

        private bool SetValueAileron(AsyncCommand aCommand)
        {
            // Aileron
            double queryValue = aCommand.Command.Aileron;
            Write("set" + aCommand.Command.ParseAileronToString());
            Write("get /controls/flight/aileron\n");
            if (!IsValidData(queryValue, Read()))
            {
                return false;
            }
            return true;
        }

        private bool SetValueElevator(AsyncCommand aCommand)
        {
            // Elevator
            double queryValue = aCommand.Command.Elevator;
            Write("set" + aCommand.Command.ParseElevatorToString());
            Write("get /controls/flight/elevator\n");
            if (!IsValidData(queryValue, Read()))
            {
                return false;
            }
            return true;
        }

        private bool SetValueRudder(AsyncCommand aCommand)
        { 
            // Rudder
            double queryValue = aCommand.Command.Rudder;
            Write("set" + aCommand.Command.ParseRudderToString());
            Write("get /controls/flight/rudder\n");
            if (!IsValidData(queryValue, Read()))
            {
                return false;
            }
            return true;
        }
        private bool SetValueThrottle(AsyncCommand aCommand) { 
            // Throttle
            double queryValue = aCommand.Command.Throttle;
            Write("get /controls/engines/current-engine/throttle\n");
            if (!IsValidData(queryValue, Read()))
            {
                return false;
            }
            return true;
            
        }

        public bool IsValidData(double val,string recieve)
        {
            if (val == Convert.ToDouble(recieve))
            {
                return true;
            }
            return false;
        }

        public bool Connect(string ip, int port)
        {
            try
            {

                    if (isConected == false)
                {
                    this.tcp_client = new TcpClient(ip, port);
                    Console.WriteLine("Establishing Connection");
                    Console.WriteLine("Server Connected");
                    this.stream = tcp_client.GetStream();
                    // first command to change PROMPT
                    Write("data\n");
                    isConected = true;
                    // Set timeout.
                    tcp_client.ReceiveTimeout = 10000;
                    tcp_client.SendTimeout = 10000;


                }
                if (isConected)
                {
                    bool isConnect = tcp_client.Connected;
                    Console.WriteLine("connected");
                }
            } catch( Exception )
            {
                return false;
            }
            return true;
        }

        public void Disconnect()
        {
            if (stream != null)
            {
                this.stream.Close();
            }
            if (tcp_client != null)
            {
                this.tcp_client.Close();
            }
            Console.WriteLine("Server Disconnected");
        }

        public string Read()
        {
            // Buffer to store the response bytes.
            byte[] inData = new byte[256];
            // String to store the response ASCII representation.
            String responseData;
            // Read the first batch of the TcpServer response bytes.
            int bytes = stream.Read(inData, 0, inData.Length);
            responseData = Encoding.ASCII.GetString(inData, 0, bytes);
   
            return responseData;
        }

        public void Write(string command)
        {
            
            // Translate the passed message into ASCII and store it as a Byte array.
            byte[] outData = new byte[1024];
            outData = Encoding.ASCII.GetBytes(command);
            // Send the message to the connected TcpServer.
            
            this.stream.Write(outData, 0, outData.Length);
            Console.WriteLine("Sent: {0}", command);
        }
    }
}
