using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace soc_remote.control
{
   public class cc_event
    {

        public String mCtr_Event;
        public String mCtrl_Line;
        List<cc_event> mEventCollection  = new List<cc_event>();
        cc_event mevent;
        public List<cc_event> eventItems
        {
            get
            {
                return mEventCollection;
            }
        }
       public cc_event()
        {
           
        }

       #region  binding event for  controls
       public Int32 addEvent(DataRow dr, DataTable dt)
        {
            if (dt != null)
            {
                DataRelationCollection relations = dt.ChildRelations;
                if (relations.Count > 0)
                {
                    DataRelation relationAddress = relations[0];
                    DataRow[] rowsAddress = dr.GetChildRows(relationAddress);
                    foreach (DataRow mRow in rowsAddress)
                    {
                        try
                        {
                            mevent = new cc_event();
                            mevent.mCtr_Event = mRow[gf_main.CTRLEVENT].ToString();
                            mevent.mCtrl_Line = mRow[gf_main.CTRLLINE].ToString();

                            mEventCollection.Add(mevent);
                        }
                        catch (FormatException)
                        {
                        }
                    }
                }
                else 
                {
                    try
                    {
                        mevent = new cc_event();
                        mevent.mCtr_Event = dr[gf_main.CTRLTYPE].ToString();
                        mevent.mCtrl_Line = dr[gf_main.CTRLLINE].ToString();

                        mEventCollection.Add(mevent);
                    }
                    catch (FormatException)
                    {
                    }
                }
            }
            return 0;
        }
       #endregion
    }
}
