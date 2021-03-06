﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WalkingTec.Mvvm.Core.Extensions
{
    public static class ListVMExtension
    {
        /// <summary>
        /// 获取Jason格式的列表数据
        /// </summary>
        /// <param name="self">是否需要对数据进行Json编码</param>
        /// <returns>Json格式的数据</returns>
        public static string GetDataJson<T>(this IBasePagedListVM<T, BaseSearcher> self) where T : TopBasePoco
        {
            //如果列表没有执行过搜索，则自动执行
            if (self.IsSearched == false)
            {
                self.DoSearch();
            }
            var sb = new StringBuilder();
            var el = self.GetEntityList().ToList();
            //如果列表主键都为0，则生成自增主键，避免主键重复
            if (el.All(x => x.ID == Guid.Empty))
            {
                el.ForEach(x => x.ID = Guid.NewGuid());
            }
            //循环生成列表数据
            for (int x = 0; x < el.Count; x++)
            {
                var sou = el[x];
                sb.Append(self.GetSingleDataJson(sou));
                if (x < el.Count - 1)
                {
                    sb.Append(",");
                }
            }
            return $"[{sb.ToString()}]";
        }

        private static string GetFormatResult(BaseVM vm, ColumnFormatInfo info)
        {
            string rv = "";
            switch (info.FormatType)
            {
                case ColumnFormatTypeEnum.Dialog:
                    rv = vm.UIService.MakeDialogButton(info.ButtonType, info.Url, info.Text, info.Width, info.Height, info.Title, info.ButtonID, info.ShowDialog, info.Resizable).ToString();
                    break;
                case ColumnFormatTypeEnum.Redirect:
                    rv = vm.UIService.MakeRedirectButton(info.ButtonType, info.Url, info.Text).ToString();
                    break;
                case ColumnFormatTypeEnum.Download:
                    if (info.FileID == null)
                    {
                        rv = "";
                    }
                    else
                    {
                        rv = vm.UIService.MakeDownloadButton(info.ButtonType, info.FileID.Value, info.Text).ToString();
                    }
                    break;
                case ColumnFormatTypeEnum.ViewPic:
                    if (info.FileID == null)
                    {
                        rv = "";
                    }
                    else
                    {
                        rv = vm.UIService.MakeViewButton(info.ButtonType, info.FileID.Value, info.Text, info.Width, info.Height, info.Title, info.Resizable).ToString();
                    }
                    break;
                case ColumnFormatTypeEnum.Script:
                    rv = vm.UIService.MakeScriptButton(info.ButtonType, info.Url, info.Width, info.Height, info.WindowID, info.Text, info.Title, info.ButtonID, info.Script).ToString();
                    break;
                case ColumnFormatTypeEnum.Html:
                    rv = info.Html;
                    break;
                default:
                    break;
            }
            return rv;
        }

        /// <summary>
        /// 生成单条数据的Json格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="sou">数据</param>
        /// <returns>Json格式的数据</returns>
        public static string GetSingleDataJson<T>(this IBasePagedListVM<T, BaseSearcher> self, T sou) where T : TopBasePoco
        {
            var sb = new StringBuilder();
            var RowBgColor = string.Empty;
            var RowColor = string.Empty;
            RowBgColor = self.SetFullRowBgColor(sou);
            RowColor = self.SetFullRowColor(sou);
            var isSelected = self.GetIsSelected(sou);
            //循环所有列
            sb.Append("{");
            var containsID = false;
            foreach (var baseCol in self.GetHeaders())
            {
                foreach (var col in baseCol.BottomChildren)
                {
                    if (col.ColumnType != GridColumnTypeEnum.Normal)
                    {
                        continue;
                    }
                    if (col.FieldName == "Id")
                    {
                        containsID = true;
                    }
                    var backColor = col.GetBackGroundColor(sou);
                    //获取ListVM中设定的单元格前景色
                    var foreColor = col.GetForeGroundColor(sou);

                    if (backColor == string.Empty)
                    {
                        backColor = RowBgColor;
                    }
                    if (foreColor == string.Empty)
                    {
                        foreColor = RowColor;
                    }
                    var style = string.Empty;
                    if (backColor != string.Empty)
                    {

                        style += $"background-color:#{backColor};";
                    }
                    if (foreColor != string.Empty)
                    {
                        style += $"color:#{foreColor};";
                    }
                    //设定列名，如果是主键ID，则列名为id，如果不是主键列，则使用f0，f1,f2...这种方式命名，避免重复
                    sb.Append($"\"{col.Field}\":");
                    var html = string.Empty;
                    var info = col.GetText(sou);

                    if (info is ColumnFormatInfo)
                    {
                        html = GetFormatResult(self as BaseVM, info as ColumnFormatInfo);
                    }
                    else if (info is List<ColumnFormatInfo>)
                    {
                        var temp = string.Empty;
                        foreach (var item in info as List<ColumnFormatInfo>)
                        {
                            temp += GetFormatResult(self as BaseVM, item);
                            temp += "&nbsp;&nbsp;";
                        }
                        html = temp;
                    }
                    else
                    {
                        html = info.ToString();
                    }

                    var ptype = col.FieldType;
                    //如果列是布尔值，直接返回true或false，让ExtJS生成CheckBox
                    if (ptype == typeof(bool) || ptype == typeof(bool?))
                    {
                        if (html.ToLower() == "true")
                        {
                            html = (self as BaseVM).UIService.MakeCheckBox(true,isReadOnly:true);
                        }
                        if (html.ToLower() == "false" || html == string.Empty)
                        {
                            html = (self as BaseVM).UIService.MakeCheckBox(false, isReadOnly: true);
                        }
                    }
                    //如果列是枚举，直接使用枚举的文本作为多语言的Key查询多语言文字
                    else if (ptype.IsEnumOrNullableEnum())
                    {
                        html = PropertyHelper.GetEnumDisplayName(ptype, html);
                    }
                    if (style != string.Empty)
                    {
                        html = $"<div style=\"{style}\">{html}</div>";
                    }
                    html = "\"" + html.Replace(Environment.NewLine, "").Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
                    sb.Append(html);
                    sb.Append(",");
                }
            }
            sb.Append($"\"TempIsSelected\":\"{ (isSelected == true ? "1" : "0") }\"");
            if (containsID == false)
            {
                sb.Append($",\"ID\":\"{sou.ID}\"");
            }
            // 标识当前行数据是否被选中
            sb.Append($@",""LAY_CHECKED"":{sou.Checked.ToString().ToLower()}");
            sb.Append(string.Empty);
            sb.Append("}");
            return sb.ToString();
        }

    }
}
