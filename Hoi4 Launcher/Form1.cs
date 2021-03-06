﻿using Hoi4_Launcher.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using ImgButton;

namespace Hoi4_Launcher
{
    public partial class Form1 : Form
    {
        private static string ParadoxFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Paradox Interactive");
        private static string Hoi4_Doc = Path.Combine(ParadoxFolder, "Hearts of Iron IV");
        private static string Hoi4_Enb_Mods = Path.Combine(Hoi4_Doc, "dlc_load.json");
        private static string Hoi4_Mods = Path.Combine(Hoi4_Doc, "mods_registry.json");
        private static dlcModel[] dis_dlc = null;


        static launchSettings data = new launchSettings();
        public Form1()
        {
            InitializeComponent();
            dis_dlc = GetDLCs();
            load();
        }

        public dlcModel[] GetDLCs()
        {
            List<dlcModel> dlcs = new List<dlcModel>();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "dlc");

            foreach (var dir in Directory.GetDirectories(path))
            {
                try
                {
                    DirectoryInfo dInfo = new DirectoryInfo(dir);
                    var dlcFullPath = dInfo.GetFilesByExtensions(".dlc").First().FullName;
                    var dlc = new dlcModel();
                    var x = File.ReadLines(dlcFullPath);
                    dlc.name = x.First().Split('"')[1].Replace('"', ' ');
                    dlc.path = x.ElementAt(1).Split('"')[1].Replace('"', ' ').Split('.').First() + ".dlc";
                    var party = x.ElementAt(x.Count() - 2).Split('=')[1].Replace(" ", "");
                    if (party == "yes")
                    { dlc._3rdparty = true; userControl11._3rdParty = true; }
                    else { dlc._3rdparty = false; userControl11._3rdParty = false; }
                    dlcs.Add(dlc);
                }
                catch (Exception ex)
                {
                }
            }
            return dlcs.ToArray();
        }

        private void load()
        {
            //Load Mods
            var items = load_items();
            var mods = load_mods_info();
            int enabled_mods = 0;
            foreach (var mod in mods)
            {
                bool enabled = false;
                if (items.enabled_mods.Contains(mod.gameRegistryId)) { enabled = true; enabled_mods++; }
                list_mods.Items.Add(mod.displayName, enabled);
                //var cacheimg = cacheImages(mod.displayName, "0").response.publishedfiledetails.First();
                //var img = cacheimg.previews.First();
                //var x  = img.url;
            }
            label_mods.Text = "Mods: " + enabled_mods + "/" + mods.Length;

            //Load DLC
            foreach (var dlc in dis_dlc)
            {
                bool enabled = true;
                if (items.disabled_dlcs.Contains(dlc.path)) { enabled = false; }
                list_dlc.Items.Add(dlc.name, enabled);
            }
            //Load LHSetthings
            string data = File.ReadAllText(@"launcher-settings.json");
            var obj = JsonConvert.DeserializeObject<LHSettings>(data);
            label_version.Text += " " + obj.version;
        }

        public launchSettings load_items()
        {
            launchSettings obj;

            string data = File.ReadAllText(Hoi4_Enb_Mods);
            obj = JsonConvert.DeserializeObject<launchSettings>(data);
            return obj;
        }

        public modInfo[] load_mods_info() {

            string data = File.ReadAllText(Hoi4_Mods);
            JObject mods = JObject.Parse(data);

            IList<JToken> results = mods.Children().Children().ToList();

            IList<modInfo> modsList = new List<modInfo>();

            foreach (JToken result in results)
            {
                modInfo mod = result.ToObject<modInfo>();
                modsList.Add(mod);
            }
            return modsList.ToArray();
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Button_play_Click(object sender, EventArgs e)
        {

        }

        public void SerializeConfig(object x)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            File.WriteAllText(Hoi4_Enb_Mods, JsonConvert.SerializeObject(x, Formatting.Indented, settings));
        }

        private void List_mods_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        public string GetURL(string url)
        {
            var client = new HttpClient();
            return client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
        }


        private void UserControl11_Click(object sender, EventArgs e)
        {
            var mods = load_mods_info();
            var enabled_mods = new List<string>();
            foreach (var mod in mods)
            {
                if (list_mods.CheckedItems.Contains(mod.displayName))
                {
                    if (mod.gameRegistryId != null)
                        enabled_mods.Add(mod.gameRegistryId);
                }
            }
            var disabled_dlc = new List<string>();
            foreach (var dlc in list_dlc.Items)
            {
                if (!list_dlc.CheckedItems.Contains(dlc))
                {
                    foreach (var disdlc in dis_dlc)
                    {
                        if (disdlc.name == dlc.ToString()) { disabled_dlc.Add(disdlc.path); }
                    }
                }
            }
            var config = load_items();
            config.enabled_mods = enabled_mods;
            config.disabled_dlcs = disabled_dlc;
            SerializeConfig(config);
            Process.Start(@"hoi4.exe");
            Application.Exit();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            bool thrd_party = false;
            foreach(var _dlc in list_dlc.CheckedItems)
            {
                foreach (dlcModel dlc in dis_dlc)
                {
                    if (dlc.name == _dlc.ToString() && dlc._3rdparty) { thrd_party = true; break; }
                }
                if (thrd_party) break;
            }


            userControl11._3rdParty = thrd_party;
        }

    }

}
