using System;
using Xamarin.Forms;

namespace TouchIDAuthentication
{
	public class PageFactory
	{
		private static readonly Lazy<PageFactory> _instance = new Lazy<PageFactory> (() => new PageFactory ());

		private PageFactory ()
		{
		}

		public static PageFactory Instance {
			get {
				return _instance.Value;
			}
		}

		public Page GetLoginPage ()
		{
			var page = new Views.LoginPage ();
			var vm = new ViewModels.LoginPageViewModel (page);

			page.BindingContext = vm;

			return page;
		}

		public Page GetSecretPage ()
		{
			var secretPage = new SecretPage ();
			var vm = new SecretPageViewModel (secretPage);

			secretPage.BindingContext = vm;

			return secretPage;
		}
	}
}

