using ActiproSoftware.Text;
using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Text.Languages.DotNet.Implementation;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting;
using ActiproSoftware.Windows.Themes;
using Flaky;
using System;
using System.Collections.Generic;
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

			ThemeManager.CurrentTheme = "MetroDark";

			Editor.Document.Language = LoadLanguage();
			Editor.Document.Language.RegisterLineCommenter(new LineBasedLineCommenter() { StartDelimiter = "//" });
			Editor.IsLineNumberMarginVisible = true;
			Editor.IsOutliningMarginVisible = true;
			Editor.AreIndentationGuidesVisible = true;
			Editor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
			Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

			var classificationTypes = AmbientHighlightingStyleRegistry.Instance.ClassificationTypes.ToArray();
			foreach (var classificationType in classificationTypes)
				AmbientHighlightingStyleRegistry.Instance.Unregister(classificationType);

			new DisplayItemClassificationTypeProvider().RegisterAll();
			new DotNetClassificationTypeProvider().RegisterAll();

			using (var stream = LoadFile(@"dark.vssettings"))
			{
				AmbientHighlightingStyleRegistry.Instance.ImportHighlightingStyles(stream);
			}

			Editor.FontSize = 14;
			this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, OnSaveExecuted, OnSaveCanExecute));

			Host = new Host(1, @"c:\temp\flakyrec.wav");
			var code = Load();
			textBlock.Text = string.Join("\n", Host.Recompile(0, code));
			Host.Play();
			Editor.Text = code;

			this.Closing += MainWindow_Closing;
		}

		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Host.Dispose();
		}

		private static void OnSaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			MainWindow control = (MainWindow)sender;
			e.CanExecute = control.Editor.Document.IsModified;
		}

		private static void OnSaveExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			MainWindow control = (MainWindow)sender;

			control.textBlock.Text = string.Join("\n", control.Host.Recompile(0, control.Editor.Text));
			Save(control.Editor.Text);

			control.Editor.Document.IsModified = false;
		}

		private readonly Host Host;

		private static ISyntaxLanguage LoadLanguage()
		{
			SyntaxLanguageDefinitionSerializer serializer = new SyntaxLanguageDefinitionSerializer();
			return serializer.LoadFromStream(LoadFile("CSharp.langdef"));
		}

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

			using (var file = File.Open(codeFilePath, FileMode.Open))
			using (var reader = new StreamReader(file))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
