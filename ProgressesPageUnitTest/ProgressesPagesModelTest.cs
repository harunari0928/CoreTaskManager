using System;
using Xunit;
using CoreTaskManager.Model;
using CoreTaskManager.Models;
using CoreTaskmanagerUnitTest;
using System.Linq;
using System.Collections.Generic;

namespace ProgressesPageUnitTest
{
    public class ProgressesPagesModelTest
    {

        [Fact]
        public void CorrectBehave_Filtering_WhenInputSearchQuery()
        {
            using (var db = new CoreTaskManagerContext(Utilities.TestDbContextOptions()))
            {
                // Arrange
                var testProgresses = new List<Progress>
                {
                    new Progress
                    {
                        Id = 1,
                        Title = "テスト"
                    },
                    new Progress
                    {
                        Id = 2,
                        Title = "テスト",
                        Genre = "ソフトウェア"
                    },
                    new Progress
                    {
                        Id = 3,
                        Title = "Web開発",
                        Genre = "ソフトウェア"
                    },
                    new Progress
                    {
                        Id = 4,
                        Title = "Web開発",
                        Genre = "ソフトウェア"
                    },
                    new Progress
                    {
                        Id = 5,
                        Title = "い",
                    },
                    new Progress
                    {
                        Id = 6,
                        Title = "Web開発",
                    }
                };
                testProgresses.ForEach(t => db.Progresses.Add(t));
                db.SaveChanges();
                var expectedProgresses = db.Progresses.Where(d => d.Title == "a");
                var expectedProgresses2 = db.Progresses.Where(d => d.Title == "あ");
                var expectedProgresses3 = db.Progresses.Where(d => d.Title == "い");
                var expectedProgresses4 = db.Progresses.Where(d => d.Title == "テスト");
                var expectedProgresses5 = db.Progresses.Where(d => d.Genre == "ソフトウェア");
                var expectedProgresses6 = db.Progresses.Where(d => d.Genre == "ソフトウェア").Where(d => d.Title == "Web開発");

                // Assert
                var actualProgresses = db.FilterUsingSearchStrings("", "a");
                Assert.Equal(
                    expectedProgresses2.OrderBy(x => x.Id),
                    actualProgresses.OrderBy(x => x.Id)
                    );
                var actualProgresses2 = db.FilterUsingSearchStrings("", "あ");
                Assert.Equal(
                    expectedProgresses2.OrderBy(x => x.Id),
                    actualProgresses2.OrderBy(x => x.Id)
                    );
                var actualProgresses3 = db.FilterUsingSearchStrings("", "い");
                Assert.Equal(
                    expectedProgresses3.OrderBy(x => x.Id),
                    actualProgresses3.OrderBy(x => x.Id)
                    );
                var actualProgresses4 = db.FilterUsingSearchStrings("", "テスト");
                Assert.Equal(
                    expectedProgresses4.OrderBy(x => x.Id),
                    actualProgresses4.OrderBy(x => x.Id)
                    );
                var actualProgresses5 = db.FilterUsingSearchStrings("ソフトウェア", "");
                Assert.Equal(
                    expectedProgresses5.OrderBy(x => x.Id),
                    actualProgresses5.OrderBy(x => x.Id)
                    );
                var actualProgresses6 = db.FilterUsingSearchStrings("ソフトウェア", "Web開発");
                Assert.Equal(
                    expectedProgresses6.OrderBy(x => x.Id),
                    actualProgresses6.OrderBy(x => x.Id)
                    );

            }
        }

        [Fact]
        public void CorrectBehave_Paging()
        {
            using (var db = new CoreTaskManagerContext(Utilities.TestDbContextOptions()))
            {
                // Arrage
                var tests = new List<Progress>
                {
                    new Progress
                    {
                        Id = 1
                    },
                    new Progress
                    {
                        Id = 2
                    },
                    new Progress
                    {
                        Id = 3
                    },
                    new Progress
                    {
                        Id = 4
                    },
                    new Progress
                    {
                        Id = 5
                    },
                    new Progress
                    {
                        Id = 6
                    }
                };
                var progresses = db.Progresses.AsQueryable();
                tests.ForEach(t => db.Progresses.Add(t));
                db.SaveChanges();
                // Act
                var result = progresses.Paging(1, 1);

                // Assert
                Assert.IsAssignableFrom<IQueryable<Progress>>(result);

                Assert.Equal(6, progresses.Paging(1, 6).Count());
                Assert.Equal(4, progresses.Paging(1, 4).Count());
                Assert.Equal(1, progresses.Paging(1, 1).Count());

                Assert.ThrowsAny<ArgumentException>(() =>
                {
                    return progresses.Paging(-1, 1);
                });
                Assert.ThrowsAny<ArgumentException>(() =>
                {
                    return progresses.Paging(0, -1);
                });
                Assert.ThrowsAny<ArgumentException>(() =>
                {
                    return progresses.Paging(1, -1);
                });
            }
        }
    }
}
