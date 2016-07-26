/*
 *  Copyright © 2016 Russell Libby
 */

using BandWeather.Pages;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace BandWeather
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : INotifyPropertyChanged
    {
        #region Private fields

        private TransitionCollection _transitions;
        private bool _syncing = true;
        private bool _paired;
        private bool _tileAdded;

        #endregion

        #region Private methods

        /// <summary>
        /// Determines if we can sync to and/or remove the band tile.
        /// </summary>
        /// <returns>True if connected to a band tile and not currently syncing.</returns>
        private bool CanSyncOrRemove()
        {
            return (_paired && _tileAdded && !_syncing);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;

            if (rootFrame == null) throw new ArgumentException(@"RootFrame is null.");

            rootFrame.ContentTransitions = _transitions ?? new TransitionCollection { new NavigationThemeTransition() };
            rootFrame.Navigated -= RootFrame_FirstNavigated;
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;

            if (!((e.PreviousExecutionState == ApplicationExecutionState.Running) || (e.PreviousExecutionState == ApplicationExecutionState.Suspended)))
            {
                /* Add persistent loading here */
            }

            if (rootFrame == null)
            {
                rootFrame = new Frame { CacheSize = 1 };
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                if (rootFrame.ContentTransitions != null)
                {
                    _transitions = new TransitionCollection();

                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        _transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += RootFrame_FirstNavigated;

                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments)) throw new Exception("Failed to create initial page");
            }

            Window.Current.Activate();
        }

        #endregion

        #region Constuctor

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        #endregion

        #region Public methods


        #endregion

        #region Public properties

        /// <summary>
        /// True if a tile can be added, otherwise false.
        /// </summary>
        public bool CanAddTile
        {
            get { return (_paired && !_tileAdded && !_syncing); }
        }

        /// <summary>
        /// True if a tile can be removed, otherwise false.
        /// </summary>
        public bool CanRemoveTile
        {
            get { return CanSyncOrRemove(); }
        }

        /// <summary>
        /// True if we can sync items with the band, otherwise false.
        /// </summary>
        public bool CanSync
        {
            get { return CanSyncOrRemove(); }
        }

        /// <summary>
        /// Returns the menu description based on alternate site value.
        /// </summary>
        public string SiteDescription
        {
            get { return UseAlternateSource ? "use primary site" : "use secondary site"; }
        }

        /// <summary>
        /// True if the alternate azure site should be used.
        /// </summary>
        public bool UseAlternateSource
        {
            get
            {
                var localSettings = ApplicationData.Current.LocalSettings;

                return (localSettings.Values["UseAlternateSource"] != null) && (bool)localSettings.Values["UseAlternateSource"];
            }
            set
            {
                var localSettings = ApplicationData.Current.LocalSettings;

                localSettings.Values["UseAlternateSource"] = value;

                if (PropertyChanged == null) return;

                PropertyChanged(this, new PropertyChangedEventArgs("UseAlternateSource"));
                PropertyChanged(this, new PropertyChangedEventArgs("SiteDescription"));
            }
        }

        /// <summary>
        /// True if a band is paired, otherwise false.
        /// </summary>
        public bool IsPaired
        {
            get { return _paired; }
            set
            {
                _paired = value;

                if (PropertyChanged == null) return;

                PropertyChanged(this, new PropertyChangedEventArgs("IsPaired"));
            }
        }

        /// <summary>
        /// True if a band is paired, otherwise false.
        /// </summary>
        public bool IsTileAdded
        {
            get { return _tileAdded; }
            set
            {
                _tileAdded = value;

                if (PropertyChanged == null) return;

                PropertyChanged(this, new PropertyChangedEventArgs("IsTileAdded"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanAddTile"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanRemoveTile"));
            }
        }

        /// <summary>
        /// Returns the visibility state for the progress bar when syncing.
        /// </summary>
        public Visibility SyncVisibility
        {
            get { return (_syncing ? Visibility.Visible : Visibility.Collapsed); }
        }

        /// <summary>
        /// Returns true if we are already syncing with the band.
        /// </summary>
        public bool IsSyncing
        {
            get { return _syncing; }
            set
            {
                _syncing = value;

                if (PropertyChanged == null) return;

                PropertyChanged(this, new PropertyChangedEventArgs("IsSyncing"));
                PropertyChanged(this, new PropertyChangedEventArgs("SyncVisibility"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanSync"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanAddTile"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanRemoveTile"));
            }
        }

        /// <summary>
        /// True if the sync was initiated by the background task, otherwise false.
        /// </summary>
        public bool BackgroundSync { get; set; }

        /// <summary>
        /// The event that is fired when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Returns the current instance of the application.
        /// </summary>
        public static new App Current
        {
            get { return (App)Application.Current; }
        }

        #endregion
    }
}