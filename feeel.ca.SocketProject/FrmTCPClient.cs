// ***********************************************************************
//    Assembly       : 新阁教育
//    Created          : 2022-11-01
// ***********************************************************************
//     Copyright by 新阁教育（天津星阁教育科技有限公司）
//     QQ：        2934008828（付老师）  
//     WeChat：thinger002（付老师）
//     公众号：   dotNet工控上位机
//     哔哩哔哩：dotNet工控上位机
//     知乎：      dotNet工控上位机
//     头条：      dotNet工控上位机
//     视频号：   dotNet工控上位机
//     版权所有，严禁传播
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace feeel.ca.SocketProject
{
    public partial class FrmTCPClient : Form
    {
        public FrmTCPClient()
        {
            InitializeComponent();
            this.Load += FrmTCPClient_Load;
        }

        /*
         
             客户端程序编写步骤：
            第一步：调用socket()函数创建一个用于通信的套接字。
            第二步：通过设置套接字地址结构，说明客户端与之通信的服务器的IP地址和端口号。
            第三步：调用connect()函数来建立与服务器的连接。
            第四步：调用读写函数发送或者接收数据。
            第五步：终止连接。
                       
        */

        private Socket socketClient;

        private void FrmTCPClient_Load(object sender, EventArgs e)
        {
            this.lst_Rcv.Columns[1].Width = this.lst_Rcv.ClientSize.Width - this.lst_Rcv.Columns[0].Width;
        }


        #region 连接服务器
        private void btn_Connect_Click(object sender, EventArgs e)
        {
            AddLog(0, "与服务器连接中");

            // 第一步：调用socket()函数创建一个用于通信的套接字。
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // 第二步：通过设置套接字地址结构，说明客户端与之通信的服务器的IP地址和端口号。
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(this.txt_IP.Text.Trim()), int.Parse(this.txt_Port.Text.Trim()));

            // 第三步：调用connect()函数来建立与服务器的连接。

            try
            {
                socketClient.Connect(ipe);
            }
            catch (Exception ex)
            {
                AddLog(2, "连接服务器失败：" + ex.Message);
                return;
            }

            //创建一个监听的线程

            Task.Run(new Action(() =>
            {
                CheckReceiveMsg();
            }));

            AddLog(0, "成功连接至服务器");

            this.btn_Connect.Enabled = false;
        }

        #endregion

        #region 多线程接收数据

        private void CheckReceiveMsg()
        {
            while (true)
            {
                // 创建一个缓冲区

                byte[] buffer = new byte[1024 * 1024 * 10];

                int length = -1;

                //  第四步：调用读写函数发送或者接收数据。
                try
                {
                    length = socketClient.Receive(buffer);
                }
                catch (Exception)
                {
                    break;
                }

                if (length > 0)
                {
                    string msg = string.Empty;

                    MessageType type = (MessageType)buffer[0];

                    switch (type)
                    {
                        case MessageType.ASCII:

                            msg = Encoding.ASCII.GetString(buffer, 1, length - 1);

                            AddLog(0,  "服务器：" + msg);

                            break;
                        case MessageType.UTF8:

                            msg = Encoding.UTF8.GetString(buffer, 1, length - 1);

                            AddLog(0,  "服务器：" + msg);

                            break;
                        case MessageType.Hex:

                            msg = HexGetString(buffer, 1, length - 1);

                            AddLog(0,  "服务器：" + msg);

                            break;
                        case MessageType.File:

                            Invoke(new Action(() =>
                            {
                                SaveFileDialog sfd = new SaveFileDialog();

                                sfd.Filter = "txt files(*.txt)|*.txt|xls files(*.xls)|*.xls|xlsx files(*.xlsx)|*.xlsx|All files(*.*)|*.*";

                                if (sfd.ShowDialog() == DialogResult.OK)
                                {
                                    string fileSavePath = sfd.FileName;

                                    using (FileStream fs = new FileStream(fileSavePath, FileMode.Create))
                                    {
                                        fs.Write(buffer, 1, length - 1);
                                    }

                                    AddLog(0, "文件成功保存至" + fileSavePath);
                                }
                            }));

                            break;
                        case MessageType.JSON:

                            Invoke(new Action(() =>
                            {
                                string res = Encoding.Default.GetString(buffer, 1, length);

                                List<Student> StuList = JSONHelper.JSONToEntity<List<Student>>(res);

                                new FrmJSON(StuList).Show();

                                AddLog(0, "接收JSON数据：" + res);

                            }));

                            break;
                        default:
                            break;
                    }



                }
                else
                {
                    break;
                }
            }
        }

        #endregion

        #region 接收信息的方法

        //当前时间属性
        private string CurrentTime
        {
            get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); }
        }


        private void AddLog(int index, string info)
        {
            if (!this.lst_Rcv.InvokeRequired)
            {
                ListViewItem lst = new ListViewItem("   " + CurrentTime, index);

                lst.SubItems.Add(info);

                lst_Rcv.Items.Insert(lst_Rcv.Items.Count, lst);

            }
            else
            {
                Invoke(new Action(() =>
                {
                    ListViewItem lst = new ListViewItem("   " + CurrentTime, index);

                    lst.SubItems.Add(info);

                    lst_Rcv.Items.Insert(lst_Rcv.Items.Count, lst);
                }));
            }
        }

        #endregion

        #region 窗体关闭
        private void FrmTCPClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            socketClient?.Close();
        }

        #endregion

        #region 断开服务器

        private void btn_DisConn_Click(object sender, EventArgs e)
        {
            socketClient?.Close();
        }

        #endregion

        #region 发送ASCII
        private void btn_SendASCII_Click(object sender, EventArgs e)
        {
            AddLog(0, "发送内容：" + this.txt_Send.Text.Trim());

            byte[] send = Encoding.ASCII.GetBytes(this.txt_Send.Text.Trim());

            //创建最终发送的数组
            byte[] sendMsg = new byte[send.Length + 1];

            //整体拷贝数组
            Array.Copy(send, 0, sendMsg, 1, send.Length);

            //给首字节赋值

            sendMsg[0] = (byte)MessageType.ASCII;

            socketClient?.Send(sendMsg);

            this.txt_Send.Clear();
        }

        #endregion

        #region 发送UTF8

        private void btn_SendUTF8_Click(object sender, EventArgs e)
        {
            AddLog(0, "发送内容：" + this.txt_Send.Text.Trim());

            byte[] send = Encoding.UTF8.GetBytes(this.txt_Send.Text.Trim());

            //创建最终发送的数组
            byte[] sendMsg = new byte[send.Length + 1];

            //整体拷贝数组
            Array.Copy(send, 0, sendMsg, 1, send.Length);

            //给首字节赋值

            sendMsg[0] = (byte)MessageType.UTF8;

            socketClient?.Send(sendMsg);

            this.txt_Send.Clear();
        }


        #endregion

        #region 发送Hex
        private void btn_SendHex_Click(object sender, EventArgs e)
        {
            AddLog(0, "发送内容：" + this.txt_Send.Text.Trim());

            byte[] send = Encoding.Default.GetBytes(this.txt_Send.Text.Trim());

            //创建最终发送的数组
            byte[] sendMsg = new byte[send.Length + 1];

            //整体拷贝数组
            Array.Copy(send, 0, sendMsg, 1, send.Length);

            //给首字节赋值

            sendMsg[0] = (byte)MessageType.Hex;

            socketClient?.Send(sendMsg);

            this.txt_Send.Clear();
        }

        #endregion

        #region 选择文件
        private void btn_SelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            //设置默认的路径
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this.txt_File.Text = ofd.FileName;

                AddLog(0, "选择文件：" + this.txt_File.Text);
            }
        }
        #endregion

        #region 发送文件

        private void btn_SendFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txt_File.Text))
            {
                MessageBox.Show("请先选择你要发送的文件路径", "发送文件");
                return;
            }
            else
            {
                //发送两次

                using (FileStream fs = new FileStream(this.txt_File.Text, FileMode.Open))
                {
                    //第一次发送文件名称

                    //获取文件名称
                    string filename = Path.GetFileName(this.txt_File.Text);
                    //获取后缀名
                    string fileExtension = Path.GetExtension(this.txt_File.Text);

                    string strMsg = "发送文件：" + filename + "." + fileExtension;

                    byte[] send1 = Encoding.UTF8.GetBytes(strMsg);

                    byte[] send1Msg = new byte[send1.Length + 1];

                    Array.Copy(send1, 0, send1Msg, 1, send1.Length);

                    send1Msg[0] = (byte)MessageType.UTF8;

                    socketClient?.Send(send1Msg);

                    //第二次发送文件内容

                    byte[] send2 = new byte[1024 * 1024 * 10];

                    //有效长度
                    int length = fs.Read(send2, 0, send2.Length);

                    byte[] send2Msg = new byte[length + 1];

                    Array.Copy(send2, 0, send2Msg, 1, length);

                    send2Msg[0] = (byte)MessageType.File;

                    socketClient?.Send(send2Msg);

                    this.txt_File.Clear();

                    AddLog(0, strMsg);
                }
            }
        }

        #endregion

        #region 发送JSON
        private void btn_SendJSON_Click(object sender, EventArgs e)
        {
            //创建集合
            List<Student> stuList = new List<Student>()
            {
                new Student(){ StudentID=10001,StudentName="小明",ClassName="软件一班"},
                new Student(){ StudentID=10002,StudentName="小红",ClassName="软件二班"},
                new Student(){ StudentID=10003,StudentName="小花",ClassName="软件三班"},
            };


            string str = JSONHelper.EntityToJSON(stuList);

            byte[] send = Encoding.Default.GetBytes(str);

            byte[] sendMsg = new byte[send.Length + 1];

            Array.Copy(send, 0, sendMsg, 1, send.Length);

            sendMsg[0] =(byte) MessageType.JSON;

            socketClient?.Send(sendMsg);
        }

        #endregion

        #region 16进制字符串处理

        private string HexGetString(byte[] buffer, int start, int length)
        {
            string Result = string.Empty;

            if (buffer != null && buffer.Length >= start + length)
            {
                //截取字节数组

                byte[] res = new byte[length];

                Array.Copy(buffer, start, res, 0, length);

                string Hex = Encoding.Default.GetString(res, 0, res.Length);

                // 01   03 0 40 0A
                if (Hex.Contains(" "))
                {
                    string[] str = Regex.Split(Hex, "\\s+", RegexOptions.IgnoreCase);

                    foreach (var item in str)
                    {
                        Result += "0x" + item + " ";
                    }
                }
                else
                {
                    Result += "0x" + Hex;
                }

            }
            else
            {
                Result = "Error";
            }
            return Result;
        }


        #endregion

    }
}
