using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using WGPM.R.RoomInfo;

namespace WGPM.R.XML
{
    class OperateConfig
    {
        public OperateConfig(string fileName)
        {
            IniParmsValue(fileName);
        }
        public OperateConfig(string fileName, List<TPushPlan> recPlanLst)
        {
            IniParmsValue(fileName);
            this.recPlanLst = recPlanLst;
        }
        public string Path { get { return path; } }
        private string path;
        public XmlDocument XmlDoc { get { return xmlDoc; } }
        private XmlDocument xmlDoc;
        //节点对应每个config的<Addr/> or <Plan/>；
        public XmlNode XmlNode { get; set; }
        //对应单个对象，如单孔炉号对应的物理地址；又如单孔炉号对应的生产计划：计划推焦时间，实际推焦时间，规定结焦时间，实际结焦时间，计划装煤时间，实际装煤时间（平煤）
        public XmlElement element;
        public List<TPushPlan> recPlanLst;
        public XmlNodeList XmlNodeList { get; set; }
        /// <summary>
        /// 更改RoomPlanInfo.Config的Valid
        /// </summary>
        /// <param name="roomNum">炉号用来定位XMLElement</param>
        /// <param name="keyName">要更改的Attribute</param>
        /// <param name="keyValue">要更改的Attribute的Value</param>
        public void SetPlanAttributeValue(int roomNum, string keyName, string keyValue)
        {

            if (XmlNode != null)
            {
                element = (XmlElement)XmlNode.SelectSingleNode("//add[@R='" + roomNum.ToString("000") + "']");
                if (element != null)
                {
                    element.SetAttribute(keyName, keyValue);
                }
            }
        }

        public void RecToConfig()
        {
            if (recPlanLst != null && recPlanLst.Count > 0)
            {
                for (int i = 0; i < recPlanLst.Count; i++)
                {
                    SetPlanAttributeValue(recPlanLst[i].RoomNum, "Valid", "1");
                    SetPlanAttributeValue(recPlanLst[i].RoomNum, "Period", recPlanLst[i].Period.ToString());
                    SetPlanAttributeValue(recPlanLst[i].RoomNum, "Group", recPlanLst[i].Group.ToString());
                    SetPlanAttributeValue(recPlanLst[i].RoomNum, "PlannedTimeToPush", recPlanLst[i].PushTime.ToString("g"));
                }
                XmlDoc.Save(Path);
            }
        }
        private void IniParmsValue(string fileName)
        {
            xmlDoc = new XmlDocument();
            path = Environment.CurrentDirectory + @"\\" + fileName;
            xmlDoc.Load(path);
            XmlNode = xmlDoc.DocumentElement;
            XmlNodeList = XmlNode.SelectNodes(@"//add");
        }
        #region Schedule,GetValueSetValue
        public string GetValueByAttribute(string attribute)
        {
            return XmlNodeList[0].Attributes[attribute].ToString();
        }
        public void SetValueByAttribute(string attribute,int aValue)
        {
            XmlNodeList[0].Attributes[attribute].Value = aValue.ToString();
            Save();
        }
        #endregion
        public void Save()
        {
            XmlDoc.Save(Path);
        }
        
    }
}
