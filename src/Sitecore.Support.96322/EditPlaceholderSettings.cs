using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Pipelines;
using Sitecore.Pipelines.GetPlaceholderRenderings;
using Sitecore.Shell.Applications.Dialogs;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Specialized;
using System.Linq;


namespace Sitecore.Support.Shell.Applications.WebEdit.Commands
{
    [System.Serializable]
    public class EditPlaceholderSettings : Sitecore.Shell.Applications.WebEdit.Commands.EditPlaceholderSettings
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            string value = context.Parameters["key"];
            Assert.IsNotNullOrEmpty(value, "Placeholder key");
            NameValueCollection nameValueCollection = new NameValueCollection();
            nameValueCollection["key"] = value;
            Context.ClientPage.Start(this, "Run", nameValueCollection);
        }

        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            string formValue = WebUtil.GetFormValue("scLayout");
            Assert.IsNotNullOrEmpty(formValue, "layout");
            ID iD = ShortID.DecodeID(WebUtil.GetFormValue("scDeviceID"));
            string text = Sitecore.Web.WebEditUtil.ConvertJSONLayoutToXML(formValue);
            Assert.IsNotNull(text, "layout definition");
            Assert.IsNotNull(Context.ContentDatabase, "database");
            Assert.IsNotNull(Context.Page, "page");
            string text2 = args.Parameters["key"];
            Assert.IsNotNull(text2, "key");
            if (!args.IsPostBack)
            {
                Item placeholderItem;
                using (new DeviceSwitcher(iD, Context.ContentDatabase))
                {
                    placeholderItem = Context.Page.GetPlaceholderItem(text2, Context.ContentDatabase, text);
                }
                SelectPlaceholderSettingsOptions selectPlaceholderSettingsOptions = new SelectPlaceholderSettingsOptions();
                if (placeholderItem != null)
                {
                    selectPlaceholderSettingsOptions.SelectedItem = placeholderItem;
                    selectPlaceholderSettingsOptions.CurrentSettingsItem = placeholderItem;
                }
                selectPlaceholderSettingsOptions.PlaceholderKey = text2;
                SheerResponse.ShowModalDialog(selectPlaceholderSettingsOptions.ToUrlString().ToString(), "460px", "460px", string.Empty, true);
                args.WaitForPostBack();
                return;
            }
            if (!args.HasResult)
            {
                SheerResponse.SetAttribute("scLayoutDefinition", "value", string.Empty);
                return;
            }
            string text3;
            Item item = SelectPlaceholderSettingsOptions.ParseDialogResult(args.Result, Client.ContentDatabase, out text3);
            if (item == null)
            {
                SheerResponse.SetAttribute("scLayoutDefinition", "value", string.Empty);
                Item placeholderItem2;
                using (new DeviceSwitcher(iD, Context.ContentDatabase))
                {
                    placeholderItem2 = Context.Page.GetPlaceholderItem(text2, Context.ContentDatabase, text);
                }
                Assert.IsNotNull(placeholderItem2, "currentSettingsItem");
                base.SetReturnValues(args, text2, placeholderItem2, text);
                return;
            }
            LayoutDefinition layoutDefinition = LayoutDefinition.Parse(text);
            PlaceholderDefinition placeholderDefinition = Context.Page.GetPlaceholderDefinition(layoutDefinition, text2, iD);
            if (placeholderDefinition == null)
            {
                placeholderDefinition = new PlaceholderDefinition
                {
                    Key = text2,
                    UniqueId = ID.NewID.ToString()
                };
                DeviceDefinition device = layoutDefinition.GetDevice(iD.ToString());
                Assert.IsNotNull(device, "device");
                device.AddPlaceholder(placeholderDefinition);
            }

            //Sitecore.Support.96322
            //modified from placeholderDefinition.MetaDataItemId = item.Paths.FullPath; to
            placeholderDefinition.MetaDataItemId = item.ID.ToString();
            string text4 = layoutDefinition.ToXml();
            SheerResponse.SetAttribute("scLayoutDefinition", "value", Sitecore.Web.WebEditUtil.ConvertXMLLayoutToJSON(text4));
            base.SetReturnValues(args, text2, item, text4);
        }
    }
}
