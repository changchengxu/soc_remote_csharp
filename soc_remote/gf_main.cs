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
using soc_remote.control;


namespace soc_remote
{
    public partial class gf_main : Form
    {
        const string VERSION = "1.0";
        public const string FILEPATH = "config";         //read the xml folder
        public const string TREENAME = "remote.xml";     //read the xml file about the tree 
        public const string CTRLNAME = "controlFile.xml";//read the xml file about the controls

        public const string DIRTREE = "dirtree";        //read the xml file node
        public const string DIRCTRL = "control";        //read the xml file node
        public const string DIREVENT = "event";         //read the xml file event node

        public const string CURRENT = "current";        //binding tree node...
        public const string PARENT = "parent";
        public const string CURRENTNMAE = "currentname";
        public const string VMPU = "VMPU";          //tree parent node
        public const string REMOTE = "Remote";        //tree parent node

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
        cc_controls mctrls;

        public gf_main(string[] args)
        {
            InitializeComponent();

#if printf
            Text = String.Format("{0} v{1} < build {2} >", Text, VERSION, DateTime.Now.ToString());
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
            mctrls = new cc_controls();//initialization of controls object

            DockU mDockTuner = new DockU(this);
            mDockTuner.isEnabled = true;

            debugToolStripMenuItem.Checked = true;
        }


        private void gf_main_Shown(object sender, EventArgs e)
        {
            ConsoleU.show();
        }

        #region main load
        private void gf_main_Load(object sender, EventArgs e)
        {
            loadRemoteTree();

            dir = String.Format("{0}/{1}/{2}",
                System.IO.Directory.GetCurrentDirectory(), FILEPATH, CTRLNAME);
            DataSet ds = new DataSet();

            mctrls.loadDataSet(dir, ds);//init Controls DataSet
            dataGridView1.DataSource = ds.Tables[DIREVENT];//init dataGridView 

            loadCtrls();//init control
        }
        #endregion

        #region load Tree
        void loadRemoteTree()
        {
            treeView_dir.Nodes.Clear();
            DataTable dtTemp = new DataTable();

            String printInfo = "bind tree node... ...";
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

        #region Treeview Select // [9/20/2012 Administrator]
        void treeView_dir_AfterSelect(System.Object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
#if print
	ConsoleU.writeLine(String.Format("node selected. . {0}", e.Node.FullPath), ConsoleU.Level.Info);
#endif
            if (e.Node.Text == VMPU)
            {

                panel2.Visible = true;
                dataGridView1.Visible = false;
            }
            else if (e.Node.Text == REMOTE)
            {
                /*panel2.Visible=false;*/
                dataGridView1.Visible = true;
            }
        }
        #endregion

        #region loadCtrl // [9/20/2012 Administrator]
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
                        this.panel2.Controls.Add(button);

                        button.Location = new System.Drawing.Point(ctrl.Ctr_X, ctrl.Ctr_Y);
                        button.Size = new System.Drawing.Size(ctrl.Ctr_Width, ctrl.Ctr_Height);
                        button.UseVisualStyleBackColor = true;
                        button.Click += new System.EventHandler(button_Click);
                        button.Enter += new System.EventHandler(button_Enter);

                        String color = "0,1,2,3,4,5,6,7,8,9,UP,DOWN,LEFT,RIGHT"; //set controls color 
                        foreach (String tempColor in color.Split(','))
                        {
                            if (ctrl.Ctr_Text == tempColor) { button.BackColor = System.Drawing.Color.Gray; }
                        }
                        if (ctrl.Ctr_Text == "RED")   { button.BackColor = System.Drawing.Color.Red; }
                        if (ctrl.Ctr_Text == "GREEN") { button.BackColor = System.Drawing.Color.Green; }
                        if (ctrl.Ctr_Text == "YELLOW"){ button.BackColor = System.Drawing.Color.Yellow; }
                        if (ctrl.Ctr_Text == "BLUE")  { button.BackColor = System.Drawing.Color.Blue; }
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

        #region button click
        private void button_Click(object sender, EventArgs e)
        {
            Button btn_Click = (Button)sender;
            for (int i=0; i < cc_control.dtEvent.Rows.Count; i++)
            {
                if (cc_control.dtEvent.Rows[i][CTRLEVENTNAME].ToString() == btn_Click.Text)
                {
                    ConsoleU.writeLine(String.Format("you click: {0} button \"command line:{1}\"", btn_Click.Text, cc_control.dtEvent.Rows[i][CTRLLINE].ToString()), ConsoleU.Level.Info);
                }
            }

        }
        #endregion

        private void button_Enter(object sender, EventArgs e)
        {

        }

    }
}
