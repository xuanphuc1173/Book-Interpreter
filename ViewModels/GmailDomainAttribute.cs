using System.ComponentModel.DataAnnotations;

namespace AppleStore.Pages.Client
{

    public class GmailDomainAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var username = value as string;
            return !string.IsNullOrEmpty(username) && username.EndsWith("@gmail.com");
        }
    }
}