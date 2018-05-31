using Flaky;
using GraphX.Controls;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Models;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using GraphX.PCL.Logic.Models;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using QuickGraph;
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

			Host = new Host(1, Path.Combine(GetLocation(), "flaky.wav"));
			var code = Load();

			Recompile(code);

			Host.Play();
			textEditor.Text = code;

			this.Closing += MainWindow_Closing;
		}

		private bool Recompile(string code)
		{
			try
			{
				var (errors, sourceRoot) = Host.Recompile(0, code);

				var totalWeight = sourceRoot.GetOrder();

				textBlock.Text = string.Join("\n", errors);

				var graph = new BidirectionalGraph<SourceVertex, SourceTreeEdge>();

				var vertexLookup = new Dictionary<SourceTreeNode, SourceVertex>();

				sourceRoot.Enumerate(
					(r, c) =>
					{
						if (!vertexLookup.ContainsKey(r))
							vertexLookup[r] = new SourceVertex(r);

						if (!vertexLookup.ContainsKey(c))
							vertexLookup[c] = new SourceVertex(c);

						graph.AddVerticesAndEdge(
							new SourceTreeEdge(vertexLookup[c], vertexLookup[r]));
					}
				);

				var logicCore = new GXLogicCore
					<SourceVertex, SourceTreeEdge, BidirectionalGraph<SourceVertex, SourceTreeEdge>>()
					{ Graph = graph };

				logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.LinLog;
				logicCore.DefaultLayoutAlgorithmParams = logicCore.AlgorithmFactory
					.CreateLayoutParameters(LayoutAlgorithmTypeEnum.LinLog);
				((LinLogLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).AttractionExponent = 2;
				((LinLogLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).GravitationMultiplier = 0.8;
				logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
				logicCore.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 50;
				logicCore.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 50;
				logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
				logicCore.AsyncAlgorithmCompute = false;
				Area.LogicCore = logicCore;
				var nodeBackground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#000000"));

				var pallette = new[] {
					"#4ABDAC",
					"#FC4A1A",
					"#DFDCE3",
					"#FFCE00",
					"#0375B4",
					"#66B9BF",
					"#A239CA",
					"#C09F80"}
				.Select(c => (SolidColorBrush)(new BrushConverter().ConvertFrom(c)))
				.ToArray();

				Area.GenerateGraph(true, true);
				Area
					.EdgesList
					.ToList()
					.ForEach(e =>
					{
						var node = e.Key.Source.Node;

						e.Value.Foreground =
							 new SolidColorBrush(Color.FromArgb(
									 (byte)(55 + (200 * node.GetOrder()) / totalWeight),
									 pallette[node.Subtree].Color.R,
									 pallette[node.Subtree].Color.G,
									 pallette[node.Subtree].Color.B
								 ));
					});
				Area
					.VertexList
					.ToList()
					.ForEach(e => 
					{
						e.Value.Background = nodeBackground;
						e.Value.FontSize = 24;
					});

				zoomctrl.ZoomToFill();

				return !errors.Any();
			}
			catch (Exception ex)
			{
				textBlock.Text = ex.Message;

				return false;
			}
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

			if(control.Recompile(control.textEditor.Text))
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
