using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using Newtonsoft.Json;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System;
using System.Collections.Generic;
using System.IO;

namespace MaliMissionRoller2
{
    public class Main : AOPluginEntry
    {
        public static string PluginDir;
        private MainWindow _window;
        public static Sounds Sounds;
        public static Settings Settings;
        public static List<KeyValuePair<ItemInfo, List<Stat>>> ItemDb;

        public unsafe override void Run(string pluginDir)
        {
            Chat.WriteLine("- Mali's Mission Roller 2.0 -", ChatColor.Gold);

            PluginDir = pluginDir;
            Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText($"{pluginDir}\\JSON\\Settings.json"));
            Extensions.FormatItemDb(Settings.Database["Implants"], Settings.Database["Clusters"], Settings.Database["Nanos"], Settings.Database["Rest"]);
            _window = new MainWindow("MaliMissionRoller", $"{pluginDir}\\UI\\Windows\\MainWindow.xml");
            //_window.Show();
            //_window.Window.MoveTo(Settings.Frame.X, Settings.Frame.Y);
            //Game.OnUpdate += Update;
           // Mission.RollListChanged += RollListChanged;
            //Network.N3MessageSent += N3Message_Sent;
           // Game.TeleportEnded += Game_OnTeleportEnded;
            //Sounds = new Sounds();
            //Extensions.PlaySound(Sounds.Alert);
            Chat.RegisterCommand("mmr", (string command, string[] param, ChatWindow chatWindow) => DevCmd(param));
        }
        private void DevCmd(string[] param)
        {
            if (param.Length < 2)
                return;

            string userInput = param[0].ToLower();

            if (!int.TryParse(param[1], out int result))
                return;

            switch (userInput)
            {
                case "maxitems":
                    Settings.Dev["MaxItems"] = result;
                    _window.SettingsView.ItemDisplay.DeleteBrowserEntries();
                    _window.SettingsView.ItemDisplay.FormatBrowserEntries();
                    Chat.WriteLine($"Max Display items set to: {Settings.Dev["MaxItems"]}",ChatColor.Red);
                    break;
                case "shopvalue":
                    Settings.Dev["ShopValue"] = result;
                    Chat.WriteLine($"Shop Value Factor set to: {Settings.Dev["ShopValue"]}", ChatColor.Red);
                    break;
            }
        }

        private void Game_OnTeleportEnded(object sender, EventArgs e)
        {
            _window.SettingsView.Locations.BoundsCheck();
        }

        private void N3Message_Sent(object sender, N3Message n3Msg)
        {
            if (n3Msg.Identity != DynelManager.LocalPlayer.Identity && 
                n3Msg.N3MessageType != N3MessageType.GenericCmd)
                return;

            if (((GenericCmdMessage)n3Msg).Target.Type != IdentityType.MissionTerminal)
                return;

            MainWindow.CurrentTerminal = new MissionTerminal(DynelManager.GetDynel(((GenericCmdMessage)n3Msg).Target));
            _window.MissionView.ShopValue = Math.Round(Settings.Dev["ShopValue"] * (1 + (float)DynelManager.LocalPlayer.GetStat(Stat.ComputerLiteracy) / (40 * 100)) / 100, 3);
            _window.SwapViews();
        }

        private void RollListChanged(object sender, RollListChangedArgs rollListChanged)
        {
            _window.RollMatchCheck(rollListChanged.MissionDetails);
        }

        private void Update(object sender, float e)
        {
            _window.Update(e);
        }

        public override void Teardown()
        {
            Settings.Save(_window);
        }
    }
}