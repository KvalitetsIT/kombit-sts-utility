using dk.nsi.seal.Model;

namespace dk.nsi.seal
{
    public class ModelUtilities
    {
        public static void ValidateNotEmpty(string attribute, string msg)
        {
            if (IsEmpty(attribute))
            {
                throw new ModelException(msg);
            }
        }

        public static void ValidateNotNull(object obj, string msg)
        {
            if (obj == null)
            {
                throw new ModelException(msg);
            }
        }

        public static bool IsEmpty(string str)
        {
            return str == null || str.Trim().Length == 0;
        }
    }
}
