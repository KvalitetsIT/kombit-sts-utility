using System.Xml.Linq;
using dk.nsi.seal.Model.Constants;

namespace dk.nsi.seal.Model.DomBuilders
{
    public abstract class AbstractSoapBuilder : AbstractDomBuilder<XDocument>
	{

		/// <summary>
		/// Method called by <see cref="AppendToRoot"/>.<br />
		/// Override this method to control the creation of the root <code>Element</code>.<br />
		/// The default implementation adds <i>soapenv:Envelope</i> element and invokes the {@link #addRootAttributes(org.w3c.dom.Element)} method.
		/// </summary>
		/// <returns></returns>
		protected override XElement CreateRoot()
		{
			return XmlUtil.CreateElement(SoapTags.Envelope);
		}


		protected override void AppendToRoot(XElement root)
		{
			// HACK: Needs to write body first, or the assertion validation will fail after signing the message in the header.
			// hash values for assertion will change if header is not last ?!?
			AppendBody(root);
			AppendHeader(root);
		}

		protected abstract void AddBodyContent(XElement body);


		/// <summary>
		///Method called by <see cref="AppendHeader"/>.<br />
		/// Use this method to add the actual contens to the header element.
		/// </summary>
		/// <param name="header">THe content is added to this header</param>
		protected abstract void AddHeaderContent(XElement header);


		/// <summary>
		/// Method called by <see cref="AppendToRoot"/>.<br />
		/// Override this method to control the creation of the body <code>Element</code>.<br />
		/// The default implementation adds <i>soapenv:Body</i> element.
		/// </summary>
		/// <param name="envelope">Envelope the body is added to</param>
		protected void AppendBody(XElement envelope)
		{
			var body = XmlUtil.CreateElement(SoapTags.Body);
			body.Add(new XAttribute(NameSpaces.xwsu + "Id", "body"));


			envelope.Add(body);
			AddBodyContent(body);
		}

		/**
		 * Method called by {@link #createDocument()}.<br />
		 * Override this method to control the creation of the header <code>Element</code>.<br />
		 * The default implementation adds <i>soapenv:Header</i> element.
		 *
		 * @param doc
		 *            The <code>Document</code> container instance.
		 * @param envelope
		 *            The root <code>Element</code> instance.
		 */

		/// <summary>
		/// Method called by <see cref="AppendToRoot"/>.<br />
		/// Override this method to control the creation of the header<code>Element</code>.<br />
		/// The default implementation adds <i>soapenv:Header</i> element.
		/// </summary>
		/// <param name="envelope">Envelope the body is added to</param>
		protected void AppendHeader(XElement envelope)
		{
			var header = XmlUtil.CreateElement(SoapTags.Header);
			envelope.Add(header);
			AddHeaderContent(header);
		}
	}
}
