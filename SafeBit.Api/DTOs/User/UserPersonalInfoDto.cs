using SafeBit.Api.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace SafeBit.Api.DTOs.User
{
	public class UserPersonalInfoDto
	{
		[StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
		public string? FirstName { get; set; }

		[StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
		public string? LastName { get; set; }

		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email address.")]
		[StringLength(100)]
		public string Email { get; set; } = string.Empty;

		[Phone(ErrorMessage = "Invalid phone number.")]
		[StringLength(20)]
		public string? Phone { get; set; }

		[Required(ErrorMessage = "Date of birth is required.")]
		[DataType(DataType.Date)]
		public DateTime DateOfBirth { get; set; }

		[Required(ErrorMessage = "Gender is required.")]
		[EnumDataType(typeof(Gender), ErrorMessage = "Invalid gender value.")]
		public Gender Gender { get; set; }
	}
}
