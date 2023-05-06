using System;
using System.Collections.Generic;

namespace Triggered.modules.options
{
    public class Options_Manager
    {
        public Options_MainMenu MainMenu = new Options_MainMenu();
        public Options_StashSorter StashSorter = new Options_StashSorter();
        public IEnumerable<Options> Itterate()
        {
            yield return MainMenu;
            yield return StashSorter;
        }
        public void Save()
        {
            foreach (var options in Itterate())
            {
                options.Save();
            }
        }
        public void Save(Type type)
        {
            foreach (var options in Itterate())
            {
                if (options.GetType() == type)
                {
                    options.Save();
                    break;
                }
            }
        }
        public void Load()
        {
            foreach (var options in Itterate())
            {
                options.Load();
            }
        }
        public void Load(Type type)
        {
            foreach (var options in Itterate())
            {
                if (options.GetType() == type)
                {
                    options.Load();
                    break;
                }
            }
        }
        public void SaveChanged()
        {
            foreach (var options in Itterate())
            {
                if (options._changed)
                    options.SaveChanged();
            }
        }
    }
}
