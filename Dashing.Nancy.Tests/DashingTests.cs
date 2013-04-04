using FakeItEasy;
using Nancy;
using Nancy.Testing;
using Xunit;

namespace Dashing.Tests
{
    public class DashingTests
    {
        [Fact]
        public void Should_return_status_ok_when_route_exists()
        {
            // Given
            var bootstrapper = new ConfigurableBootstrapper(config => config.Modules(typeof(DashingModule)));
            var browser = new Browser(bootstrapper);

            // When
            var result = browser.Get("/", with =>
            {
                with.HttpRequest();
            });

            // Then
            Assert.Equal(HttpStatusCode.SeeOther, result.StatusCode);
        }

        [Fact]
        public void Should_stream_the_event()
        {
            // Given
            var bootstrapper = new ConfigurableBootstrapper(config =>
            {
                config.Modules(typeof(DashingModule));
            });
            var browser = new Browser(bootstrapper);
            var client = A.Dummy<EventStreamWriterResponse>();
            ClientBus.Register(client);

            // When
            var result = browser.Post("/widgets/number", with =>
            {
                with.HttpRequest();
                with.JsonBody(new { current = 123 });
            });

            // Then
            A.CallTo(() => client.Write(null))
                .WhenArgumentsMatch(args =>
                    args[0].ToExpando().id == "number" &&
                    args[0].ToExpando().current == 123)
                    .MustHaveHappened();
        }

        public static bool IsValueTypeOrString<T>(T obj)
        {
            return obj != null && (obj.GetType().IsValueType || obj is string);
        }
    }
}
