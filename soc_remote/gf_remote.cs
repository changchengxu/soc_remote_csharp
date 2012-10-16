#define  printf
#undef   printf
#define printf

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using soc_remote.control;
using GSoft.Utility;
using SocProtocol;
using SocSerialPort;

using System.IO.Ports;
using System.Threading;

namespace soc_remote
{
    public  delegate gf_serialize mainFormTrans(string a);// Avoid repeatedly Open subform
    public delegate void serializeFormTrans(string b);//add or edit serialize frm

    public partial class gf_remote : Form
    {
        #region //in order to move the form
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        #endregion

        public event mainFormTrans mMainFormTrans;//this is a delegate in order to  close subForm
        cc_controls mctrls;//create a controls object
        string dir;//directory
        SocProtocol.UartProtocol ProtocolObject; //create protocol object
        Thread t;

        public gf_remote(SocSerialPort.Uart mSpSlot)
        {
            InitializeComponent();

            mctrls = new cc_controls();//initialization of controls object
            ProtocolObject = new SocProtocol.UartProtocol(mSpSlot.slot);

            t = new Thread(new ThreadStart(LoopRandom));
            t.Start();
            t.IsBackground = true;
            //            mCurrentSport = sp;
            //            ConsoleU.writeLine(String.Format("remote display serial port config: \n dataBits({0}) \n parity({1}) \n stopBit({2}) \n baudRate({3}) \n readTimeout({4})",
            //mCurrentSport.DataBits, mCurrentSport.Parity, mCurrentSport.StopBits, mCurrentSport.BaudRate, mCurrentSport.ReadTimeout), ConsoleU.Level.Error);
        }

        #region initialization interface //  [9/23/2012 Administrator]
        private void gf_remote_Paint(object sender, PaintEventArgs e)
        {
            Type(this, 15, 0.11);//create a oval interface
        }
        private void Type(Control sender, int p_1, double p_2)
        {
            GraphicsPath oPath = new GraphicsPath();
            oPath.AddClosedCurve(new Point[] { new Point(0, sender.Height / p_1),
                new Point(sender.Width / p_1, 0),
                new Point(sender.Width - sender.Width / p_1, 0), 
                new Point(sender.Width, sender.Height / p_1), 
                new Point(sender.Width, sender.Height - sender.Height / p_1),
                new Point(sender.Width - sender.Width / p_1, sender.Height),
                new Point(sender.Width / p_1, sender.Height),
                new Point(0, sender.Height - sender.Height / p_1) }, (float)p_2);
            sender.Region = new Region(oPath);
        }

