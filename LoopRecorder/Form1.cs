using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace LoopRecorder
{
    public partial class Form1 : Form
    {
        private List<byte> buffer1 = new List<byte>(4096);
        private List<byte> buffer2 = new List<byte>(4096);
        int tolNum;
        int count = 1;

        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        //打开
        private void btn_PortOpen_Click(object sender, EventArgs e)
        {
            try
            {
                string[] ports = System.IO.Ports.SerialPort.GetPortNames();

                serialPort1.PortName = txtPortName1.Text;
                serialPort1.BaudRate = int.Parse(txtBaudRate1.Text);
                serialPort1.DataBits = 8;
                serialPort1.Parity = System.IO.Ports.Parity.None;
                serialPort1.StopBits = System.IO.Ports.StopBits.One;
                serialPort1.Open();

                btn_PortOpen.Enabled = false;
                btn_PortClose.Enabled = true;


                if(ports.Contains(txtPortName2.Text))
                {
                    serialPort2.PortName = txtPortName2.Text;
                    serialPort2.BaudRate = int.Parse(txtBaudRate2.Text);
                    serialPort2.DataBits = 8;
                    serialPort2.Parity = System.IO.Ports.Parity.None;
                    serialPort2.StopBits = System.IO.Ports.StopBits.One;
                    serialPort2.Open();
                }
                tolNum = int.Parse(tolNumBox.Text) / 50 - 1;

                Thread th = new Thread(() => setLabRemainTurns(tolNum));
                th.Start();

            }
            catch
            {
                MessageBox.Show("端口错误，请检查串口！");
                return;
            }
        }
        //关闭
        private void btn_PortClose_Click(object sender, EventArgs e)
        {
            try
            {
                buffer1.Clear();
                buffer2.Clear();
                serialPort1.Close();
                serialPort2.Close();
                btn_PortOpen.Enabled = true;
                btn_PortClose.Enabled = false;
                MessageBox.Show("端口已关闭");
            }
            catch
            {
                return;
            }
        }

        //串口发送请求读线程
        public void update()
        {
            try
            {
                for(int i = 0;i<2;i++)
                {
                    Thread.Sleep(50);
                    byte[] respBytes1 = new byte[] { 0x01, 0x03, 0x4e, 0x20, 0x00, 0x05, 0x93, 0x2b };
                    serialPort1.Write(respBytes1, 0, 8);
                    Thread.Sleep(50);
                    byte[] respBytes2 = new byte[] { 0x02, 0x03, 0x4e, 0x20, 0x00, 0x05, 0x93, 0x18 };
                    serialPort1.Write(respBytes2, 0, 8);
                    Thread.Sleep(50);
                    byte[] respBytes3 = new byte[] { 0x03, 0x03, 0x4e, 0x20, 0x00, 0x05, 0x92, 0xc9 };
                    serialPort1.Write(respBytes3, 0, 8);
                    Thread.Sleep(50);
                    byte[] respBytes4 = new byte[] { 0x04, 0x03, 0x4e, 0x20, 0x00, 0x05, 0x93, 0x7e };
                    serialPort1.Write(respBytes4, 0, 8);
                    Thread.Sleep(50);
                    byte[] respBytes5 = new byte[] { 0x05, 0x03, 0x4e, 0x20, 0x00, 0x05, 0x92, 0xaf };
                    serialPort1.Write(respBytes5, 0, 8);
                    Thread.Sleep(50);
                    byte[] respBytes6 = new byte[] { 0x06, 0x03, 0x4e, 0x20, 0x00, 0x05, 0x92, 0x9c };
                    serialPort1.Write(respBytes6, 0, 8);
                    Thread.Sleep(50);
                    byte[] respBytes7 = new byte[] { 0x07, 0x03, 0x4e, 0x20, 0x00, 0x05, 0x93, 0x4d };
                    serialPort1.Write(respBytes7, 0, 8);
                    Thread.Sleep(50);
                    byte[] respBytes8 = new byte[] { 0x08, 0x03, 0x4e, 0x20, 0x00, 0x05, 0x93, 0xb2 };
                    serialPort1.Write(respBytes8, 0, 8);
                    Thread.Sleep(50);
                    byte[] respBytes9 = new byte[] { 0x09, 0x03, 0x4e, 0x20, 0x00, 0x05, 0x92, 0x63 };
                    serialPort1.Write(respBytes9, 0, 8);
                    Thread.Sleep(50);
                    byte[] respBytes10 = new byte[] { 0x0a, 0x03, 0x4e, 0x20, 0x00, 0x05, 0x92, 0x50 };
                    serialPort1.Write(respBytes10, 0, 8);
                }
            }
            catch
            {
                return;
            }
        }
        //串口1接收数据触发接收函数
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] respBytes = new byte[serialPort1.BytesToRead];
                serialPort1.Read(respBytes, 0, respBytes.Length);
                if (respBytes.Length == 0)
                {
                    return;
                }
                //Console.WriteLine(BitConverter.ToString(respBytes));
                buffer1.AddRange(respBytes);
                if (buffer1[1] == 3 && buffer1.Count >= 15)//0x03响应报文
                {
                    byte line = buffer1[0];//泳道

                    if(count < line)
                    {
                        while(count < line)
                        {
                            close(count);
                            count++;
                        }
                    }
                    else if (count > line)
                    {
                        while(count != line)
                        {
                            close(count);
                            count = (count + 1) % 11;
                        }
                    }

                    open(line);
                    count = (line + 1) % 11;

                    byte[] data = new byte[2];
                    data[1] = 0x00;
                    data[0] = buffer1[10];
                    string value = BitConverter.ToUInt16(data, 0).ToString();
                    switch (line - 1)
                    {
                        case 0: lab_remainTurns_0.Text = value; break;
                        case 1: lab_remainTurns_1.Text = value; break;
                        case 2: lab_remainTurns_2.Text = value; break;
                        case 3: lab_remainTurns_3.Text = value; break;
                        case 4: lab_remainTurns_4.Text = value; break;
                        case 5: lab_remainTurns_5.Text = value; break;
                        case 6: lab_remainTurns_6.Text = value; break;
                        case 7: lab_remainTurns_7.Text = value; break;
                        case 8: lab_remainTurns_8.Text = value; break;
                        case 9: lab_remainTurns_9.Text = value; break;
                        default: break;
                    }
                    buffer1.RemoveRange(0, 15);
                }
                else if (buffer1[1] == 16 && buffer1.Count >= 8)//0x10响应报文
                {
                    buffer1.RemoveRange(0, 8);
                    return;
                }
            }
            catch
            {
                return;
            }
        }

        //串口2接收数据触发接收函数
        private void serialPort2_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] respBytes = new byte[serialPort2.BytesToRead];
                serialPort2.Read(respBytes, 0, respBytes.Length);
                //Console.WriteLine(BitConverter.ToString(respBytes));
                buffer2.AddRange(respBytes);
                while (buffer2[0] != 84 || buffer2[1] != 80 && buffer2.Count > 0)//头2位不是0x54和0x50,则删除非数据报文
                {
                    if (buffer2.Contains(10))//如果存在0x0A
                    {
                        buffer2.RemoveRange(0, buffer2.IndexOf(10) + 1);//则删除到0x0A
                    }
                    else//否则全部删除
                    {
                        buffer2.Clear();
                    }
                }
                if (buffer2.Count >= 32)
                {
                    ushort line = AscillToTen(buffer2[21]);//泳道
                    int num = AscillToTen(buffer2[28]) * 10 + AscillToTen(buffer2[29]);//圈数

                    if (num % 2 == 0)//圈数为偶数
                    {
                        if (num > tolNum)
                        {
                            num = 0;
                        }
                        else
                        {
                            num = tolNum - num;
                        }
                    }
                    else//圈数为奇数
                    {
                        if (num != 1)
                        {
                            buffer2.RemoveRange(0, 32);
                            return;
                        }
                        else
                        {
                            num = tolNum;
                        }
                    }
                    switch (line)
                    {
                        case 0: lab_remainTurns_0.Text = num.ToString(); break;
                        case 1: lab_remainTurns_1.Text = num.ToString(); break;
                        case 2: lab_remainTurns_2.Text = num.ToString(); break;
                        case 3: lab_remainTurns_3.Text = num.ToString(); break;
                        case 4: lab_remainTurns_4.Text = num.ToString(); break;
                        case 5: lab_remainTurns_5.Text = num.ToString(); break;
                        case 6: lab_remainTurns_6.Text = num.ToString(); break;
                        case 7: lab_remainTurns_7.Text = num.ToString(); break;
                        case 8: lab_remainTurns_8.Text = num.ToString(); break;
                        case 9: lab_remainTurns_9.Text = num.ToString(); break;
                        default: break;
                    }
                    Thread th = new Thread(() => correct(num.ToString(), (ushort)(line+1)));
                    th.Start();
                    buffer2.RemoveRange(0, 32);
                }
            }
            catch
            {
                return;
            }
        }

        //修正计数器线程
        public void correct(string lab_remainTurns, ushort slave)
        {
            try
            {
                Thread.Sleep(80);

                ushort startAddr = 20003;//写入地址
                ushort writeLen = 2;//写入数量
                ushort value = ushort.Parse(lab_remainTurns);//写入值

                List<byte> command = new List<byte>();

                command.Add((byte)slave);//从站地址
                command.Add(0x10);//功能码，写多个寄存器
                                  //写入地址
                command.Add(BitConverter.GetBytes(startAddr)[1]);
                command.Add(BitConverter.GetBytes(startAddr)[0]);
                //写入数量
                command.Add(BitConverter.GetBytes(writeLen)[1]);
                command.Add(BitConverter.GetBytes(writeLen)[0]);
                //字节数
                command.Add(0x04);
                //写入值
                command.Add(BitConverter.GetBytes(value)[1]);
                command.Add(BitConverter.GetBytes(value)[0]);
                command.Add(0x00);
                command.Add(0x00);
                //CRC校验
                command = CRC16(command);

                serialPort1.Write(command.ToArray(), 0, command.Count);
                buffer1.Clear();
            }
            catch
            {
                return;
            }
            
        }

        //开启响应的泳道
        private void open(int line)
        {
            switch (line)
            {
                case 1: btn_Open_0.PerformClick(); break;
                case 2: btn_Open_1.PerformClick(); break;
                case 3: btn_Open_2.PerformClick(); break;
                case 4: btn_Open_3.PerformClick(); break;
                case 5: btn_Open_4.PerformClick(); break;
                case 6: btn_Open_5.PerformClick(); break;
                case 7: btn_Open_6.PerformClick(); break;
                case 8: btn_Open_7.PerformClick(); break;
                case 9: btn_Open_8.PerformClick(); break;
                case 10: btn_Open_9.PerformClick(); break;
                default: break;
            }
        }



        //关闭没有响应的泳道
        private void close (int line)
        {
            switch (line)
            {
                case 1: btn_Close_0.PerformClick(); break;
                case 2: btn_Close_1.PerformClick(); break;
                case 3: btn_Close_2.PerformClick(); break;
                case 4: btn_Close_3.PerformClick(); break;
                case 5: btn_Close_4.PerformClick(); break;
                case 6: btn_Close_5.PerformClick(); break;
                case 7: btn_Close_6.PerformClick(); break;
                case 8: btn_Close_7.PerformClick(); break;
                case 9: btn_Close_8.PerformClick(); break;
                case 10: btn_Close_9.PerformClick(); break;
                default: break;
            }
        }

        //CRC校验
        static List<byte> CRC16(List<byte> value, ushort poly = 0xA001, ushort crcInit = 0xFFFF)
        {
            if (value == null || !value.Any())
                throw new ArgumentException("");

            //运算
            ushort crc = crcInit;
            for (int i = 0; i < value.Count; i++)
            {
                crc = (ushort)(crc ^ (value[i]));
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ poly) : (ushort)(crc >> 1);
                }
            }
            byte hi = (byte)((crc & 0xFF00) >> 8);  //高位置
            byte lo = (byte)(crc & 0x00FF);         //低位置

            List<byte> buffer = new List<byte>();
            //添加校验原始值
            buffer.AddRange(value);
            //添加校验位值
            buffer.Add(lo);
            buffer.Add(hi);

            //加上原始校验值返回
            return buffer;
        }

        //ASCILL码转0-9数字
        public ushort AscillToTen(byte ascillCode)
        {
            ushort num;
            switch (ascillCode)
            {
                case 48: num = 0; break;
                case 49: num = 1; break;
                case 50: num = 2; break;
                case 51: num = 3; break;
                case 52: num = 4; break;
                case 53: num = 5; break;
                case 54: num = 6; break;
                case 55: num = 7; break;
                case 56: num = 8; break;
                case 57: num = 9; break;
                default: num = 0; break;
            }
            return num;
        }

        //打开设置初始值并校验
        public void setLabRemainTurns(int num)
        {
            correct(num.ToString(), 1);
            correct(num.ToString(), 2);
            correct(num.ToString(), 3);
            correct(num.ToString(), 4);
            correct(num.ToString(), 5);
            correct(num.ToString(), 6);
            correct(num.ToString(), 7);
            correct(num.ToString(), 8);
            correct(num.ToString(), 9);
            correct(num.ToString(), 10);

            btn_check.PerformClick();
        }

        //检验
        private void btn_check_Click(object sender, EventArgs e)
        {
            Thread th1 = new Thread(update);
            th1.Start();
        }

        //0道
        private void btn_Correct_0_Click(object sender, EventArgs e)
        {

            if (text_correct_0.Text.Length != 0)
            {
                lab_remainTurns_0.Text = text_correct_0.Text;
                text_correct_0.Text = "";
            }
            if (serialPort1.IsOpen)
            {
                Thread th1 = new Thread(() => correct(lab_remainTurns_0.Text, 1));
                th1.Start();
            }
        }
        //1道
        private void btn_Correct_1_Click(object sender, EventArgs e)
        {
            if (text_correct_1.Text.Length != 0)
            {
                lab_remainTurns_1.Text = text_correct_1.Text;
                text_correct_1.Text = "";
            }
            if (serialPort1.IsOpen)
            {
                Thread th2 = new Thread(() => correct(lab_remainTurns_1.Text, 2));
                th2.Start();
            }
        }
        //2道
        private void btn_Correct_2_Click(object sender, EventArgs e)
        {
            if (text_correct_2.Text.Length != 0)
            {
                lab_remainTurns_2.Text = text_correct_2.Text;
                text_correct_2.Text = "";
            }
            if (serialPort1.IsOpen)
            {
                Thread th3 = new Thread(() => correct(lab_remainTurns_2.Text, 3));
                th3.Start();
            }
        }
        //3道
        private void btn_Correct_3_Click(object sender, EventArgs e)
        {
            if (text_correct_3.Text.Length != 0)
            {
                lab_remainTurns_3.Text = text_correct_3.Text;
                text_correct_3.Text = "";
            }
            if (serialPort1.IsOpen)
            {
                Thread th4 = new Thread(() => correct(lab_remainTurns_3.Text, 4));
                th4.Start();
            }
        }
        //4道
        private void btn_Correct_4_Click(object sender, EventArgs e)
        {
            if (text_correct_4.Text.Length != 0)
            {
                lab_remainTurns_4.Text = text_correct_4.Text;
                text_correct_4.Text = "";
            }
            if (serialPort1.IsOpen)
            {
                Thread th5 = new Thread(() => correct(lab_remainTurns_4.Text, 5));
                th5.Start();
            }
        }
        //5道
        private void btn_Correct_5_Click(object sender, EventArgs e)
        {
            if (text_correct_5.Text.Length != 0)
            {
                lab_remainTurns_5.Text = text_correct_5.Text;
                text_correct_5.Text = "";
            }
            if (serialPort1.IsOpen)
            {
                Thread th6 = new Thread(() => correct(lab_remainTurns_5.Text, 6));
                th6.Start();
            }
        }
        //6道
        private void btn_Correct_6_Click(object sender, EventArgs e)
        {
            if (text_correct_6.Text.Length != 0)
            {
                lab_remainTurns_6.Text = text_correct_6.Text;
                text_correct_6.Text = "";
            }
            if (serialPort1.IsOpen)
            {
                Thread th7 = new Thread(() => correct(lab_remainTurns_6.Text, 7));
                th7.Start();
            }
        }
        //7道
        private void btn_Correct_7_Click(object sender, EventArgs e)
        {
            if (text_correct_7.Text.Length != 0)
            {
                lab_remainTurns_7.Text = text_correct_7.Text;
                text_correct_7.Text = "";
            }
            if (serialPort1.IsOpen)
            {
                Thread th8 = new Thread(() => correct(lab_remainTurns_7.Text, 8));
                th8.Start();
            }
        }
        //8道
        private void btn_Correct_8_Click(object sender, EventArgs e)
        {
            if (text_correct_8.Text.Length != 0)
            {
                lab_remainTurns_8.Text = text_correct_8.Text;
                text_correct_8.Text = "";
            }
            if (serialPort1.IsOpen)
            {
                Thread th9 = new Thread(() => correct(lab_remainTurns_8.Text, 9));
                th9.Start();
            }
        }
        //9道
        private void btn_Correct_9_Click(object sender, EventArgs e)
        {
            if (text_correct_9.Text.Length != 0)
            {
                lab_remainTurns_9.Text = text_correct_9.Text;
                text_correct_9.Text = "";
            }
            if (serialPort1.IsOpen)
            {
                Thread th10 = new Thread(() => correct(lab_remainTurns_9.Text, 10));
                th10.Start();
            }
        }

        private void btn_Open_0_Click(object sender, EventArgs e)
        {
            btn_Correct_0.Enabled = true;
            btn_Open_0.Enabled = false;
            btn_Close_0.Enabled = true;
            lab_remainTurns_0.Visible = true;
        }

        private void btn_Open_1_Click(object sender, EventArgs e)
        {
            btn_Correct_1.Enabled = true;
            btn_Open_1.Enabled = false;
            btn_Close_1.Enabled = true;
            lab_remainTurns_1.Visible = true;
        }

        private void btn_Open_2_Click(object sender, EventArgs e)
        {
            btn_Correct_2.Enabled = true;
            btn_Open_2.Enabled = false;
            btn_Close_2.Enabled = true;
            lab_remainTurns_2.Visible = true;
        }

        private void btn_Open_3_Click(object sender, EventArgs e)
        {
            btn_Correct_3.Enabled = true;
            btn_Open_3.Enabled = false;
            btn_Close_3.Enabled = true;
            lab_remainTurns_3.Visible = true;
        }

        private void btn_Open_4_Click(object sender, EventArgs e)
        {
            btn_Correct_4.Enabled = true;
            btn_Open_4.Enabled = false;
            btn_Close_4.Enabled = true;
            lab_remainTurns_4.Visible = true;
        }

        private void btn_Open_5_Click(object sender, EventArgs e)
        {
            btn_Correct_5.Enabled = true;
            btn_Open_5.Enabled = false;
            btn_Close_5.Enabled = true;
            lab_remainTurns_5.Visible = true;
        }

        private void btn_Open_6_Click(object sender, EventArgs e)
        {
            btn_Correct_6.Enabled = true;
            btn_Open_6.Enabled = false;
            btn_Close_6.Enabled = true;
            lab_remainTurns_6.Visible = true;
        }

        private void btn_Open_7_Click(object sender, EventArgs e)
        {
            btn_Correct_7.Enabled = true;
            btn_Open_7.Enabled = false;
            btn_Close_7.Enabled = true;
            lab_remainTurns_7.Visible = true;
        }

        private void btn_Open_8_Click(object sender, EventArgs e)
        {
            btn_Correct_8.Enabled = true;
            btn_Open_8.Enabled = false;
            btn_Close_8.Enabled = true;
            lab_remainTurns_8.Visible = true;
        }

        private void btn_Open_9_Click(object sender, EventArgs e)
        {
            btn_Correct_9.Enabled = true;
            btn_Open_9.Enabled = false;
            btn_Close_9.Enabled = true;
            lab_remainTurns_9.Visible = true;
        }

        private void btn_Close_0_Click(object sender, EventArgs e)
        {
            btn_Correct_0.Enabled = false;
            btn_Open_0.Enabled = true;
            btn_Close_0.Enabled = false;
            lab_remainTurns_0.Visible = false;
        }

        private void btn_Close_1_Click(object sender, EventArgs e)
        {
            btn_Correct_1.Enabled = false;
            btn_Open_1.Enabled = true;
            btn_Close_1.Enabled = false;
            lab_remainTurns_1.Visible = false;
        }

        private void btn_Close_2_Click(object sender, EventArgs e)
        {
            btn_Correct_2.Enabled = false;
            btn_Open_2.Enabled = true;
            btn_Close_2.Enabled = false;
            lab_remainTurns_2.Visible = false;
        }

        private void btn_Close_3_Click(object sender, EventArgs e)
        {
            btn_Correct_3.Enabled = false;
            btn_Open_3.Enabled = true;
            btn_Close_3.Enabled = false;
            lab_remainTurns_3.Visible = false;
        }

        private void btn_Close_4_Click(object sender, EventArgs e)
        {
            btn_Correct_4.Enabled = false;
            btn_Open_4.Enabled = true;
            btn_Close_4.Enabled = false;
            lab_remainTurns_4.Visible = false;
        }

        private void btn_Close_5_Click(object sender, EventArgs e)
        {
            btn_Correct_5.Enabled = false;
            btn_Open_5.Enabled = true;
            btn_Close_5.Enabled = false;
            lab_remainTurns_5.Visible = false;
        }

        private void btn_Close_6_Click(object sender, EventArgs e)
        {
            btn_Correct_6.Enabled = false;
            btn_Open_6.Enabled = true;
            btn_Close_6.Enabled = false;
            lab_remainTurns_6.Visible = false;
        }

        private void btn_Close_7_Click(object sender, EventArgs e)
        {
            btn_Correct_7.Enabled = false;
            btn_Open_7.Enabled = true;
            btn_Close_7.Enabled = false;
            lab_remainTurns_7.Visible = false;
        }

        private void btn_Close_8_Click(object sender, EventArgs e)
        {
            btn_Correct_8.Enabled = false;
            btn_Open_8.Enabled = true;
            btn_Close_8.Enabled = false;
            lab_remainTurns_8.Visible = false;
        }

        private void btn_Close_9_Click(object sender, EventArgs e)
        {
            btn_Correct_9.Enabled = false;
            btn_Open_9.Enabled = true;
            btn_Close_9.Enabled = false;
            lab_remainTurns_9.Visible = false;
        }

        private void btn_reduce_0_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_0.Text = (int.Parse(lab_remainTurns_0.Text) - 2).ToString();
                Thread th1 = new Thread(() => correct(lab_remainTurns_0.Text, 1));
                th1.Start();
            }
        }

        private void btn_reduce_1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_1.Text = (int.Parse(lab_remainTurns_1.Text) - 2).ToString();
                Thread th2 = new Thread(() => correct(lab_remainTurns_1.Text, 2));
                th2.Start();
            }
        }

        private void btn_reduce_2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_2.Text = (int.Parse(lab_remainTurns_2.Text) - 2).ToString();
                Thread th3 = new Thread(() => correct(lab_remainTurns_2.Text, 3));
                th3.Start();
            }
        }

        private void btn_reduce_3_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_3.Text = (int.Parse(lab_remainTurns_3.Text) - 2).ToString();
                Thread th4 = new Thread(() => correct(lab_remainTurns_3.Text, 4));
                th4.Start();
            }
        }

        private void btn_reduce_4_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_4.Text = (int.Parse(lab_remainTurns_4.Text) - 2).ToString();
                Thread th5 = new Thread(() => correct(lab_remainTurns_4.Text, 5));
                th5.Start();
            }
        }

        private void btn_reduce_5_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_5.Text = (int.Parse(lab_remainTurns_5.Text) - 2).ToString();
                Thread th6 = new Thread(() => correct(lab_remainTurns_5.Text, 6));
                th6.Start();
            }
        }

        private void btn_reduce_6_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_6.Text = (int.Parse(lab_remainTurns_6.Text) - 2).ToString();
                Thread th7 = new Thread(() => correct(lab_remainTurns_6.Text, 7));
                th7.Start();
            }
        }

        private void btn_reduce_7_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_7.Text = (int.Parse(lab_remainTurns_7.Text) - 2).ToString();
                Thread th8 = new Thread(() => correct(lab_remainTurns_7.Text, 8));
                th8.Start();
            }
        }

        private void btn_reduce_8_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_8.Text = (int.Parse(lab_remainTurns_8.Text) - 2).ToString();
                Thread th9 = new Thread(() => correct(lab_remainTurns_8.Text, 9));
                th9.Start();
            }
        }

        private void btn_reduce_9_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_9.Text = (int.Parse(lab_remainTurns_9.Text) - 2).ToString();
                Thread th10 = new Thread(() => correct(lab_remainTurns_9.Text, 10));
                th10.Start();
            }
        }


        private void btn_add_0_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_0.Text = (int.Parse(lab_remainTurns_0.Text) + 2).ToString();
                Thread th1 = new Thread(() => correct(lab_remainTurns_0.Text, 1));
                th1.Start();
            }
        }

        private void btn_add_1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_1.Text = (int.Parse(lab_remainTurns_1.Text) + 2).ToString();
                Thread th2 = new Thread(() => correct(lab_remainTurns_1.Text, 2));
                th2.Start();
            }
        }

        private void btn_add_2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_2.Text = (int.Parse(lab_remainTurns_2.Text) + 2).ToString();
                Thread th3 = new Thread(() => correct(lab_remainTurns_2.Text, 3));
                th3.Start();
            }
        }

        private void btn_add_3_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_3.Text = (int.Parse(lab_remainTurns_3.Text) + 2).ToString();
                Thread th4 = new Thread(() => correct(lab_remainTurns_3.Text, 4));
                th4.Start();
            }
        }

        private void btn_add_4_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_4.Text = (int.Parse(lab_remainTurns_4.Text) + 2).ToString();
                Thread th5 = new Thread(() => correct(lab_remainTurns_4.Text, 5));
                th5.Start();
            }
        }

        private void btn_add_5_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_5.Text = (int.Parse(lab_remainTurns_5.Text) + 2).ToString();
                Thread th6 = new Thread(() => correct(lab_remainTurns_5.Text, 6));
                th6.Start();
            }
        }

        private void btn_add_6_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_6.Text = (int.Parse(lab_remainTurns_6.Text) + 2).ToString();
                Thread th7 = new Thread(() => correct(lab_remainTurns_6.Text, 7));
                th7.Start();
            }
        }

        private void btn_add_7_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_7.Text = (int.Parse(lab_remainTurns_7.Text) + 2).ToString();
                Thread th8 = new Thread(() => correct(lab_remainTurns_7.Text, 8));
                th8.Start();
            }
        }

        private void btn_add_8_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_8.Text = (int.Parse(lab_remainTurns_8.Text) + 2).ToString();
                Thread th9 = new Thread(() => correct(lab_remainTurns_8.Text, 9));
                th9.Start();
            }
        }

        private void btn_add_9_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lab_remainTurns_9.Text = (int.Parse(lab_remainTurns_9.Text) + 2).ToString();
                Thread th10 = new Thread(() => correct(lab_remainTurns_9.Text, 10));
                th10.Start();
            }
        }
    }
}