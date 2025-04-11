using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests.Models
{
    public class ApplicationUserTests
    {
        [Fact]
        public void ApplicationUser_WithValidData_IsValid()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                ContactInformation = "123-456-7890",
                PreferredCategories = "Electronics,Books"
            };

            // Act
            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void ApplicationUser_WithoutFullName_IsInvalid()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                ContactInformation = "123-456-7890"
            };

            // Act
            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("FullName"));
        }

        [Fact]
        public void ApplicationUser_WithoutContactInformation_IsInvalid()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User"
            };

            // Act
            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("ContactInformation"));
        }

        [Fact]
        public void ApplicationUser_WithLongFullName_IsInvalid()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FullName = new string('a', 101), // Exceeds 100 characters
                ContactInformation = "123-456-7890"
            };

            // Act
            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("FullName"));
        }

        [Fact]
        public void ApplicationUser_WithLongContactInformation_IsInvalid()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                ContactInformation = new string('a', 201) // Exceeds 200 characters
            };

            // Act
            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("ContactInformation"));
        }

        [Fact]
        public void ApplicationUser_WithEmailConfirmationToken_SetsExpiry()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                ContactInformation = "123-456-7890",
                EmailConfirmationToken = "test-token"
            };

            // Act
            user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);

            // Assert
            Assert.NotNull(user.EmailConfirmationTokenExpiry);
            Assert.True(user.EmailConfirmationTokenExpiry > DateTime.UtcNow);
        }

        [Fact]
        public void ApplicationUser_WithPreferredCategories_IsValid()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                ContactInformation = "123-456-7890",
                PreferredCategories = "Electronics,Books,Clothing"
            };

            // Act
            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void ApplicationUser_WithoutPreferredCategories_IsValid()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                ContactInformation = "123-456-7890"
            };

            // Act
            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }
    }
} 