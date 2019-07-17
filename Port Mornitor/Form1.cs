using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.IO;

namespace SN_Port_Mornitor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button1_Click(null, null);
            saveFileDialog1.Title = "Save as";
            saveFileDialog1.Filter = "Text Files(*.txt)|*.txt|All Files(*.*)|*.*";
            saveFileDialog1.RestoreDirectory = true;
        }

        public bool isworking = false;
        Thread backgroundThread;
        SerialPort sp1;
        SerialPort sp2;

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            foreach (string port in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(port);
                comboBox2.Items.Add(port);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "Connect")
            {
                if (comboBox1.SelectedItem.ToString() == comboBox2.SelectedItem.ToString())
                {
                    MessageBox.Show("The application cannot open the same port at a time. Please choose a different port and try again","Error");
                    button1_Click(null, null);
                }
                else
                {
                    isworking = true;
                    button2.Text = "Disconnect";
                    string port1 = comboBox1.SelectedItem.ToString();
                    string port2 = comboBox2.SelectedItem.ToString();
                    backgroundThread = new Thread(new ThreadStart(() => forward_thread(port1, port2)));
                    backgroundThread.Start();
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                }
            }
            else
            {
                isworking = false;
                button2.Text = "Connect";
                backgroundThread.Abort();
                sp1.Close();
                sp2.Close();
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
            }
        }
        public void forward_thread(string port1, string port2)
        {
            sp1 = new SerialPort(port1);
            sp2 = new SerialPort(port2);
            byte[] b = new byte[32];
            string sbuffer;
            sp1.Open();
            sp1.DiscardInBuffer();
            sp1.DiscardOutBuffer();
            sp2.Open();
            sp2.DiscardInBuffer();
            sp2.DiscardOutBuffer();
            while (true)
            {
                //port1 -> port2
                sbuffer = "";
                sbuffer = sp1.ReadExisting();
                if (sbuffer != "")
                {
                    sp2.Write(sbuffer);
                    /*
                    sp1.Read(b, 0, b.Length);
                    sp2.Write(b, 0, b.Length);
                    StringBuilder sb1 = new StringBuilder();
                    for (int i = 0; i < b.Length; i++)
                    {
                        sb1.Append(Convert.ToString(b[i]) + " ");
                    }
                    */
                    AppendTextBox(port1 + " -> " + port2 + ":\r\n");
                    if (checkBox1.Checked == true)
                    {
                        AppendTextBox(ToHex(sbuffer));
                    }
                    else
                    {
                        AppendTextBox(sbuffer);
                    }
                    AppendTextBox("\r\n=============================\r\n\r\n");
                }
                //port2 -> port1
                sbuffer = "";
                sbuffer = sp2.ReadExisting();
                if (sbuffer != "")
                {
                    sp1.Write(sbuffer);
                    /*
                    sp2.Read(b, 0, b.Length);
                    sp1.Write(b, 0, b.Length);
                    StringBuilder sb2 = new StringBuilder();
                    for (int i = 0; i < b.Length; i++)
                    {
                        sb2.Append(Convert.ToString(b[i]) + " ");
                    }
                    */
                    AppendTextBox(port2 + " -> " + port1 + ":\r\n");
                    if (checkBox1.Checked == true)
                    {
                        AppendTextBox(ToHex(sbuffer));
                    }
                    else
                    {
                        AppendTextBox(sbuffer);
                    }
                    AppendTextBox("\r\n=============================\r\n\r\n");
                }
            }
        }
        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            textBox1.Text += value;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete all messages?", "Confirm Message", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                textBox1.Text = "";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName=comboBox1.SelectedItem.ToString() + "-" + comboBox2.SelectedItem.ToString() + DateTime.Now.ToString("yyyy-MM-dd HH-mm") + ".txt";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string localFilePath = saveFileDialog1.FileName.ToString();
                string result = textBox1.Text;
                StreamWriter sw = File.AppendText(localFilePath);
                sw.Write(result);
                sw.Flush();
                sw.Close();
            }
        }

        public static string ToHex(string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
                sb.AppendFormat("0x{0:X2} ", (int)c);
            return sb.ToString().Trim();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Port Mornitor V" + Application.ProductVersion;
        }
    }
}
