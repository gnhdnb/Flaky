﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleDraw.Windows.Base;
using ConsoleDraw.Inputs;
using ConsoleDraw;
using ConsoleDraw.Windows;

namespace Flaky
{
	public class MainWindow : FullWindow
	{
		public TextArea textArea;
		public Label label;
		public Label recompileHelp;
		public Label exitHelp;

		public MainWindow()
			: base(0, 0, Console.WindowWidth, Console.WindowHeight, null)
		{
			label = new Label("F L A K Y", 2, 58, "label", this);
			recompileHelp = new Label("F5 RECOMPILE", 2, 2, "recompileHelp", this);
			exitHelp = new Label("F10 EXIT", 2, 16, "exitHelp", this);

			textArea = new TextArea(3, 1, Console.WindowWidth - 3, Console.WindowHeight - 5, "textArea", this);
			textArea.BackgroundColour = ConsoleColor.DarkBlue;

			Inputs.Add(label);
			Inputs.Add(recompileHelp);
			Inputs.Add(exitHelp);
			Inputs.Add(textArea);

			CurrentlySelected = textArea;
			Draw();
		}

		public Action<string> OnTextChange
		{
			get
			{
				return textArea.OnChange;
			}
			set
			{
				textArea.OnChange = value;
			}
		}

		public void SetText(string text)
		{
			textArea.SetText(text);
		}

		public string GetText()
		{
			return textArea.GetText();
		}

		public override void ReDraw()
		{
			textArea.Height = Console.WindowHeight - 5;
			textArea.Width = Console.WindowWidth - 3;

			//Black Border
			WindowManager.DrawColourBlock(ConsoleColor.Black, 2, 1, 3, Console.WindowWidth - 2); //Top
		}
	}
}
