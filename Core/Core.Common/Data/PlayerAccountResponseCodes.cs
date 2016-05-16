namespace AFT.RegoV2.Core.Common.Data
{
    public enum PlayerAccountResponseCode
    {
        Succeeded = 0,
        PlayerIsAlreadyLoggedIn = 1,
        UsernamePasswordCombinationIsNotValid = 2,
        UsernameShouldNotBeEmpty = 3,
        PasswordShouldNotBeEmpty = 4,
        PlayerDoesNotExist = 5,
        UnknownBrand = 6,
        NonActive = 7,
        OtherError = 8,
        Unknown = 9,
        AccountLocked = 10,
        InactiveBrand,
        PasswordIsNotWithinItsAllowedRange
    }
}
