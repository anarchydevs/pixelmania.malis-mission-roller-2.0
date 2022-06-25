using AOSharp.Common.GameData;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MaliMissionRoller2
{
    public class Settings
    {
        public Dictionary<string, bool> Types;
        public Dictionary<string, bool> Extras;
        public Dictionary<string, float> Sliders;
        public Dictionary<string, bool> Database;
        public Dictionary<string, int> Dev;

        public Dictionary<string, LocationModel> Locations;
        public Vector2 Frame;

        public class LocationModel
        {
            public Bounds Bounds;
            public bool State;
        }

        public Settings()
        {
        }

        public void Save(MainWindow window)
        {
            foreach (PlayfieldEntryView locationEntry in window.SettingsView.Locations.Entries)
                Locations[locationEntry.Name.Text] = new LocationModel
                {
                    State = (bool)locationEntry.Toggle.Tag,
                    Bounds = new Bounds
                    {
                        Coord1 = locationEntry.Bounds.Coord1,
                        Coord2 = locationEntry.Bounds.Coord2,
                    }
                };

            Sliders["EasyHard"] = window.SettingsView.Sliders.EasyHard.Value;
            Sliders["GoodBad"] = window.SettingsView.Sliders.GoodBad.Value;
            Sliders["OrderChaos"] = window.SettingsView.Sliders.OrderChaos.Value;
            Sliders["OpenHidden"] = window.SettingsView.Sliders.OpenHidden.Value;
            Sliders["PhysicalMystical"] = window.SettingsView.Sliders.PhysicalMystical.Value;
            Sliders["HeadonStealth"] = window.SettingsView.Sliders.HeadonStealth.Value;
            Sliders["CreditsXp"] = window.SettingsView.Sliders.CreditsXp.Value;

            Types["ReturnItem"] = (bool)window.SettingsView.MissionTypes.ReturnItem.Tag;
            Types["KillTarget"] = (bool)window.SettingsView.MissionTypes.KillTarget.Tag;
            Types["FindTarget"] = (bool)window.SettingsView.MissionTypes.FindTarget.Tag;
            Types["FindItem"] = (bool)window.SettingsView.MissionTypes.FindItem.Tag;
            Types["UseItem"] = (bool)window.SettingsView.MissionTypes.UseItem.Tag;

            Extras["PlayAlertSound"] = (bool)window.SettingsView.ExtraOptions.PlayAlertSound.Tag;
            Extras["AutoAdjustQl"] = (bool)window.SettingsView.ExtraOptions.AutoAdjustQl.Tag;
            Extras["RemoveRoll"] = (bool)window.SettingsView.ExtraOptions.RemoveRoll.Tag;
            Extras["AutoAccept"] = (bool)window.SettingsView.ExtraOptions.AutoAccept.Tag;
            Extras["ShowBounds"] = (bool)window.SettingsView.ExtraOptions.ShowBounds.Tag;
            Extras["StartHelp"] = false;


            Database["Implants"] = (bool)window.SettingsView.ItemDisplay.Implants.Tag;
            Database["Clusters"] = (bool)window.SettingsView.ItemDisplay.Clusters.Tag;
            Database["Nanos"] = (bool)window.SettingsView.ItemDisplay.Nanos.Tag;
            Database["Rest"] = (bool)window.SettingsView.ItemDisplay.Rest.Tag;


            Frame.X = window.Window.GetFrame().MinX;
            Frame.Y = window.Window.GetFrame().MinY;

            File.WriteAllText($"{Main.PluginDir}\\JSON\\Settings.json", JsonConvert.SerializeObject(this));

            List<RollEntryViewModel> rollModels = window.SettingsView.ItemDisplay.RollEntryViews.Select(x => x.RollEntryModel).ToList();
            File.WriteAllText($"{Main.PluginDir}\\JSON\\RollList.json", JsonConvert.SerializeObject(rollModels));
        }
    }
}