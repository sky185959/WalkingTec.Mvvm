﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Mvc.Admin.ViewModels.FrameworkMenuVMs;

namespace WalkingTec.Mvvm.Mvc.Admin.ViewModels.FrameworkGroupVMs
{
    public class FrameworkGroupMDVM : BaseCRUDVM<FrameworkGroup>
    {
        public FrameworkMenuListVM ListVM { get; set; }

        public FrameworkGroupMDVM()
        {
            ListVM = new FrameworkMenuListVM();
        }

        protected override void InitVM()
        {
            ListVM.Searcher.RoleID = Entity.ID;
        }

        public bool DoChange()
        {
        var all = FC.Where(x => x.Key.StartsWith("menu_")).ToList();
            List<Guid> AllowedMenuIds = all.Where(x => x.Value.ToString() == "1").Select(x=> Guid.Parse(x.Key.Replace("menu_",""))).ToList();
            List<Guid> DeniedMenuIds = all.Where(x => x.Value.ToString() == "2").Select(x => Guid.Parse(x.Key.Replace("menu_", ""))).ToList();
            List<Guid> DefaultMenuIds = all.Where(x => x.Value.ToString() == "0").Select(x => Guid.Parse(x.Key.Replace("menu_", ""))).ToList();
            var torem = AllowedMenuIds.Concat(DeniedMenuIds).Concat(DefaultMenuIds).Distinct();
            var oldIDs = DC.Set<FunctionPrivilege>().Where(x => x.RoleId == Entity.ID && torem.Contains(x.MenuItemId)).Select(x => x.ID).ToList();

            foreach (var oldid in oldIDs)
            {
                FunctionPrivilege fp = new FunctionPrivilege { ID = oldid };
                DC.Set<FunctionPrivilege>().Attach(fp);
                DC.DeleteEntity(fp);
            }
            foreach (var menuid in AllowedMenuIds)
            {
                FunctionPrivilege fp = new FunctionPrivilege();
                fp.MenuItemId = menuid;
                fp.RoleId = Entity.ID;
                fp.UserId = null;
                fp.Allowed = true;
                DC.Set<FunctionPrivilege>().Add(fp);
            }
            foreach (var menuid in DeniedMenuIds)
            {
                FunctionPrivilege fp = new FunctionPrivilege();
                fp.MenuItemId = menuid;
                fp.RoleId = Entity.ID;
                fp.UserId = null;
                fp.Allowed = false;
                DC.Set<FunctionPrivilege>().Add(fp);
            }
            DC.SaveChanges();
            return true;
        }

    }
}
