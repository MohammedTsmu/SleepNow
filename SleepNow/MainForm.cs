using System;
using System.Windows.Forms;

namespace SleepNow
{
    public partial class MainForm : Form
    {
        private bool closeToTray = true; // Prevents form from actually closing
        private Timer alertTimer;

        public MainForm()
        {
            InitializeComponent();
            InitializeFormControls();
            InitializeNotifyIcon();
            InitializeAlertTimer();
        }

        private void InitializeFormControls()
        {
            // Form Settings
            this.Text = "Sleep Now - Settings";
            this.Size = new System.Drawing.Size(400, 250);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Sleep Time Label
            Label lblSleepTime = new Label
            {
                Text = "Set Sleep Time:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lblSleepTime);

            // Sleep Time Picker
            DateTimePicker dtpSleepTime = new DateTimePicker
            {
                Name = "dtpSleepTime",
                Format = DateTimePickerFormat.Time,
                Location = new System.Drawing.Point(150, 15),
                Width = 200,
                Value = SettingsManager.CurrentSettings.SleepTime
            };
            this.Controls.Add(dtpSleepTime);

            // Pre-Alert Time Label
            Label lblAlertTime = new Label
            {
                Text = "Pre-Alert Time (minutes):",
                Location = new System.Drawing.Point(20, 60),
                AutoSize = true
            };
            this.Controls.Add(lblAlertTime);

            // Pre-Alert Numeric UpDown
            NumericUpDown numAlertTime = new NumericUpDown
            {
                Name = "numAlertTime",
                Minimum = 1,
                Maximum = 60,
                Value = SettingsManager.CurrentSettings.PreAlertMinutes,
                Location = new System.Drawing.Point(200, 55),
                Width = 50
            };
            this.Controls.Add(numAlertTime);

            // Restrict Changes Label
            Label lblRestrictChanges = new Label
            {
                Text = "Restrict Changes (hours):",
                Location = new System.Drawing.Point(20, 100),
                AutoSize = true
            };
            this.Controls.Add(lblRestrictChanges);

            // Restrict Changes Numeric UpDown
            NumericUpDown numRestrictChanges = new NumericUpDown
            {
                Name = "numRestrictChanges",
                Minimum = 1,
                Maximum = 24,
                Value = SettingsManager.CurrentSettings.RestrictionDuration / 60,
                Location = new System.Drawing.Point(200, 95),
                Width = 50
            };
            this.Controls.Add(numRestrictChanges);

            // Save Settings Button
            Button btnSaveSettings = new Button
            {
                Text = "Save Settings",
                Location = new System.Drawing.Point(150, 150),
                Width = 100
            };
            btnSaveSettings.Click += BtnSaveSettings_Click;
            this.Controls.Add(btnSaveSettings);
        }

        private void BtnSaveSettings_Click(object sender, EventArgs e)
        {
            // Retrieve Sleep Time and Alert Time
            var sleepTime = ((DateTimePicker)this.Controls["dtpSleepTime"]).Value;
            var alertTime = (int)((NumericUpDown)this.Controls["numAlertTime"]).Value;
            var restrictDuration = (int)((NumericUpDown)this.Controls["numRestrictChanges"]).Value;

            // Save to SettingsManager
            SettingsManager.CurrentSettings.SleepTime = sleepTime;
            SettingsManager.CurrentSettings.PreAlertMinutes = alertTime;
            SettingsManager.CurrentSettings.RestrictionDuration = restrictDuration * 60; // Convert to minutes
            SettingsManager.SaveSettings();

            MessageBox.Show($"Settings Saved!\nSleep Time: {sleepTime}\nAlert Time: {alertTime} minutes.",
                "Sleep Now", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void InitializeNotifyIcon()
        {
            // Setup NotifyIcon
            notifyIconSleepNow.Text = "Sleep Now - Running";
            notifyIconSleepNow.Icon = Properties.Resources.sleeping_mask; // Ensure AppIcon.ico is added to Resources
            notifyIconSleepNow.Visible = true;

            // Context menu for NotifyIcon
            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add("Open Settings", (s, e) => ShowForm());
            contextMenu.MenuItems.Add("Exit", (s, e) => ExitApplication());

            notifyIconSleepNow.ContextMenu = contextMenu;

            // Double-click action
            notifyIconSleepNow.DoubleClick += (s, e) => ShowForm();
        }

        private void ShowForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void ExitApplication()
        {
            closeToTray = false; // Allow the app to close
            notifyIconSleepNow.Visible = false;
            Application.Exit();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (closeToTray)
            {
                e.Cancel = true; // Cancel the close event
                this.Hide();
                notifyIconSleepNow.ShowBalloonTip(3000, "Sleep Now", "The application is still running in the system tray.", ToolTipIcon.Info);
            }
        }

        private void InitializeAlertTimer()
        {
            alertTimer = new Timer
            {
                Interval = 60000 // Check every minute
            };
            alertTimer.Tick += AlertTimer_Tick;
            alertTimer.Start();
        }

        private void AlertTimer_Tick(object sender, EventArgs e)
        {
            var currentTime = DateTime.Now;
            var sleepTime = SettingsManager.CurrentSettings.SleepTime;
            var preAlertMinutes = SettingsManager.CurrentSettings.PreAlertMinutes;

            // Check if current time matches pre-alert time
            if (sleepTime.Subtract(TimeSpan.FromMinutes(preAlertMinutes)) <= currentTime &&
                currentTime < sleepTime)
            {
                ShowPreAlertNotification();
            }
        }

        private void ShowPreAlertNotification()
        {
            notifyIconSleepNow.ShowBalloonTip(3000, "Sleep Now",
                $"It's time to prepare for sleep! Your computer will shut down in {SettingsManager.CurrentSettings.PreAlertMinutes} minutes.",
                ToolTipIcon.Warning);
        }
    }
}
