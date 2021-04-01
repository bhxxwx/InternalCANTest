using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Laike.Can.CanCmd;   // 引用dll定义

namespace CANDriverLayer
{
    public class DecodeData
    {
        /// <summary>
        /// CAN Json class
        /// </summary>
        public class JSON
        {
            //GPS
            public string Mark { get; set; }

            public string E { get; set; }
            public string N { get; set; }
            public string time { get; set; }
            public string High { get; set; }
            public string Speed { get; set; }
            public string Path { get; set; }
            public string CANerr { get; set; }
            public string GPSerr { get; set; }

            //CAN1
            public string HXCS { get; set; }

            public string ZXCS { get; set; }
            public string XLGWY { get; set; }
            public string XLGZJ { get; set; }

            //CAN2
            public string GTGD { get; set; }

            public string BHLGD { get; set; }
            public string XGD { get; set; }

            //CAN3
            public string DPZQ { get; set; }

            public string DPYQ { get; set; }
            public string DPZH { get; set; }
            public string DPYH { get; set; }

            //CAN4
            public string QJSD { get; set; }

            public string LZLL { get; set; }

            //CAN5
            public string ZGDZS { get; set; }

            public string BHLZS { get; set; }
            public string SSCZS { get; set; }
            public string TLGT { get; set; }

            //CAN6
            public string FJZS { get; set; }

            public string SLJLZS { get; set; }
            public string ZYJLZS { get; set; }

            //CAN7
            public string QXSS { get; set; }

            public string JDSS { get; set; }
            public string TLJX { get; set; }
            public string DLBKD { get; set; }
            public string JFKKD { get; set; }
            public string YLKSD { get; set; }

            //CAN8
            public string GFKD { get; set; }

            public string HZL { get; set; }
            public string PSL { get; set; }

            public string GTGD_R { get; set; }
        }

        private static JSON json = new JSON();

        public static int[] count = new int[8];

        public JSON getjson
        {
            get { return json; }
        }

        public bool DecodePacket(CAN_DataFrame cAN_Data)
        {
            switch (cAN_Data.uID)
            {
                case 0x18FF2111: Analysis_DATA(1, cAN_Data); count[0] = 1; break;
                case 0x18FF2313: Analysis_DATA(2, cAN_Data); count[1] = 1; break;
                case 0x18FF2413: Analysis_DATA(3, cAN_Data); count[2] = 1; break;
                case 0x0CFF2715: Analysis_DATA(4, cAN_Data); count[3] = 1; break;
                case 0x18FF2815: Analysis_DATA(5, cAN_Data); count[4] = 1; break;
                case 0x18FF2915: Analysis_DATA(6, cAN_Data); count[5] = 1; break;
                case 0x18FF2515: Analysis_DATA(7, cAN_Data); count[6] = 1; break;
                case 0x0CFF2615: Analysis_DATA(8, cAN_Data); count[7] = 1; break;
            }
            if (count.Min() == 1)
            {
                count.ToList().ForEach(a =>
                {
                    a = 0;
                });
                return true;
            }
            return false;
        }

