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

        private float[] savedPos = new float[5]{0, 0, 0,  0, 0};
        private bool noclipFlag, ghostFlag, godFlag;
        private float playerSpeed = 0;
        private float[] playerPos = new float[3] { 0, 0, 0};

        // pointers
        private IntPtr vLookPtr, hLookPtr;
        IntPtr xVelPtr, yVelPtr, zVelPtr, ghostPtr, godPtr, noclipPtr, movePtr, clipmovePtr, airPtr, collisionPtr, sumPtr, injPtr, gameSpeedPtr, mapNamePtr, angleSinPtr, angleCosPtr;


        private GlobalKeyboardHook kbHook = new GlobalKeyboardHook();
        public Timer updateTimer;
        private GameHook gameHook;

        public MainWindow() {
            InitializeComponent();
            // game hook
            gameHook = new GameHook(this);

            // hotkeys
            kbHook.KeyDown += InputKeyDown;
            //kbHook.HookedKeys.Add(Keys.F1);
            //kbHook.HookedKeys.Add(Keys.F2);
            //kbHook.HookedKeys.Add(Keys.F3);
            //kbHook.HookedKeys.Add(Keys.F4);
            kbHook.HookedKeys.Add(Keys.F5);
            kbHook.HookedKeys.Add(Keys.F6);
            // update timer
            updateTimer = new Timer {
                Interval = (16) // ~60 Hz
            };
            updateTimer.Tick += new EventHandler(Update);
            updateTimer.Start();
        }

        DateTime? timeStamp = null;
        private Vector3f posTmp;

        private void Update(object sender, EventArgs e) {
            // GAME HOOK
            gameHook.Update();
            if(!gameHook.hooked) return;

            // Read values
            float x, y, z;
            GameHook.game.ReadValue(gameHook.EP.Pointers["PlayerPos"].Item2, out x);
            GameHook.game.ReadValue(gameHook.EP.Pointers["PlayerPos"].Item2 + 4, out y);
            GameHook.game.ReadValue(gameHook.EP.Pointers["PlayerPos"].Item2 + 8, out z);
            playerPos = new float[3] { x, y, z };

            // Update UI
            ToggleState(ghostFlag, ghostLabel);
            ToggleState(godFlag, godLabel);
            ToggleState(noclipFlag, noclipLabel);
            SetGameSpeed();
            positionBlock.Text = $"{playerPos[0] / 100:0.00}\n{playerPos[1] / 100:0.00}\n{playerPos[2] / 100:0.00}";

            // player speed
            if(timeStamp == null) {
                timeStamp = DateTime.Now;
                posTmp = new Vector3f(x, y, z);
            } else {
                TimeSpan span = DateTime.Now - (DateTime)timeStamp;
                if(span.TotalMilliseconds >= 300) {
                    double spanRatio = 1000 / span.TotalMilliseconds;
                    double distance = Math.Sqrt(
                        Math.Pow(x - posTmp.X, 2) +
                        Math.Pow(y - posTmp.Y, 2) +
                        Math.Pow(z - posTmp.Z, 2));
                    playerSpeed = (float)(distance * spanRatio) / 100;

                timeStamp = DateTime.Now;
                posTmp = new Vector3f(x, y, z);
                }
            }


            speedLabel.Content = $"{(int)playerSpeed} m/s";
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
            gameHook.EP.DerefPointers(GameHook.game);

            xVelPtr = gameHook.EP.Pointers["PlayerCollision"].Item2 + 0xC4;
            yVelPtr = gameHook.EP.Pointers["PlayerCollision"].Item2 + 0xC8;
            zVelPtr = gameHook.EP.Pointers["PlayerCollision"].Item2 + 0xCC;
            movePtr = gameHook.EP.Pointers["PlayerCollision"].Item2 + 0x168;
            airPtr = gameHook.EP.Pointers["PlayerCollision"].Item2 + 0x388;
            clipmovePtr = gameHook.EP.Pointers["PlayerCollision"].Item2 + 0x199;
            collisionPtr = gameHook.EP.Pointers["PlayerObject"].Item2 + 0x5C;

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

            //IncInj();
            //byte[] byteToWrite = new byte[1];
            //if(noclipFlag) {
            //    byteToWrite[0] = 0x0;
            //    GameHook.game.WriteBytes(collisionPtr, new byte[1] { 0x44 });
            //    GameHook.game.WriteBytes(movePtr, new byte[1] { 0x01 });
            //    GameHook.game.WriteBytes(airPtr, new byte[1] { 0x60 });
            //} else {
            //    byteToWrite[0] = 0x1;
            //    GameHook.game.WriteBytes(movePtr, new byte[1] { 0x05 });
            //    GameHook.game.WriteBytes(airPtr, new byte[1] { 0x48 });
            //    GameHook.game.WriteBytes(collisionPtr, new byte[1] { 0x40 });
            //    GameHook.game.WriteBytes(clipmovePtr, BitConverter.GetBytes(0x00458CA0));
            //}
            //GameHook.game.WriteBytes(noclipPtr, byteToWrite);
        }

        private void IncInj() {
            int current;
            GameHook.game.ReadValue<int>(injPtr, out current);
            GameHook.game.WriteBytes(injPtr, BitConverter.GetBytes(current + 1));
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
