﻿using Serilog;
using System;
using System.Windows;


namespace Linguine
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File($"logs/log-{DateTime.Now:yyyyMMdd_HHmmss}.txt") // New file for each app start
                .CreateLogger();

            Log.Information("WPF Application Started");
        }
    }
}
