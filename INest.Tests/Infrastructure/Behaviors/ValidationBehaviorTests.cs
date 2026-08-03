using FluentValidation;
using FluentValidation.Results;
using INest.Exceptions;
using INest.Infrastructure.Behaviors;
using MediatR;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Infrastructure.Behaviors
{
    public class ValidationBehaviorTests
    {
        public class TestRequest : IRequest<string>
        {
            public string Name { get; set; } = string.Empty;
        }

        [Fact]
        public async Task Handle_ShouldCallNext_WhenNoValidationErrors()
        {
            // Arrange
            var validatorMock = Substitute.For<IValidator<TestRequest>>();
            validatorMock.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
                .Returns(new ValidationResult());

            var validators = new List<IValidator<TestRequest>> { validatorMock };
            var behavior = new ValidationBehavior<TestRequest, string>(validators);

            RequestHandlerDelegate<string> next = (ct) => Task.FromResult("OK");

            // Act
            var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

            // Assert
            result.ShouldBe("OK");
        }

        [Fact]
        public async Task Handle_ShouldThrowValidationAppException_WhenValidationFails()
        {
            // Arrange
            var validatorMock = Substitute.For<IValidator<TestRequest>>();
            var failure = new ValidationFailure("Name", "Имя обязательно");
            validatorMock.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
                .Returns(new ValidationResult(new[] { failure }));

            var validators = new List<IValidator<TestRequest>> { validatorMock };
            var behavior = new ValidationBehavior<TestRequest, string>(validators);

            RequestHandlerDelegate<string> next = (ct) => Task.FromResult("OK");

            // Act & Assert
            var ex = await Should.ThrowAsync<ValidationAppException>(async () =>
            {
                await behavior.Handle(new TestRequest { Name = "" }, next, CancellationToken.None);
            });

            ex.Errors.Any(e => e.PropertyName == "Name").ShouldBeTrue();
        }
    }
}