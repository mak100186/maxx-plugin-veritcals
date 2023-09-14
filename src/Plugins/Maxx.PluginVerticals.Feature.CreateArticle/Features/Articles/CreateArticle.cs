using Carter;

using FluentValidation;

using Maxx.PluginVerticals.Core.Database;
using Maxx.PluginVerticals.Core.Entities;
using Maxx.PluginVerticals.Core.Shared;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Riok.Mapperly.Abstractions;

namespace Maxx.PluginVerticals.Feature.CreateArticle.Features.Articles;


public class CreateArticleRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public static class CreateArticle
{
    public class Command : IRequest<Result<Guid>>
    {
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Title).NotEmpty();
            RuleFor(c => c.Content).NotEmpty();
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

            var article = new Article
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                CreatedOnUtc = DateTime.UtcNow
            };

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
        app.MapPost<IResult>("api/articles", async (CreateArticleRequest request, ISender sender) =>
        {
            var command = new RequestToDto().RequestToCommand(request);

            var result = await sender.Send(command);

            return result.IsFailure
                ? Results.BadRequest(result.Error)
                : Results.Ok(result.Value);
        });
    }
}

[Mapper]
public partial class RequestToDto
{
    public partial CreateArticle.Command RequestToCommand(CreateArticleRequest request);
}
