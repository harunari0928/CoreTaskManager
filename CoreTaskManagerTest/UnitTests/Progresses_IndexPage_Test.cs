using CoreTaskManager.Data;
using CoreTaskManager.Models;
using CoreTaskManager.Pages.Progresses;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace CoreTaskManagerTest.UnitTests
{
    public class Progresses_IndexPage_Test : PageModel
    {      
        [Fact]
        public async Task Wether_ReturnCorrectActionResult_NextPage_Test()
        {

            // Arrange
            var optionsBuilderCtmc = new DbContextOptionsBuilder<CoreTaskManagerContext>()
                .UseInMemoryDatabase("InMemoryDb1");
            var optionsBuilderADC = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("InMemoryDb2");
            var mockCtmc = new Mock<CoreTaskManagerContext>(optionsBuilderCtmc.Options);
            var mockAdc = new Mock<ApplicationDbContext>(optionsBuilderADC.Options);
            var expectedProgresses = CoreTaskManagerContext.GetSeedingProgresses();
            var expectedIdentities = ApplicationDbContext.GetSeedingUserIdentities();
            mockCtmc.Setup(db => db.GetProgressAsync()).Returns(Task.FromResult(expectedProgresses));
            mockAdc.Setup(db => db.GetIdentityAsync()).Returns(Task.FromResult(expectedIdentities));
            var pageModel = new IndexModel(mockCtmc.Object, mockAdc.Object);
            
            PageModel.HttpContext

            // Act
            await pageModel.OnGetAsync("","","");

            //Assert.Equal(1, 1);
            // Assert
            // When Session(pageNumber.etc) is Null
            //var redirectActionResult = Assert.IsType<RedirectResult>(result);
            //Assert.Equal("Progresses?progressesGenre=&searchString=&currentPageString=1", redirectActionResult.Url);

        }

        
    }
}
