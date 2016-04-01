using System;
using TouchIDAuthentication.Contracts;
using Security;
using Xamarin.Auth;
using Foundation;
using System.Linq;

using TouchIDAuthentication.Constants;
using System.Diagnostics;
using TouchIDAuthentication.iOS.Services;

[assembly: Xamarin.Forms.Dependency (typeof(CredentialsService))]
namespace TouchIDAuthentication.iOS.Services
{
	public class CredentialsService:ICredentialsService
	{
		/// <summary>
		/// This method trigger the authentication when user is setup on device.
		/// </summary>
		/// <returns>The credentials.</returns>
		/// <param name="callback">Callback.</param>
		public AuthenticationStatusEnum VerifyCredentials (Action<AuthenticationStatusEnum> callback)
		{
			Debug.WriteLine ("Verify credentials called");
			var record = new SecRecord (SecKind.GenericPassword) {
				Service = AppConstants.FullAppName,
				UseOperationPrompt = "Triggering Authentication by OS"
			};

			SecStatusCode result;

			SecKeyChain.QueryAsRecord (record, out result);

			Debug.WriteLine ("result " + result.ToString ());
			if (SecStatusCode.Success == result) {
				return AuthenticationStatusEnum.Success;
			} else if (SecStatusCode.ItemNotFound == result) {
				//authenticated but no record found
				return AuthenticationStatusEnum.NotFound;
			} else {
				//not able authenticate - user cancelled
				return AuthenticationStatusEnum.Error;
			}                
		}

		/// <summary>
		/// This method is required to save credentials on Device and to Xamarin.Auth
		/// </summary>
		/// <returns><c>true</c>, if credentials was saved to OS Key Store, <c>false</c> otherwise.</returns>
		/// <param name="userId">User identifier.</param>
		public bool SaveCredentials (string userId)
		{
			SaveCredentialsToXamarinAuth (userId);

			return SaveCredentialsToOSKeyStore (userId);
		}

		/// <summary>
		/// Saves the credentials to Xamarin.Auth component
		/// </summary>
		/// <param name="userId">User identifier.</param>
		private void SaveCredentialsToXamarinAuth (string userId)
		{
			Account account = new Account {
				Username = userId
			};

			AccountStore.Create ().Save (account, AppConstants.AppName);
			Debug.WriteLine ("save done. ");
			//var x = DoCredentialsExist ();
			//Debug.WriteLine ("saved: " + x);
		}

		/// <summary>
		/// Saves the credentials to OS key store.
		/// </summary>
		/// <returns><c>true</c>, if credentials to OS key store was saved, <c>false</c> otherwise.</returns>
		/// <param name="userId">User identifier.</param>
		private bool SaveCredentialsToOSKeyStore (string userId)
		{
			var secRecord = new SecRecord (SecKind.GenericPassword) {
				Label = "Keychain item for TouchId App",
				Description = "Keychain item for TouchId App",
				Account = "Account",
				Service = AppConstants.FullAppName,
				Comment = "TouchId App",
				ValueData = NSData.FromString (userId),
				Generic = NSData.FromString ("foobar")
			};

			secRecord.AccessControl = new SecAccessControl (SecAccessible.WhenPasscodeSetThisDeviceOnly, SecAccessControlCreateFlags.UserPresence);

			var result = SecKeyChain.Add (secRecord);
			Debug.WriteLine ("OS Keystore: " + result.ToString ());
			return result == SecStatusCode.Success || result == SecStatusCode.DuplicateItem;
		}

		/// <summary>
		/// Does the credentials exist in Xamarin.Auth component.
		/// </summary>
		/// <returns><c>true</c>, if credentials exist , <c>false</c> otherwise.</returns>
		public bool DoCredentialsExist ()
		{
			Debug.WriteLine ("DoCredentialsExist called");
			var temp = AccountStore.Create ().FindAccountsForService (AppConstants.AppName).Any ();
			Debug.WriteLine ("DoCredentialsExist: " + temp);
			return temp;
		}

		/// <summary>
		/// Gets the user identifier from Xamarin.Auth component.
		/// </summary>
		/// <value>The user identifier.</value>
		public string UserId { 
			get {
				Debug.WriteLine ("Getting user id");
				var account = AccountStore.Create ().FindAccountsForService (AppConstants.AppName).FirstOrDefault (t => t.Username != "");
				Debug.WriteLine ("User id got: " + ((account != null) ? account.Username : String.Empty));
				return (account != null) ? account.Username : String.Empty;
			}
		}

		/// <summary>
		/// Deletes the credentials from Xamarin.Auth, No need to delete from Device.
		/// </summary>
		public void DeleteCredentials ()
		{
			Debug.WriteLine ("Delete credentials called");
			var accounts = AccountStore.Create ().FindAccountsForService (AppConstants.AppName).Where (t => t.Username != "");
			if (accounts != null) {
				Debug.WriteLine ("accounts : " + accounts.Count ());
				foreach (var account in accounts) {
					try {
						AccountStore.Create ().Delete (account, AppConstants.AppName);
						Debug.WriteLine ("Deleted : " + account.Username);
					} catch (Exception ex) {
						Debug.WriteLine ("Error deleting: " + account.Username);
					}
				}
			}
		}
	}
}

