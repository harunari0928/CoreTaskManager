using CoreTaskmanager.Utilities;
using CoreTaskManager.Model;
using CoreTaskManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CoreTaskManagerTest.UnitTests
{
    public class DataAccessLayerTest
    {

        [Fact]
        public void Wether_CorrectBehave_Filtering_WhenInputSearchQuery_UsingTestData1()
        {
            using (var db = new CoreTaskManagerContext(Utilities.TestDbContextOptions()))
            {
                // Arrange
                db.Progresses.AddRange(GetSeedingProgressesTestData1());
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
        public void Wether_CorrectBehave_Paging_UsingTestData1()
        {
            using (var db = new CoreTaskManagerContext(Utilities.TestDbContextOptions()))
            {
                // Arrage
                db.Progresses.AddRange(GetSeedingProgressesTestData1());
                db.SaveChanges();
                var progresses = db.Progresses.AsQueryable();

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

        [Fact]
        public void Wether_CorrectBehave_GenerateGenre_UsingTestData1()
        {
            using (var db = new CoreTaskManagerContext(Utilities.TestDbContextOptions()))
            {
                // Arrage
                db.Progresses.AddRange(GetSeedingProgressesTestData1());
                db.SaveChanges();

                // Act
                var genre = db.GenerateGenreList();

                // Assert
                Assert.Equal(2, genre.Count());
                Assert.Equal(1, genre.Where(g => g == "ソフトウェア").Count());
                Assert.Equal(1, genre.Where(g => g == "松村").Count());
            }
        }

        [Fact]
        public void Wether_CorrectBehave_GenerateGenre_UsingNoData()
        {
            using (var db = new CoreTaskManagerContext(Utilities.TestDbContextOptions()))
            {
                // Act
                var genre = db.GenerateGenreList();

                // Assert
                Assert.Equal(0, genre.Count());
            }
        }

        // Dont change this method contents! Tests depedent on this Method.
        private List<Progress> GetSeedingProgressesTestData1()
        {
            return new List<Progress>
                {
                    new Progress
                    {
                        Id = 1,
                        Title = "テスト",
                        Genre = "松村"
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
                        Genre = "松村"
                    },
                    new Progress
                    {
                        Id = 6,
                        Title = "Web開発",
                        Genre = "ソフトウェア"
                    }
                };
        }
    }
}
