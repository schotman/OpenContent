﻿using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.TemplateHelpers;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Lucene.Config;
using UserRoleInfo = Satrabel.OpenContent.Components.Querying.UserRoleInfo;
using DotNetNuke.UI.Skins;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Web;
using DotNetNuke.Services.Journal;
using System.Reflection;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Common;
using System.Security;
using DotNetNuke.Entities.Urls;
using Satrabel.OpenContent.Components.Handlebars;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;

namespace Satrabel.OpenContent.Components
{
    public static class JournalUtils
    {
        /// <summary>
        /// get's the type of the journal entry from manifest
        /// </summary>
        /// <param name="manifest"></param>
        /// <returns></returns>
        internal static JournalTypeInfo GetJournalTypeInfo(Manifest.Manifest manifest)
        {
            var journalTypeName = ManifestUtils.GetJournalTypeName(manifest);
            if (journalTypeName != "")
            {
                return JournalController.Instance.GetJournalType(journalTypeName);
            }
            return null;
        }
        internal static string GetJournalItemContentTitle(Manifest.Manifest manifest, JToken data)
        {
            string content = data["Title"]?.ToString() ?? "";
            string titleTemplate = ManifestUtils.GetJournalContentTitle(manifest);
            if (titleTemplate != "")
            {
                HandlebarsEngine hbEngine = new HandlebarsEngine();
                content = hbEngine.Execute(titleTemplate, data);
            }
            return content;
        }
        internal static string GetJournalItemContent(Manifest.Manifest manifest, JToken data)
        {
            string content = data["Title"]?.ToString() ?? "";
            string contentTemplate = ManifestUtils.GetJournalContent(manifest);

            if (contentTemplate != "")
            {
                HandlebarsEngine hbEngine = new HandlebarsEngine();
                content = hbEngine.Execute(contentTemplate, data);
            }
            return content;
        }

        private static string GetItemSecurity(Manifest.Manifest manifest, JToken data)
        {
            string securitySet = "U";

            // check for view permissions on the item based on whether the item is still in draft mode
            if (data["publishstatus"] != null)
            {
                string publishStatus = data["publishstatus"].ToString();

                if (publishStatus == "draft")
                {
                    return securitySet;
                }
            }
            // check for view permissions on the item through the userroles property
            string groupId = null;
            if (data["userroles"] != null)
            {
                groupId = data["userroles"].ToString();
            }
            if (groupId == null || groupId == "" || groupId == "all")
            {
                if (ManifestUtils.GetJournalAudience(manifest) == "Community")
                {
                    securitySet += ",C";
                }
                else
                {
                    securitySet += ",E";
                }
            }
            else
            {
                securitySet += ",R";
            }

            return securitySet;
        }

        /// <summary>
        /// Add an entry to the journal
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="context"></param>
        /// <param name="data"></param>
        internal static void CreateJournalItem(Manifest.Manifest manifest, DataSourceContext context, JToken data)
        {
            var journalItemType = GetJournalTypeInfo(manifest);

            if (journalItemType != null)
            {
                ModuleInfo module = ModuleController.Instance.GetModule(context.TabId, context.ModuleId, false);
                string taburl = Globals.ApplicationURL(context.TabId).Replace("~/", "");
                string securitySet = GetItemSecurity(manifest, data);

                // reference key to connect OC item with journal item
                string journalObjectKey = "OpenContent" + context.Id.ToString(); 
                // used in the default templates see journal SharedResources.resx
                string title = GetJournalItemContentTitle(manifest, data);
                string summary = GetJournalItemContent(manifest, data);
                string description = summary;

                int groupId = -1;
                if (securitySet == "U")
                {
                    // Hack to prevent showing draft items to anyone but the user, to prevent draft post to become visisble
                    groupId = 1; // must be > 0
                }
                else if (securitySet.Contains("R"))
                {
                    int.TryParse(data["userroles"].ToString(), out groupId);
                    // refer to the group
                    taburl = taburl + "&groupid=" + groupId.ToString();
                }

                ItemData itemData = new ItemData()
                {
                    Title = title,
                    Description = description,
                    Url = "/" + taburl + "&id=" + context.Id
                };

                JournalItem journalItem = new JournalItem()
                {
                    UserId = context.UserId,
                    ProfileId = context.UserId,
                    SecuritySet = securitySet,
                    PortalId = context.PortalId,
                    Title = title,
                    Summary = summary,
                    JournalTypeId = journalItemType.JournalTypeId,
                    ObjectKey = journalObjectKey,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    SocialGroupId = groupId,
                    ItemData = itemData
                };
                JournalController.Instance.SaveJournalItem(journalItem, module);
                // toDo
                // publish date time

                // correct security set, not sure why, dnn does not use the socialgroupid in the creation of the journal item
                if (securitySet.Contains("R") && !journalItem.SecuritySet.Contains("R") && groupId > 0)
                {
                    string CorrectedSecuritySet = journalItem.SecuritySet;
                    CorrectedSecuritySet = CorrectedSecuritySet + "," + "R" + groupId.ToString();
                    journalItem.SecuritySet = CorrectedSecuritySet;
                    JournalController.Instance.UpdateJournalItem(journalItem, module);

                }

            }
        }
        internal static void UpdateJournalItem(Manifest.Manifest manifest, IDataItem dItem, DataSourceContext context, JToken data)
        {
            var journalItemType = GetJournalTypeInfo(manifest);

            if (journalItemType != null)
            {
                ModuleInfo module = ModuleController.Instance.GetModule(context.TabId, context.ModuleId, false);

                string journalObjectKey = "OpenContent" + dItem.Id.ToString();
                var journalItem = JournalController.Instance.GetJournalItemByKey(context.PortalId, journalObjectKey);

                // update security
                string securitySet = GetItemSecurity(manifest, data);

                string title = GetJournalItemContentTitle(manifest, data);
                string summary = GetJournalItemContent(manifest, data);
                string description = summary;

                if (journalItem != null)
                {
                    journalItem.UserId = context.UserId;
                    journalItem.Title = title;
                    journalItem.Summary = summary;
                    journalItem.DateUpdated = DateTime.Now;
                    journalItem.ItemData.Title = title;
                    journalItem.ItemData.Description = description;
                    journalItem.SecuritySet = securitySet;
                    JournalController.Instance.UpdateJournalItem(journalItem, module);
                }
                else
                {
                    // should not happen (unless some template or manifest change)
                    context.Id = dItem.Id;
                    CreateJournalItem(manifest, context, data);
                }

               
            }

        }
        internal static void DeleteJournalItem(Manifest.Manifest manifest, DataSourceContext context, IDataItem dItem)
        {
            var journalItemType = GetJournalTypeInfo(manifest);

            if (journalItemType != null)
            {
                string journalObjectKey = "OpenContent" + dItem.Id.ToString();
                JournalController.Instance.DeleteJournalItemByKey(context.PortalId, journalObjectKey);
            }

        }
    }
}