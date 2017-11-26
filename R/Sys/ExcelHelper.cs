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
            CreateCellStyle();
        }
        public int area;
        public XSSFWorkbook workbook;
        public ISheet sheet;
        public List<TPushPlan> plan;
        //public List<TPushPlan> plan1;
        //public List<TPushPlan> plan2;
        public ICellStyle style;
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
        public void GetDetailData(List<TPushPlan> plan, int add)
        {
            IRow[] rows = new IRow[plan.Count];
            for (int j = 0; j < 2; j++)
            {//2个大表
                int index = 1;
                for (int i = 0; i < plan.Count; i++)
                {//单个表单的内容
                    int columnIndex = 0;
                    rows[i] = sheet.GetRow(4 + add + i);
                    if (i > 0)
                    {//上下两炉出焦的间隔时间大于30min，则用粗下划线标记出来
                        int[] arr = new int[] { 0 + j * 9, 6 + j * 9 };
                        CreateThickLine(plan[i], plan[i - 1], rows[i - 1], arr);
                    }
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
                {//单个表单的内容
                    if (i > 0)
                    {//上下两炉出焦的间隔时间大于30min，则用粗下划线标记出来
                        int[] arr = new int[] { 18 + j * 5, 20 + j * 5 };
                        CreateThickLine(plan[i], plan[i - 1], rows[i - 1], arr);
                    }
                    rows[i].GetCell(18 + 5 * j).SetCellValue(index++.ToString());
                    rows[i].GetCell(18 + 1 + 5 * j).SetCellValue(plan[i].RoomNum);
                    rows[i].GetCell(18 + 2 + 5 * j).SetCellValue(plan[i].PushTime.ToString("t"));
                }
            }
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
            GetDetailData(plan, 0);
            //GetDetailData(plan2, 48);
        }
        private void CreateCellStyle()
        {
            style = workbook.CreateCellStyle();
            style.BorderBottom = BorderStyle.Medium;
            style.BorderTop = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            //字体待设置
            IFont font = workbook.CreateFont();
            font.IsBold = true;
            font.FontHeightInPoints = 15;
            style.SetFont(font);
        }
        private void CreateThickLine(TPushPlan p1, TPushPlan p2, IRow row, int[] arr)
        {
            if ((p1.PushTime - p2.PushTime).TotalMinutes >= 30)
            {
                for (int i = arr[0]; i < arr[1] + 1; i++)
                {
                    if (row.Cells.Count <= i) return;
                    ICell cell = row.Cells[i];
                    cell.CellStyle = style;
                }
            }
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
