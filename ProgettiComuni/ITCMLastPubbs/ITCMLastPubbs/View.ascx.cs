/*
' Copyright (c) 2014  Christoc.com
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Services.Localization;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI;
using Christoc.Modules.ITCMLastPubbs.lib;

namespace Christoc.Modules.ITCMLastPubbs
{
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// The View class displays the content
	/// 
	/// Typically your view control would be used to display content or functionality in your module.
	/// 
	/// View may be the only control you have in your project depending on the complexity of your module
	/// 
	/// Because the control inherits from ITCMLastPubbsModuleBase you have access to any custom properties
	/// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
	/// 
	/// </summary>
	/// -----------------------------------------------------------------------------
	/// 

	public partial class View : ITCMLastPubbsModuleBase, IActionable
	{
		KnoSContext kC;
		string KnosUrl;
		public string idSubject;
		public string ticket;
		public string sToken;
		protected void Page_Load(object sender, EventArgs e)
		{
			try
			{
				string adminUserName = "";
				string adminPassword = "";
				string targetUserName = "";
				string toolTipField = "";
				string idView = "";

				sToken = "";
				ticket = "";
				idSubject = "";
				KnosUrl = "";
				int pageSize = 0;

				DotNetNuke.Entities.Users.UserInfo currentUser = DotNetNuke.Entities.Users.UserController.Instance.GetCurrentUserInfo();
				string currentUsername = currentUser.Username;
				targetUserName = currentUsername;

				if (Settings.Contains("Username"))
					adminUserName = Settings["Username"].ToString();

				if (Settings.Contains("Password"))
					adminPassword = Settings["Password"].ToString();

				if (Settings.Contains("TargetUser"))
					targetUserName = Settings["TargetUser"].ToString();

				if (Settings.Contains("KnosURL"))
					KnosUrl = Settings["KnosURL"].ToString();

				if (Settings.Contains("PageSize") && Settings["PageSize"].ToString().Length > 0)
					pageSize = (int)Convert.ToInt32(Settings["PageSize"]);

				if (Settings.Contains("ToolTipField"))
					toolTipField = Settings["ToolTipField"].ToString();

				if (Settings.Contains("IdView"))
					idView = Settings["IdView"].ToString();

				if (ViewState["CurrentPage"] == null)
					ViewState["CurrentPage"] = 1;

				kC = new KnoSContext(adminUserName, adminPassword, targetUserName, KnosUrl, pageSize);

				if (Settings.Contains("Username"))
				{
					kC.DoLogin(out idSubject, out ticket, out sToken);
				}

				kC = new KnoSContext(adminUserName, adminPassword, targetUserName, KnosUrl, pageSize);

				if (!String.IsNullOrEmpty(currentUser.Username))
				{
					var searchUsr = String.Format("DomainUser = '{0}'", currentUser.Username);
					List<UsersItem>users = kC.GetUsers(searchUsr);
					if (users.Count>0)
					{
						UsersItem user = users[0];
						idSubject = user.IdSubject;
					}
				}

				var searchExp = "LEN(Title) > 0 AND IdStatus = 2";
				if (!String.IsNullOrEmpty(idSubject) && Int32.Parse(idSubject) > 0)
				{
					//searchExp = "LEN(Title) > 0 AND IdStatus = 2 AND (EXISTS(select 1 FROM Object_Security WHERE IdObject = _idobject_ AND dbo.fn_IsDescendant(idsubject," + idSubject + ") > 0)";
					//searchExp = searchExp + " OR EXISTS(select 1 FROM Object_Mail_SubjectListLog WHERE IdObject = _idobject_ AND dbo.fn_IsDescendant(idsubject," + idSubject + ") > 0))";

					searchExp = "LEN(Title) > 0 AND IdStatus = 2 AND EXISTS(select 1 FROM Object_Mail_SubjectListLog WHERE IdObject = _idobject_ AND dbo.fn_IsDescendant(idsubject," + idSubject + ") > 0)";
					//searchExp = "LEN(Title) > 0 AND IdStatus = 2";

				}

				this.debug1.Value = adminUserName;
				this.debug2.Value = adminPassword;
				this.debug3.Value = targetUserName;
				this.debug4.Value = searchExp;
				this.debug5.Value = currentUser.Username;


				if (Settings.Contains("TypesToView"))
				{
					string types = Settings["TypesToView"].ToString();
					if (types.Contains(","))
					{
						searchExp = searchExp + " AND IdClass IN (" + types + ")";
					} 
					else if (types.Length > 0)
					{
						searchExp = searchExp + " AND IdClass = " + types;
					}
				}

				doSearch(null, null, searchExp, "", "DateCreation desc", 1, toolTipField, idView);


			}
			catch (Exception exc) //Module failed to load
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		public ModuleActionCollection ModuleActions
		{
			get
			{
				var actions = new ModuleActionCollection
                    {
                        {
                            GetNextActionID(), Localization.GetString("EditModule", LocalResourceFile), "", "", "",
                            EditUrl(), false, SecurityAccessLevel.Edit, true, false
                        }
                    };
				return actions;
			}
		}

		private void doSearch(string idObject, string idClass, string searchExpression, string searchAttributes, string sortExpression, int pageNumber, string toolTipField, string idView)
		{
			List<Item> items = this.kC.GetItems(idObject, idClass, searchExpression, searchAttributes, sortExpression, pageNumber,idSubject, toolTipField, idView);

			HtmlGenericControl li;

			if (items.Count > 0)
			{
				for (int x = 0; x < items.Count; x++)
				{
					Item item = (Item)items[x];
					li = new HtmlGenericControl("li");
					li.Attributes.Add("class", "lastItemsListItem");

					string knosPage = "";
					if (Settings.Contains("KnosPage"))
					{
						knosPage = Settings["KnosPage"].ToString();
					}
					if (String.IsNullOrEmpty(knosPage))
						knosPage = KnosUrl + "Preview.asp";

					li.InnerHtml = "<a title='" + item.ToolTip.Replace("'", "&#39;") + "' target='_new' href='" + knosPage + "?IdObject=" + item.ObjectId + "'>" + item.DateCreation + " - " + item.ClassName + " - " + item.Title + "</a>";

					if (x == 0)
						li.InnerHtml = li.InnerHtml + "<img src='\\DesktopModules\\ITCMLastPubbs\\img\\new.gif' class='imgNew' />";

					this.itemsList.Controls.Add(li);
				}
			}
			else
			{
				this.itemsList.InnerHtml = "Nessuna pubblicazione da visualizzare";
			}
			
		}
	}
}