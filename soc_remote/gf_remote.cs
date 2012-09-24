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
using System.IO.Ports;
using soc_remote.serialport;

namespace soc_remote
{
    public partial class gf_remote : Form
    {
        #region //in order to move the form
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        #endregion

        cc_controls mctrls;//create a controls object
        string dir;//directory

        public gf_remote()
        {
            InitializeComponent();
          
            mctrls = new cc_controls();//initialization of controls object
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
                        string infoColor = "tv_radio,return,OK";
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
                        foreach (string mInfoColor in infoColor.Split(','))
                        {
                            //if (ctrl.Ctr_Text == mInfoColor)
                            //{
                            //    myPath.AddRectangle(new Rectangle(4, 4, button.Width, button.Height));
                            //    button.Region = new Region(myPath);
                            //    //填充渐变
                            //    Bitmap B = new Bitmap(button.Width, button.Height); ;
                            //    Graphics G = Graphics.FromImage(B); ;

                            //    G.FillPath(new System.Drawing.Drawing2D.LinearGradientBrush(new Point(4, 4), new Point(4, button.Height), Color.Yellow, Color.Red), myPath);
                            //    //描绘椭圆边框
                            //    //G.DrawRectangle(new Pen(Brushes.Black, 3), new Rectangle(4, 4, button.Width, button.Height - 4));
                            //    //设置为其背景，不影响Image属性
                            //    button.BackgroundImage = B;
                            //    //取消难看的自有边框
                            //    button.FlatAppearance.BorderSize = 0;
                            //    //使用“抬起”效果是为了使按钮文字有点击响应视觉
                            //    button.FlatStyle = FlatStyle.Popup;
                            //}

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
        bool sign = true;
        private void button_Click(object sender, EventArgs e)
        {
            Button btn_Click = (Button)sender;
            for (int i = 0; i < cc_control.dtEvent.Rows.Count; i++)
            {
                if (cc_control.dtEvent.Rows[i][gf_main.CTRLEVENTNAME].ToString() == btn_Click.Text)
                {
                    if (cc_serial.mSpSlot== null)
                    {
                        ConsoleU.writeLine(String.Format("No choice of serial"), ConsoleU.Level.Error);
                    }
                    else
                    {
                        ConsoleU.writeLine(String.Format("Your choice of serial:{0}", cc_serial.mSpSlot.PortName), ConsoleU.Level.Info);
                    }
                    ConsoleU.writeLine(String.Format("you click: {0} button, \"{1}\"", btn_Click.Text, cc_control.dtEvent.Rows[i][gf_main.CTRLLINE].ToString()), ConsoleU.Level.Info);
                }

                //open the main window 
                if (btn_Click.Text == "detail")
                {
                    //if (!sign)
                    //{
                    //    sign = !sign;
                    //    mMain.Move += new EventHandler(subFormMove);
                    //    mMain.Show();
                    //    //this.AddOwnedForm(mMain);
                    //    mDockMain = new DockU(this);
                    //    mDockMain.isEnabled = true;
                    //    mDockMain.position = DockU.Position.MiddleRight;
                    //    mDockMain.process(mMain);

                    //    break;
                    //}
                    //if (sign)
                    //{
                    //    sign = !sign;
                    //    mMain.Hide();
                    //    this.AddOwnedForm(mMain);
                    //    break;
                    //}
                    
                }
            }

        }
        #endregion

        #region Off remote controller
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
