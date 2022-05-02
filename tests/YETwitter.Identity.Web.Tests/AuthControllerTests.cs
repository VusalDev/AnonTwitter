namespace YETwitter.Identity.Web.Tests
{
    public class AuthControllerTests
    {
        protected WebApplicationFactory<Program> appFactory;

        public AuthControllerTests(ITestOutputHelper testOutput)
        {
            appFactory = new ApplicationFactory(testOutput)
                .WithWebHostBuilder(builder =>
                {
                    // ... Configure test services
                });

        }

        #region Register

        [Fact]
        public async Task TestRegister_Success()
        {
            var client = appFactory.CreateClient();
            var (_, json) = await this.RegisterUsernameAsync(client);

            var data = JsonConvert.DeserializeObject<ResponseModel>(json);

            Assert.Equal("Success", data.Status);
        }

        [Fact]
        public async Task TestRegister_Failed_Duplicate()
        {
            var client = appFactory.CreateClient();
            var body = new RegisterModel
            {
                Username = $"test_usr_{Guid.NewGuid().ToString("N")}",
                Password = "test_usr_pass",
            };
            var response1 = await client.PostAsJsonAsync("api/v1/auth/register", body);
            response1.EnsureSuccessStatusCode();

            var response2 = await client.PostAsJsonAsync("api/v1/auth/register", body);
            Assert.Equal(HttpStatusCode.InternalServerError, response2.StatusCode);

            var json = await response2.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ProblemDetails>(json);

            Assert.Equal((int)HttpStatusCode.InternalServerError, data.Status);
            Assert.Equal("User creation failed", data.Title);
            Assert.Equal("User already exists", data.Detail);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("12345")]
        public async Task TestRegister_Failed_NoPassword(string password)
        {
            var client = appFactory.CreateClient();
            var body = new RegisterModel
            {
                Username = $"test_usr_{Guid.NewGuid().ToString("N")}",
                Password = password,
            };

            var response = await client.PostAsJsonAsync("api/v1/auth/register", body);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ValidationProblemDetails>(json);

            Assert.Equal((int)HttpStatusCode.BadRequest, data.Status);
            Assert.Equal("One or more validation errors occurred.", data.Title);
            Assert.Contains("Password", data.Errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("1")]
        public async Task TestRegister_Failed_NoUsername(string username)
        {
            var client = appFactory.CreateClient();
            var body = new RegisterModel
            {
                Username = username,
                Password = "111111",
            };

            var response = await client.PostAsJsonAsync("api/v1/auth/register", body);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ValidationProblemDetails>(json);

            Assert.Equal((int)HttpStatusCode.BadRequest, data.Status);
            Assert.Equal("One or more validation errors occurred.", data.Title);
            Assert.Contains("Username", data.Errors);
        }

        #endregion

        #region Login

        [Fact]
        public async Task TestLogin_Success()
        {
            var client = appFactory.CreateClient();
            var (registerModel, _) = await this.RegisterUsernameAsync(client);

            var body = new LoginModel
            {
                Username = registerModel.Username,
                Password = registerModel.Password,
            };
            var response = await client.PostAsJsonAsync("api/v1/auth/login", body);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<TokenDataModel>(json);

            Assert.Equal(DateTime.UtcNow.AddMinutes(120), data.ValidTo, TimeSpan.FromSeconds(1)); //from appsettings
        }

        #endregion

        private async Task<(RegisterModel, string)> RegisterUsernameAsync(HttpClient client, RegisterModel registerModel = null)
        {
            var body = registerModel ?? new RegisterModel
            {
                Username = $"test_usr_{Guid.NewGuid().ToString("N")}",
                Password = "test_usr_pass",
            };
            var response = await client.PostAsJsonAsync("api/v1/auth/register", body);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            return (body, json);
        }
    }
}