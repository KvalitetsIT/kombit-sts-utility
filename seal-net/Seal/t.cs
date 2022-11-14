using System;
using System.Xml.Linq;

namespace dk.nsi.seal
{
    internal class t
    {
        public string[] attributes;
        public t[] children;
        public XName name;

        public t(XName name)
        {
            this.name = name;
            children = new t[] {};
            attributes = new string[] {};
        }

        public t(XName name, t child)
        {
            this.name = name;
            children = new[] {child};
            attributes = new string[] {};
        }

        public t(XName name, t[] children)
        {
            this.name = name;
            this.children = children;
            attributes = new string[] {};
        }

        public t(XName name, t[] children, string[] attributes)
        {
            this.name = name;
            this.children = children;
            this.attributes = attributes;
        }

        public t(XName name, string attribute)
        {
            this.name = name;
            children = new t[] {};
            attributes = new[] {attribute};
        }

        public t(XName name, string[] attributes)
        {
            this.name = name;
            children = new t[] {};
            this.attributes = attributes;
        }

        public static t sectree = new t(
            NameSpaces.xwsse + "Security",
            new[]
            {
                new t(NameSpaces.xwsu + "Timestamp", new t(NameSpaces.xwsu + "Created")),
                new t(
                    NameSpaces.xsaml + "Assertion",
                    new[]
                    {
                        new t(NameSpaces.xsaml + "Issuer"),
                        new t(
                            NameSpaces.xsaml + "Subject",
                            new[]
                            {
                                new t(NameSpaces.xsaml + "NameID", "Format"),
                                new t(
                                    NameSpaces.xsaml + "SubjectConfirmation",
                                    new[]
                                    {
                                        new t(NameSpaces.xsaml + "ConfirmationMethod"),
                                        new t(NameSpaces.xsaml + "SubjectConfirmationData")
                                    })
                            }
                            ),
                        new t(NameSpaces.xsaml + "Conditions", new[] {"NotBefore", "NotOnOrAfter"})
                    },
                    new[] {"IssueInstant", "Version", "id"}
                    )
            }
            );

        public static Tuple<string, string> check(XElement xml, t tree)
        {
            if (tree.name != xml.Name) return new Tuple<string, string>("invalid_idcard", tree.name.LocalName + " element forventet");
            foreach (var a in tree.attributes)
            {
                if (xml.Attribute(a) == null) return new Tuple<string, string>("invalid_idcard", a + " attribute mangler");
            }

            foreach (var c in tree.children)
            {
                var e = xml.Element(c.name);
                if (e == null) return new Tuple<string, string>("invalid_idcard", c.name.LocalName + " element forventet");
                var msg = check(e, c);
                if (msg != null) return msg;
            }
            return null;
        }
    }
}