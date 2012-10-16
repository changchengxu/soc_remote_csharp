using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace soc_remote
{
    public partial class gf_serialize : Form
    {
        #region //in order to move the form
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);//in order to move the form   
        #endregion
        gf_main main;     //Create an object in order to bind the data to the main dgv 
        string msign = "";//Judgment is to add or modify
        int cursorPos;    //Get the cursor position in the editing

        public gf_serialize(gf_main mainForm, string sign, string mdgvRowConent)//Transmission dgv data
        {
            InitializeComponent();

            main = mainForm;

            msign = sign;
            if (sign == "edit")
            {
                richtxt_command.Text = mdgvRowConent;
                richtxt_command.SelectionStart = richtxt_command.Text.Length;// Position the cursor to the last
                cursorPos = richtxt_command.SelectionStart;
            }
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
                if (richtxt_command.Text.Trim() == "")
                {
                    MessageBox.Show("Can not save empty please try again", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (msign == "add")
                {

                    main.bindSerializeDGV(richtxt_command.Text.Trim(), "add");
                }
                else if (msign == "edit")
                {
                    main.bindSerializeDGV(richtxt_command.Text.Trim(), "edit");
                }
                main.Enabled = true;
                gf_main.serializeSigh = "";
                this.Close();
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            if (msign == "add")
            {
                main.bindSerializeDGV("", "add");
            }
            else if (msign == "edit")
            {
                main.bindSerializeDGV("", "edit");
            }
            main.Enabled = true;
            gf_main.serializeSigh = "";
            this.Close();
        }

        #region remoteEventReceive
        public void remoteEventReceive(string remoteButton)
        {
            if (msign == "add")
            {
                if (richtxt_command.Text.Trim() == "")
                {
                    richtxt_command.Text = remoteButton;
                }
                else
                {
                    richtxt_command.Text += "," + remoteButton;
                }
            }
            else if (msign == "edit")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(richtxt_command.Text.Trim());
                if (cursorPos == richtxt_command.Text.Length)
                {
                    sb.Insert(cursorPos, ","+remoteButton);
                }
                else
                {
                    sb.Insert(cursorPos, remoteButton + ",");
                }
                richtxt_command.Text = sb.ToString();
                cursorPos += ((remoteButton + ",").Length);
            }
        }
        #endregion

        #region Click the mouse to move the form
        private void gf_serialize_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left & this.WindowState == FormWindowState.Normal)
            {
                // 移动窗体
                this.Capture = false;
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }

        }
        #endregion

        private void richtxt_command_MouseDown(object sender, MouseEventArgs e)
        {
            cursorPos = richtxt_command.GetCharIndexFromPosition(e.Location);
            // or   cursorPos = richtxt_command.SelectionStart;
        }

        private void richtxt_command_KeyUp(object sender, KeyEventArgs e)
        {
            cursorPos = richtxt_command.SelectionStart;
        }

    }
}
