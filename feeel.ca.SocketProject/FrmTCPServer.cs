// ***********************************************************************
//    Assembly       : �¸����
//    Created          : 2022-11-01
// ***********************************************************************
//     Copyright by �¸����������Ǹ�����Ƽ����޹�˾��
//     QQ��        2934008828������ʦ��  
//     WeChat��thinger002������ʦ��
//     ���ںţ�   dotNet������λ��
//     ����������dotNet������λ��
//     ֪����      dotNet������λ��
//     ͷ����      dotNet������λ��
//     ��Ƶ�ţ�   dotNet������λ��
//     ��Ȩ���У��Ͻ�����
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
    public enum MessageType
    {
        ASCII,
        UTF8,
        Hex,
        File,
        JSON
    }

    public partial class FrmTCPServer : Form
    {
        public FrmTCPServer()
        {
            InitializeComponent();
            this.Load += FrmTCPServer_Load;
        }

        private void FrmTCPServer_Load(object sender, EventArgs e)
        {
            this.lst_Rcv.Columns[1].Width = this.lst_Rcv.ClientSize.Width - this.lst_Rcv.Columns[0].Width;
        }

        /*
         
         �������˳���ı�д���裺
         ��һ��������socket()��������һ������ͨ�ŵ��׽��֡�
         �ڶ��������Ѿ��������׽��ְ�һ���˿ںţ���һ��ͨ�����������׽ӿڵ�ַ�͵���bind()������ʵ�֡�
         ������������listen()����ʹ�׽��ֳ�Ϊһ�������׽��֡�
         ���Ĳ�������accept()���������ܿͻ��˵����ӣ����ǾͿ��ԺͿͻ���ͨ���ˡ�
         ���岽������ͻ��˵���������
         ����������ֹ���ӡ�
       
         */

        //����һ��Socket����
        private Socket socketSever;

        //�����ֵ伯�ϣ�����ClientIp,ֵ��SocketClient
        private Dictionary<string, Socket> CurrentClientlist = new Dictionary<string, Socket>();

        #region ��������
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_StartService_Click(object sender, EventArgs e)
        {
            // ��һ��������socket()��������һ������ͨ�ŵ��׽���
            socketSever = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // �ڶ��������Ѿ��������׽��ְ�һ���˿ںţ���һ��ͨ�����������׽ӿڵ�ַ�͵���bind()������ʵ�֡�

            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(this.txt_IP.Text), int.Parse(this.txt_Port.Text));

            try
            {
                socketSever.Bind(ipe);
            }
            catch (Exception ex)
            {
                //д����־

                AddLog(2, "����������ʧ�ܣ�" + ex.Message);
                return;
            }

            // ������������listen()����ʹ�׽��ֳ�Ϊһ�������׽��֡�
            socketSever.Listen(10);

            //����һ���������߳�

            Task.Run(new Action(() =>
            {
                CheckListening();
            }));

            AddLog(0, "�����������ɹ�");

            this.btn_StartService.Enabled = false;
        }

        #endregion

        #region �����߳�

        /// <summary>
        /// ���������̷߳�����
        /// </summary>
        private void CheckListening()
        {
            while (true)
            {
                // ���Ĳ�������accept()���������ܿͻ��˵����ӣ����ǾͿ��ԺͿͻ���ͨ���ˡ�
                Socket socketClient = socketSever.Accept();

                string client = socketClient.RemoteEndPoint.ToString();

                AddLog(0, client + "������");

                CurrentClientlist.Add(client, socketClient);

                UpdateOnline(client, true);

                Task.Run(new Action(() =>
                {
                    ReceiveMessage(socketClient);
                }));
            }
        }


        #endregion

        #region ���߳̽�������
        /// <summary>
        /// ���տͻ������ݵķ���
        /// </summary>
        /// <param name="socketClient"></param>
        private void ReceiveMessage(Socket socketClient)
        {
            while (true)
            {
                // ����һ��������

                byte[] buffer = new byte[1024 * 1024 * 10];

                int length = -1;

                string client = socketClient.RemoteEndPoint.ToString();

                // ���岽������ͻ��˵���������
                try
                {
                    length = socketClient.Receive(buffer);
                }
                catch (Exception)
                {
                    UpdateOnline(client, false);
                    AddLog(0, client + "������");
                    CurrentClientlist.Remove(client);
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

                            AddLog(0, client + "��" + msg);

                            break;
                        case MessageType.UTF8:

                            msg = Encoding.UTF8.GetString(buffer, 1, length - 1);

                            AddLog(0, client + "��" + msg);

                            break;
                        case MessageType.Hex:

                            msg = HexGetString(buffer, 1, length - 1);

                            AddLog(0, client + "��" + msg);

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

                                    AddLog(0, "�ļ��ɹ�������" + fileSavePath);
                                }
                            }));

                            break;
                        case MessageType.JSON:

                            Invoke(new Action(() =>
                            {
                                string res = Encoding.Default.GetString(buffer, 1, length);

                                List<Student> StuList = JSONHelper.JSONToEntity<List<Student>>(res);

                                new FrmJSON(StuList).Show();

                                AddLog(0, "����JSON���ݣ�" + res);

                            }));

                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    UpdateOnline(client, false);
                    AddLog(0, client + "������");
                    CurrentClientlist.Remove(client);
                    break;
                }
            }
        }

        #endregion

        #region ������Ϣ�ķ���

        //��ǰʱ������
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

        #region �����б����

        private void UpdateOnline(string client, bool operate)
        {
            if (!this.lst_Online.InvokeRequired)
            {
                if (operate)
                {
                    this.lst_Online.Items.Add(client);
                }
                else
                {
                    foreach (string item in this.lst_Online.Items)
                    {
                        if (item == client)
                        {
                            this.lst_Online.Items.Remove(item);
                            break;
                        }
                    }
                }
            }
            else
            {
                Invoke(new Action(() =>
                {
                    if (operate)
                    {
                        this.lst_Online.Items.Add(client);
                    }
                    else
                    {
                        foreach (string item in this.lst_Online.Items)
                        {
                            if (item == client)
                            {
                                this.lst_Online.Items.Remove(item);
                                break;
                            }
                        }
                    }
                }));
            }
        }

        #endregion

        #region 16�����ַ�������

        private string HexGetString(byte[] buffer, int start, int length)
        {
            string Result = string.Empty;

            if (buffer != null && buffer.Length >= start + length)
            {
                //��ȡ�ֽ�����

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

        #region ����ASCII

        private void btn_SendASCII_Click(object sender, EventArgs e)
        {
            if (this.lst_Online.SelectedItems.Count > 0)
            {
                AddLog(0, "�������ݣ�" + this.txt_Send.Text.Trim());

                byte[] send = Encoding.ASCII.GetBytes(this.txt_Send.Text.Trim());

                //�������շ��͵�����
                byte[] sendMsg = new byte[send.Length + 1];

                //���忽������
                Array.Copy(send, 0, sendMsg, 1, send.Length);

                //�����ֽڸ�ֵ

                sendMsg[0] = (byte)MessageType.ASCII;

                foreach (var item in this.lst_Online.SelectedItems)
                {
                    //��ȡSocket����
                    string client = item.ToString();

                    CurrentClientlist[client]?.Send(sendMsg);
                }

                this.txt_Send.Clear();
            }
            else
            {
                MessageBox.Show("��ѡ����Ҫ���͵Ŀͻ��˶���", "������Ϣ");
            }
        }

        #endregion

        #region �򿪿ͻ���
        private void btn_Client_Click(object sender, EventArgs e)
        {
            new FrmTCPClient().Show();
        }

        #endregion

        #region ����UTF8
        private void btn_SendUTF8_Click(object sender, EventArgs e)
        {
            if (this.lst_Online.SelectedItems.Count > 0)
            {
                AddLog(0, "�������ݣ�" + this.txt_Send.Text.Trim());

                byte[] send = Encoding.UTF8.GetBytes(this.txt_Send.Text.Trim());

                //�������շ��͵�����
                byte[] sendMsg = new byte[send.Length + 1];

                //���忽������
                Array.Copy(send, 0, sendMsg, 1, send.Length);

                //�����ֽڸ�ֵ

                sendMsg[0] = (byte)MessageType.UTF8;

                foreach (var item in this.lst_Online.SelectedItems)
                {
                    //��ȡSocket����
                    string client = item.ToString();

                    CurrentClientlist[client]?.Send(sendMsg);
                }

                this.txt_Send.Clear();
            }
            else
            {
                MessageBox.Show("��ѡ����Ҫ���͵Ŀͻ��˶���", "������Ϣ");
            }
        }

        #endregion

        #region ����Hex
        private void btn_SendHex_Click(object sender, EventArgs e)
        {
            if (this.lst_Online.SelectedItems.Count > 0)
            {
                AddLog(0, "�������ݣ�" + this.txt_Send.Text.Trim());

                byte[] send = Encoding.Default.GetBytes(this.txt_Send.Text.Trim());

                //�������շ��͵�����
                byte[] sendMsg = new byte[send.Length + 1];

                //���忽������
                Array.Copy(send, 0, sendMsg, 1, send.Length);

                //�����ֽڸ�ֵ

                sendMsg[0] = (byte)MessageType.Hex;

                foreach (var item in this.lst_Online.SelectedItems)
                {
                    //��ȡSocket����
                    string client = item.ToString();

                    CurrentClientlist[client]?.Send(sendMsg);
                }

                this.txt_Send.Clear();
            }
            else
            {
                MessageBox.Show("��ѡ����Ҫ���͵Ŀͻ��˶���", "������Ϣ");
            }
        }

        #endregion

        #region ѡ���ļ�
        private void btn_SelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            //����Ĭ�ϵ�·��
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this.txt_File.Text = ofd.FileName;

                AddLog(0, "ѡ���ļ���" + this.txt_File.Text);
            }
        }

        #endregion

        #region �����ļ�
        private void btn_SendFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txt_File.Text))
            {
                MessageBox.Show("����ѡ����Ҫ���͵��ļ�·��", "�����ļ�");
                return;
            }
            else
            {
                if (this.lst_Online.SelectedItems.Count > 0)
                {
                    //��������

                    using (FileStream fs = new FileStream(this.txt_File.Text, FileMode.Open))
                    {
                        //��һ�η����ļ�����

                        //��ȡ�ļ�����
                        string filename = Path.GetFileName(this.txt_File.Text);
                        //��ȡ��׺��
                        string fileExtension = Path.GetExtension(this.txt_File.Text);

                        string strMsg = "�����ļ���" + filename + "." + fileExtension;

                        byte[] send1 = Encoding.UTF8.GetBytes(strMsg);

                        byte[] send1Msg = new byte[send1.Length + 1];

                        Array.Copy(send1, 0, send1Msg, 1, send1.Length);

                        send1Msg[0] = (byte)MessageType.UTF8;

                        foreach (var item in this.lst_Online.SelectedItems)
                        {
                            //��ȡSocket����
                            string client = item.ToString();

                            CurrentClientlist[client]?.Send(send1Msg);
                        }


                        //�ڶ��η����ļ�����

                        byte[] send2 = new byte[1024 * 1024 * 10];

                        //��Ч����
                        int length = fs.Read(send2, 0, send2.Length);

                        byte[] send2Msg = new byte[length + 1];

                        Array.Copy(send2, 0, send2Msg, 1, length);

                        send2Msg[0] = (byte)MessageType.File;

                        foreach (var item in this.lst_Online.SelectedItems)
                        {
                            //��ȡSocket����
                            string client = item.ToString();

                            CurrentClientlist[client]?.Send(send2Msg);
                        }

                        this.txt_File.Clear();

                        AddLog(0, strMsg);
                    }
                }
                else
                {
                    MessageBox.Show("��ѡ����Ҫ���͵Ŀͻ��˶���", "������Ϣ");
                }
            }
        }

        #endregion

        #region ����JSON
        private void btn_SendJSON_Click(object sender, EventArgs e)
        {
            if (this.lst_Online.SelectedItems.Count > 0)
            {
                //��������
                List<Student> stuList = new List<Student>()
            {
                new Student(){ StudentID=10001,StudentName="С��",ClassName="���һ��"},
                new Student(){ StudentID=10002,StudentName="С��",ClassName="�������"},
                new Student(){ StudentID=10003,StudentName="С��",ClassName="�������"},
            };


                string str = JSONHelper.EntityToJSON(stuList);

                byte[] send = Encoding.Default.GetBytes(str);

                byte[] sendMsg = new byte[send.Length + 1];

                Array.Copy(send, 0, sendMsg, 1, send.Length);

                sendMsg[0] = (byte)MessageType.JSON;

                foreach (var item in this.lst_Online.SelectedItems)
                {
                    //��ȡSocket����
                    string client = item.ToString();

                    CurrentClientlist[client]?.Send(sendMsg);
                }

            }
            else
            {
                MessageBox.Show("��ѡ����Ҫ���͵Ŀͻ��˶���", "������Ϣ");
            }
        }

        #endregion

        #region ȫ��ѡ��
        private void btn_SelectAll_Click(object sender, EventArgs e)
        {
            if (this.lst_Online.Items.Count == 0)
            {
                MessageBox.Show("��ǰ�����б�Ϊ�գ��޷�ȫѡ��","ȫ��ѡ��");
                return;
            }

            for (int i = 0; i < this.lst_Online.Items.Count; i++)
            {
                this.lst_Online.SetSelected(i, true);
            }
        }
        #endregion

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
