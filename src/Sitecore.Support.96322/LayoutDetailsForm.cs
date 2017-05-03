using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Shell.Applications.Dialogs;
using Sitecore.Shell.Applications.Dialogs.LayoutDetails;
using Sitecore.Shell.Applications.Layouts.DeviceEditor;
using Sitecore.Shell.Framework;
using Sitecore.Shell.Web.UI;
using Sitecore.Sites;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Sitecore.Xml;
using System;
using System.Collections.Specialized;
using System.Xml;

namespace Sitecore.Support.Shell.Applications.ContentManager.Dialogs.LayoutDetails
{
    /// <summary>
    /// Represents a gallery layout form.
    /// </summary>
    public class LayoutDetailsForm : DialogForm
    {
        /// <summary>
        /// The tab enumeration.
        /// </summary>
        private enum TabType
        {
            /// <summary>
            /// The shared layout tab.
            /// </summary>
            Shared,
            /// <summary>
            /// The final layout tab.
            /// </summary>
            Final,
            /// <summary>
            /// The unknown tab.
            /// </summary>
            Unknown
        }

        /// <summary>
        /// The layout panel.
        /// </summary>
        protected Border LayoutPanel;

        /// <summary>
        /// The final layout panel.
        /// </summary>
        protected Border FinalLayoutPanel;

        /// <summary>
        /// The final layout warning panel.
        /// </summary>
        protected Border FinalLayoutNoVersionWarningPanel;

        /// <summary>
        /// The shared layout tab.
        /// </summary>
        protected Tab SharedLayoutTab;

        /// <summary>
        /// The final layout tab.
        /// </summary>
        protected Tab FinalLayoutTab;

        /// <summary>
        /// The tabs.
        /// </summary>
        protected Tabstrip Tabs;

        /// <summary>
        /// The title of the warning.
        /// </summary>
        protected Literal WarningTitle;

