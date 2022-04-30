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
            Description = $"Username '{userName}' is invalid, can only contain letters or digits."
        };

        public static GenericError InvalidEmail(string email) => new()
        {
            Code = nameof(InvalidEmail),
            Description = $"Email '{email}' is invalid."
        };

        public static GenericError DuplicateUserName(string userName) => new()
        {
            Code = nameof(DuplicateUserName),
            Description = $"Username '{userName}' is already taken."
        };

        public static GenericError DuplicateEmail(string email) => new()
        {
            Code = nameof(DuplicateEmail),
            Description = $"Email '{email}' is already taken."
        };

        public static GenericError InvalidRoleName(string role) => new()
        {
            Code = nameof(InvalidRoleName),
            Description = $"Role name '{role}' is invalid."
        };

        public static GenericError DuplicateRoleName(string role) => new()
        {
            Code = nameof(DuplicateRoleName),
            Description = $"Role name '{role}' is already taken."
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
            Description = $"User already in role '{role}'."
        };

        public static GenericError UserNotInRole(string role) => new()
        {
            Code = nameof(UserNotInRole),
            Description = $"User is not in role '{role}'."
        };

        public static GenericError PasswordTooShort(int length) => new()
        {
            Code = nameof(PasswordTooShort),
            Description = $"Passwords must be at least {length} characters."
        };

        public static GenericError PasswordRequiresUniqueChars(int uniqueChars) => new()
        {
            Code = nameof(PasswordRequiresUniqueChars),
            Description = $"Passwords must use at least {uniqueChars} different characters."
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