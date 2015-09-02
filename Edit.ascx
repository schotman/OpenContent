<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.Edit" CodeBehind="Edit.ascx.cs" %>

<%@ Register TagPrefix="dnncl" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<dnncl:DnnCssInclude ID="customJS" runat="server" FilePath="~/DesktopModules/OpenContent/alpaca/css/alpaca-dnn.css" AddTag="false" />
<dnncl:DnnJsInclude ID="DnnJsInclude1" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/handlebars/handlebars.js" Priority="106" ForceProvider="DnnPageHeaderProvider" />
<dnncl:DnnJsInclude ID="DnnJsInclude2" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/alpaca/web/alpaca.js" Priority="107" ForceProvider="DnnPageHeaderProvider" />
<dnncl:DnnJsInclude ID="DnnJsInclude4" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/typeahead.js/dist/typeahead.bundle.min.js" Priority="106" ForceProvider="DnnPageHeaderProvider" />

<dnncl:DnnJsInclude ID="DnnJsInclude14" runat="server" FilePath="~/DesktopModules/OpenContent/js/wysihtml/wysihtml-toolbar.js" Priority="107" ForceProvider="DnnPageHeaderProvider" />
<dnncl:DnnJsInclude ID="DnnJsInclude6" runat="server" FilePath="~/DesktopModules/OpenContent/js/wysihtml/parser_rules/advanced_opencontent.js" Priority="107" ForceProvider="DnnPageHeaderProvider" />
<dnncl:DnnJsInclude ID="DnnJsInclude11" runat="server" FilePath="~/DesktopModules/OpenContent/alpaca/js/fields/dnn/CKEditorField.js" Priority="107" ForceProvider="DnnPageHeaderProvider" />
<dnncl:DnnJsInclude ID="DnnJsInclude12" runat="server" FilePath="~/DesktopModules/OpenContent/alpaca/js/fields/dnn/wysihtmlField.js" Priority="107" ForceProvider="DnnPageHeaderProvider" />
<dnncl:DnnJsInclude ID="DnnJsInclude13" runat="server" FilePath="~/DesktopModules/OpenContent/alpaca/js/fields/dnn/NumberField.js" Priority="107" ForceProvider="DnnPageHeaderProvider" />

<dnncl:DnnJsInclude ID="DnnJsInclude9" runat="server" FilePath="~/DesktopModules/OpenContent/alpaca/js/fields/dnn/ImageField.js" Priority="107" ForceProvider="DnnFormBottomProvider" />
<dnncl:DnnJsInclude ID="DnnJsInclude10" runat="server" FilePath="~/DesktopModules/OpenContent/alpaca/js/fields/dnn/FileField.js" Priority="107" ForceProvider="DnnFormBottomProvider" />
<dnncl:dnnjsinclude id="DnnJsInclude8" runat="server" filepath="~/DesktopModules/OpenContent/alpaca/js/fields/dnn/UrlField.js" priority="108" forceprovider="DnnFormBottomProvider" />
<dnncl:DnnJsInclude ID="DnnJsInclude7" runat="server" FilePath="~/DesktopModules/OpenContent/alpaca/js/views/dnndefault.js" Priority="107" ForceProvider="DnnFormBottomProvider" />
<dnncl:DnnJsInclude ID="DnnJsInclude3" runat="server" FilePath="~/DesktopModules/OpenContent/js/requirejs/require.js" Priority="110" ForceProvider="DnnFormBottomProvider" />
<dnncl:DnnJsInclude ID="DnnJsInclude5" runat="server" FilePath="~/DesktopModules/OpenContent/js/requirejs/config.js" Priority="111" ForceProvider="DnnFormBottomProvider" />
<dnncl:DnnCssInclude ID="DnnCssInclude1" runat="server" FilePath="~/DesktopModules/OpenContent/css/font-awesome/css/font-awesome.min.css" AddTag="false" />

<asp:Panel ID="ScopeWrapper" runat="server">
    <div id="field1" class="alpaca"></div>
    <ul class="dnnActions dnnClear" style="display: block; padding-left: 35%">
        <li>
            <asp:HyperLink ID="cmdSave" runat="server" class="dnnPrimaryAction" resourcekey="cmdSave" /></li>
        <li>
            <asp:HyperLink ID="hlCancel" runat="server" class="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
        <li style="padding-left: 10px;">
            <asp:DropDownList ID="ddlVersions" runat="server" ClientIDMode="Static" />
        </li>
    </ul>
</asp:Panel>
<asp:HiddenField ID="CKDNNporid" runat="server" ClientIDMode="Static" />

