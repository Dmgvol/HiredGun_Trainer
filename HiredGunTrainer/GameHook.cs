using System;
using System.Windows;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;

namespace HiredGunTrainer {
    public class GameHook {
		// CONST
		private const string PROCESS_NAME = "Necromunda-Win64-Shipping";

		// Hook
		public static Process game { get; private set; }
        public bool hooked = false;
        private MainWindow main;

        public GameHook(MainWindow main) {
            this.main = main;
        }

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
				case 78856192:
					


					//Debug.WriteLine("found steam6");
					//capsuleDP = new DeepPointer(0x0438BB50, 0x30, 0x130, 0x0);
					//mapNameDP = new DeepPointer(0x0438BB40, 0x30, 0xF8, 0x0);
					//preciseTimeDP = new DeepPointer(0x04609420, 0x138, 0xB0, 0x128);
					//hcDP = new DeepPointer(0x0438BB40, 0x330, 0x30);
					//LoadingDP = new DeepPointer(0x044C4478, 0x1E8);
					//reloadCounterDP = new DeepPointer(0x04609420, 0x128, 0x388);
					//break;

				default:
					main.updateTimer.Stop();
					Console.WriteLine(moduleSize.ToString());
					System.Windows.MessageBox.Show("This game version (" + moduleSize.ToString() + ") is not supported.\nPlease Contact the developers.", "Unsupported Game Version", MessageBoxButton.OK, MessageBoxImage.Error);
					Environment.Exit(0);
					break;
			}
		}

	}
}
