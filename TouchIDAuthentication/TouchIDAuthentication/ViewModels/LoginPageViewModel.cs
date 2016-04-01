using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TouchIDAuthentication.Constants;
using TouchIDAuthentication.Contracts;
using Xamarin.Forms;

namespace TouchIDAuthentication.ViewModels
{
	public class LoginPageViewModel: BaseViewModel, ILoginPage
	{
		ICredentialsService credentialsService;

		#region constructors

		public LoginPageViewModel (Page page)
		{
			page.Title = "Login Page";

			credentialsService = DependencyService.Get<ICredentialsService> ();

			TriggerAuthCommand = new Command (async () => await TouchIdBtnTapped ());

			SetupOnDeviceCommand = new Command (async () => await SetupOnDeviceBtnTapped ());

			page.Appearing += async (object sender, EventArgs e) => {
				await OnAppearing (sender, e);
			};
		}

		#endregion constructors

		#region private methods

		async Task OnAppearing (object sender, EventArgs e)
		{
			Debug.WriteLine ("Appearing called");

			//Check if user previously logged in or not
			//if (credentialsService.DoCredentialsExist ()) {
			//LoggedInId = credentialsService.UserId;
			//Debug.WriteLine ("CredentialsExist, LoggedInId: " + LoggedInId);

			if (credentialsService.DoCredentialsExist ()) {
				if (App.PromptForAuthentication) {
					Debug.WriteLine ("Triggering touchid auth");
					await TouchIdBtnTapped ();
				} else {
					Debug.WriteLine ("PromptForAuthentication is false - so not triggering any auth");
					IsSetupOnDeviceRequired = false;
					UserCanTriggerAuthentication = true;
				}
			} else {
				IsSetupOnDeviceRequired = true;
				UserCanTriggerAuthentication = false;
				LoggedInId = "";
			}
		}

		/// <summary>
		/// This method performs any type of authentication, and then saves some identifier on device
		/// and then redirects user to secret page. Net time you access the app, even putting the app
		/// in background and bringing it back will trigger the authentication
		/// </summary>
		public async Task SetupOnDeviceBtnTapped ()
		{
			if (!String.IsNullOrWhiteSpace (UserLoginId)) {
				var result = credentialsService.SaveCredentials (UserLoginId);
				Debug.WriteLine ("SaveCredentials: " + result);
				if (result) {
					//Message = "Logged In Successfully";
					SaveAppSetting ("SavedOnDevice", true);
					SaveAppSetting ("LoggedIn", true);
					//IsSetupOnDeviceRequired = false;
					//LoggedInId = UserLoginId;
					//UserCanTriggerAuthentication = true;
					//IsSetupOnDeviceRequired = false;

					await GotoSecretPage ();
				} else {
					Message = "Unable to save credentials on device - " + result.ToString ();
				}
			} else {
				Message = "Enter a valid Login ID";
			}
		}

		/// <summary>
		/// Raises the touch identifier tapped event and triggers Android or iOS specific implementation
		/// </summary>
		public async Task TouchIdBtnTapped ()
		{
			//Android specific
			if (Device.OS == TargetPlatform.Android) {
				Action<AuthenticationStatusEnum> osAuthenticationCallbackAction = (s) => {
					OSAuthenticationResult (s).Wait ();
				};

				credentialsService.VerifyCredentials (osAuthenticationCallbackAction);
			} else {
				//iOS specific
				var status = credentialsService.VerifyCredentials (null);
				Debug.WriteLine ("Auth Status: " + status.ToString ());
				await OSAuthenticationResult (status);
			}
		}

		/// <summary>
		/// Tis is the method to do post OS Authentication activities
		/// </summary>
		/// <param name="status">Status.</param>
		public async Task OSAuthenticationResult (AuthenticationStatusEnum status)
		{
			switch (status) {
			case AuthenticationStatusEnum.Success:
				await OnSuccess ();
				break;
			case AuthenticationStatusEnum.Error:
				await OnFailure ("Error - Please try again");
				break;
			case AuthenticationStatusEnum.NotFound:
				await OnKeyNotFound ("Passcode/TouchID not set.");
				break;
			}
		}

		public async Task OnSuccess ()
		{
			UserCanTriggerAuthentication = false;
			IsSetupOnDeviceRequired = false;

			IsUserLoggedIn = true;
			Message = "Logged In Successfully.";

			await GotoSecretPage ();
		}

		public async Task OnFailure (string msg)
		{
			await Task.Run (() => {
				UserCanTriggerAuthentication = true;
				App.PromptForAuthentication = false;
				IsSetupOnDeviceRequired = false;
				Message = msg;
				LoggedInId = "";
			});
		}

		public async Task OnKeyNotFound (string msg)
		{
			await Task.Run (() => {
				UserCanTriggerAuthentication = false;
				IsSetupOnDeviceRequired = true;
				Message = msg;
				LoggedInId = "";
				App.PromptForAuthentication = false;
			});
		}

		/// <summary>
		/// Set MainPage to secret page
		/// </summary>
		async Task GotoSecretPage ()
		{
			Debug.WriteLine ("Secret page requested");
			await Task.Run (() => {
				//No need to go to login page
				Device.BeginInvokeOnMainThread (() => {
					var currentNavPage = App.Current.MainPage as NavigationPage;

					if (currentNavPage != null) {
						currentNavPage.PushAsync (PageFactory.Instance.GetSecretPage ());
					}
				});
			});
		}

		#endregion private methods

		#region properties

		string message = "Messages will show up here";

		public string Message {
			get { return message; }
			set { SetProperty (ref message, value); }
		}

		public ICommand TriggerAuthCommand { get; }

		public ICommand SetupOnDeviceCommand { get; }

		bool isSetupOnDeviceRequired = false;

		public bool IsSetupOnDeviceRequired {
			get { return isSetupOnDeviceRequired; }
			set {
				SetProperty (ref isSetupOnDeviceRequired, value);
			}
		}

		bool userCanTriggerAuthentication = false;

		public bool UserCanTriggerAuthentication {
			get { return userCanTriggerAuthentication; }
			set { SetProperty (ref userCanTriggerAuthentication, value); }
		}

		string userLoginId = "Login1";

		public string UserLoginId {
			get { return userLoginId; }
			set { SetProperty (ref userLoginId, value); }
		}

		string loggedInId = "Login1";

		public string LoggedInId {
			get { 
				if (String.IsNullOrWhiteSpace (loggedInId)) {
					return string.Empty;
				}

				return "Logged In User: " + loggedInId; 
			}
			set { SetProperty (ref loggedInId, value); }
		}

		#endregion properties

	}
}

