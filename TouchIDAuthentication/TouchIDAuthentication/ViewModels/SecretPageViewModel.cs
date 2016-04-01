using System;
using Xamarin.Forms;
using TouchIDAuthentication.Contracts;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TouchIDAuthentication
{
	public class SecretPageViewModel: BaseViewModel
	{
		public SecretPageViewModel (Page page)
		{
			page.Title = "Secret Page";
			App.PromptForAuthentication = false;
			LogoutCommand = new Command (async () => {
				await LogoutTapped ();
			});

			if (!IsUserLoggedIn) {
				//if this ever occurs
				((NavigationPage)App.Current.MainPage).PopToRootAsync ().Wait ();
			} else {
				LoggedInId = DependencyService.Get<ICredentialsService> ().UserId;
			}

			page.Appearing += (object sender, EventArgs e) => {
				App.PromptForAuthentication = true;
			};
		}

		async Task LogoutTapped ()
		{
			DependencyService.Get<ICredentialsService> ().DeleteCredentials ();
			Debug.WriteLine ("Logout");
			IsUserLoggedIn = false;
			AreCredentialsSavedOnDevice = false;
			await ((NavigationPage)App.Current.MainPage).PopToRootAsync ();
		}

		public ICommand LogoutCommand { get; }

		string loggedInId = "";

		public string LoggedInId {
			get { 
				if (String.IsNullOrWhiteSpace (loggedInId)) {
					return string.Empty;
				}

				return "Logged In User: " + loggedInId; 
			}
			set { SetProperty (ref loggedInId, value); }
		}
	}
}