<script type="text/javascript">
    $(document).ready(function () {
        var itemId = "<%=Page.Request.QueryString["id"]%>";
        var windowTop = parent;
        var popup = windowTop.jQuery("#iPopUp");
        if (popup.length) {

            var $window = $(windowTop),
                            newHeight,
                            newWidth;

            newHeight = $window.height() - 46;
            newWidth = Math.min($window.width() - 40, 1100);

            popup.dialog("option", {
                close: function () { window.dnnModal.closePopUp(false, ""); },
                //'position': 'top',
                height: newHeight,
                width: newWidth,
                //position: 'center'
            });
            $("#<%=hlCancel.ClientID%>").click(function () {
                dnnModal.closePopUp(false, "");
                return false;
            });
            //$(popup).height(windowTop.innerHeight-111);
            //$(popup).css('height', windowTop.innerHeight + 100).dialog('option', );
            //$(popup).dialog({ position: 'center' });
        }

        var moduleScope = $('#<%=ScopeWrapper.ClientID %>'),
            self = moduleScope,
            sf = $.ServicesFramework(<%=ModuleId %>);

        var postData = {};
        var getData = "";
        var action = "Edit";
        if (itemId) getData = "id=" + itemId;
        $.ajax({
            type: "GET",
            url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
            data: getData,
            beforeSend: sf.setModuleHeaders
        }).done(function (config) {

            oc_loadmodules(config.options, function () {
                self.FormEdit(config);
            });

        }).fail(function (xhr, result, status) {
            alert("Uh-oh, something broke: " + status);
        });

        self.FormEdit = function (config) {
            var ConnectorClass = Alpaca.getConnectorClass("default");
            connector = new ConnectorClass("default");
            connector.servicesFramework = sf;
            connector.culture = '<%=CurrentCulture%>';
            connector.numberDecimalSeparator = '<%=NumberDecimalSeparator%>';
            if (config.versions) {
                $.each(config.versions, function (i, item) {
                    $("#<%=ddlVersions.ClientID%>").append($('<option>', {
                        value: item.ticks,
                        text: item.text
                    }));
                    //$("#<%=ddlVersions.ClientID%>").data(item.CreatedOnDate, item.Json);
                });
            }

            $.alpaca.setDefaultLocale(connector.culture.replace('-', '_'));
            self.CreateForm(connector, config, config.data);

        };

        self.CreateForm = function (connector, config, data) {

            $("#field1").alpaca({
                "schema": config.schema,
                "options": config.options,
                "data": data,
                "view": "dnn-edit",
                "connector": connector,
                "postRender": function (control) {
                    var selfControl = control;
                    $("#<%=cmdSave.ClientID%>").click(function () {
                        selfControl.refreshValidationState(true);
                        if (selfControl.isValid(true)) {
                            var value = selfControl.getValue();
                            //alert(JSON.stringify(value, null, "  "));
                            var href = $(this).attr('href');
                            self.FormSubmit(value, href);
                        }
                        return false;
                    });

                    $("#<%=ddlVersions.ClientID%>").change(function () {
                        //var versions = config.versions;
                        //var ver = $("#<%=ddlVersions.ClientID%>").data($(this).val());
                        //$("#field1").empty();
                        //$("#<%=cmdSave.ClientID%>").off("click");
                        //self.CreateForm(connector, config, ver);
                        //selfControl.setValue(ver);
                        self.Version(itemId, $(this).val(), control);
                        return false;
                    });

                }
            });

        };

        self.FormSubmit = function (data, href) {
            //var postData = { form: data };
            var postData = JSON.stringify({ form: data, id: itemId });
            var action = "Update"; //self.getUpdateAction();

            $.ajax({
                type: "POST",
                url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: postData,
                beforeSend: sf.setModuleHeaders
            }).done(function (data) {
                //alert('ok:' + data);
                //self.loadSettings();
                //window.location.href = href;

                var windowTop = parent; //needs to be assign to a varaible for Opera compatibility issues.
                var popup = windowTop.jQuery("#iPopUp");
                if (popup.length > 0) {
                    windowTop.__doPostBack('dnn_ctr<%=ModuleId %>_View__UP', '');
                    dnnModal.closePopUp(false, href);
                }
                else {
                    window.location.href = href;
                }
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
            });
        };

        self.Version = function (id, ticks, control) {
            if (!id) id = 0;
            var postData = { id: id, ticks: ticks };
            var action = "Version";

            $.ajax({
                type: "GET",
                url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
                //contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: postData,
                beforeSend: sf.setModuleHeaders
            }).done(function (data) {
                //alert('ok:' + data);
                control.setValue(data);
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
            });
        };

    });


</script>