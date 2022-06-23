﻿using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using Newtonsoft.Json;
using SmokeLounge.AOtomation.Messaging.GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MaliMissionRoller2
{
    public class MainWindow: AOSharpWindow
    {
        public static MissionTerminal CurrentTerminal;
        internal HeaderView HeaderView;
        internal MissionView MissionView;
        internal SettingsView SettingsView;
        internal bool InSettings;
        private bool _isRolling;
        private float _requestTimer;
        private int _missionLevel;
        private readonly List<List<int>> MissionLvls = JsonConvert.DeserializeObject<List<List<int>>>(File.ReadAllText($"{Main.PluginDir}\\JSON\\MissionLevels.json"));

        public MainWindow(string name, string path, WindowStyle windowStyle = WindowStyle.Popup, WindowFlags flags = WindowFlags.AutoScale | WindowFlags.NoFade) : base(name, path, windowStyle, flags)
        {
            Extensions.LoadCustomTextures($"{Main.PluginDir}\\UI\\Textures\\", 1000035);
        }

        protected override void OnWindowCreating()
        {
            try
            {
                HelpWindow _helpWindow = new HelpWindow();
                if (Window.FindView("HeaderRoot", out View headerRoot))
                {
                    InSettings = false;
                    _isRolling = false;
                    _requestTimer = 0.9f;
                    HeaderView = new HeaderView(headerRoot);
                    HeaderView.Help.Clicked += HelpClick;
                    HeaderView.Start.Clicked += StartClick;
                    HeaderView.Settings.Tag = InSettings;
                    HeaderView.Settings.Clicked += SettingsClick;
                    HeaderView.Request.Clicked += RequestClick;
                }

                if (Window.FindView("MissionRoot", out View missionRoot))
                {
                    MissionView = new MissionView(missionRoot);
                    MissionView.Hide();
                }

                if (Window.FindView("SettingsRoot", out View settingsRoot))
                {
                    SettingsView = new SettingsView(settingsRoot);
                    SettingsView.Locations.BoundsCheck();
                    SettingsView.Hide();
                }
            }
            catch (Exception e)
            {
                Chat.WriteLine(e);
            }
        }

        private void HelpClick(object sender, ButtonBase e)
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Window.Show(true);
        }

        private void StartClick(object sender, ButtonBase e)
        {
            Extensions.PlaySound(Main.Sounds.Click);

            _isRolling = !_isRolling;
            Chat.WriteLine($"Rolling set to: {_isRolling.ToString().ToUpper()}");
        }

        private void SettingsClick(object sender, ButtonBase e)
        {
            Extensions.PlaySound(Main.Sounds.Click);

            if (InSettings)
            {
                Extensions.ButtonSetGfx(HeaderView.Settings, 1000050);
                MissionView.Show();
                SettingsView.Hide();
            }
            else
            {
                Extensions.ButtonSetGfx(HeaderView.Settings, 1000043);
                MissionView.Hide();
                SettingsView.Show();
            }
            InSettings = !InSettings;
        }

        public void SwapViews()
        {
            Extensions.PlaySound(Main.Sounds.Click);

            if (InSettings)
            {
                MissionView.Show();
                SettingsView.Hide();
                InSettings = false;
            }
            else
            {
                MissionView.Show();
            }

            Extensions.ButtonSetGfx(HeaderView.Settings, 1000050);
        }

        private void RequestClick(object sender, ButtonBase e)
        {
            Extensions.PlaySound(Main.Sounds.Click);
            SwapViews();
            RequestMission();
        }

        internal void RequestMission()
        {
            if (CurrentTerminal == null)
            {
                Chat.WriteLine("Invalid terminal.");
                _isRolling = false;
                return;
            }

            if (Inventory.NumFreeSlots < 2)
            {
                Chat.WriteLine("You need at least 2 free inventory slots to roll.");
                _isRolling = false;
                return;
            }

            List<RollEntryView> rollEntries = SettingsView.ItemDisplay.RollEntryViews;

            if (rollEntries.Count == 0 && _isRolling)
            {
                _isRolling = false;
                Chat.WriteLine("Roll List is empty!");
                Chat.WriteLine("Rolling set to: FALSE");
                return;
            }

            if ((bool)SettingsView.ExtraOptions.AutoAdjustQl.Tag && _isRolling)
            {

                var rollEntry = SettingsView.ItemDisplay.RollEntryViews
                    .FirstOrDefault(entry => MissionLvls[DynelManager.LocalPlayer.Level - 1]
                    .Any(missionRange => entry.RollEntryModel.Ql - missionRange == 0 || new[] { "Nano Crystal", "NanoCrystal" }.Any(entry.RollEntryModel.Name.Contains) && Math.Abs(entry.RollEntryModel.Ql - missionRange) <= 10));

                if (rollEntry == null)
                {

                    rollEntry = SettingsView.ItemDisplay.RollEntryViews.FirstOrDefault(x => x.RollEntryModel.LowId == 297315);

                    if (rollEntry == null)
                    {
                        Extensions.PlaySound(Main.Sounds.Alert);

                        Chat.WriteLine("Remaining roll items outside characters level reach.\n" +
                            "If you think this is wrong, disable the 'Auto Adjust Level Slider'\n" +
                            "temporarily and contact me so I can update the mission level table!\n" +
                            "(press '?' in the top right corner for contact details)");

                        _isRolling = false;
                        return;
                    }
                }

                if (rollEntry.RollEntryModel.LowId != 297315)
                {
                    _missionLevel = MissionLvls[DynelManager.LocalPlayer.Level - 1].OrderBy(x => Math.Abs(x - rollEntry.RollEntryModel.Ql)).FirstOrDefault();
                    int count = SettingsView.ItemDisplay.RollEntryViews
                        .Count(y => Math.Abs(_missionLevel - y.RollEntryModel.Ql) <= 10 && new[] { "Nano Crystal", "NanoCrystal" }
                        .Any(y.RollEntryModel.Name.Contains) ||
                        _missionLevel - y.RollEntryModel.Ql == 0);

                    SettingsView.Sliders.EasyHard.Value = MissionLvls[DynelManager.LocalPlayer.Level - 1].IndexOf(_missionLevel) + 1;

                    Chat.WriteLine($"Mission level set to: {_missionLevel}\nItems to roll in this range: {count}");
                }
                else
                {
                    Chat.WriteLine($"Rolling for Mission Combined Credit Reward >= {rollEntry.RollEntryModel.Ql}");
                }
            }

            MissionSliders sliders = SettingsView.Sliders.GetSliderValues();

            CurrentTerminal.RequestMissions(
                sliders.Difficulty,
                sliders.GoodBad,
                sliders.OrderChaos,
                sliders.OpenHidden,
                sliders.PhysicalMystical,
                sliders.HeadonStealth,
                sliders.CreditsXp
                );
        }
        internal void RollMatchCheck(MissionInfo[] rollList)
        {
            MissionView.Update(rollList);
            int missionIndex = -1;
            foreach (MissionInfo missionInfo in rollList)
            {
                RollEntryView rollEntry = SettingsView.ItemDisplay.RollEntryViews
                    .Where(b => missionInfo.MissionItemData.Any(a => a.HighId == b.RollEntryModel.HighId && a.Ql == b.RollEntryModel.Ql) ||
                                missionInfo.Description.Contains(b.RollEntryModel.Name) && new[] { "Nano Crystal", "NanoCrystal" }.Any(b.RollEntryModel.Name.Contains) ||
                                missionInfo.Description.Contains(b.RollEntryModel.Name) && b.RollEntryModel.Ql == _missionLevel)
                    .FirstOrDefault();

                missionIndex++;

                if (rollEntry == null)
                {
                    rollEntry = SettingsView.ItemDisplay.RollEntryViews.FirstOrDefault(x => x.RollEntryModel.LowId == 297315);

                    if (rollEntry == null || rollEntry.RollEntryModel.Ql > MissionView.CombinedItemValue[missionIndex])
                        continue;
                }

                if (!(bool)SettingsView.MissionTypes.ReturnItem.Tag && missionInfo.MissionIcon == 11329)
                {
                    Chat.WriteLine($"Skipping {rollEntry.RollEntryModel.Name} (Return Item disabled)");
                    continue;
                }
                if (!(bool)SettingsView.MissionTypes.KillTarget.Tag && missionInfo.MissionIcon == 11330)
                {
                    Chat.WriteLine($"Skipping {rollEntry.RollEntryModel.Name} (Kill Target disabled)");
                    continue;
                }
                if (!(bool)SettingsView.MissionTypes.FindTarget.Tag && missionInfo.MissionIcon == 11335)
                {
                    Chat.WriteLine($"Skipping {rollEntry.RollEntryModel.Name} (Find Target disabled)");
                    continue;
                }
                if (!(bool)SettingsView.MissionTypes.FindItem.Tag && missionInfo.MissionIcon == 11337)
                {
                    Chat.WriteLine($"Skipping {rollEntry.RollEntryModel.Name} (Find Item disabled)");
                    continue;
                }
                if (!(bool)SettingsView.MissionTypes.UseItem.Tag && missionInfo.MissionIcon == 11342)
                {
                    Chat.WriteLine($"Skipping {rollEntry.RollEntryModel.Name} (Use Item disabled)");
                    continue;
                }

                LocationViewEntry locEntry = SettingsView.Locations.Entries.Where(x => (bool)x.Toggle.Tag && x.PfId == missionInfo.Playfield.Instance).FirstOrDefault();

                if (locEntry == null)
                {
                    Chat.WriteLine($"Skipping {rollEntry.RollEntryModel.Name} (Location turned off)");
                    continue;
                }
                if (locEntry.Bounds.Coord1.X != 0 && locEntry.Bounds.Coord2.X != 0 && !locEntry.Bounds.Contains(missionInfo.Location))
                {
                    Chat.WriteLine($"Skipping {rollEntry.RollEntryModel.Name} (Location out of bounds)");
                    continue;
                }

                SettingsView.ItemDisplay.UpdateRollEntry(rollEntry, (bool)SettingsView.ExtraOptions.RemoveRoll.Tag);

                if ((bool)SettingsView.ExtraOptions.AutoAccept.Tag)
                {
                    Chat.WriteLine($"{rollEntry.RollEntryModel.Ql} {MissionView.CombinedItemValue[missionIndex]}");
                    MissionView.AcceptMission(missionInfo.MissionIdentity, (bool)SettingsView.ExtraOptions.PlayAlertSound.Tag);
                }
                else
                {
                    _isRolling = false;
                    Chat.WriteLine($"Match found! {rollEntry.RollEntryModel.Name} (Auto Accept turned off)");
                }
                return;
            }
            _requestTimer = 0.9f;
        }

        public void Update(float e)
        {
            _requestTimer -= e;

            MissionView.UpdateDistance();
            SettingsView.UpdateUI((bool)SettingsView.ExtraOptions.ShowBounds.Tag);

            if (_isRolling && _requestTimer < 0)
            {
                RequestMission();
                _requestTimer = 1.5f;
            }
        }

        public void UpdateTerminal(MissionTerminal terminal)
        {
            CurrentTerminal = terminal;
            if ((bool)HeaderView.Settings.Tag == true)
                MissionView.Show();
        }
    }
}