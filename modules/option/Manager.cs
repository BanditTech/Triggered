using System;
using System.Collections.Generic;
using System.Linq;

namespace Triggered.modules.options
{
    /// <summary>
    /// Manage all Options objects in one place.
    /// Instantiate to App.Options for ease of use.
    /// </summary>
    public class Manager
    {
        /// <summary>
        /// Panel Options object
        /// </summary>
        public Options_Panel Panel = new();
        /// <summary>
        /// Log Options object
        /// </summary>
        public Options_Log Log = new();
        /// <summary>
        /// Viewport Options object
        /// </summary>
        public Options_Viewport Viewport = new();
        /// <summary>
        /// Font Options object
        /// </summary>
        public Options_Font Font = new();
        /// <summary>
        /// StashSorter Options object
        /// </summary>
        public Options_StashSorter StashSorter = new();
        /// <summary>
        /// StashSorter Options object
        /// </summary>
        public Options_DemoCV DemoCV = new();
        /// <summary>
        /// Locations Options object
        /// </summary>
        public Options_Locations Locations = new();
        /// <summary>
        /// Locations Options object
        /// </summary>
        public Options_Colors Colors = new();
        /// <summary>
        /// Allows to itterate this list of Options.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Options> Iterate()
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
            foreach (var options in Iterate())
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
            foreach (var options in Iterate())
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
            foreach (var options in Iterate())
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
            foreach (var options in Iterate())
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
            foreach (var options in Iterate())
            {
                if (options._changed)
                    options.SaveChanged();
            }
        }
    }
}
