using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using GSoft.Utility;
using System.IO;
using System.Data;

namespace soc_remote.serialport
{
    public class cc_serial
    {
        public const string TABLE_NAME	=	"root";    //serial port parameter... ...
        public const string DATA_BITS	=	"data_bits";
        public const string PARITY		=	"parity";
        public const string STOP_BIT	=	"stop_bit";
        public const string BAUD_RATE	=	"baud_rate";
        public const string FLOW_CONTROL=	"flow_control";
        public const string READ_TIMEOUT = "read_timeout";

        string mConfigFile;
	  public static SerialPort mSpSlot;
     public  static String mName;
		Int32 mBaudRate;
		Parity mParity;
		Int32 mDataBits;
		StopBits mStopBit;
		Int32 mReadTimeout;

        public cc_serial(string name)
        {
            Int32 errCode = 0;
            //if (name == null)
            //{
            //    return;
            //}

            mName = name;
            ConsoleU.writeLine(String.Format("serial port: \"{0}\"", name),
                ConsoleU.Level.Info);

            mConfigFile = String.Format("{0}/{1}/{2}",
        System.IO.Directory.GetCurrentDirectory(), soc_remote.gf_main.FILEPATH, soc_remote.gf_main.SERIAL);

            errCode = loadConfig();
            if (errCode < 0)
            {
                return;
            }
            errCode = close();
            errCode = open();
            if (errCode < 0)
            {
                return;
            }
        }

        //this is return slot
       public SerialPort slot {
			 get {
                 if(mSpSlot==null)
                     return null;
				return mSpSlot;
			}
		}
        //String name {
        //     get {
        //        return mName;
        //    }
        //     set {
        //        Int32 errCode = 0;

        //        close(string name);

        //        mName = name;

        //        errCode = open();
        //        if (errCode < 0) {
        //            return;
        //        }
        //    }
        //}
#region open()
       Int32 open()
       {
           //if (mName == String.Empty)
           //{
           //    return -1;
           //}
           if (mSpSlot != null)
           {
               return -2;
           }

           mSpSlot = new SerialPort(mName, mBaudRate, mParity, mDataBits, mStopBit);
           mSpSlot.ReadTimeout = mReadTimeout;

           try
           {
               mSpSlot.Open();
           }
           catch (InvalidOperationException e)
           {
               ConsoleU.writeLine(string.Format("open [{0}] failed, {1}", mName, e.Message),
                   ConsoleU.Level.Error);
               return -1;
           }
           ConsoleU.writeLine(String.Format("open [{0}] ok", mName), ConsoleU.Level.Info);

           return 0;
       }
#endregion

        #region close()
        Int32 close()
        {
            if (mSpSlot == null)
            {
                return -1;
            }
            try
            {
                if (mSpSlot.IsOpen)
                {
                    mSpSlot.Close();
                }
            }
            catch (InvalidOperationException e)
            {
                ConsoleU.writeLine(String.Format("close [{0}] failed, {1}", mName, e.Message),
                    ConsoleU.Level.Error);
                return -1;
            }
            ConsoleU.writeLine(String.Format("close [{0}] ok", mName), ConsoleU.Level.Info);
            mSpSlot = null;
            return 0;
        }
        #endregion

        Int32 loadConfig()
        {
            if (mConfigFile == String.Empty || !File.Exists(mConfigFile))
            {
                ConsoleU.writeLine(String.Format("serial port config file is invalid"),
                    ConsoleU.Level.Error);
                return -1;
            }

            DataSet ds = new DataSet();
            ds.ReadXml(mConfigFile);

            foreach (DataTable dt in ds.Tables)
            {
                if (dt.TableName != TABLE_NAME)
                {
                    continue;
                }

                foreach (DataRow row in dt.Rows)
                {
                    bool result;
                    String value = row[DATA_BITS].ToString();

                    result = Int32.TryParse(value,out mDataBits);
                    if (!result)
                    {
                        ConsoleU.writeLine(String.Format("convert \"{0}\" to int32 failed", value),
                            ConsoleU.Level.Error);
                    }

                    mParity = (Parity)Enum.Parse(typeof(Parity), row[PARITY].ToString(), true);

                    mStopBit = (StopBits)Enum.Parse(typeof(StopBits), row[STOP_BIT].ToString(), true);

                    result = Int32.TryParse(row[BAUD_RATE].ToString(),out mBaudRate);
                    if (!result)
                    {
                        ConsoleU.writeLine(String.Format("convert \"{0}\" to int32 failed", value),
                            ConsoleU.Level.Error);
                    }

                    result = Int32.TryParse(row[READ_TIMEOUT].ToString(),out mReadTimeout);
                    if (!result)
                    {
                        ConsoleU.writeLine(String.Format("convert \"{0}\" to int32 failed", value),
                            ConsoleU.Level.Error);
                    }
                }
            }
            ConsoleU.writeLine(String.Format("serial port config: \n dataBits({0}) \n parity({1}) \n stopBit({2}) \n baudRate({3}) \n readTimeout({4})", 
		mDataBits, mParity, mStopBit, mBaudRate, mReadTimeout), ConsoleU.Level.Info);

	return 0;
        }
    }
}
