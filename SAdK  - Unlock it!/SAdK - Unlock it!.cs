using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Windows.Forms;

namespace SAdK____Unlock_it_
{
    public partial class SAdKUnlockIt : Form
    {
        // The path of the currently selected file
        public string selectedFilePath;
        public string filePath;

        public SAdKUnlockIt()
        {
            InitializeComponent();

            // Disable certain UI elements initially
            PagesWithCategories.Enabled = false;

            // Attach a file deletion event handler to the form closing event
            this.FormClosing += FileDelete;
        }

        //File selection
        public void FilePath_Click(object sender, EventArgs e)
        {
            // Open a file dialog for the user to select a .s2m file
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select a map that you want to edit";
                openFileDialog.Filter = "Settlers 2 remake map (*.s2m)|*.s2m";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Set the selected file path to the text box
                    selectedFilePath = openFileDialog.FileName;
                    FilePath.Text = selectedFilePath;
                }
            }
        }

        public void LoadFile_Click(object sender, EventArgs e)
        {
            // Fix a bug when you are able to edit text at the start
            if (Path.IsPathRooted(FilePath.Text)/*FilePath.Text != "Enter file path"*/)
            {
                // Copy the selected file to the application folder
                string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                string destinationPath = Path.Combine(executablePath, Path.GetFileName(selectedFilePath));
                FilePath.Text = destinationPath;
                try
                {
                    File.Copy(selectedFilePath, destinationPath, true);

                    // Decode the file using an external process
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(executablePath, "AdKEd.exe"),
                        Arguments = $"\"{destinationPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    // Start the process
                    using (Process process = new Process { StartInfo = startInfo })
                    {
                        process.Start();
                        process.WaitForExit();
                    }

                    // Display success or error messages to the user
                    MessageBox.Show("File loaded successfully.");

                    // Enable/disable UI elements accordingly
                    LoadFile.Enabled = false;
                    FilePath.Enabled = false;

                    PagesWithCategories.Enabled = true;
                }
                catch (Exception ex)
                {
                    // Display success or error messages to the user
                    MessageBox.Show($"Error loading file: {selectedFilePath}. {ex.Message}");
                }
            }
            else
            {
                // Handle the case where no file has been selected
                MessageBox.Show("Please select a file first.", "File Not Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void FileDelete(object sender, FormClosingEventArgs e)
        {
            // Delete the file if it exists and the application is closing
            if (File.Exists(FilePath.Text) && !LoadFile.Enabled)
            {
                try
                {
                    File.Delete(FilePath.Text);
                }
                catch (Exception ex)
                {
                    // Handle an error when deleting
                    MessageBox.Show($"File deleting error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Event handler for the "Save" button
        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Get the destination and executable paths
            string destinationPath = FilePath.Text;
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;

            // Encode the file using an external process
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(executablePath, "AdKEd.exe"),
                // Move the modified file back to its original location
                Arguments = $"\"{destinationPath}\"",

                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            // Start the process
            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                process.WaitForExit();
            }

            // Display error messages if any issues occur
            try
            {
                File.Copy(FilePath.Text, selectedFilePath, true); // True means that original file will be overrited
                File.Delete(FilePath.Text);
                // Exit the application
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"File operation error: {ex.Message}. Source file: {FilePath.Text}, destination file: {selectedFilePath}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Discord
        private void Discord_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "https://discord.gg/UAXH3VS9Qy";
            Process.Start(url);
        }

        //ModDB
        private void MapSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "https://www.moddb.com/games/the-settlers-rise-of-cultures/addons";
            Process.Start(url);
        }

        //GitHub
        private void GitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            string url = "https://github.com/S2-modders";
            Process.Start(url);
        }

        //Main element of the converter
        static void ReplaceAllHexInFile(string destinationPath, string hexToReplace, string hexReplacement)
        {
            try
            {
                // Read file content to byte array
                byte[] fileBytes = File.ReadAllBytes(destinationPath);

                // Convert hex value to byte array
                byte[] hexToReplaceBytes = StringToByteArray(hexToReplace);
                byte[] hexReplacementBytes = StringToByteArray(hexReplacement);

                // Replace all occurrences of a value in a byte array
                for (int i = 0; i <= fileBytes.Length - hexToReplaceBytes.Length; i++)
                {
                    bool match = true;
                    for (int j = 0; j < hexToReplaceBytes.Length; j++)
                    {
                        if (fileBytes[i + j] != hexToReplaceBytes[j])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        Array.Copy(hexReplacementBytes, 0, fileBytes, i, hexReplacementBytes.Length);
                        i += hexReplacementBytes.Length - 1;
                    }
                }

                // Save changed bytes to the file
                File.WriteAllBytes(destinationPath, fileBytes);

                MessageBox.Show("Bytes got replaced succesfull");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occured: {ex.Message}");
            }
        }

        static byte[] StringToByteArray(string hex)
        {
            int length = hex.Length / 2;
            byte[] byteArray = new byte[length];
            for (int i = 0; i < length; i++)
            {
                byteArray[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return byteArray;
        }

        //Plains conversion
        private void PlainsButton_Click(object sender, EventArgs e)
        {
            if (Plains10.SelectedIndex != -1 && PlainsAdK.SelectedIndex != -1)
            {
                // If something is selected then index can be read
                int Plains10Index = Plains10.SelectedIndex;
                int PlainsAdKIndex = PlainsAdK.SelectedIndex;
                string destinationPath = Path.GetFileName(FilePath.Text);
                string hexToReplace = "";
                string hexReplacement = "";

                //10th Anniversary

                //DO NOT USE (pattern_acre)
                if (Plains10Index == 0)
                {
                    hexToReplace = "1a7056ca";
                }
                //meadow (pattern_meadow)
                if (Plains10Index == 1)
                {
                    hexToReplace = "e3e8e4bf";
                }
                //meadow yellow flowers (pattern_meadow1)
                if (Plains10Index == 2)
                {
                    hexToReplace = "c1fa4545";
                }
                //earth (pattern_meadow2)
                if (Plains10Index == 3)
                {
                    hexToReplace = "c2fa4545";
                }
                //meadow bright (pattern_meadow3)
                if (Plains10Index == 4)
                {
                    hexToReplace = "c3fa4545";
                }
                //leaf (pattern_meadow4)
                if (Plains10Index == 5)
                {
                    hexToReplace = "c4fa4545";
                }
                //meadow leaf (pattern_meadow5)
                if (Plains10Index == 6)
                {
                    hexToReplace = "c5fa4545";
                }
                //meadow dark small (pattern_meadow6)
                if (Plains10Index == 7)
                {
                    hexToReplace = "c6fa4545";
                }
                //meadow red flowers (pattern_meadow7)
                if (Plains10Index == 8)
                {
                    hexToReplace = "c7fa4545";
                }
                //§§Desert meadow (pattern_meadow9)
                if (Plains10Index == 9)
                {
                    hexToReplace = "c9fa4545";
                }
                //sand stones (pattern_sand1)
                if (Plains10Index == 10)
                {
                    hexToReplace = "0eb0deba";
                }
                //§§Desert earth (pattern_earth1)
                if (Plains10Index == 11)
                {
                    hexToReplace = "c0a87f77";
                }
                //stone ground (pattern_rock9)
                if (Plains10Index == 12)
                {
                    hexToReplace = "05cbfeca";
                }
                //swamp meadow (unblocked) (pattern_swamp2)
                if (Plains10Index == 13)
                {
                    hexToReplace = "e6040068";
                }
                //HARBOR (pattern_pavement)
                if (Plains10Index == 14)
                {
                    hexToReplace = "01decade";
                }
                //meadow ground (pattern_ground)
                if (Plains10Index == 15)
                {
                    hexToReplace = "10115ede";
                }
                //((00 LAVA ground (pattern_l_ground_00)
                if (Plains10Index == 16)
                {
                    hexToReplace = "02decade";
                }
                //((00 LAVA ground rough (pattern_l_ground_02)
                if (Plains10Index == 17)
                {
                    hexToReplace = "07decade";
                }
                //((00 LAVA ground flat (pattern_l_ground_04)
                if (Plains10Index == 18)
                {
                    hexToReplace = "09decade";
                }
                //((00 LAVA Meadow 00 (pattern_l_meadow_00)
                if (Plains10Index == 19)
                {
                    hexToReplace = "70db7af6";
                }
                //!!!MED meadow 00 (pattern_m_meadow_00)
                if (Plains10Index == 20)
                {
                    hexToReplace = "60a51cfa";
                }
                //!!!MED meadow 01 (pattern_m_meadow_01)
                if (Plains10Index == 21)
                {
                    hexToReplace = "61a51cfa";
                }
                //!!!MED meadow 02 (pattern_m_meadow_02)
                if (Plains10Index == 22)
                {
                    hexToReplace = "62a51cfa";
                }
                //!!!MED meadow 03 (pattern_m_meadow_03)
                if (Plains10Index == 23)
                {
                    hexToReplace = "63a51cfa";
                }
                //!!!MED ground 00 (pattern_m_ground_00)
                if (Plains10Index == 24)
                {
                    hexToReplace = "70a51cfa";
                }
                //!!!MED ground 01 (pattern_m_ground_01)
                if (Plains10Index == 25)
                {
                    hexToReplace = "71a51cfa";
                }
                //!!!MED stone ground (pattern_m_rock_10)
                if (Plains10Index == 26)
                {
                    hexToReplace = "8aa51cfa";
                }

                //Aufbruch der Kulturen

                //__Highland meadow bright (pattern_h_meadow_00)
                if (PlainsAdKIndex == 0)
                {
                    hexReplacement = "024ac47a";
                }
                //__Highland meadow bright rocks (pattern_h_meadow_01)
                if (PlainsAdKIndex == 1)
                {
                    hexReplacement = "034ac47a";
                }
                //__Highland meadow medium (pattern_h_meadow_02)
                if (PlainsAdKIndex == 2)
                {
                    hexReplacement = "044ac47a";
                }
                //__Highland meadow medium rocks (pattern_h_meadow_03)
                if (PlainsAdKIndex == 3)
                {
                    hexReplacement = "054ac47a";
                }
                //__Highland meadow dark (pattern_h_meadow_04)
                if (PlainsAdKIndex == 4)
                {
                    hexReplacement = "064ac47a";
                }
                //__Highland meadow dark rocks (pattern_h_meadow_05)
                if (PlainsAdKIndex == 5)
                {
                    hexReplacement = "074ac47a";
                }
                //__Highland earth fir moss (pattern_h_earth_00)
                if (PlainsAdKIndex == 6)
                {
                    hexReplacement = "004dc47a";
                }
                //__Highland earth fir (pattern_h_earth_01)
                if (PlainsAdKIndex == 7)
                {
                    hexReplacement = "014dc47a";
                }
                //__Highland earth (pattern_h_earth_02)
                if (PlainsAdKIndex == 8)
                {
                    hexReplacement = "024dc47a";
                }
                //__Highland stone ground (pattern_h_rock_07)
                if (PlainsAdKIndex == 9)
                {
                    hexReplacement = "0d4bc47a";
                }
                //--Snow meadow (pattern_s_meadow_00)
                if (PlainsAdKIndex == 10)
                {
                    hexReplacement = "0b4ec47a";
                }
                //--Snow meadow snow (pattern_s_meadow_01)
                if (PlainsAdKIndex == 11)
                {
                    hexReplacement = "0c4ec47a";
                }
                //--Snow meadow snow 2 (pattern_s_meadow_02)
                if (PlainsAdKIndex == 12)
                {
                    hexReplacement = "0d4ec47a";
                }
                //--Snow meadow snow 3 (pattern_s_meadow_03)
                if (PlainsAdKIndex == 13)
                {
                    hexReplacement = "0e4ec47a";
                }
                //--Snow meadow Treeground 80x80,200x200 (pattern_s_meadow_04)
                if (PlainsAdKIndex == 14)
                {
                    hexReplacement = "0f4ec47a";
                }
                //--Snow meadow Treeground 125x125 (pattern_s_meadow_04a)
                if (PlainsAdKIndex == 15)
                {
                    hexReplacement = "104ec47a";
                }
                //--Snow meadow Treeground 170x170 (pattern_s_meadow_04b)
                if (PlainsAdKIndex == 16)
                {
                    hexReplacement = "114ec47a";
                }
                //--Snow meadow Treeground 255x255 (pattern_s_meadow_04c)
                if (PlainsAdKIndex == 17)
                {
                    hexReplacement = "124ec47a";
                }
                //__Highland swamp meadow (unblocked) (pattern_h_swamp_02)
                if (PlainsAdKIndex == 18)
                {
                    hexReplacement = "124cc47a";
                }

                //MessageBox.Show(hexToReplace);
                //MessageBox.Show(hexReplacement);

                ReplaceAllHexInFile(destinationPath, hexToReplace, hexReplacement);
            }
            else
            {
                // Something is not selected
                MessageBox.Show("Please select something.", "Something is not selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //Rocks conversion
        private void RocksButton_Click(object sender, EventArgs e)
        {
            if (Rocks10.SelectedIndex != -1 && RocksAdK.SelectedIndex != -1)
            {
                // If something is selected then index can be read
                int Rocks10Index = Rocks10.SelectedIndex;
                int RocksAdKIndex = RocksAdK.SelectedIndex;
                string destinationPath = Path.GetFileName(FilePath.Text);
                string hexToReplace = "";
                string hexReplacement = "";

                //10th Anniversary

                //rock (pattern_rock)
                if (Rocks10Index == 0)
                {
                    hexToReplace = "feaf0fd0";
                }
                //rock big (pattern_rock1)
                if (Rocks10Index == 1)
                {
                    hexToReplace = "efbeadde";
                }
                //rock small (pattern_rock2)
                if (Rocks10Index == 2)
                {
                    hexToReplace = "fecafeca";
                }
                //(RES) rocky earth (pattern_rock3)
                if (Rocks10Index == 3)
                {
                    hexToReplace = "ffcafeca";
                }
                //rock stretched x (pattern_rock4)
                if (Rocks10Index == 4)
                {
                    hexToReplace = "00cbfeca";
                }
                //rock stretched y (pattern_rock5)
                if (Rocks10Index == 5)
                {
                    hexToReplace = "01cbfeca";
                }
                //(RES) rocky earth big (pattern_rock6)
                if (Rocks10Index == 6)
                {
                    hexToReplace = "02cbfeca";
                }
                //(RES) rocky plants (pattern_rock7)
                if (Rocks10Index == 7)
                {
                    hexToReplace = "03cbfeca";
                }
                //(RES) rocky earth dark (pattern_rock8)
                if (Rocks10Index == 8)
                {
                    hexToReplace = "04cbfeca";
                }
                //((00 LAVA rock (pattern_l_rock_00)
                if (Rocks10Index == 9)
                {
                    hexToReplace = "04decade";
                }
                //((00 LAVA rock big (pattern_l_rock_01)
                if (Rocks10Index == 10)
                {
                    hexToReplace = "05decade";
                }
                //((00 LAVA rock small (pattern_l_rock_02)
                if (Rocks10Index == 11)
                {
                    hexToReplace = "06decade";
                }
                //((00 LAVA rock floating lava (pattern_l_rock_02)
                if (Rocks10Index == 12)
                {
                    hexToReplace = "b0fa87ca";
                }
                //!!!MED rock (pattern_m_rock_00)
                if (Rocks10Index == 13)
                {
                    hexToReplace = "80a51cfa";
                }
                //!!!MED rock big (pattern_m_rock_01)
                if (Rocks10Index == 14)
                {
                    hexToReplace = "81a51cfa";
                }
                //!!!MED rock small (pattern_m_rock_02)
                if (Rocks10Index == 15)
                {
                    hexToReplace = "82a51cfa";
                }
                //!!!MED rock red (pattern_m_rock_03)
                if (Rocks10Index == 16)
                {
                    hexToReplace = "83a51cfa";
                }
                //!!!MED rock red small (pattern_m_rock_04)
                if (Rocks10Index == 17)
                {
                    hexToReplace = "84a51cfa";
                }
                //!!!MED rock red big (pattern_m_rock_05)
                if (Rocks10Index == 18)
                {
                    hexToReplace = "85a51cfa";
                }
                //!!!MED (RES) rocky earth big (pattern_m_rock_06)
                if (Rocks10Index == 19)
                {
                    hexToReplace = "86a51cfa";
                }
                //!!!MED (RES) rocky plants (pattern_m_rock_07)
                if (Rocks10Index == 20)
                {
                    hexToReplace = "87a51cfa";
                }
                //!!!MED (RES) rocky earth dark (pattern_m_rock_08)
                if (Rocks10Index == 21)
                {
                    hexToReplace = "88a51cfa";
                }
                //!!!MED (RES) rocky earth (pattern_m_rock_09)
                if (Rocks10Index == 22)
                {
                    hexToReplace = "89a51cfa";
                }

                //Aufbruch der Kulturen

                //__Highland rock (pattern_h_rock_00)
                if (RocksAdKIndex == 0)
                {
                    hexReplacement = "024bc47a";
                }
                //__Highland rock big (pattern_h_rock_01)
                if (RocksAdKIndex == 1)
                {
                    hexReplacement = "034bc47a";
                }
                //__Highland (RES) rocky earth (pattern_h_rock_02)
                if (RocksAdKIndex == 2)
                {
                    hexReplacement = "044bc47a";
                }
                //__Highland rock flat (pattern_h_rock_03)
                if (RocksAdKIndex == 3)
                {
                    hexReplacement = "054bc47a";
                }
                //__Highland rock dark big (pattern_h_rock_04)
                if (RocksAdKIndex == 4)
                {
                    hexReplacement = "064bc47a";
                }
                //__Highland rock dark flat (pattern_h_rock_05)
                if (RocksAdKIndex == 5)
                {
                    hexReplacement = "074bc47a";
                }
                //__Highland rock braid flat (pattern_h_rock_06)
                if (RocksAdKIndex == 6)
                {
                    hexReplacement = "084bc47a";
                }
                //--Snow highland rock much (pattern_s_rock_00)
                if (RocksAdKIndex == 7)
                {
                    hexReplacement = "094bc47a";
                }
                //--Snow highland rock (pattern_s_rock_01)
                if (RocksAdKIndex == 8)
                {
                    hexReplacement = "0a4bc47a";
                }
                //--Snow highland rock part (pattern_s_rock_02)
                if (RocksAdKIndex == 9)
                {
                    hexReplacement = "0b4bc47a";
                }
                //--Snow (RES) rocky earth (pattern_s_rock_03)
                if (RocksAdKIndex == 10)
                {
                    hexReplacement = "0c4bc47a";
                }


                //MessageBox.Show(hexToReplace);
                //MessageBox.Show(hexReplacement);

                ReplaceAllHexInFile(destinationPath, hexToReplace, hexReplacement);
            }
            else
            {
                // Something is not selected
                MessageBox.Show("Please select something.", "Something is not selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SandsButton_Click(object sender, EventArgs e)
        {
            if (Sands10.SelectedIndex != -1 && SandsAdK.SelectedIndex != -1)
            {
                // If something is selected then index can be read
                int Sands10Index = Sands10.SelectedIndex;
                int SandsAdKIndex = SandsAdK.SelectedIndex;
                string destinationPath = Path.GetFileName(FilePath.Text);
                string hexToReplace = "";
                string hexReplacement = "";

                //10th Anniversary

                //sand (pattern_sand)
                if (Sands10Index == 0)
                {
                    hexToReplace = "0db0deba";
                }
                //§§Desert sand dune (pattern_sand2)
                if (Sands10Index == 1)
                {
                    hexToReplace = "0fb0deba";
                }
                //§§Desert sand yellow (pattern_sand3)
                if (Sands10Index == 2)
                {
                    hexToReplace = "10b0deba";
                }
                //§§Desert sand small dune (pattern_sand4)
                if (Sands10Index == 3)
                {
                    hexToReplace = "11b0deba";
                }
                //§§Desert sand ripple (pattern_sand5)
                if (Sands10Index == 4)
                {
                    hexToReplace = "12b0deba";
                }
                //§§Desert sand small ripple (pattern_sand6)
                if (Sands10Index == 5)
                {
                    hexToReplace = "13b0deba";
                }
                //seaground (pattern_seaground)
                if (Sands10Index == 6)
                {
                    hexToReplace = "0bb0beba";
                }
                //seaground plants (pattern_seaground1)
                if (Sands10Index == 7)
                {
                    hexToReplace = "e4743301";
                }
                //seaground sand (pattern_seaground2)
                if (Sands10Index == 8)
                {
                    hexToReplace = "e5743301";
                }
                //seaground plants rock (pattern_seaground3)
                if (Sands10Index == 9)
                {
                    hexToReplace = "e6743301";
                }
                //seaground rock (pattern_seaground4)
                if (Sands10Index == 10)
                {
                    hexToReplace = "e7743301";
                }
                //seaground rocky (pattern_seaground5)
                if (Sands10Index == 11)
                {
                    hexToReplace = "e8743301";
                }
                //((00 LAVA Sand 00 (pattern_l_sand_00)
                if (Sands10Index == 12)
                {
                    hexToReplace = "70bbcaf1";
                }
                //!!!MED seaground rock (pattern_m_seaground_00)
                if (Sands10Index == 13)
                {
                    hexToReplace = "90a51cfa";
                }
                //!!!MED seaground rock red (pattern_m_seaground_01)
                if (Sands10Index == 14)
                {
                    hexToReplace = "91a51cfa";
                }

                //Aufbruch der Kulturen

                //__Highland seaground rocks (pattern_h_seaground_00)
                if (SandsAdKIndex == 0)
                {
                    hexReplacement = "024cc47a";
                }
                //__Highland seaground rocks dark flat (pattern_h_seaground_01)
                if (SandsAdKIndex == 1)
                {
                    hexReplacement = "034cc47a";
                }
                //__Highland seaground pebbles (pattern_h_seaground_02)
                if (SandsAdKIndex == 2)
                {
                    hexReplacement = "044cc47a";
                }

                //MessageBox.Show(hexToReplace);
                //MessageBox.Show(hexReplacement);

                ReplaceAllHexInFile(destinationPath, hexToReplace, hexReplacement);
            }
            else
            {
                // Something is not selected
                MessageBox.Show("Please select something.", "Something is not selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BlockedButton_Click(object sender, EventArgs e)
        {
            if (Blocked10.SelectedIndex != -1 && BlockedAdK.SelectedIndex != -1)
            {
                // If something is selected then index can be read
                int Blocked10Index = Blocked10.SelectedIndex;
                int BlockedAdKIndex = BlockedAdK.SelectedIndex;
                string destinationPath = Path.GetFileName(FilePath.Text);
                string hexToReplace = "";
                string hexReplacement = "";

                //10th Anniversary

                //border (pattern_border)
                if (Blocked10Index == 0)
                {
                    hexToReplace = "7318d376";
                }
                //water (pattern_water)
                if (Blocked10Index == 1)
                {
                    hexToReplace = "b3d16bfe";
                }
                //snow (pattern_snow)
                if (Blocked10Index == 2)
                {
                    hexToReplace = "ffe0ad0f";
                }
                //swamp land (pattern_swamp)
                if (Blocked10Index == 3)
                {
                    hexToReplace = "e4040068";
                }
                //swamp water (pattern_swamp1)
                if (Blocked10Index == 4)
                {
                    hexToReplace = "e5040068";
                }
                //((00 LAVA 01 (pattern_l_ground_01)
                if (Blocked10Index == 5)
                {
                    hexToReplace = "03decade";
                }
                //((00 LAVA 02 (pattern_l_ground_03)
                if (Blocked10Index == 6)
                {
                    hexToReplace = "08decade";
                }
                //((00 LAVA 01 soft (pattern_l_ground_05)
                if (Blocked10Index == 7)
                {
                    hexToReplace = "0adecade";
                }

                //Aufbruch der Kulturen

                //__Highland swamp land (pattern_h_swamp_00)
                if (BlockedAdKIndex == 0)
                {
                    hexReplacement = "104cc47a";
                }
                //__Highland swamp water (pattern_h_swamp_01)
                if (BlockedAdKIndex == 1)
                {
                    hexReplacement = "114cc47a";
                }
                //--Snow Ice Crackles (pattern_s_ice_00)
                if (BlockedAdKIndex == 2)
                {
                    hexReplacement = "0e5ec47a";
                }
                //--Snow Ice Crackles Dark (pattern_s_ice_01)
                if (BlockedAdKIndex == 3)
                {
                    hexReplacement = "0f5ec47a";
                }
                //--Snow Ice Clean (pattern_s_ice_02)
                if (BlockedAdKIndex == 4)
                {
                    hexReplacement = "105ec47a";
                }
                //--Snow Ice Clean Dark (pattern_s_ice_03)
                if (BlockedAdKIndex == 5)
                {
                    hexReplacement = "135ec47a";
                }
                //--Snow medium border (pattern_z_snow_01)
                if (BlockedAdKIndex == 6)
                {
                    hexReplacement = "115ec47a";
                }
                //--Snow soft border (pattern_z_snow_02)
                if (BlockedAdKIndex == 7)
                {
                    hexReplacement = "125ec47a";
                }

                //MessageBox.Show(hexToReplace);
                //MessageBox.Show(hexReplacement);

                ReplaceAllHexInFile(destinationPath, hexToReplace, hexReplacement);
            }
            else
            {
                // Something is not selected
                MessageBox.Show("Please select something.", "Something is not selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DepositsButton_Click(object sender, EventArgs e)
        {
            if (Deposits10.SelectedIndex != -1 && DepositsAdK.SelectedIndex != -1)
            {
                // If something is selected then index can be read
                int Deposits10Index = Deposits10.SelectedIndex;
                int DepositsAdKIndex = DepositsAdK.SelectedIndex;
                string destinationPath = Path.GetFileName(FilePath.Text);
                string hexToReplace = "";
                string hexReplacement = "";

                //10th Anniversary

                //Wheatfield (field_01)
                if (Deposits10Index == 0)
                {
                    hexToReplace = "9e4ceddf";
                }
                //StoneResourceA01 (stone_01)
                if (Deposits10Index == 1)
                {
                    hexToReplace = "0ed61b9f";
                }
                //StoneResourceA02 (stone_02)
                if (Deposits10Index == 2)
                {
                    hexToReplace = "5e11b15b";
                }
                //StoneResourceA03 (stone_03)
                if (Deposits10Index == 3)
                {
                    hexToReplace = "ee5bef21";
                }
                //StoneResourceA04 (stone_04)
                if (Deposits10Index == 4)
                {
                    hexToReplace = "8ecd4619";
                }
                //StoneResourceA05 (stone_05)
                if (Deposits10Index == 5)
                {
                    hexToReplace = "9e6a935d";
                }
                //StoneResourceA06 (stone_06)
                if (Deposits10Index == 6)
                {
                    hexToReplace = "fea22be4";
                }
                //!!!MED StoneResourceA01 (med_stone_01)
                if (Deposits10Index == 7)
                {
                    hexToReplace = "d07fab1d";
                }
                //!!!MED StoneResourceA02 (med_stone_02)
                if (Deposits10Index == 8)
                {
                    hexToReplace = "d17fab1d";
                }
                //!!!MED StoneResourceA03 (med_stone_03)
                if (Deposits10Index == 9)
                {
                    hexToReplace = "d27fab1d";
                }
                //!!!MED StoneResourceA04 (med_stone_04)
                if (Deposits10Index == 10)
                {
                    hexToReplace = "d37fab1d";
                }
                //!!!MED StoneResourceA05 (med_stone_05)
                if (Deposits10Index == 11)
                {
                    hexToReplace = "d47fab1d";
                }
                //!!!MED StoneResourceA06 (med_stone_06)
                if (Deposits10Index == 12)
                {
                    hexToReplace = "d57fab1d";
                }
                //BirchA (tree_01)
                if (Deposits10Index == 13)
                {
                    hexToReplace = "73ce997e";
                }
                //BirchB (tree_02)
                if (Deposits10Index == 14)
                {
                    hexToReplace = "b3873206";
                }
                //BirchC (tree_03)
                if (Deposits10Index == 15)
                {
                    hexToReplace = "83cb9c48";
                }
                //BroadLeafA (tree_04)
                if (Deposits10Index == 16)
                {
                    hexToReplace = "b3479f11";
                }
                //BroadLeafB (tree_05)
                if (Deposits10Index == 17)
                {
                    hexToReplace = "d321cfe6";
                }
                //BroadLeafC (tree_06)
                if (Deposits10Index == 18)
                {
                    hexToReplace = "c344efad";
                }
                //FirA (tree_07)
                if (Deposits10Index == 19)
                {
                    hexToReplace = "730e2d73";
                }
                //FirB (tree_08)
                if (Deposits10Index == 20)
                {
                    hexToReplace = "730ecfe6";
                }
                //PalmA (tree_09)
                if (Deposits10Index == 21)
                {
                    hexToReplace = "741ecfe7";
                }
                //PalmB (tree_10)
                if (Deposits10Index == 22)
                {
                    hexToReplace = "752ecfe8";
                }
                //CypressA (tree_11)
                if (Deposits10Index == 23)
                {
                    hexToReplace = "762ecfe8";
                }
                //OliveA (tree_12)
                if (Deposits10Index == 24)
                {
                    hexToReplace = "772ecfe8";
                }
                //AfricanA (tree_13)
                if (Deposits10Index == 25)
                {
                    hexToReplace = "782ecfe8";
                }
                //LavaTreeA (tree_lava01)
                if (Deposits10Index == 26)
                {
                    hexToReplace = "792ecfe8";
                }
                //LavaTreeB (tree_lava02)
                if (Deposits10Index == 27)
                {
                    hexToReplace = "7a2ecfe8";
                }
                //LavaTreeC (tree_lava03)
                if (Deposits10Index == 28)
                {
                    hexToReplace = "7b2ecfe8";
                }
                //AsianA (tree_14)
                if (Deposits10Index == 29)
                {
                    hexToReplace = "7c2ecfe8";
                }

                //Aufbruch der Kulturen

                //field_egypt (field_egypt)
                if (DepositsAdKIndex == 0)
                {
                    hexReplacement = "1a2e6ba2";
                }
                //__HighlandFirA (tree_15)
                if (DepositsAdKIndex == 1)
                {
                    hexReplacement = "7d2ecfe8";
                }
                //__HighlandFirB (tree_16)
                if (DepositsAdKIndex == 2)
                {
                    hexReplacement = "7e2ecfe8";
                }
                //__HighlandFirC (tree_17)
                if (DepositsAdKIndex == 3)
                {
                    hexReplacement = "7f2ecfe8";
                }
                //--SnowFirA straight pos (tree_18)
                if (DepositsAdKIndex == 4)
                {
                    hexReplacement = "802ecfe8";
                }
                //--SnowFirB straight pos (tree_19)
                if (DepositsAdKIndex == 5)
                {
                    hexReplacement = "812ecfe8";
                }
                //--SnowFirC straight pos (tree_20)
                if (DepositsAdKIndex == 6)
                {
                    hexReplacement = "822ecfe8";
                }
                //--SnowFirA random pos (tree_21)
                if (DepositsAdKIndex == 7)
                {
                    hexReplacement = "832ecfe8";
                }
                //--SnowFirB random pos (tree_22)
                if (DepositsAdKIndex == 8)
                {
                    hexReplacement = "842ecfe8";
                }
                //--SnowFirC random pos (tree_23)
                if (DepositsAdKIndex == 9)
                {
                    hexReplacement = "852ecfe8";
                }
                //--SnowFirD random pos (tree_24)
                if (DepositsAdKIndex == 10)
                {
                    hexReplacement = "862ecfe8";
                }
                //--SnowFirE random pos (tree_25)
                if (DepositsAdKIndex == 11)
                {
                    hexReplacement = "872ecfe8";
                }
                //--SnowFirF random pos (tree_26)
                if (DepositsAdKIndex == 12)
                {
                    hexReplacement = "882ecfe8";
                }
                //Weeping Willow (tree_27)
                if (DepositsAdKIndex == 13)
                {
                    hexReplacement = "892ecfe8";
                }
                //Birch New 1 (tree_28)
                if (DepositsAdKIndex == 14)
                {
                    hexReplacement = "8a2ecfe8";
                }
                //Birch New 2 (tree_29)
                if (DepositsAdKIndex == 15)
                {
                    hexReplacement = "8b2ecfe8";
                }
                //Birch New 3 (tree_30)
                if (DepositsAdKIndex == 16)
                {
                    hexReplacement = "8c2ecfe8";
                }
                //Chestnut 1 (tree_31)
                if (DepositsAdKIndex == 17)
                {
                    hexReplacement = "8d2ecfe8";
                }
                //Chestnut 2 (tree_32)
                if (DepositsAdKIndex == 18)
                {
                    hexReplacement = "8e2ecfe8";
                }
                //Chestnut 3 (tree_33)
                if (DepositsAdKIndex == 19)
                {
                    hexReplacement = "8f2ecfe8";
                }
                //Apple Tree 1 (tree_34)
                if (DepositsAdKIndex == 20)
                {
                    hexReplacement = "902ecfe8";
                }
                //Apple Tree 2 (tree_35)
                if (DepositsAdKIndex == 21)
                {
                    hexReplacement = "912ecfe8";
                }
                //Tent (tent)
                if (DepositsAdKIndex == 22)
                {
                    hexReplacement = "a3e75d77";
                }

                //MessageBox.Show(hexToReplace);
                //MessageBox.Show(hexReplacement);

                ReplaceAllHexInFile(destinationPath, hexToReplace, hexReplacement);
            }
            else
            {
                // Something is not selected
                MessageBox.Show("Please select something.", "Something is not selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DoodadsButton_Click(object sender, EventArgs e)
        {
            if (Doodads10.SelectedIndex != -1 && DoodadsAdK.SelectedIndex != -1)
            {
                // If something is selected then index can be read
                int Doodads10Index = Doodads10.SelectedIndex;
                int DoodadsAdKIndex = DoodadsAdK.SelectedIndex;
                string destinationPath = Path.GetFileName(FilePath.Text);
                string hexToReplace = "";
                string hexReplacement = "";

                //10th Anniversary

                //bush01 (doodad_bush_0)
                if (Doodads10Index == 0)
                {
                    hexToReplace = "de2e276b";
                }
                //stone01 (doodad_stones_0)
                if (Doodads10Index == 1)
                {
                    hexToReplace = "4e1f5c45";
                }
                //stone02 (doodad_stones_1)
                if (Doodads10Index == 2)
                {
                    hexToReplace = "3e0236ea";
                }
                //stone03 (doodad_stones_2)
                if (Doodads10Index == 3)
                {
                    hexToReplace = "8ed8419f";
                }
                //stone04 (doodad_stones_3)
                if (Doodads10Index == 4)
                {
                    hexToReplace = "6ebecba7";
                }
                //stone01 grey (doodad_stones_4)
                if (Doodads10Index == 5)
                {
                    hexToReplace = "4f1f5c45";
                }
                //stone02 grey (doodad_stones_5)
                if (Doodads10Index == 6)
                {
                    hexToReplace = "3f0236ea";
                }
                //stone03 grey (doodad_stones_6)
                if (Doodads10Index == 7)
                {
                    hexToReplace = "8fd8419f";
                }
                //stone04 grey (doodad_stones_7)
                if (Doodads10Index == 8)
                {
                    hexToReplace = "6fbecba7";
                }
                //dead Tree 1 (doodad_deadtree_0)
                if (Doodads10Index == 9)
                {
                    hexToReplace = "1ee8bff2";
                }
                //dead Tree 2 (doodad_deadtree_1)
                if (Doodads10Index == 10)
                {
                    hexToReplace = "1fe8bff2";
                }
                //grass01 (doodad_grass_0)
                if (Doodads10Index == 11)
                {
                    hexToReplace = "ae37d13a";
                }
                //grass02 (doodad_grass_1)
                if (Doodads10Index == 12)
                {
                    hexToReplace = "af37d13a";
                }
                //grass03 (doodad_grass_2)
                if (Doodads10Index == 13)
                {
                    hexToReplace = "b037d13a";
                }
                //grass04 (doodad_grass_3)
                if (Doodads10Index == 14)
                {
                    hexToReplace = "b137d13a";
                }
                //cactus01 (doodad_plants_0)
                if (Doodads10Index == 15)
                {
                    hexToReplace = "ee502048";
                }
                //cactus02 (doodad_plants_1)
                if (Doodads10Index == 16)
                {
                    hexToReplace = "0e8b06a3";
                }
                //cactus03 (doodad_plants_2)
                if (Doodads10Index == 17)
                {
                    hexToReplace = "7e5b18ec";
                }
                //high flower white big (doodad_plants_3)
                if (Doodads10Index == 18)
                {
                    hexToReplace = "fecd49fb";
                }
                //high flower white (doodad_plants_4)
                if (Doodads10Index == 19)
                {
                    hexToReplace = "be34d404";
                }
                //fern big (doodad_plants_5)
                if (Doodads10Index == 20)
                {
                    hexToReplace = "ce3124a1";
                }
                //fern medium (doodad_plants_6)
                if (Doodads10Index == 21)
                {
                    hexToReplace = "33aef589";
                }
                //fern small (doodad_plants_7)
                if (Doodads10Index == 22)
                {
                    hexToReplace = "34aef589";
                }
                //high flower red big (doodad_plants_8)
                if (Doodads10Index == 23)
                {
                    hexToReplace = "35aef589";
                }
                //high flower red (doodad_plants_9)
                if (Doodads10Index == 24)
                {
                    hexToReplace = "36aef589";
                }
                //high flower yellow big (doodad_plants_10)
                if (Doodads10Index == 25)
                {
                    hexToReplace = "37aef589";
                }
                //high flower yellow (doodad_plants_11)
                if (Doodads10Index == 26)
                {
                    hexToReplace = "38aef589";
                }
                //cactus04 (doodad_plants_12)
                if (Doodads10Index == 27)
                {
                    hexToReplace = "39aef589";
                }
                //swampthing01 (doodad_swamp_0)
                if (Doodads10Index == 28)
                {
                    hexToReplace = "e1bedefa";
                }
                //swampthing02 (doodad_swamp_1)
                if (Doodads10Index == 29)
                {
                    hexToReplace = "e2bedefa";
                }
                //swamp calmus 01 (doodad_swamp_2)
                if (Doodads10Index == 30)
                {
                    hexToReplace = "e3bedefa";
                }
                //swamp calmus 02 (doodad_swamp_3)
                if (Doodads10Index == 31)
                {
                    hexToReplace = "e4bedefa";
                }
                //swamp calmus 03 (doodad_swamp_4)
                if (Doodads10Index == 32)
                {
                    hexToReplace = "e5bedefa";
                }
                //nettle (doodad_greenery_0)
                if (Doodads10Index == 33)
                {
                    hexToReplace = "e1afa10f";
                }
                //nettle big (doodad_greenery_1)
                if (Doodads10Index == 34)
                {
                    hexToReplace = "e2afa10f";
                }
                //nettle high (doodad_greenery_2)
                if (Doodads10Index == 35)
                {
                    hexToReplace = "e3afa10f";
                }
                //flower red (doodad_greenery_3)
                if (Doodads10Index == 36)
                {
                    hexToReplace = "e4afa10f";
                }
                //flower red big (doodad_greenery_4)
                if (Doodads10Index == 37)
                {
                    hexToReplace = "e5afa10f";
                }
                //flower red high (doodad_greenery_5)
                if (Doodads10Index == 38)
                {
                    hexToReplace = "e6afa10f";
                }
                //flower white (doodad_greenery_6)
                if (Doodads10Index == 39)
                {
                    hexToReplace = "e7afa10f";
                }
                //flower white big (doodad_greenery_7)
                if (Doodads10Index == 40)
                {
                    hexToReplace = "e8afa10f";
                }
                //flower white high (doodad_greenery_8)
                if (Doodads10Index == 41)
                {
                    hexToReplace = "e9afa10f";
                }
                //flower yellow (doodad_greenery_9)
                if (Doodads10Index == 42)
                {
                    hexToReplace = "eaafa10f";
                }
                //flower yellow big (doodad_greenery_10)
                if (Doodads10Index == 43)
                {
                    hexToReplace = "ebafa10f";
                }
                //flower yellow high (doodad_greenery_11)
                if (Doodads10Index == 44)
                {
                    hexToReplace = "ecafa10f";
                }
                //flower violet (doodad_greenery_12)
                if (Doodads10Index == 45)
                {
                    hexToReplace = "edafa10f";
                }
                //flower violet big (doodad_greenery_13)
                if (Doodads10Index == 46)
                {
                    hexToReplace = "eeafa10f";
                }
                //flower violet high (doodad_greenery_14)
                if (Doodads10Index == 47)
                {
                    hexToReplace = "efafa10f";
                }
                //grass translucent (doodad_greenery_15)
                if (Doodads10Index == 48)
                {
                    hexToReplace = "f0afa10f";
                }
                //grass translucent big dark (doodad_greenery_16)
                if (Doodads10Index == 49)
                {
                    hexToReplace = "f1afa10f";
                }
                //waterplant 1 (doodad_waterplant_0)
                if (Doodads10Index == 50)
                {
                    hexToReplace = "30a2c6f1";
                }
                //waterplant 2 (doodad_waterplant_1)
                if (Doodads10Index == 51)
                {
                    hexToReplace = "31a2c6f1";
                }
                //waterplant 3 (doodad_waterplant_2)
                if (Doodads10Index == 52)
                {
                    hexToReplace = "32a2c6f1";
                }
                //waterlily 1 (doodad_waterlily_0)
                if (Doodads10Index == 53)
                {
                    hexToReplace = "30a2d6f1";
                }
                //waterlily 2 (doodad_waterlily_1)
                if (Doodads10Index == 54)
                {
                    hexToReplace = "31a2d6f1";
                }
                //Empty (doodad_emptySign)
                if (Doodads10Index == 55)
                {
                    hexToReplace = "4323f428";
                }
                //Water (doodad_waterSign)
                if (Doodads10Index == 56)
                {
                    hexToReplace = "43a31a12";
                }
                //Coal (few) (doodad_coalSignS)
                if (Doodads10Index == 57)
                {
                    hexToReplace = "93b7ee90";
                }
                //Coal (medium) (doodad_coalSignM)
                if (Doodads10Index == 58)
                {
                    hexToReplace = "436109c5";
                }
                //Coal (much) (doodad_coalSignL)
                if (Doodads10Index == 59)
                {
                    hexToReplace = "f36fad00";
                }
                //Iron (few) (doodad_ironSignS)
                if (Doodads10Index == 60)
                {
                    hexToReplace = "63e5db45";
                }
                //Iron (medium) (doodad_ironSignM)
                if (Doodads10Index == 61)
                {
                    hexToReplace = "e3523ad3";
                }
                //Iron (much) (doodad_ironSignL)
                if (Doodads10Index == 62)
                {
                    hexToReplace = "23f1824b";
                }
                //Gold (few) (doodad_goldSignS)
                if (Doodads10Index == 63)
                {
                    hexToReplace = "d31a7796";
                }
                //Gold (medium) (doodad_goldSignM)
                if (Doodads10Index == 64)
                {
                    hexToReplace = "a3c36ae0";
                }
                //Gold (much) (doodad_goldSignL)
                if (Doodads10Index == 65)
                {
                    hexToReplace = "23d112e8";
                }
                //Granit (few) (doodad_granitSignS)
                if (Doodads10Index == 66)
                {
                    hexToReplace = "93a12431";
                }
                //Granit (medium) (doodad_granitSignM)
                if (Doodads10Index == 67)
                {
                    hexToReplace = "530dcf8e";
                }
                //Granit (much) (doodad_granitSignL)
                if (Doodads10Index == 68)
                {
                    hexToReplace = "73476817";
                }
                //fingerpost E (doodad_fingerpost_0)
                if (Doodads10Index == 69)
                {
                    hexToReplace = "e0f1a0aa";
                }
                //fingerpost SE (doodad_fingerpost_1)
                if (Doodads10Index == 70)
                {
                    hexToReplace = "e1f1a0aa";
                }
                //fingerpost S (doodad_fingerpost_2)
                if (Doodads10Index == 71)
                {
                    hexToReplace = "e2f1a0aa";
                }
                //fingerpost SW (doodad_fingerpost_3)
                if (Doodads10Index == 72)
                {
                    hexToReplace = "e3f1a0aa";
                }
                //fingerpost W (doodad_fingerpost_4)
                if (Doodads10Index == 73)
                {
                    hexToReplace = "e4f1a0aa";
                }
                //fingerpost NW (doodad_fingerpost_5)
                if (Doodads10Index == 74)
                {
                    hexToReplace = "e5f1a0aa";
                }
                //fingerpost N (doodad_fingerpost_6)
                if (Doodads10Index == 75)
                {
                    hexToReplace = "e6f1a0aa";
                }
                //fingerpost NE (doodad_fingerpost_7)
                if (Doodads10Index == 76)
                {
                    hexToReplace = "e7f1a0aa";
                }
                //mushroom red (doodad_mushroom_0)
                if (Doodads10Index == 77)
                {
                    hexToReplace = "f0efadac";
                }
                //mushroom red big (doodad_mushroom_1)
                if (Doodads10Index == 78)
                {
                    hexToReplace = "f1efadac";
                }
                //mushroom brown (doodad_mushroom_2)
                if (Doodads10Index == 79)
                {
                    hexToReplace = "f2efadac";
                }
                //mushroom brown big (doodad_mushroom_3)
                if (Doodads10Index == 80)
                {
                    hexToReplace = "f3efadac";
                }
                //agave big (doodad_agave_0)
                if (Doodads10Index == 81)
                {
                    hexToReplace = "f07ca68f";
                }
                //agave medium (doodad_agave_1)
                if (Doodads10Index == 82)
                {
                    hexToReplace = "f17ca68f";
                }
                //agave small (doodad_agave_2)
                if (Doodads10Index == 83)
                {
                    hexToReplace = "f27ca68f";
                }
                //bones0 (doodad_bones_0)
                if (Doodads10Index == 84)
                {
                    hexToReplace = "9da7f5d5";
                }
                //bones1 (doodad_bones_1)
                if (Doodads10Index == 85)
                {
                    hexToReplace = "cd225117";
                }
                //bones2 (doodad_bones_2)
                if (Doodads10Index == 86)
                {
                    hexToReplace = "ada44572";
                }
                //bones3 (doodad_bones_3)
                if (Doodads10Index == 87)
                {
                    hexToReplace = "5d6e8337";
                }
                //wreck (doodad_wreck_0)
                if (Doodads10Index == 88)
                {
                    hexToReplace = "10e211fa";
                }
                //wreck big (doodad_wreck_1)
                if (Doodads10Index == 89)
                {
                    hexToReplace = "11e211fa";
                }
                //shell (doodad_shell_0)
                if (Doodads10Index == 90)
                {
                    hexToReplace = "10e311fa";
                }
                //shell small (doodad_shell_1)
                if (Doodads10Index == 91)
                {
                    hexToReplace = "11e311fa";
                }
                //((LAVA fog (doodad_lavafog_1)
                if (Doodads10Index == 92)
                {
                    hexToReplace = "c017ffaa";
                }
                //((LAVA fog high (doodad_lavafog_2)
                if (Doodads10Index == 93)
                {
                    hexToReplace = "c117ffaa";
                }
                //((LAVA fog highest (doodad_lavafog_3)
                if (Doodads10Index == 94)
                {
                    hexToReplace = "c217ffaa";
                }
                //((LAVA fog vertical (doodad_lavafog_4)
                if (Doodads10Index == 95)
                {
                    hexToReplace = "c317ffaa";
                }
                //!!MED nettle (doodad_med_greenery_0)
                if (Doodads10Index == 96)
                {
                    hexToReplace = "3042a7bc";
                }
                //!!MED nettle big (doodad_med_greenery_1)
                if (Doodads10Index == 97)
                {
                    hexToReplace = "3142a7bc";
                }
                //!!MED nettle high (doodad_med_greenery_2)
                if (Doodads10Index == 98)
                {
                    hexToReplace = "3242a7bc";
                }
                //DoNotUse-Skull01 (doodad_deadsettler)
                if (Doodads10Index == 99)
                {
                    hexToReplace = "130acbda";
                }

                //Aufbruch der Kulturen

                //Chest (doodad_chest)
                if (DoodadsAdKIndex == 0)
                {
                    hexReplacement = "74be457a";
                }
                //OpenChest (doodad_openChest)
                if (DoodadsAdKIndex == 1)
                {
                    hexReplacement = "34bff916";
                }
                //Anchor (doodad_anchor)
                if (DoodadsAdKIndex == 2)
                {
                    hexReplacement = "001bfff1";
                }
                //Coal (endless) (doodad_coalSignX)
                if (DoodadsAdKIndex == 3)
                {
                    hexReplacement = "63a05ac5";
                }
                //Iron (endless) (doodad_ironSignX)
                if (DoodadsAdKIndex == 4)
                {
                    hexReplacement = "739d5d8f";
                }
                //Gold (endless) (doodad_goldSignX)
                if (DoodadsAdKIndex == 5)
                {
                    hexReplacement = "63c0ca28";
                }
                //Granite (endless) (doodad_granitSignX)
                if (DoodadsAdKIndex == 6)
                {
                    hexReplacement = "e3d245b2";
                }
                //Gemstones (few) (doodad_gemstonesSignS)
                if (DoodadsAdKIndex == 7)
                {
                    hexReplacement = "33d2284e";
                }
                //Gemstones (medium) (doodad_gemstonesSignM)
                if (DoodadsAdKIndex == 8)
                {
                    hexReplacement = "037696e8";
                }
                //Gemstones (much) (doodad_gemstonesSignL)
                if (DoodadsAdKIndex == 9)
                {
                    hexReplacement = "536cc52e";
                }
                //Gemstones (endless) (doodad_gemstonesSignX)
                if (DoodadsAdKIndex == 10)
                {
                    hexReplacement = "030ddf3a";
                }
                //Salt (few) (doodad_saltSignS)
                if (DoodadsAdKIndex == 11)
                {
                    hexReplacement = "c3dc20f2";
                }
                //Salt (medium) (doodad_saltSignM)
                if (DoodadsAdKIndex == 12)
                {
                    hexReplacement = "c33cd777";
                }
                //Salt (much) (doodad_saltSignL)
                if (DoodadsAdKIndex == 13)
                {
                    hexReplacement = "f35823bb";
                }
                //Salt (endless) (doodad_saltSignX)
                if (DoodadsAdKIndex == 14)
                {
                    hexReplacement = "a37e3632";
                }
                //--Snow Ice Floe 01 moving (doodad_ice_01moving)
                if (DoodadsAdKIndex == 15)
                {
                    hexReplacement = "01bb81a1";
                }
                //--Snow Ice Floe 01 static (doodad_ice_01static)
                if (DoodadsAdKIndex == 16)
                {
                    hexReplacement = "02bb81a1";
                }
                //--Snow Ice Floe 02 static (doodad_ice_02static)
                if (DoodadsAdKIndex == 17)
                {
                    hexReplacement = "03bb81a1";
                }
                //--Snow Ice Floe 03 static (doodad_ice_03static)
                if (DoodadsAdKIndex == 18)
                {
                    hexReplacement = "04bb81a1";
                }
                //--Snow Ice Floe 04 static (doodad_ice_04static)
                if (DoodadsAdKIndex == 19)
                {
                    hexReplacement = "05bb81a1";
                }
                //--Snow Ice Floe 05 static (doodad_ice_05static)
                if (DoodadsAdKIndex == 20)
                {
                    hexReplacement = "06bb81a1";
                }
                //--Snow Ice Floe 06 moving (doodad_ice_06moving)
                if (DoodadsAdKIndex == 21)
                {
                    hexReplacement = "07bb81a1";
                }
                //--Snow Ice Floe 07 moving (doodad_ice_07moving)
                if (DoodadsAdKIndex == 22)
                {
                    hexReplacement = "08bb81a1";
                }
                //--Snow Ice Floe 08 moving (doodad_ice_08moving)
                if (DoodadsAdKIndex == 23)
                {
                    hexReplacement = "09bb81a1";
                }
                //--Snow Ice Floe 09 moving (doodad_ice_09moving)
                if (DoodadsAdKIndex == 24)
                {
                    hexReplacement = "0abb81a1";
                }
                //__Highland fern big (doodad_fernHighland_00)
                if (DoodadsAdKIndex == 25)
                {
                    hexReplacement = "20bb81a1";
                }
                //__Highland fern miedium (doodad_fernHighland_01)
                if (DoodadsAdKIndex == 26)
                {
                    hexReplacement = "21bb81a1";
                }
                //__Highland fern small (doodad_fernHighland_02)
                if (DoodadsAdKIndex == 27)
                {
                    hexReplacement = "22bb81a1";
                }
                //__Highland nettle (doodad_greeneryHighland_00)
                if (DoodadsAdKIndex == 28)
                {
                    hexReplacement = "30bb81a1";
                }
                //__Highland nettle big (doodad_greeneryHighland_01)
                if (DoodadsAdKIndex == 29)
                {
                    hexReplacement = "31bb81a1";
                }
                //__Highland nettle high (doodad_greeneryHighland_02)
                if (DoodadsAdKIndex == 30)
                {
                    hexReplacement = "32bb81a1";
                }
                //__Highland Edelweiss 1 (doodad_greeneryHighland_03)
                if (DoodadsAdKIndex == 31)
                {
                    hexReplacement = "33bb81a1";
                }
                //__Highland Edelweiss 2 (doodad_greeneryHighland_04)
                if (DoodadsAdKIndex == 32)
                {
                    hexReplacement = "34bb81a1";
                }
                //__Highland Edelweiss 3 (doodad_greeneryHighland_05)
                if (DoodadsAdKIndex == 33)
                {
                    hexReplacement = "35bb81a1";
                }
                //__Highland Snowdrop (doodad_greeneryHighland_06)
                if (DoodadsAdKIndex == 34)
                {
                    hexReplacement = "36bb81a1";
                }
                //__Highland Crocus (doodad_greeneryHighland_07)
                if (DoodadsAdKIndex == 35)
                {
                    hexReplacement = "37bb81a1";
                }
                //__Highland Foundling 1 (doodad_stoneHighland_00)
                if (DoodadsAdKIndex == 36)
                {
                    hexReplacement = "40bb81a1";
                }
                //__Highland Foundling 2 (doodad_stoneHighland_01)
                if (DoodadsAdKIndex == 37)
                {
                    hexReplacement = "41bb81a1";
                }
                //__Highland Foundling 3 (doodad_stoneHighland_02)
                if (DoodadsAdKIndex == 38)
                {
                    hexReplacement = "42bb81a1";
                }
                //__Highland Underwater Foundling 1 (doodad_stoneHighland_03)
                if (DoodadsAdKIndex == 39)
                {
                    hexReplacement = "43bb81a1";
                }
                //__Highland Underwater Foundling 2 (doodad_stoneHighland_04)
                if (DoodadsAdKIndex == 40)
                {
                    hexReplacement = "44bb81a1";
                }
                //__Highland Underwater Foundling 3 (doodad_stoneHighland_05)
                if (DoodadsAdKIndex == 41)
                {
                    hexReplacement = "45bb81a1";
                }
                //__Highland swamp calmus 01 (doodad_swamp_Highland2)
                if (DoodadsAdKIndex == 42)
                {
                    hexReplacement = "40bc81a1";
                }
                //__Highland swamp calmus 02 (doodad_swamp_Highland3)
                if (DoodadsAdKIndex == 43)
                {
                    hexReplacement = "41bc81a1";
                }
                //__Highland swamp calmus 03 (doodad_swamp_Highland4)
                if (DoodadsAdKIndex == 44)
                {
                    hexReplacement = "42bc81a1";
                }
                //__Highland Fog 01 (doodad_fogHighland_00)
                if (DoodadsAdKIndex == 45)
                {
                    hexReplacement = "00bd81a1";
                }
                //__Highland Fog 02 (doodad_fogHighland_01)
                if (DoodadsAdKIndex == 46)
                {
                    hexReplacement = "01bd81a1";
                }
                //Male Duck (doodad_duck_00)
                if (DoodadsAdKIndex == 47)
                {
                    hexReplacement = "10bd81a1";
                }
                //Female Duck (doodad_duck_01)
                if (DoodadsAdKIndex == 48)
                {
                    hexReplacement = "11bd81a1";
                }

                //MessageBox.Show(hexToReplace);
                //MessageBox.Show(hexReplacement);

                ReplaceAllHexInFile(destinationPath, hexToReplace, hexReplacement);
            }
            else
            {
                // Something is not selected
                MessageBox.Show("Please select something.", "Something is not selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BlockedDoodadsButton_Click(object sender, EventArgs e)
        {
            if (BlockedDoodads10.SelectedIndex != -1 && BlockedDoodadsAdK.SelectedIndex != -1)
            {
                // If something is selected then index can be read
                int BlockedDoodads10Index = BlockedDoodads10.SelectedIndex;
                int BlockedDoodadsAdKIndex = BlockedDoodadsAdK.SelectedIndex;
                string destinationPath = Path.GetFileName(FilePath.Text);
                string hexToReplace = "";
                string hexReplacement = "";

                //10th Anniversary

                //Gate01 (doodad_gate_0)
                if (BlockedDoodads10Index == 0)
                {
                    hexToReplace = "e6bedefa";
                }
                //rock 1 (doodad_rock_0)
                if (BlockedDoodads10Index == 1)
                {
                    hexToReplace = "a0e0af6f";
                }
                //rock 2 (doodad_rock_1)
                if (BlockedDoodads10Index == 2)
                {
                    hexToReplace = "a1e0af6f";
                }
                //rock 3 (doodad_rock_2)
                if (BlockedDoodads10Index == 3)
                {
                    hexToReplace = "a2e0af6f";
                }
                //rock 4 (doodad_rock_3)
                if (BlockedDoodads10Index == 4)
                {
                    hexToReplace = "a3e0af6f";
                }
                //((LAVA rock 0 (doodad_lavarock_0)
                if (BlockedDoodads10Index == 5)
                {
                    hexToReplace = "a0eeffca";
                }
                //((LAVA rock 1 (doodad_lavarock_1)
                if (BlockedDoodads10Index == 6)
                {
                    hexToReplace = "a1eeffca";
                }
                //((LAVA rock 2 (doodad_lavarock_2)
                if (BlockedDoodads10Index == 7)
                {
                    hexToReplace = "a2eeffca";
                }
                //!!MED rock 1 (doodad_medrock_0)
                if (BlockedDoodads10Index == 8)
                {
                    hexToReplace = "a0c091fa";
                }
                //!!MED rock 2 (doodad_medrock_1)
                if (BlockedDoodads10Index == 9)
                {
                    hexToReplace = "a1c091fa";
                }
                //!MED rock 3 (doodad_medrock_2)
                if (BlockedDoodads10Index == 10)
                {
                    hexToReplace = "a2c091fa";
                }
                //!!MED rock 4 (doodad_medrock_3)
                if (BlockedDoodads10Index == 11)
                {
                    hexToReplace = "a3c091fa";
                }

                //Aufbruch der Kulturen

                //__Highland rock 1 (doodad_rockHighland_00)
                if (BlockedDoodadsAdKIndex == 0)
                {
                    hexReplacement = "10bb81a1";
                }
                //__Highland rock 2 (doodad_rockHighland_01)
                if (BlockedDoodadsAdKIndex == 1)
                {
                    hexReplacement = "11bb81a1";
                }
                //__Highland rock 3 (doodad_rockHighland_02)
                if (BlockedDoodadsAdKIndex == 2)
                {
                    hexReplacement = "12bb81a1";
                }
                //__Highland rock 4 (doodad_rockHighland_03)
                if (BlockedDoodadsAdKIndex == 3)
                {
                    hexReplacement = "13bb81a1";
                }
                //--Snow Iceberg 1 (doodad_iceberg_00)
                if (BlockedDoodadsAdKIndex == 4)
                {
                    hexReplacement = "20102ff2";
                }
                //--Snow Iceberg 2 (doodad_iceberg_01)
                if (BlockedDoodadsAdKIndex == 5)
                {
                    hexReplacement = "21102ff2";
                }
                //Tent (doodad_tent)
                if (BlockedDoodadsAdKIndex == 6)
                {
                    hexReplacement = "032d663d";
                }

                //MessageBox.Show(hexToReplace);
                //MessageBox.Show(hexReplacement);

                ReplaceAllHexInFile(destinationPath, hexToReplace, hexReplacement);
            }
            else
            {
                // Something is not selected
                MessageBox.Show("Please select something.", "Something is not selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AnimalsButton_Click(object sender, EventArgs e)
        {
            if (Animals10.SelectedIndex != -1 && AnimalsAdK.SelectedIndex != -1)
            {
                // If something is then selected index can be read
                int Animals10Index = Animals10.SelectedIndex;
                int AnimalsAdKIndex = AnimalsAdK.SelectedIndex;
                string destinationPath = Path.GetFileName(FilePath.Text);
                string hexToReplace = "";
                string hexReplacement = "";

                //10th Anniversary

                //Deer (animal_deer)
                if (Animals10Index == 0)
                {
                    hexToReplace = "83ef9b4a";
                }
                //Rabbit (animal_rabbit)
                if (Animals10Index == 1)
                {
                    hexToReplace = "767b7941";
                }
                //Elk (animal_elk)
                if (Animals10Index == 2)
                {
                    hexToReplace = "947c6e70";
                }

                //Aufbruch der Kulturen

                //Sheep (animal_sheep)
                if (AnimalsAdKIndex == 0)
                {
                    hexReplacement = "33caae62";
                }
                //Bear (animal_bear)
                if (AnimalsAdKIndex == 1)
                {
                    hexReplacement = "72a51f10";
                }
                //Ox (animal_ox)
                if (AnimalsAdKIndex == 2)
                {
                    hexReplacement = "73a51f10";
                }
                //Highland Cattle (animal_highlandcattle)
                if (AnimalsAdKIndex == 3)
                {
                    hexReplacement = "79a51f10";
                }
                //Goat (animal_goat)
                if (AnimalsAdKIndex == 4)
                {
                    hexReplacement = "74a51f10";
                }
                //Polarbear (animal_polarbear)
                if (AnimalsAdKIndex == 5)
                {
                    hexReplacement = "75a51f10";
                }
                //Mountain Hare (animal_mountainhare)
                if (AnimalsAdKIndex == 6)
                {
                    hexReplacement = "76a51f10";
                }
                //Boar (animal_boar)
                if (AnimalsAdKIndex == 7)
                {
                    hexReplacement = "77a51f10";
                }
                //Camel (animal_camel)
                if (AnimalsAdKIndex == 8)
                {
                    hexReplacement = "78a51f10";
                }

                //MessageBox.Show(hexToReplace);
                //MessageBox.Show(hexReplacement);

                ReplaceAllHexInFile(destinationPath, hexToReplace, hexReplacement);
            }
            else
            {
                // Something is not selected
                MessageBox.Show("Please select something.", "Something is not selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AmbientsButton_Click(object sender, EventArgs e)
        {
            if (Ambients10.SelectedIndex != -1 && AmbientsAdK.SelectedIndex != -1)
            {
                // If something is selected then index can be read
                int Ambients10Index = Ambients10.SelectedIndex;
                int AmbientsAdKIndex = AmbientsAdK.SelectedIndex;
                string destinationPath = Path.GetFileName(FilePath.Text);
                string hexToReplace = "";
                string hexReplacement = "";

                //10th Anniversary

                //Beach (ambient_beach)
                if (Ambients10Index == 0)
                {
                    hexToReplace = "7348dc5b";
                }
                //Strong Desert Wind (ambient_desert1)
                if (Ambients10Index == 1)
                {
                    hexToReplace = "f30256df";
                }
                //Middle Desert Wind (ambient_desert2)
                if (Ambients10Index == 2)
                {
                    hexToReplace = "133def67";
                }
                //Low Desert Wind (ambient_desert3)
                if (Ambients10Index == 3)
                {
                    hexToReplace = "233af231";
                }
                //bright Forest with birds (ambient_forest1)
                if (Ambients10Index == 4)
                {
                    hexToReplace = "239af589";
                }
                //dark Forest with owl (ambient_forest2)
                if (Ambients10Index == 5)
                {
                    hexToReplace = "63538e11";
                }
                //meadow with much crickets (ambient_meadow1)
                if (Ambients10Index == 6)
                {
                    hexToReplace = "d3d2aa5a";
                }
                //meadow with some crickets and birds (ambient_meadow2)
                if (Ambients10Index == 7)
                {
                    hexToReplace = "d3373462";
                }
                //small water stream (ambient_water1)
                if (Ambients10Index == 8)
                {
                    hexToReplace = "d35757f3";
                }
                //swamp (ambient_water2)
                if (Ambients10Index == 9)
                {
                    hexToReplace = "63a7683b";
                }
                //water waves (ambient_water3)
                if (Ambients10Index == 10)
                {
                    hexToReplace = "1371a600";
                }
                //river (ambient_water4)
                if (Ambients10Index == 11)
                {
                    hexToReplace = "f3515d87";
                }
                //lava (ambient_lava)
                if (Ambients10Index == 12)
                {
                    hexToReplace = "a3bb52a9";
                }

                //Aufbruch der Kulturen

                //hightlands less birds (ambient_highlands1)
                if (AmbientsAdKIndex == 0)
                {
                    hexReplacement = "530d69a4";
                }
                //hightlands normal birds (ambient_highlands2)
                if (AmbientsAdKIndex == 1)
                {
                    hexReplacement = "23b1896c";
                }
                //hightlands much birds (ambient_highlands3)
                if (AmbientsAdKIndex == 2)
                {
                    hexReplacement = "83e44e71";
                }
                //ice (ambient_ice)
                if (AmbientsAdKIndex == 3)
                {
                    hexReplacement = "630a6c6e";
                }
                //mountains (ambient_mountains)
                if (AmbientsAdKIndex == 4)
                {
                    hexReplacement = "43eb22f5";
                }

                //MessageBox.Show(hexToReplace);
                //MessageBox.Show(hexReplacement);

                ReplaceAllHexInFile(destinationPath, hexToReplace, hexReplacement);
            }
            else
            {
                // Something is not selected
                MessageBox.Show("Please select something.", "Something is not selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
