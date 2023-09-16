using Carter;

using Maxx.PluginVerticals.Shared.Database;
using Maxx.PluginVerticals.Shared.Entities;
using Maxx.PluginVerticals.Shared.Shared;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

using Riok.Mapperly.Abstractions;

namespace Maxx.PluginVerticals.Feature.GetArticle;

public static class FeatureFlags
{
    public const string ClipArticleContent = "ClipArticleContent";

    public const string ShowArticlePreview = "ShowArticlePreview";
}
public class ArticleResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? Preview { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? PublishedOnUtc { get; set; }
}


public static class GetArticle
{
    public class Query : IRequest<Result<ArticleResponse>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, Result<ArticleResponse>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IFeatureManager _featureManager;

        public Handler(ApplicationDbContext dbContext, IFeatureManager featureManager)
        {
            _dbContext = dbContext;
            _featureManager = featureManager;
        }

        public async Task<Result<ArticleResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var articleResponse = await _dbContext
                .Articles
                .AsNoTracking()
                .Where(article => article.Id == request.Id)
                .Select(article => new Mappings().ToResponse(article))
                .FirstOrDefaultAsync(cancellationToken);

            if (articleResponse is null)
            {
                return Result.Failure<ArticleResponse>(new(
                    "GetArticle.Null",
                    "The article with the specified ID was not found"));
            }

            if (await _featureManager.IsEnabledAsync(FeatureFlags.ShowArticlePreview))
            {
                articleResponse.Preview =
                    articleResponse.Content.Length < 30
                        ? articleResponse.Content
                        : $"{articleResponse.Content[..30]}...";
            }

            return articleResponse;
        }
    }
}

public class GetArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/articles/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetArticle.Query { Id = id };

            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }

            return Results.Ok(result.Value);
        });
    }
}

[Mapper]
public partial class Mappings
{
    public partial ArticleResponse ToResponse(Article data);
}
