﻿using AppKit;
using System;
using Eto.Forms;

namespace Eto.CodeEditor.TestAppMac
{
	static class MainClass
	{
    static void Main(string[] args)
    {
      new Application(Eto.Platform.Detect).Run(new MainForm());
    }
  }
}
