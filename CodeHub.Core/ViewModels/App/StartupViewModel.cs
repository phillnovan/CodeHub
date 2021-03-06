using System;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using System.Linq;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.App
{
    public class StartupViewModel : BaseStartupViewModel
    {
        private readonly ILoginFactory _loginFactory;
        private readonly IApplicationService _applicationService;

        public StartupViewModel(ILoginFactory loginFactory, IApplicationService applicationService)
        {
            _loginFactory = loginFactory;
            _applicationService = applicationService;
        }

        protected async override void Startup()
        {
            if (!_applicationService.Accounts.Any())
            {
                ShowViewModel<Accounts.AccountsViewModel>();
                ShowViewModel<Accounts.NewAccountViewModel>();
                return;
            }

            var account = GetDefaultAccount() as GitHubAccount;
            if (account == null)
            {
                ShowViewModel<Accounts.AccountsViewModel>();
                return;
            }

            var isEnterprise = account.IsEnterprise || !string.IsNullOrEmpty(account.Password);
            if (account.DontRemember)
            {
                ShowViewModel<Accounts.AccountsViewModel>();

                //Hack for now
                if (isEnterprise)
                {
                    ShowViewModel<Accounts.AddAccountViewModel>(new Accounts.AddAccountViewModel.NavObject { IsEnterprise = true, AttemptedAccountId = account.Id });
                }
                else
                {
                    ShowViewModel<Accounts.LoginViewModel>(Accounts.LoginViewModel.NavObject.CreateDontRemember(account));
                }

                return;
            }

            //Lets login!
            try
            {
                IsLoggingIn = true;

                Uri accountAvatarUri = null;
                Uri.TryCreate(account.AvatarUrl, UriKind.Absolute, out accountAvatarUri);
                ImageUrl = accountAvatarUri;
                Status = "Logging in as " + account.Username;

                var client = await _loginFactory.LoginAccount(account);
                _applicationService.ActivateUser(account, client);
            }
            catch (GitHubSharp.UnauthorizedException e)
            {
                DisplayAlert("The credentials for the selected account are incorrect. " + e.Message);

                ShowViewModel<Accounts.AccountsViewModel>();
                if (isEnterprise)
                    ShowViewModel<Accounts.AddAccountViewModel>(new Accounts.AddAccountViewModel.NavObject { IsEnterprise = true, AttemptedAccountId = account.Id });
                else
                    ShowViewModel<Accounts.LoginViewModel>(Accounts.LoginViewModel.NavObject.CreateDontRemember(account));
            }
            catch (Exception e)
            {
                DisplayAlert(e.Message);
                ShowViewModel<Accounts.AccountsViewModel>();
            }
            finally
            {
                IsLoggingIn = false;
            }

        }
    }
}

