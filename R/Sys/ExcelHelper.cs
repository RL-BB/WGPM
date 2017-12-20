using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WGPM.R.RoomInfo;
using WGPM.R.UI;

namespace WGPM.R.Sys
{
    /// <summary>
    /// 计划打印
    /// </summary>
    class PlanPrintHelper
    {
        public PlanPrintHelper(XSSFWorkbook workbook, List<TPushPlan> plan, int area)
        {
            this.workbook = workbook;
            sheet = this.workbook.GetSheet("Plan");
            this.plan = plan;
            this.area = area;
            //SplitPlanByArea();
            flagStyle = CreateNormalStyle(true);
            normalStyle = CreateNormalStyle(false);
            dottedStyle = CreateDottedStyle();
        }
        public int area;
        public XSSFWorkbook workbook;
        public ISheet sheet;
        public List<TPushPlan> plan;
        //public List<TPushPlan> plan1;
        //public List<TPushPlan> plan2;
        public ICellStyle flagStyle;
        public ICellStyle normalStyle;
        public ICellStyle dottedStyle;
        private void SplitPlanByArea()
        {
            //plan1 = new List<TPushPlan>();
            //plan2 = new List<TPushPlan>();
            //if (plan == null)
            //{
            //    return;
            //}
            //for (int i = 0; i < plan.Count; i++)
            //{
            //    if (plan[i].RoomNum <= 55)
            //    {
            //        plan1.Add(plan[i]);
            //    }
            //    else
            //    {
            //        plan2.Add(plan[i]);
            //    }
            //}
            //plan1.Sort(TPushPlan.CompareByTime);
            //plan2.Sort(TPushPlan.CompareByTime);
        }
        /// <summary>
        /// "一炼焦","1#炉区"
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public void GetCokeArea(int add)
        {
            //“一炼焦：”，“1#炉区”
            IRow row0 = sheet.GetRow(0 + add);
            IRow row1 = sheet.GetRow(0 + add);
            area = area + (Setting.AreaFlag ? 1 : 3);//表示几号炉区，和cboPrintArea.SelectedIndex相关
            row0.GetCell(4).SetCellValue(area + "#炉区");
            row0.GetCell(9 + 4).SetCellValue(area + "#炉区");
            row1.GetCell(9 + 9).SetCellValue(area + "#炉区");
            row1.GetCell(9 + 9 + 5).SetCellValue(area + "#炉区");
            row1.GetCell(9 + 9 + 5 + 5).SetCellValue(area + "#炉区");
        }
        public void GetDate(int add)
        {
            //“日期”，“推焦计划”
            IRow row = sheet.GetRow(1 + add);
            row.GetCell(0).SetCellValue(plan[0].PushTime.ToString("D"));
            row.GetCell(9).SetCellValue(plan[0].PushTime.ToString("D"));
            row.GetCell(9 + 9).SetCellValue(plan[0].PushTime.ToString("D"));
            row.GetCell(9 + 9 + 5).SetCellValue(plan[0].PushTime.ToString("D"));
            row.GetCell(9 + 9 + 5 + 5).SetCellValue(plan[0].PushTime.ToString("D"));
            //row4.CreateCell(4).SetCellValue("推焦计划");
        }
        public void GetSchedule(int add)
        {
            //“时段”，“班组”
            IRow row = sheet.GetRow(2 + add);
            //row3.GetCell(startIndex + 0);
            row.GetCell(2).SetCellValue(plan[0].StrPeriod);
            row.GetCell(9 + 2).SetCellValue(plan[0].StrPeriod);
            row.GetCell(9 + 9).SetCellValue(plan[0].StrPeriod);
            row.GetCell(9 + 9 + 5).SetCellValue(plan[0].StrPeriod);
            row.GetCell(9 + 9 + 5 + 5).SetCellValue(plan[0].StrPeriod);
            //row3.GetCell(startIndex + 4);
            row.GetCell(5).SetCellValue(plan[0].StrGroup);
            row.GetCell(9 + 5).SetCellValue(plan[0].StrGroup);
            row.GetCell(9 + 9 + 2).SetCellValue(plan[0].StrGroup);
            row.GetCell(9 + 9 + 2 + 5).SetCellValue(plan[0].StrGroup);
            row.GetCell(9 + 9 + 2 + 5 + 5).SetCellValue(plan[0].StrGroup);
        }
        public void GetTitle()
        {
            IRow row4 = sheet.GetRow(3);
            //IRow row56 = sheet.GetRow(4 + 48);
        }
        public IRow[] GetDetailData(List<TPushPlan> plan, int add)
        {
            IRow[] rows = CreateRowsAndNormalCellStyle();
            for (int j = 0; j < 2; j++)
            {//2个大表
                int index = 1;
                for (int i = 0; i < plan.Count; i++)
                {//单个表单的内容
                    int columnIndex = 0;
                    rows[i].GetCell(9 * j + columnIndex++).SetCellValue(index++.ToString());
                    rows[i].GetCell(9 * j + columnIndex++).SetCellValue(plan[i].RoomNum);
                    rows[i].GetCell(9 * j + columnIndex++).SetCellValue(plan[i].BurnTimeToString());
                    rows[i].GetCell(9 * j + columnIndex++).SetCellValue(plan[i].PushTime.ToString("t"));
                }
            }
            for (int j = 0; j < 3; j++)
            {
                int index = 1;
                for (int i = 0; i < plan.Count; i++)
                {//单个表单的内容(三个小表)
                    rows[i].GetCell(18 + 0 + 5 * j).SetCellValue(index++.ToString());
                    rows[i].GetCell(18 + 1 + 5 * j).SetCellValue(plan[i].RoomNum);
                    rows[i].GetCell(18 + 2 + 5 * j).SetCellValue(plan[i].PushTime.ToString("t"));
                }
            }
            return rows;
        }
        /// <summary>
        /// 生成用于打印的Excel表格
        /// </summary>
        public void GetToPrintedExcel()
        {
            GetCokeArea(0);
            //GetCokeArea(48);
            GetDate(0);
            //GetDate(48);
            GetSchedule(0);
            //GetSchedule(48);
            GetTitle();
            IRow[] rows = GetDetailData(plan, 0);
            //GetDetailData(plan2, 48);
        }
        /// <summary>
        /// 为包含有效数据内容（计划信息）的行设置Style
        /// </summary>
        /// <param name="flag">用以判断是否需要作特殊标识的</param>
        /// <returns>设置了单元格格式的行</returns>
        private ICellStyle CreateNormalStyle(bool flag)
        {
            ICellStyle style = workbook.CreateCellStyle();
            style = workbook.CreateCellStyle();
            style.BorderBottom = flag ? BorderStyle.Medium : BorderStyle.Thin;
            style.BorderTop = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            //字体待设置
            IFont font = workbook.CreateFont();
            font.IsBold = flag;
            font.FontHeightInPoints = flag ? (short)15 : (short)14;
            style.SetFont(font);
            return style;
        }
        private ICellStyle CreateDottedStyle()
        {
            ICellStyle style = workbook.CreateCellStyle();
            style.BorderRight = BorderStyle.MediumDashDot;
            return style;
        }
        /// <summary>
        /// 设置单元格格式：①正常格式；②第二趟笺的上一行加粗以作区分；③两个表单之间加虚线；
        /// </summary>
        /// <param name="breakFlag">和上一条计划的间隔时间是否过长（boundary：30min）</param>
        /// <param name="row">待设置的行</param>
        private void CreateThickLine(bool breakFlag, IRow row)
        {
            for (int i = 0; i < 31; i++)//20171220 数字31的意义：从PlanModel.xlsx中可得知每一行的有效单元格数量为31；
            {
                if (row.Cells.Count <= i) return;
                //20171220：数字7,8,16,17...时为为单元格左虚线；
                if (i == 7 || i == 8 || i == 16 || i == 17 || i == 21 || i == 22 || i == 26 || i == 27)
                {
                    CreateDottedCell(row);
                    continue;
                }
                row.Cells[i].CellStyle = breakFlag ? flagStyle : normalStyle;
            }
        }
        private void CreateDottedCell(IRow row)
        {
            row.Cells[7].CellStyle = dottedStyle;
            row.Cells[16].CellStyle = dottedStyle;
            row.Cells[21].CellStyle = dottedStyle;
            row.Cells[26].CellStyle = dottedStyle;
        }
        private IRow[] CreateRowsAndNormalCellStyle()
        {
            IRow[] rows = new IRow[plan.Count];
            for (int i = 0; i < plan.Count; i++)
            {
                rows[i] = sheet.CreateRow(4 + i);
                for (int j = 0; j < 31; j++)//20171220 数字31的意义：从PlanModel.xlsx中可得知每一行的有效单元格数量为31；
                {
                    rows[i].CreateCell(j);
                }
                if (i == 0)
                {
                    CreateThickLine(false, rows[i]);
                    continue;
                }
                bool breakFlag = (plan[i].PushTime - plan[i - 1].PushTime).TotalMinutes > 30;
                CreateThickLine(breakFlag, rows[i - 1]);
                if (i == plan.Count - 1) CreateThickLine(false, rows[i]);
            }
            return rows;
        }
    }
    /// <summary>
    /// 生产统计打印
    /// </summary>
    class SumPrintHelper
    {
        public SumPrintHelper()
        {

        }
        private XSSFWorkbook workbook;
        private ISheet sheet;
        private ICellStyle style;
    }
}
