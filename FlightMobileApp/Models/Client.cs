using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FlightMobileApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlightMobileApp.Model
{
    interface IClient
    {
        void connect(string ip, int port);
        void write(string command);
        string read();
        void disconnect();
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
        private static object syncLock = new object();

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
            Task.Factory.StartNew(ProcessCommands);
        }
        
        public void ProcessCommands()
        {
            //connect only once
            connect("127.0.0.1", 5404);
            double queryValue = 0;
            Result res = Result.Ok;
            string a;
            foreach(AsyncCommand aCommand in this.queue.GetConsumingEnumerable())
            {
                // Aileron
                queryValue = aCommand.Command.Aileron;
                write("set"+aCommand.Command.ParseAileronToString());  
                write("get /controls/flight/aileron\n");
                if(!IsValidData(queryValue, read()))
                {
                    res = Result.NotOk;
                }

                // Elevator
                queryValue = aCommand.Command.Elevator;
                write("set"+aCommand.Command.ParseElevatorToString());
                write("get /controls/flight/elevator\n" );
                if (! IsValidData(queryValue, read()))
                {
                    res = Result.NotOk;
                }
               
                // Rudder
                queryValue = aCommand.Command.Rudder;
                write("set"+aCommand.Command.ParseRudderToString());
                write("get /controls/flight/rudder\n");
                if (!IsValidData(queryValue, read()))
                {
                    res = Result.NotOk;
                }

                // Throttle
                queryValue = aCommand.Command.Throttle;
                write("set"+ aCommand.Command.ParseThrottleToString());
                write("get /controls/engines/current-engine/throttle\n");
                if (!IsValidData(queryValue, read()))
                {
                    res = Result.NotOk;
                }
                
                aCommand.Completion.SetResult(res);
            }
        }

        public bool IsValidData(double val,string recieve)
        {
            if (val == Convert.ToDouble(recieve))
            {
                return true;
            }
            return false;
        }


        public void connect(string ip, int port)
        {
            if (isConected == false)
            {
                this.tcp_client = new TcpClient(ip, port);
                Console.WriteLine("Establishing Connection");
                Console.WriteLine("Server Connected");
                this.stream = tcp_client.GetStream();
                // first command to change PROMPT
                write("data\n");
                isConected = true;
                // Set timeout.
                tcp_client.ReceiveTimeout = 10000;
                tcp_client.SendTimeout = 10000;
            }
            if (isConected)
            {
                Console.WriteLine("connected");
            }
            

        }

        public void disconnect()
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

        public string read()
        {
            // Buffer to store the response bytes.
            byte[] inData = new byte[256];
            // String to store the response ASCII representation.
            String responseData;
            // Read the first batch of the TcpServer response bytes.
            int bytes = stream.Read(inData, 0, inData.Length);
            responseData = Encoding.ASCII.GetString(inData, 0, bytes);
            //Console.WriteLine("Received: {0}", responseData);
            return responseData;
        }

        public void write(string command)
        {
            //Console.WriteLine(command);
            // Translate the passed message into ASCII and store it as a Byte array.
            byte[] outData = new byte[1024];
            outData = Encoding.ASCII.GetBytes(command);
            // Send the message to the connected TcpServer.
            // if (stream != null)
            // {
            this.stream.Write(outData, 0, outData.Length);
            // }
            Console.WriteLine("Sent: {0}", command);
        }
    }
}
