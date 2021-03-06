﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using VuetifySpa.Data.ViewModel;
using WkWrap.Core;

namespace VuetifySpa.Data
{
    public static class Code
    {
        public static long ToLong(this Guid val)
        {
            return BitConverter.ToInt64(val.ToByteArray(), 0);
        }

        public static void SaveBase64(string data64, string path)
        {
            var row = "base64,";
            var index = data64.IndexOf(row, StringComparison.OrdinalIgnoreCase);
            var data = Convert.FromBase64String(data64.Substring(index + row.Length));
            System.IO.File.WriteAllBytes(path, data);
        }

        /// <summary>
        /// заменить base64 на img путь
        /// </summary>
        /// <param name="id">ид модели</param>
        /// <param name="img64">масив</param>
        /// <param name="imageext">расшерение</param>
        /// <param name="rootpath">webroot path</param>
        /// <param name="folderType">"тип папки (Avatar)"</param>
        /// <returns></returns>
        public static string SaveImage64(Guid id, string img64, string imageext, string rootpath, string folderType)
        {
            ////img/avatar/2b4d3c9a-4519-414f-8d15-55c90fdf9288.jpg?v=2018102111301294


            if (img64.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            {
                var filename = string.Format("{0}{1}", id, imageext);
                var physicalpath = System.IO.Path.Combine(rootpath, string.Format(@"img{2}{0}{2}{1}", folderType, filename, System.IO.Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(physicalpath)) System.IO.File.Delete(physicalpath);
                SaveBase64(img64, physicalpath);
                return filename;

            }
            else
            {

                var start = img64.IndexOf(id.ToString());
                var length = id.ToString().Length + 4;
                return img64.Substring(start, length);
            }

        }
        /// <summary>
        /// экспорт в excell 
        /// </summary>
        public static byte[] ExcellExport(List<ExportDataView> exportDataView, int hipiStyle = 0)
        {
            var data = new List<ExportDataView>();


            if (exportDataView != null && exportDataView.Any())
            {
                data = exportDataView;
            }

            MemoryStream stream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(stream))
            {
                foreach (var exportData in exportDataView)
                {

                    var worksheet = package.Workbook.Worksheets.Add(exportData.WorkSheetTitle);

                    var rowIndex = 1;



                    var headerLength = exportData.Header.Count;
                    if (hipiStyle == 1)
                    {
                        // worksheet.Cells[rowIndex, headerLength].AutoFitColumns();

                        worksheet.Row(1).Height = 55;
                        worksheet.Cells[rowIndex, 1, rowIndex, headerLength].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[rowIndex, 1, rowIndex, headerLength].Style.Font.Bold = true;
                        //worksheet.Cells[rowIndex, 1, rowIndex, headerLength].Merge = true;
                        worksheet.Cells[rowIndex, 1, rowIndex, headerLength].Style.WrapText = true;

                    }

                    //worksheet.Cells[rowIndex, 1, rowIndex, length].Value = data[table].Title;

                    //rowIndex++;


                    for (var i = 0; i < headerLength; i++)
                    {


                        worksheet.Cells[rowIndex, i + 1].Value = exportData.Header[i];
                    }

                    rowIndex++;
                    for (var i = 0; i < exportData.Data.Count(); i++)
                    {
                        for (var j = 0; j < exportData.Data[i].Count(); j++)
                        {
                            worksheet.Cells[rowIndex, j + 1].Value = exportData.Data[i][j];
                        }

                        rowIndex++;
                    }
                    worksheet.Cells[rowIndex, 1, rowIndex, headerLength].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowIndex, 1, rowIndex, headerLength].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }
                return package.GetAsByteArray();
            }
        }
        private static IOrderedQueryable<T> OrderingHelper<T>(IQueryable<T> source, string propertyName, bool descending, bool anotherLevel)
        {
            ParameterExpression param = Expression.Parameter(typeof(T), string.Empty); // I don't care about some naming
            MemberExpression property = Expression.PropertyOrField(param, propertyName);
            LambdaExpression sort = Expression.Lambda(property, param);
            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                (!anotherLevel ? "OrderBy" : "ThenBy") + (descending ? "Descending" : string.Empty),
                new[] { typeof(T), property.Type },
                source.Expression,
                Expression.Quote(sort));
            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
        }
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return OrderingHelper(source, propertyName, false, false);
        }
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            return OrderingHelper(source, propertyName, true, false);
        }
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string propertyName)
        {
            return OrderingHelper(source, propertyName, false, true);
        }
        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string propertyName)
        {
            return OrderingHelper(source, propertyName, true, true);
        }


        public static byte[] ExportToPdf(List<ExportDataView> exportDataView, IHostingEnvironment hostingEviroment)
        {
            var html = "<html><head><meta charset='utf-8'>" +
                "<meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>" +
                "</head><body style='margin-top: 10px; text-align: center;'><h2 style='text-align: center;'>" +
                " <style type='text/css'>thead{display:table-row-group}tr{page-break-inside:avoid}table {border-spacing:0px}table td,table th{padding:.8em;border:1px solid}table th{background-color:#F0F0F0;font-weight:700}</style>report_title</h2>" +
                "<div style='display:inline-block;'>report_table</div></body></html>";

            html = html.Replace("report_title", exportDataView[0].WorkSheetTitle);
            var tables = new List<string>();
            foreach (var tableData in exportDataView)
            {
                var tableHtml = new StringBuilder();
                tableHtml.Append("<table style:'margin:20px 0'>");
                tableHtml.Append("<thead><tr>");
                foreach (var th in tableData.Header) 
                {
                    tableHtml.Append("<th>");
                    tableHtml.Append(th);
                    tableHtml.Append("</th>");
                }
                tableHtml.Append("</tr></thead><tbody>");
                foreach (var tr in tableData.Data)
                {
                    tableHtml.Append("<tr>");
                    foreach (var td in tr)
                    {
                        tableHtml.Append("<td>");
                        tableHtml.Append(td);
                        tableHtml.Append("</td>");
                    }
                    tableHtml.Append("<tr>");
                }
                tableHtml.AppendLine("</tbody></table></div><br><br>");
                tables.Add(tableHtml.ToString());
                
            }
            html = html.Replace("report_table", string.Join(" ",tables));

          
          

            string wkhtmltopdfPath;
             if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))

                wkhtmltopdfPath = Path.Combine(hostingEviroment.WebRootPath, "dll", "wkhtmltopdf.exe");
            else
                wkhtmltopdfPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Lib", "wkhtmltopdf");

            var wkhtmltopdf = new FileInfo(wkhtmltopdfPath);
            var converter = new HtmlToPdfConverter(wkhtmltopdf);
            return converter.ConvertToPdf(html);
           
        }
    }

}
