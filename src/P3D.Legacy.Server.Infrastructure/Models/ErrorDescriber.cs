namespace P3D.Legacy.Server.Infrastructure.Models
{
    internal static class ErrorDescriber
    {
        public static GenericError DefaultError() => new()
        {
            Code = nameof(DefaultError),
            Description = "An unknown failure has occurred."
        };

        public static GenericError ConcurrencyFailure() => new()
        {
            Code = nameof(ConcurrencyFailure),
            Description = "Optimistic concurrency failure, object has been modified."
        };

        public static GenericError PasswordMismatch() => new()
        {
            Code = nameof(PasswordMismatch),
            Description = "Incorrect password."
        };

        public static GenericError InvalidToken() => new()
        {
            Code = nameof(InvalidToken),
            Description = "Invalid token."
        };

        public static GenericError RecoveryCodeRedemptionFailed() => new()
        {
            Code = nameof(RecoveryCodeRedemptionFailed),
            Description = "Recovery code redemption failed."
        };

        public static GenericError LoginAlreadyAssociated() => new()
        {
            Code = nameof(LoginAlreadyAssociated),
            Description = "A user with this login already exists."
        };

        public static GenericError InvalidUserName(string userName) => new()
        {
            Code = nameof(InvalidUserName),
            Description = string.Format("Username '{0}' is invalid, can only contain letters or digits.", userName)
        };

        public static GenericError InvalidEmail(string email) => new()
        {
            Code = nameof(InvalidEmail),
            Description = string.Format("Email '{0}' is invalid.", email)
        };

        public static GenericError DuplicateUserName(string userName) => new()
        {
            Code = nameof(DuplicateUserName),
            Description = string.Format("Username '{0}' is already taken.", userName)
        };

        public static GenericError DuplicateEmail(string email) => new()
        {
            Code = nameof(DuplicateEmail),
            Description = string.Format("Email '{0}' is already taken.", email)
        };

        public static GenericError InvalidRoleName(string role) => new()
        {
            Code = nameof(InvalidRoleName),
            Description = string.Format("Role name '{0}' is invalid.", role)
        };

        public static GenericError DuplicateRoleName(string role) => new()
        {
            Code = nameof(DuplicateRoleName),
            Description = string.Format("Role name '{0}' is already taken.", role)
        };

        public static GenericError UserAlreadyHasPassword() => new()
        {
            Code = nameof(UserAlreadyHasPassword),
            Description = "User already has a password set."
        };

        public static GenericError UserLockoutNotEnabled() => new()
        {
            Code = nameof(UserLockoutNotEnabled),
            Description = "Lockout is not enabled for this user."
        };

        public static GenericError UserAlreadyInRole(string role) => new()
        {
            Code = nameof(UserAlreadyInRole),
            Description = string.Format("User already in role '{0}'.", role)
        };

        public static GenericError UserNotInRole(string role) => new()
        {
            Code = nameof(UserNotInRole),
            Description = string.Format("User is not in role '{0}'.", role)
        };

        public static GenericError PasswordTooShort(int length) => new()
        {
            Code = nameof(PasswordTooShort),
            Description = string.Format("Passwords must be at least {0} characters.", length)
        };

        public static GenericError PasswordRequiresUniqueChars(int uniqueChars) => new()
        {
            Code = nameof(PasswordRequiresUniqueChars),
            Description = string.Format("Passwords must use at least {0} different characters.", uniqueChars)
        };

        public static GenericError PasswordRequiresNonAlphanumeric() => new()
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = "Passwords must have at least one non alphanumeric character."
        };

        public static GenericError PasswordRequiresDigit() => new()
        {
            Code = nameof(PasswordRequiresDigit),
            Description = "Passwords must have at least one digit ('0'-'9')."
        };

        public static GenericError PasswordRequiresLower() => new()
        {
            Code = nameof(PasswordRequiresLower),
            Description = "Passwords must have at least one lowercase ('a'-'z')."
        };

        public static GenericError PasswordRequiresUpper() => new()
        {
            Code = nameof(PasswordRequiresUpper),
            Description = "Passwords must have at least one uppercase ('A'-'Z')."
        };
    }
}