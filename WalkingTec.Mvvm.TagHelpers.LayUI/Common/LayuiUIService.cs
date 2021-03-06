﻿using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using WalkingTec.Mvvm.Core;

namespace WalkingTec.Mvvm.TagHelpers.LayUI.Common
{
    public class LayuiUIService : IUIService
    {
        public string MakeDialogButton(ButtonTypesEnum buttonType, string url, string buttonText, int? width, int? height, string title = null, string buttonID = null, bool showDialog = true, bool resizable = true)
        {
            if (buttonID == null)
            {
                buttonID = Guid.NewGuid().ToString();
            }
            var innerClick = "";
            string windowid = Guid.NewGuid().ToString();
            if (showDialog == true)
            {
                innerClick = $"ff.OpenDialog('{url}', '{windowid}', '{title ?? ""}',{width?.ToString() ?? "null"}, {height?.ToString() ?? "null"});";
            }
            else
            {
                innerClick = $"$.ajax({{cache: false,type: 'GET',url: '{url}',async: true,success: function(data, textStatus, request) {{eval(data);}} }});";
            }
            var click = $"<script>$('#{buttonID}').on('click',function(){{{innerClick};return false;}});</script>";
            string rv = "";
            if(buttonType == ButtonTypesEnum.Link)
            {
                rv = $"<a id='{buttonID}' style='color:blue;cursor:pointer'>{buttonText}</a>";
            }
            if(buttonType == ButtonTypesEnum.Button)
            {
                rv = $"<a id='{buttonID}' class='layui-btn layui-btn-primary layui-btn-xs'>{buttonText}</a>";
            }
            rv += click;
            return rv;
        }

        public string MakeDownloadButton(ButtonTypesEnum buttonType, Guid fileID, string buttonText = null)
        {
            string rv = "";
            if (buttonType == ButtonTypesEnum.Link)
            {
                rv = $"<a style='color:blue;cursor:pointer' href='/_Framework/GetFile/{fileID}'>{buttonText}</a>";
            }
            if (buttonType == ButtonTypesEnum.Button)
            {
                rv = $"<a class='layui-btn layui-btn-primary layui-btn-xs' href='/_Framework/GetFile/{fileID}'>{buttonText}</a>";
            }
            return rv;
        }

        public string MakeCheckBox(bool ischeck, string text = null, string name = null, string value = null, bool isReadOnly = false)
        {
            var disable = isReadOnly ? " disabled=\"\" class=\"layui-disabled\"" : " ";
            var selected = ischeck ? " checked" : " ";
            return $@"<input lay-skin=""primary"" type=""checkbox"" name=""{name ?? ""}"" id=""{name ?? Utils.GetIdByName(name)}"" value=""{value ?? ""}"" title=""{text ?? ""}"" {selected} {disable}/>";
        }

        public string MakeRadio(bool ischeck, string text = null, string name = null, string value = null, bool isReadOnly = false)
        {
                var selected = ischeck ? " checked" : " ";
            var disable = isReadOnly ? " disabled=\"\" class=\"layui-disabled\"" : " ";
                return $@"<input lay-skin=""primary"" type=""radio"" name=""{name ?? ""}"" id=""{name ?? Utils.GetIdByName(name)}"" value=""{value ?? ""}"" title=""{text ?? ""}"" {selected} {disable}/>";
        }

        public string MakeCombo(string name = null, List<ComboSelectListItem> value = null, string selectedValue = null, string emptyText = null, bool isReadOnly = false)
        {
            var disable = isReadOnly ? " disabled=\"\" class=\"layui-disabled\"" : " ";
            string rv = $"<select name=\"{name}\" lay-ignore>";
            if(string.IsNullOrEmpty(emptyText) == false)
            {
                rv += $@"
<option value=''>{emptyText}</option>";
            }
            if (value != null)
            {
                foreach (var item in value)
                {
                    rv += $@"
<option value='{item.Value}'>{item.Text}</option>";
                }
            }
            rv += $@"
</select>
";
            return rv;
        }


        public string MakeRedirectButton(ButtonTypesEnum buttonType, string url, string buttonText)
        {
            return "";
        }

        public string MakeViewButton(ButtonTypesEnum buttonType, Guid fileID, string buttonText = null, int? width = null, int? height = null, string title = null,  bool resizable = true)
        {
            return MakeDialogButton(buttonType, $"/_Framework/ViewFile/{fileID}", buttonText, width, height, title, null, true, resizable);
        }

        public string MakeScriptButton(ButtonTypesEnum buttonType, string url, int? width, int? height, string windowID, string buttonText, string title = null, string buttonID = null, string script = "")
        {
            return "";
        }
    }
}
