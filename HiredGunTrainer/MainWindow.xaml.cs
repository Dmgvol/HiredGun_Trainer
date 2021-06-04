﻿using HiredGunTrainer.MemoryUtils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace HiredGunTrainer {
    public partial class MainWindow : Window {

        // GLOBAL
        public const string VERSION = "0.6.1";

        // game speeds
        private float[] gameSpeeds = new float[4] { 1.0f, 2.0f, 4.0f, 0.5f };
        private int currGameSpeed = 0;
        // player stats and flags
        private float[] savedPos = new float[5]{0, 0, 0,  0, 0};
        private bool noclipFlag, ghostFlag, godFlag;
        private double playerSpeed = 0;
        private float[] playerPos = new float[3] { 0, 0, 0};

        // pointers
        private IntPtr xVelPtr, yVelPtr, zVelPtr;

        private GlobalKeyboardHook kbHook = new GlobalKeyboardHook();
        public Timer updateTimer;
        private GameHook gameHook;

        public MainWindow() {
            InitializeComponent();
            VersionLabel.Content = $"v{VERSION}";
            // game hook
            gameHook = new GameHook(this);

            // hotkeys
            kbHook.KeyDown += InputKeyDown;
            //kbHook.HookedKeys.Add(Keys.F1);
            //kbHook.HookedKeys.Add(Keys.F2);
            kbHook.HookedKeys.Add(Keys.F3);
            kbHook.HookedKeys.Add(Keys.F4);
            kbHook.HookedKeys.Add(Keys.F5);
            kbHook.HookedKeys.Add(Keys.F6);
            kbHook.HookedKeys.Add(Keys.F7);
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

            //// Read values /////
            // pos
            float x, y, z;
            GameHook.game.ReadValue(gameHook.EP.Pointers["PlayerPos"].Item2, out x);
            GameHook.game.ReadValue(gameHook.EP.Pointers["PlayerPos"].Item2 + 4, out y);
            GameHook.game.ReadValue(gameHook.EP.Pointers["PlayerPos"].Item2 + 8, out z);
            playerPos = new float[3] { x, y, z };
            // flying
            byte[] flyingByte = new byte[1] {0};
            GameHook.game.ReadBytes(gameHook.EP.Pointers["PlayerMovement"].Item2, 1, out flyingByte);
            if(flyingByte != null) noclipFlag = flyingByte[0] == 5;
            // player speed
            float xVel, yVel, zVel;
            GameHook.game.ReadValue(xVelPtr, out xVel);
            GameHook.game.ReadValue(yVelPtr, out yVel);
            GameHook.game.ReadValue(zVelPtr, out zVel);
            playerSpeed = Math.Floor(Math.Sqrt(xVel * xVel + yVel * yVel) + 0.5f) / 100;
            // game speed
            float speed;
            GameHook.game.ReadValue(gameHook.EP.Pointers["GameSpeed"].Item2 , out speed);
            // check if pre-defined speed
            for(int i = 0; i < gameSpeeds.Length; i++) {
                if(gameSpeeds[i] == speed)
                    currGameSpeed = i;
            }

            //// Update UI ////
            ToggleState(ghostFlag, ghostLabel);
            ToggleState(godFlag, godLabel);
            ToggleState(noclipFlag, noclipLabel);
            SetGameSpeed();
            positionBlock.Text = $"{(int)playerPos[0]}\n{(int)playerPos[1]}\n{(int)playerPos[2]}";
            speedLabel.Content = $"{playerSpeed:0.00} m/s";
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
                case Keys.F7:
                   teleport_Click(null, null);
                    break;
                default:
                    break;
            }
            e.Handled = false;
        }

        public void DerefPointers() {
            // EasyPointers
            gameHook.EP.DerefPointers(GameHook.game);

            // Velocity
            xVelPtr = gameHook.EP.Pointers["PlayerMoveComp"].Item2 + 0xC4;
            yVelPtr = gameHook.EP.Pointers["PlayerMoveComp"].Item2 + 0xC8;
            zVelPtr = gameHook.EP.Pointers["PlayerMoveComp"].Item2 + 0xCC;
        }

        private void ToggleGhost() {
            if(!gameHook.hooked) return;

            ghostFlag = !ghostFlag;
            ToggleState(ghostFlag, ghostLabel);

            // Placeholder
        }

        private void ToggleGod() {
            if(!gameHook.hooked) return;

            godFlag = !godFlag;
            ToggleState(godFlag, godLabel);

            // Placeholder
        }

        private void ToggleNoclip() {
            if(!gameHook.hooked) return;

            noclipFlag = !noclipFlag;
            ToggleState(noclipFlag, noclipLabel);

            GameHook.game.WriteBytes(gameHook.EP.Pointers["PlayerMovement"].Item2, noclipFlag ? new byte[] {05} : new byte[] {01});
            GameHook.game.WriteBytes(gameHook.EP.Pointers["FallMode"].Item2, noclipFlag ? new byte[] { 8 } : new byte[] { 40 });
            GameHook.game.WriteBytes(gameHook.EP.Pointers["PlayerCollision"].Item2, noclipFlag ? new byte[] { 40 } : new byte[] { 44 });
        }

        private void SavePosition() {
            if(!gameHook.hooked) return;

            float vlook, hlook;
            GameHook.game.ReadValue(gameHook.EP.Pointers["PlayerController"].Item2 + 0x288, out vlook);
            GameHook.game.ReadValue(gameHook.EP.Pointers["PlayerController"].Item2 + 0x28C, out hlook);
            savedPos = new float[5] { playerPos[0], playerPos[1], playerPos[2], vlook, hlook};
        }

        private void TeleportPlayer() {
            if(!gameHook.hooked) return;
            if(savedPos[0] == 0 && savedPos[1] == 0 && savedPos[2] == 0) return;

            // location
            GameHook.game.WriteValue(gameHook.EP.Pointers["PlayerPos"].Item2, savedPos[0]);
            GameHook.game.WriteValue(gameHook.EP.Pointers["PlayerPos"].Item2 + 4, savedPos[1]);
            GameHook.game.WriteValue(gameHook.EP.Pointers["PlayerPos"].Item2 + 8, savedPos[2]);
            // rotation
            GameHook.game.WriteValue(gameHook.EP.Pointers["PlayerController"].Item2 + 0x288, savedPos[3]);
            GameHook.game.WriteValue(gameHook.EP.Pointers["PlayerController"].Item2 + 0x28C, savedPos[4]);
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
            if(!gameHook.hooked) return;
            GameHook.game.WriteValue(gameHook.EP.Pointers["GameSpeed"].Item2, (float)gameSpeeds[currGameSpeed]);
        }

        #region UI
        //// UI ////
        private void ToggleState(bool state, System.Windows.Controls.Label label) {
            label.Content = state ? "ON" : "OFF";
            label.Foreground = state ? Brushes.Green : Brushes.Red;
        }

        //// UI BUTTONS ////
        private void ghostButton_Click(object sender, RoutedEventArgs e) => ToggleGhost();

        // Manual Teleport
        private void teleport_Click(object sender, RoutedEventArgs e) {
            if(!gameHook.hooked) return;

            if(IsNumeric(inputX.Text) && IsNumeric(inputY.Text) && IsNumeric(inputZ.Text)) {
                // teleport
                GameHook.game.WriteValue(gameHook.EP.Pointers["PlayerPos"].Item2, float.Parse(inputX.Text));
                GameHook.game.WriteValue(gameHook.EP.Pointers["PlayerPos"].Item2 + 4, float.Parse(inputY.Text));
                GameHook.game.WriteValue(gameHook.EP.Pointers["PlayerPos"].Item2 + 8, float.Parse(inputZ.Text));
            }
        }

        // ManualTP X coords text changed - parse if copy-paste
        private void inputX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            if(!string.IsNullOrEmpty(inputX.Text) && inputX.Text.Count(f => f == ' ') == 2) {
                string[] coords = inputX.Text.Split(' ');
                if(IsNumeric(coords[0]) && IsNumeric(coords[1]) && IsNumeric(coords[0])) {
                    inputX.Text = coords[0].Trim().Replace(",", "");
                    inputY.Text = coords[1].Trim().Replace(",", "");
                    inputZ.Text = coords[2].Trim().Replace(",", "");
                }
            }
        }

        public bool IsNumeric(string value) => float.TryParse(value, out _);
        private void godButton_Click(object sender, RoutedEventArgs e) => ToggleGod();
        private void Label_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) =>
            System.Windows.Clipboard.SetText($"{playerPos[0]}, {playerPos[1]}, {playerPos[2]}"); 

        private void noclipButton_Click(object sender, RoutedEventArgs e) => ToggleNoclip();
        private void gameSpeedButton_Click(object sender, RoutedEventArgs e) => ChangeGameSpeed();
        private void SavePosButton_Click(object sender, RoutedEventArgs e) => SavePosition();
        private void TeleportButton_Click(object sender, RoutedEventArgs e) => TeleportPlayer();
        #endregion
    }
}
