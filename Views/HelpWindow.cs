using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Common.Helpers;
using AOSharp.Common.Unmanaged.Imports;
using AOSharp.Common.Unmanaged.Interfaces;
using AOSharp.Core;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MaliMissionRoller2
{
    public class HelpWindow
    {
        public Window Window;

        public HelpWindow()
        {
            Window = Window.CreateFromXml("MaliMissionRollerHelp", $"{Main.PluginDir}\\UI\\Windows\\HelpWindow.xml", 
                WindowStyle.Popup, WindowFlags.AutoScale | WindowFlags.NoFade);

            if (Window.FindView("Text", out TextView textView))
            {
                textView.Text = $"\n " +
                $"- Take advantage over continuous rolls\n " +
                $"- Roll with multiple clients simultaneously\n " +
                $"- Never skip a good roll due to server lag\n " +
                $"- Built in item db with filtering options\n " +
                $"- Use the 'Auto Adjust Lvl Slider' option \n " +
                $"  to automatically adjust the lvl slider\n " +
                $"  inbetween rolls for full automation\n " +
                $"- Press '?' in the UI to open this menu\n\n\n " +
                $"* QUICK ITEM BROWSER GUIDE *\n " +
                $"- You can search for items based on\n " +
                $"  their name or modifications or both\n " +
                $"  Use the two blue textboxes provided\n " +
                $"  after switching to 'DB Browser'\n " +
                $"  Textbox1 example: eye imp ref\n " +
                $"  Textbox2 example: assa rif,tutor'\n\n\n " +
                $"- For bugs / glitches / requests:\n " +
                $"  Discord:  Pixelmania#0349\n\n\n\n "+
                $"               ~ Made with AOSharp\n ";
            }

            if (Window.FindView("Close", out Button _close))
            {
                Extensions.ButtonSetGfx(_close, 1000064);
                _close.Clicked = CloseClick;
            }

            if (Window.FindView("Logo", out BitmapView logo))
            {
                logo.SetBitmap("BigLogo");
            }

            if (Main.Settings.Extras["StartHelp"])
            {
                Window.MoveToCenter();
                Window.Show(true);
            }
        }

        private void CloseClick(object sender, ButtonBase e)
        {
            Midi.Play("Click");
            Window.Close();
        }
    }
}