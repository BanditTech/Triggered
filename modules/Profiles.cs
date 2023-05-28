using ClickableTransparentOverlay.Win32;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Triggered.modules.options;

namespace Triggered.modules
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
        // Dictionary to store the checkbox state for each Options object
        private Dictionary<string, bool> selectedOptions = new Dictionary<string, bool>();

        public void Initialize()
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

        public bool RenderSave()
        {
            bool returnState = false;
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 200), ImGuiCond.FirstUseEver);
            ImGui.Begin("Save Profile");

            // Dropdown box to select a profile
            if (ImGui.BeginCombo("Profile", selectedProfile))
            {
                foreach (string profile in profileFiles)
                {
                    bool isSelected = (selectedProfile == profile);
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

        internal bool LoadProfile(Dictionary<string,bool> selected)
        {
            throw new NotImplementedException();
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
            string profileFilePath = Path.Combine(AppContext.BaseDirectory, "profiles", profileFileName);
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
            string profileFilePath = Path.Combine("profiles", profileFileName);
            File.Delete(profileFilePath);

            // Refresh the profile files list
            Initialize();
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
                    bool isSelected = (selectedProfile == profile);
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
                bool isSelected = selectedOptions.Keys.Contains(options.Name) ? selectedOptions[options.Name] : true;
                // Display the checkbox for the current Options object
                if (ImGui.Checkbox(options.Name, ref isSelected))
                    selectedOptions[options.Name] = isSelected;
            }

            // Load button
            if (ImGui.Button("Load"))
            {
                // Load the selected profile with the chosen Options
                LoadProfile(selectedOptions);

                ImGui.CloseCurrentPopup();
                returnState = true;
            }

            ImGui.End();

            if (Utils.IsKeyPressedAndNotTimeout(VK.ESCAPE)) // Escape.
                returnState = true;
            return returnState;
        }
    }
}
