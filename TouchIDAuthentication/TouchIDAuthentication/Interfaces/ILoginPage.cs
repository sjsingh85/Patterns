using System;
using System.Threading.Tasks;

namespace TouchIDAuthentication
{
	public interface ILoginPage
	{
		Task SetupOnDeviceBtnTapped ();

		Task TouchIdBtnTapped ();

		Task OSAuthenticationResult (AuthenticationStatusEnum status);

		Task OnSuccess ();

		Task OnFailure (string msg);

		Task OnKeyNotFound (string msg);
	}
}

