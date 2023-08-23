﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Socket ClientSocket=new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int PORT = 27001;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConnectToServer();
            //RequestLoop();
        }

        private void RequestLoop()
        {
            throw new NotImplementedException();
        }

        private void ConnectToServer()
        {
            while (!ClientSocket.Connected) {
                try
                {
                    ClientSocket.Connect(IPAddress.Parse("10.1.18.1"), PORT);
                }
                catch (Exception)
                {
                }
            }

            MessageBox.Show("Connected");

            var buffer = new byte[2048];
            int received=ClientSocket.Receive(buffer,SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);

            string text=Encoding.ASCII.GetString(data);

            App.Current.Dispatcher.Invoke(() =>
            {
                ParseForView(text);
            });

        }

        private void ParseForView(string text)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var commands = text.Split('\n');
                var result = commands.ToList();
                result.Remove("");

                foreach (var item in result)
                {
                    Button button = new Button();
                    button.FontSize = 22;
                    button.Margin = new Thickness(0, 10, 0, 0);
                    button.Content = item;
                    button.Click += Button_CommandClick;
                    CommandsStackPanel.Children.Add(button);
                }
            });
        }

        TextBox textbox=new TextBox();
        public string SelectedCommand { get; set; }
        private void Button_CommandClick(object sender, RoutedEventArgs e)
        {
            if(sender is Button bt)
            {
                var content = bt.Content.ToString();
                var result = content.Remove(content.Length - 1, 1);
                SelectedCommand = result;

                var splitResult = result.Split('\\');
                if (splitResult.Length > 2)
                {
                    textbox.Width = 200;
                    textbox.Height = 60;
                    textbox.Text = "*" + splitResult[2];
                    textbox.FontSize = 22;

                    if (paramsStackPanel.Children.Count > 3)
                    {
                        paramsStackPanel.Children.RemoveAt(3);
                        //paramsStackPanel.Children.RemoveAt(3);
                    }

                    paramsStackPanel.Children.Add(textbox);

                    Button button = new Button();
                    button.FontSize = 22;
                    button.Margin = new Thickness(0, 10, 0, 0);
                    button.Content = "Execute";
                    button.Click += ExecuteClick;
                    paramsStackPanel.Children.Add (button);

                }
                else
                {
                    if (paramsStackPanel.Children.Count > 3)
                    {
                        paramsStackPanel.Children.RemoveAt(3);
                        //paramsStackPanel.Children.RemoveAt(3);
                    }
                    SendString(result);
                }

            }
        }

        private void ExecuteClick(object sender, RoutedEventArgs e)
        {
            var result = SelectedCommand.Split('\\');
            var resultText = result[0] + "\\" + result[1] + "\\" + textbox.Text;

            if (SelectedCommand.Contains("json"))
            {
                resultText = result[0] + "\\" + result[1]+" "+textbox.Text;
            }
            SendString(resultText);
        }

        private void SendString(string resultText)
        {
            byte[]buffer=Encoding.ASCII.GetBytes(resultText);
            ClientSocket.Send(buffer, 0, buffer.Length,SocketFlags.None);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