        /// <summary>
        /// Gets or sets the layout.
        /// </summary>
        /// <value>The layout.</value>
        public virtual string Layout
        {
            get
            {
                return StringUtil.GetString(base.ServerProperties["Layout"]);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                base.ServerProperties["Layout"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the final layout.
        /// </summary>
        /// <value>The final layout.</value>
        public virtual string FinalLayout
        {
            get
            {
                string layoutDelta = this.LayoutDelta;
                if (string.IsNullOrWhiteSpace(layoutDelta))
                {
                    return this.Layout;
                }
                if (string.IsNullOrWhiteSpace(this.Layout))
                {
                    return layoutDelta;
                }
                return XmlDeltas.ApplyDelta(this.Layout, layoutDelta);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                if (!string.IsNullOrWhiteSpace(this.Layout))
                {
                    this.LayoutDelta = (XmlUtil.XmlStringsAreEqual(this.Layout, value) ? null : XmlDeltas.GetDelta(value, this.Layout));
                    return;
                }
                this.LayoutDelta = value;
            }
        }

        /// <summary>
        /// Gets or sets the layout delta.
        /// </summary>
        /// <value>
        /// The layout delta.
        /// </value>
        protected virtual string LayoutDelta
        {
            get
            {
                return StringUtil.GetString(base.ServerProperties["LayoutDelta"]);
            }
            set
            {
                base.ServerProperties["LayoutDelta"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether version has been created.
        /// </summary>
        /// <value>
        /// The value indicating whether version has been created.
        /// </value>
        protected bool VersionCreated
        {
            get
            {
                return MainUtil.GetBool(base.ServerProperties["VersionCreated"], false);
            }
            set
            {
                base.ServerProperties["VersionCreated"] = value;
            }
        }

        /// <summary>
        /// Gets the current active tab.
        /// </summary>
        /// <value>
        /// The active tab.
        /// </value>
        private LayoutDetailsForm.TabType ActiveTab
        {
            get
            {
                int active = this.Tabs.Active;
                if (active == 0)
                {
                    return LayoutDetailsForm.TabType.Shared;
                }
                if (active == 1)
                {
                    return LayoutDetailsForm.TabType.Final;
                }
                return LayoutDetailsForm.TabType.Unknown;
            }
        }

        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            if (message.Name == "item:addversion")
            {
                Item currentItem = LayoutDetailsForm.GetCurrentItem();
                Dispatcher.Dispatch(message, currentItem);
                return;
            }
            base.HandleMessage(message);
        }

        /// <summary>
        /// Copy the device.
        /// </summary>
        /// <param name="deviceID">
        /// The device ID.
        /// </param>
        protected void CopyDevice(string deviceID)
        {
            Assert.ArgumentNotNullOrEmpty(deviceID, "deviceID");
            NameValueCollection nameValueCollection = new NameValueCollection();
            nameValueCollection.Add("deviceid", deviceID);
            Context.ClientPage.Start(this, "CopyDevicePipeline", nameValueCollection);
        }

        /// <summary>
        /// Copy the device pipeline.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        protected void CopyDevicePipeline(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.IsPostBack)
            {
                if (!string.IsNullOrEmpty(args.Result) && args.Result != "undefined")
                {
                    string[] array = args.Result.Split(new char[]
                    {
                        '^'
                    });
                    string @string = StringUtil.GetString(new string[]
                    {
                        args.Parameters["deviceid"]
                    });
                    ListString devices = new ListString(array[0]);
                    string text = array[1];
                    XmlDocument doc = this.GetDoc();
                    XmlNode xmlNode = doc.SelectSingleNode("/r/d[@id='" + @string + "']");
                    if (text == WebUtil.GetQueryString("id"))
                    {
                        if (xmlNode != null)
                        {
                            this.CopyDevice(xmlNode, devices);
                        }
                    }
                    else if (xmlNode != null)
                    {
                        Language language;
                        if (!Language.TryParse(WebUtil.GetQueryString("la"), out language))
                        {
                            language = Context.Language;
                        }
                        Sitecore.Data.Version version = Sitecore.Data.Version.Parse(WebUtil.GetQueryString("ve"));
                        Item itemNotNull = Client.GetItemNotNull(text, language, version);
                        this.CopyDevice(xmlNode, devices, itemNotNull);
                    }
                    this.Refresh();
                    return;
                }
            }
            else
            {
                XmlDocument doc2 = this.GetDoc();
                WebUtil.SetSessionValue("SC_DEVICEEDITOR", doc2.OuterXml);
                UrlString urlString = new UrlString(UIUtil.GetUri("control:CopyDeviceTo"));
                urlString["de"] = StringUtil.GetString(new string[]
                {
                    args.Parameters["deviceid"]
                });
                urlString["fo"] = WebUtil.GetQueryString("id");
                SheerResponse.ShowModalDialog(urlString.ToString(), "1200px", "700px", string.Empty, true);
                args.WaitForPostBack();
            }
        }

        /// <summary>
        /// Gets the layout value from the active tab.
        /// </summary>
        /// <returns>The layout value.</returns>
        protected string GetActiveLayout()
        {
            if (this.ActiveTab == LayoutDetailsForm.TabType.Final)
            {
                return this.FinalLayout;
            }
            return this.Layout;
        }

        /// <summary>
        /// Gets the dialog result.
        /// </summary>
        /// <returns>An aggregated XML with the both layouts shared and final.</returns>
        protected string GetDialogResult()
        {
            return new LayoutDetailsDialogResult
            {
                Layout = this.Layout,
                FinalLayout = this.FinalLayout,
                VersionCreated = this.VersionCreated
            }.ToString();
        }

        /// <summary>
        /// Sets the layout value on the active tab.
        /// </summary>
        /// <param name="value">The value.</param>
        protected void SetActiveLayout(string value)
        {
            if (this.ActiveTab == LayoutDetailsForm.TabType.Final)
            {
                this.FinalLayout = value;
                return;
            }
            this.Layout = value;
        }

        /// <summary>
        /// Edits the placeholder.
        /// </summary>
        /// <param name="deviceID">
        /// The device ID.
        /// </param>
        /// <param name="uniqueID">
        /// The unique ID.
        /// </param>
        protected void EditPlaceholder(string deviceID, string uniqueID)
        {
            Assert.ArgumentNotNull(deviceID, "deviceID");
            Assert.ArgumentNotNullOrEmpty(uniqueID, "uniqueID");
            NameValueCollection nameValueCollection = new NameValueCollection();
            nameValueCollection.Add("deviceid", deviceID);
            nameValueCollection.Add("uniqueid", uniqueID);
            Context.ClientPage.Start(this, "EditPlaceholderPipeline", nameValueCollection);
        }

        /// <summary>
        /// Edits the placeholder pipeline.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        protected void EditPlaceholderPipeline(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            XmlDocument doc = this.GetDoc();
            LayoutDefinition layoutDefinition = LayoutDefinition.Parse(doc.OuterXml);
            DeviceDefinition device = layoutDefinition.GetDevice(args.Parameters["deviceid"]);
            PlaceholderDefinition placeholder = device.GetPlaceholder(args.Parameters["uniqueid"]);
            if (placeholder == null)
            {
                return;
            }
            if (args.IsPostBack)
            {
                if (!string.IsNullOrEmpty(args.Result) && args.Result != "undefined")
                {
                    string text;
                    Item item = SelectPlaceholderSettingsOptions.ParseDialogResult(args.Result, Client.ContentDatabase, out text);
                    if (item != null)
                    {
                        //Sitecore.Support.96322
                        //modified from placeholder.MetaDataItemId = item.Paths.FullPath; to
                        placeholder.MetaDataItemId = item.ID.ToString();
                    }
                    if (!string.IsNullOrEmpty(text))
                    {
                        placeholder.Key = text;
                    }
                    this.SetActiveLayout(layoutDefinition.ToXml());
                    this.Refresh();
                    return;
                }
            }
            else
            {
                Item item2 = string.IsNullOrEmpty(placeholder.MetaDataItemId) ? null : Client.ContentDatabase.GetItem(placeholder.MetaDataItemId);
                SelectPlaceholderSettingsOptions selectPlaceholderSettingsOptions = new SelectPlaceholderSettingsOptions
                {
                    TemplateForCreating = null,
                    PlaceholderKey = placeholder.Key,
                    CurrentSettingsItem = item2,
                    SelectedItem = item2,
                    IsPlaceholderKeyEditable = true
                };
                SheerResponse.ShowModalDialog(selectPlaceholderSettingsOptions.ToUrlString().ToString(), "460px", "460px", string.Empty, true);
                args.WaitForPostBack();
            }
        }

        /// <summary>
        /// Edits the rendering.
        /// </summary>
        /// <param name="deviceID">
        /// The device ID.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        protected void EditRendering(string deviceID, string index)
        {
            Assert.ArgumentNotNull(deviceID, "deviceID");
            Assert.ArgumentNotNull(index, "index");
            NameValueCollection nameValueCollection = new NameValueCollection();
            nameValueCollection.Add("deviceid", deviceID);
            nameValueCollection.Add("index", index);
            Context.ClientPage.Start(this, "EditRenderingPipeline", nameValueCollection);
        }

        /// <summary>
        /// Edits the rendering pipeline.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        protected void EditRenderingPipeline(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            RenderingParameters renderingParameters = new RenderingParameters();
            renderingParameters.Args = args;
            renderingParameters.DeviceId = StringUtil.GetString(new string[]
            {
                args.Parameters["deviceid"]
            });
            renderingParameters.SelectedIndex = MainUtil.GetInt(StringUtil.GetString(new string[]
            {
                args.Parameters["index"]
            }), 0);
            renderingParameters.Item = UIUtil.GetItemFromQueryString(Client.ContentDatabase);
            if (!args.IsPostBack)
            {
                XmlDocument doc = this.GetDoc();
                WebUtil.SetSessionValue("SC_DEVICEEDITOR", doc.OuterXml);
            }
            if (renderingParameters.Show())
            {
                XmlDocument doc2 = XmlUtil.LoadXml(WebUtil.GetSessionString("SC_DEVICEEDITOR"));
                WebUtil.SetSessionValue("SC_DEVICEEDITOR", null);
                this.SetActiveLayout(LayoutDetailsForm.GetLayoutValue(doc2));
                this.Refresh();
            }
        }

        /// <summary>
        /// Raises the load event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="T:System.EventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>
        /// This method notifies the server control that it should perform actions common to each HTTP
        /// request for the page it is associated with, such as setting up a database query. At this
        /// stage in the page lifecycle, server controls in the hierarchy are created and initialized,
        /// view state is restored, and form controls reflect client-side data. Use the IsPostBack
        /// property to determine whether the page is being loaded in response to a client postback,
        /// or if it is being loaded and accessed for the first time.
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            Assert.CanRunApplication("Content Editor/Ribbons/Chunks/Layout");
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            this.Tabs.OnChange += delegate (object sender, EventArgs args)
            {
                this.Refresh();
            };
            if (!Context.ClientPage.IsEvent)
            {
                Item currentItem = LayoutDetailsForm.GetCurrentItem();
                Assert.IsNotNull(currentItem, "Item not found");
                this.Layout = LayoutField.GetFieldValue(currentItem.Fields[FieldIDs.LayoutField]);
                Field field = currentItem.Fields[FieldIDs.FinalLayoutField];
                if (currentItem.Name != "__Standard Values")
                {
                    string arg_B4_1;
                    if ((arg_B4_1 = field.GetValue(false, false)) == null)
                    {
                        arg_B4_1 = (field.GetInheritedValue(false) ?? field.GetValue(false, false, true, false, false));
                    }
                    this.LayoutDelta = arg_B4_1;
                }
                else
                {
                    this.LayoutDelta = field.GetStandardValue();
                }
                this.ToggleVisibilityOfControlsOnFinalLayoutTab(currentItem);
                this.Refresh();
            }
            SiteContext site = Context.Site;
            if (site == null)
            {
                return;
            }
            site.Notifications.ItemSaved += new ItemSavedDelegate(this.ItemSavedNotification);
        }

        /// <summary>
        /// Toggles the visibility of controls on final layout tab.
        /// </summary>
        /// <param name="item">The item.</param>
        protected void ToggleVisibilityOfControlsOnFinalLayoutTab(Item item)
        {
            bool flag = item.Versions.Count > 0;
            this.FinalLayoutPanel.Visible = flag;
            this.FinalLayoutNoVersionWarningPanel.Visible = !flag;
            if (!flag)
            {
                this.WarningTitle.Text = string.Format(Translate.Text("The current item does not have a version in \"{0}\"."), item.Language.GetDisplayName());
            }
        }

        /// <summary>
        /// Handles a click on the OK button.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        /// <remarks>
        /// When the user clicks OK, the dialog is closed by calling
        /// the <see cref="M:Sitecore.Web.UI.Sheer.ClientResponse.CloseWindow">CloseWindow</see> method.
        /// </remarks>
        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            SheerResponse.SetDialogValue(this.GetDialogResult());
            base.OnOK(sender, args);
        }

