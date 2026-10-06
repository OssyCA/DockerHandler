using System.ComponentModel.DataAnnotations;
using DockerController.Core.Configuration;
using Microsoft.Extensions.Options;

namespace DockerController.Api.Configuration;

public sealed class DockerOptionsValidator : IValidateOptions<DockerOptions>
{
    public ValidateOptionsResult Validate(string? name, DockerOptions options)
    {
        var errors = new List<string>();
        var annotationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(options, new ValidationContext(options), annotationResults, validateAllProperties: true))
        {
            errors.AddRange(annotationResults.Select(result => result.ErrorMessage ?? "Okänt valideringsfel."));
        }

        errors.AddRange(options.Validate());

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
