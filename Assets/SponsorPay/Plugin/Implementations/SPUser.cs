using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LitJson;

namespace SponsorPay
{
	
	public enum SPUserSexualOrientation {
		straight = 0,
		bisexual,
		gay,
		unknown
	};
	
	public enum SPUserGender {
		male = 0,
		female,
		other
	};
	
	public enum SPUserMaritalStatus {
		single = 0,
		relationship,
		married,
		divorced,
		engaged
	};
	
	public enum SPUserEducation {
		other = 0,	
		none,	
		high_school,	
		in_college,
		some_college,	
		associates,	
		bachelors,	
		masters,	
		doctorate
	};
	
	public enum SPUserEthnicity {
		asian = 0,
		black,
		hispanic,
		indian,
		middle_eastern,	
		native_american,	
		pacific_islander,	
		white,	
		other
	};
	
	public enum SPUserConnection {
		wifi = 0,	
		three_g
	};
	
	public abstract class SPUser
	{
		public void Reset() {
			NativeReset();
		}
		
		virtual public int? GetAge() {
			return Get<int?>(AGE);
		}
		
		public void SetAge(int age) {
			Put(AGE, age);
		}
		
		public DateTime? GetBirthdate() {
			string value = Get<string>(BIRTHDATE);
			DateTime parsedDate;
			if (DateTime.TryParseExact(value, "yyyy/MM/dd",
			                           System.Globalization.CultureInfo.InvariantCulture,
			                           System.Globalization.DateTimeStyles.None, 
			                           out parsedDate))
			{
				return parsedDate;
			}
			return null;
		}
		
		public void SetBirthdate(DateTime birthdate) {
			Put(BIRTHDATE, birthdate);
		}
		
		
		virtual public SPUserGender? GetGender() {
			return Get<SPUserGender?>(GENDER);
		}
		
		public void SetGender(SPUserGender gender) {
			Put(GENDER, gender);
		}
		
		virtual public SPUserSexualOrientation? GetSexualOrientation() {
			return Get<SPUserSexualOrientation?>(SEXUAL_ORIENTATION);
		}
		
		public void SetSexualOrientation(SPUserSexualOrientation sexualOrientation) {
			Put(SEXUAL_ORIENTATION, sexualOrientation);
		}
		
		virtual public SPUserEthnicity? GetEthnicity() {
			return Get<SPUserEthnicity?>(ETHNICITY);
		}
		
		public void SetEthnicity(SPUserEthnicity ethnicity) {
			Put(ETHNICITY, ethnicity);
		}
		
		public SPLocation GetLocation() {
			return Get<SPLocation>(SPLOCATION);
		}
		
		public void SetLocation(SPLocation location) {
			Put(SPLOCATION, location);
		}
		
		virtual public SPUserMaritalStatus? GetMaritalStatus() {
			return Get<SPUserMaritalStatus?>(MARITAL_STATUS);
		}
		
		public void SetMaritalStatus(SPUserMaritalStatus maritalStatus) {
			Put(MARITAL_STATUS, maritalStatus);
		}	
		
		virtual public int? GetNumberOfChildrens(){
			return Get<int?>(NUMBER_OF_CHILDRENS);
		}
		
		public void SetNumberOfChildrens(int numberOfChildrens) {
			Put(NUMBER_OF_CHILDRENS, numberOfChildrens);
		}
		
		virtual public int? GetAnnualHouseholdIncome() {
			return Get<int?>(ANNUAL_HOUSEHOLD_INCOME);
		}
		
		public void SetAnnualHouseholdIncome(int annualHouseholdIncome) {
			Put(ANNUAL_HOUSEHOLD_INCOME, annualHouseholdIncome);
		}
		
		virtual public SPUserEducation? GetEducation() {		
			return Get<SPUserEducation?>(EDUCATION);
		}
		
		public void SetEducation(SPUserEducation education) {
			Put(EDUCATION, education);
		}
		
		public string GetZipcode() {
			return Get<string>(ZIPCODE);
		}
		
		public void SetZipcode(string zipcode) {
			Put(ZIPCODE, zipcode);
		}
		
		public string[] GetInterests() {
			return Get<string[]>(INTERESTS);
		}
		
		public void SetInterests(string[] interests) {
			Put(INTERESTS, interests);
		}
		
