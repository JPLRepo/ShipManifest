﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
    internal class Hatch
    {
        private PartModule _hatchModule;
        internal PartModule HatchModule
        {
            get { return _hatchModule; }
            set { _hatchModule = value; }
        }

        private ICLSPart _clsPart;
        internal ICLSPart CLSPart
        {
            get { return _clsPart; }
            set { _clsPart = value; }
        }

        internal bool HatchOpen
        {
            get { return iModule.HatchOpen; }
            set { iModule.HatchOpen = value; }
        }

        internal string HatchStatus
        {
            get { return iModule.HatchStatus; }
        }

        internal bool IsDocked
        {
            get { return iModule.IsDocked; }
        }

        internal string Title
        {
            get { return iModule.ModDockNode.part.parent.partInfo.title; }
        }

        private IModuleDockingHatch iModule
        {
            get { return (IModuleDockingHatch)this.HatchModule; }
        }

        internal Hatch() { }
        internal Hatch(PartModule pModule, ICLSPart iPart)
        {
            this.HatchModule = pModule;
            this.CLSPart = iPart;
        }

        internal void OpenHatch()
        {
            iModule.HatchEvents["CloseHatch"].active = true;
            iModule.HatchEvents["OpenHatch"].active = false;
            iModule.HatchOpen = true;
            SMAddon.FireEventTriggers();
        }
        internal void CloseHatch()
        {
            iModule.HatchEvents["CloseHatch"].active = false;
            iModule.HatchEvents["OpenHatch"].active = true;
            iModule.HatchOpen = false;
            SMAddon.FireEventTriggers();
        }

        internal void Highlight()
        {
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                if (iModule.HatchOpen)
                    iModule.ModDockNode.part.SetHighlightColor(Settings.Colors[Settings.HatchOpenColor]);
                else
                    iModule.ModDockNode.part.SetHighlightColor(Settings.Colors[Settings.HatchCloseColor]);
                iModule.ModDockNode.part.SetHighlight(true, false);
            }
            else
            {
                if (iModule.ModDockNode.part.highlightColor == Settings.Colors[Settings.HatchOpenColor] || iModule.ModDockNode.part.highlightColor == Settings.Colors[Settings.HatchCloseColor])
                {
                    if (Settings.EnableCLS && SMAddon.smController.SelectedResource == "Crew" && Settings.ShowTransferWindow)
                    {
                        CLSPart.Highlight(true, true);
                    }
                    else
                    {
                        iModule.ModDockNode.part.SetHighlight(false, false);
                        iModule.ModDockNode.part.SetHighlightDefault();
                        iModule.ModDockNode.part.SetHighlightType(Part.HighlightType.OnMouseOver);
                    }
                }
            }
        }
    }

}
