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
using GSoft.Utility;
using soc_remote.serialport;
using System.Drawing.Drawing2D;
using System.IO.Ports;

namespace soc_remote
{

    public partial class gf_main : Form
    {
        public const string VERSION = "1.0";
        public const string FILEPATH = "config";         //read the xml folder
        public const string TREENAME = "remote.xml";     //read the xml file about the tree 
        public const string CTRLNAME = "controlFile.xml";//read the xml file about the controls
        public const string SERIAL = "serialport.xml"; //read the xml file about the serial port


        public const string DIRTREE = "dirtree";        //read the xml file node
        public const string DIRCTRL = "control";        //read the xml file node
        public const string DIREVENT = "event";         //read the xml file event node

        public const string CURRENT = "current";        //binding tree node...
        public const string PARENT = "parent";
        public const string CURRENTNMAE = "currentname";
        public const string REMOTE_MENU = "remote menu";          //tree parent node
        public const string SYS_TIME = "sys time";        //tree parent node

        public const string CTRLTEXT = "text"; // control parameter...
        public const string CTRLX = "x";
        public const string CTRLY = "y";
        public const string CTRLWIDTH = "w";
        public const string CTRLHEIGHT = "h";
        public const string CTRLTYPE = "type";
        public const string CTRLEVENT = "event";//control event...
        public const string CTRLLINE = "line";
        public const string CTRLEVENTNAME = "eventname";

        string dir;//directory
        public static SerialPort mCurrentSport;//Gets the current serial port object 
        DockU mDockRemote;//Create a subsidiary form

        #region //in order to move the form
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);//in order to move the form   
        #endregion

        public gf_main(string[] args)
        {
            InitializeComponent();
#if printf
            Text = String.Format("{0} v{1} < build {2} >", Text, gf_main.VERSION, DateTime.Now.ToString());
            ConsoleU.writeLine(Text, ConsoleU.Level.Warning);

            string mArgs = String.Empty;
            foreach (string arg in args)
            {
                mArgs += arg + " ";
            }
            mArgs = mArgs.Trim();

            ConsoleU.writeLine(String.Format("args = \"{0}\"", mArgs), ConsoleU.Level.Warning);
            ConsoleU.writeLine(String.Format("work dir = \"{0}\"", Application.StartupPath),
                ConsoleU.Level.Warning);
#endif

            debugToolStripMenuItem.Checked = true;//for debug initialize selected

            toolStripStatusLabel1.Text = String.Format(" {0} {1} v{2} < build {2} >","     ",Text , VERSION, DateTime.Now.ToString());

        }

