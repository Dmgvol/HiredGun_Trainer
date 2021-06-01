using HiredGunTrainer.MemoryUtils;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace HiredGunTrainer {
    public partial class MainWindow : Window {

        // game speeds
        private float[] gameSpeeds = new float[4] { 1.0f, 2.0f, 4.0f, 0.5f };
        private int currGameSpeed = 0;

        private float[] savedPos = new float[5]{0, 0, 0, 0, 0}; // X,Y,Z rotationSin, rotationCos
        private bool noclipFlag, ghostFlag, godFlag;
        private float playerSpeed = 0;
        private float[] playerPos = new float[3] { 0, 0, 0};


        private GlobalKeyboardHook kbHook = new GlobalKeyboardHook();
        public Timer updateTimer;
        private GameHook gameHook;

        public MainWindow() {
            InitializeComponent();
            // game hook
            gameHook = new GameHook(this);

            // hotkeys
            kbHook.KeyDown += InputKeyDown;
            kbHook.HookedKeys.Add(Keys.F1);
            kbHook.HookedKeys.Add(Keys.F2);
            kbHook.HookedKeys.Add(Keys.F3);
            kbHook.HookedKeys.Add(Keys.F4);
            kbHook.HookedKeys.Add(Keys.F5);
            kbHook.HookedKeys.Add(Keys.F6);
            // update timer
            updateTimer = new Timer {
                Interval = (16) // ~60 Hz
            };
            updateTimer.Tick += new EventHandler(Update);
            updateTimer.Start();
        }

        private void Update(object sender, EventArgs e) {
            // GAME HOOK
            gameHook.Update();
            if(!gameHook.hooked) return;

            // Read values

            // Update UI
            ToggleState(ghostFlag, ghostLabel);
            ToggleState(godFlag, godLabel);
            ToggleState(noclipFlag, noclipLabel);
            SetGameSpeed();
            positionBlock.Text = $"{playerPos[0] / 100:0.00}\n{playerPos[1] / 100:0.00}\n{playerPos[2] / 100:0.00}";
            speedLabel.Content = $"{playerSpeed} m/s";
        }


        private void InputKeyDown(object sender, KeyEventArgs e) {
            switch(e.KeyCode) {
                case Keys.F1:
                    ToggleGhost();
                    break;
                case Keys.F2:
                    ToggleGod();
                    break;
                case Keys.F3:
                    ToggleNoclip();
                    break;
                case Keys.F4:
                    ChangeGameSpeed();
                    break;
                case Keys.F5:
                    SavePosition();
                    break;
                case Keys.F6:
                    TeleportPlayer();
                    break;
                default:
                    break;
            }
            e.Handled = false;
        }

        public void DerefPointers() {
            
        }

        private void ToggleGhost() {
            if(!gameHook.hooked) return;

            ghostFlag = !ghostFlag;
            ToggleState(ghostFlag, ghostLabel);
        }

        private void ToggleGod() {
            if(!gameHook.hooked) return;

            godFlag = !godFlag;
            ToggleState(godFlag, godLabel);
        }

        private void ToggleNoclip() {
            if(!gameHook.hooked) return;

            noclipFlag = !noclipFlag;
            ToggleState(noclipFlag, noclipLabel);
        }

        private void SavePosition() {
            if(!gameHook.hooked) return;


        }

        private void TeleportPlayer() {
            if(!gameHook.hooked) return;


        }

        private void ChangeGameSpeed() {
            // increment curr speed
            currGameSpeed++;
            if(currGameSpeed >= gameSpeeds.Length) currGameSpeed = 0;
            // update ui
            SetGameSpeed();
        }

        private void SetGameSpeed() {
            gameSpeedLabel.Content = $"{gameSpeeds[currGameSpeed]:0.0}x";
        }

        #region UI
        //// UI ////
        private void ToggleState(bool state, System.Windows.Controls.Label label) {
            label.Content = state ? "ON" : "OFF";
            label.Foreground = state ? Brushes.Green : Brushes.Red;
        }

        //// UI BUTTONS ////
        private void ghostButton_Click(object sender, RoutedEventArgs e) => ToggleGhost();
        private void godButton_Click(object sender, RoutedEventArgs e) => ToggleGod();
        private void noclipButton_Click(object sender, RoutedEventArgs e) => ToggleNoclip();
        private void gameSpeedButton_Click(object sender, RoutedEventArgs e) => ChangeGameSpeed();
        private void SavePosButton_Click(object sender, RoutedEventArgs e) => SavePosition();
        private void TeleportButton_Click(object sender, RoutedEventArgs e) => TeleportPlayer();
        #endregion
    }
}
