using Moq;

using YETwitter.Posts.Web.Data.Entities;
using YETwitter.Posts.Web.Models;
using YETwitter.Posts.Web.Services;

namespace YETwitter.Posts.Web.Tests
{
    public class ParserTests
    {
        protected readonly Mock<IDatabaseService> databaseServiceMock = new();

        [Theory]
        [InlineData(0, "notag qweqwe ")]
        [InlineData(5, "#tag1 qweqwe qwe #tag2 #tag3#notag #tag4")]
        [InlineData(5, " #tag1 qweqwe qwe #tag2 #tag3#notag #tag4 ")]
        [InlineData(2, "###tag1#tag2")]
        [InlineData(2, "#tag1###tag2")]
        [InlineData(2, "#tag1 qweqwe qwe #tag2 @tag3@notag @tag4")]
        public async Task ParseHashTag_SuccessAsync(int expectedCount, string text)
        {
            databaseServiceMock
                .Setup(x => x.AddOrUpdatePostAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Post(Guid.NewGuid(), text, "author"))
                .Callback<Post, CancellationToken>((p, cts) =>
                {
                    Assert.NotNull(p);
                    Assert.Equal(expectedCount, p.Hashtags.Count);
                });

            var postSrv = new PostsService(databaseServiceMock.Object);
            var post = new CreatePostDto(text, "author");
            await postSrv.CreateAsync(post);
        }
        [Theory]
        [InlineData(0, "notag qweqwe ")]
        [InlineData(5, "@tag1 qweqwe qwe @tag2 @tag3@notag @tag4")]
        [InlineData(5, " @tag1 qweqwe qwe @tag2 @tag3@notag @tag4 ")]
        [InlineData(2, "@@@tag1@tag2")]
        [InlineData(2, "@tag1@@@tag2")]
        [InlineData(3, "#tag1 qweqwe qwe #tag2 @tag3@notag @tag4")]
        public async Task ParseAppeal_SuccessAsync(int expectedCount, string text)
        {
            databaseServiceMock
                .Setup(x => x.AddOrUpdatePostAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Post(Guid.NewGuid(), text, "author"))
                .Callback<Post, CancellationToken>((p, cts) =>
                {
                    Assert.NotNull(p);
                    Assert.Equal(expectedCount, p.Appeals.Count);
                });

            var postSrv = new PostsService(databaseServiceMock.Object);
            var post = new CreatePostDto(text, "author");
            await postSrv.CreateAsync(post);
        }
    }
}