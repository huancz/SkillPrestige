using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using SkillPrestige.Framework;
using SkillPrestige.Framework.Commands;
using SkillPrestige.Framework.InputHandling;
using SkillPrestige.Framework.Menus;
using SkillPrestige.Framework.Menus.Elements.Buttons;
using SkillPrestige.Logging;
using SkillPrestige.Menus;
using SkillPrestige.Mods;
using SkillPrestige.Professions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace SkillPrestige
{
    /// <summary>The mod entry class.</summary>
    internal class ModEntry : Mod
    {
        /**********
        ** Fields
        *********/
        private bool SaveIsLoaded { get; set; }


        /**********
        ** Accessors
        *********/
        public static string ModPath { get; private set; }

        public static ModConfig Config { get; private set; }

        public static IMonitor LogMonitor { get; private set; }

        public static string CurrentSaveOptionsPath => Path.Combine(ModPath, "psconfigs/", Constants.SaveFolderName);

        public static string PerSaveOptionsDirectory { get; private set; }

        public static Texture2D PrestigeIconTexture { get; private set; }

        public static Texture2D CheckmarkTexture { get; private set; }

        public static IModRegistry ModRegistry { get; private set; }


        /**********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // init
            LogMonitor = Monitor;
            ModPath = helper.DirectoryPath;
            ModRegistry = helper.ModRegistry;
            PerSaveOptionsDirectory = Path.Combine(ModPath, "psconfigs/");
            Config = helper.ReadConfig<ModConfig>();

            // disable mod if All Professions is installed
            if (this.Helper.ModRegistry.IsLoaded("community.AllProfessions"))
            {
                Logger.LogCriticalWarning("Conflict Detected. This mod cannot work with AllProfessions. Skill Prestige disabled.");
                Logger.LogDisplay("Skill Prestige Mod: If you wish to use this mod in place of AllProfessions, remove the AllProfessions mod and run the player_resetallprofessions command.");
                return;
            }

            // hook events
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Input.CursorMoved += this.OnCursorMoved;
            helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
        }


        /**********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            bool isClick = e.Button == SButton.MouseLeft;

            SkillsMenuExtension.OnButtonPressed(e, isClick);
            if (Game1.activeClickableMenu is IInputHandler handler)
                handler.OnButtonPressed(e, isClick);
        }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            SkillsMenuExtension.OnCursorMoved(e);
            if (Game1.activeClickableMenu is IInputHandler handler)
                handler.OnCursorMoved(e);
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            Logger.LogVerbose("New Day Started");
            AnimalProduceHandler.HandleSpawnedAnimalProductQuantityIncrease();
        }

        /// <summary>Raised after the player loads a save slot and the world is initialised.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            PrestigeSaveData.Instance.UpdateCurrentSaveFileInformation();
            PerSaveOptions.Instance.Check();
            Profession.AddMissingProfessions();
            SaveIsLoaded = true;
        }

        /// <summary>Raised after the game returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            PrestigeSaveData.Instance.Read();
            SaveIsLoaded = false;
            Logger.LogInformation("Return To Title.");
            PerSaveOptions.ClearLoadedPerSaveOptionsFile();
            ExperienceHandler.ResetExperience();
        }

        /// <summary>When a menu is open (<see cref="Game1.activeClickableMenu"/> isn't null), raised after that menu is drawn to the sprite batch but before it's rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            SkillsMenuExtension.AddPrestigeButtonsToMenu();
        }

        private void LoadSprites()
        {
            Logger.LogInformation("Loading sprites...");
            Button.DefaultButtonTexture = Game1.content.Load<Texture2D>(@"LooseSprites\DialogBoxGreen");
            MinimalistProfessionButton.ProfessionButtonTexture = Game1.content.Load<Texture2D>(@"LooseSprites\boardGameBorder");

            string prestigeIconFilePath = Path.Combine(ModPath, "assets", "prestige-icon.png");
            Logger.LogInformation($"Prestige Icon Path: {prestigeIconFilePath}");
            FileStream prestigeIconFileStream = new FileStream(prestigeIconFilePath, FileMode.Open);
            PrestigeIconTexture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, prestigeIconFileStream);

            string checkmarkFilePath = Path.Combine(ModPath, "assets", "checkmark.png");
            Logger.LogInformation($"Checkmark Path: {checkmarkFilePath}");
            FileStream checkmarkFileStream = new FileStream(checkmarkFilePath, FileMode.Open);
            CheckmarkTexture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, checkmarkFileStream);
            Logger.LogInformation("Sprites loaded.");
        }

        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // init mod
            LoadSprites();
            PrestigeSaveData.Instance.Read();
            ModHandler.Initialise();

            // register commands
            if (ModEntry.Config.TestingMode)
                RegisterTestingCommands();
            RegisterCommands();
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            CheckForGameSave();
            if (Game1.activeClickableMenu != null)
            {
                CheckForLevelUpMenu();
            }

            if (e.IsMultipleOf(30)) // half-second
                ToolProficiencyHandler.HandleToolProficiency(); //from what I can tell of the original game code, tools cannot be used quicker than 600ms, so a half second tick is the largest tick that will always catch that the tool was used.
            if (e.IsOneSecond)
                UpdateExperience(); //one second tick for this, as the detection of changed experience can happen as infrequently as possible. a 10 second tick would be well within tolerance.
        }

        private void UpdateExperience()
        {
            if (SaveIsLoaded)
                ExperienceHandler.UpdateExperience();
        }

        private void CheckForGameSave()
        {
            if (!Game1.newDay || Game1.fadeToBlackAlpha <= 0.95f)
                return;
            Logger.LogInformation("New game day detected.");
            PrestigeSaveData.Instance.Save();
        }

        private void CheckForLevelUpMenu()
        {
            foreach (LevelUpManager levelUpManager in Skill.AllSkills.Select(x => x.LevelUpManager))
            {
                if (!levelUpManager.IsMenu(Game1.activeClickableMenu))
                    continue;
                int currentLevel = levelUpManager.GetLevel.Invoke();
                if (currentLevel % 5 != 0)
                    return;
                Logger.LogInformation("Level up menu as profession chooser detected.");
                Skill currentSkill = levelUpManager.GetSkill.Invoke();
                Game1.activeClickableMenu = levelUpManager.CreateNewLevelUpMenu.Invoke(currentSkill, currentLevel);
                Logger.LogInformation("Replaced level up menu with custom menu.");
            }
        }

        private void RegisterTestingCommands()
        {
            Logger.LogInformation("Registering Testing commands...");
            SkillPrestigeCommand.RegisterCommands(this.Helper.ConsoleCommands, true);
            Logger.LogInformation("Testing commands registered.");
        }

        private void RegisterCommands()
        {
            Logger.LogInformation("Registering commands...");
            SkillPrestigeCommand.RegisterCommands(this.Helper.ConsoleCommands, false);
            Logger.LogInformation("Commands registered.");
        }
    }
}
