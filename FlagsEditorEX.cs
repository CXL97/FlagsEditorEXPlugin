﻿namespace FlagsEditorEXPlugin
{
    public class FlagsEditorEX : IPlugin
    {
        public string Name => LocalizedStrings.Find("MainWin.TitleName", "Flags Editor EX");
        public int Priority => 100; // Loading order, lowest is first.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ISaveFileProvider SaveFileEditor { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private ToolStripMenuItem? ctrl;

        private ToolStripMenuItem? menuEntry_EditFlags;
        private ToolStripMenuItem? menuEntry_DumpAllFlags;

        public void Initialize(params object[] args)
        {
            LocalizedStrings.Initialize(GameInfo.CurrentLanguage);
            SaveFileEditor = (ISaveFileProvider)Array.Find(args, z => z is ISaveFileProvider)!;
            var menu = (ToolStrip)Array.Find(args, z => z is ToolStrip)!;
            LoadMenuStrip(menu);
        }

        private void LoadMenuStrip(ToolStrip menuStrip)
        {
            var items = menuStrip.Items;
            var tools = (ToolStripDropDownItem)items.Find("Menu_Tools", false)[0];
            AddPluginControl(tools);
        }


        private void AddPluginControl(ToolStripDropDownItem tools)
        {
            ctrl = new ToolStripMenuItem(Name);
            ctrl.Enabled = false;
            tools.DropDownItems.Add(ctrl);

            menuEntry_EditFlags = new ToolStripMenuItem(LocalizedStrings.Find("FlagsEditorEX.menuEntry_EditFlags", "Edit flags..."));
            menuEntry_EditFlags.Enabled = false;
            menuEntry_EditFlags.Click += EditFlags_UIEvt;
            ctrl.DropDownItems.Add(menuEntry_EditFlags);

            menuEntry_DumpAllFlags = new ToolStripMenuItem(LocalizedStrings.Find("FlagsEditorEX.menuEntry_DumpAllFlags", "Dump all Flags"));
            menuEntry_DumpAllFlags.Enabled = false;
            menuEntry_DumpAllFlags.Click += DumpAllFlags_UIEvt;
            ctrl.DropDownItems.Add(menuEntry_DumpAllFlags);
        }

        private void DumpAllFlags_UIEvt(object? sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "Text File|*.txt",
                FileName = string.Format("flags_dump_{0}.txt", SaveFileEditor.SAV.Version),
            };
            var result = saveFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var flagsOrganizer = FlagsOrganizer.CreateFlagsOrganizer(SaveFileEditor.SAV, resData: null);
                System.IO.File.WriteAllText(saveFileDialog.FileName, flagsOrganizer.DumpAllFlags());
            }
            saveFileDialog.Dispose();
        }

        private void EditFlags_UIEvt(object? sender, EventArgs e)
        {
            var flagsOrganizer = FlagsOrganizer.CreateFlagsOrganizer(SaveFileEditor.SAV, resData: null);
            var form = new Forms.MainWin(flagsOrganizer);
            form.ShowDialog();
            form.Dispose();
        }

        public void NotifySaveLoaded()
        {
            ctrl!.Enabled = true;
            menuEntry_DumpAllFlags!.Enabled = true;
            menuEntry_EditFlags!.Enabled = true;

            var savData = SaveFileEditor.SAV;

            // Prevent usage if state is not Exportable
            if (!savData.State.Exportable)
            {
                ctrl.Enabled = false;
                return;
            }

            ctrl.Enabled = savData.Version switch
            {
                GameVersion.Any or
                GameVersion.RBY or
                GameVersion.StadiumJ or
                GameVersion.Stadium or
                GameVersion.Stadium2 or
                GameVersion.RSBOX or
                GameVersion.COLO or
                GameVersion.XD or
                GameVersion.CXD or
                GameVersion.BATREV or
                GameVersion.ORASDEMO or
                GameVersion.GO or
                GameVersion.Invalid
                    => false,

                // Check for AS Demo
                GameVersion.AS
                    => savData is not SAV6AODemo,

                // Check for SN Demo
                GameVersion.SN
                    // Can't have a renamed box which is locked in non-demo version
                    => !(((SAV7SM)savData).BoxLayout.BoxesUnlocked == 8 && string.IsNullOrWhiteSpace(((SAV7SM)savData).BoxLayout.GetBoxName(10))),

                _ => true
            };

#if DEBUG
            // Quick dump all flags on load during DEBUG
            if (ctrl.Enabled)
            {
                var flagsOrganizer = FlagsOrganizer.CreateFlagsOrganizer(SaveFileEditor.SAV, resData: null);
                System.IO.File.WriteAllText(string.Format("flags_dump_{0}.txt", SaveFileEditor.SAV.Version), flagsOrganizer.DumpAllFlags());
            }
#endif
        }

        public void NotifyDisplayLanguageChanged(string language)
        {
            // Refresh language if needed
            LocalizedStrings.Initialize(language);

            ctrl!.Text = Name;
            menuEntry_DumpAllFlags!.Text = LocalizedStrings.Find("FlagsEditorEX.menuEntry_DumpAllFlags", "Dump all Flags");
            menuEntry_EditFlags!.Text = LocalizedStrings.Find("FlagsEditorEX.menuEntry_EditFlags", "Edit flags...");
        }

        public bool TryLoadFile(string filePath)
        {
            return false; // no action taken
        }

    }

}
