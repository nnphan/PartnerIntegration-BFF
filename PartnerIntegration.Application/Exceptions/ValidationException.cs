using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerIntegration.Application.Exceptions
{
    /// <summary>
    /// Thrown when an inbound request fails FluentValidation rules. Caught by the
    /// API layer's global exception handler and translated into a 400 ProblemDetails.
    /// </summary>
    public class ValidationException : Exception
    {
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }

        public ValidationException(string field, string message)
            : base(message)
        {
            Errors = new Dictionary<string, string[]> { [field] = new[] { message } };
        }
    }
}
