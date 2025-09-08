using SecureUserManager.Models;
using Xunit;

namespace SecureUserManager.Tests
{
    public class PasswordHelperTests
    {
        [Fact]
        public void HashPasswordValidInputGeneratesConsistentHash()
        {
            // Arrange
            string password = "Secure123";
            byte[] salt = PasswordHelper.GenerateSalt();

            // Act
            byte[] hash1 = PasswordHelper.HashPassword(password, salt);
            byte[] hash2 = PasswordHelper.HashPassword(password, salt);

            // Assert
            Assert.Equal(hash1, hash2);
        }
        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            string password = "Secure123";
            byte[] salt = PasswordHelper.GenerateSalt();
            byte[] hash = PasswordHelper.HashPassword(password, salt);

            // Act
            bool isValid = PasswordHelper.VerifyPassword(password, salt, hash);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            // Arrange
            byte[] salt = PasswordHelper.GenerateSalt();
            byte[] hash = PasswordHelper.HashPassword("CorrectPass", salt);

            // Act
            bool isValid = PasswordHelper.VerifyPassword("WrongPass", salt, hash);

            // Assert
            Assert.False(isValid);
        }
    }
}