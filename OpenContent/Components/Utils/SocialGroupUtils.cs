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

namespace Satrabel.OpenContent.Components
{
    public static class SocialGroupUtils
    {
        #region View fitering
        /// <summary>
        /// get the group for extending the details en edit url's
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static string GetSocialGroupParameter(Manifest.Manifest manifest, NameValueCollection queryString)
        {
            string groupquery = null;
            if (manifest.GetSocialGroupFilter())
            {
                if (queryString?["groupid"] != null)
                {
                    groupquery = "groupid=" + queryString?["groupid"];
                }
            }
            return groupquery;
        }

        /// <summary>
        /// create filter additional for groups, filter on groupid (userroles)
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="filterQuery"></param>
        /// <param name="queryString"></param>
        public static void AddSocialGroupQueryFilter(Manifest.Manifest manifest, JObject filterQuery, NameValueCollection queryString)
        {
            if (manifest.GetSocialGroupFilter())
            {
                var UserRolesFilter = filterQuery["Filter"] as JObject;
                JArray UserRoles = (JArray)UserRolesFilter["userroles"];
                if (queryString?["groupid"] != null)
                {
                    UserRoles.Add(queryString?["groupid"]);
                }
                else
                {
                    // in the context of social groups, we don't want to see any items when there is no groupid, so we mock an non-existing userroles value
                    var dummy = Guid.NewGuid().ToString();
                    UserRoles.Add(dummy);
                }
            }
        }

        #endregion

        #region Edit functions
        public static bool AllowEditAdd(OpenContentModuleConfig config)
        {
            if (config.Settings.Manifest.GetSocialGroupFilter())
            {
                NameValueCollection queryString = HttpContext.Current.Request.QueryString;
                if (queryString?["groupid"] != null)
                {
                    if (config.Settings.Manifest.GetSocialGroupFilter())
                    {
                        int roleid = -1;
                        Int32.TryParse(queryString?["groupid"], out roleid);
                        var role = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(config.PortalId, roleid);
                        return config.IsInRole(role.RoleName);
                    }
                }
                return false;
            }
            return true;
        }

        internal static bool HasSocialGroupEditPermissions(int portalId, string groupId, int userId)
        {
            int roleid = -1;
            Int32.TryParse(groupId, out roleid);

            var role = DotNetNuke.Security.Roles.RoleController.Instance.GetUserRole(portalId, userId, roleid);

            if (role != null)
            {
                return true;
            }
            return false;

        }


        /// <summary>
        /// checks wether the user is member and thus can post in the social group
        /// </summary>
        /// <param name="dsContext"></param>
        /// <param name="manifest"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        internal static bool HasSocialGroupCreateItemPermissions(DataSourceContext dsContext, Manifest.Manifest manifest, JObject json)
        {
            if (manifest.GetSocialGroupFilter())
            {
                JObject formdata = (JObject)json["form"];
                int roleid = -1;
                if (Int32.TryParse(formdata["userroles"].ToString(), out roleid))
                {
                    var role = DotNetNuke.Security.Roles.RoleController.Instance.GetUserRole(dsContext.PortalId, dsContext.UserId, roleid);

                    if (role != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        #endregion
    }
}