        /// <summary>
        /// Opens the device.
        /// </summary>
        /// <param name="deviceID">
        /// The device ID.
        /// </param>
        protected void OpenDevice(string deviceID)
        {
            Assert.ArgumentNotNullOrEmpty(deviceID, "deviceID");
            NameValueCollection nameValueCollection = new NameValueCollection();
            nameValueCollection.Add("deviceid", deviceID);
            Context.ClientPage.Start(this, "OpenDevicePipeline", nameValueCollection);
        }

        /// <summary>
        /// Opens the device pipeline.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        protected void OpenDevicePipeline(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.IsPostBack)
            {
                if (!string.IsNullOrEmpty(args.Result) && args.Result != "undefined")
                {
                    XmlDocument xmlDocument = XmlUtil.LoadXml(WebUtil.GetSessionString("SC_DEVICEEDITOR"));
                    WebUtil.SetSessionValue("SC_DEVICEEDITOR", null);
                    if (xmlDocument != null)
                    {
                        this.SetActiveLayout(LayoutDetailsForm.GetLayoutValue(xmlDocument));
                    }
                    else
                    {
                        this.SetActiveLayout(string.Empty);
                    }
                    this.Refresh();
                    return;
                }
            }
            else
            {
                XmlDocument doc = this.GetDoc();
                WebUtil.SetSessionValue("SC_DEVICEEDITOR", doc.OuterXml);
                UrlString urlString = new UrlString(UIUtil.GetUri("control:DeviceEditor"));
                urlString.Append("de", StringUtil.GetString(new string[]
                {
                    args.Parameters["deviceid"]
                }));
                urlString.Append("id", WebUtil.GetQueryString("id"));
                urlString.Append("vs", WebUtil.GetQueryString("vs"));
                urlString.Append("la", WebUtil.GetQueryString("la"));
                Context.ClientPage.ClientResponse.ShowModalDialog(new ModalDialogOptions(urlString.ToString())
                {
                    Response = true,
                    Width = "700"
                });
                args.WaitForPostBack();
            }
        }

