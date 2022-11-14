using dk.nsi.seal.Model.Constants;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace dk.nsi.seal.Model
{
	public class BasicPrivileges
	{
		public class Constraint
		{
            public string Name { get; private set; }
			public string Value { get; private set; }

			public Constraint(string name, string value)
			{
				this.Name = name;
				this.Value = value;
			}

			public override string ToString()
			{
				return "{name=" + Name + ", value=" + Value + "}";
			}
		}

		public static BasicPrivileges Decode(String base64Encoded)
		{
			byte[] decoded = System.Convert.FromBase64String(base64Encoded);
			String decodedString = System.Text.Encoding.UTF8.GetString(decoded);
			XDocument privilegesXml = XDocument.Parse(decodedString);
			return new BasicPrivileges(privilegesXml.Root);
		}

		public Dictionary<string, IList<string>> Privileges => privileges;
		public Dictionary<string, IList<Constraint>> Constraints => constraints;

		private readonly Dictionary<string, IList<string>> privileges = new Dictionary<string, IList<string>>();
		private readonly Dictionary<string, IList<Constraint>> constraints = new Dictionary<string, IList<Constraint>>();

		public BasicPrivileges(XElement root)
		{
			/* root=PrivilegeList */
			/* PrivilegeGroup (0..*) */
			var x = root.Elements();
			var privileGroups = root.Elements(BasicPrivilegesTags.PrivilegeGroup);
			foreach (XElement group in privileGroups)
			{
				/* PrivilegeGroup Scope='?' */
				XAttribute scopeAtt = group.Attribute(BasicPrivilegesAttributes.Scope);
				/* Privilege (0..*) */
				CreatePrivileges(group, scopeAtt);
				/* Constraint (0..*) */
				CreateConstrants(group, scopeAtt);
			}
		}

		private void CreatePrivileges(XElement group, XAttribute scopeAtt)
		{
			var privilegesElm = group.Elements(BasicPrivilegesTags.Privilege);
			IList<string> privilegeValues = new List<string>();
			foreach (XElement valueElm in privilegesElm)
			{
				privilegeValues.Add(valueElm.Value);
			}
			privileges.Add(scopeAtt.Value, new ReadOnlyCollection<string>(privilegeValues));
		}

		private void CreateConstrants(XElement group, XAttribute scopeAtt)
		{

			var constraintElms = group.Elements(XName.Get("Constraint"));
			IList<Constraint> constraintValues = new List<Constraint>();
			foreach (XElement constraintElm in constraintElms)
			{
				var name = constraintElm.Attribute(BasicPrivilegesAttributes.Name);
				string value = constraintElm.Value;
				constraintValues.Add(new Constraint(name.Value, value));
			}
			constraints.Add(scopeAtt.Value, new ReadOnlyCollection<Constraint>(constraintValues));
		}

		public Dictionary<string, IList<string>>.KeyCollection getScopes()
		{
			return privileges.Keys;
		}

		public IList<string> getPrivileges(string scope)
		{
			return privileges[scope];
		}

		public IList<Constraint> getConstraints(string scope)
		{
			return constraints[scope];
		}
	}
}
