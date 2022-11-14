using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace dk.nsi.seal
{
    internal class ns
    {
        public const string wsa = "http://www.w3.org/2005/08/addressing",
            wsa04 = "http://schemas.xmlsoap.org/ws/2004/08/addressing",
            wsa2 = "http://schemas.microsoft.com/ws/2005/05/addressing/none",
            wsu = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd",
            wsse = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd",
            ds = "http://www.w3.org/2000/09/xmldsig#",
            soap = "http://schemas.xmlsoap.org/soap/envelope/",
            trust = "http://docs.oasis-open.org/ws-sx/ws-trust/200512",
            tr = "http://docs.oasis-open.org/ws-sx/ws-trust/200802",
            saml = "urn:oasis:names:tc:SAML:2.0:assertion",
            HealthContextAssertion = "HealthContextAssertion",
            DGWSAssertion = "DGWSAssertion",
            dgws = "http://www.medcom.dk/dgws/2006/04/dgws-1.0.xsd",
            sosi = "http://sosi.dk/gw/2007.09.01",
			wst = "http://schemas.xmlsoap.org/ws/2005/02/trust",
			wst13 = "http://docs.oasis-open.org/ws-sx/ws-trust/200512",
			wst14 = "http://docs.oasis-open.org/ws-sx/ws-trust/200802",
			wsp = "http://schemas.xmlsoap.org/ws/2004/09/policy",
			wsfAuth = "http://docs.oasis-open.org/wsfed/authorization/200706",
			LibertySbfProfileSchema = "urn:liberty:sb:profile",
			LibertyDiscoverySchema = "urn:liberty:disco:2006-08",
			LibertySbfSchema = "urn:liberty:sb",
			LibertySecuritySchema = "urn:liberty:security:2006-08",
			medcom = "medcom";


	    public static XNamespace xsoap = soap,
		    xwsu = wsu,
		    xwsa = wsa,
		    xwsa04 = wsa04,
		    xds = ds,
		    xwsse = wsse,
		    xwsa2 = wsa2,
		    xdgws = dgws,
		    xsaml = saml,
		    xsosi = sosi,
			xwst = wst,
			xwst13 = wst13,
			xwst14 = wst14,
			xtrust = trust,
		    xtr = tr,
			xwsfAuth = wsfAuth,
			xLibertySbfProfileSchema = LibertySbfProfileSchema,
			xLibertyDiscoverySchema = LibertyDiscoverySchema,
			xLibertySbfSchema = LibertySbfSchema,
			xLibertySecuritySchema = LibertySecuritySchema,
			xwsp = wsp;

		public static Dictionary<string, string> alias = new Dictionary<string, string>
        {
            {soap, "soap"},
            {wsa, "wsa"},
            {wsu, "wsu"},
            {wsse, "wsse"},
            {ds, "ds"},
            {trust, "trust"},
            {tr, "tr"},
			{saml, "saml"},
			{wsfAuth, "auth"},
			{wst, "wst"},
			{wst13, "wst13"},
			{wst14, "wst14"},
			{LibertyDiscoverySchema, "disco"},
			{LibertySbfSchema, "sbf"},
			{LibertySecuritySchema, "sec"},
			{dgws, "dgws"}
		};

        internal static void SetMissingNamespaces(XDocument doc)
        {
            var docnss = new HashSet<string>(doc.Root.Attributes().Where(a => a.Name.Namespace == XNamespace.Xmlns).Select(a => a.Value));

            var q = from kv in alias
                where !docnss.Contains(kv.Key)
                select kv;

            foreach (var kv in q)
            {
                doc.Root.Add(new XAttribute(XNamespace.Xmlns + kv.Value, kv.Key));
            }
        }

        internal static XmlNamespaceManager MakeNsManager(XmlNameTable nt)
        {
            var mng = new XmlNamespaceManager(nt);
            foreach (var kv in alias)
            {
                mng.AddNamespace(kv.Value, kv.Key);
            }
            return mng;
        }
    }
}