        /// <summary>
        /// Copies the device2.
        /// </summary>
        /// <param name="sourceDevice">
        /// The device node.
        /// </param>
        /// <param name="devices">
        /// The devices.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        private void CopyDevice(XmlNode sourceDevice, ListString devices, Item item)
        {
            Assert.ArgumentNotNull(sourceDevice, "sourceDevice");
            Assert.ArgumentNotNull(devices, "devices");
            Assert.ArgumentNotNull(item, "item");
            Field layoutField = this.GetLayoutField(item);
            LayoutField layoutField2 = layoutField;
            XmlDocument data = layoutField2.Data;
            LayoutDetailsForm.CopyDevices(data, devices, sourceDevice);
            item.Editing.BeginEdit();
            layoutField.Value = data.OuterXml;
            item.Editing.EndEdit();
        }

        /// <summary>
        /// Called when the item is saved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        private void ItemSavedNotification(object sender, ItemSavedEventArgs args)
        {
            this.VersionCreated = true;
            this.ToggleVisibilityOfControlsOnFinalLayoutTab(args.Item);
            SheerResponse.SetDialogValue(this.GetDialogResult());
        }

        /// <summary>
        /// Copies the devices.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="devices">
        /// The devices.
        /// </param>
        /// <param name="sourceDevice">
        /// The source device.
        /// </param>
        private static void CopyDevices(XmlDocument doc, ListString devices, XmlNode sourceDevice)
        {
            Assert.ArgumentNotNull(doc, "doc");
            Assert.ArgumentNotNull(devices, "devices");
            Assert.ArgumentNotNull(sourceDevice, "sourceDevice");
            XmlNode xmlNode = doc.ImportNode(sourceDevice, true);
            foreach (string current in devices)
            {
                if (doc.DocumentElement != null)
                {
                    XmlNode xmlNode2 = doc.DocumentElement.SelectSingleNode("d[@id='" + current + "']");
                    if (xmlNode2 != null)
                    {
                        XmlUtil.RemoveNode(xmlNode2);
                    }
                    xmlNode2 = xmlNode.CloneNode(true);
                    XmlUtil.SetAttribute("id", current, xmlNode2);
                    doc.DocumentElement.AppendChild(xmlNode2);
                }
            }
        }

