namespace Doctorla.Application.Identity.Tokens;

public record TokenRequest(string Email, string Password);

public class TokenRequestValidator : CustomValidator<TokenRequest>
{
    public TokenRequestValidator(IStringLocalizer<TokenRequestValidator> localizer)
    {
        RuleFor(p => p.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress()
                .WithMessage(localizer["Invalid Email Address."]);

        RuleFor(p => p.Password).Cascade(CascadeMode.Stop)
            .NotEmpty();
    }
}