        unsafe private void Analysis_DATA(UInt16 Pack_NO, CAN_DataFrame CANRX)
        {
            switch (Pack_NO)
            {
                case 1:
                    json.HXCS = CANRX.arryData[0].ToString();
                    json.ZXCS = CANRX.arryData[1].ToString();
                    json.XLGWY = ((CANRX.arryData[3] << 8) + CANRX.arryData[2]).ToString();
                    // CANDATA.XLGWY = CANRX.arryData[2]&0XFFFF;
                    json.XLGZJ = ((CANRX.arryData[5] << 8) + CANRX.arryData[4]).ToString();
                    // CANDATA.XLGZJ += CANRX.arryData[4];
                    break;

                case 2:
                    json.GTGD = (((ushort)CANRX.arryData[1] << 8) + (ushort)CANRX.arryData[0]).ToString();
                    // CANDATA.GTGD += CANRX.arryData[0];
                    json.BHLGD = ((ushort)(CANRX.arryData[3] << 8) + CANRX.arryData[2]).ToString();
                    // CANDATA.BHLGD += CANRX.arryData[2];
                    json.XGD = ((ushort)(CANRX.arryData[5] << 8) + CANRX.arryData[4]).ToString();
                    // CANDATA.XGD += CANRX.arryData[4];
                    json.GTGD_R = ((ushort)(CANRX.arryData[7] << 8) + CANRX.arryData[6]).ToString();
                    break;

                case 3:
                    json.DPZQ = ((CANRX.arryData[1] << 8) + CANRX.arryData[0]).ToString();
                    // CANDATA.DPZQ += CANRX.arryData[0];
                    json.DPYQ = ((CANRX.arryData[3] << 8) + CANRX.arryData[2]).ToString();
                    // CANDATA.DPYQ += CANRX.arryData[2];
                    json.DPZH = ((CANRX.arryData[5] << 8) + CANRX.arryData[4]).ToString();
                    // CANDATA.DPZH += CANRX.arryData[4];
                    json.DPYH = ((CANRX.arryData[7] << 8) + CANRX.arryData[6]).ToString();
                    // CANDATA.DPYH += CANRX.arryData[6];
                    break;

                case 4:
                    json.QJSD = ((CANRX.arryData[3] << 8) + CANRX.arryData[2]).ToString();
                    // CANDATA.QJSD += CANRX.arryData[2];
                    json.LZLL = ((CANRX.arryData[5] << 8) + CANRX.arryData[4]).ToString();
                    // CANDATA.LZLL += CANRX.arryData[4];
                    break;

                case 5:
                    json.ZGDZS = ((CANRX.arryData[1] << 8) + CANRX.arryData[0]).ToString();
                    // CANDATA.ZGDZS += CANRX.arryData[0];
                    json.BHLZS = ((CANRX.arryData[3] << 8) + CANRX.arryData[2]).ToString();
                    // CANDATA.BHLZS += CANRX.arryData[2];
                    json.SSCZS = ((CANRX.arryData[5] << 8) + CANRX.arryData[4]).ToString();
                    // CANDATA.SSCZS += CANRX.arryData[4];
                    json.TLGT = ((CANRX.arryData[7] << 8) + CANRX.arryData[6]).ToString();
                    // CANDATA.TLGT += CANRX.arryData[6];
                    break;

                case 6:
                    json.FJZS = ((CANRX.arryData[1] << 8) + CANRX.arryData[0]).ToString();
                    // CANDATA.FJZS += CANRX.arryData[0];
                    json.SLJLZS = ((CANRX.arryData[3] << 8) + CANRX.arryData[2]).ToString();
                    // CANDATA.SLJLZS += CANRX.arryData[2];
                    json.ZYJLZS = ((CANRX.arryData[5] << 8) + CANRX.arryData[4]).ToString();
                    // CANDATA.ZYJLZS += CANRX.arryData[4];
                    break;

                case 7:
                    json.QXSS = ((CANRX.arryData[1] << 8) + CANRX.arryData[0]).ToString();
                    // CANDATA.QXSS += CANRX.arryData[0];
                    json.JDSS = ((CANRX.arryData[3] << 8) + CANRX.arryData[2]).ToString();
                    // CANDATA.JDSS += CANRX.arryData[2];
                    json.TLJX = CANRX.arryData[4].ToString();
                    json.DLBKD = CANRX.arryData[5].ToString();
                    json.JFKKD = CANRX.arryData[6].ToString();
                    json.YLKSD = CANRX.arryData[7].ToString();
                    break;

                case 8:
                    json.GFKD = ((CANRX.arryData[4] << 8) + CANRX.arryData[3]).ToString();
                    // CANDATA.GFKD += CANRX.arryData[3];
                    json.HZL = CANRX.arryData[5].ToString();
                    json.PSL = CANRX.arryData[6].ToString();
                    break;

                default:
                    break;
            }
        }
    }
}