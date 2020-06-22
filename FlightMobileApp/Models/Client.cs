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
using Microsoft.Extensions.Configuration;

namespace FlightMobileApp.Model
{
    interface IClient
    {
        bool connect(string ip, int port);
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
        // Connection data.
        private string address, port;

        private bool isConected = false;


        // Lock synchronization object
        private static object syncLock = new object();

        // Constructor (protected)
        protected FlightGearClient(IConfiguration conf)
        {
            this.queue = new BlockingCollection<AsyncCommand>();
            this.address = conf.GetValue<string>("Logging:CommandsConnectionData:Host");
            this.port = conf.GetValue<string>("Logging:CommandsConnectionData:Port");

            //start a new task 
            this.Start();
        }

        public static FlightGearClient GetFlightGearClient(IConfiguration conf)
        {
            if (instance == null)
            {
                lock (syncLock)
                {
                    if (instance == null)
                    {
                        instance = new FlightGearClient(conf);
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

            
            foreach(AsyncCommand aCommand in this.queue.GetConsumingEnumerable())
            {
                double queryValue = 0;
                Result res = Result.Ok;
                int port = Int32.Parse(this.port);
                if (!connect(this.address, port))
                {
                    aCommand.Completion.SetException(new Exception("failed Conecting to FlightGear"));
                    continue;
                }
                try
                {
                    // Aileron
                    queryValue = aCommand.Command.Aileron;
                    write("set" + aCommand.Command.ParseAileronToString());
                    write("get /controls/flight/aileron\n");
                    if (!IsValidData(queryValue, read()))
                    {
                        aCommand.Completion.SetException(new Exception("Error - can not set ailron"));
                        continue;
                    }

                    // Elevator
                    queryValue = aCommand.Command.Elevator;
                    write("set" + aCommand.Command.ParseElevatorToString());
                    write("get /controls/flight/elevator\n");
                    if (!IsValidData(queryValue, read()))
                    {
                        aCommand.Completion.SetException(new Exception("Error - can not set elevator"));
                        continue;
                    }

                    // Rudder
                    queryValue = aCommand.Command.Rudder;
                    write("set" + aCommand.Command.ParseRudderToString());
                    write("get /controls/flight/rudder\n");
                    if (!IsValidData(queryValue, read()))
                    {
                        aCommand.Completion.SetException(new Exception("Error - can not set rudder"));
                        continue;
                    }

                    // Throttle
                    queryValue = aCommand.Command.Throttle;
                    write("set" + aCommand.Command.ParseThrottleToString());
                    write("get /controls/engines/current-engine/throttle\n");
                    if (!IsValidData(queryValue, read()))
                    {
                        aCommand.Completion.SetException(new Exception("Error - can not set throttle"));
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

        public bool IsValidData(double val,string recieve)
        {
            if (val == Convert.ToDouble(recieve))
            {
                return true;
            }
            return false;
        }


        public bool connect(string ip, int port)
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
                    write("data\n");
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
   
            return responseData;
        }

        public void write(string command)
        {
            //Console.WriteLine(command);
            // Translate the passed message into ASCII and store it as a Byte array.
            byte[] outData = new byte[1024];
            outData = Encoding.ASCII.GetBytes(command);
            // Send the message to the connected TcpServer.
            
            this.stream.Write(outData, 0, outData.Length);
            Console.WriteLine("Sent: {0}", command);
        }
    }
}
