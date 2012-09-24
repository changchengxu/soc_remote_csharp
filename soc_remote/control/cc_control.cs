#define  ctrlPrintf
#undef   ctrlPrintf

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GSoft.Utility;
using System.Data;


namespace soc_remote.control
{
    public class cc_control
    {
        public Int32 Ctr_X;
        public Int32 Ctr_Y;
        public Int32 Ctr_Width;
        public Int32 Ctr_Height;
        public String Ctr_Type;
        public String Ctr_Text;
        //String Ctr_Event;
        //String Ctrl_Line;
        public cc_event mevent;
static  public DataTable dtEvent;//event DataTable
        public cc_control()
        {
            mevent = new cc_event();
        }
    }
    public class cc_controls
    {
        List<cc_control> mcontrolItems;


        public cc_controls()
        {
            mcontrolItems = new List<cc_control>();
        }

        public List<cc_control> ctrlsItem
        {
            get
            {
                return mcontrolItems;
            }
        }

        public Int32 ctrlCount
        {
            get
            {
                if (mcontrolItems == null) return -1;
                return mcontrolItems.Count;
            }
        }

        #region load  controls and binding event 
        Int32 load(String fileName)
        {
            DataTable dtTemp = new DataTable();
            mcontrolItems.Clear();

            String printInfo = "binding control... ...";
            ConsoleU.writeLine(printInfo, ConsoleU.Level.Warning);

            DataSet ds = new DataSet();
            String fileExtension = System.IO.Path.GetExtension(fileName);
            if (fileExtension != ".xml" || !System.IO.File.Exists(fileName))
            {
                ConsoleU.writeLine("this file is not xml or path error,please select again!", ConsoleU.Level.Warning);
            }
            ds.ReadXml(fileName);
            dtTemp = ds.Tables[gf_main.DIRCTRL];

            DataSet dsEvent = new DataSet();//load  xml about event DataSet
            dsEvent.ReadXml(fileName);

            if (dtTemp == null) { printInfo = "Cannot find node,please try again..."; ConsoleU.writeLine(printInfo, ConsoleU.Level.Warning); return -1; }
            if (dtTemp.Rows == null) { printInfo = "Cannot find node rows,please try again..."; ConsoleU.writeLine(printInfo, ConsoleU.Level.Warning); return -1; }

            foreach (DataRow row in dtTemp.Rows)
            {
                cc_control ctrl = new cc_control();
                try
                { ctrl.Ctr_Text = row[gf_main.CTRLTEXT].ToString(); }
                catch (FormatException) { }
                try
                { ctrl.Ctr_X = Int32.Parse(row[gf_main.CTRLX].ToString()); }
                catch (FormatException) { }
                try
                { ctrl.Ctr_Y = Int32.Parse(row[gf_main.CTRLY].ToString()); }
                catch (FormatException) { }
                try
                { ctrl.Ctr_Width = Int32.Parse(row[gf_main.CTRLWIDTH].ToString()); }
                catch (FormatException) { }
                try
                { ctrl.Ctr_Height = Int32.Parse(row[gf_main.CTRLHEIGHT].ToString()); }
                catch (FormatException) { }
                try
                { ctrl.Ctr_Type = row[gf_main.CTRLTYPE].ToString(); }
                catch (FormatException) { }


                DataRelationCollection childRelations = dtTemp.ChildRelations;//relationship building
                if (childRelations.Count > 0)
                {
                    DataRelation relation = childRelations[String.Format("{0}_{1}", gf_main.DIRCTRL, gf_main.DIREVENT)];
                    DataRow[] ruleRows = row.GetChildRows(relation);
                    foreach (DataRow ruleRow in ruleRows)
                    {
                        ctrl.mevent.addEvent(ruleRow, dsEvent.Tables[gf_main.DIREVENT]);//init ion controls event
                    }
                }

                mcontrolItems.Add(ctrl);
            }
#if ctrlPrintf
            ConsoleU.writeLine(String.Format("control loaded. . {0}", ctrlCount), ConsoleU.Level.Normal);
#endif
            return 0;
        }
        #endregion

        #region load DataSet and To Value
        public Int32 loadDataSet(String dir, DataSet ds)
        {
            load(dir);
            createDataSet(ds);
            DataTable dtCtrol = ds.Tables[gf_main.DIRCTRL];
            cc_control.dtEvent = ds.Tables[gf_main.DIREVENT];
            foreach (cc_control ctrl in ctrlsItem)
            {
                DataRow dr = dtCtrol.NewRow();
                dr[gf_main.CTRLTEXT] = ctrl.Ctr_Text;
                dr[gf_main.CTRLX] = ctrl.Ctr_X;
                dr[gf_main.CTRLY] = ctrl.Ctr_Y;
                dr[gf_main.CTRLWIDTH] = ctrl.Ctr_Width;
                dr[gf_main.CTRLHEIGHT] = ctrl.Ctr_Height;
                dr[gf_main.CTRLTYPE] = ctrl.Ctr_Type;
                dtCtrol.Rows.Add(dr);
                foreach (cc_event locEvent in ctrl.mevent.eventItems)
                {
                    DataRow drEvent = cc_control.dtEvent.NewRow();
                    drEvent[gf_main.CTRLEVENTNAME] = ctrl.Ctr_Text;
                    drEvent[gf_main.CTRLEVENT] = locEvent.mCtr_Event;
                    drEvent[gf_main.CTRLLINE] = locEvent.mCtrl_Line;
                    cc_control.dtEvent.Rows.Add(drEvent);
                }
            }
            return 0;
        }
        #endregion

        #region create dataSet
        Int32 createDataSet(DataSet ds)
        {
            //init control
            DataTable dtCtrol = new DataTable(gf_main.DIRCTRL);
            Object[,] columnsCtrol = new Object[,]{
            {gf_main. CTRLTEXT,	System.Type.GetType("System.String")},
            {gf_main. CTRLX,	System.Type.GetType("System.Int32")},
            {gf_main. CTRLY,	System.Type.GetType("System.Int32")},
            {gf_main. CTRLWIDTH,System.Type.GetType("System.Int32")},
            {gf_main. CTRLHEIGHT,System.Type.GetType("System.Int32")},
            {gf_main. CTRLTYPE,	System.Type.GetType("System.String")},
            };

            Table.makeDataTable(dtCtrol, columnsCtrol);
            ds.Tables.Add(dtCtrol);

            //init control event
            DataTable dtEvent = new DataTable(gf_main.DIREVENT);
            Object[,] columnsEvent = new Object[,] {
            {gf_main. CTRLEVENTNAME,System.Type.GetType("System.String")},
            {gf_main. CTRLEVENT,	System.Type.GetType("System.String")},
            {gf_main. CTRLLINE,		System.Type.GetType("System.String")},
            };

            Table.makeDataTable(dtEvent, columnsEvent);
            ds.Tables.Add(dtEvent);

            // create relation
            DataRelation drPageReg = new DataRelation("", dtCtrol.Columns[gf_main.CTRLTEXT],
                dtEvent.Columns[gf_main.CTRLEVENTNAME]);
            drPageReg.Nested = true;
            ds.Relations.Add(drPageReg);
            return 0;
        }
        #endregion
    }
}
