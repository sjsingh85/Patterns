using System;

using Xamarin.Forms;
using System.Linq.Expressions;
using TouchIDAuthentication.Contracts;
using System.Diagnostics;

namespace TouchIDAuthentication
{
	public class App : Application
	{
		public static bool PromptForAuthentication = true;

		public App ()
		{
			MainPage = new NavigationPage (PageFactory.Instance.GetLoginPage ());
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		/// <summary>
		/// If user returns to app, trigger authentication by moving user to login page
		/// </summary>
		/// <remarks>To be added.</remarks>
		protected override void OnResume ()
		{
			Debug.WriteLine ("OnResume Called");
			var currentNavPage = App.Current.MainPage as NavigationPage;

			if (currentNavPage != null && App.PromptForAuthentication) {
				var currentMainPage = ((NavigationPage)App.Current.MainPage).CurrentPage;

				var currentPageViewModel = currentMainPage.BindingContext as ILoginPage;
				if (currentPageViewModel == null) {
					Debug.WriteLine ("It is not a Login Page - so going to Login Page");
					App.PromptForAuthentication = true;
					currentNavPage.PopToRootAsync ();
				}
			}
		}
	}
}

