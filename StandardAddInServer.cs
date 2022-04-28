using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using Inventor;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

namespace Entitlement

{
	
	/// <summary>
	/// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
	/// that all Inventor AddIns are required to implement. The communication between Inventor and
	/// the AddIn is via the methods on this interface.
	/// </summary>

	[GuidAttribute("963308E2-D850-466D-A1C5-503A2E171552")]
	public class AddInServer : Inventor.ApplicationAddInServer
	{
		#region Data Members

		static readonly HttpClient httpClient = new HttpClient
		{
			BaseAddress = new Uri("https://apps.autodesk.com/webservices/checkentitlement")
		};

		//Inventor application object
		private Inventor.Application m_inventorApplication;

		#endregion

		public AddInServer()
		{
		}

		#region ApplicationAddInServer Members

		public async void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
		{
			try
			{
				//the Activate method is called by Inventor when it loads the addin
				//the AddInSiteObject provides access to the Inventor Application object
				//the FirstTime flag indicates if the addin is loaded for the first time

				//initialize AddIn members
                m_inventorApplication = addInSiteObject.Application;
				if (m_inventorApplication.LoggedIn) // check if user has logged in
				{

					//string username = m_inventorApplication.LoginUserName; //returns the logged in username
					string userId = m_inventorApplication.LoginUserId; // returns the logged in userID
					string appId = "<Enter Your APP ID here>";
					
					string urlParameters = String.Format("?userid={0}&appid={1}", userId, appId);
					string responseBody = await httpClient.GetStringAsync(urlParameters);
                    EntitlementResponse entitlementResponse = JsonSerializer.Deserialize<EntitlementResponse>(responseBody);

					if (entitlementResponse.IsValid == true)
					{ 
						// user validated.. execute rest of the code
					}

				}
				else
                {
					MessageBox.Show("User not logged in");
                }
            }

			catch(Exception e)
			{
				MessageBox.Show(e.ToString());
			}
		}

		public void Deactivate()
		{
			//the Deactivate method is called by Inventor when the AddIn is unloaded
			//the AddIn will be unloaded either manually by the user or
			//when the Inventor session is terminated
		
			try
			{
            	//release inventor Application object
				Marshal.ReleaseComObject(m_inventorApplication);
                m_inventorApplication = null;

				GC.WaitForPendingFinalizers();
				GC.Collect();
			}
			catch(Exception e)
			{
				MessageBox.Show(e.ToString());
			}
		}

        public void ExecuteCommand(int CommandID)
        {
            //this method was used to notify when an AddIn command was executed
            //the CommandID parameter identifies the command that was executed

            //Note:this method is now obsolete, you should use the new
            //ControlDefinition objects to implement commands, they have
            //their own event sinks to notify when the command is executed
        }

        public object Automation
		{
			//if you want to return an interface to another client of this addin,
			//implement that interface in a class and return that class object 
			//through this property

			get
			{
				return null;
			}
		}
		#endregion
	}

	public class EntitlementResponse
	{
		public string UserId { get; set; }
		public string AppId { get; set; }
		public bool IsValid { get; set; }
		public string Message { get; set; }
	}

}
