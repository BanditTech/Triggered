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
    public class Profile
    {
        private string[] profileFiles;
        private string selectedProfile;
        private bool removeConfirmationPopup;
        private Dictionary<string, JObject> savedObjects;

        public void Initialize()
        {
            // Load profile files from the profiles folder
            profileFiles = Directory.GetFiles("profiles", "*.json");
            for (int i = 0; i < profileFiles.Length; i++)
            {
                // Remove the file extension and path to make it more readable
                profileFiles[i] = Path.GetFileNameWithoutExtension(profileFiles[i]);
            }

            selectedProfile = "";
            removeConfirmationPopup = false;
            savedObjects = new Dictionary<string, JObject>();
        }

        public void Render()
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 200), ImGuiCond.FirstUseEver);
            ImGui.Begin("Profile Editor");

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
                ImGui.EndCombo();
            }

            // Save button
            if (ImGui.Button("Save"))
            {
                SaveProfile();
                ImGui.CloseCurrentPopup();
            }

            // Remove button
            ImGui.SameLine();
            bool isRemoveButtonDisabled = !profileFiles.Contains(selectedProfile);
            if (ImGui.Button("Remove", new System.Numerics.Vector2(60, 0)) && !isRemoveButtonDisabled)
                removeConfirmationPopup = true;
            // Confirmation popup for remove button
            if (removeConfirmationPopup)
                ImGui.OpenPopup("Confirm Remove");
            if (ImGui.BeginPopupModal("Confirm Remove", ref removeConfirmationPopup, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Are you sure you want to remove this profile?");
                ImGui.Separator();

                if (ImGui.Button("Yes", new System.Numerics.Vector2(80, 0)))
                {
                    RemoveProfile();
                    ImGui.CloseCurrentPopup();
                    removeConfirmationPopup = false;
                }

                ImGui.SameLine();

                if (ImGui.Button("No", new System.Numerics.Vector2(80, 0)))
                {
                    ImGui.CloseCurrentPopup();
                    removeConfirmationPopup = false;
                }

                ImGui.EndPopup();
            }

            ImGui.End();
        }

        private void SaveProfile()
        {
            // Create a new JObject to hold the saved options
            JObject profileObject = new JObject();

            // Iterate through all loaded Options objects
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
            File.WriteAllText(profileFilePath, profileObject.ToString());
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
    }
}
