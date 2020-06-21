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
        private readonly BlockingCollection<AsyncCommand> queue;
        // Tcp client.
        private TcpClient tcp_client;
        // Data stream.
        NetworkStream stream;

        public FlightGearClient()
        {
            this.queue = new BlockingCollection<AsyncCommand>();
        }

        public Task<Result> Execute(Command cmd)
        {
            var asyncCommand = new AsyncCommand(cmd);
            this.queue.Add(asyncCommand);
            return asyncCommand.Task;
        }

        public void Start()
        {
            connect("127.0.0.1", 5403);
            Task.Factory.StartNew(ProcessCommands);
        }
        
        public void ProcessCommands()
        {
            //connect("127.0.0.1", 5403);
            double queryValue = 0;
            Result res;
            foreach(AsyncCommand aCommand in this.queue.GetConsumingEnumerable())
            {
                // Aileron
                queryValue = aCommand.Command.Aileron;
                string cmd = aCommand.Command.ParseAileronToString();
                write(cmd);
                string ret = read();
                res = CheckData(queryValue, ret);
                aCommand.Completion.SetResult(res);

                // Elevator
                queryValue = aCommand.Command.Elevator;
                write(aCommand.Command.ParseElevatorToString());
                res = CheckData(queryValue, read());
                aCommand.Completion.SetResult(res);

                // Rudder
                queryValue = aCommand.Command.Rudder;
                write(aCommand.Command.ParseRudderToString());
                res = CheckData(queryValue, read());
                aCommand.Completion.SetResult(res);

                // Throttle
                queryValue = aCommand.Command.Throttle;
                write(aCommand.Command.ParseThrottleToString());
                res = CheckData(queryValue, read());
                aCommand.Completion.SetResult(res);
            }
        }

        public Result CheckData(double val,string recieve)
        {
            double d;
            if (Double.TryParse(recieve, out d))
            {
                if (val == d)
                {
                    return Result.Ok;
                }
            } else
            {
                Console.WriteLine("Unable to parse '{0}'", recieve);
            }
            return Result.NotOk;
        }


        public void connect(string ip, int port)
        {
            this.tcp_client = new TcpClient(ip, port);
            // Set timeout.
            //tcp_client.ReceiveTimeout = 10000;
            //tcp_client.SendTimeout = 10000;
            Console.WriteLine("Establishing Connection");
            Console.WriteLine("Server Connected");
            this.stream = tcp_client.GetStream();
            // first command to change PROMPT
            write("data\n");
            //read();
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
