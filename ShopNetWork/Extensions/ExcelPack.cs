using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;

namespace ShopNetWork.Extensions
{
    /// <summary>
    /// Excel导入导出封装
    /// </summary>
    public class ExcelPack
    {
        /// <summary>
        /// 导出excal封装
        /// </summary>
        /// <typeparam name ="T"></typeparam>
        /// <param name     =    "data"></param>
        /// <returns></returns>
        
        public static byte[] ListToExcelPack<T>(List<T> data)
        {
            // 2007版本
            string sheetName = "sheet";
            bool isColumnWritten = true;
            IWorkbook workbook = new XSSFWorkbook();
            try
            {
                var sheet = workbook.CreateSheet(sheetName);
                //创建首行的样式
                ICellStyle s = workbook.CreateCellStyle();
                s.FillForegroundColor = HSSFColor.BlueGrey.Index;
                s.FillPattern = FillPattern.SolidForeground;
                var count = 0;
                var list = new List<string>();
                //标题
                PropertyInfo[] properties = typeof(T).GetProperties();//反射获取属性名称和displayName
                if (isColumnWritten)
                {
                    var row = sheet.CreateRow(0);
                    //循环填入首行
                    for (int j = 0; j < properties.Count(); j++)
                    {
                        var item = properties[j];
                        var attrs = item.GetCustomAttributes(typeof(DisplayNameAttribute), true);//反射获取属性
                        if (attrs != null && attrs.Count() > 0)
                        {
                            var displayName = ((DisplayNameAttribute)attrs[0]).DisplayName;
                            row.CreateCell(list.Count()).SetCellValue(displayName);
                            row.GetCell(list.Count()).CellStyle = s;
                            list.Add(item.Name);
                        }
                    }
                    count = 1;
                }
                else
                {
                    count = 0;
                }
                int totalRow = sheet.LastRowNum;
                // 总列数（1开始）
                int totalColumn = sheet.GetRow(0).LastCellNum;
                var a = sheet.GetRow(0);
                if (data.Count > 0)
                {
                    //循环向行中填入列数据
                    for (var i = 0; i < data.Count; ++i)
                    {
                        var itemData = data[i];
                        var row = sheet.CreateRow(count);
                        for (int iCell = 0; iCell < list.Count; iCell++)
                        {
                            var p = list[iCell];
                            var Properties = itemData.GetType().GetProperties().Where(c => c.Name == p).FirstOrDefault();
                            var value = Properties.GetValue(itemData)?.ToString();
                            row.CreateCell(iCell).SetCellValue(value);
                        }
                        ++count;
                    }
                }
                else
                {
                    var row = sheet.CreateRow(count);
                    for (int iCell = 0; iCell < list.Count; iCell++)
                        row.CreateCell(iCell).SetCellValue("");
                }

                //调整表格宽度防止没有显示数据
                for (int columnNum = 0; columnNum <= list.Count; columnNum++)
                {
                    int columnWidth = sheet.GetColumnWidth(columnNum) / 256;//获取当前列宽度
                    for (int rowNum = 0; rowNum <= sheet.LastRowNum; rowNum++)//在这一列上循环行
                    {
                        IRow currentRow = sheet.GetRow(rowNum);
                        ICell currentCell = currentRow.GetCell(columnNum);
                        if (currentCell != null && !string.IsNullOrEmpty(currentCell.ToString()))
                        {
                            int length = Encoding.UTF8.GetBytes(currentCell.ToString()).Length;//获取当前单元格的内容宽度
                            if (columnWidth < length + 1)
                            {
                                columnWidth = length + 1;
                            }//若当前单元格内容宽度大于列宽，则调整列宽为当前单元格宽度，后面的+1是我人为的将宽度增加一个字符
                        }

                    }
                    sheet.SetColumnWidth(columnNum, columnWidth * 256);
                }

                //创建内存流
                MemoryStream ms = new MemoryStream();
                //写入到excel
                //var ms = new MemoryStream();
                Console.WriteLine(ms.CanRead);
                Console.WriteLine(ms.CanWrite);//防止流写入失败

                workbook.Write(ms, true);//写入ms
                ms.Flush();//清空流
                ms.Seek(0, SeekOrigin.Begin);
                byte[] bytes = ms.ToArray();//转byte类型(非必要)
                return bytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return null;
            }

        }


        /// <summary>
        /// 导入excal封装
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        
        public static List<T> ExcelToListPack<T>(Stream stream, string fileName) where T : class, new()
        {
            IWorkbook workbook = null;
            string _ext = fileName.Substring(fileName.LastIndexOf("."), fileName.Length - fileName.LastIndexOf("."));
            if (_ext == ".xlsx")//判断版本
            {
                workbook = new XSSFWorkbook(stream);
            }
            else
            {
                workbook = new HSSFWorkbook(stream);
            }

            ISheet sheet = workbook.GetSheetAt(0);
            IRow ITitleRow = sheet.GetRow(0);
            int totalColumn = ITitleRow.LastCellNum;
            int totalRow = sheet.LastRowNum;
            //通过反射获取类的属性,写入excel表头
            Dictionary<string, int> dic = new Dictionary<string, int>();
            var properties = typeof(T).GetProperties();
            for (int i = 0, len = properties.Length; i < len; i++)
            {
                object[] _attributes = properties[i].GetCustomAttributes(typeof(DisplayNameAttribute), true);
                if (_attributes.Length == 0)
                {
                    continue;
                }
                string _description = ((DisplayNameAttribute)_attributes[0]).DisplayName;
                if (!string.IsNullOrWhiteSpace(_description))
                {
                    dic.Add(_description, i);
                }
            }

            string _value = string.Empty;
            string _type = string.Empty;
            int index = 0;
            List<T> list = new List<T>();
            //写入内容
            for (int i = 1; i <= totalRow; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null)
                {
                    continue;
                }
                T t = new T() { };

                var obj = new T();
                for (int j = 0; j < totalColumn; j++)
                {
                    if (dic.TryGetValue(ITitleRow.GetCell(j).ToString(), out index) && row.GetCell(j) != null)
                    {
                        //判断主是不是主键
                        _value = row.GetCell(j).ToString();


                        _type = (properties[index].PropertyType).FullName;


                        if (_type == "System.String")
                        {
                            properties[index].SetValue(obj, _value, null);
                        }
                        else if (_type == "System.DateTime")
                        {
                            DateTime pdt = Convert.ToDateTime(_value, CultureInfo.InvariantCulture);
                            properties[index].SetValue(obj, pdt, null);
                        }
                        else if (_type == "System.Boolean")
                        {
                            bool pb = Convert.ToBoolean(_value);
                            properties[index].SetValue(obj, pb, null);
                        }
                        else if (_type == "System.Int16")
                        {
                            short pi16 = Convert.ToInt16(_value);
                            properties[index].SetValue(obj, pi16, null);
                        }
                        else if (_type == "System.Int32")
                        {
                            int pi32 = Convert.ToInt32(_value);
                            properties[index].SetValue(obj, pi32, null);
                        }
                        else if (_type == "System.Int64")
                        {
                            long pi64 = Convert.ToInt64(_value);
                            properties[index].SetValue(obj, pi64, null);
                        }
                        else if (_type == "System.Byte")
                        {
                            byte pb = Convert.ToByte(_value);
                            properties[index].SetValue(obj, pb, null);
                        }
                        else
                        {
                            properties[index].SetValue(obj, null, null);
                        }
                    }
                }
                list.Add(obj);
            }
            return list;
        }
    }


    
}
