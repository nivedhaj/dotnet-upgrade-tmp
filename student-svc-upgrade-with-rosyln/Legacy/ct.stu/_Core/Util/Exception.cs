using System;

namespace ct._Core.Util
{
    public class DuplicateObjectException : Exception
    {
        public DuplicateObjectException()
        {
        }

        public DuplicateObjectException(string message)
            : base(message)
        {
        }

        public DuplicateObjectException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    public class InvalidAccessCodeException : Exception
    {
        public InvalidAccessCodeException()
        {
        }

        public InvalidAccessCodeException(string message)
            : base(message)
        {
        }

        public InvalidAccessCodeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    public class InvalidAccessTokenException : Exception
    {
        public InvalidAccessTokenException()
        {
        }

        public InvalidAccessTokenException(string message)
            : base(message)
        {
        }

        public InvalidAccessTokenException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException()
        {
        }

        public InvalidCredentialsException(string message)
            : base(message)
        {
        }

        public InvalidCredentialsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    public class InvalidUserException : Exception
    {
        public InvalidUserException()
        {
        }

        public InvalidUserException(string message)
            : base(message)
        {
        }

        public InvalidUserException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    public class NotInScopeException : Exception
    {
        public NotInScopeException()
        {
        }

        public NotInScopeException(string message)
            : base(message)
        {
        }

        public NotInScopeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    public class ObjectNotFoundException : Exception
    {
        public ObjectNotFoundException()
        {
        }

        public ObjectNotFoundException(string message)
            : base(message)
        {
        }

        public ObjectNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    public class UserEmailAlreadyRegisteredException : Exception
    {
        public UserEmailAlreadyRegisteredException()
        {
        }

        public UserEmailAlreadyRegisteredException(string message)
            : base(message)
        {
        }

        public UserEmailAlreadyRegisteredException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