        /// <summary>
        /// Gets the current item.
        /// </summary>
        /// <returns>
        /// The current item.
        /// </returns>
        /// <contract>
        ///   <ensures condition="nullable" />
        /// </contract>
        private static Item GetCurrentItem()
        {
            string queryString = WebUtil.GetQueryString("id");
            Language language = Language.Parse(WebUtil.GetQueryString("la"));
            Sitecore.Data.Version version = Sitecore.Data.Version.Parse(WebUtil.GetQueryString("vs"));
            return Client.ContentDatabase.GetItem(queryString, language, version);
        }

        /// <summary>
        /// Gets the layout value.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <returns>
        /// The layout value.
        /// </returns>
        /// <contract>
        ///   <requires name="doc" condition="not null" />
        ///   <ensures condition="not null" />
        /// </contract>
        private static string GetLayoutValue(XmlDocument doc)
        {
            Assert.ArgumentNotNull(doc, "doc");
            XmlNodeList xmlNodeList = doc.SelectNodes("/r/d");
            if (xmlNodeList == null || xmlNodeList.Count == 0)
            {
                return string.Empty;
            }
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                if (xmlNode.ChildNodes.Count > 0 || XmlUtil.GetAttribute("l", xmlNode).Length > 0)
                {
                    return doc.OuterXml;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Copies the device.
        /// </summary>
        /// <param name="sourceDevice">
        /// The source device.
        /// </param>
        /// <param name="devices">
        /// The devices.
        /// </param>
        private void CopyDevice(XmlNode sourceDevice, ListString devices)
        {
            Assert.ArgumentNotNull(sourceDevice, "sourceDevice");
            Assert.ArgumentNotNull(devices, "devices");
            XmlDocument xmlDocument = XmlUtil.LoadXml(this.GetActiveLayout());
            LayoutDetailsForm.CopyDevices(xmlDocument, devices, sourceDevice);
            this.SetActiveLayout(xmlDocument.OuterXml);
        }

        /// <summary>
        /// Gets the doc.
        /// </summary>
        /// <returns>
        /// The doc.
        /// </returns>
        /// <contract>
        ///   <ensures condition="not null" />
        /// </contract>
        private XmlDocument GetDoc()
        {
            XmlDocument xmlDocument = new XmlDocument();
            string activeLayout = this.GetActiveLayout();
            if (activeLayout.Length > 0)
            {
                xmlDocument.LoadXml(activeLayout);
            }
            else
            {
                xmlDocument.LoadXml("<r/>");
            }
            return xmlDocument;
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        private void Refresh()
        {
            string activeLayout = this.GetActiveLayout();
            Control renderingContainer = (this.ActiveTab == LayoutDetailsForm.TabType.Final) ? this.FinalLayoutPanel : this.LayoutPanel;
            this.RenderLayoutGridBuilder(activeLayout, renderingContainer);
        }

        /// <summary>
        /// Renders the LayoutGridBuilder.
        /// </summary>
        /// <param name="layoutValue">The layout value.</param>
        /// <param name="renderingContainer">The rendering container.</param>
        private void RenderLayoutGridBuilder(string layoutValue, Control renderingContainer)
        {
            string iD = renderingContainer.ID + "LayoutGrid";
            LayoutGridBuilder layoutGridBuilder = new LayoutGridBuilder
            {
                ID = iD,
                Value = layoutValue,
                EditRenderingClick = "EditRendering(\"$Device\", \"$Index\")",
                EditPlaceholderClick = "EditPlaceholder(\"$Device\", \"$UniqueID\")",
                OpenDeviceClick = "OpenDevice(\"$Device\")",
                CopyToClick = "CopyDevice(\"$Device\")"
            };
            renderingContainer.Controls.Clear();
            layoutGridBuilder.BuildGrid(renderingContainer);
            if (Context.ClientPage.IsEvent)
            {
                SheerResponse.SetOuterHtml(renderingContainer.ID, renderingContainer);
                SheerResponse.Eval("if (!scForm.browser.isIE) { scForm.browser.initializeFixsizeElements(); }");
            }
        }

        /// <summary>
        /// Gets the layout field.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private Field GetLayoutField(Item item)
        {
            if (this.ActiveTab == LayoutDetailsForm.TabType.Final)
            {
                return item.Fields[FieldIDs.FinalLayoutField];
            }
            return item.Fields[FieldIDs.LayoutField];
        }
    }
}
