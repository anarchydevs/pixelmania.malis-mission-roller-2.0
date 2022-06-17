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
    public class LocationView
    {
        internal List<LocationViewEntry> Entries;
        internal View Root;
        private Bounds _bounds;
        private Button _coords;
        private Button _enableAll;
        private Button _disableAll;
        private readonly int[] _pfIds = new int[] { 760, 585, 655, 550, 545, 505, 605, 800, 665, 590, 670, 595, 620, 685, 687, 717, 647, 791, 695, 625, 560, 696, 567, 566, 565, 540, 716, 705, 700, 710, 570, 630, 735, 740, 730, 610, 615, 635, 790, 795, 640, 646, 650, 600, 551, 586 };
       
        public LocationView(View root)
        {
            Entries = new List<LocationViewEntry>();
            _bounds = new Bounds();
            Root = root;

            View _view = View.CreateFromXml($"{Main.PluginDir}\\UI\\Views\\LocationView.xml");
            _view.FindChild("ScrollListRoot", out View _scrollListRoot);
            _view.FindChild("Background", out BitmapView _background);
            _background.SetBitmap("LocationsBg");
            _view.FindChild("Coords", out _coords);
            _coords.Tag = 0;
            _coords.Clicked = CoordsClick;
            Extensions.ButtonSetGfx(_coords, 1000061);
            _view.FindChild("EnableAll", out _enableAll);
            _enableAll.Clicked = EnableAllClick;
            Extensions.ButtonSetGfx(_enableAll, 1000072);
            _view.FindChild("DisableAll", out _disableAll);
            _disableAll.Clicked = DisableAllClick;
            Extensions.ButtonSetGfx(_disableAll, 1000071);

            for (int i = 0; i < _pfIds.Length; i++)
            {
                string pfName = Utils.UnsafePointerToString(N3InterfaceModule_t.GetPFName(_pfIds[i]));
                LocationViewEntry locationModel = new LocationViewEntry();
                locationModel.Root = View.CreateFromXml($"{Main.PluginDir}\\UI\\Views\\LocationEntryView.xml");
                locationModel.Root.Tag = _pfIds[i].ToString();
                locationModel.Root.FindChild("Background", out locationModel.Background);
                locationModel.Background.SetBitmap("LocationPreviewBg");
                locationModel.Root.FindChild("Toggle", out locationModel.Toggle);
                locationModel.Toggle.Tag = Main.Settings.Locations[pfName].State;
                if (Main.Settings.Locations[pfName].State)
                    Extensions.ButtonSetGfx(locationModel.Toggle, 1000036);
                else
                    Extensions.ButtonSetGfx(locationModel.Toggle, 1000046);
                locationModel.Toggle.Clicked = LocationClick;
                locationModel.Root.FindChild("Coord1", out locationModel.Coord1);
                Vector2 coord1 = Main.Settings.Locations[pfName].Bounds.Coord1;
                locationModel.Bounds = new Bounds();
                locationModel.Bounds.Coord1 = coord1;
                locationModel.Coord1.Text = coord1.X != 0 && coord1.Y != 0 ? coord1.ToString() : "";
                locationModel.Root.FindChild("Coord2", out locationModel.Coord2);
                Vector2 coord2 = Main.Settings.Locations[pfName].Bounds.Coord2;
                locationModel.Bounds.Coord2 = coord2;
                locationModel.Coord2.Text = coord2.X != 0 && coord2.Y != 0 ? coord2.ToString() : "";
                locationModel.Root.FindChild("Name", out locationModel.Name);
                locationModel.Name.Text = pfName;
                locationModel.PfId = _pfIds[i];
                Entries.Add(locationModel);
                _scrollListRoot.AddChild(locationModel.Root, false);
            }

            Root.AddChild(_view, false);
        }

        private void DisableAllClick(object sender, ButtonBase e)
        {
            Extensions.PlaySound(Main.Sounds.Click);

            foreach (LocationViewEntry viewEntry in Entries)
            {
                viewEntry.Toggle.Tag = false;
                Extensions.ButtonSetGfx(viewEntry.Toggle, 1000046);
            }
        }

        private void EnableAllClick(object sender, ButtonBase e)
        {
            Extensions.PlaySound(Main.Sounds.Click);

            foreach (LocationViewEntry viewEntry in Entries)
            {
                viewEntry.Toggle.Tag = true;
                Extensions.ButtonSetGfx(viewEntry.Toggle, 1000036);
            }
        }

        internal void BoundsCheck()
        {
            LocationViewEntry viewEntry = Entries.Where(x => x.PfId == Playfield.Identity.Instance && x.Coord1.Text != "" && x.Coord2.Text != "").FirstOrDefault();

            if (viewEntry == null)
            {
                Extensions.ButtonSetGfx(_coords, 1000061);
                _coords.Tag = 0;
                _bounds = new Bounds();
            }
            else
            {
                Extensions.ButtonSetGfx(_coords, 1000062);
                _coords.Tag = 2;

                if (viewEntry.Bounds.Coord1.X != 0)
                {
                    _bounds.Coord1 = viewEntry.Bounds.Coord1;
                    _bounds.Coord2 = viewEntry.Bounds.Coord2;
                }
            }
        }

        private void CoordsClick(object sender, ButtonBase e)
        {
            Extensions.PlaySound(Main.Sounds.Click);
            CoordsButtonState((int)e.Tag);
            e.Tag = (int)e.Tag + 1;

            if ((int)e.Tag == 3)
                e.Tag = 0;
        }

        internal void CoordsButtonState(int state)
        {
            Vector3 playerPos = DynelManager.LocalPlayer.Position;
            LocationViewEntry viewEntry = Entries.Where(x => x.PfId == Playfield.Identity.Instance).FirstOrDefault();

            if (viewEntry == null)
            {
                Chat.WriteLine($"{Playfield.Name} is not a valid zone for setting coordinate bounds.");
                return;
            }
            switch (state)
            {
                case 0:
                    Extensions.ButtonSetGfx(_coords, 1000061);
                    _bounds.Coord1 = new Vector2(Convert.ToInt32(playerPos.X), Convert.ToInt32(playerPos.Z));
                    Chat.WriteLine($"Coordinate 1 set: {_bounds.Coord1}");
                    _bounds.Coord2 = new Vector2();
                    break;
                case 1:
                    Extensions.ButtonSetGfx(_coords, 1000062);
                    _bounds.Coord2 = new Vector2(Convert.ToInt32(playerPos.X), Convert.ToInt32(playerPos.Z));
                    Chat.WriteLine($"Coordinate 2 set: {_bounds.Coord2}");
                    _bounds.Reorder();
                    if (viewEntry != null)
                    {
                        viewEntry.Bounds.Coord1 = _bounds.Coord1;
                        viewEntry.Bounds.Coord2 = _bounds.Coord2;
                        viewEntry.Coord1.Text = _bounds.Coord1.ToString();
                        viewEntry.Coord2.Text = _bounds.Coord2.ToString();

                        Chat.WriteLine($"Coordinate roll bounds for {Playfield.Name} set.\nfrom: {_bounds.Coord1} to: {_bounds.Coord2}\n" +
                            $"Warning: This will limit the roller search area for {Playfield.Name}");
                    }
                    break;
                case 2:
                    Extensions.ButtonSetGfx(_coords, 1000061);
                    Extensions.PlaySound(Main.Sounds.Click);
                    viewEntry.Coord1.Text = "";
                    viewEntry.Coord2.Text = "";
                    _bounds = new Bounds();
                    viewEntry.Bounds = new Bounds();
                    Chat.WriteLine($"Coordinate roll bounds for {Playfield.Name} removed.");
                    break;
                default:
                    // code block
                    break;
            }
        }

        internal void DrawBounds()
        {
            if (_bounds.Coord1.X == 0)
                return;

            Vector3 playerPos = DynelManager.LocalPlayer.Position;

            if (_bounds.Coord2.X == 0)
            {
                float xCoord = playerPos.X - _bounds.Coord1.X;
                float yCoord = playerPos.Z - _bounds.Coord1.Y;

                Debug.DrawLine(
                    new Vector3(_bounds.Coord1.X, playerPos.Y, _bounds.Coord1.Y),
                    new Vector3(_bounds.Coord1.X + xCoord, playerPos.Y, _bounds.Coord1.Y),
                    DebuggingColor.Green);

                Debug.DrawLine(
                    new Vector3(_bounds.Coord1.X, playerPos.Y, _bounds.Coord1.Y),
                    new Vector3(_bounds.Coord1.X, playerPos.Y, _bounds.Coord1.Y + yCoord),
                    DebuggingColor.Green);

                Debug.DrawLine(
                    playerPos,
                    new Vector3(playerPos.X - xCoord, playerPos.Y, playerPos.Z),
                    DebuggingColor.Green);

                Debug.DrawLine(
                    playerPos,
                    new Vector3(playerPos.X, playerPos.Y, playerPos.Z - yCoord),
                    DebuggingColor.Green);
            }
            else
            {
                int xCoord = (int)(_bounds.Coord2.X - _bounds.Coord1.X);
                int yCoord = (int)(_bounds.Coord2.Y - _bounds.Coord1.Y);

                Debug.DrawLine(
                    new Vector3(_bounds.Coord1.X, playerPos.Y, _bounds.Coord1.Y),
                    new Vector3(_bounds.Coord1.X + xCoord, playerPos.Y, _bounds.Coord1.Y),
                    DebuggingColor.Green);

                Debug.DrawLine(
                    new Vector3(_bounds.Coord1.X, playerPos.Y, _bounds.Coord1.Y),
                    new Vector3(_bounds.Coord1.X, playerPos.Y, _bounds.Coord1.Y + yCoord),
                    DebuggingColor.Green);

                Debug.DrawLine(
                    new Vector3(_bounds.Coord2.X, playerPos.Y, _bounds.Coord2.Y),
                    new Vector3(_bounds.Coord2.X - xCoord, playerPos.Y, _bounds.Coord2.Y),
                    DebuggingColor.Green);

                Debug.DrawLine(
                    new Vector3(_bounds.Coord2.X, playerPos.Y, _bounds.Coord2.Y),
                    new Vector3(_bounds.Coord2.X, playerPos.Y, _bounds.Coord2.Y - yCoord),
                    DebuggingColor.Green);
            }
        }
        private void LocationClick(object sender, ButtonBase e)
        {
            Extensions.PlaySound(Main.Sounds.Click);

            bool on = (bool)e.Tag;

            if (!on)
                Extensions.ButtonSetGfx((Button)e, 1000036);
            else
                Extensions.ButtonSetGfx((Button)e, 1000046);

            e.Tag = !on;
        }
    }
    public class LocationViewEntry
    {
        public View Root;
        public BitmapView Background;
        public Button Toggle;
        public TextView Name;
        public Button SetBounds;
        public Button Remove;
        public TextView Coord1;
        public TextView Coord2;
        public Bounds Bounds;
        public int PfId;
    }
    public class Bounds
    {
        public Vector2 Coord1;
        public Vector2 Coord2;

        public void Reorder()
        {
            Vector2 _coord1 = Coord1;
            Vector2 _coord2 = Coord2;

            if (_coord1.X > _coord2.X)
            {
                Coord2.X = _coord1.X;
                Coord1.X = _coord2.X;
            }
            if (_coord1.Y > _coord2.Y)
            {
                Coord2.Y = _coord1.Y;
                Coord1.Y = _coord2.Y;
            }
        }
        public bool Contains(Vector3 point)
        {
            return point.X >= Coord1.X && point.X <= Coord2.X;
        }
    }
}