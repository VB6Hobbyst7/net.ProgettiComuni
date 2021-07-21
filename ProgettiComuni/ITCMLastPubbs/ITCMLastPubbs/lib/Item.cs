using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace Christoc.Modules.ITCMLastPubbs.lib
{
	public class Item
	{
		// public
		public int classId;
		public int userId;
		public int pageCount;

		// private
		private string objectId;
		private string title;
		private string dateModify;
		private string dateCreation;
		private string className;
		private string toolTip;

		// custom accessors


		public int ClassId
		{
			get
			{
				return classId;
			}
			set { classId = value; }
		}

		public string ClassCode
		{
			get
			{
				return "attType" + classId;
			}
		}

		public string ClassName
		{
			get
			{
				return className;
			}
			set { className = value; }
		}

		public string DateModify
		{
			get
			{
				return dateModify;
			}
			set { dateModify = value; }
		}

		public string DateCreation
		{
			get
			{
				return dateCreation;
			}
			set { dateCreation = value; }
		}

		public string Title
		{
			get { return title; }
			set { title = value; }
		}

		public string ObjectId
		{
			get { return objectId; }
			set { objectId = value; }
		}

		public string CSSClass
		{
			get { return String.Format("type{0}", classId.ToString()); }
		}

		public string ToolTip
		{
			get { return toolTip; }
			set { toolTip = value; }
		}


		public static Item CreateItemFromXmlResponse(XmlDocument response, XmlNode xn, string toolTipField = "")
		{
			Item item = new Item();

			item.ObjectId = GetAttrName<String>(xn, "IdObject", "0");
			item.Title = GetAttrName<String>(xn, "Title", "0");
			string strDateModify = GetAttrName<String>(xn, "DateModify", "0");
			if (!String.IsNullOrEmpty(strDateModify))
			{
				DateTime d = DateTime.Parse(strDateModify);
				item.DateModify = d.ToShortDateString();
			}
			string strDateCreation = GetAttrName<String>(xn, "DateCreation", "0");
			if (!String.IsNullOrEmpty(strDateCreation))
			{
				DateTime d = DateTime.Parse(strDateCreation);
				item.DateCreation = d.ToShortDateString();
			}

			item.pageCount = (int)Convert.ToInt32(response.SelectSingleNode("//PageCount").InnerXml);
			item.ClassId = (int)Convert.ToInt32(GetAttrName<String>(xn, "IdClass", "0"));
			item.className = GetAttrName<String>(xn, "ClassName", "0");

			if (!String.IsNullOrEmpty(toolTipField))
			{
				item.ToolTip = GetAttrName<string>(xn, toolTipField, "");
			}

			return item;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dataRow"></param>
		/// <param name="attrName"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		private static T GetAttrName<T>(XmlNode dataRow, string attrName, T defaultValue)
		{
			T result = defaultValue;

			XmlAttribute attr = dataRow.Attributes.Cast<XmlAttribute>().Where(p => p.Name == attrName).FirstOrDefault();

			if (attr != null)
			{

				string rawValue = dataRow.Attributes[attrName].Value;
				if (!String.IsNullOrEmpty(rawValue))
				{

					if (typeof(T) == typeof(DateTime))
						result = (T)Convert.ChangeType(XmlConvert.ToDateTime(rawValue, XmlDateTimeSerializationMode.Utc), typeof(T));
					else if (typeof(T) == typeof(DateTime?))
					{
						Type t = Nullable.GetUnderlyingType(typeof(T));
						result = (T)Convert.ChangeType(XmlConvert.ToDateTime(rawValue, XmlDateTimeSerializationMode.Utc), t);
					}
					else
						result = (T)Convert.ChangeType(rawValue, typeof(T));

				}
			}
			return result;
		}

	}
}