        private void gf_remote_MouseDown(object sender, MouseEventArgs e)//click the mouse can move the form
        {
            if (e.Button == MouseButtons.Left & this.WindowState == FormWindowState.Normal)
            {
                // 移动窗体
                this.Capture = false;
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        #endregion

        #region gf_remote_Load
        private void gf_remote_Load(object sender, EventArgs e)
        {
            //load control
            dir = String.Format("{0}/{1}/{2}",
                 System.IO.Directory.GetCurrentDirectory(), gf_main.FILEPATH, gf_main.CTRLNAME);
            DataSet ds = new DataSet();
            mctrls.loadDataSet(dir, ds);//init Controls DataSet
            loadCtrls();//init control
        }
        #endregion

        #region load Control // [9/20/2012 Administrator]
        void loadCtrls()
        {
            if (mctrls.ctrlCount > 0)
            {
                Int32 index = 0;
                foreach (cc_control ctrl in mctrls.ctrlsItem)
                {
                    if (ctrl.Ctr_Type == "button")
                    {
                        System.Windows.Forms.Button button;
                        button = (new System.Windows.Forms.Button());
                        button.Name = "button_" + index;
                        button.Text = ctrl.Ctr_Text;
                        button.TabIndex = index;
                        this.Controls.Add(button);

                        button.Location = new System.Drawing.Point(ctrl.Ctr_X, ctrl.Ctr_Y);
                        button.Size = new System.Drawing.Size(ctrl.Ctr_Width, ctrl.Ctr_Height);
                        button.UseVisualStyleBackColor = true;
                        button.Click += new System.EventHandler(button_Click);
                        //button.Enter += new System.EventHandler(button_Enter);

                        String channelColor = "0,1,2,3,4,5,6,7,8,9,UP,DOWN,LEFT,RIGHT"; //set channel color 
                        //下面是我在网上搜集的
                        System.Drawing.Drawing2D.GraphicsPath myPath = new System.Drawing.Drawing2D.GraphicsPath();

                        foreach (String mChannelColor in channelColor.Split(','))
                        {
                            if (ctrl.Ctr_Text == mChannelColor)
                            {
                                //绘制从边缘向内收缩4像素的椭圆
                                myPath.AddEllipse(new Rectangle(4, 4, button.Width - 8, button.Height - 8));
                                //'设置按钮形状为该椭圆形状
                                button.Region = new Region(myPath);
                                //填充渐变
                                Bitmap B = new Bitmap(button.Width, button.Height); ;
                                Graphics G = Graphics.FromImage(B); ;

                                G.FillPath(new System.Drawing.Drawing2D.LinearGradientBrush(new Point(4, 4), new Point(4, button.Height), Color.Yellow, Color.Red), myPath);
                                //描绘椭圆边框
                                G.DrawEllipse(new Pen(Brushes.Black, 3), new Rectangle(4, 4, button.Width - 8, button.Height - 8));
                                //设置为其背景，不影响Image属性
                                button.BackgroundImage = B;
                                //取消难看的自有边框
                                button.FlatAppearance.BorderSize = 0;
                                //使用“抬起”效果是为了使按钮文字有点击响应视觉
                                button.FlatStyle = FlatStyle.Popup;
                            }
                        }
                        button.FlatStyle = FlatStyle.Popup;

                        if (ctrl.Ctr_Text == "RED") { button.BackColor = System.Drawing.Color.Red; }
                        if (ctrl.Ctr_Text == "GREEN") { button.BackColor = System.Drawing.Color.Green; }
                        if (ctrl.Ctr_Text == "YELLOW") { button.BackColor = System.Drawing.Color.Yellow; }
                        if (ctrl.Ctr_Text == "BLUE") { button.BackColor = System.Drawing.Color.Blue; }
                        //button.Click+=new EventHandler();
#if print
				ConsoleU.writeLine(String.Format("buttonName={0}\"buttonText={1}\"X={2}\"Y={3}",button.Name,button.Text,ctrl.Ctr_X,ctrl.Ctr_Y), ConsoleU.Level.Info);
#endif
                    }
                    index++;
                }
            }
        }
        #endregion

        #region remote button click
        //////////Byte[] sendData;

        gf_serialize a;
        private void button_Click(object sender, EventArgs e)
        {
            Button btn_Click = (Button)sender;
            if (gf_main.serializeSigh == "serialize")
            {
                gf_serialize a= mMainFormTrans("serializeFrm");
                a.remoteEventReceive(btn_Click.Text);
                return;
            }

            for (int i = 0; i < cc_control.dtEvent.Rows.Count; i++)
            {
                if (cc_control.dtEvent.Rows[i][gf_main.CTRLEVENTNAME].ToString() == btn_Click.Text)
                {

                        ConsoleU.writeLine(String.Format("you click: {0} button, \"{1}\"", btn_Click.Text, cc_control.dtEvent.Rows[i][gf_main.CTRLLINE].ToString()), ConsoleU.Level.Info);
                        ProtocolObject.remoteWrite(SocProtocol.Drive.remote, System.Text.Encoding.ASCII.GetBytes(cc_control.dtEvent.Rows[i][gf_main.CTRLLINE].ToString()));
                 
                    break;
                }
            }

        }

        //#region  Component package
        //Byte[] aLengTotal;
        //Int32 pack(Byte[] data, ref Byte[] pPacket)
        //{
        //    Int32 errCode = 0;

        //    UInt16 lenTotal = 7;
        //    if (data != null)
        //    {
        //        lenTotal += (UInt16)data.Length;
        //    }
        //    Byte[] packet = new Byte[lenTotal];

        //    packet[(Int32)Index.startCode] = 0xE6;
        //    packet[(Int32)Index.protocolType] = 0x00;
        //    packet[(Int32)Index.commandDirection] = 0xDB;
        //    packet[(Int32)Index.commandCode] = 0x1A;

        //    //aLengTotal = System.BitConverter.GetBytes(lenTotal); 

        //    errCode = uIntToByteArray(lenTotal, ref aLengTotal);
        //    if (errCode < 0)
        //    {
        //        return errCode;
        //    }

        //    aLengTotal.CopyTo(packet, (Int32)Index.commnadLengthH);

        //    // pack data
        //    if (data != null)
        //    {
        //        data.CopyTo(packet, (Int32)Index.data);
        //    }

        //    Byte sum = 0;
        //    foreach (Byte cell in packet)
        //    {
        //        sum += cell;
        //    }

        //    packet[packet.Length - 1] = sum;

        //    pPacket = packet;

        //    return 0;
        //}

        ////Packet format
        //public enum Index
        //{
        //    startCode,
        //    protocolType,
        //    commandDirection,
        //    commandCode,
        //    commnadLengthH,
        //    commandLengthL,
        //    data
        //};
        ////uInt to ByteArray
        //public Int32 uIntToByteArray(UInt16 uInt, ref Byte[] pArray)
        //{
        //    Int32 errCode = 0;
        //    pArray = new Byte[sizeof(UInt16)];

        //    (pArray)[0] = Convert.ToByte((uInt & 0x0000FF00) >> (int)getMaskOffset(0x0000FF00));
        //    (pArray)[1] = Convert.ToByte((uInt & 0x000000FF) >> (int)getMaskOffset(0x000000FF));
        //    return errCode;
        //}

        //UInt32 getMaskOffset(UInt32 mask)
        //{
        //    UInt32 offset = 0;
        //    const UInt32 maskBit0 = 0x00000001;

        //    if (mask <= 0)
        //    {
        //        return offset;
        //    }

        //    for (offset = 0; offset < sizeof(UInt32) * 8; offset++)
        //    {
        //        if ((mask & maskBit0) > 0)
        //        { // bit0 is 1
        //            break;
        //        }

        //        mask >>= 1;
        //    }

        //    return offset;
        //}
        //#endregion

        #endregion

        #region random Loop

        Random ran = new Random();
        Int32 ranID = 0;
        //private void timer1_Tick(object sender, EventArgs e)
        private void LoopRandom()
        {
            while (true)
            {
                if (gf_main.testBegin)//click test button
                {
                    if (gf_main.randomOrNo)//click random button
                    {
                        ranID = ran.Next(cc_control.dtEvent.Rows.Count);
                        if (cc_control.dtEvent.Rows[ranID][gf_main.CTRLLINE].ToString() != "")//avoid empty event
                        {
                            ConsoleU.writeLine(String.Format("you click: {0} button, \"{1}\"", cc_control.dtEvent.Rows[ranID][gf_main.CTRLEVENTNAME].ToString(), cc_control.dtEvent.Rows[ranID][gf_main.CTRLLINE].ToString()), ConsoleU.Level.Info);
                            ProtocolObject.remoteWrite(SocProtocol.Drive.remote, System.Text.Encoding.ASCII.GetBytes(cc_control.dtEvent.Rows[ranID][gf_main.CTRLLINE].ToString()));
                            System.Threading.Thread.Sleep(2000);
                        }
                    }
                    else
                    {
                        for (int i = gf_main.dgvIndex; i < gf_main.dsRandomXml.Tables[0].Rows.Count; i++)
                        {
                            string[] random = gf_main.dsRandomXml.Tables[0].Rows[i]["serialize_Text"].ToString().Split(',');
                            foreach (string mRandom in random)
                            {
                                for (int index = 0; index < cc_control.dtEvent.Rows.Count; index++)
                                {
                                    if (!gf_main.testBegin)//click test button
                                    {
                                        break;
                                    }
                                    if (mRandom == cc_control.dtEvent.Rows[index][gf_main.CTRLEVENTNAME].ToString())
                                    {
                                        ConsoleU.writeLine(String.Format("you click: {0} button, \"{1}\"", cc_control.dtEvent.Rows[index][gf_main.CTRLEVENTNAME].ToString(), cc_control.dtEvent.Rows[index][gf_main.CTRLLINE].ToString()), ConsoleU.Level.Info);
                                        ProtocolObject.remoteWrite(SocProtocol.Drive.remote, System.Text.Encoding.ASCII.GetBytes(cc_control.dtEvent.Rows[index][gf_main.CTRLLINE].ToString()));
                                        System.Threading.Thread.Sleep(2000);
                                    }
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }
        #endregion

        #region Off remote controller
        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("To exit from this page", pictureBox1, pictureBox1.Width, pictureBox1.Height, 2000);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            mMainFormTrans("off");
            t.Abort();
            gf_main.testBegin = false;
            this.Close();
        }
        #endregion

    }

}
