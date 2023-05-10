using System;
using System.Collections.Generic;
using System.Linq;

namespace Triggered.modules.options
{
    /// <summary>
    /// Manage all Options objects in one place.
    /// Instantiate to App.Options for ease of use.
    /// </summary>
    public class Options_Manager
    {
        /// <summary>
        /// MainMenu Options object
        /// </summary>
        public Options_MainMenu MainMenu = new Options_MainMenu();
        /// <summary>
        /// StashSorter Options object
        /// </summary>
        public Options_StashSorter StashSorter = new Options_StashSorter();
        /// <summary>
        /// StashSorter Options object
        /// </summary>
        public Options_DemoCV DemoCV = new Options_DemoCV();
        /// <summary>
        /// Allows to itterate this list of Options.
        /// </summary>
        /// <returns></returns>
        //public IEnumerable<Options> Itterate()
        //{
        //    yield return MainMenu;
        //    yield return StashSorter;
        //}
        public IEnumerable<Options> Itterate()
        {
            // Get all fields in this class that are of type Options
            var optionsFields = this.GetType().GetFields().Where(f => f.FieldType.IsSubclassOf(typeof(Options)));

            // Yield each Options object
            foreach (var field in optionsFields)
            {
                yield return (Options)field.GetValue(this);
            }
        }

        /// <summary>
        /// Save all Options in the class.
        /// </summary>
        public void Save()
        {
            foreach (var options in Itterate())
            {
                options.Save();
            }
        }
        /// <summary>
        /// Save a specific Type of options.
        /// </summary>
        /// <param name="type"></param>
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
        /// <summary>
        /// Load all Options in the class.
        /// </summary>
        public void Load()
        {
            foreach (var options in Itterate())
            {
                options.Load();
            }
        }
        /// <summary>
        /// Load a specific Type of Options.
        /// </summary>
        /// <param name="type"></param>
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
        /// <summary>
        /// Save the changed Options.
        /// </summary>
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
