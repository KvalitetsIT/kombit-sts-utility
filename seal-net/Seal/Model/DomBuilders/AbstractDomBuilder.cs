using System;
using System.Xml.Linq;

namespace dk.nsi.seal.Model.DomBuilders
{
	public abstract class AbstractDomBuilder<T>
	{
		private XDocument localDoc;

		/// <summary>
		/// Instructs the build to construct the object.
		/// </summary>
		/// <returns>The constructed instance.</returns>
		public abstract T Build();

		/// <summary>
		/// Add a NameSpace attribute to the supplied<see cref="XElement"/>.
		/// </summary>
		/// <param name = "element" > The < code > Element </ code > to modify.</param>
		/// <param name = "ns" > The name space short name.When added, that value will be prefixed with xmlns:.</param>
		/// <param name = "schema" > The schema location of the NameSpace.</param>
		protected void AddNs(XElement element, string ns, string schema) => element.Add(new XAttribute(XNamespace.Xmlns + ns, schema));


		protected XDocument CreateDocument()
		{
			ValidateBeforeBuild();

			// Store in class to temporary use.
			localDoc = new XDocument();

			var root = CreateRoot();
			AddRootAttributes(root);
			localDoc.Add(root);
			AppendToRoot(root);

			var result = localDoc;
			localDoc = null;

			return result;
		}

		protected abstract XElement CreateRoot();

		/// <summary>
		/// Method called by <see cref="CreateDocument"/>.
		/// Use this method to add additional NameSpace declarations to the <see cref="XDocument"/> or attributes to the root element.
		/// </summary>
		/// <param name="root">The envelope <see cref="XElement"/> instance.</param>
		protected abstract void AddRootAttributes(XElement root);

		protected abstract void AppendToRoot(XElement root);


		/// <summary>
		/// Validate the value of the attribute.
		/// This method is a simple validator, that only checks for null values.
		/// Throws <see cref="ModelException"/> if the value fails validation.
		/// </summary>
		/// <param name="attribute">The name of the attribute being validated.
		/// This value is used for providing an informative exception cause.</param>
		/// <param name="value">The value to validate.</param>
		protected void Validate(string attribute, Object value)
		{
			if (value == null)
			{
				throw new ModelException(attribute + " is mandatory - but was null.");
			}
		}

		/// <summary>
		/// Validate an attribute.
		/// This method validates that a string attribute is neither null, an empty string or a string of spaces.
		/// Throws <see cref="ModelException"/> if the value fails validation.
		/// </summary>
		/// <param name="attribute">The name of the attribute being validated.
		/// This value is used for providing an informative exception cause.</param>
		/// <param name="value">The value to validate.</param>
		protected void Validate(string attribute, string value)
		{
			if (value == null)
			{
				throw new ModelException(attribute + " is mandatory - but was null.");
			}
			ValidateValue(attribute, value);
		}


		/// <summary>
		/// Method called by <see cref="CreateDocument"/> before the process of creating the <see cref="XDocument"/> is initiated.
		/// Implementers can use the method to verify that all mandatory attriutes are set before constructing the <see cref="XDocument"/>.
		/// Throws <see cref="ModelException"/> if one or more requirements are not fulfilled.
		/// </summary>
		protected abstract void ValidateBeforeBuild();

		/// <summary>
		/// Validate the value of an attribute.
		/// This method validates that a string attribute is neither an empty string or a string of spaces.
		/// Throws <see cref="ModelException"/> if the value fails validation.
		/// </summary>
		/// <param name="attribute">The name of the attribute being validated.
		/// This value is used for providing an informative exception cause.</param>
		/// <param name="value">The value to validate - if null the validation will be skipped.</param>
		protected void ValidateValue(string attribute, string value)
		{
			if (value != null)
			{
				if (value.Length == 0)
				{
					throw new ModelException(attribute + " is mandatory - but was an empty String.");
				}
				else if (string.IsNullOrWhiteSpace(value))
				{
					throw new ModelException(attribute + " is mandatory - but was an empty String.");
				}
			}
		}


	}
}
