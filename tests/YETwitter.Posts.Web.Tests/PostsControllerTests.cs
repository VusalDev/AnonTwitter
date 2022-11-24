using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

using YETwitter.Posts.Web.Configuration;
using YETwitter.Posts.Web.Controllers;

namespace YETwitter.Posts.Web.Tests
{
    public class PostsControllerTests
    {
        protected ITestOutputHelper testOutput;

        protected WebApplicationFactory<Program> appFactory;

        public PostsControllerTests(ITestOutputHelper testOutput)
        {
            this.testOutput = testOutput;
            appFactory = new ApplicationFactory(testOutput)
                .WithWebHostBuilder(builder =>
                {
                    // ... Configure test services
                });

        }

        #region Create

        [Fact]
        public async Task Create_Success()
        {
            var client = GetClient();

            var body = new CreatePostModel
            {
                Rawtext = $"Hello world! #cool @elonmusk",
            };
            var createdPost = await CreatePost(client, body);

            Assert.NotEqual(Guid.Empty, createdPost.Id);

            var post = await client.GetFromJsonAsync<Post>($"api/v1/post/{createdPost.Id}");
            Assert.Equal(createdPost.Id, post.Id);
            Assert.Equal(body.Rawtext, post.Text);
            Assert.Equal(1, post.Appeals?.Count);
            Assert.Equal(1, post.Hashtags?.Count);

            var post2 = await client.GetFromJsonAsync<Post>($"api/v1/post/{createdPost.Id.ToString("N")}");
            Assert.Equal(createdPost.Id, post2.Id);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task TestDelete_Success()
        {
            var client = GetClient();

            var createdPost = await CreatePost(client);
            Assert.NotEqual(Guid.Empty, createdPost.Id);

            var deleteResponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"api/v1/post/{createdPost.Id}"));
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"api/v1/post/{createdPost.Id}"));
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("1e727c75-e771-4e27-9eaf-bc6c299aa03c")]
        [InlineData("1e727c75e7714e279eafbc6c299aa03c")]
        public async Task TestDelete_NotFound(string postId)
        {
            var client = GetClient();

            var createdPost = await CreatePost(client);
            Assert.NotEqual(Guid.Empty, createdPost.Id);

            var deleteResponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"api/v1/post/{postId}"));
            Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }

        #endregion

        #region Query

        [Fact]
        public async Task TestGetById_Success()
        {
            var client = GetClient();

            var createdPost = await CreatePost(client);
            Assert.NotEqual(Guid.Empty, createdPost.Id);

            var post = await client.GetFromJsonAsync<Post>($"api/v1/post/{createdPost.Id}");
            Assert.Equal(createdPost.Id, post.Id);
        }

        [Theory]
        [InlineData("1e727c75-e771-4e27-9eaf-bc6c299aa03c")]
        [InlineData("1e727c75e7714e279eafbc6c299aa03c")]
        public async Task TestGetById_NotFound(string postId)
        {
            var client = GetClient();

            var createdPost = await CreatePost(client);
            Assert.NotEqual(Guid.Empty, createdPost.Id);

            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"api/v1/post/{postId}"));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("qwerty")]
        public async Task TestGetById_BadRequest(string postId)
        {
            var client = GetClient();

            var createdPost = await CreatePost(client);
            Assert.NotEqual(Guid.Empty, createdPost.Id);

            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"api/v1/post/{postId}"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("1e727c75-e771-4e27-9eaf-bc6c299aa03c")]
        [InlineData("1e727c75e7714 e279eafbc6c299aa03c")]
        public async Task TestQuerySearch_NotFound(string query)
        {
            var client = GetClient();

            await CreatePost(client, new CreatePostModel
            {
                Rawtext = $"Hello world! #cool @elonmusk",
            });
            await CreatePost(client, new CreatePostModel
            {
                Rawtext = $"Hello, stub post example",
            });

            var posts = await client.GetFromJsonAsync<QueryPostResponseModel>($"api/v1/post?query={query}");
            Assert.Empty(posts.Items);
        }

        [Theory(Skip = "Allow null query")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("#cool")]
        public async Task TestQuerySearch_BadRequest(string query)
        {
            var client = GetClient();

            await CreatePost(client, new CreatePostModel
            {
                Rawtext = $"Hello world! #cool @elonmusk",
            });
            await CreatePost(client, new CreatePostModel
            {
                Rawtext = $"Hello, stub post example",
            });

            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"api/v1/post?query={query}"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(2, "Hello")]
        [InlineData(1, "Hello world")]
        [InlineData(1, "cool")]
        [InlineData(1, "example")]
        public async Task TestQuerySearch_Success(int expected, string query)
        {
            var client = GetClient();

            await CreatePost(client, new CreatePostModel
            {
                Rawtext = $"Hello world! #cool @elonmusk",
            });
            await CreatePost(client, new CreatePostModel
            {
                Rawtext = $"Hello, stub post example",
            });

            var posts = await client.GetFromJsonAsync<QueryPostResponseModel>($"api/v1/post?query={query}");
            Assert.Equal(expected, posts.Items.Count);
        }

        [Theory]
        [InlineData(2, "after")]
        [InlineData(1, "cool")]
        [InlineData(1, "mega")]
        [InlineData(1, "proj")]
        public async Task TestQueryHashtag_Success(int expected, string tag)
        {
            var client = GetClient();

            await CreatePost(client, new CreatePostModel
            {
                Rawtext = $"Hello world! #cool @elonmusk #after",
            });
            await CreatePost(client, new CreatePostModel
            {
                Rawtext = $"Hello, stub post example #after #mega###proj",
            });

            var posts = await client.GetFromJsonAsync<QueryPostResponseModel>($"api/v1/post?hashtag={tag}");
            Assert.Equal(expected, posts.Items.Count);
        }

        [Theory]
        [InlineData(2, "me")]
        [InlineData(1, "elonmusk")]
        [InlineData(1, "elon")]
        [InlineData(1, "musk")]
        public async Task TestQueryAppeal_Success(int expected, string tag)
        {
            var client = GetClient();

            await CreatePost(client, new CreatePostModel
            {
                Rawtext = $"Hello world! #cool @elonmusk @me",
            });
            await CreatePost(client, new CreatePostModel
            {
                Rawtext = $"Hello, stub post example @me @elon@@@musk",
            });

            var posts = await client.GetFromJsonAsync<QueryPostResponseModel>($"api/v1/post?userId={tag}");
            Assert.Equal(expected, posts.Items.Count);
        }

        #endregion


        [Theory]
        [InlineData(20, 0)]
        [InlineData(20, 1)]
        [InlineData(10, 2)]
        [InlineData(0, 3)]
        public async Task TestQueryPagination_Success(int expectedCount, int skip)
        {
            int pageSize = 20;
            var a = appFactory.WithWebHostBuilder(c =>
             c.ConfigureServices(s =>
             {
                 s.PostConfigure<PostServiceOptions>(opts => opts.PageSize = pageSize);
             }));

            var client = GetClient(a);

            for (int i = 0; i < 50; i++)
            {
                await CreatePost(client, new CreatePostModel
                {
                    Rawtext = $"Post number {i}",
                });
            }

            var posts = await client.GetFromJsonAsync<QueryPostResponseModel>($"api/v1/post?pageSkip={skip}");
            Assert.Equal(expectedCount, posts.Items.Count);
            if (expectedCount > 0)
            {
                Assert.Equal($"Post number {skip * pageSize}", posts.Items?.FirstOrDefault()?.Text);
            }
        }

        [Theory]
        [InlineData(10, 0)]
        [InlineData(10, 1)]
        [InlineData(10, 2)]
        [InlineData(0, 3)]
        public async Task TestQueryPaginationDefaultPageSize_Success(int expectedCount, int skip)
        {
            int pageSize = 10;
            var a = appFactory.WithWebHostBuilder(c =>
             c.ConfigureServices(s =>
             {
                 s.PostConfigure<PostServiceOptions>(opts => opts.PageSize = pageSize);
             }));

            var client = GetClient(a);

            for (int i = 0; i < 30; i++)
            {
                await CreatePost(client, new CreatePostModel
                {
                    Rawtext = $"Post number {i}",
                });
            }

            var posts = await client.GetFromJsonAsync<QueryPostResponseModel>($"api/v1/post?pageSkip={skip}");
            Assert.Equal(expectedCount, posts.Items.Count);
            if (expectedCount > 0)
            {
                Assert.Equal($"Post number {skip * pageSize}", posts.Items?.FirstOrDefault()?.Text);
            }
        }

        private HttpClient GetClient(WebApplicationFactory<Program> customFactory = null)
        {
            var client = (customFactory ?? appFactory).CreateClient();
            client.DefaultRequestHeaders.Authorization
                = new System.Net.Http.Headers.AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, AuthHelper.GetJwtToken());
            return client;
        }

        private static async Task<CreatePostResponseModel> CreatePost(HttpClient client, CreatePostModel body = null)
        {
            body ??= new CreatePostModel
            {
                Rawtext = $"Hello world! #cool @elonmusk",
            };
            var response = await client.PutAsJsonAsync("api/v1/post", body);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<CreatePostResponseModel>(json);
            return data;
        }
    }
}