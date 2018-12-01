using Flaky;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Xml;

namespace WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			LoadTheme();

			this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, OnSaveExecuted, OnSaveCanExecute));

			SetProcessPriority();

			//Host = new Host(1, Path.Combine(GetLocation(), "flaky.wav"));
			Host = new Host(1);
			var code = Load();

			try
			{
				textBlock.Text = string.Join("\n", Host.Recompile(0, code));
			} catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			Host.Play();
			textEditor.Text = code;

			this.Closing += MainWindow_Closing;
		}

		private void LoadTheme()
		{
			using (Stream s = LoadFile("CSharp-Dark-Mode.xshd"))
			{
				using (XmlTextReader reader = new XmlTextReader(s))
				{
					textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
					textEditor.TextArea.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC8C8C8"));
					textEditor.TextArea.FontFamily = new FontFamily("Consolas");
					textEditor.WordWrap = true;
					textEditor.TextArea.FontSize = 14;
					textEditor.ShowLineNumbers = true;

					textBlock.FontFamily = new FontFamily("Consolas");
					textBlock.FontSize = 14;
				}
			}
		}

		private void SetProcessPriority()
		{
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
		}

		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Host.Dispose();
		}

		private static void OnSaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			MainWindow control = (MainWindow)sender;
			e.CanExecute = control.textEditor.IsModified;
		}

		private static void OnSaveExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow control = (MainWindow)sender;

			control.textBlock.Text = string.Join("\n", control.Host.Recompile(0, control.textEditor.Text));
			Save(control.textEditor.Text);

			control.textEditor.IsModified = false;
		}

		private readonly Host Host;

		private static Stream LoadFile(string fileName)
		{
			var assembly = Assembly.GetExecutingAssembly();

			var resources = assembly.GetManifestResourceNames();
			var resourceName = resources.Single(r => r.EndsWith(fileName));

			return assembly.GetManifestResourceStream(resourceName);
		}

		private static string GetTemporaryCodeFilePath()
		{
			return Path.Combine(GetLocation(), "temp.cs");
		}

		private static string GetLocation()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		private static void Save(string text)
		{
			using (var file = File.Open(GetTemporaryCodeFilePath(), FileMode.Create))
			using (var writer = new StreamWriter(file))
			{
				writer.Write(text);
				writer.Flush();
			}
		}

		private static string Load()
		{
			var codeFilePath = GetTemporaryCodeFilePath();

			if (!File.Exists(codeFilePath))
				return GetDemoSong();

			using (var file = File.Open(codeFilePath, FileMode.Open))
			using (var reader = new StreamReader(file))
			{
				return reader.ReadToEnd();
			}
		}

		private static string GetDemoSong()
		{
			using (Stream s = LoadFile("demo.flk"))
			using (var reader = new StreamReader(s))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
