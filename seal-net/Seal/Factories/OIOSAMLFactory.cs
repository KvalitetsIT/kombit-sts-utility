using dk.nsi.seal.Model;
using dk.nsi.seal.Model.DomBuilders;
using dk.nsi.seal.Model.ModelBuilders;

namespace dk.nsi.seal.Factories
{
    public static class OIOSAMLFactory
    {
     /**
     * Creates a new <code>OIOSAMLAssertionToIDCardRequestDOMBuilder</code>
     *
     * @return  The newly created <code>OIOSAMLAssertionToIDCardRequestDOMBuilder</code>
     */

        public static OioSamlAssertionToIdCardRequestDomBuilder CreateOiosamlAssertionToIdCardRequestDomBuilder() => 
            new OioSamlAssertionToIdCardRequestDomBuilder();

        /**
		 * Creates a new <code>OIOSAMLAssertionToIDCardRequestModelBuilder</code>
		 *
		 * @return  The newly created <code>OIOSAMLAssertionToIDCardRequestModelBuilder</code>
		 */
        public static OioSamlAssertionToIdCardRequestModelBuilder CreateOioSamlAssertionToIdCardRequestModelBuilder() =>
            new OioSamlAssertionToIdCardRequestModelBuilder();

        public static OIOBSTSAMLAssertionToIDCardResponseModelBuilder CreateOIOBSTSAMLAssertionToIDCardResponseModelBuilder() => 
            new OIOBSTSAMLAssertionToIDCardResponseModelBuilder();

        /**
   * Creates a new <code>IDCardToOIOSAMLAssertionRequestDOMBuilder</code>
   *
   * @return  The newly created <code>IDCardToOIOSAMLAssertionRequestDOMBuilder</code>
   */
        public static IdCardToOioSamlAssertionRequestDomBuilder CreateIdCardToOioSamlAssertionRequestDomBuilder() => 
            new IdCardToOioSamlAssertionRequestDomBuilder();

        /**
		 * Creates a new <code>IDCardToOIOSAMLAssertionRequestModelBuilder</code>
		 *
		 * @return  The newly created <code>IDCardToOIOSAMLAssertionRequestModelBuilder</code>
		 */
        public static IdCardToOioSamlAssertionRequestModelBuilder CreateIdCardToOioSamlAssertionRequestModelBuilder() => 
            new IdCardToOioSamlAssertionRequestModelBuilder();

        /**
  * Creates a new <code>OIOSAMLAssertionBuilder</code>
  *
  * @return  The newly created <code>OIOSAMLAssertionBuilder</code>
  */

        public static OioSamlAssertionBuilder CreateOioSamlAssertionBuilder() => 
            new OioSamlAssertionBuilder();

        public static OIO3BSTSAMLAssertionBuilder CreateOIO3BSTSAMLAssertionBuilder() => 
            new OIO3BSTSAMLAssertionBuilder();

        public static OIOBSTSAMLAssertionToIDCardRequestModelBuilder CreateOIOBSTSAMLAssertionToIDCardRequestModelBuilder() => 
            new OIOBSTSAMLAssertionToIDCardRequestModelBuilder();

        public static OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIO3BSTSAMLAssertion> CreateOIO3BSTSAMLAssertionToIDCardRequestDOMBuilder() => 
            new OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIO3BSTSAMLAssertion>();

        public static OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIOH3BSTSAMLAssertion> CreateOIOH3BSTSAMLAssertionToIDCardRequestDOMBuilder() => 
            new OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIOH3BSTSAMLAssertion>();

        public static OIOH3BSTSAMLAssertionBuilder CreateOIOH3BSTSAMLAssertionBuilder() => 
            new OIOH3BSTSAMLAssertionBuilder();

        public static OIOH2BSTSAMLAssertionBuilder CreateOIOH2BSTSAMLAssertionBuilder() => 
            new OIOH2BSTSAMLAssertionBuilder();

        public static OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIOH2BSTSAMLAssertion> CreateOIOH2BSTSAMLAssertionToIDCardRequestDOMBuilder() => 
            new OIOBSTSAMLAssertionToIDCardRequestDOMBuilder<OIOH2BSTSAMLAssertion>();

        public static OIO2BSTCitizenSAMLAssertionBuilder CreateOIO2BSTCitizenSAMLAssertionBuilder() => 
            new OIO2BSTCitizenSAMLAssertionBuilder();

        public static OioSamlAssertionToIdCardResponseModelBuilder CreateOioSamlAssertionToIDCardResponseModelBuilder() => 
            new OioSamlAssertionToIdCardResponseModelBuilder();
    }
}
