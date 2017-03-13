using System;
using System.Collections.Generic;
using System.Linq;
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
using Renci.SshNet;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace ServerManager
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private SshClient sshclient;
		private string address;
		private string user;
		private string pass;

		public MainWindow()
		{
			InitializeComponent();
			disconnectButton.IsEnabled = false;
			command.IsEnabled = false;
			commandButton.IsEnabled = false;
			logButton.IsEnabled = false;
		}

		private void Connect_Button(object sender, RoutedEventArgs e)
		{
			var connection = Connect();
			if (connection != null)
			{
				sshclient = new SshClient(connection);
				try
				{
					sshclient.Connect();
					output.Text = $"Connected successfully to {host.Text} as {username.Text}\n";
					var uptime = sshclient.CreateCommand("uptime");
					uptime.Execute();
					output.Text += $"\nUptime {uptime.Result}";
					connectButton.IsEnabled = false;
					disconnectButton.IsEnabled = true;
					command.IsEnabled = true;
					commandButton.IsEnabled = true;
					logButton.IsEnabled = true;
				}
				catch (Exception ex)
				{
					MessageBox.Show("Failed to connect.\nCheck server details.");
					Console.WriteLine(ex.Message);

				}
			}

		}

		public ConnectionInfo Connect()
		{
			address = host.Text;
			user = username.Text;
			pass = password.Password;
			ConnectionInfo connection = null;
			try
			{
				connection = new ConnectionInfo(address, user,
				  new AuthenticationMethod[]
				  {
					new PasswordAuthenticationMethod(user,pass)
				  });

			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to connect.\nCheck server details.");
				Console.WriteLine(ex.Message);
			}
			return connection;
		}

		private void disconnectButton_Click(object sender, RoutedEventArgs e)
		{
			closeConnection();
			output.Text += "\nDisconnected from server.";
			connectButton.IsEnabled = true;
			disconnectButton.IsEnabled = false;
			commandButton.IsEnabled = false;
		}

		private void closeConnection()
		{
			sshclient.Disconnect();
			sshclient.Dispose();
			sshclient = null;
		}

		private void runCommand()
		{

			var cmd = sshclient.CreateCommand(command.Text);

			output.Text += $"${command.Text}\n";
			var result = cmd.Execute();
			output.Text += $"{cmd.Result}\n";

			var reader = new StreamReader(cmd.ExtendedOutputStream);

			output.Text += reader.ReadToEnd();


		}

		private void commandButton_Click(object sender, RoutedEventArgs e)
		{
			if (command.Text != "")
			{
				runCommand();

			}
			else
			{
				MessageBox.Show("Command input is empty.");
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				File.WriteAllText("output.log", output.Text);
				MessageBox.Show("Log saved.");

			}
			catch (Exception ex)
			{
				Console.Write(ex.Message);
			}

		}
	}
}
