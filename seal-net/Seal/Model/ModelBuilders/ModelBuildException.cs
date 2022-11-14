using System;

namespace dk.nsi.seal.Model.ModelBuilders
{
    public class ModelBuildException : Exception
	{
		public ModelBuildException(string message) : base(message)
		{
		}

		public ModelBuildException(string message, Exception e) : base(message, e)
		{
		}
	}
}
