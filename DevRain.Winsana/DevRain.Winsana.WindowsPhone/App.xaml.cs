﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227
using DevRain.Winsana.Core;
using DevRain.Winsana.Pages;

namespace DevRain.Winsana
{

    public sealed partial class App : Application
    {

        MyBootstrapper _bootstrapper = new MyBootstrapper();

        public App()
        {
            this.InitializeComponent();

            _bootstrapper.RegisterViewModelMap<StartPage, StartPageViewModel>();
            _bootstrapper.RegisterViewModelMap<MainPage, MainPageViewModel>();
            _bootstrapper.RegisterViewModelMap<LoginPage, LoginPageViewModel>();
            _bootstrapper.RegisterViewModelMap<AboutPage, AboutPageViewModel>();
            _bootstrapper.RegisterViewModelMap<SettingsPage, SettingsPageViewModel>();
            _bootstrapper.RegisterViewModelMap<WorkspaceDetails, WorkspaceDetailsViewModel>();
            _bootstrapper.RegisterViewModelMap<ProjectDetails, ProjectDetailsViewModel>();

            _bootstrapper.Initialize(this, typeof(StartPage));



        }


        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            _bootstrapper.OnLaunched(e);
        }

        
    }
}