using System;

namespace dk.nsi.seal.Model
{
    public class ModelException : Exception
    {
        public ModelException(string str) : base(str)
        {
            
        }

		public ModelException(string str, Exception e) : base(str, e)
		{

		}

	}
}
