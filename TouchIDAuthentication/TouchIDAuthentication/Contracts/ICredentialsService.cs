using System;

namespace TouchIDAuthentication.Contracts
{
	public interface ICredentialsService
	{
		AuthenticationStatusEnum VerifyCredentials (Action<AuthenticationStatusEnum> callback);

		bool SaveCredentials (string userId);

		bool DoCredentialsExist ();

		string UserId { get; }

		void DeleteCredentials ();
	}
}

