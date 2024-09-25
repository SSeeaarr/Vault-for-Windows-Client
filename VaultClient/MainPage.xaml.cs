﻿
using Microsoft.Maui.Storage;
using System.Collections;
using System.Net.Sockets;

namespace VaultClient
{
    public partial class MainPage : ContentPage
    {
        Queue paths = new Queue();

        public MainPage()
        {
            InitializeComponent();
            string path = Path.Combine(FileSystem.AppDataDirectory, "config.txt");

            if (File.Exists(path)) //check if config file exists, and if it does, loads information into the boxes.
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string line = sr.ReadLine();
                    string toRemove = "IPv4:";
                    string result = string.Empty;
                    int i = line.IndexOf(toRemove);
                    if (i >= 0)
                    {
                        result = line.Remove(i, toRemove.Length);
                        string removespaces = result.Trim();
                        ipBox.Text = removespaces;
                    }

                    string portline = sr.ReadLine();
                    string remove = "Port:";
                    result = string.Empty;
                    i = portline.IndexOf(remove);
                    if (i >= 0)
                    {
                        result = portline.Remove(i, remove.Length);
                        string removespaces = result.Trim();
                        portBox.Text = removespaces;
                    }


                }
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            string ipval = ipBox.Text;
            string port = portBox.Text;
            DisplayAlert("Vault", "Changes Saved", "Ok");

            string path = Path.Combine(FileSystem.AppDataDirectory, "config.txt");

            var dict = new Dictionary<string, string> //save inputs to dictionary in a config file.
            {
                ["IPv4: "] = ipval,
                ["Port: "] = port
            };


            using (StreamWriter sw = File.CreateText(path))
            {
                foreach (string key in dict.Keys)
                {
                    sw.WriteLine(key + dict[key]);
                }

            }

        }

        private async void selectFiles_Click(object sender, EventArgs e)
        {
            var FileNames = await FilePicker.PickMultipleAsync();

                if (FileNames != null)
                {
                    //runBox.Enabled = true; //enable only when at least one file is selected to be sent
                    foreach (var file in FileNames)
                    {
                        string currentfile = file.FullPath;
                        paths.Enqueue(currentfile);
                        System.Diagnostics.Debug.WriteLine("path queued for: " + file.FullPath);

                    }

                }
            
        }

        private void run_Click(Object sender, EventArgs e)
        {
            try
            {

                TcpClient client = new TcpClient(ipBox.Text, Int32.Parse(portBox.Text)); //connect to server on inputted ip and port


                System.Diagnostics.Debug.WriteLine("connected.");
                ipBox.IsEnabled = false;
                portBox.IsEnabled = false;
                runBtn.IsEnabled = false;
                

                var originalcount = paths.Count - 1;
                while (paths.Count > 0)
                {

                    var temp = paths.Dequeue();
                    var convertedstring = Convert.ToString(temp); //have to convert queue values into a string so it can be passed through

                    if (paths.Count < originalcount)
                    {
                        client = new TcpClient(ipBox.Text, Int32.Parse(portBox.Text));
                    }

                    SendFile(convertedstring, client);
                    
                }

                ipBox.IsEnabled = true;
                portBox.IsEnabled = true;
                runBtn.IsEnabled = true;

            }
            catch (Exception)
            {
                DisplayAlert("Vault", "Connection Failed", "Ok");
            }
        }

        private static void SendFile(String filePath, TcpClient client)
        {

            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read); //point file stream to the file path we want to send.

            byte[] buffer = new byte[4 * 1024];
            int bytesRead;

            using NetworkStream nws = client.GetStream();
            using BinaryWriter writer = new BinaryWriter(nws); //pass the network stream into the binary writer

            while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
            {
                writer.Write(buffer, 0, bytesRead);
                writer.Flush();
            }

            System.Diagnostics.Debug.WriteLine("File: " + filePath + "was sent successfully.");
        }
    }

}
