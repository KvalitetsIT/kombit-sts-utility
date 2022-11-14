using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using dk.nsi.seal.Model.Constants;

namespace dk.nsi.seal.Model
{
    public abstract class AbstractDomInfoExtractor
	{

		/** The underlying DOM representation */
		protected XElement dom;
		private Dictionary<string, string> namespaces;

		public XElement XAssertion => dom;

		public AbstractDomInfoExtractor(XElement dom)
		{
			if (dom == null)
			{
				throw new ArgumentException("Element cannot be null");
			}
			this.dom = dom;
		}

		public AbstractDomInfoExtractor(XDocument doc)
		{
			if (doc == null) { throw new ArgumentException("Document cannot be null"); }
			this.dom = doc.Root;
		}

		protected XElement GetTag(params ITag[] tags) => GetTag(dom, tags.ToList());


		/// <summary>
		/// Retrieve the <see cref="XElement"/> identified by the supplied tag path structure.<br />
		/// This method will traverse the DOM retrieve the first child for each specified tag.
		/// </summary>
		/// <param name="tags">List of tag names to traverse.</param>
		/// <returns>The <see cref="List{XElement}"/> matching the final tag in the list.</returns>
		protected XElement GetTag(List<ITag> tags) => GetTag(dom, tags);

		protected XElement GetTag(XElement elm, List<ITag> tags)
		{
			if (tags.Count == 1)
			{
				return elm;
			}
			return GetFirstElement(GetTags(elm, tags));
		}


		/// <summary>
		/// Retrieve the <code>NodeList</code> identified by the supplied tag path structure.<br />
		/// This method will traverse the DOM retrieve the first child for each specified tag.
		/// </summary>
		/// <param name="tags">List of tag names to traverse.</param>
		/// <returns>The <see cref="List{XElement}"/> matching the final tag in the list.</returns>
		protected List<XElement> GetTags(List<ITag> tags)
		{
			return GetTags(dom, tags);
		}

		protected List<XElement> GetTags(XElement elm, List<ITag> tags)
		{
			for (int i = 1; i < tags.Count - 1; i++)
			{
				var tag2 = tags[i];
				elm = GetFirstElement(GetElements(elm, tag2, true));
			}

			var tag = tags[tags.Count - 1];
			return GetElements(elm, tag, false);
		}

		private List<XElement> GetElements(XElement elm, ITag tag, bool onlyFirst)
		{
			List<XElement> res = new List<XElement>();

			if (elm == null)
			{
				return res;
			}

			var childNodes = elm.Elements();
			foreach (var childNode in childNodes)
			{
				var childElm = childNode as XElement;
				if (childElm != null) {
					if (Matches(childElm, tag))
					{
						res.Add(childElm);
						if (onlyFirst)
						{
							break;
						}
					}
				}
			}
        return res;
    }

	private bool Matches(XElement childElm, ITag tag)
	{
		if (childElm.Name.LocalName != null)
		{
			if (childElm.Name.Namespace != null)
			{
				return childElm.Name.Namespace.Equals(tag.Ns) && tag.TagName.Equals(childElm.Name.LocalName);
			}
			return tag.TagName.Equals(childElm.Name.LocalName);
		}

		string tn = childElm.Name.LocalName;
		int colonIndex = tn.IndexOf(':');

		if (colonIndex == -1)
		{
			return tag.Ns == null && tag.TagName.Equals(tn);
		}

		var nsPrefix = GetNamesSpaces()[tag.Ns.NamespaceName];
		return childElm.Name.LocalName.Equals(nsPrefix + ":" + tag.TagName);
	}

		protected string SafeGetTagTextContent(params ITag[] tag)
        {
			return this.SafeGetTagTextContent(tag.ToList());
        }

	protected string SafeGetTagTextContent(List<ITag> tags)
	{
		var element = GetTag(tags);
		return element?.Value;
	}

		protected string SafeGetAttribute(String attributeName, params ITag[] tags)
		{
			return this.SafeGetAttribute(attributeName, tags.ToList());
		}

		protected string SafeGetAttribute(String attributeName, List<ITag> tags)
	{
		var element = GetTag(tags);
		return element?.Attributes(attributeName).FirstOrDefault()?.Value;
	}

	private Dictionary<string, string> GetNamesSpaces()
	{
		if (namespaces == null)
		{
			namespaces = new Dictionary<string, string>();

			var attributes = dom.Attributes();
			foreach (var attribute in attributes)
			{
				var name = attribute.Name.LocalName;
				var value = attribute.Value;

				if (name.StartsWith("xmlns:"))
				{
					namespaces.Add(value, name.Substring("xmlns:".Length));
				}
			}
		}
		return namespaces;
	}

	private XElement GetFirstElement(List<XElement> nodeList)
	{
		if (nodeList.Count == 0)
		{
			return null;
		}
		return nodeList.FirstOrDefault();
	}
}
}