		virtual public Boolean? GetIap() {
			return Get<Boolean?>(IAP);
		}
		
		public void SetIap(Boolean iap) {
			Put(IAP, iap);
		}
		
		virtual public float? GetIapAmount() {
			return (float?)Get<double?>(IAP_AMOUNT);
		}
		
		public void SetIapAmount(float iap_amount) {
			Put(IAP_AMOUNT, (double)iap_amount);
		}
		
		virtual public int? GetNumberOfSessions() {
			return Get<int?>(NUMBER_OF_SESSIONS);
		}
		
		public void SetNumberOfSessions(int numberOfSessions) {
			Put(NUMBER_OF_SESSIONS, numberOfSessions);
		}
		
		virtual public long? GetPsTime() {
			return Get<long?>(PS_TIME);
		}
		
		public void SetPsTime(long ps_time) {
			Put(PS_TIME, ps_time);
		}
		
		virtual public long? GetLastSession() {
			return Get<long?>(LAST_SESSION);
		}
		
		public void SetLastSession(long lastSession) {
			Put(LAST_SESSION, lastSession);
		}
		
		virtual public SPUserConnection? GetConnection() {
			return Get<SPUserConnection?>(CONNECTION);
		}
		
		public void SetConnection(SPUserConnection connection) {
			Put(CONNECTION, connection);
		}
		
		public string GetDevice() {
			return Get<string>(DEVICE);
		}
		
		public void SetDevice(string device) {
			Put(DEVICE, device);
		}
		
		public string GetAppVersion() {
			return Get<string>(APP_VERSION);
		}
		
		public void SetAppVersion(string appVersion) {
			Put(APP_VERSION, appVersion);
		}
		
		public void PutCustomValue(string key, string value) {
			Put(key, value);
		}
		
		public string GetCustomValue(string key) {
			return Get<string>(key);
		}
		
		// Helper methods
		private void Put(string key, object value)
		{
			string json = GeneratePutJsonString(key, value);
			NativePut(json);
		}
		
		protected abstract void NativePut(string json);
		protected abstract void NativeReset();
		
		protected abstract string GetJsonMessage(string key);
		
		protected T Get<T>(string key)
		{
			string message = GetJsonMessage(key);
			JsonResponse<T> response = JsonMapper.ToObject<JsonResponse<T>>(message);
			if (response.success)
			{
				return response.value;
			}
			UnityEngine.Debug.Log(response.error);
			return default(T);
		}
		
		private string GeneratePutJsonString(string key, object value)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("action", "put");
			dictionary.Add("key", key);
			dictionary.Add("type", value.GetType().ToString());
			if (value is DateTime){
				dictionary.Add("value", ((DateTime)value).ToString("yyyy/MM/dd"));
			}
			else {
				dictionary.Add("value", value);
			}
			return JsonMapper.ToJson(dictionary);
		}
		
		protected string GenerateGetJsonString(string key)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("action", "get");
			dictionary.Add("key", key);
			return JsonMapper.ToJson(dictionary);
		}
		
		protected const string AGE = "age";
		protected const string BIRTHDATE = "birthdate";
		protected const string GENDER = "gender";
		protected const string SEXUAL_ORIENTATION = "sexual_orientation";
		protected const string ETHNICITY = "ethnicity";
		protected const string MARITAL_STATUS = "marital_status";
		protected const string NUMBER_OF_CHILDRENS = "children";
		protected const string ANNUAL_HOUSEHOLD_INCOME = "annual_household_income";
		protected const string EDUCATION = "education";
		protected const string ZIPCODE = "zipcode";
		protected const string INTERESTS = "interests";
		protected const string IAP = "iap";
		protected const string IAP_AMOUNT = "iap_amount";
		protected const string NUMBER_OF_SESSIONS = "number_of_sessions";
		protected const string PS_TIME = "ps_time";
		protected const string LAST_SESSION = "last_session";
		protected const string CONNECTION = "connection";
		protected const string DEVICE = "device";
		protected const string APP_VERSION  = "app_version";
		protected const string SPLOCATION  = "splocation";
		
		private class JsonResponse<T> : AbstractResponse
		{
			public string key { get; set; }
			public T value { get; set; }
			public string error { get; set;}
		}
		
	}
}


