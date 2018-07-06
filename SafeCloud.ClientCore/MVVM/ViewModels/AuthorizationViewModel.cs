﻿using System.Windows.Input;
using ReactiveUI;
using SafeCloud.ClientCore.Abstractions;
using SafeCloud.ClientCore.Infrastructure;

namespace SafeCloud.ClientCore.MVVM.ViewModels
{
    public class LauncherViewModel : ReactiveViewModel
    {
        public override void Initialize()
        {
        }

        public ICommand StartCommand => ReactiveCommand.CreateFromTask(async () =>
        {
            /*await ApplicationFacade.Facade.Navigator.PushToView<AuthorizationViewModel>(model =>
            {
                model.LoginModel.Login = "mark0611@gmail.com";
                model.LoginModel.Password = "Mark95!";
            }, true);*/
            await ApplicationFacade.Facade.Navigator.PushToViewWithPartialView<MainNavigationViewModel, FileListViewModel>(removeFromHistory: true);
        });
    }
}