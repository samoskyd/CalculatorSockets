using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CalculatorClient1
{
    public partial class Form1 : Form
    {
        string operand1 = "";
        string operand2 = "";
        string operation = "";
        static IPAddress address = IPAddress.Loopback;
        static int port = 8005;
        IPEndPoint point = new IPEndPoint(address, port);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Expression ex = new Expression();
            ex.Operand1 = 0;
            ex.Operand2 = 0;
            ex.Operation = "shutting down";
            MessageBox.Show(ToServer(ex));
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            operand1 = textBox1.Text;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            operation = comboBox1.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            operand2 = textBox2.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Expression ex = new Expression();
                ex.Operand1 = Int32.Parse(operand1);
                ex.Operand2 = Int32.Parse(operand2);
                ex.Operation = operation;

                richTextBox1.Clear();
                richTextBox1.AppendText(ToServer(ex));
            }
            catch (Exception exxc)
            {
                MessageBox.Show(exxc.Message);
            }
        }

        private string ToServer(Expression ex)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(point);
                string message = JsonSerializer.Serialize<Expression>(ex);
                socket.Send(Encoding.UTF8.GetBytes(message));
                byte[] buffer = new byte[256];
                int size;
                string str;
                do
                {
                    size = socket.Receive(buffer);
                    str = Encoding.UTF8.GetString(buffer, 0, size);
                }
                while (socket.Available > 0);

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                return str;
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
                return "error";
            }
        }
    }
}
