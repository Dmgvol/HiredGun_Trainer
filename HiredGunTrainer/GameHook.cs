using System;
using System.Windows;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using HiredGunTrainer.MemoryUtils;

namespace HiredGunTrainer {


    public class GameHook {
        // CONST
        private const string PROCESS_NAME = "Necromunda-Win64-Shipping";
        static public string gameversion = "";
        // Hook
        public static Process game { get; private set; }
        public bool hooked = false;
        private MainWindow main;

        public GameHook(MainWindow main) {
            this.main = main;

            if(Process.GetProcesses().ToList().Count(x => x.ProcessName.Contains(Process.GetCurrentProcess().ProcessName)) > 1) {
                MessageBox.Show("HiredGun Trainer is already running", "Already running!", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }

        public EasyPointers EP = new EasyPointers();

        private bool Hook() {
            List<Process> processList = Process.GetProcesses().ToList().FindAll(x => x.ProcessName.Contains(PROCESS_NAME));
            if(processList.Count == 0) {
                game = null;
                return false;
            }
            game = processList[0];
            if(game.HasExited) return false;

            try {
                int mainModuleSize = game.MainModule.ModuleMemorySize;
                SetPointersByModuleSize(mainModuleSize);
                return true;
            } catch(Exception ex) {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public void Update() {
            // Check if game is running/hooked
            if(game == null || game.HasExited) {
                game = null;
                hooked = false;

                main.errorGrid.Visibility = Visibility.Visible;
                main.errorMsg.Content = "HiredGun not found";

            } else {
                main.errorGrid.Visibility = Visibility.Hidden;
                main.errorMsg.Content = "";
            }
            if(!hooked)
                hooked = Hook();

            if(!hooked)
                return;
            try {
                main.DerefPointers();
            } catch(Exception) {
                return;
            }
        }

        private void SetPointersByModuleSize(int moduleSize) {
            switch(moduleSize) {
                case 95641600:
                case 95408128: // steam1/gog1 - Release Version
                    Debug.WriteLine("found steam1/gog1");
                    EP.Add("PlayerPos", new DeepPointer(0x05534880, 0x30, 0x250, 0x130, 0x1d0));
                    EP.Add("PlayerController", new DeepPointer(0x05534880, 0x30, 0x250, 0xE0, 0x0));
                    EP.Add("PlayerObject", new DeepPointer(0x05534880, 0x30, 0x250, 0x0));
                    EP.Add("PlayerCollision", new DeepPointer(0x05534880, 0x30, 0x250, 0x5c));
                    EP.Add("PlayerMovement", new DeepPointer(0x05534880, 0x30, 0x250, 0x288, 0x168));
                    EP.Add("FallMode", new DeepPointer(0x05534880, 0x30, 0x250, 0x288, 0x38C));
                    EP.Add("PlayerMoveComp", new DeepPointer(0x05534880, 0x30, 0x250, 0x288, 0x0));
                    EP.Add("GameSpeed", new DeepPointer(0x0554BBA0, 0x30, 0x258, 0x2e8));
                    EP.Add("WeaponDamage", new DeepPointer(0x05534880, 0x30, 0x250, 0x8d0, 0x410, 0x3c));
                    EP.Add("WeaponDamageBase", new DeepPointer(0x05534880, 0x30, 0x250, 0x8d0, 0x410, 0x38));
                    EP.Add("MapBeginTime", new DeepPointer(0x05534880, 0x30, 0x228, 0x348));
                    break;
                case 96124928:
                case 95891456:// steam2 - Day 4 Update & late GOG update
                    Debug.WriteLine("found steam2/gog2");
                    EP.Add("PlayerPos", new DeepPointer(0x055A3A00, 0x30, 0x250, 0x130, 0x1d0));
                    EP.Add("PlayerController", new DeepPointer(0x055A3A00, 0x30, 0x250, 0xE0, 0x0));
                    EP.Add("PlayerObject", new DeepPointer(0x055A3A00, 0x30, 0x250, 0x0));
                    EP.Add("PlayerCollision", new DeepPointer(0x055A3A00, 0x30, 0x250, 0x5c));
                    EP.Add("PlayerMovement", new DeepPointer(0x055A3A00, 0x30, 0x250, 0x288, 0x168));
                    EP.Add("FallMode", new DeepPointer(0x055A3A00, 0x30, 0x250, 0x288, 0x38C));
                    EP.Add("PlayerMoveComp", new DeepPointer(0x055A3A00, 0x30, 0x250, 0x288, 0x0));
                    EP.Add("GameSpeed", new DeepPointer(0x055BAD20, 0x30, 0x258, 0x2E8));
                    EP.Add("WeaponDamage", new DeepPointer(0x055A3A00, 0x30, 0x250, 0x8d0, 0x410, 0x3c));
                    EP.Add("WeaponDamageBase", new DeepPointer(0x055A3A00, 0x30, 0x250, 0x8d0, 0x410, 0x38));
                    EP.Add("MapBeginTime", new DeepPointer(0x055A3A00, 0x30, 0x228, 0x348));
                    break;
                case 95600640: // steam 3 - July 2021
                    Debug.WriteLine("found steam3");
                    EP.Add("PlayerPos", new DeepPointer(0x05527E90, 0x30, 0x250, 0x130, 0x1d0));
                    EP.Add("PlayerController", new DeepPointer(0x05527E90, 0x30, 0x250, 0xE0, 0x0));
                    EP.Add("PlayerObject", new DeepPointer(0x05527E90, 0x30, 0x250, 0x0));
                    EP.Add("PlayerCollision", new DeepPointer(0x05527E90, 0x30, 0x250, 0x5c));
                    EP.Add("PlayerMovement", new DeepPointer(0x05527E90, 0x30, 0x250, 0x288, 0x168));
                    EP.Add("FallMode", new DeepPointer(0x05527E90, 0x30, 0x250, 0x288, 0x38C));
                    EP.Add("PlayerMoveComp", new DeepPointer(0x05527E90, 0x30, 0x250, 0x288, 0x0));
                    EP.Add("GameSpeed", new DeepPointer(0x0553F1B0, 0x30, 0x258, 0x2E8));
                    EP.Add("WeaponDamage", new DeepPointer(0x05527E90, 0x30, 0x250, 0x8d0, 0x410, 0x3c));
                    EP.Add("WeaponDamageBase", new DeepPointer(0x05527E90, 0x30, 0x250, 0x8d0, 0x410, 0x38));
                    EP.Add("MapBeginTime", new DeepPointer(0x05527E90, 0x30, 0x228, 0x348));
                    break;
                case 95670272: // steam 4 - July 2021 (second update of July)
                    Debug.WriteLine("found steam4");
                    EP.Add("PlayerPos", new DeepPointer(0x05537B60, 0x30, 0x250, 0x130, 0x1d0));
                    EP.Add("PlayerController", new DeepPointer(0x05537B60, 0x30, 0x250, 0xE0, 0x0));
                    EP.Add("PlayerObject", new DeepPointer(0x05537B60, 0x30, 0x250, 0x0));
                    EP.Add("PlayerCollision", new DeepPointer(0x05537B60, 0x30, 0x250, 0x5c));
                    EP.Add("PlayerMovement", new DeepPointer(0x05537B60, 0x30, 0x250, 0x288, 0x168));
                    EP.Add("FallMode", new DeepPointer(0x05537B60, 0x30, 0x250, 0x288, 0x38C));
                    EP.Add("PlayerMoveComp", new DeepPointer(0x05537B60, 0x30, 0x250, 0x288, 0x0));
                    EP.Add("GameSpeed", new DeepPointer(0x0554EEB0, 0x30, 0x258, 0x2E8));
                    EP.Add("WeaponDamage", new DeepPointer(0x05537B60, 0x30, 0x250, 0x8d0, 0x410, 0x3c));
                    EP.Add("WeaponDamageBase", new DeepPointer(0x05537B60, 0x30, 0x250, 0x8d0, 0x410, 0x38));
                    EP.Add("MapBeginTime", new DeepPointer(0x05537B60, 0x30, 0x228, 0x348));
                    break;
                case 96051200: // steam 5 - July 2021 (third update of July)
                    Debug.WriteLine("found steam5");
                    EP.Add("PlayerPos", new DeepPointer(0x0558E120, 0x30, 0x250, 0x130, 0x1d0));
                    EP.Add("PlayerController", new DeepPointer(0x0558E120, 0x30, 0x250, 0xE0, 0x0));
                    EP.Add("PlayerObject", new DeepPointer(0x0558E120, 0x30, 0x250, 0x0));
                    EP.Add("PlayerCollision", new DeepPointer(0x0558E120, 0x30, 0x250, 0x5c));
                    EP.Add("PlayerMovement", new DeepPointer(0x0558E120, 0x30, 0x250, 0x288, 0x168));
                    EP.Add("FallMode", new DeepPointer(0x0558E120, 0x30, 0x250, 0x288, 0x38C));
                    EP.Add("PlayerMoveComp", new DeepPointer(0x0558E120, 0x30, 0x250, 0x288, 0x0));
                    EP.Add("GameSpeed", new DeepPointer(0x055A5470, 0x30, 0x258, 0x2E8));
                    EP.Add("WeaponDamage", new DeepPointer(0x0558E120, 0x30, 0x250, 0x8d0, 0x410, 0x3c));
                    EP.Add("WeaponDamageBase", new DeepPointer(0x0558E120, 0x30, 0x250, 0x8d0, 0x410, 0x38));
                    EP.Add("MapBeginTime", new DeepPointer(0x0558E120, 0x30, 0x228, 0x348));
                    break;
                case 96591872:
                case 96354304: // steam 6 & GOG 6 - September 2021 [Update by LongerWarrior]
                    Debug.WriteLine("found steam6");
                    gameversion = "steam6";
                    EP.Add("PlayerPos", new DeepPointer(0x0560B590, 0x30, 0x250, 0x130, 0x1d0));
                    EP.Add("PlayerController", new DeepPointer(0x0560B590, 0x30, 0x250, 0xE0, 0x0));
                    EP.Add("PlayerObject", new DeepPointer(0x0560B590, 0x30, 0x250, 0x0));
                    EP.Add("PlayerCollision", new DeepPointer(0x0560B590, 0x30, 0x250, 0x5c));
                    EP.Add("PlayerMovement", new DeepPointer(0x0560B590, 0x30, 0x250, 0x288, 0x168));
                    EP.Add("FallMode", new DeepPointer(0x0560B590, 0x30, 0x250, 0x288, 0x38C));
                    EP.Add("PlayerMoveComp", new DeepPointer(0x0560B590, 0x30, 0x250, 0x288, 0x0));
                    EP.Add("GameSpeed", new DeepPointer(0x056228D0, 0x30, 0x258, 0x2E8));
                    EP.Add("WeaponDamage", new DeepPointer(0x0560B590, 0x30, 0x250, 0x910, 0x410, 0x3c));
                    EP.Add("WeaponDamageBase", new DeepPointer(0x0560B590, 0x30, 0x250, 0x910, 0x410, 0x38));
                    EP.Add("MapBeginTime", new DeepPointer(0x0560B590, 0x30, 0x228, 0x348));
                    break;
                default:
                    main.updateTimer.Stop();
                    Console.WriteLine(moduleSize.ToString());
                    MessageBox.Show("This game version (" + moduleSize.ToString() + ") is not supported.\nPlease contact the developers.", "Unsupported Game Version", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                    break;
            }
        }
    }
}
