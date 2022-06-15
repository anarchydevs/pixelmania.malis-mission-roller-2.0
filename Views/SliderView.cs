﻿using AOSharp.Common.GameData;
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
    public class SlidersView
    {
        private View _root;
        private BitmapView _background;
        internal SliderView EasyHard;
        internal SliderView GoodBad;
        internal SliderView OrderChaos;
        internal SliderView OpenHidden;
        internal SliderView PhysicalMystical;
        internal SliderView HeadonStealth;
        internal SliderView CreditsXp;

        public SlidersView(View root)
        {
            _root = root;
            View _view = View.CreateFromXml($"{Main.PluginDir}\\UI\\Views\\SliderView.xml");

            _view.FindChild("Background", out _background);
            _background.SetBitmap("SlidersBg");
            _view.FindChild("EasyHard", out EasyHard);
            EasyHard.Value = Main.Settings.Sliders["EasyHard"];
            _view.FindChild("GoodBad", out GoodBad);
            GoodBad.Value = Main.Settings.Sliders["GoodBad"];
            _view.FindChild("OrderChaos", out OrderChaos);
            OrderChaos.Value = Main.Settings.Sliders["OrderChaos"];
            _view.FindChild("OpenHidden", out OpenHidden);
            OpenHidden.Value = Main.Settings.Sliders["OpenHidden"];
            _view.FindChild("PhysicalMystical", out PhysicalMystical);
            PhysicalMystical.Value = Main.Settings.Sliders["PhysicalMystical"];
            _view.FindChild("HeadonStealth", out HeadonStealth);
            HeadonStealth.Value = Main.Settings.Sliders["HeadonStealth"];
            _view.FindChild("CreditsXp", out CreditsXp);
            CreditsXp.Value = Main.Settings.Sliders["CreditsXp"];
            _root.AddChild(_view, false);
        }

        internal MissionSliders GetSliderValues()
        {
            return new MissionSliders
            {
                Difficulty = (byte)EasyHard.Value,
                GoodBad = unchecked((byte)GoodBad.Value),
                OrderChaos = unchecked((byte)OrderChaos.Value),
                OpenHidden = unchecked((byte)OpenHidden.Value),
                PhysicalMystical = unchecked((byte)PhysicalMystical.Value),
                HeadonStealth = unchecked((byte)HeadonStealth.Value),
                CreditsXp = unchecked((byte)CreditsXp.Value)
            };

        }
    }
}