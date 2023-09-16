using Carter;

using FluentValidation;

using Maxx.PluginVerticals.Shared.Database;
using Maxx.PluginVerticals.Shared.Entities;
using Maxx.PluginVerticals.Shared.Shared;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Riok.Mapperly.Abstractions;

namespace Maxx.PluginVerticals.Feature.CreateArticle;


public class CreateArticleRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedOn { get; set; }
}

public static class CreateArticle
{
    public class Command : IRequest<Result<Guid>>
    {
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime PublishedOn { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Title).NotEmpty();
            RuleFor(c => c.Content).NotEmpty();
            RuleFor(c => c.PublishedOn).LessThan(p => DateTime.UtcNow);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<Guid>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IValidator<Command> _validator;

        public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<Guid>(new(
                    "CreateArticle.Validation",
                    validationResult.ToString()));
            }

            var article = new Mappings().ToData(request);
            article.Id = Guid.NewGuid();
            article.CreatedOnUtc = DateTime.UtcNow;

            _dbContext.Add(article);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return article.Id;
        }
    }
}

public class CreateArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/articles", async (CreateArticleRequest request, ISender sender) =>
        {
            var command = new Mappings().ToCommand(request);

            var result = await sender.Send(command);

            return result.IsFailure
                ? Results.BadRequest(result.Error)
                : Results.Ok(result.Value);
        });
    }
}

[Mapper]
public partial class Mappings
{
    public partial CreateArticle.Command ToCommand(CreateArticleRequest request);
    public partial Article ToData(CreateArticle.Command command);
}
