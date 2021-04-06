using Laike.Can.CanCmd;   // 引用dll定义

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace CANDriverLayer
{
    public class CanDriver
    {
        private static UInt32 CANDeviceHandle;

        private UInt32 canDviceType = CanCmd.LCMiniPcie_432;
        private string canDeviceName = "LCMiniPcie_432";
        private UInt32 canDeviceChannel = 0;

        public CAN_InitConfig config = new CAN_InitConfig();

        public delegate void InfoEventHandler(string infos);

        public event InfoEventHandler Info;

        private System.Threading.Timer timer1;

        private bool IsCANopen = false;

        private DecodeData decodeData = new DecodeData();

        public DecodeData.JSON JSON;

        private Thread T_CANRecv;
        private bool T_CANRecv_NeedDel = false;

        private List<CAN_DataFrame> CANRecvedData = new List<CAN_DataFrame>();

        //---------------------------------------------------------------------------------------------------

        #region CAN解析数据结构体

        public delegate void TLQX_ReceviedEventHandler(DecodeData.JSON json);

        public event TLQX_ReceviedEventHandler received_TLQX;

        public delegate void LX_ReceviedEventHandler(DecodeData.JSON json);

        public event LX_ReceviedEventHandler received_LX;

        public delegate void GT_ReceviedEventHandler(DecodeData.JSON json);

        public event GT_ReceviedEventHandler received_GT;

        public delegate void DP_ReceviedEventHandler(DecodeData.JSON json);

        public event DP_ReceviedEventHandler received_DP;

        public delegate void JWD_ReceviedEventHandler(DecodeData.JSON json);

        public event JWD_ReceviedEventHandler received_JWD;

        /// <summary>
        /// 获取串口是否为打开状态
        /// </summary>
        /// <returns>bool类型</returns>
        public bool checkPortState()
        {
            return true;
        }

        /// <summary>
        /// 管理JSON接收数据
        /// </summary>
        private class ReceiveJson_inf
        {
            private int count;
            private char[] DATA;
            private bool IsFin;
            private bool IsSta;

            public ReceiveJson_inf(int len)
            {
                DATA = new char[len];
                clean();
            }

            /// <summary>
            /// 清理所有变量
            /// </summary>
            public void clean()
            {
                count = 0;
                IsFin = false;
                IsSta = false;
                for (int i = 0; i < DATA.Length; i++)
                    DATA[i] = '\0';
            }

            /// <summary>
            /// 接收到JSON数据开头
            /// </summary>
            public void JsonStart()
            {
                IsSta = true;
                IsFin = false;
            }

            /// <summary>
            /// 接收到JSON数据结尾
            /// </summary>
            public void JsonFinish()
            {
                IsSta = false;
                IsFin = true;
            }

            /// <summary>
            /// 读取数据缓冲区的数据
            /// </summary>
            /// <returns>char[]类型</returns>
            public char[] JsonData()
            {
                return DATA;
            }

            /// <summary>
            /// 向缓冲区写入数据
            /// </summary>
            /// <param name="offset">数据写入偏置地址</param>
            /// <param name="data">数据</param>
            public void SetJsonData(int offset, char data)
            {
                DATA[offset] = data;
                count++;
            }

            /// <summary>
            /// 设置或读取写入数量,即向缓冲区写入的数据数量
            /// </summary>
            public int GetCount
            {
                get { return count; }
                set { count = value; }
            }

            /// <summary>
            /// 设置或读取JSON数据是否已经开始写入缓冲区
            /// </summary>
            public bool GetIsStart
            {
                get { return IsSta; }
                set { IsSta = value; }
            }

            /// <summary>
            /// 设置或读取JSON数据是否已经写完
            /// </summary>
            public bool GetIsFin
            {
                get { return IsFin; }
                set { IsFin = value; }
            }
        }

        public class TLQX_Data
        {
            private string[] data;
            private string[] name;
            private int len;

            /// <summary>
            /// 构造函数
            /// </summary>
            public TLQX_Data()
            {
                len = 13;
                data = new string[len];
                name = new string[len];
                name[0] = "脱粒滚筒转速";
                name[1] = "脱粒间隙";
                name[2] = "导流板开度";
                name[3] = "风机转速";
                name[4] = "鱼鳞筛开度";
                name[5] = "进风口开度";
                name[6] = "输粮搅龙转速";
                name[7] = "杂余搅龙转速";
                name[8] = "夹带损失";
                name[9] = "清选损失";
                name[10] = "含杂率";
                name[11] = "破碎率";
                name[12] = "籽粒流量";
            }

            /// <summary>
            /// 写入JSON数据
            /// </summary>
            /// <param name="jSON">JSON数据格式</param>
            public void Write(DecodeData.JSON jSON)
            {
                CleanAll();
                data[0] = jSON.TLGT;
                data[1] = jSON.TLJX;
                data[2] = jSON.DLBKD;
                data[3] = jSON.FJZS;
                data[4] = jSON.YLKSD;
                data[5] = jSON.JFKKD;
                data[6] = jSON.SLJLZS;
                data[7] = jSON.ZYJLZS;
                data[8] = jSON.JDSS;
                data[9] = jSON.QXSS;
                data[10] = jSON.HZL;
                data[11] = jSON.PSL;
                data[12] = jSON.LZLL;
            }

            /// <summary>
            /// 获取指定的数据
            /// </summary>
            /// <param name="i">获取指定偏置位置的数据</param>
            /// <returns>string类型数据</returns>
            public string GetValue(int i)
            {
                if (i < len)
                    return data[i];
                return "-";
            }

            /// <summary>
            /// 清理所有变量
            /// </summary>
            private void CleanAll()
            {
                for (int i = 0; i < len; i++)
                    data[i] = "-";
            }

            public string GetName(int i)
            {
                if (i < len)
                    return name[i];
                return "-";
            }

            public int Len
            {
                get { return len; }
            }
        }

        public class LX_Data
        {
            private string[] data;
            private string[] name;
            private int len;

            public LX_Data()
            {
                len = 2;
                data = new string[len];
                name = new string[len];
                name[0] = "卸粮筒液压缸位移";
                name[1] = "卸粮筒旋转角度";
            }

            public void Write(DecodeData.JSON jSON)
            {
                CleanAll();
                data[0] = jSON.XLGWY;
                data[1] = jSON.XLGZJ;
            }

            public string GetValue(int i)
            {
                if (i < len)
                    return data[i];
                return "-";
            }

            private void CleanAll()
            {
                for (int i = 0; i < len; i++)
                    data[i] = "-";
            }

            public string GetName(int i)
            {
                if (i < len)
                    return name[i];
                return "-";
            }

            public int Len
            {
                get { return len; }
            }
        }

        public class GT_Data
        {
            private string[] data;
            private string[] name;
            private int len;

            public GT_Data()
            {
                len = 8;
                data = new string[len];
                name = new string[len];
                name[0] = "割台高度左";
                name[1] = "下割刀高度";
                name[2] = "拨禾轮高度";
                name[3] = "拨禾轮转速";
                name[4] = "主割刀转速";
                name[5] = "输送槽转速";
                name[6] = "割幅宽度";
                name[7] = "割台高度右";
            }

            public void Write(DecodeData.JSON jSON)
            {
                CleanAll();
                data[0] = jSON.GTGD;
                data[1] = jSON.XGD;
                data[2] = jSON.BHLGD;
                data[3] = jSON.BHLZS;
                data[4] = jSON.ZGDZS;
                data[5] = jSON.SSCZS;
                data[6] = jSON.GFKD;
                data[7] = jSON.GTGD_R;
            }

            public string GetValue(int i)
            {
                if (i < len)
                    return data[i];
                return "-";
            }

            private void CleanAll()
            {
                for (int i = 0; i < len; i++)
                    data[i] = "-";
            }

            public string GetName(int i)
            {
                if (i < len)
                    return name[i];
                return "-";
            }

            public int Len
            {
                get { return len; }
            }
        }

        public class DP_Data
        {
            private string[] data;
            private string[] name;
            private int len;

            public DP_Data()
            {
                len = 6;
                data = new string[len];
                name = new string[len];

                name[0] = "横向车身倾角";
                name[1] = "纵向车身倾角";
                name[2] = "底盘左前液压缸位移";
                name[3] = "底盘有前液压缸位移";
                name[4] = "底盘左后液压缸位移";
                name[5] = "底盘右后液压缸位移";
            }

            public void Write(DecodeData.JSON jSON)
            {
                CleanAll();
                data[0] = jSON.HXCS;
                data[1] = jSON.ZXCS;
                data[2] = jSON.DPZQ;
                data[3] = jSON.DPYQ;
                data[4] = jSON.DPZH;
                data[5] = jSON.DPYH;
            }

            public string GetValue(int i)
            {
                if (i < len)
                    return data[i];
                return "-";
            }

            private void CleanAll()
            {
                for (int i = 0; i < len; i++)
                    data[i] = "-";
            }

            public string GetName(int i)
            {
                if (i < len)
                    return name[i];
                return "-";
            }

            public int Len
            {
                get { return len; }
            }
        }

        public class JWD_Data
        {
            private string[] data;
            private string[] name;
            private int len;

            public JWD_Data()
            {
                len = 3;
                data = new string[len];
                name = new string[len];
                name[0] = "经度";
                name[1] = "纬度";
                name[2] = "海拔高度";
            }

            public void Write(DecodeData.JSON jSON)
            {
                CleanAll();
                //data[0] = jSON.JDSS;
                //data[1] = jSON.;
                //data[2] = jSON.High;
            }

            public string GetValue(int i)
            {
                if (i < len)
                    return data[i];
                return "-";
            }

            private void CleanAll()
            {
                for (int i = 0; i < len; i++)
                    data[i] = "-";
            }

            public string GetName(int i)
            {
                if (i < len)
                    return name[i];
                return "-";
            }

            public int Len
            {
                get { return len; }
            }
        }

        private ReceiveJson_inf JsonData = new ReceiveJson_inf(1000);

        /// <summary>
        /// 获取JSON数据
        /// </summary>
        public DecodeData.JSON GetJSON
        {
            get { return decodeData.getjson; }
        }

        #endregion CAN解析数据结构体

        //---------------------------------------------------------------------------------------------------

        unsafe private void timer_rec_Tick(object status)
        {
            if (CANRecvedData.Count == 0)
                return;
            timer1.Change(Timeout.Infinite, Timeout.Infinite);
            int count = 0;
            String str = "";
            CANRecvedData.ForEach(a =>
            {
                count = count + 1;
                str += "[I]接收到数据: ";
                str += "  帧ID:0x" + System.Convert.ToString((Int32)a.uID, 16);
                str += "  数据: ";
                for (int j = 0; j < 8; j++)
                {
                    str += " " + System.Convert.ToString(a.arryData[j], 16) + "\n";
                }
                decodeData.DecodePacket(a);
            });
            CANRecvedData.RemoveRange(0, count);
            received_TLQX?.Invoke(decodeData.getjson);
            received_DP?.Invoke(decodeData.getjson);
            received_GT?.Invoke(decodeData.getjson);
            received_JWD?.Invoke(decodeData.getjson);
            received_LX?.Invoke(decodeData.getjson);
            Info?.Invoke(str);
            timer1.Change(100, 100);
        }

        private static string DecodeErrCode(CAN_ErrorCode ErrCode)
        {
            switch (ErrCode)
            {
                case CAN_ErrorCode.CAN_E_NOERROR: return "没有发现错误";
                case (CAN_ErrorCode)0x0001: return "CAN控制器内部FIFO溢出";
                case (CAN_ErrorCode)0x0002: return "CAN控制器错误报警";
                case (CAN_ErrorCode)0x0004: return "CAN控制器消极错误";
                case (CAN_ErrorCode)0x0008: return "CAN控制器仲裁丢失";
                case (CAN_ErrorCode)0x0010: return "CAN控制器总线错误";

                case (CAN_ErrorCode)0x0100: return "设备已经打开";
                case (CAN_ErrorCode)0x0200: return "打开设备错误";
                case (CAN_ErrorCode)0x0400: return "设备没有打开";
                case (CAN_ErrorCode)0x0800: return "缓冲区溢出";
                case (CAN_ErrorCode)0x1000: return "此设备不存在";
                case (CAN_ErrorCode)0x2000: return "装载动态库失败";
                case (CAN_ErrorCode)0x4000: return "执行命令失败错误码";
                case (CAN_ErrorCode)0x8000: return "内存不足";
                default: return "Unknow ERR";
            }
        }

        /// <summary>
        /// build function
        /// </summary>
        public CanDriver(uint _channel = 1)
        {
            timer1 = new System.Threading.Timer(new System.Threading.TimerCallback(timer_rec_Tick), this, 100, 500);
            config.dwAccCode = System.Convert.ToUInt32("0x00000000", 16);
            config.dwAccMask = System.Convert.ToUInt32("0xFFFFFFFF", 16);
            config.nBtrType = 1;   // 位定时参数模式(1表示SJA1000,0表示LPC21XX)
            // 1M ： dwBtr0=00 dwBtr1=0x14 Other: 0014 -1M 0016-800K 001C-500K 011C-250K 031C-125K
            // 041C-100K 091C-50K 181C-20K 311C-10K BFFF-5K
            config.dwBtr0 = System.Convert.ToByte("0x01", 16);
            config.dwBtr1 = System.Convert.ToByte("0x1C", 16);
            config.nFilter = 0;
            config.bMode = 0;

            //OpenCAN();
            //OpenChannel(_channel);

            T_CANRecv = new Thread(TRecv);
            T_CANRecv.Name = "CAN_Recvive_Thread";
            T_CANRecv.Start();
        }

        private unsafe void TRecv()
        {
            while (!T_CANRecv_NeedDel)
            {
                if (IsCANopen)
                {
                    //check is have data
                    uint res = CanCmd.CAN_GetReceiveCount(CANDeviceHandle, canDeviceChannel);
                    if (res == 0)
                    {
                        return;//not receive data
                    }

                    /////////////////////////////////////
                    UInt32 con_maxlen = 50;
                    IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CAN_DataFrame)) * (Int32)con_maxlen);

                    res = CanCmd.CAN_ChannelReceive(CANDeviceHandle, canDeviceChannel, pt, con_maxlen, 100);
                    ////////////////////////////////////////////////////////
                    if (res == 0)
                    {
                        // 读取错误信息
                        CAN_ErrorInformation err = new CAN_ErrorInformation();
                        // 必须调用此函数
                        if (CanCmd.CAN_GetErrorInfo(CANDeviceHandle, canDeviceChannel, ref err) == CanCmd.CAN_RESULT_OK)
                        {  // CAN通讯有错误
                           // 处理错误信息
                            Info?.Invoke("[E]CAN通讯有错误: " + DecodeErrCode(err.uErrorCode));
                        }
                        else
                        {  // 没有收到CAN数据
                            Info?.Invoke("[E]没有收到CAN数据,获取错误信息失败");
                        }
                    }
                    else
                    {
                        for (UInt32 i = 0; i < res; i++)
                        {
                            CAN_DataFrame obj = (CAN_DataFrame)Marshal.PtrToStructure((IntPtr)((UInt64)pt + (UInt64)(i * Marshal.SizeOf(typeof(CAN_DataFrame)))), typeof(CAN_DataFrame));
                            CANRecvedData.Add(obj);
                        }
                    }
                    Marshal.FreeHGlobal(pt);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="CanDeceivePort"></param>
        /// <returns></returns>
        private void OpenCAN(UInt32 CanDeceivePort = 0)
        {
            char t_char = '0';
            CANDeviceHandle = CanCmd.CAN_DeviceOpen(canDviceType, CanDeceivePort, ref t_char);
            if (CANDeviceHandle == 0)
            {
                Info?.Invoke("[E]打开" + canDeviceName + "设备失败");
                return;
            }
            Info?.Invoke("[I]打开" + canDeviceName + "设备成功");
        }

        /// <summary>
        /// </summary>
        /// <param name="channel"></param>
        private void OpenChannel(UInt32 channel)
        {
            canDeviceChannel = channel;
            if (CANDeviceHandle != 0)
            {
                if (CanCmd.CAN_RESULT_OK != CanCmd.CAN_ChannelStart(CANDeviceHandle, canDeviceChannel, ref config))
                {
                    Info?.Invoke("[E]打开" + canDeviceName + "设备的通道" + canDeviceChannel.ToString() + "失败");
                    return;
                }
                Info?.Invoke("[I]打开" + canDeviceName + "设备的通道" + canDeviceChannel.ToString() + "成功");
                IsCANopen = true;
            }
            else
                Info?.Invoke("[E]没有打开设备之前不可操作CAN通道");
        }

        /// <summary>
        /// close the can device
        /// </summary>
        public void CloseCAN()
        {
            try
            {
                T_CANRecv_NeedDel = true;
                T_CANRecv.Abort();
            }
            catch (Exception ex)
            {
                Info?.Invoke("[I]关闭线程" + T_CANRecv.Name + "：" + ex.ToString());
            }

            if (CanCmd.CAN_RESULT_OK != CanCmd.CAN_ChannelStop(CANDeviceHandle, canDeviceChannel))
            {
                Info?.Invoke("[E]关闭" + canDeviceName + "设备的通道" + canDeviceChannel.ToString() + "失败");
            }
            Info?.Invoke("[I]已关闭" + canDeviceName + "设备的通道" + canDeviceChannel.ToString());

            CanCmd.CAN_DeviceClose(CANDeviceHandle);
            IsCANopen = false;
            Info?.Invoke("[I]已关闭" + canDeviceName + "设备");
        }

        public void OpenPort(UInt32 _CanDeceivePort = 0, UInt32 _channel = 1)
        {
            OpenCAN(_CanDeceivePort);
            OpenChannel(_channel);
        }

        private struct SendCMD
        {
            public uint ID;
            public string Name;
            public int Value;
        }

        private uint CheckID(string name)
        {
            var _name = name.Substring(0, 2);
            switch (_name)
            {
                case "CL":
                case "GT": return 0X0CFF5130;
                case "GF": return 0X18FF5230;
                case "SS": return 0X18FF5330;
                case "TL": return 0X18FF5430;
                case "SJ": return 0X18FF5530;
                case "ZY": return 0X18FF5630;
                case "SD":
                case "QZ": return 0X18FF5730;
                case "HZ":
                case "PS": return 0x0CFF2615;
                case "ZX": return 0X0CFF2717;
            }
            return 0;
        }

        private unsafe void OganizeData(List<SendCMD> cMDs, ref CAN_DataFrame cAN_Data)
        {
            for (int i = 0; i < cMDs.Count; i++)
            {
                var cmd = cMDs[i];
                switch (cmd.Name)
                {
                    case "GT_PER":
                        cAN_Data.arryData[0] = (byte)cmd.Value;
                        cAN_Data.arryData[1] = (byte)(cmd.Value >> 8); break;
                    case "GT_MAX":
                        cAN_Data.arryData[2] = (byte)cmd.Value;
                        cAN_Data.arryData[3] = (byte)(cmd.Value >> 8); break;
                    case "GT_MIN":
                        cAN_Data.arryData[4] = (byte)cmd.Value;
                        cAN_Data.arryData[5] = (byte)(cmd.Value >> 8); break;

                    case "GF_PER":
                        cAN_Data.arryData[0] = (byte)cmd.Value;
                        cAN_Data.arryData[1] = (byte)(cmd.Value >> 8); break;
                    case "GF_MAX":
                        cAN_Data.arryData[2] = (byte)cmd.Value;
                        cAN_Data.arryData[3] = (byte)(cmd.Value >> 8); break;
                    case "GF_MIN":
                        cAN_Data.arryData[4] = (byte)cmd.Value;
                        cAN_Data.arryData[5] = (byte)(cmd.Value >> 8); break;

                    case "SD_MAX":
                        cAN_Data.arryData[0] = (byte)cmd.Value;
                        cAN_Data.arryData[1] = (byte)(cmd.Value >> 8); break;
                    case "SD_MIN":
                        cAN_Data.arryData[2] = (byte)cmd.Value;
                        cAN_Data.arryData[3] = (byte)(cmd.Value >> 8); break;
                    case "QZ_SS1":
                        cAN_Data.arryData[4] = (byte)cmd.Value; break;
                    case "QZ_TL2":
                        cAN_Data.arryData[5] = (byte)cmd.Value; break;
                    case "QZ_SL3":
                        cAN_Data.arryData[6] = (byte)cmd.Value; break;
                    case "QZ_ZY4":
                        cAN_Data.arryData[7] = (byte)cmd.Value; break;

                    case "SS_PER":
                        cAN_Data.arryData[0] = (byte)cmd.Value;
                        cAN_Data.arryData[1] = (byte)(cmd.Value >> 8); break;
                    case "SS_MAX":
                        cAN_Data.arryData[2] = (byte)cmd.Value;
                        cAN_Data.arryData[3] = (byte)(cmd.Value >> 8); break;
                    case "SS_MIN":
                        cAN_Data.arryData[4] = (byte)cmd.Value;
                        cAN_Data.arryData[5] = (byte)(cmd.Value >> 8); break;

                    case "TL_PER":
                        cAN_Data.arryData[0] = (byte)cmd.Value;
                        cAN_Data.arryData[1] = (byte)(cmd.Value >> 8); break;
                    case "TL_NAX":
                        cAN_Data.arryData[2] = (byte)cmd.Value;
                        cAN_Data.arryData[3] = (byte)(cmd.Value >> 8); break;
                    case "TL_MIN":
                        cAN_Data.arryData[4] = (byte)cmd.Value;
                        cAN_Data.arryData[5] = (byte)(cmd.Value >> 8); break;

                    case "SJ_PER":
                        cAN_Data.arryData[0] = (byte)cmd.Value;
                        cAN_Data.arryData[1] = (byte)(cmd.Value >> 8); break;
                    case "SJ_MAX":
                        cAN_Data.arryData[2] = (byte)cmd.Value;
                        cAN_Data.arryData[3] = (byte)(cmd.Value >> 8); break;
                    case "SJ_MIN":
                        cAN_Data.arryData[4] = (byte)cmd.Value;
                        cAN_Data.arryData[5] = (byte)(cmd.Value >> 8); break;

                    case "ZY_PER":
                        cAN_Data.arryData[0] = (byte)cmd.Value;
                        cAN_Data.arryData[1] = (byte)(cmd.Value >> 8); break;
                    case "ZY_MAX":
                        cAN_Data.arryData[2] = (byte)cmd.Value;
                        cAN_Data.arryData[3] = (byte)(cmd.Value >> 8); break;
                    case "ZY_MIN":
                        cAN_Data.arryData[2] = (byte)cmd.Value;
                        cAN_Data.arryData[3] = (byte)(cmd.Value >> 8); break;

                    case "ZX_CUR":
                        cAN_Data.arryData[0] = (byte)cmd.Value;
                        cAN_Data.arryData[1] = (byte)(cmd.Value >> 8); break;
                    case "ZX_FLG":
                        cAN_Data.arryData[2] = (byte)cmd.Value; break;
                    case "ZX_TTF":
                        cAN_Data.arryData[3] = (byte)cmd.Value; break;
                    case "ZX_TTD":
                        cAN_Data.arryData[4] = (byte)cmd.Value; break;

                    case "CL_FLG":
                        cAN_Data.arryData[6] = (byte)cmd.Value; break;

                    case "HZL___":
                        cAN_Data.arryData[5] = (byte)cmd.Value; break;

                    case "PSL___":
                        cAN_Data.arryData[6] = (byte)cmd.Value; break;
                }
            }
        }

        public bool Write(string str)
        {
            List<SendCMD> sendCMDs = new List<SendCMD>();
            CAN_DataFrame sendobj = new CAN_DataFrame();
            sendobj.nDataLen = 8;
            str = str.Replace('$', ' ');
            var cmds = str.Split(',');
            for (int i = 0; i < cmds.Length; i++)
            {
                if (cmds[i] == "" || cmds[i] == ",")
                    continue;
                SendCMD a = new SendCMD();
                a.Name = cmds[i].Substring(0, 6);
                a.Value = Convert.ToInt32(cmds[i].Substring(6));
                a.ID = CheckID(a.Name);
                sendobj.uID = a.ID;
                sendCMDs.Add(a);
            }

            sendobj.nSendType = 0;//Normal send
            sendobj.bRemoteFlag = 0;//不是远程帧
            sendobj.bExternFlag = 1;//扩展帧
            sendobj.nDataLen = 8;
            OganizeData(sendCMDs, ref sendobj);
            if (IsCANopen)
                if (CanCmd.CAN_ChannelSend(CANDeviceHandle, canDeviceChannel, ref sendobj, 1) == 0)
                    Info?.Invoke("[E]发送失败");
                else
                    Info?.Invoke("[I]发送成功");
            else
                Info?.Invoke("[E]CAN未打开");
            return true;
        }
    }
}