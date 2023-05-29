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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace feeel.ca.SocketProject
{
    public partial class FrmJSON : Form
    {
        public FrmJSON(List<Student> stuList)
        {
            InitializeComponent();

            this.dataGridView1.AutoGenerateColumns = false;

            this.dataGridView1.DataSource = null;
            this.dataGridView1.DataSource = stuList;
        }
    }
}