        #region main load
        private void gf_main_Load(object sender, EventArgs e)
        {
            //load Tree
            loadRemoteTree();

            //load serial
            String[] ports = System.IO.Ports.SerialPort.GetPortNames();
            Array.Sort(ports);
            Int32 index = 0;
            foreach (String port in ports)
            {

                ToolStripMenuItem sportToolStripMenuItem;
                sportToolStripMenuItem = new ToolStripMenuItem();

                sportToolStripMenuItem.Name = "sportToolStripMenuItem" + index;
                sportToolStripMenuItem.Size = new System.Drawing.Size(109, 22 + 5 + index);
                sportToolStripMenuItem.Text = port;
                sportToolStripMenuItem.Click += new System.EventHandler(this.serialToolStripMenuItem_Click);
                serialToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                sportToolStripMenuItem});
                index++;
            }

            //dataGridView1.DataSource = ds.Tables[DIREVENT];//init dataGridView 

        }
        #endregion

        #region load Tree    // [9/20/2012 Administrator]
        void loadRemoteTree()
        {
            treeView_dir.Nodes.Clear();
            DataTable dtTemp = new DataTable();

            String printInfo = "binding tree node... ...";
            ConsoleU.writeLine(printInfo, ConsoleU.Level.Warning);

            DataSet ds = new DataSet();
            /*dir=String.Format("{0}/{1}/{2}", 
            Directory.GetCurrentDirectory(), FILEPATH, CTRLNAME);*/
            dir = String.Format("{0}/{1}/{2}",
                Application.StartupPath, FILEPATH, TREENAME);

            String fileExtension = System.IO.Path.GetExtension(dir);
            if (fileExtension != ".xml" || !System.IO.File.Exists(dir))
            {
                ConsoleU.writeLine("this file is not xml or path error,please select again!", ConsoleU.Level.Warning);
            }
            ds.ReadXml(dir);
            dtTemp = ds.Tables[DIRTREE];//this is Tree xml file

            if (dtTemp == null) { printInfo = "Cannot find node,please try again..."; ConsoleU.writeLine(printInfo, ConsoleU.Level.Warning); return; }
            if (dtTemp.Rows == null) { printInfo = "Cannot find node rows,please try again..."; ConsoleU.writeLine(printInfo, ConsoleU.Level.Warning); return; }

            Int32 index = 0;
            foreach (DataRow row in dtTemp.Rows)
            {
                List<String> tempnode = new List<String>();
                if (Int32.Parse(row[PARENT].ToString()) == 0)
                {
                    tempnode.Add(row[CURRENTNMAE].ToString());
                    tempnode.Add(row[CURRENT].ToString());
                    treeView_dir.Nodes.Add(row[CURRENTNMAE].ToString());

                    funcBack(dtTemp.Rows, tempnode[1], tempnode[0], treeView_dir.Nodes[index]);
                    index++;
                }
            }
            treeView_dir.ExpandAll();

        }
        //call back
        void funcBack(DataRowCollection tempDtRows, String nodeID, String nodeText, TreeNode tmpNode)
        {
            if (tempDtRows == null) return;
            Int32 index = 0;
            foreach (DataRow mrow in tempDtRows)
            {
                List<String> tempnode = new List<String>();
                //if(row[PARENT].ToString()==nodeID && nodeTextFunc(nodeID,nodeText))	
                if (mrow[PARENT].ToString() == nodeID)
                {
                    tempnode.Add(mrow[CURRENTNMAE].ToString());
                    tempnode.Add(mrow[CURRENT].ToString());
                    tmpNode.Nodes.Add(mrow[CURRENTNMAE].ToString());
                    funcBack(tempDtRows, tempnode[1], tempnode[0], tmpNode.Nodes[index]);
                    index++;
                }
            }
        }
        #endregion

        #region Treeview Select
        bool sign = false;
        gf_remote mRemote;

        void treeView_dir_AfterSelect(System.Object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
#if print
	ConsoleU.writeLine(String.Format("node selected. . {0}", e.Node.FullPath), ConsoleU.Level.Info);
#endif
            if (e.Node.Text == REMOTE_MENU)
            {
                if (!sign)
                {
                    //if (mCurrentSport == null)
                    //{
                    //    ConsoleU.writeLine("Please select the serial port to enter", ConsoleU.Level.Warning);
                    //    return;
                    //}
                    mRemote = new gf_remote();
                    mRemote.Move += new EventHandler(subFormMove);
                  
                    mRemote.Show();
                    this.AddOwnedForm(mRemote);
                    mDockRemote = new DockU(this);
                    mDockRemote.isEnabled = true;
                    mDockRemote.position = DockU.Position.MiddleRight;
                    mDockRemote.process(mRemote);
                }
                else if (sign)
                    mRemote.Close();
                sign = !sign;
            }
            else if (e.Node.Text == SYS_TIME)
            {
                /*panel2.Visible=false;*/
                dataGridView1.Visible = true;

            }

            //foreach (TreeNode tr in treeView_dir.Nodes[0].Nodes)
            //{
            //    if (tr.Text == REMOTE_MENU)
            //    {

            //        panel2.Visible = true;
            //        dataGridView1.Visible = false;
            //        break;
            //    }
            //    else if (tr.Text == SYS_TIME)
            //    {
            //        /*panel2.Visible=false;*/
            //        dataGridView1.Visible = true;
            //        break;
            //    }
            //}
        }
        #endregion

        #region dock the window
        private void subFormMove(object sender, EventArgs e)//
        {
            if (ActiveForm == sender)
            {
                if (ActiveForm.Text == REMOTE_MENU)
                {
                    mDockRemote.isDocked = false;
                }
            }
        }
        private void gf_main_Move(object sender, EventArgs e)
        {
            if (mRemote != null)
            {
                mDockRemote.process(mRemote);
            }

        }
        #endregion

        #region serial click and received data
        private void serialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem serial_Sport = (ToolStripMenuItem)sender;
            if (mCurrentSport==null || mCurrentSport.PortName != serial_Sport.Text)// select the serial port
            {
                cc_serial  sport = new cc_serial(serial_Sport.Text);
                sport.slot.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(dataReceived);
                serial_Sport.Checked = true;
                mCurrentSport = cc_serial.mSpSlot;
            }
            else //close the serial port
            {
                foreach (ToolStripMenuItem mSerial in serialToolStripMenuItem.DropDownItems)
                {
                    if (mSerial.Text == serial_Sport.Text)
                    {
                        ConsoleU.writeLine(String.Format("close [{0}] ok", cc_serial.mName), ConsoleU.Level.Info);
                       try
                       {
                           cc_serial.mName = null;
                           cc_serial.mSpSlot = null;
                           mCurrentSport.Close();
                           mCurrentSport = null;
                           serial_Sport.Checked = false;

                       }
                       catch (System.Exception ex)
                       {
                           ConsoleU.writeLine(String.Format("close [{0}] failed,cause to:{1}", cc_serial.mName,ex.ToString()), ConsoleU.Level.Error);
                       }
                    }
                }
            }

        }
        void dataReceived(System.Object sender, System.IO.Ports.SerialDataReceivedEventArgs e) //received data
        {
            System.IO.Ports.SerialPort sp = (System.IO.Ports.SerialPort)sender;
            string indata = sp.ReadExisting();
            ConsoleU.writeLine(string.Format("Data Received: {0}", indata), ConsoleU.Level.Info);
        }

        #endregion

        #region open debug console
        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            menuItem.Checked = !menuItem.Checked;
            if (menuItem.Checked)
            {
                ConsoleU.createConsole();
            }
            else
            {
                ConsoleU.releaseConsole();
            }
        }
        #endregion

        #region initialization interface //  [9/23/2012 Administrator]
        private void gf_main_Paint(object sender, PaintEventArgs e)
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
        private void treeView_dir_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left & this.WindowState == FormWindowState.Normal)
            {
                // 移动窗体
                this.treeView_dir.Capture = false;
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void statusStrip1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left & this.WindowState == FormWindowState.Normal)
            {
                // 移动窗体
                this.Capture = false;
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        #endregion

        #region display console
        private void gf_main_Shown(object sender, EventArgs e)
        {
            ConsoleU.show();
        }
        #endregion

        #region close the program button
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

    }
}
