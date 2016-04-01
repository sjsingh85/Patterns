using System;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Xamarin.Forms;

namespace TouchIDAuthentication
{
	public class BaseViewModel: INotifyPropertyChanged
	{
		public BaseViewModel ()
		{
		}

		protected bool IsUserLoggedIn {
			get {
				return GetAppSetting<bool> ("LoggedIn");
			}
			set {
				SaveAppSetting ("LoggedIn", value);
			}
		}

		protected bool AreCredentialsSavedOnDevice {
			get {
				return GetAppSetting<bool> ("SavedOnDevice");
			}
			set {
				SaveAppSetting ("SavedOnDevice", value);
			}
		}

		protected T GetAppSetting<T> (string key)
		{
			if (Application.Current.Properties.ContainsKey (key)) {
				return (T)Application.Current.Properties [key];
			} else
				return default(T);
		}

		protected void SaveAppSetting (string key, object value)
		{
			Application.Current.Properties [key] = value;
		}


		#region INotifyProperty changed impl

		protected void SetProperty<T> (ref T key, T value, Action onChanged = null, [CallerMemberName]string propertyName = "")
		{
			//TODO validate if value is same, then return
			key = value;

			if (onChanged != null)
				onChanged ();

			OnPropertyChanged (propertyName);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged ([CallerMemberName] string propertyName = "")
		{
			var changed = PropertyChanged;
			if (changed == null)
				return;

			changed (this, new PropertyChangedEventArgs (propertyName));
		}

		#endregion INotifyProperty changed impl
	}
}

