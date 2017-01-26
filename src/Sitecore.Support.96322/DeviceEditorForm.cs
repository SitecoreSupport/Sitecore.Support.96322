using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Pipelines.RenderDeviceEditorRendering;
using Sitecore.Resources;
using Sitecore.Rules;
using Sitecore.SecurityModel;
using Sitecore.Shell.Applications.Dialogs;
using Sitecore.Shell.Applications.Dialogs.ItemLister;
using Sitecore.Shell.Applications.Dialogs.Personalize;
using Sitecore.Shell.Applications.Dialogs.Testing;
using Sitecore.Shell.Applications.Layouts.DeviceEditor;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XmlControls;
using System;
using System.Collections;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;

namespace Sitecore.Support.Shell.Applications.Layouts.DeviceEditor
{
    /// <summary>
    /// Represents the Device Editor form.
    /// </summary>
    [UsedImplicitly]
    public class DeviceEditorForm : DialogForm
    {
        /// <summary>
        ///   The command name.
        /// </summary>
        private const string CommandName = "device:settestdetails";

        /// <summary>
        /// Gets or sets the controls.
        /// </summary>
        /// <value>The controls.</value>
        public ArrayList Controls
        {
            get
            {
                return (ArrayList)Context.ClientPage.ServerProperties["Controls"];
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                Context.ClientPage.ServerProperties["Controls"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the device ID.
        /// </summary>
        /// <value>The device ID.</value>
        public string DeviceID
        {
            get
            {
                return StringUtil.GetString(Context.ClientPage.ServerProperties["DeviceID"]);
            }
            set
            {
                Assert.ArgumentNotNullOrEmpty(value, "value");
                Context.ClientPage.ServerProperties["DeviceID"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the index of the selected.
        /// </summary>
        /// <value>The index of the selected.</value>
        public int SelectedIndex
        {
            get
            {
                return MainUtil.GetInt(Context.ClientPage.ServerProperties["SelectedIndex"], -1);
            }
            set
            {
                Context.ClientPage.ServerProperties["SelectedIndex"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the unique id.
        /// </summary>
        /// <value>The unique id.</value>
        public string UniqueId
        {
            get
            {
                return StringUtil.GetString(Context.ClientPage.ServerProperties["PlaceholderUniqueID"]);
            }
            set
            {
                Assert.ArgumentNotNullOrEmpty(value, "value");
                Context.ClientPage.ServerProperties["PlaceholderUniqueID"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the layout.
        /// </summary>
        /// <value>The layout.</value>
        protected TreePicker Layout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the placeholders.
        /// </summary>
        /// <value>The placeholders.</value>
        protected Scrollbox Placeholders
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the renderings.
        /// </summary>
        /// <value>The renderings.</value>
        protected Scrollbox Renderings
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the test.
        /// </summary>
        /// <value>The test button.</value>
        protected Button Test
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the personalize button control.
        /// </summary>
        /// <value>The personalize button control.</value>
        protected Button Personalize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the edit.
        /// </summary>
        /// <value>The edit button.</value>
        protected Button btnEdit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the change.
        /// </summary>
        /// <value>The change button.</value>
        protected Button btnChange
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the remove.
        /// </summary>
        /// <value>The Remove button.</value>
        protected Button btnRemove
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the move up.
        /// </summary>
        /// <value>The Move Up button.</value>
        protected Button MoveUp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the move down.
        /// </summary>
        /// <value>The Move Down button.</value>
        protected Button MoveDown
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Edit placeholder button.
        /// </summary>
        /// <value>The Edit placeholder button.</value>
        protected Button phEdit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the phRemove button.
        /// </summary>
        /// <value>Remove place holder button.</value>
        protected Button phRemove
        {
            get;
            set;
        }

        /// <summary>
        /// Adds the specified arguments.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [UsedImplicitly, HandleMessage("device:add", true)]
        protected void Add(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    string[] array = args.Result.Split(new char[]
					{
						','
					});
                    string text = array[0];
                    string placeholder = array[1].Replace("-c-", ",");
                    bool flag = array[2] == "1";
                    LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
                    DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
                    RenderingDefinition renderingDefinition = new RenderingDefinition
                    {
                        ItemID = text,
                        Placeholder = placeholder
                    };
                    device.AddRendering(renderingDefinition);
                    DeviceEditorForm.SetDefinition(layoutDefinition);
                    this.Refresh();
                    if (flag)
                    {
                        ArrayList renderings = device.Renderings;
                        if (renderings != null)
                        {
                            this.SelectedIndex = renderings.Count - 1;
                            Context.ClientPage.SendMessage(this, "device:edit");
                        }
                    }
                    Registry.SetString("/Current_User/SelectRendering/Selected", text);
                    return;
                }
            }
            else
            {
                SelectRenderingOptions selectRenderingOptions = new SelectRenderingOptions
                {
                    ShowOpenProperties = true,
                    ShowPlaceholderName = true,
                    PlaceholderName = string.Empty
                };
                string @string = Registry.GetString("/Current_User/SelectRendering/Selected");
                if (!string.IsNullOrEmpty(@string))
                {
                    selectRenderingOptions.SelectedItem = Client.ContentDatabase.GetItem(@string);
                }
                string url = selectRenderingOptions.ToUrlString(Client.ContentDatabase).ToString();
                SheerResponse.ShowModalDialog(url, true);
                args.WaitForPostBack();
            }
        }

        /// <summary>
        /// Adds the specified arguments.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [UsedImplicitly, HandleMessage("device:addplaceholder", true)]
        protected void AddPlaceholder(ClientPipelineArgs args)
        {
            if (args.IsPostBack)
            {
                if (!string.IsNullOrEmpty(args.Result) && args.Result != "undefined")
                {
                    LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
                    DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
                    string text;
                    Item item = SelectPlaceholderSettingsOptions.ParseDialogResult(args.Result, Client.ContentDatabase, out text);
                    if (item == null || string.IsNullOrEmpty(text))
                    {
                        return;
                    }
                    PlaceholderDefinition placeholderDefinition = new PlaceholderDefinition
                    {
                        UniqueId = ID.NewID.ToString(),
                        // Sitecore.Support.96322
                        // MetaDataItemId = item.Paths.FullPath,
                        MetaDataItemId = item.ID.ToString(),
                        Key = text
                    };
                    device.AddPlaceholder(placeholderDefinition);
                    DeviceEditorForm.SetDefinition(layoutDefinition);
                    this.Refresh();
                    return;
                }
            }
            else
            {
                SelectPlaceholderSettingsOptions selectPlaceholderSettingsOptions = new SelectPlaceholderSettingsOptions
                {
                    IsPlaceholderKeyEditable = true
                };
                SheerResponse.ShowModalDialog(selectPlaceholderSettingsOptions.ToUrlString().ToString(), "460px", "460px", string.Empty, true);
                args.WaitForPostBack();
            }
        }

        /// <summary>
        /// Adds the specified arguments.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [UsedImplicitly, HandleMessage("device:change", true)]
        protected void Change(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (this.SelectedIndex < 0)
            {
                return;
            }
            LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
            DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
            ArrayList renderings = device.Renderings;
            if (renderings == null)
            {
                return;
            }
            RenderingDefinition renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
            if (renderingDefinition == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(renderingDefinition.ItemID))
            {
                return;
            }
            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    string[] array = args.Result.Split(new char[]
					{
						','
					});
                    renderingDefinition.ItemID = array[0];
                    bool flag = array[2] == "1";
                    DeviceEditorForm.SetDefinition(layoutDefinition);
                    this.Refresh();
                    if (flag)
                    {
                        Context.ClientPage.SendMessage(this, "device:edit");
                        return;
                    }
                }
            }
            else
            {
                SelectRenderingOptions selectRenderingOptions = new SelectRenderingOptions
                {
                    ShowOpenProperties = true,
                    ShowPlaceholderName = false,
                    PlaceholderName = string.Empty,
                    SelectedItem = Client.ContentDatabase.GetItem(renderingDefinition.ItemID)
                };
                string url = selectRenderingOptions.ToUrlString(Client.ContentDatabase).ToString();
                SheerResponse.ShowModalDialog(url, true);
                args.WaitForPostBack();
            }
        }

        /// <summary>
        /// Edits the specified arguments.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [UsedImplicitly, HandleMessage("device:edit", true)]
        protected void Edit(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            RenderingParameters renderingParameters = new RenderingParameters
            {
                Args = args,
                DeviceId = this.DeviceID,
                SelectedIndex = this.SelectedIndex,
                Item = UIUtil.GetItemFromQueryString(Client.ContentDatabase)
            };
            if (renderingParameters.Show())
            {
                this.Refresh();
            }
        }

        /// <summary>
        /// Edits the placeholder.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [UsedImplicitly, HandleMessage("device:editplaceholder", true)]
        protected void EditPlaceholder(ClientPipelineArgs args)
        {
            if (string.IsNullOrEmpty(this.UniqueId))
            {
                return;
            }
            LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
            DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
            PlaceholderDefinition placeholder = device.GetPlaceholder(this.UniqueId);
            if (placeholder == null)
            {
                return;
            }
            if (args.IsPostBack)
            {
                if (!string.IsNullOrEmpty(args.Result) && args.Result != "undefined")
                {
                    string key;
                    Item item = SelectPlaceholderSettingsOptions.ParseDialogResult(args.Result, Client.ContentDatabase, out key);
                    if (item == null)
                    {
                        return;
                    }
                    // Sitecore.Support.96322
                    // placeholder.MetaDataItemId = item.ID.ToString();
                    placeholder.MetaDataItemId = item.Paths.FullPath;
                    placeholder.Key = key;
                    DeviceEditorForm.SetDefinition(layoutDefinition);
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
        /// The set test
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [UsedImplicitly, HandleMessage("device:test", true)]
        protected void SetTest(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (this.SelectedIndex < 0)
            {
                return;
            }
            LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
            DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
            ArrayList renderings = device.Renderings;
            if (renderings == null)
            {
                return;
            }
            RenderingDefinition renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
            if (renderingDefinition == null)
            {
                return;
            }
            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    if (args.Result == "#reset#")
                    {
                        renderingDefinition.MultiVariateTest = string.Empty;
                        DeviceEditorForm.SetDefinition(layoutDefinition);
                        this.Refresh();
                        return;
                    }
                    ID iD = SetTestDetailsOptions.ParseDialogResult(args.Result);
                    if (ID.IsNullOrEmpty(iD))
                    {
                        SheerResponse.Alert("Item not found.", new string[0]);
                        return;
                    }
                    renderingDefinition.MultiVariateTest = iD.ToString();
                    DeviceEditorForm.SetDefinition(layoutDefinition);
                    this.Refresh();
                    return;
                }
            }
            else
            {
                Command command = CommandManager.GetCommand("device:settestdetails");
                Assert.IsNotNull(command, "deviceTestCommand");
                CommandContext commandContext = new CommandContext();
                commandContext.Parameters["deviceDefinitionId"] = device.ID;
                commandContext.Parameters["renderingDefinitionUniqueId"] = renderingDefinition.UniqueId;
                command.Execute(commandContext);
                args.WaitForPostBack();
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
        /// stage in the page life cycle, server controls in the hierarchy are created and initialized,
        /// view state is restored, and form controls reflect client-side data. Use the IsPostBack
        /// property to determine whether the page is being loaded in response to a client post back,
        /// or if it is being loaded and accessed for the first time.
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            if (!Context.ClientPage.IsEvent)
            {
                this.DeviceID = WebUtil.GetQueryString("de");
                LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
                DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
                if (device.Layout != null)
                {
                    this.Layout.Value = device.Layout;
                }
                this.Personalize.Visible = Policy.IsAllowed("Page Editor/Extended features/Personalization");
                Command command = CommandManager.GetCommand("device:settestdetails");
                this.Test.Visible = (command != null && command.QueryState(CommandContext.Empty) != CommandState.Hidden);
                this.Refresh();
                this.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Handles a click on the OK button.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The arguments.
        /// </param>
        /// <remarks>
        /// When the user clicks OK, the dialog is closed by calling
        /// the <see cref="M:Sitecore.Web.UI.Sheer.ClientResponse.CloseWindow">CloseWindow</see> method.
        /// </remarks>
        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            if (this.Layout.Value.Length > 0)
            {
                Item item = Client.ContentDatabase.GetItem(this.Layout.Value);
                if (item == null)
                {
                    Context.ClientPage.ClientResponse.Alert("Layout not found.");
                    return;
                }
                if (item.TemplateID == TemplateIDs.Folder || item.TemplateID == TemplateIDs.Node)
                {
                    Context.ClientPage.ClientResponse.Alert(Translate.Text("\"{0}\" is not a layout.", new object[]
					{
						item.DisplayName
					}));
                    return;
                }
            }
            LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
            DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
            ArrayList renderings = device.Renderings;
            if (renderings != null && renderings.Count > 0 && this.Layout.Value.Length == 0)
            {
                Context.ClientPage.ClientResponse.Alert("You must specify a layout when you specify renderings.");
                return;
            }
            device.Layout = this.Layout.Value;
            DeviceEditorForm.SetDefinition(layoutDefinition);
            Context.ClientPage.ClientResponse.SetDialogValue("yes");
            base.OnOK(sender, args);
        }

        /// <summary>
        /// Called when the rendering has click.
        /// </summary>
        /// <param name="uniqueId">
        /// The unique Id.
        /// </param>
        [UsedImplicitly]
        protected void OnPlaceholderClick(string uniqueId)
        {
            Assert.ArgumentNotNullOrEmpty(uniqueId, "uniqueId");
            if (!string.IsNullOrEmpty(this.UniqueId))
            {
                SheerResponse.SetStyle("ph_" + ID.Parse(this.UniqueId).ToShortID(), "background", string.Empty);
            }
            this.UniqueId = uniqueId;
            if (!string.IsNullOrEmpty(uniqueId))
            {
                SheerResponse.SetStyle("ph_" + ID.Parse(uniqueId).ToShortID(), "background", "#D0EBF6");
            }
            this.UpdatePlaceholdersCommandsState();
        }

        /// <summary>
        /// Called when the rendering has click.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        [UsedImplicitly]
        protected void OnRenderingClick(string index)
        {
            Assert.ArgumentNotNull(index, "index");
            if (this.SelectedIndex >= 0)
            {
                SheerResponse.SetStyle(StringUtil.GetString(this.Controls[this.SelectedIndex]), "background", string.Empty);
            }
            this.SelectedIndex = MainUtil.GetInt(index, -1);
            if (this.SelectedIndex >= 0)
            {
                SheerResponse.SetStyle(StringUtil.GetString(this.Controls[this.SelectedIndex]), "background", "#D0EBF6");
            }
            this.UpdateRenderingsCommandsState();
        }

        /// <summary>
        /// Personalizes the selected control.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        [UsedImplicitly, HandleMessage("device:personalize", true)]
        protected void PersonalizeControl(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (this.SelectedIndex < 0)
            {
                return;
            }
            LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
            DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
            ArrayList renderings = device.Renderings;
            if (renderings == null)
            {
                return;
            }
            RenderingDefinition renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
            if (renderingDefinition == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(renderingDefinition.ItemID) || string.IsNullOrEmpty(renderingDefinition.UniqueId))
            {
                return;
            }
            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    XElement rules = XElement.Parse(args.Result);
                    renderingDefinition.Rules = rules;
                    DeviceEditorForm.SetDefinition(layoutDefinition);
                    this.Refresh();
                    return;
                }
            }
            else
            {
                Item itemFromQueryString = UIUtil.GetItemFromQueryString(Client.ContentDatabase);
                string contextItemUri = (itemFromQueryString != null) ? itemFromQueryString.Uri.ToString() : string.Empty;
                PersonalizeOptions personalizeOptions = new PersonalizeOptions
                {
                    SessionHandle = DeviceEditorForm.GetSessionHandle(),
                    DeviceId = this.DeviceID,
                    RenderingUniqueId = renderingDefinition.UniqueId,
                    ContextItemUri = contextItemUri
                };
                SheerResponse.ShowModalDialog(personalizeOptions.ToUrlString().ToString(), "980px", "712px", string.Empty, true);
                args.WaitForPostBack();
            }
        }

        /// <summary>
        /// Removes the specified message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        [UsedImplicitly, HandleMessage("device:remove")]
        protected void Remove(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            int selectedIndex = this.SelectedIndex;
            if (selectedIndex < 0)
            {
                return;
            }
            LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
            DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
            ArrayList renderings = device.Renderings;
            if (renderings == null)
            {
                return;
            }
            if (selectedIndex < 0 || selectedIndex >= renderings.Count)
            {
                return;
            }
            renderings.RemoveAt(selectedIndex);
            if (selectedIndex >= 0)
            {
                this.SelectedIndex--;
            }
            DeviceEditorForm.SetDefinition(layoutDefinition);
            this.Refresh();
        }

        /// <summary>
        /// Removes the placeholder.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        [UsedImplicitly, HandleMessage("device:removeplaceholder")]
        protected void RemovePlaceholder(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            if (string.IsNullOrEmpty(this.UniqueId))
            {
                return;
            }
            LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
            DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
            PlaceholderDefinition placeholder = device.GetPlaceholder(this.UniqueId);
            if (placeholder == null)
            {
                return;
            }
            ArrayList placeholders = device.Placeholders;
            if (placeholders != null)
            {
                placeholders.Remove(placeholder);
            }
            DeviceEditorForm.SetDefinition(layoutDefinition);
            this.Refresh();
        }

        /// <summary>
        /// Sorts the down.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        [UsedImplicitly, HandleMessage("device:sortdown")]
        protected void SortDown(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            if (this.SelectedIndex < 0)
            {
                return;
            }
            LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
            DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
            ArrayList renderings = device.Renderings;
            if (renderings == null)
            {
                return;
            }
            if (this.SelectedIndex >= renderings.Count - 1)
            {
                return;
            }
            RenderingDefinition renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
            if (renderingDefinition == null)
            {
                return;
            }
            renderings.Remove(renderingDefinition);
            renderings.Insert(this.SelectedIndex + 1, renderingDefinition);
            this.SelectedIndex++;
            DeviceEditorForm.SetDefinition(layoutDefinition);
            this.Refresh();
        }

        /// <summary>
        /// Sorts the up.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        [UsedImplicitly, HandleMessage("device:sortup")]
        protected void SortUp(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            if (this.SelectedIndex <= 0)
            {
                return;
            }
            LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
            DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
            ArrayList renderings = device.Renderings;
            if (renderings == null)
            {
                return;
            }
            RenderingDefinition renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
            if (renderingDefinition == null)
            {
                return;
            }
            renderings.Remove(renderingDefinition);
            renderings.Insert(this.SelectedIndex - 1, renderingDefinition);
            this.SelectedIndex--;
            DeviceEditorForm.SetDefinition(layoutDefinition);
            this.Refresh();
        }

        /// <summary>
        /// Gets the layout definition.
        /// </summary>
        /// <returns>
        /// The layout definition.
        /// </returns>
        /// <contract><ensures condition="not null" /></contract>
        private static LayoutDefinition GetLayoutDefinition()
        {
            string sessionString = WebUtil.GetSessionString(DeviceEditorForm.GetSessionHandle());
            Assert.IsNotNull(sessionString, "layout definition");
            return LayoutDefinition.Parse(sessionString);
        }

        /// <summary>
        /// Gets the session handle.
        /// </summary>
        /// <returns>
        /// The session handle string.
        /// </returns>
        private static string GetSessionHandle()
        {
            return "SC_DEVICEEDITOR";
        }

        /// <summary>
        /// Sets the definition.
        /// </summary>
        /// <param name="layout">
        /// The layout.
        /// </param>
        private static void SetDefinition(LayoutDefinition layout)
        {
            Assert.ArgumentNotNull(layout, "layout");
            string value = layout.ToXml();
            WebUtil.SetSessionValue(DeviceEditorForm.GetSessionHandle(), value);
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        private void Refresh()
        {
            this.Renderings.Controls.Clear();
            this.Placeholders.Controls.Clear();
            this.Controls = new ArrayList();
            LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
            DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
            if (device.Renderings == null)
            {
                SheerResponse.SetOuterHtml("Renderings", this.Renderings);
                SheerResponse.SetOuterHtml("Placeholders", this.Placeholders);
                SheerResponse.Eval("if (!scForm.browser.isIE) { scForm.browser.initializeFixsizeElements(); }");
                return;
            }
            int selectedIndex = this.SelectedIndex;
            this.RenderRenderings(device, selectedIndex, 0);
            this.RenderPlaceholders(device);
            this.UpdateRenderingsCommandsState();
            this.UpdatePlaceholdersCommandsState();
            SheerResponse.SetOuterHtml("Renderings", this.Renderings);
            SheerResponse.SetOuterHtml("Placeholders", this.Placeholders);
            SheerResponse.Eval("if (!scForm.browser.isIE) { scForm.browser.initializeFixsizeElements(); }");
        }

        /// <summary>
        /// Renders the placeholders.
        /// </summary>
        /// <param name="deviceDefinition">
        /// The device definition.
        /// </param>
        private void RenderPlaceholders(DeviceDefinition deviceDefinition)
        {
            Assert.ArgumentNotNull(deviceDefinition, "deviceDefinition");
            ArrayList placeholders = deviceDefinition.Placeholders;
            if (placeholders == null)
            {
                return;
            }
            foreach (PlaceholderDefinition placeholderDefinition in placeholders)
            {
                Item item = null;
                string metaDataItemId = placeholderDefinition.MetaDataItemId;
                if (!string.IsNullOrEmpty(metaDataItemId))
                {
                    item = Client.ContentDatabase.GetItem(metaDataItemId);
                }
                XmlControl xmlControl = Resource.GetWebControl("DeviceRendering") as XmlControl;
                Assert.IsNotNull(xmlControl, typeof(XmlControl));
                this.Placeholders.Controls.Add(xmlControl);
                ID iD = ID.Parse(placeholderDefinition.UniqueId);
                if (placeholderDefinition.UniqueId == this.UniqueId)
                {
                    xmlControl["Background"] = "#D0EBF6";
                }
                string value = "ph_" + iD.ToShortID();
                xmlControl["ID"] = value;
                xmlControl["Header"] = placeholderDefinition.Key;
                xmlControl["Click"] = "OnPlaceholderClick(\"" + placeholderDefinition.UniqueId + "\")";
                xmlControl["DblClick"] = "device:editplaceholder";
                if (item != null)
                {
                    xmlControl["Icon"] = item.Appearance.Icon;
                }
                else
                {
                    xmlControl["Icon"] = "Imaging/24x24/layer_blend.png";
                }
            }
        }

        /// <summary>
        /// Renders the specified device definition.
        /// </summary>
        /// <param name="deviceDefinition">
        /// The device definition.
        /// </param>
        /// <param name="selectedIndex">
        /// Index of the selected.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        private void RenderRenderings(DeviceDefinition deviceDefinition, int selectedIndex, int index)
        {
            Assert.ArgumentNotNull(deviceDefinition, "deviceDefinition");
            ArrayList renderings = deviceDefinition.Renderings;
            if (renderings == null)
            {
                return;
            }
            foreach (RenderingDefinition renderingDefinition in renderings)
            {
                if (renderingDefinition.ItemID != null)
                {
                    Item item = Client.ContentDatabase.GetItem(renderingDefinition.ItemID);
                    XmlControl xmlControl = Resource.GetWebControl("DeviceRendering") as XmlControl;
                    Assert.IsNotNull(xmlControl, typeof(XmlControl));
                    System.Web.UI.HtmlControls.HtmlGenericControl htmlGenericControl = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
                    htmlGenericControl.Style.Add("padding", "0");
                    htmlGenericControl.Style.Add("margin", "0");
                    htmlGenericControl.Style.Add("border", "0");
                    htmlGenericControl.Style.Add("position", "relative");
                    htmlGenericControl.Controls.Add(xmlControl);
                    string uniqueID = Control.GetUniqueID("R");
                    this.Renderings.Controls.Add(htmlGenericControl);
                    htmlGenericControl.ID = Control.GetUniqueID("C");
                    xmlControl["Click"] = "OnRenderingClick(\"" + index + "\")";
                    xmlControl["DblClick"] = "device:edit";
                    if (index == selectedIndex)
                    {
                        xmlControl["Background"] = "#D0EBF6";
                    }
                    this.Controls.Add(uniqueID);
                    if (item != null)
                    {
                        xmlControl["ID"] = uniqueID;
                        xmlControl["Icon"] = item.Appearance.Icon;
                        xmlControl["Header"] = item.DisplayName;
                        xmlControl["Placeholder"] = WebUtil.SafeEncode(renderingDefinition.Placeholder);
                    }
                    else
                    {
                        xmlControl["ID"] = uniqueID;
                        xmlControl["Icon"] = "Applications/24x24/forbidden.png";
                        xmlControl["Header"] = "Unknown rendering";
                        xmlControl["Placeholder"] = string.Empty;
                    }
                    if (renderingDefinition.Rules != null && !renderingDefinition.Rules.IsEmpty)
                    {
                        int num = renderingDefinition.Rules.Elements("rule").Count<XElement>();
                        if (num > 1)
                        {
                            System.Web.UI.HtmlControls.HtmlGenericControl htmlGenericControl2 = new System.Web.UI.HtmlControls.HtmlGenericControl("span");
                            if (num > 9)
                            {
                                htmlGenericControl2.Attributes["class"] = "scConditionContainer scLongConditionContainer";
                            }
                            else
                            {
                                htmlGenericControl2.Attributes["class"] = "scConditionContainer";
                            }
                            htmlGenericControl2.InnerText = num.ToString();
                            htmlGenericControl.Controls.Add(htmlGenericControl2);
                        }
                    }
                    RenderDeviceEditorRenderingPipeline.Run(renderingDefinition, xmlControl, htmlGenericControl);
                    index++;
                }
            }
        }

        /// <summary>
        /// Updates the state of the commands.
        /// </summary>
        private void UpdateRenderingsCommandsState()
        {
            if (this.SelectedIndex < 0)
            {
                this.ChangeButtonsState(true);
                return;
            }
            LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
            DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
            ArrayList renderings = device.Renderings;
            if (renderings == null)
            {
                this.ChangeButtonsState(true);
                return;
            }
            RenderingDefinition renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
            if (renderingDefinition == null)
            {
                this.ChangeButtonsState(true);
                return;
            }
            this.ChangeButtonsState(false);
            this.Personalize.Disabled = !string.IsNullOrEmpty(renderingDefinition.MultiVariateTest);
            this.Test.Disabled = DeviceEditorForm.HasRenderingRules(renderingDefinition);
        }

        private void UpdatePlaceholdersCommandsState()
        {
            this.phEdit.Disabled = string.IsNullOrEmpty(this.UniqueId);
            this.phRemove.Disabled = string.IsNullOrEmpty(this.UniqueId);
        }

        /// <summary>
        /// Changes the disable of the buttons.
        /// </summary>
        /// <param name="disable">if set to <c>true</c> buttons are disabled.</param>
        private void ChangeButtonsState(bool disable)
        {
            this.Personalize.Disabled = disable;
            this.btnEdit.Disabled = disable;
            this.btnChange.Disabled = disable;
            this.btnRemove.Disabled = disable;
            this.MoveUp.Disabled = disable;
            this.MoveDown.Disabled = disable;
            this.Test.Disabled = disable;
        }

        /// <summary>
        /// Determines whether [has rendering rules] [the specified definition].
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <returns><c>true</c> if the definition has a defined rule with action; otherwise, <c>false</c>.</returns>
        private static bool HasRenderingRules(RenderingDefinition definition)
        {
            if (definition.Rules == null)
            {
                return false;
            }
            RulesDefinition rulesDefinition = new RulesDefinition(definition.Rules.ToString());
            foreach (XElement current in from rule in rulesDefinition.GetRules()
                                         where rule.Attribute("uid").Value != ItemIDs.Null.ToString()
                                         select rule)
            {
                XElement xElement = current.Descendants("actions").FirstOrDefault<XElement>();
                if (xElement != null && xElement.Descendants().Any<XElement>())
                {
                    return true;
                }
            }
            return false;
        }
    }
}