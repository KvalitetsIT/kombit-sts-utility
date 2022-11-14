using dk.nsi.seal.Model.Constants;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using static dk.nsi.seal.Model.BasicPrivileges;

namespace dk.nsi.seal.Model.DomBuilders
{
    public class BasicPrivilegesDOMBuilder : AbstractDomBuilder<XDocument>
    {

        private Dictionary<string, IList<string>> privileges = new Dictionary<string, IList<string>>();
        private Dictionary<string, IList<Constraint>> constraints = new Dictionary<string, IList<Constraint>>();

        public BasicPrivilegesDOMBuilder()
        {
        }

        public BasicPrivilegesDOMBuilder(BasicPrivileges basicPrivileges)
        {
            if (basicPrivileges != null)
            {
                this.privileges = basicPrivileges.Privileges;
                this.constraints = basicPrivileges.Constraints;
            }
        }

        public BasicPrivilegesDOMBuilder AddPrivilege(string scope, String privilege)
        {
            bool exists = privileges.TryGetValue(scope, out IList<string> privilegesByScope);
            if (!exists)
            {
                privilegesByScope = new List<string>();
                privileges.Add(scope, privilegesByScope);
            }
            privilegesByScope.Add(privilege);
            return this;
        }

        public BasicPrivilegesDOMBuilder AddConstraint(String scope, Constraint constraint)
        {
            var exists = constraints.TryGetValue(scope, out IList<Constraint> constraintsByScope);
            if (!exists)
            {
                constraintsByScope = new List<Constraint>();
                constraints.Add(scope, constraintsByScope);
            }
            constraintsByScope.Add(constraint);

            return this;
        }

        public void PublicValidateBeforeBuild()
        {
            if (privileges.Count == 0)
            {
                throw new ModelException("No privileges defined");
            }
            foreach (String scope in privileges.Keys)
            {
                if (scope == null)
                {
                    throw new ModelException("PrivilegeGroup with missing scope");
                }
                // list cannot be empty due to constraint by API (#addPrivilege method)
                foreach (String privilege in privileges[scope])
                {
                    if (privilege == null)
                    {
                        throw new ModelException("Blank privilege for scope " + scope);
                    }
                }
            }

            foreach (String scope in constraints.Keys)
            {
                if (scope == null)
                {
                    throw new ModelException("Constraint missing scope");
                }
                else if (!privileges.ContainsKey(scope))
                {
                    throw new ModelException("Constraint scope without privileges. Scope=" + scope);
                }
                foreach (Constraint constraint in constraints[scope])
                {
                    if (constraint == null)
                    {
                        throw new ModelException("Blank constraint. Scope " + scope);
                    }
                    else if (constraint.Name == null)
                    {
                        throw new ModelException("Constraint missing name. Scope " + scope);
                    }
                    else if (constraint.Value == null)
                    {
                        throw new ModelException("Constraint missing value. Scope " + scope);
                    }
                }
            }
        }

        protected override XElement CreateRoot()
        {
            return XmlUtil.CreateElement(BasicPrivilegesTags.PrivilegeList);
        }

        protected override void AddRootAttributes(XElement root) { }

        protected override void AppendToRoot(XElement privilegeListElm)
        {
            foreach (string scope in privileges.Keys)
            {
                /* PrivilegeGroup Scope="?" */
                XElement groupElm = new XElement(BasicPrivilegesTags.PrivilegeGroup);
                groupElm.SetAttributeValue(BasicPrivilegesAttributes.Scope, scope);
                AppendPrivileges(scope, groupElm);
                AppendConstraints(scope, groupElm);
                privilegeListElm.Add(groupElm);
            }
        }

        private void AppendPrivileges(string scope, XElement groupElm)
        {
            foreach (string privilege in privileges[scope])
            {
                XElement privilegeElm = new XElement(BasicPrivilegesTags.Privilege);
                privilegeElm.Value = privilege;
                groupElm.Add(privilegeElm);
            }

        }

        private void AppendConstraints(string scope, XElement groupElm)
        {
            if (constraints.ContainsKey(scope))
            {
                foreach (Constraint constraint in constraints[scope])
                {
                    if (constraint.Name == null)
                    {
                        continue;
                    }
                    XElement constraintElm = new XElement(BasicPrivilegesTags.Constraint);
                    constraintElm.SetAttributeValue(BasicPrivilegesAttributes.Name, constraint.Name);
                    constraintElm.Value = constraint.Value;
                    groupElm.Add(constraintElm);
                }
            }
        }

        public override XDocument Build() => CreateDocument();

        protected override void ValidateBeforeBuild() { }
    }
}
