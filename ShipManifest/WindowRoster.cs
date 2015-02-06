using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using ConnectedLivingSpace;

namespace ShipManifest
{
    internal static class WindowRoster
    {
        internal static string ToolTip = "";
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = Settings.RosterToolTips;

        internal static bool resetRosterSize = true;
        private static KerbalModel _selectedKerbal;
        internal static KerbalModel SelectedKerbal
        {
            get { return _selectedKerbal; }
            set
            {
                _selectedKerbal = value;
                if (_selectedKerbal == null)
                {
                    SMAddon.smController.saveMessage = string.Empty;
                    resetRosterSize = true;
                }
            }
        }
        private static Vector2 ScrollViewerPosition = Vector2.zero;
        internal static void Display(int windowId)
        {
            // Reset Tooltip active flag...
            ToolTipActive = false;
            ShowToolTips = Settings.RosterToolTips;

            Rect rect = new Rect(396, 4, 16, 16);
            if (GUI.Button(rect, new GUIContent("", "Close Window")))
            {
                Settings.ShowRoster = false;
                ToolTip = "";
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.RosterPosition, GUI.tooltip, ref ToolTipActive, 0, 0);
            try
            {
                GUIStyle style = GUI.skin.button;
                var defaultColor = style.normal.textColor;
                GUILayout.BeginVertical();

                RosterListViewer();

                if (SelectedKerbal != null)
                {
                    GUILayout.Label(SelectedKerbal.IsNew ? "Create a Kerbal" : "Edit a Kerbal");
                    if (SelectedKerbal.IsNew)
                        SelectedKerbal.Name = GUILayout.TextField(SelectedKerbal.Name);
                    else
                        GUILayout.Label(SelectedKerbal.Name + " - (" + SelectedKerbal.Kerbal.experienceTrait.Title + ")");

                    if (!string.IsNullOrEmpty(SMAddon.smController.saveMessage))
                    {
                        GUILayout.Label(SMAddon.smController.saveMessage, ManifestStyle.ErrorLabelRedStyle);
                    }

                    GUILayout.Label("Courage");
                    SelectedKerbal.Courage = GUILayout.HorizontalSlider(SelectedKerbal.Courage, 0, 1);

                    GUILayout.Label("Stupidity");
                    SelectedKerbal.Stupidity = GUILayout.HorizontalSlider(SelectedKerbal.Stupidity, 0, 1);

                    SelectedKerbal.Badass = GUILayout.Toggle(SelectedKerbal.Badass, "Badass");

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Cancel", GUILayout.MaxWidth(50)))
                    {
                        SelectedKerbal = null;
                    }
                    if (GUILayout.Button("Apply", GUILayout.MaxWidth(50)))
                    {
                        SMAddon.smController.saveMessage = SelectedKerbal.SubmitChanges();
                        if (string.IsNullOrEmpty(SMAddon.smController.saveMessage))
                            SelectedKerbal = null;
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    if (GUILayout.Button("Create Kerbal", GUILayout.MaxWidth(120)))
                    {
                        SelectedKerbal = SMAddon.smController.CreateKerbal();
                    }
                }

                GUILayout.EndVertical();
                GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Roster Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static void RosterListViewer()
        {
            try
            {
                ScrollViewerPosition = GUILayout.BeginScrollView(ScrollViewerPosition, GUILayout.Height(200), GUILayout.Width(400));
                GUILayout.BeginVertical();

                foreach (ProtoCrewMember kerbal in HighLogic.CurrentGame.CrewRoster.Crew)
                {
                    GUIStyle labelStyle = null;
                    if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
                        labelStyle = ManifestStyle.LabelStyleRed;
                    else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
                        labelStyle = ManifestStyle.LabelStyleYellow;
                    else
                        labelStyle = ManifestStyle.LabelStyle;

                    // What vessel is this Kerbal Assigned to?
                    string vesselName = "";
                    if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
                    {
                        foreach (Vessel thisVessel in FlightGlobals.Vessels)
                        {
                            List<ProtoCrewMember> crew = thisVessel.GetVesselCrew();
                            foreach (ProtoCrewMember crewMember in crew)
                            {
                                if (crewMember == kerbal)
                                {
                                    vesselName = "\r\n  -  " + thisVessel.name.Replace("(unloaded)", "");
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Since the kerbal has no vessel assignment, lets show what their status is instead...
                        vesselName = "\r\n  -  " + kerbal.rosterStatus;
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("{0}{1}", kerbal.name + ", (" + kerbal.experienceTrait.Title + ")", vesselName), labelStyle, GUILayout.Width(230), GUILayout.Height(10));  // + "  (" + kerbal.seat.vessel.name + ")"
                    string buttonText = string.Empty;
                    string buttonToolTip = string.Empty;

                    if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                        GUI.enabled = true;
                    else
                        GUI.enabled = false;

                    buttonText = (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal) ? "Edit" : "Cancel";
                    if (GUI.enabled)
                        buttonToolTip = (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal) ? "Edit this Kerbal's characteristics" : "Cancel any changes to this Kerbal";
                    else
                        buttonToolTip = "Kerbal is not available at this time.\r\nEditing is disabled";

                    if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(60)))
                    {
                        if (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal)
                        {
                            SelectedKerbal = new KerbalModel(kerbal, false);
                        }
                        else
                        {
                            SelectedKerbal = null;
                        }
                    }
                    Rect rect = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        ToolTip = Utilities.SetActiveTooltip(rect, Settings.RosterPosition, GUI.tooltip, ref ToolTipActive, 30, 20-ScrollViewerPosition.y);

                    if (((Settings.RealismMode && SMAddon.smController.IsPreLaunch) || !Settings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && SMAddon.smController.SelectedPartSource != null && !SMController.PartCrewIsFull(SMAddon.smController.SelectedPartSource))
                    {
                        GUI.enabled = true;
                        buttonText = "Add";
                        buttonToolTip = "Adds a kerbal to the Selected Source Part,\r\nin the first available seat.";
                    }
                    else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
                    {
                        GUI.enabled = true;
                        buttonText = "Respawn";
                        buttonToolTip = "Brings a Kerbal back to life.\r\nWill then become available.";
                    }
                    else if (((Settings.RealismMode && SMAddon.smController.IsPreLaunch) || !Settings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned && FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal))
                    {
                        GUI.enabled = true;
                        buttonText = "Remove";
                        buttonToolTip = "Removes a Kerbal from the active vessel.\r\nWill then become available.";
                    }
                    else if (((Settings.RealismMode && SMAddon.smController.IsPreLaunch) || !Settings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && SMAddon.smController.SelectedPartSource == null)
                    {
                        GUI.enabled = false;
                        buttonText = "Add";
                        buttonToolTip = "Add Disabled.  No source part is selected.\r\nTo add a Kerbal, Select a Source Part with an available seat.";
                    }
                    else if ((Settings.RealismMode && !SMAddon.smController.IsPreLaunch) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                    {
                        GUI.enabled = false;
                        buttonText = "Add";
                        buttonToolTip = "Add Disabled.  Realism Settings are preventing this action.\r\nTo add a Kerbal, Change your realism Settings.";
                    }
                    else
                    {
                        GUI.enabled = false;
                        buttonText = "--";
                            buttonToolTip = "Kerbal is not available (" + kerbal.rosterStatus + ").\r\nCurrent status does not allow any action.";
                    }

                    if (GUILayout.Button(new GUIContent(buttonText,buttonToolTip), GUILayout.Width(60)))
                    {
                        if (buttonText == "Add")
                            SMController.AddCrew(kerbal, SMAddon.smController.SelectedPartSource);
                        else if (buttonText == "Respawn")
                            SMController.RespawnKerbal(kerbal);
                        else if (buttonText == "Remove")
                        {
                            // get part...
                            Part part = SMAddon.smController.FindPart(kerbal);
                            if (part != null)
                                SMController.RemoveCrew(kerbal, part);
                        }
                    }
                    Rect rect2 = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        ToolTip = Utilities.SetActiveTooltip(rect2, Settings.RosterPosition, GUI.tooltip, ref ToolTipActive, 30, 20-ScrollViewerPosition.y);
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }

                GUILayout.EndVertical();
                GUILayout.EndScrollView();

            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in RosterListViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }
    }
}
