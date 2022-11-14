using System;

namespace dk.nsi.seal
{
    public class AuthenticationLevel
    {
        public static AuthenticationLevel NoAuthentication = new AuthenticationLevel(1);
        public static AuthenticationLevel UsernamePasswordAuthentication = new AuthenticationLevel(2);
        public static AuthenticationLevel VocesTrustedSystem = new AuthenticationLevel(3);
        public static AuthenticationLevel MocesTrustedUser = new AuthenticationLevel(4);

		public int Level { get; private set; }

		private AuthenticationLevel(int level)
        {
            Level = level;
        }

        public static AuthenticationLevel GetEnumeratedValue(int authLevel)
        {
            AuthenticationLevel result;
            switch (authLevel)
            {
                case 1:
                    result = NoAuthentication;
                    break;
                case 2:
                    result = UsernamePasswordAuthentication;
                    break;
                case 3:
                    result = VocesTrustedSystem; // NOPMD
                    break;
                case 4:
                    result = MocesTrustedUser; // NOPMD
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Authentication level presently not supported by SOSI");
            }
            return result;
        }

        public override bool Equals(Object obj)
        {
            return obj == this || obj != null && obj.GetType() == GetType() && obj.GetHashCode() == GetHashCode()
            && Level == ((AuthenticationLevel)obj).Level;
        }

        public override int GetHashCode()
        {
            return Level;
        }

	}
}
