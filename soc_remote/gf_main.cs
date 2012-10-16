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
        public const string SRANDOM = "serialize.xml"; //read the xml file about the serialize random

        public const string DIRTREE = "dirtree";        //read the tree    xml file node
        public const string DIRCTRL = "control";        //read the control xml file node
        public const string DIREVENT = "event";         //read the control xml file event node
        public const string DIRRANDOM = "root";    //read the random  xml file 

        public const string CURRENT = "current";        //binding tree node...
        public const string PARENT = "parent";
        public const string CURRENTNMAE = "currentname";
        public const string REMOTE_MENU = "remote menu";  //tree parent node
        public const string SYS_TIME = "sys time";        //tree parent node
        public const string TEST = "test";                //tree parent node

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
        DockU mDockRemote;//Create a subsidiary form
        SocSerialPort.Uart mSpSlot;// create serial port object 

        #region //in order to move the form
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);//in order to move the form   
        #endregion

        public static string  serializeSigh = "";//为了区分是命令还是添加修改序列化
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

            //load serial port
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

            //set color
            treeView_dir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(235)))), ((int)(((byte)(252)))));
            this.BackColor = Color.Aqua;
            panel1.BackColor = panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(235)))), ((int)(((byte)(252)))));
            menuStrip1.BackColor = statusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(235)))), ((int)(((byte)(252)))));
             foreach (ToolStripMenuItem menItem in menuStrip1.Items)
            {
                foreach (ToolStripItem mMenItem in menItem.DropDownItems)
                {
                    mMenItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(235)))), ((int)(((byte)(252)))));
                }
            }

        }
        #endregion

        #region load Tree
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
                if (Int32.Parse(row[PARENT].ToString()) == 0)
                {
                       treeView_dir.Nodes.Add(row[CURRENT].ToString(), row[CURRENTNMAE].ToString());
                       AddTreeNode(dtTemp.Rows, row[CURRENT].ToString(), treeView_dir.Nodes[index++]);
                }
            }
            treeView_dir.Nodes[0].Expand();
            treeView_dir.Nodes[0].ImageIndex = 0;
            treeView_dir.SelectedNode = null;
            
         }
         ////call back
         private void AddTreeNode(DataRowCollection tempDtRows, string RootNodeID, TreeNode tmpNode)
         {
             Int32 index = 0;
             
             if (tempDtRows == null) return;
             foreach (DataRow mrow in tempDtRows)
             {
                 if (mrow[PARENT].ToString() == RootNodeID)
                 {
                     tmpNode.Nodes.Add( mrow[CURRENT].ToString(), mrow[CURRENTNMAE].ToString());
                     AddTreeNode(tempDtRows, mrow[CURRENT].ToString(), tmpNode.Nodes[index]);
                     index++;
                 }
             }
         }
        #endregion

        #region Treeview Select
         gf_remote mRemote;// create remote form object
         string sign = "off";

         private void treeView_dir_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
         {
#if print
	ConsoleU.writeLine(String.Format("node selected. . {0}", e.Node.FullPath), ConsoleU.Level.Info);
#endif
             if (e.Node.Text == REMOTE_MENU)
             {
                 dataGridView();//load dsRandomXml from xml and Bound to dgb
                 groupbox();

                 if (sign == "on")
                 {
                     dataGridView1.Visible = true;
                     groupBox1.Visible = true;
                 }
                 else if (sign == "off")
                 {
                     if (mSpSlot != null)
                     {
                         dataGridView1.Visible = true;
                         groupBox1.Visible = true;

                         mRemote = new gf_remote(mSpSlot);// open remote form
                         mRemote.Owner = this;
                         mRemote.Move += new EventHandler(subFormMove);
                         mRemote.mMainFormTrans += new mainFormTrans(remoteEventReceive);//this is a delegate in order to  close subForm
                         mRemote.Show();
                         sign = "on";
                         this.AddOwnedForm(mRemote);
                         mDockRemote = new DockU(this);//DockU
                         mDockRemote.isEnabled = true;
                         mDockRemote.position = DockU.Position.MiddleRight;
                         mDockRemote.process(mRemote);
                         serialToolStripMenuItem.Enabled = false;

                     }
                     else
                     {
                         ConsoleU.writeLine(String.Format("No choice of serial"), ConsoleU.Level.Error);
                         treeView_dir.SelectedNode = null;
                         dataGridView1.Visible = false;
                         groupBox1.Visible = false;

                     }
                 }
                 else
                 {
                     dataGridView1.Visible = false;
                     groupBox1.Visible = false;
                 }
             }
             else
             {
                 dataGridView1.Visible = false;
                 groupBox1.Visible = false;

             }
         }

         private void treeView_dir_BeforeSelect(object sender, TreeViewCancelEventArgs e)
         {
             if (e.Action == TreeViewAction.Unknown)
                 e.Cancel = true;//treeview gets the focus is not selected by default
         }

        private gf_serialize remoteEventReceive(string msign)//Delegate event from remote frm
        {
            sign = msign;
            if (sign == "off")
            {
                treeView_dir.SelectedNode = null;//close subform then cancel treeView select
                serialToolStripMenuItem.Enabled = true;

                dataGridView1.Visible = false;
                groupBox1.Visible = false;
                serializeForm.Close();
                this.Enabled = true;
            }
            if (sign == "serializeFrm")
            {
                return serializeForm;
            }
            return null;
        }
        #endregion

        #region port click and received data
        private void serialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem serial_Sport = (ToolStripMenuItem)sender;
            if (mSpSlot == null )// select the serial port
            {
                mSpSlot = new SocSerialPort.Uart(serial_Sport.Text);
                //sport.slot.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(dataReceived);
                serial_Sport.Checked = true;
            }
            else if(mSpSlot.name != serial_Sport.Text)
            {
                foreach (ToolStripMenuItem mSerial in serialToolStripMenuItem.DropDownItems)
                {
                    mSerial.Checked = false;
                }
                mSpSlot.close();
                mSpSlot = new SocSerialPort.Uart(serial_Sport.Text);
                serial_Sport.Checked = true;

            }
            else //close the serial port
            {
                foreach (ToolStripMenuItem mSerial in serialToolStripMenuItem.DropDownItems)
                {
                    if (mSerial.Text == serial_Sport.Text)
                    {
                           mSpSlot.close();
                           serial_Sport.Checked = false;
                    }
                }
            }

        }
        //Int32 mBytesRead=0;
        //Byte[] mReadBuffer = new Byte[10];
        void dataReceived(System.Object sender, System.IO.Ports.SerialDataReceivedEventArgs e) //received data
        {
            System.IO.Ports.SerialPort sp = (System.IO.Ports.SerialPort)sender;
            string indata = sp.ReadExisting();

            //if (sp.BytesToRead <= 0)
            //{
            //    ConsoleU.writeLine("empty trigger", ConsoleU.Level.Warning);
            //    return;
            //}
            //mBytesRead += sp.Read(mReadBuffer, mBytesRead, sp.BytesToRead);
            //ConsoleU.writeLine(string.Format("Data Received: {0}", System.Text.Encoding.Default.GetString (mReadBuffer)), ConsoleU.Level.Info);
            ConsoleU.writeLine(string.Format("Data Received: {0}", indata), ConsoleU.Level.Info);
        }

        #endregion

        #region create dataGridView
        public static DataSet dsRandomXml;
        System.Windows.Forms.DataGridView dataGridView1 = new System.Windows.Forms.DataGridView();

        private void dataGridView()
        {
            dir = String.Format("{0}/{1}/{2}",
               System.IO.Directory.GetCurrentDirectory(), gf_main.FILEPATH, gf_main.SRANDOM);
            dsRandomXml = new DataSet();
            dsRandomXml.ReadXml(dir);

            //System.Windows.Forms.DataGridViewTextBoxColumn Column2 = null;
            //System.Windows.Forms.DataGridViewLinkColumn Column4 = null;

            //Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            //Column4 = new System.Windows.Forms.DataGridViewLinkColumn();

            ((System.ComponentModel.ISupportInitialize)(dataGridView1)).BeginInit();
            dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BackgroundColor = System.Drawing.Color.White;
            dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
           // dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
           //Column2,
           //Column4});
            //dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            dataGridView1.GridColor = System.Drawing.Color.Red;
            //dataGridView1.Location = new System.Drawing.Point(123, 22);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowTemplate.Height = 23;
            dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            dataGridView1.Size = new System.Drawing.Size(panel2.Size.Width-5, panel2.Size.Height-55);
            dataGridView1.TabIndex = 0;

            dataGridView1.DataSource = dsRandomXml.Tables[0];
            ((System.ComponentModel.ISupportInitialize)(dataGridView1)).EndInit();
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.RowHeadersVisible = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;
            //所有的列的背景色设定为水色
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.White;

            //单数行为黄色
            //dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.Yellow;


            panel2.Controls.Add(dataGridView1);
            
            //-----------------------------
            ////    System.Windows.Forms.DataGridView mDataGridView;
            ////    mDataGridView = new System.Windows.Forms.DataGridView();
            ////    mDataGridView.Dock = DockStyle.Fill;
            ////    mDataGridView.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView1_EditingControlShowing);
            ////    mDataGridView.OnRegisterValueChanged += new DataGridViewEdit.registerValueChangeHandler(dataGridView_RegisterAdjust);
            ////    mDataGridView.CellEndEdit += new DataGridViewCellEventHandler(dataGridView1_CellEndEdit);
            ////    mDataGridView.CellClick += new DataGridViewCellEventHandler(dataGridView1_CellClick);
            ////    mDataGridView.CellValueChanged += new DataGridViewCellEventHandler(dataGridView1_CellValueChanged);
            ////    mDataGridView.CellMouseEnter += new DataGridViewCellEventHandler(dataGridView1_CellMouseEnter);
            ////    panel2.Controls.Add(mDataGridView);

            ////    DataTable mDataTable = new DataTable();

            ////    // automatically generate the DataGridView columns.
            ////    mDataGridView.AutoGenerateColumns = true;


            ////    // set up the data source.
            ////    mDataGridView.DataSource = mDataTable;


            ////    // automatically resize the visible rows.
            ////    mDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            ////    mDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            ////    // disable edit by click
            ////    //	mDataGridView.EditMode = DataGridViewEditMode.EditProgrammatically;

            ////    // only vertical scroll bar
            ////    mDataGridView.ScrollBars = ScrollBars.Vertical;

            ////    // hide row header
            ////    mDataGridView.RowHeadersVisible = false;

            ////    // forbid add new rows
            ////    mDataGridView.AllowUserToAddRows = false;

            ////    Object[,] columns = new Object[,]{
            //////	Name				Type				Visible
            ////    { COL_NAME,			System.Type.GetType("System.Int32"),		true },
            ////    { COL_VALUE,		System.Type.GetType("System.Int32"),		true },
            ////    { COL_WIDTH,		System.Type.GetType("System.Int32"),		true },
            ////    { COL_R_W,			System.Type.GetType("System.Int32"),		true },
            ////    { COL_REMARK,		System.Type.GetType("System.Int32"),		true },
            ////    { COL_SELECT,		System.Type.GetType("System.Int32"),		true },
            ////    { COL_PAGENAME,		System.Type.GetType("System.String"),		true },
            ////    { COL_INDEX,		System.Type.GetType("System.Int32"),		false },
            ////};

            ////    Table.makeDataTable(mDataGridView, mDataTable, columns);

            ////    foreach (DataGridViewColumn column in mDataGridView.Columns)
            ////    {
            ////        if (column.Name == COL_VALUE)
            ////        {
            ////            mDataGridView.Columns[COL_VALUE].CellTemplate.Style.BackColor = Color.White;
            ////        }
            ////        else
            ////        {
            ////            column.ReadOnly = true;
            ////        }
            ////    }
        }

        #endregion

        #region create groupBox button

        private System.Windows.Forms.GroupBox groupBox1 = new System.Windows.Forms.GroupBox();
        System.Windows.Forms.CheckBox chk_random = new System.Windows.Forms.CheckBox();
        private void groupbox()
        {
            System.Windows.Forms.Button btn_edit = new System.Windows.Forms.Button();
            System.Windows.Forms.Button btn_add = new System.Windows.Forms.Button();
            System.Windows.Forms.Button btn_delete = new System.Windows.Forms.Button();
            System.Windows.Forms.Button btn_save = new System.Windows.Forms.Button();
            System.Windows.Forms.Button btn_start = new System.Windows.Forms.Button();

            // groupBox1
            // 
           groupBox1.Controls.Add(chk_random);
           groupBox1.Controls.Add(btn_add);
           groupBox1.Controls.Add(btn_edit);
           groupBox1.Controls.Add(btn_delete);
           groupBox1.Controls.Add(btn_save);
           groupBox1.Controls.Add(btn_start);
           groupBox1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
           //groupBox1.Location = new System.Drawing.Point(18, 455);
           groupBox1.Location = new System.Drawing.Point(panel2.Location.X-110, panel2.Height- panel2.Location.Y-30);
           groupBox1.Name = "groupBox1";
           groupBox1.Size = new System.Drawing.Size(496, 38);
           groupBox1.TabIndex = 0;
           groupBox1.TabStop = false;
           groupBox1.Text = "operation";

          groupBox1.ResumeLayout(false);
          groupBox1.PerformLayout();

          groupBox1.SuspendLayout();
          // 
          // btn_add
          // 
          btn_add.BackColor = System.Drawing.Color.White;
          btn_add.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
          btn_add.Location = new System.Drawing.Point(14, 12);
          btn_add.Name = "btn_add";
          btn_add.Size = new System.Drawing.Size(60, 23);
          btn_add.TabIndex = 0;
          btn_add.Text = "ADD";
          btn_add.UseVisualStyleBackColor = false;
          btn_add.Click += new System.EventHandler(this.btn_add_Click);
          // 
          // btn_edit
          // 
          btn_edit.BackColor = System.Drawing.Color.White;
          btn_edit.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
          btn_edit.Location = new System.Drawing.Point(74, 12);
          btn_edit.Name = "btn_edit";
          btn_edit.Size = new System.Drawing.Size(60, 23);
          btn_edit.TabIndex = 0;
          btn_edit.Text = "EDIT";
          btn_edit.UseVisualStyleBackColor = false;
          btn_edit.Click += new System.EventHandler(this.btn_edit_Click);
          // 
          // btn_delete
          // 
          btn_delete.BackColor = System.Drawing.Color.White;
          btn_delete.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
          btn_delete.Location = new System.Drawing.Point(134, 12);
          btn_delete.Name = "btn_delete";
          btn_delete.Size = new System.Drawing.Size(60, 23);
          btn_delete.TabIndex = 2;
          btn_delete.Text = "DELETE";
          btn_delete.UseVisualStyleBackColor = false;
          btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
          // 
          // btn_save
          // 
          btn_save.BackColor = System.Drawing.Color.White;
          btn_save.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
          btn_save.Location = new System.Drawing.Point(194, 12);
          btn_save.Name = "btn_edit";
          btn_save.Size = new System.Drawing.Size(60, 23);
          btn_save.TabIndex = 1;
          btn_save.Text = "SAVE";
          btn_save.UseVisualStyleBackColor = false;
          btn_save.Click += new System.EventHandler(this.btn_save_Click);
          // 
          // btn_start
          // 
          btn_start.BackColor = System.Drawing.Color.White;
          btn_start.Location = new System.Drawing.Point(326, 12);
          btn_start.Name = "btn_start";
          btn_start.Size = new System.Drawing.Size(49, 23);
          btn_start.TabIndex = 3;
          btn_start.Text = "START";
          btn_start.UseVisualStyleBackColor = false;
          btn_start.Click += new System.EventHandler(this.btn_start_Click);
            // 
            // chk_random
            // 
          chk_random.AutoSize = true;
          chk_random.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
          chk_random.Location = new System.Drawing.Point(410, 19);
          chk_random.Name = "chk_random";
          chk_random.Size = new System.Drawing.Size(60, 16);
          chk_random.TabIndex = 1;
          chk_random.Text = "random";
          chk_random.UseVisualStyleBackColor = true;
          chk_random.CheckedChanged += new System.EventHandler(this.chk_random_CheckedChanged);
          

            panel2.Controls.Add(groupBox1);
            //groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
        }
        #endregion

        #region dataGridView ADD、DELETE、SAVE、START、CHECK Button Click
        gf_serialize serializeForm;
        private void btn_add_Click(object sender, EventArgs e)//dataGridView1 add a new line 
        {
            serializeSigh = "serialize";

            serializeForm = new gf_serialize(this,"add",null);
            serializeForm.Owner = this;
            serializeForm.Show();

            serializeForm.TopMost = true;
            this.Enabled = false;
        }

        private void btn_edit_Click(object sender, EventArgs e)//dataGridView1 edit the selected line
        {
            serializeSigh = "serialize";

            serializeForm = new gf_serialize(this, "edit", dataGridView1[0, dataGridView1.CurrentCell.RowIndex].Value.ToString());
            serializeForm.Show();

            serializeForm.TopMost = true;
            this.Enabled = false;
        }
      
        private void btn_delete_Click(object sender, EventArgs e)//dataGridView1 delete the selected line
        {
            int index = dataGridView1.CurrentCell.RowIndex;
            dataGridView1.Rows.RemoveAt(index);

        }

        public void bindSerializeDGV(string content, string addOrEditSign)
        {
            if (addOrEditSign == "add" && content != "")
            {
                ((DataTable)dataGridView1.DataSource).Rows.Add(content);//add to the last row 
            }
            else if (addOrEditSign == "edit" && content != "")
            {
                dataGridView1[0, dataGridView1.CurrentCell.RowIndex].Value = content;
            }
            mDockRemote = new DockU(this);//DockU
            mDockRemote.isEnabled = true;
            mDockRemote.position = DockU.Position.MiddleRight;
            mDockRemote.process(mRemote);

            sign = "on";
        }

        private void btn_save_Click(object sender, EventArgs e)//dataGridView1 save 
        {
            if (MessageBox.Show("Are you sure you want to save it?", "info", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (dataGridView1[0, dataGridView1.Rows.Count - 1].Value == null)
                {
                    dataGridView1.AllowUserToAddRows = false;
                }
                XmlU.Write(dsRandomXml, dir);
            }
            else
            {
                dataGridView();//load dsRandomXml from xml and Bound to dgb
            }
        }

        public static bool testBegin = false; //test start signal
        public static bool randomOrNo = false;//judgments random or custom 
        public static Int32 dgvIndex = 0;     //dgv current index
        private void btn_start_Click(object sender, EventArgs e)
        {
            testBegin = !testBegin;

            if (chk_random.Checked)
            {
                randomOrNo = true;
                dgvIndex = 0;
            }
            else if (!chk_random.Checked)
            {
                randomOrNo = false;
                dgvIndex = dataGridView1.CurrentCell.RowIndex;
            }
        }

        private void chk_random_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_random.Checked)
            {
                randomOrNo = true;
                dgvIndex = 0;
            }
            else if (!chk_random.Checked)
            {
                randomOrNo = false;
                dgvIndex = dataGridView1.CurrentCell.RowIndex;
            }
        }

        #endregion

        #region dock the window
        private void subFormMove(object sender, EventArgs e)
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
        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left & this.WindowState == FormWindowState.Normal)
            {
                // 移动窗体
                panel2.Capture = false;
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

        #region close the main form
        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("To exit from this page", pictureBox1, pictureBox1.Width, pictureBox1.Height, 2000);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

    }
}
