﻿namespace Lumina
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using Lumina.CompatibilityPolice;
    using Lumina.CompChecker;
    using System.IO;
    using UnityEngine;
    using static Lumina.MainAdvancedTab;

    /// <summary>
    /// The Lumina UI panel.
    /// </summary>
    public sealed class LuminaPanel : StandalonePanel
    {
        /// <summary>
        /// Width of panel content.
        /// </summary>
        internal const float ContentWidth = 500f;


        ExternalSettingsHandler handler;
        ShadowTab VisualismHandler;

        // Layout constants - private.
        private const float TitleHeight = 40f;
        private const float ContainerHeight = 650f;

        /// <summary>
        /// Gets the panel width.
        /// </summary>
        public override float PanelWidth => ContentWidth + (Margin * 2f);

        /// <summary>
        /// Gets the panel height.
        /// </summary>
        public override float PanelHeight => TitleHeight + ContainerHeight;

        /// <summary>
        /// Gets the panel's title.
        /// </summary>
        protected override string PanelTitle => Translations.Translate(LuminaTR.TranslationID.MOD_NAME);

        /// <summary>
        /// Gets the panel opacity.
        /// </summary>
        protected override float PanelOpacity => 0.8f;

        /// <summary>
        /// Called by Unity before first display.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Add tabstrip.
            AutoTabstrip tabStrip = AutoTabstrip.AddTabstrip(this, Margin, TitleHeight, ContentWidth, ContainerHeight, out _);

            // Add tabs and panels.
            new LightingTab(tabStrip, 0);
            new StylesTab(tabStrip, 1);
            new ShadowTab(tabStrip, 2);

            if (ModUtils.IsModEnabled("lutcreator"))
            {
                Debug.Log("[LUMINA] LUT Creator plugin enabled.");
                new LookUpTableTab(tabStrip, 3);
            }


            // Force initial tab selection.
            tabStrip.selectedIndex = -1;
            tabStrip.selectedIndex = 0;

            SetIcon(UITextures.LoadSprite("UUI"), "normal");
            VisualismHandler.LoadSettings();
            handler.LoadSettings();

        }

        /// <summary>
        /// Performs any actions required before closing the panel and checks that it's safe to do so.
        /// </summary>
        /// <returns>Always true.</returns>
        protected override bool PreClose()
        {
            // Deselect UUI button.
            LuminaLogic.Instance?.ResetButton();
            LuminaLogic.Instance.UUIButton.IsPressed = false;
            // Save settings.
            ModSettings.Save();

            // Always okay to close.
            return true;
        }
    }
}