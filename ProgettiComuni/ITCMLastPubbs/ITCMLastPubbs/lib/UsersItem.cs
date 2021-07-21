using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace Christoc.Modules.ITCMLastPubbs.lib
{
	public class UsersItem
	{
		// private
		private string idSubject;
		private string subject;
		private string fullName;

		// custom accessors

		public string IdSubject
		{
			get
			{
				return idSubject;
			}
			set
			{
				idSubject = value;
			}
		}

		public string Subject
		{
			get
			{
				return subject;
			}
			set
			{
				subject = value;
			}
		}

		public string FullName
		{
			get
			{
				return fullName;
			}
			set { fullName = value; }
		}

		public static UsersItem CreateItemFromXmlResponse(XmlDocument response, XmlNode xn)
		{
			UsersItem item = new UsersItem();

			item.IdSubject = GetAttrName<String>(xn, "IdSubject", "0");
			item.Subject = GetAttrName<String>(xn, "Subject", "0");
			item.FullName = GetAttrName<String>(xn, "FullName", "0");

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