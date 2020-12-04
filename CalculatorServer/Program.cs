using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace CalculatorServer
{
    class Program
    {
        class Expression
        {
            public int Operand1 { get; set; }
            public int Operand2 { get; set; }
            public string Operation { get; set; }
        }
        static void Main(string[] args)
        {
            List<int> operands = new List<int>();
            int port = 8005;

            IPAddress address = IPAddress.Parse("127.0.0.1");
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint point = new IPEndPoint(address, port);
            try
            {
                socket.Bind(point);
                socket.Listen(1);
                Console.WriteLine("Сервер запущен. Ожидание подключений...");
                while (true)
                {
                    Socket listener = socket.Accept();
                    string str = "";
                    int bytes = 0;
                    byte[] data = new byte[256];
                    Expression ex = new Expression();
                    do
                    {
                        bytes = listener.Receive(data);
                        str = Encoding.UTF8.GetString(data, 0, bytes);
                        Console.WriteLine(str);
                        ex = (Expression) JsonSerializer.Deserialize<Expression>(str);
                    }
                    while (listener.Available > 0);

                    if (ex.Operation == "shutting down")
                    {
                        string message = "Мінімальний операнд: " + operands.Min().ToString() + "\n";
                        message += "Максимальний операнд: " + operands.Max().ToString() + "\n";
                        message += "Середнє всіх операндів: " + operands.Average().ToString() + "\n";
                        data = Encoding.UTF8.GetBytes(message);
                        listener.Send(data);
                        listener.Shutdown(SocketShutdown.Both);
                        listener.Close();
                    }
                    else
                    {
                        operands.Add(ex.Operand1);
                        operands.Add(ex.Operand2);
                        string response = Calculating(ref ex).ToString();
                        data = Encoding.UTF8.GetBytes(response);
                        listener.Send(data);
                        listener.Shutdown(SocketShutdown.Both);
                        listener.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static int Calculating (ref Expression ex)
        {
            int result = 0;
            if (ex.Operation == "+") result = ex.Operand1 + ex.Operand2;
            else if (ex.Operation == "-") result = ex.Operand1 - ex.Operand2;
            else if (ex.Operation == "*") result = ex.Operand1 * ex.Operand2;
            else if (ex.Operation == "/") result = ex.Operand1 / ex.Operand2;

            return result;
        }
    }
}
