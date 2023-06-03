using ClickableTransparentOverlay.Win32;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Triggered.modules.options;

namespace Triggered.modules.option
{
    /// <summary>
    /// Produce and receive Profile Dictionary.
    /// </summary>
    public class Profiles
    {
        private string[] profileFiles;
        private string selectedProfile;
        private bool removeConfirmationPopup;
        private Dictionary<string, JObject> savedObjects;
        private Dictionary<string, bool> selectedOptions = App.Options.Itterate()
            .ToDictionary(options => options.Name, options => true);

        internal void Initialize()
        {
            // Load profile files from the profiles folder
            profileFiles = Directory.GetFiles("profile", "*.json");
            for (int i = 0; i < profileFiles.Length; i++)
            {
                // Remove the file extension and path to make it more readable
                profileFiles[i] = Path.GetFileNameWithoutExtension(profileFiles[i]);
            }

            selectedProfile = "";
            removeConfirmationPopup = false;
            savedObjects = new Dictionary<string, JObject>();
        }

        internal void LoadProfile()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "profile", selectedProfile);
            if (!File.Exists(path))
                return;
            JObject fileObj = (JObject)wrapper.JSON.Obj(File.ReadAllText(path));
            Dictionary<string, JToken> profile = fileObj.Properties()
                .ToDictionary(
                prop => prop.Name,
                prop => prop.Value
                );
            Dictionary<string, JToken> matchingOptions = selectedOptions
                .Where(option => option.Value && profile.ContainsKey(option.Key))
                .ToDictionary(option => option.Key, option => profile[option.Key]);
            App.Options.Itterate()
                .Where(option => matchingOptions.ContainsKey(option.Name))
                .ToList()
                .ForEach(option => option.Merge(matchingOptions[option.Name]));
        }

        private void SaveProfile()
        {
            if (string.IsNullOrEmpty(selectedProfile))
                return;
            // Create a new JObject to hold the saved options
            JObject profileObject = new JObject();


            // Iterate through all loaded Options objects
            savedObjects.Clear();
            foreach (Options options in App.Options.Itterate())
            {
                // Prepare the save object for each Options object
                JObject saveObject = options.PrepareSaveObject();
                // Use the Options object name as the key for the saved object
                savedObjects[options.Name] = saveObject;
            }

            // Save the profile object to a file with the selected profile name
            string profileFileName = selectedProfile + ".json";
            string profileFilePath = Path.Combine(AppContext.BaseDirectory, "profile", profileFileName);
            bool refreshNames = false;
            if (!File.Exists(profileFilePath))
                refreshNames = true;
            // We write the profile in human readable format
            File.WriteAllText(profileFilePath, wrapper.JSON.Str(profileObject));
            if (refreshNames)
                Initialize();
        }

        private void RemoveProfile()
        {
            // Remove the selected profile file from the profiles folder
            string profileFileName = selectedProfile + ".json";
            string profileFilePath = Path.Combine(AppContext.BaseDirectory, "profile", profileFileName);
            File.Delete(profileFilePath);

            // Refresh the profile files list
            Initialize();
        }

        private string newName = string.Empty;
        internal bool RenderSave()
        {
            bool returnState = false;
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 200), ImGuiCond.FirstUseEver);
            ImGui.Begin("Save Profile");

            ImGui.Text("Add a new Profile:");
            ImGui.InputText("##newName", ref newName, 256);
            ImGui.SameLine();
            if (ImGui.Button("Create") && IsValidFilename(newName))
            {
                selectedProfile = newName;
                newName = string.Empty;
                SaveProfile();
                ImGui.CloseCurrentPopup();
                returnState = true;
            }
            ImGui.Separator();
            ImGui.Text("Select an existing Profile:");
            // Dropdown box to select a profile
            if (ImGui.BeginCombo("##selectedProfile", selectedProfile))
            {
                foreach (string profile in profileFiles)
                {
                    bool isSelected = selectedProfile == profile;
                    if (ImGui.Selectable(profile, isSelected))
                    {
                        selectedProfile = profile;
                        removeConfirmationPopup = false;
                    }

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                if (profileFiles.Length == 0)
                    ImGui.Selectable(selectedProfile);
                ImGui.EndCombo();
            }
            ImGui.SameLine();
            // Save button
            if (ImGui.Button("Save"))
            {
                SaveProfile();
                ImGui.CloseCurrentPopup();
                returnState = true;
            }

            // Remove button
            ImGui.SameLine();
            bool isRemoveButtonDisabled = !profileFiles.Contains(selectedProfile);
            if (ImGui.Button("Remove", new System.Numerics.Vector2(60, 0)) && !isRemoveButtonDisabled)
                removeConfirmationPopup = true;
            // Confirmation popup for remove button
            if (removeConfirmationPopup)
                ImGui.OpenPopup("Confirm Remove");
            var modalOpen = true;
            if (ImGui.BeginPopupModal("Confirm Remove", ref modalOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Are you sure you want to remove this profile?");
                ImGui.Separator();

                if (ImGui.Button("Yes", new System.Numerics.Vector2(80, 0)))
                {
                    RemoveProfile();
                    ImGui.CloseCurrentPopup();
                    removeConfirmationPopup = false;
                    returnState = true;
                }

                ImGui.SameLine();

                if (ImGui.Button("No", new System.Numerics.Vector2(80, 0)))
                {
                    ImGui.CloseCurrentPopup();
                    removeConfirmationPopup = false;
                    returnState = true;
                }

                ImGui.EndPopup();
            }

            ImGui.End();

            if (Utils.IsKeyPressedAndNotTimeout(VK.ESCAPE)) //Escape.
                returnState = true;
            return returnState;
        }

        internal bool RenderLoad()
        {
            bool returnState = false;
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 200), ImGuiCond.FirstUseEver);
            ImGui.Begin("Load Profile");

            // Dropdown box to select a profile
            if (ImGui.BeginCombo("Profile", selectedProfile))
            {
                foreach (string profile in profileFiles)
                {
                    bool isSelected = selectedProfile == profile;
                    if (ImGui.Selectable(profile, isSelected))
                    {
                        selectedProfile = profile;
                    }

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                if (profileFiles.Length == 0)
                    ImGui.Selectable(selectedProfile);
                ImGui.EndCombo();
            }

            // Iterate through all loaded Options objects
            foreach (Options options in App.Options.Itterate())
            {
                bool isSelected = selectedOptions[options.Name];
                // Display the checkbox for the current Options object
                if (ImGui.Checkbox(options.Name, ref isSelected))
                    selectedOptions[options.Name] = isSelected;
            }

            // Load button
            if (ImGui.Button("Load"))
            {
                // Load the selected profile with the chosen Options
                LoadProfile();

                ImGui.CloseCurrentPopup();
                returnState = true;
            }

            ImGui.End();

            if (Utils.IsKeyPressedAndNotTimeout(VK.ESCAPE)) // Escape.
                returnState = true;
            return returnState;
        }

        private readonly char[] invalidChars = Path.GetInvalidFileNameChars();
        internal bool IsValidFilename(string fileName)
        {
            if (string.IsNullOrEmpty(newName))
                return false;
            if (fileName.IndexOfAny(invalidChars) >= 0)
                return false;
            return true;
        }
    }
}
