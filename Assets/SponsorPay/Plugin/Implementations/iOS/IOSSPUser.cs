using System;
using System.Runtime.InteropServices;

namespace SponsorPay
{
#if UNITY_IPHONE || UNITY_IOS
	public class IOSSPUser : SPUser
	{

		[DllImport ("__Internal")]
		public static extern string _SPUser(string json);
		
		[DllImport ("__Internal")]
		public static extern void _SPUserReset();
				
		override public int? GetAge() {
			return (int?)Get<Object>(AGE);
		}
		
		override public SPUserGender? GetGender() {
			return (SPUserGender?)Get<Object>(GENDER);
		}
		
		override public SPUserSexualOrientation? GetSexualOrientation() {
			return (SPUserSexualOrientation?)Get<Object>(SEXUAL_ORIENTATION);
		}
		
		override public SPUserEthnicity? GetEthnicity() {
			return (SPUserEthnicity?)Get<Object>(ETHNICITY);
		}
		
		override public SPUserMaritalStatus? GetMaritalStatus() {
			return (SPUserMaritalStatus?)Get<Object>(MARITAL_STATUS);
		}
		
		override public int? GetNumberOfChildrens(){
			return (int?)Get<Object>(NUMBER_OF_CHILDRENS);
		}
		
		override public int? GetAnnualHouseholdIncome() {
			return (int?)Get<Object>(ANNUAL_HOUSEHOLD_INCOME);
		}
		
		override public SPUserEducation? GetEducation() {		
			return (SPUserEducation?)Get<Object>(EDUCATION);
		}
		
		override public SPUserConnection? GetConnection() {
			return (SPUserConnection?)Get<Object>(CONNECTION);
		}
		
		override public Boolean? GetIap() {
			return (Boolean?)Get<Object>(IAP);
		}
		
		override public float? GetIapAmount() {
			return (float?)Get<Object>(IAP_AMOUNT);
		}
		
		override public int? GetNumberOfSessions() {
			return (int?)Get<Object>(NUMBER_OF_SESSIONS);
		}
		
		override public long? GetPsTime() {
			return (long?)Get<Object>(PS_TIME);
		}
		
		override public long? GetLastSession() {
			return (long?)Get<Object>(LAST_SESSION);
		}

		protected override void NativePut(string json)
		{
			_SPUser(json);
		}

		protected override void NativeReset()
		{
			_SPUserReset();
		}
		
		protected override string GetJsonMessage(string key)
		{
			return _SPUser(GenerateGetJsonString(key));
		}

	}
#endif
}

