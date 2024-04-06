using System;

namespace LegacyApp
{
    public class UserService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IUserCreditService _userCreditService;
        private readonly IUserDataAccess _userDataAccess;

        public UserService(IClientRepository clientRepository, IUserCreditService userCreditService, IUserDataAccess userDataAccess)
        {
            _clientRepository = clientRepository;
            _userCreditService = userCreditService;
            _userDataAccess = userDataAccess;
        }

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            // Check username (empty name or surname)
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }

            // Check email format
            if (!IsValidEmail(email))
            {
                return false;
            }

            // Check age
            if (!IsOverAgeLimit(dateOfBirth))
            {
                return false;
            }

            var client = _clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };

            SetUserCreditLimit(user);

            if (!IsCreditLimitSufficient(user))
            {
                return false;
            }

            _userDataAccess.AddUser(user);
            return true;
        }

        private bool IsValidEmail(string email)
        {
            return email.Contains("@") && email.Contains(".");
        }

        private bool IsOverAgeLimit(DateTime dateOfBirth)
        {
            var age = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
            return age >= 21;
        }

        private void SetUserCreditLimit(User user)
        {
            if (user.Client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else
            {
                int creditLimit = _userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                if (user.Client.Type == "ImportantClient")
                {
                    creditLimit *= 2;
                }
                user.CreditLimit = creditLimit;
            }
        }

        private bool IsCreditLimitSufficient(User user)
        {
            return !user.HasCreditLimit || user.CreditLimit >= 500;
        }
    }
}
