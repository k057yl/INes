using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using INest.Exceptions;
using INest.Features.Feedback.Commands.CreateFeedback;
using INest.Features.Feedback.Commands.RateFeedback;
using INest.Features.Feedback.Commands.ToggleProcessed;
using INest.Features.Feedback.Queries.GetFeedbacks;
using INest.Infrastructure.Sanitizer;
using INest.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Features.Feedback
{
    public class FeedbackHandlersTests
    {
        private readonly ISanitizerService _sanitizerMock = Substitute.For<ISanitizerService>();

        public FeedbackHandlersTests()
        {
            _sanitizerMock.StripAllHtml(Arg.Any<string>()).Returns(x => x.Arg<string>()?.Trim());
        }

        #region CreateFeedbackHandler Tests

        [Fact]
        public async Task CreateFeedback_ShouldCreateRecord_AndReturnId()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var handler = new CreateFeedbackHandler(db, _sanitizerMock);
            var command = new CreateFeedbackCommand(userId, FeedbackType.Bug, "Найден баг в интерфейсе");

            // Act
            var feedbackId = await handler.Handle(command, CancellationToken.None);

            // Assert
            feedbackId.ShouldNotBe(Guid.Empty);

            var inDb = await db.Feedbacks.FirstOrDefaultAsync(f => f.Id == feedbackId);
            inDb.ShouldNotBeNull();
            inDb.Message.ShouldBe("Найден баг в интерфейсе");
            inDb.Type.ShouldBe(FeedbackType.Bug);
            inDb.UserId.ShouldBe(userId);
        }

        [Fact]
        public async Task CreateFeedback_ShouldThrowAppException_WhenMessageIsEmpty()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            _sanitizerMock.StripAllHtml(Arg.Any<string>()).Returns("");

            var handler = new CreateFeedbackHandler(db, _sanitizerMock);
            var command = new CreateFeedbackCommand(Guid.NewGuid(), FeedbackType.Idea, "   ");

            // Act & Assert
            await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });
        }

        #endregion

        #region RateFeedbackHandler Tests

        [Fact]
        public async Task RateFeedback_ShouldClampRatingBetween1And5()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var feedback = new INest.Data.Entities.Infrastructure.Feedback
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Message = "Тест",
                Type = FeedbackType.Other
            };

            db.Feedbacks.Add(feedback);
            await db.SaveChangesAsync();

            var handler = new RateFeedbackHandler(db);

            // Засылаем рейтинг 10 — должен сжатый стать 5
            var command = new RateFeedbackCommand(feedback.Id, 10, "Хочу темную тему");

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var inDb = await db.Feedbacks.FirstAsync(f => f.Id == feedback.Id);
            inDb.Rating.ShouldBe(5);
            inDb.MissingFeatures.ShouldBe("Хочу темную тему");
        }

        #endregion

        #region ToggleProcessedHandler Tests

        [Fact]
        public async Task ToggleProcessed_ShouldInvertIsProcessedFlag()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var feedback = new INest.Data.Entities.Infrastructure.Feedback
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Message = "Обработано ли?",
                Type = FeedbackType.Other,
                IsProcessed = false
            };

            db.Feedbacks.Add(feedback);
            await db.SaveChangesAsync();

            var handler = new ToggleProcessedHandler(db);
            var command = new ToggleProcessedCommand(feedback.Id);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var inDb = await db.Feedbacks.FirstAsync(f => f.Id == feedback.Id);
            inDb.IsProcessed.ShouldBeTrue();
        }

        [Fact]
        public async Task ToggleProcessed_ShouldThrowAppException_WhenNotFound()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var handler = new ToggleProcessedHandler(db);
            var command = new ToggleProcessedCommand(Guid.NewGuid());

            // Act & Assert
            await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });
        }

        #endregion

        #region GetFeedbacksHandler Tests

        [Fact]
        public async Task GetFeedbacks_ShouldReturnPagedResultsWithUserFallbackName()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = "test@inest.com",
                UserName = "test@inest.com",
                DisplayName = "Роман"
            };

            var f1 = new INest.Data.Entities.Infrastructure.Feedback
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                Message = "Фидбек 1",
                Type = FeedbackType.Bug,
                IsProcessed = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var f2 = new INest.Data.Entities.Infrastructure.Feedback
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                Message = "Фидбек 2",
                Type = FeedbackType.Idea,
                IsProcessed = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            db.Feedbacks.AddRange(f1, f2);
            await db.SaveChangesAsync();

            var handler = new GetFeedbacksHandler(db);
            var query = new GetFeedbacksQuery(1, 10, false, null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.TotalCount.ShouldBe(1);
            result.Items.Count.ShouldBe(1);
            result.Items[0].Message.ShouldBe("Фидбек 1");
            result.Items[0].UserName.ShouldBe("Роман");
            result.Items[0].UserEmail.ShouldBe("test@inest.com");
        }

        #endregion
    }
}